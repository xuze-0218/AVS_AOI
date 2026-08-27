using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.CameraSDKHelper.Common.Enum;
using Lmi3d.GoSdk;
using Lmi3d.GoSdk.Messages;
using Lmi3d.Zen;
using Lmi3d.Zen.Io;

namespace AVS_Drivers.Camera.Mode
{
    internal class Lmi3DCamera : BaseCamera, IDisposable
    {
        private GoSystem _system;
        private GoSensor _sensor;
        private GoSetup _setup;
        private bool _isGrabbing = false;

        private GoSensor.DataFx _dataHandler;

        private static bool _sdkInitialized = false;
        private static readonly object SdkLock = new object();

        // ==========================================
        // 固定容量环形内存池
        // ==========================================
        private const int BufferCount = 10;             // 图像缓存容量
        private const int MAX_FRAME_SIZE = 200 * 1024 * 1024; // 单帧最大 200MB（如实际帧更小可调低）
        private byte[][] _depthBuffers = new byte[BufferCount][];
        private GCHandle[] _depthHandles = new GCHandle[BufferCount];
        private int _bufferIndex = 0;
        private readonly object _bufferLock = new object();

        public Lmi3DCamera() : base()
        {
            EnsureSdkInitialized();
            _system = new GoSystem();
            PreAllocateBuffers();
        }

        private void EnsureSdkInitialized()
        {
            lock (SdkLock)
            {
                if (!_sdkInitialized)
                {
                    KApiLib.Construct();
                    GoSdkLib.Construct();
                    _sdkInitialized = true;
                }
            }
        }

        /// <summary>
        /// 一次性分配足够内存并固定，避免运行中动态分配导致悬空指针
        /// </summary>
        private void PreAllocateBuffers()
        {
            for (int i = 0; i < BufferCount; i++)
            {
                _depthBuffers[i] = new byte[MAX_FRAME_SIZE];
                _depthHandles[i] = GCHandle.Alloc(_depthBuffers[i], GCHandleType.Pinned);
            }
        }

        #region 设备枚举与初始化

        public override List<string> GetListEnum()
        {
            var snList = new List<string>();
            try
            {
                _system.Refresh();
                long sensorCount = _system.SensorCount;
                for (long i = 0; i < sensorCount; i++)
                {
                    var sensor = _system.GetSensor(i);
                    // 假设传感器有 Id 属性作为序列号，如不同请调整
                    snList.Add(sensor.Id.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] GetListEnum error: {ex.Message}");
            }
            return snList;
        }

        public override bool InitDevice(string CamSN)
        {
            if (string.IsNullOrEmpty(CamSN)) return false;

            try
            {
                _sensor = _system.FindSensorById(uint.Parse(CamSN));
                if (_sensor == null) return false;

                _sensor.Connect();
                _setup = _sensor.Setup;

                // 设置扫描模式为表面模式，确保输出 UniformSurface 数据
                try
                {
                    _setup.ScanMode = GoMode.Surface;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Lmi3D] 设置 ScanMode 失败: {ex.Message}");
                }

                // 启用数据并注册回调
                _sensor.EnableData(true);
                _dataHandler = new GoSensor.DataFx(OnDataReceived);
                _sensor.SetDataHandler(_dataHandler);

                ImageInfo.PixelFormat = CamPixelFormat.Depth;
                SN = CamSN;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] InitDevice error: {ex.Message}");
                CloseDevice();
                return false;
            }
        }

        public override void CloseDevice()
        {
            try
            {
                if (_isGrabbing) StopGrabbingCore();

                if (_sensor != null)
                {
                    _sensor.SetDataHandler(null);
                    _sensor.EnableData(false);
                    _sensor.Disconnect();
                    _sensor = null;
                }

                if (_system != null)
                {
                    _system.Dispose();
                    _system = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] CloseDevice error: {ex.Message}");
            }
        }

        #endregion

        #region 采集控制与参数设置

        protected override bool StartGrabbingCore()
        {
            try
            {
                _sensor.Start();
                _isGrabbing = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] StartGrabbing error: {ex.Message}");
                return false;
            }
        }

        protected override bool StopGrabbingCore()
        {
            try
            {
                _sensor.Stop();
                _isGrabbing = false;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] StopGrabbing error: {ex.Message}");
                return false;
            }
        }

        public override bool SoftTrigger()
        {
            try
            {
                _sensor.Trigger();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] SoftTrigger error: {ex.Message}");
                return false;
            }
        }

        public override bool Continue_SoftTrigger() => true; // 连续模式由 Start 已启动

        public override bool SetTriggerMode(TriggerMode mode, TriggerSource triggerEnum = TriggerSource.Line0)
        {
            try
            {
                if (mode == TriggerMode.Off)
                {
                    _setup.TriggerSource = GoTrigger.Time;
                }
                else
                {
                    // 开启触发模式
                    switch (triggerEnum)
                    {
                        case TriggerSource.Software:
                            _setup.TriggerSource = GoTrigger.Software;
                            break;
                        case TriggerSource.Line0:
                        case TriggerSource.Line1:
                        case TriggerSource.Line2:
                        case TriggerSource.Line3:
                            _setup.TriggerSource = GoTrigger.DigitalInput;
                            break;
                        default:
                            _setup.TriggerSource = GoTrigger.Software;
                            break;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] SetTriggerMode error: {ex.Message}");
                return false;
            }
        }

        public override bool GetTriggerMode(out TriggerMode mode, out TriggerSource hardTriggerModel)
        {
            mode = TriggerMode.On;
            hardTriggerModel = TriggerSource.Software;
            try
            {
                if (_setup.TriggerSource == GoTrigger.Software)
                    hardTriggerModel = TriggerSource.Software;
                else
                    hardTriggerModel = TriggerSource.Line0; // 假设为硬触发
                mode = TriggerMode.On;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] GetTriggerMode error: {ex.Message}");
                return false;
            }
        }

        public override bool SetExpouseTime(ushort value)
        {
            try
            {
                _setup.SetExposure(GoRole.Main, (double)value);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] SetExpouseTime error: {ex.Message}");
                return false;
            }
        }

        public override bool GetExpouseTime(out ushort value)
        {
            value = 0;
            try
            {
                value = (ushort)_setup.GetExposure(GoRole.Main);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] GetExpouseTime error: {ex.Message}");
                return false;
            }
        }

        public override bool SetGain(short gain)
        {
            return false;
        }

        public override bool GetGain(out short gain)
        {
            gain = 0;
            return false;
        }

        // 以下方法均不支持，返回 false 避免异常
        public override bool SetTriggerPolarity(TriggerPolarity polarity) => false;
        public override bool GetTriggerPolarity(out TriggerPolarity polarity)
        {
            polarity = TriggerPolarity.RisingEdge;
            return false;
        }
        public override bool SetTriggerFliter(ushort flitertime) => false;
        public override bool GetTriggerFliter(out ushort flitertime)
        {
            flitertime = 0;
            return false;
        }
        public override bool SetTriggerDelay(ushort delay) => false;
        public override bool GetTriggerDelay(out ushort delay)
        {
            delay = 0;
            return false;
        }
        public override bool SetLineMode(IOLines line, LineMode mode) => false;
        public override bool SetLineStatus(IOLines line, LineStatus linestatus) => false;
        public override bool GetLineStatus(IOLines line, out LineStatus lineStatus)
        {
            lineStatus = LineStatus.Low;
            return false;
        }
        public override bool AutoBalanceWhite() => false;
        public override bool SetALLOutPutValue(int channel) => false;

        #endregion

        #region 数据回调处理

        private void OnDataReceived(KObject data)
        {
            GoDataSet dataSet = data as GoDataSet;
            if (dataSet == null) return;

            try
            {
                for (uint i = 0; i < dataSet.Count; i++)
                {
                    GoDataMsg dataObj = (GoDataMsg)dataSet.Get(i);
                    if (dataObj.MessageType == GoDataMessageType.UniformSurface)
                    {
                        ProcessUniformSurface((GoUniformSurfaceMsg)dataObj);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Lmi3D] OnDataReceived error: {ex.Message}");
            }
            finally
            {
                // 必须销毁数据集，防止 SDK 内存泄漏
                dataSet.Dispose();
            }
        }

        private void ProcessUniformSurface(GoUniformSurfaceMsg surfaceMsg)
        {
            int width = (int)surfaceMsg.Width;
            int length = (int)surfaceMsg.Length;
            int bufferSize = width * length * sizeof(short); // 假设 16 位深度数据
            IntPtr dataPtr = surfaceMsg.Data;

            if (dataPtr == IntPtr.Zero || bufferSize <= 0) return;

            // 极限防护：超过预分配容量时丢弃该帧
            if (bufferSize > MAX_FRAME_SIZE)
            {
                Debug.WriteLine($"[Lmi3D] 警告: 当前帧大小({bufferSize} byte) 超出最大缓冲容量({MAX_FRAME_SIZE} byte)。已丢弃该帧。");
                return;
            }

            ImageInfo.Width = width;
            ImageInfo.Height = length;
            ImageInfo.Stride = width * sizeof(short);
            ImageInfo.PixelFormat = CamPixelFormat.Depth;

            int currentIndex;
            lock (_bufferLock)
            {
                currentIndex = _bufferIndex;
                IntPtr safeDestPtr = _depthHandles[currentIndex].AddrOfPinnedObject();

                unsafe
                {
                    // 将 SDK 数据拷贝到托管固定内存中，脱离 LMI 生命周期
                    Buffer.MemoryCopy(dataPtr.ToPointer(), safeDestPtr.ToPointer(), MAX_FRAME_SIZE, bufferSize);
                }

                // 更新索引，准备下一次写入
                _bufferIndex = (_bufferIndex + 1) % BufferCount;
            }

            // 在锁外调用上层回调，避免长时间持锁
            ActionGetImage?.Invoke(_depthHandles[currentIndex].AddrOfPinnedObject());
        }

        #endregion

        public void Dispose()
        {
            CloseDevice();

            // 释放固定内存句柄
            for (int i = 0; i < BufferCount; i++)
            {
                if (_depthHandles[i].IsAllocated)
                    _depthHandles[i].Free();
            }
            GC.SuppressFinalize(this);
        }
    }
}
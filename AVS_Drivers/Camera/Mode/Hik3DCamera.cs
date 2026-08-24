using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.CameraSDKHelper.Common.Enum;

namespace AVS_Drivers.Camera.Mode
{
    internal class Hik3DCamera : BaseCamera, IDisposable
    {
        private IntPtr _handle = IntPtr.Zero;
        private ImageDataCallBackHandle _imageCallback;
        private bool _isGrabbing = false;

        // 缓冲区
        private byte[] _depthBuffer = new byte[1024 * 1024 * 50]; // 默认50MB
        private byte[] _intensityBuffer = new byte[1024 * 1024 * 50];
        private readonly object _bufferLock = new object();

        // 图像模式枚举（对应SDK）
        private enum Mv3dLpImageMode
        {
            MV3D_LP_Origin_Image = 1,
            MV3D_LP_Point_Cloud_Image = 4,
            MV3D_LP_Range_Image = 7
        }

        public Hik3DCamera() : base()
        {
        }

        #region 设备枚举与初始化

        public override List<string> GetListEnum()
        {
            var snList = new List<string>();
            try
            {
                uint devNum = 0;
                int nRet = Mv3dLpSDK.MV3D_LP_GetDeviceNumber(ref devNum);
                if (nRet != Mv3dLpSDK.MV3D_LP_OK || devNum == 0)
                    return snList;

                var deviceInfoArray = new MV3D_LP_DEVICE_INFO[devNum];
                for (int i = 0; i < devNum; i++)
                    deviceInfoArray[i] = new MV3D_LP_DEVICE_INFO();

                uint filledCount = devNum;
                nRet = Mv3dLpSDK.MV3D_LP_GetDeviceList(deviceInfoArray[0], devNum, ref filledCount);
                if (nRet != Mv3dLpSDK.MV3D_LP_OK)
                    return snList;

                for (int i = 0; i < filledCount; i++)
                {
                    string sn = deviceInfoArray[i].chSerialNumber?.TrimEnd('\0');
                    if (!string.IsNullOrEmpty(sn))
                        snList.Add(sn);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Hik3DCamera] EnumDevices exception: {ex.Message}");
            }
            return snList;
        }

        public override bool InitDevice(string CamSN)
        {
            if (string.IsNullOrEmpty(CamSN))
                return false;

            try
            {
                int nRet = Mv3dLpSDK.MV3D_LP_OpenDeviceBySN(ref _handle, CamSN);
                if (nRet != Mv3dLpSDK.MV3D_LP_OK)
                {
                    Debug.WriteLine($"[Hik3DCamera] OpenDeviceBySN failed: 0x{nRet:X}");
                    return false;
                }

                // 设置图像模式为深度图
                nRet = SetDepthImageMode();
                if (nRet != Mv3dLpSDK.MV3D_LP_OK)
                {
                    Debug.WriteLine($"[Hik3DCamera] SetDepthImageMode failed: 0x{nRet:X}");
                    Mv3dLpSDK.MV3D_LP_CloseDevice(ref _handle);
                    _handle = IntPtr.Zero;
                    return false;
                }

                // 注册回调
                _imageCallback = new ImageDataCallBackHandle(this);
                nRet = _imageCallback.Register(_handle);
                if (nRet != Mv3dLpSDK.MV3D_LP_OK)
                {
                    Debug.WriteLine($"[Hik3DCamera] Register callback failed: 0x{nRet:X}");
                    Mv3dLpSDK.MV3D_LP_CloseDevice(ref _handle);
                    _handle = IntPtr.Zero;
                    return false;
                }

                // 初始图像信息（将在首次回调中更新）
                ImageInfo.Width = 0;
                ImageInfo.Height = 0;
                ImageInfo.Stride = 0;
                ImageInfo.PixelFormat = CamPixelFormat.Depth; // 默认深度图

                SN = CamSN;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Hik3DCamera] InitDevice exception: {ex.Message}");
                if (_handle != IntPtr.Zero)
                {
                    Mv3dLpSDK.MV3D_LP_CloseDevice(ref _handle);
                    _handle = IntPtr.Zero;
                }
                return false;
            }
        }

        public override void CloseDevice()
        {
            try
            {
                if (_isGrabbing)
                {
                    StopGrabbing();
                }

                if (_imageCallback != null && _handle != IntPtr.Zero)
                {
                    _imageCallback.UnRegister(_handle);
                    _imageCallback.Dispose();
                    _imageCallback = null;
                }

                if (_handle != IntPtr.Zero)
                {
                    Mv3dLpSDK.MV3D_LP_CloseDevice(ref _handle);
                    _handle = IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Hik3DCamera] CloseDevice exception: {ex.Message}");
            }
        }

        #endregion

        #region 采集控制

        protected override bool StartGrabbing()
        {
            if (_handle == IntPtr.Zero)
                return false;

            int nRet = Mv3dLpSDK.MV3D_LP_StartMeasure(_handle);
            if (nRet != Mv3dLpSDK.MV3D_LP_OK)
            {
                Debug.WriteLine($"[Hik3DCamera] StartMeasure failed: 0x{nRet:X}");
                return false;
            }
            _isGrabbing = true;
            return true;
        }

        protected override bool StopGrabbing()
        {
            if (_handle == IntPtr.Zero)
                return false;

            int nRet = Mv3dLpSDK.MV3D_LP_StopMeasure(_handle);
            if (nRet != Mv3dLpSDK.MV3D_LP_OK)
            {
                Debug.WriteLine($"[Hik3DCamera] StopMeasure failed: 0x{nRet:X}");
                return false;
            }
            _isGrabbing = false;
            return true;
        }

        public override bool SoftTrigger()
        {
            if (_handle == IntPtr.Zero)
                return false;

            int nRet = Mv3dLpSDK.MV3D_LP_SoftTrigger(_handle);
            return nRet == Mv3dLpSDK.MV3D_LP_OK;
        }

        public override bool Continue_SoftTrigger()
        {
            // 3D相机不支持连续软触发，直接返回true（兼容接口）
            return true;
        }

        #endregion

        #region 参数设置

        public override bool SetTriggerMode(TriggerMode mode, TriggerSource triggerEnum = TriggerSource.Line0)
        {
            // 3D相机通常只支持软触发/硬触发，不支持连续模式（TriggerMode.Off）
            // 因此这里仅处理软触发模式，其他返回false，上层会回退到软触发
            if (mode == TriggerMode.On && triggerEnum == TriggerSource.Software)
            {
                // 假设相机默认支持软触发，无需额外设置
                return true;
            }
            // 若需支持硬触发，请查阅SDK文档并设置相应参数
            return false;
        }

        public override bool GetTriggerMode(out TriggerMode mode, out TriggerSource hardTriggerModel)
        {
            // 默认返回软触发
            mode = TriggerMode.On;
            hardTriggerModel = TriggerSource.Software;
            return true;
        }

        public override bool SetExpouseTime(ushort value)
        {
            try
            {
                MV3D_LP_PARAM param = new MV3D_LP_PARAM();
                param.enParamType = Mv3dLpSDK.ParamType_Float;
                MV3D_LP_FLOATPARAM floatParam = new MV3D_LP_FLOATPARAM();
                floatParam.fCurValue = value;
                param.set_floatparam(floatParam);
                int nRet = Mv3dLpSDK.MV3D_LP_SetParam(_handle, Mv3dLpSDK.MV3D_LP_FLOAT_EXPOSURETIME, param);
                return nRet == Mv3dLpSDK.MV3D_LP_OK;
            }
            catch { return false; }
        }

        public override bool GetExpouseTime(out ushort value)
        {
            value = 0;
            try
            {
                MV3D_LP_PARAM param = new MV3D_LP_PARAM();
                param.enParamType = Mv3dLpSDK.ParamType_Float;
                int nRet = Mv3dLpSDK.MV3D_LP_GetParam(_handle, Mv3dLpSDK.MV3D_LP_FLOAT_EXPOSURETIME, param);
                if (nRet == Mv3dLpSDK.MV3D_LP_OK)
                {
                    MV3D_LP_FLOATPARAM floatParam = param.get_floatparam();
                    value = (ushort)floatParam.fCurValue;
                }
                return nRet == Mv3dLpSDK.MV3D_LP_OK;
            }
            catch { return false; }
        }

        public override bool SetGain(short gain)
        {
            try
            {
                MV3D_LP_PARAM param = new MV3D_LP_PARAM();
                param.enParamType = Mv3dLpSDK.ParamType_Float;
                MV3D_LP_FLOATPARAM floatParam = new MV3D_LP_FLOATPARAM();
                floatParam.fCurValue = gain;
                param.set_floatparam(floatParam);
                // 注意：增益参数键名可能不是"Gain"，请查阅SDK文档调整
                int nRet = Mv3dLpSDK.MV3D_LP_SetParam(_handle, "Gain", param);
                return nRet == Mv3dLpSDK.MV3D_LP_OK;
            }
            catch { return false; }
        }

        public override bool GetGain(out short gain)
        {
            gain = 0;
            try
            {
                MV3D_LP_PARAM param = new MV3D_LP_PARAM();
                param.enParamType = Mv3dLpSDK.ParamType_Float;
                int nRet = Mv3dLpSDK.MV3D_LP_GetParam(_handle, "Gain", param);
                if (nRet == Mv3dLpSDK.MV3D_LP_OK)
                {
                    MV3D_LP_FLOATPARAM floatParam = param.get_floatparam();
                    gain = (short)floatParam.fCurValue;
                }
                return nRet == Mv3dLpSDK.MV3D_LP_OK;
            }
            catch { return false; }
        }

        // 以下参数3D相机暂不支持，返回false
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

        #region 内部方法

        private int SetDepthImageMode()
        {
            MV3D_LP_PARAM pstValue = new MV3D_LP_PARAM();
            MV3D_LP_ENUMPARAM enumParam = new MV3D_LP_ENUMPARAM();
            enumParam.nCurValue = (uint)Mv3dLpImageMode.MV3D_LP_Range_Image;
            pstValue.set_enumparam(enumParam);
            return Mv3dLpSDK.MV3D_LP_SetParam(_handle, "ImageMode", pstValue);
        }

        #endregion

        #region 回调处理

        private class ImageDataCallBackHandle : ImageDataCallBack
        {
            private readonly Hik3DCamera _camera;

            public ImageDataCallBackHandle(Hik3DCamera camera)
            {
                _camera = camera;
            }

            public override void run(MV3D_LP_IMAGE_DATA pstImageData)
            {
                if (pstImageData == null || !_camera._isGrabbing)
                    return;

                try
                {
                    // 更新图像信息（根据实际数据类型动态调整）
                    _camera.UpdateImageInfo(pstImageData);

                    // 处理深度数据并传递
                    if (pstImageData.pData != IntPtr.Zero && pstImageData.nDataLen > 0)
                    {
                        // 复制深度数据到内部缓冲区，并固定内存
                        byte[] depthBuf = _camera.GetDepthBuffer((int)pstImageData.nDataLen);
                        Marshal.Copy(pstImageData.pData, depthBuf, 0, (int)pstImageData.nDataLen);
                        GCHandle depthHandle = GCHandle.Alloc(depthBuf, GCHandleType.Pinned);
                        try
                        {
                            IntPtr depthPtr = depthHandle.AddrOfPinnedObject();
                            // 调用上层回调
                            _camera.ActionGetImage?.Invoke(depthPtr);
                        }
                        finally
                        {
                            depthHandle.Free();
                        }
                    }

                    // 可选：处理亮度数据（暂存，后续可扩展事件）
                    if (pstImageData.pIntensityData != IntPtr.Zero && pstImageData.nIntensityDataLen > 0)
                    {
                        byte[] intensityBuf = _camera.GetIntensityBuffer((int)pstImageData.nIntensityDataLen);
                        Marshal.Copy(pstImageData.pIntensityData, intensityBuf, 0, (int)pstImageData.nIntensityDataLen);
                        GCHandle intensityHandle = GCHandle.Alloc(intensityBuf, GCHandleType.Pinned);
                        try
                        {
                            IntPtr intensityPtr = intensityHandle.AddrOfPinnedObject();
                            _camera.OnIntensityImageReceived(intensityPtr);
                        }
                        finally
                        {
                            intensityHandle.Free();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Hik3DCamera] Callback run exception: {ex.Message}");
                }
            }
        }

        private void UpdateImageInfo(MV3D_LP_IMAGE_DATA data)
        {
            lock (_bufferLock)
            {
                ImageInfo.Width = (int)data.nWidth;
                ImageInfo.Height = (int)data.nHeight;

                // 根据图像类型设置像素格式
                if (data.enImageType == Mv3dLpSDK.ImageType_Depth)
                {
                    ImageInfo.PixelFormat = CamPixelFormat.Depth;
                    ImageInfo.Stride = ImageInfo.Width * 2; // int2每像素2字节
                }
                else if (data.enImageType == Mv3dLpSDK.ImageType_Mono8)
                {
                    ImageInfo.PixelFormat = CamPixelFormat.Mono8;
                    ImageInfo.Stride = ImageInfo.Width;
                }
                else
                {
                    // 默认为深度图
                    ImageInfo.PixelFormat = CamPixelFormat.Depth;
                    ImageInfo.Stride = ImageInfo.Width * 2;
                }
            }
        }

        private byte[] GetDepthBuffer(int requiredSize)
        {
            lock (_bufferLock)
            {
                if (_depthBuffer.Length < requiredSize)
                    _depthBuffer = new byte[requiredSize];
                return _depthBuffer;
            }
        }

        private byte[] GetIntensityBuffer(int requiredSize)
        {
            lock (_bufferLock)
            {
                if (_intensityBuffer.Length < requiredSize)
                    _intensityBuffer = new byte[requiredSize];
                return _intensityBuffer;
            }
        }

        #endregion

        public void Dispose()
        {
            CloseDevice();
        }
    }
}
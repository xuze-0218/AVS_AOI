using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.CameraSDKHelper.Common.Enum;

namespace AVS_Drivers.Camera.Mode
{
    public class Hik3DCamera : BaseCamera, IDisposable
    {
        private IntPtr _handle = IntPtr.Zero;
        private ImageDataCallBackHandle _imageCallback;
        private volatile bool _isGrabbing = false;
        // 缓冲区
        private byte[] _depthBuffer = new byte[1024 * 1024 * 20]; // 默认20MB
        private byte[] _intensityBuffer = new byte[1024 * 1024 * 20];
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
                if (ex.InnerException != null)
                    Console.WriteLine("内部异常: " + ex.InnerException.Message);
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
                    StopGrabbingCore();
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
        protected override bool StartGrabbingCore()
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
        protected override bool StopGrabbingCore()
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
            return true;
        }
        #endregion
        #region 参数设置
        public override bool SetTriggerMode(TriggerMode mode, TriggerSource triggerEnum = TriggerSource.Line0)
        {
          
            if (mode == TriggerMode.On && triggerEnum == TriggerSource.Software)
            {
                return true;
            }
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
                int nRet = Mv3dLpSDK.MV3D_LP_SetParam(_handle, Mv3dLpSDK.MV3D_LP_FLOAT_GAIN, param);
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
                int nRet = Mv3dLpSDK.MV3D_LP_GetParam(_handle, Mv3dLpSDK.MV3D_LP_FLOAT_GAIN, param);
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

                if (pstImageData.bValid == 0)
                    return;

                try
                {
                    // 更新图像信息（根据实际数据类型动态调整）
                    _camera.UpdateImageInfo(pstImageData);

                    // 处理深度数据并传递
                    if (pstImageData.pData != IntPtr.Zero && pstImageData.nDataLen > 0)
                    {
                        _camera.ProcessDepthImage(pstImageData.pData, (int)pstImageData.nDataLen);
                    }

                    // 可选：处理亮度数据（暂存，后续可扩展事件）
                    if (pstImageData.pIntensityData != IntPtr.Zero && pstImageData.nIntensityDataLen > 0)
                    {
                        _camera.ProcessIntensityImage(pstImageData.pIntensityData, (int)pstImageData.nIntensityDataLen);
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

        private void ProcessDepthImage(IntPtr pData, int dataLen)
        {
            lock (_bufferLock)
            {
                if (_depthBuffer.Length < dataLen)
                    _depthBuffer = new byte[dataLen];
                Marshal.Copy(pData, _depthBuffer, 0, dataLen);
                GCHandle handle = GCHandle.Alloc(_depthBuffer, GCHandleType.Pinned);
                try
                {
                    ActionGetImage?.Invoke(handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
            }
        }

        private void ProcessIntensityImage(IntPtr pData, int dataLen)
        {
            lock (_bufferLock)
            {
                if (_intensityBuffer.Length < dataLen)
                    _intensityBuffer = new byte[dataLen];
                Marshal.Copy(pData, _intensityBuffer, 0, dataLen);
                GCHandle handle = GCHandle.Alloc(_intensityBuffer, GCHandleType.Pinned);
                try
                {
                    OnIntensityImageReceived(handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
            }
        }

        #endregion

        public void Dispose()
        {
            CloseDevice();
        }
    }
}
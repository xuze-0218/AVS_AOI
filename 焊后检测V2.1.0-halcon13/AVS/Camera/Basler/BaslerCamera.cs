using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Basler.Pylon;
using HalconDotNet;


namespace AVS
{
    public class BaslerCamera:ICamera
    {
        #region 1、全局变量
        //PYLON_DEVICE_HANDLE camera;                       // 相机句柄
        public Camera camera;                               // 相机句柄
        
        private bool isGrabOver = false;                    // 相机是否正在采集图像
        //private bool nRet;                                // 相机参数设置是否成功
       
        //private ImageProvider m_imageProvider = new ImageProvider();

        private PixelDataConverter converter = new PixelDataConverter();

        private IntPtr latestFramePtrAddress = IntPtr.Zero;
        private Version Sfnc2_0_0 = new Version(2, 0, 0);
        private static bool isGrabOk = false;

        public string ReturnString;
        public HObject Image { get { return image; } }
        private HObject image = null;
        #endregion

        #region 2、接口方法
        public override int Connect()
        {
            try
            {
                if (CameraInit() < 0)
                {
                    IsConnect = false;
                    return -1;
                }
                if (camera != null)
                {
                    if (camera.IsOpen) camera.Close();
                    Thread.Sleep(200);
                    camera.Open();
                    if(IsTigger)
                    {
                        if(TriggerSource == 0)
                        {
                            camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Software);
                        }
                        else if(TriggerSource == 1)
                        {
                            camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                            camera.Parameters[PLStream.MaxNumBuffer].SetValue(15);
                            //camera.Parameters[PLCamera.AcquisitionFrameCount].TrySetToMaximum();
                            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                            camera.Parameters[PLCamera.LineSelector].TrySetValue(PLCamera.LineSelector.Line1);
                            camera.Parameters[PLCamera.LineDebouncerTimeAbs].SetValue(100);
                            camera.Parameters[PLCamera.LineSelector].TrySetValue(PLCamera.LineSelector.Out1);
                            camera.Parameters[PLCamera.LineMode].TrySetValue(PLCamera.LineMode.Output);
                            camera.Parameters[PLCamera.LineSource].TrySetValue(PLCamera.LineSource.ExposureActive);
                            camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                        }
                    }
                    else
                    {
                        camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                        camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);
                    }
                    Global.AddLog("相机_" + IpAddress + "_连接成功!");
                    IsConnect = true;
                    return 1;
                }
                else
                {
                    IsConnect = false;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Global.AddLog("Basler相机:" + IpAddress + "_连接失败:\r\n" + ex.Message.ToString());
                IsConnect = false;
                return -1;
            }
        }

        public override int DisConnect()
        {
            try
            {
                if (camera != null)
                {
                    StopGrab();
                    camera.Close();
                    if (latestFramePtrAddress != null)
                    {
                        Marshal.FreeHGlobal(latestFramePtrAddress);
                        latestFramePtrAddress = IntPtr.Zero;
                    }                  
                }
                else
                {                    
                    return 1;
                }
                IsConnect = false;
            }
            catch (Exception ex)
            {
                Global.AddLog("相机_" + IpAddress + "_关闭出错:\r\n" + ex.Message.ToString());
                return -1;
            }
            return 1;
        }

        public bool SoftTrigger()
        {
            try
            {
                isGrabOk = false;
                camera.ExecuteSoftwareTrigger();
                return true;
            }
            catch (Exception ex)
            {
                Global.AddLog("相机_" + IpAddress + "_软触发取图出错:\r\n" + ex.Message.ToString());
                return false;
            }
        }

        public override int GrabImage()
        {
            try
            {
                isGrabOk = false;
                if(StartGrab())
                {
                    camera.ExecuteSoftwareTrigger();
                    IGrabResult grabResult = camera.StreamGrabber.RetrieveResult(900000, TimeoutHandling.ThrowException);
                    if (grabResult.IsValid)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }                
            }
            catch (Exception ex)
            {
                Global.AddLog("相机_" + IpAddress + "_软触发取图出错:\r\n" + ex.Message.ToString());
                return -1;
            }           
        }

        public bool GrabPicture()
        {
            try
            {
                isGrabOk = false;
                if (camera.StreamGrabber.IsGrabbing)
                {
                    Global.AddLog("相机_" + IpAddress + "_正处于图像采集状态！");
                    return false;
                }
                else
                {
                    camera.StreamGrabber.Start(1, GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
                    int timeOut = 0;
                    while (!isGrabOk)
                    {
                        timeOut++;
                        Thread.Sleep(10);
                        if (timeOut > 200)
                        { isGrabOk = true; }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Global.AddLog("相机_" + IpAddress + "_采图失败:\r\n" + ex.Message.ToString());
                return false;
            }
        }

        public bool StartGrab()
        {
            try
            {
                camera.StreamGrabber.Start();
            }
            catch(Exception ex)
            {
                Global.AddLog("相机-" + IpAddress + "_开始采图出错:\r\n" + ex.Message.ToString());
                return false;
            }
            return true;
        }

        public bool StopGrab()
        {
            try
            {
                camera.StreamGrabber.Stop();
            }
            catch (Exception ex)
            {
                Global.AddLog("相机-" + IpAddress + "_停止采图出错:\r\n" + ex.Message.ToString());
                return false;
            }
            return true;
        }
        #endregion

        #region 3、辅助方法

        private int CameraSearch()
        {
            try
            {
                // 枚举相机列表
                List<ICameraInfo> allCameraInfos = CameraFinder.Enumerate();
                if(allCameraInfos.Count<1)
                {
                    //returnString = GetErrorMsg("未连接到Basler相机!", 0);
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("相机:" + IpAddress + "_初始化出错：\r\n" + ex.Message.ToString());
                return -1;
            }
        }
        #endregion

        private int CameraInit()
        {
            try
            {
                // 枚举相机列表
                List<ICameraInfo> allCameraInfos = CameraFinder.Enumerate();
                if (allCameraInfos.Count < 1)
                {
                    ReturnString = GetErrorMsg("Basler相机:" + IpAddress + "_未连接!", 0);
                    Global.AddLog("Basler相机:" + IpAddress + "_未连接!");
                    return -1;
                }

                foreach (ICameraInfo cameraInfo in allCameraInfos)
                {
                    if (IpAddress == cameraInfo[CameraInfoKey.DeviceIpAddress])
                    {
                        camera = new Camera(cameraInfo);
                        camera.StreamGrabber.ImageGrabbed -= StreamGrabber_ImageGrabbed;
                        camera.StreamGrabber.ImageGrabbed += StreamGrabber_ImageGrabbed;
                        camera.ConnectionLost -= OnConnectionLost;
                        camera.ConnectionLost += OnConnectionLost;
                        break;
                    }
                }
                if (camera == null)
                {
                    Global.AddLog("相机" + IpAddress + "_未找到!");
                    return -1;
                }
                return 1;
            }
            catch
            {
                return -1;
            }
        }

        private void StreamGrabber_ImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                IGrabResult grabResult = e.GrabResult;
                HOperatorSet.GenEmptyObj(out image);
                // Acquire the image from the camera. Only show the latest image. The camera may acquire images faster than the images can be displayed.
                // Get the grab result.
                // Check if the image can be displayed.
                if (grabResult.IsValid)
                {
                    //grabTime = sw.ElapsedMilliseconds;
                    //if (eventComputeGrabTime != null) eventComputeGrabTime(grabTime);
                    //Reduce the number of displayed images to a reasonable amount if the camera is acquiring images very fast.
                    // ****  降低显示帧率，减少CPU占用率  **** //
                    //if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 33)
                    {
                        //stopWatch.Restart();
                        // 判断是否是黑白图片格式
                        if (grabResult.PixelTypeValue == PixelType.Mono8)
                        {
                            //allocate the m_stream_size amount of bytes in non-managed environment 
                            if (latestFramePtrAddress == IntPtr.Zero)
                            {
                                latestFramePtrAddress = Marshal.AllocHGlobal((Int32)grabResult.PayloadSize);
                            }
                            converter.OutputPixelFormat = PixelType.Mono8;
                            converter.Convert(latestFramePtrAddress, grabResult.PayloadSize, grabResult);

                            // 转换为Halcon图像显示
                            HOperatorSet.GenImage1(out image, "byte", grabResult.Width, grabResult.Height, latestFramePtrAddress);
                            //触发
                            callBackFunction(image);
                        }
                        else if (grabResult.PixelTypeValue == PixelType.BayerBG8 || grabResult.PixelTypeValue == PixelType.BayerGB8
                                    || grabResult.PixelTypeValue == PixelType.BayerRG8 || grabResult.PixelTypeValue == PixelType.YUV422packed)
                        {
                            int imageWidth = grabResult.Width - 1;
                            int imageHeight = grabResult.Height - 1;
                            int payloadSize = imageWidth * imageHeight;
                            //allocate the m_stream_size amount of bytes in non-managed environment 
                            if (latestFramePtrAddress == IntPtr.Zero)
                            {
                                latestFramePtrAddress = Marshal.AllocHGlobal((Int32)(3 * payloadSize));
                            }
                            converter.OutputPixelFormat = PixelType.RGB8packed;     // 根据下面halcon转换的色彩格式bgr
                            converter.Parameters[PLPixelDataConverter.InconvertibleEdgeHandling].SetValue("Clip");
                            converter.Convert(latestFramePtrAddress, 3 * payloadSize, grabResult);
                            
                            HOperatorSet.GenImageInterleaved(out image, latestFramePtrAddress, "rgb",
                             (HTuple)imageWidth, (HTuple)imageHeight, -1, "byte", (HTuple)imageWidth, (HTuple)imageHeight, 0, 0, -1, 0);

                            callBackFunction(image);
                        }
                        else
                        {
                            //NotifyG.Error(DeviceName + "拍照失败,相机图像格式设置");
                            isGrabOk = true;
                            return;
                        }
                        isGrabOk = true;
                        // 抛出图像处理事件
                        //if (EventGrab != null) EventGrab(this, new CameraGrabEventArgs(image));
                    }
                }
            }
            catch (Exception ex)
            {
                isGrabOk = true;
                Global.AddLog("Basler相机:" + IpAddress + "_取图失败:\r\n" + ex.Message.ToString());
            }
            finally
            {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
                isGrabOk = true;
            }
        }

        private void OnConnectionLost(Object sender, EventArgs e)
        {
            try
            {
                Global.AddLog(IpAddress + "_相机掉线");
                camera.Close();
                for (int i = 0; i < 100; i++)
                {
                    try
                    {
                        camera.Open();
                        //camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                        //camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);
                        camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                        //camera.Parameters[PLCamera.AcquisitionFrameCount].TrySetToMinimum();
                        camera.Parameters[PLCamera.AcquisitionFrameCount].SetToMaximum();
                        camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                        camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                        //camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
                        camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);

                        camera.Parameters[PLCamera.LineSelector].TrySetValue(PLCamera.LineSelector.Out1);
                        camera.Parameters[PLCamera.LineMode].TrySetValue(PLCamera.LineMode.Output);
                        camera.Parameters[PLCamera.LineSource].TrySetValue(PLCamera.LineSource.ExposureActive);
                        camera.Parameters[PLCamera.LineSelector].TrySetValue(PLCamera.LineSelector.Out2);
                        camera.Parameters[PLCamera.LineMode].TrySetValue(PLCamera.LineMode.Output);
                        camera.Parameters[PLCamera.LineSource].TrySetValue(PLCamera.LineSource.ExposureActive);

                        if (camera.IsOpen)
                        {
                            Global.AddLog("相机:"+ IpAddress + "_已重新连接!");
                            break;
                        }
                        Thread.Sleep(200);
                    }
                    catch
                    {
                        Global.AddLog("相机:" + IpAddress + "_重连失败，稍后尝试重连!");
                    }
                }

                if (camera == null)
                {
                    Global.AddLog("相机:" + IpAddress + "_重连超时，请检查后重连");
                }

                SetHeartBeatTime(1000);
                camera.StreamGrabber.ImageGrabbed -= StreamGrabber_ImageGrabbed;    // 注册采集回调函数
                camera.StreamGrabber.ImageGrabbed += StreamGrabber_ImageGrabbed;    // 注册采集回调函数
                camera.ConnectionLost -= OnConnectionLost;
                camera.ConnectionLost += OnConnectionLost;
            }
            catch (Exception ex)
            {
                Global.AddLog("相机:" + IpAddress + "_重连失败，请检查后重连\r\n" + ex.Message.ToString());
            }
        }
        public void SetHeartBeatTime(long value)
        {
            try
            {
                if (camera.GetSfncVersion() < Sfnc2_0_0)
                {
                    camera.Parameters[PLGigECamera.GevHeartbeatTimeout].SetValue(value);
                }
            }
            catch (Exception ex)
            {
                Global.AddLog("相机" + IpAddress + "_心跳设置出错：" + ex.Message.ToString());
            }
        }
        public override bool SetExposure(long exposureTime)
        {
            try
            {
                if(camera != null)
                {
                    camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(exposureTime);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private void StreamGrabber_GrabStarted(object sender, EventArgs e)
        {
            isGrabOver = true;
        }

        private void StreamGrabber_ImageGrabbed____(object sender, ImageGrabbedEventArgs e)
        {
            IGrabResult grabResult = e.GrabResult;
            if (grabResult.IsValid)
            {
                if (isGrabOver)
                {
                    //CameraImageEvent(GrabResult2Bmp(grabResult));
                }
            }
        }

        private void StreamGrabber_GrabStopped(object sender, GrabStopEventArgs e)
        {
            isGrabOver = false;
        }

        private void Camera_ConnectionLost(object sender, EventArgs e)
        {
            camera.StreamGrabber.Stop();
            DestroyCamera();
        }

        public void OneShot()
        {
            if (camera != null)
            {
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
        }
        /// <summary>
        /// 相机实时功能
        /// </summary>
        public void KeepShot()
        {
            if (camera != null)
            {
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
        }

        public void Stop()
        {
            if (camera != null)
            {
                camera.StreamGrabber.Stop();
            }
        }

        //释放相机
        public void DestroyCamera()
        {
            if (camera != null)
            {
                camera.Close();
                camera.Dispose();
                camera = null;
            }
        }

        private string GetErrorMsg(string csMessage, long errCode)
        {
            string errorMsg;
            if (errCode == 0)
            {
                errorMsg = csMessage;
                return errorMsg;
            }
            else
                //返回详细的错误信息
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", errCode);
            switch (errCode)
            {
                case 0xC2000001: errorMsg += " Unspecified error occurred "; break;
                case 0xC200000C: errorMsg += " An index is out of range "; break;
                case 0xC2000003: errorMsg += " Buffer size passed is less than required "; break;
                case 0xC2000002: errorMsg += " Function called with invalid argument "; break;
                case 0xC2000011: errorMsg += " An invalid file handle has been passed "; break;
                case 0xC200000F: errorMsg += " An invalid callback handle has been passed "; break;
                case 0xC2000006: errorMsg += " An invalid node handle has been passed "; break;
                case 0xC2000004: errorMsg += " An invalid node map handle has been passed "; break;
                case 0xC2000008: errorMsg += " The value exceeds the valid range "; break;
                case 0xC2000010: errorMsg += " Program logic error.Call GenApiGetLastErrorDetail() for more information about the error "; break;
                case 0xC2000005: errorMsg += " Specified node not found in node map "; break;
                case 0xC200000E: errorMsg += " Object state illegal for operation.Call GenApiGetLastErrorDetail() for more information about the error "; break;
                case 0x0: errorMsg += " Operation completed successfully "; break;
                case 0xC2000009: errorMsg += " Generic GenlCam property error occurred.Call GenApiGetLastErrorDetail() for more information about the error "; break;
                case 0xC2000007: errorMsg += " A 64 bit result will be truncated if returned as a 32 bit value  "; break;
                case 0xC200000A: errorMsg += " Timeout expired "; break;
                case 0xC200000B: errorMsg += " Expression has wrong type "; break;
                case 0xC3000005: errorMsg += " An invalid ChunkParser handle has been passed "; break;
                case 0xC3000009: errorMsg += " An invalid Converter handle has been passed "; break;
                case 0xC3000001: errorMsg += " An invalid Device handle has been passed "; break;
                case 0xC3000003: errorMsg += " An invalid DeviceinfoProperty handle has been passed "; break;
                case 0xC3000002: errorMsg += " An invalid Deviceinfo handle has been passed "; break;
                case 0xC3000008: errorMsg += " An invalid EventAdapter handle has been passed "; break;
                case 0xC3000007: errorMsg += " An invalid EventGrabber handle has been passed "; break;
                case 0xC300000C: errorMsg += " An invalid Interface handle has been passed "; break;
                case 0xC300000D: errorMsg += " An invalid InterfaceInfo handle has been passed "; break;
                case 0xC3000004: errorMsg += " An invalid StreamGrabber handle has been passed "; break;
                case 0xC3000006: errorMsg += " An invalid WaitObject handle has been passed "; break;
                case 0xC300000A: errorMsg += " An invalid WaitObjects handle has been passed "; break;
            }
            return errorMsg;
        }

    }
}

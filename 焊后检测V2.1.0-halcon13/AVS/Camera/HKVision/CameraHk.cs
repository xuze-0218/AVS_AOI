using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MvCamCtrl.NET.CameraHkSdk;
using HalconDotNet;


namespace AVS
{
    public class CameraHk:ICamera
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private Object myBufForImageLock = new Object();
        private CameraHkSdk.cbOutputExdelegate callbackImage;
        private static CameraHkSdk.MV_CC_DEVICE_INFO_LIST myDeviceList;
        //MvCamera.MV_CC_DEVICE_INFO myDeviceInfo;
        private CameraHkSdk myCamera;
        private HObject hImage;
     
        private bool isImgGrabDone = false;
               
        //回调函数
        private void ImageCallBack(IntPtr pData, ref CameraHkSdk.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            try
            {
                isImgGrabDone = false;

                GCHandle handle = GCHandle.FromIntPtr(pUser);

                //CameraHk ca = (CameraHk)handle.Target;

               

                //handle.Free();不能释放，否则再次取图像找不到

                lock (myBufForImageLock)
                {
                    //ca.imgMat = new Mat(pFrameInfo.nHeight, pFrameInfo.nWidth, MatType.CV_8UC3);
                    //CopyMemory(ca.imgMat.Data, pData, pFrameInfo.nFrameLen);
                    //ca.imgg = new Mat(pFrameInfo.nHeight, pFrameInfo.nWidth, MatType.CV_8UC3, pData);
                    //myFrameInfo = pFrameInfo;
                    if (IsMono(pFrameInfo.enPixelType))
                    {
                        //img[0] = new Mat(pFrameInfo.nHeight, pFrameInfo.nWidth, MatType.CV_8UC1, pData);
                        // 转换为Halcon图像显示
                        HOperatorSet.GenImage1(out hImage, "byte", pFrameInfo.nWidth, pFrameInfo.nHeight, pData);
                        //触发
                        callBackFunction(hImage);
                    }
                    else if((IsColor(pFrameInfo.enPixelType)))
                    {
                        HOperatorSet.GenImage1(out hImage, "byte", pFrameInfo.nHeight, pFrameInfo.nWidth, pData);
                        //触发
                        callBackFunction(hImage);
                        //img[0] = new Mat(pFrameInfo.nHeight, pFrameInfo.nWidth, MatType.CV_8UC3, pData);
                    }
                }

                Marshal.Release(pData);

                //Cv2.ImWrite("A" + 1.ToString() + ".jpg", ca.imgMat);

                isImgGrabDone = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("相机回调函数出错：\r\n" + ex.Message.ToString());
            }
        }

        //相机初始化函数
        public bool CameraInitial()
        {
            try
            {
                //imgMats = new Mat[1] { new Mat() };
                IsConnect = false;
                myDeviceList = new CameraHkSdk.MV_CC_DEVICE_INFO_LIST();
                System.GC.Collect();
                int nRet = CameraHkSdk.MV_CC_EnumDevices_NET(CameraHkSdk.MV_GIGE_DEVICE | CameraHkSdk.MV_USB_DEVICE, ref myDeviceList);
                if (0 != nRet)
                {
                    return false;
                }
                myCamera = new CameraHkSdk();
                callbackImage = new CameraHkSdk.cbOutputExdelegate(ImageCallBack);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("相机初始化出错：\r\n" + ex.Message.ToString());
                return false;
            }
        }

        //连接相机
        public override int Connect()
        {
            try
            {
                if(!CameraInitial())
                {
                    IsConnect = false;
                    return -1;
                }
                IsConnect = false;
                int nRet = CameraHkSdk.MV_OK;
                for (int i = 0; i < (int)myDeviceList.nDeviceNum; i++)
                {
                    //ch:获取选择的设备信息 | en:Get Selected Device Information
                    CameraHkSdk.MV_CC_DEVICE_INFO device = (CameraHkSdk.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(myDeviceList.pDeviceInfo[i], typeof(CameraHkSdk.MV_CC_DEVICE_INFO));

                    if (device.nTLayerType == CameraHkSdk.MV_GIGE_DEVICE)
                    {
                        CameraHkSdk.MV_GIGE_DEVICE_INFO gigeInfo = (CameraHkSdk.MV_GIGE_DEVICE_INFO)CameraHkSdk.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(CameraHkSdk.MV_GIGE_DEVICE_INFO));

                        uint[] nIp = new uint[4] { (gigeInfo.nCurrentIp & 0xff000000) >> 24, (gigeInfo.nCurrentIp & 0x00ff0000) >> 16, (gigeInfo.nCurrentIp & 0x0000ff00) >> 8, gigeInfo.nCurrentIp & 0x000000ff };
                        //uint nIp1 = ((gigeInfo.nCurrentIp & 0xff000000) >> 24);
                        //uint nIp2 = ((gigeInfo.nCurrentIp & 0x00ff0000) >> 16);
                        //uint nIp3 = ((gigeInfo.nCurrentIp & 0x0000ff00) >> 8);
                        //uint nIp4 = (gigeInfo.nCurrentIp & 0x000000ff);
                        //string camIp = nIp1.ToString("D") + "." + nIp2.ToString("D") + "." + nIp3.ToString("D") + "." + nIp4.ToString("D");
                        string[] ipInput = IpAddress.Split('.');
                        if (ipInput.Count() != 4)
                        {
                            Debug.WriteLine("连接相机IP输入错误！");
                            continue;
                        }

                        if (nIp[0] != uint.Parse(ipInput[0]) || nIp[1] != uint.Parse(ipInput[1]) || nIp[2] != uint.Parse(ipInput[2]) || nIp[3] != uint.Parse(ipInput[3]))
                        {
                            continue;
                        }
                    }
                    else if (device.nTLayerType == CameraHkSdk.MV_USB_DEVICE)
                    {
                        CameraHkSdk.MV_USB3_DEVICE_INFO usbInfo = (CameraHkSdk.MV_USB3_DEVICE_INFO)CameraHkSdk.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(CameraHkSdk.MV_USB3_DEVICE_INFO));
                        if (usbInfo.chUserDefinedName != "")
                        {
                            string StrTemp = "U3V: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")";
                        }
                        else
                        {
                            string StrTemp = "U3V: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")";
                        }
                        continue;
                    }
                    else
                    { continue; }

                    //ch:打开设备 | en:Open Device
                    if (null == myCamera)
                    {
                        myCamera = new CameraHkSdk();
                        if (null == myCamera)
                        {
                            Debug.WriteLine("连接的相机未实例化！");
                            return -1;
                        }
                    }

                    nRet = myCamera.MV_CC_CreateDevice_NET(ref device);
                    if (CameraHkSdk.MV_OK != nRet)
                    {
                        Debug.WriteLine("创建相机句柄失败！");
                        return -1;
                    }

                    nRet = myCamera.MV_CC_OpenDevice_NET();
                    if (CameraHkSdk.MV_OK != nRet)
                    {
                        Debug.WriteLine("打开相机失败！");
                        return -1;
                    }
                    else
                    {
                        // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
                        if (device.nTLayerType == CameraHkSdk.MV_GIGE_DEVICE)
                        {
                            int nPacketSize = myCamera.MV_CC_GetOptimalPacketSize_NET();
                            if (nPacketSize > 0)
                            {
                                nRet = myCamera.MV_CC_SetIntValueEx_NET("GevSCPSPacketSize", nPacketSize);
                                if (nRet != CameraHkSdk.MV_OK)
                                {
                                    Debug.WriteLine("相机网络设置失败！");
                                    return -1;
                                }
                            }
                            else
                            {
                                Debug.WriteLine("相机网络有误！");
                                return -1;
                            }
                        }
                        if(IsTigger)
                        {
                            nRet = myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)CameraHkSdk.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
                            if (nRet != CameraHkSdk.MV_OK)
                            {
                                Debug.WriteLine("相机触发模式设置失败！");
                                return -1;
                            }

                            nRet = myCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)CameraHkSdk.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
                            if (nRet != CameraHkSdk.MV_OK)
                            {
                                Debug.WriteLine("相机触发来源设置失败！");
                                return -1;
                            }

                            myCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);                        
                            myCamera.MV_CC_SetEnumValue_NET("TriggerActivation", 0);

                            //设置输入信号
                            myCamera.MV_CC_SetEnumValue_NET("LineSelector", (uint)CameraHkSdk.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
                            myCamera.MV_CC_SetIntValue_NET("LineDebouncerTime", 50);


                            //设置输出信号
                            myCamera.MV_CC_SetEnumValue_NET("LineSelector", (uint)CameraHkSdk.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE1);
                            // 6 - HardTriggerActive
                            myCamera.MV_CC_SetEnumValue_NET("Line Source", 6);
                            myCamera.MV_CC_SetBoolValue_NET("StrobeEnable", true);
                            myCamera.MV_CC_SetIntValue_NET("StrobeLineDuration", 100000);
                            myCamera.MV_CC_SetIntValue_NET("StrobeLineDelay", 0);
                        }


                        GCHandle handle = GCHandle.Alloc(hImage);
                        IntPtr p = GCHandle.ToIntPtr(handle);

                        //根据图像格式不同选择不同图像格式的回调注册
                        //nRet = myCamera.MV_CC_RegisterImageCallBackForBGR_NET(callbackImage, p)
                        
                        nRet = myCamera.MV_CC_RegisterImageCallBackEx_NET(callbackImage, p);
                        if (nRet != CameraHkSdk.MV_OK)
                        {
                            Debug.WriteLine("相机回调函数置失败！");
                            return -1;
                        }
                        //nRet = m_MyCamera.RegisterImageCallBackEx(ImageCallback, IntPtr.Zero);
                        //nRet = m_MyCamera.RegisterImageCallBackForRGB(ImageCallback, IntPtr.Zero);
                        //nRet = m_MyCamera.RegisterImageCallBackForBGR(ImageCallback, IntPtr.Zero);
                        nRet = myCamera.MV_CC_StartGrabbing_NET();
                        if (CameraHkSdk.MV_OK != nRet)
                        {
                            Debug.WriteLine("相机图像采集失败！");
                            return -1;
                        }
                        IsConnect = true;
                        return 1;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("相机" + "连接出错！" + ex.Message.ToString());
                IsConnect = false; 
                return -1;
            }
        }

        //断开相机
        public override int DisConnect()
        {
            try
            {
                //停止图像采集
                int nRet;
                nRet = myCamera.MV_CC_StopGrabbing_NET();
                if (CameraHkSdk.MV_OK != nRet)
                {
                    Debug.WriteLine("相机停止图像采集出错！");
                    IsConnect = false;
                    return -1;
                }
                nRet = myCamera.MV_CC_CloseDevice_NET();
                if (CameraHkSdk.MV_OK != nRet)
                {
                    Debug.WriteLine("相机关闭出错！");
                    IsConnect = false;
                    return -1;
                }
                nRet = myCamera.MV_CC_DestroyDevice_NET();
                if (CameraHkSdk.MV_OK != nRet)
                {
                    Debug.WriteLine("相机句柄销毁断开出错！");
                    IsConnect = false;
                    return -1;
                }
                IsConnect = false;
                return 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("相机" + "断开出错！" + ex.Message.ToString());
                IsConnect = false;
                return -1;
            }
        }

        public override int GrabImage()
        {
            try
            {
                isImgGrabDone = false;

                int nRet;

                nRet = myCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                if (CameraHkSdk.MV_OK != nRet)
                {
                    Debug.WriteLine("相机软触发命令出错！");
                    return -1;
                }

                int timeCount = 0;
                while (isImgGrabDone == false)
                {
                    Thread.Sleep(10);
                    timeCount++;
                    if (timeCount > 200)
                    {
                        Debug.WriteLine("图像采集超时！");
                        isImgGrabDone = true;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("图像采集出错，" + ex.Message);
                return -1;
            }
        }

        public override bool SetExposure(long exposureTime)
        {
            try
            {
                if (null == myCamera)
                {
                    Debug.WriteLine("相机参数设置出错，相机未实例化！\r\n");
                    return false;
                }

                int nRet = myCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);

                nRet = myCamera.MV_CC_SetFloatValue_NET("ExposureTime", exposureTime);
                if (nRet != CameraHkSdk.MV_OK)
                {
                    Debug.WriteLine(String.Format("相机曝光设置出错，nRet=0x{0}\r\n", nRet.ToString("X")));
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("相机曝光设置出错，{0}\r\n" + ex.Message.ToString());
                return false;
            }
        }

        public bool GetExposure(out float exposureTime)
        {
            exposureTime = 0;
            try
            {
                if (null == myCamera)
                {
                    Debug.WriteLine("相机参数获取出错，相机未实例化！\r\n");
                    return false;
                }

                CameraHkSdk.MVCC_FLOATVALUE eT = new MVCC_FLOATVALUE();
                int nRet = myCamera.MV_CC_GetFloatValue_NET("ExposureTime", ref eT);
                if (nRet != CameraHkSdk.MV_OK)
                {
                    Debug.WriteLine(String.Format("相机曝光获取出错，nRet=0x{0}\r\n", nRet.ToString("X")));
                }
                exposureTime = (float)(eT.fCurValue);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("相机曝光获取出错，{0}\r\n" + ex.Message.ToString());
                return false;
            }
        }

        //public bool SetGain(float gainValue)
        //{
        //    try
        //    {
        //        if (null == myCamera)
        //        {
        //            Debug.WriteLine("相机参数设置出错，相机未实例化！");
        //            return false;
        //        }

        //        myCamera.MV_CC_SetEnumValue_NET("GainAuto", 0);
        //        int nRet = myCamera.MV_CC_SetFloatValue_NET("Gain", gainValue);
        //        if (nRet != CameraHkSdk.MV_OK)
        //        {
        //            Debug.WriteLine(String.Format("相机增益设置出错，nRet=0x{0}\r\n", nRet.ToString("X")));
        //            return false;
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("相机增益设置出错，{0}\r\n" + ex.Message.ToString());
        //        return false;
        //    }
        //}

        public bool GetGain(out float gainValue)
        {
            gainValue = 0;
            try
            {
                if (null == myCamera)
                {
                    Debug.WriteLine("相机参数获取出错，相机未实例化！\r\n");
                    return false;
                }

                CameraHkSdk.MVCC_FLOATVALUE eT = new MVCC_FLOATVALUE();
                int nRet = myCamera.MV_CC_GetFloatValue_NET("Gain", ref eT);
                if (nRet != CameraHkSdk.MV_OK)
                {
                    Debug.WriteLine(String.Format("相机增益获取出错，nRet=0x{0}\r\n", nRet.ToString("X")));
                }
                gainValue = (float)(eT.fCurValue);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("相机增益获取出错，{0}\r\n" + ex.Message.ToString());
                return false;
            }
        }

        public bool CameraGetParams(out int A)
        {
            A = 0;
            if (null == myCamera)
            {
                Debug.WriteLine("相机参数获取出错，相机未实例化！");
                return false;
            }
            int nRet = CameraHkSdk.MV_OK;

            // Get value of Integer nodes. Such as, 'width' etc.
            CameraHkSdk.MVCC_INTVALUE_EX stIntVal = new CameraHkSdk.MVCC_INTVALUE_EX();
            nRet = myCamera.MV_CC_GetIntValueEx_NET("Width", ref stIntVal);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Debug.WriteLine("Get width failed:{0:x8}", nRet);
                return false;
            }
            Debug.WriteLine("Current Width:{0:d}", stIntVal.nCurValue);

            // Get value of Enum nodes. Such as, 'TriggerMode' etc.
            CameraHkSdk.MVCC_ENUMVALUE stEnumVal = new CameraHkSdk.MVCC_ENUMVALUE();
            nRet = myCamera.MV_CC_GetEnumValue_NET("TriggerMode", ref stEnumVal);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Debug.WriteLine("Get Trigger Mode failed:{0:x8}", nRet);
                return false;
            }
            Console.WriteLine("Current TriggerMode:{0:d}", stEnumVal.nCurValue);

            // Get value of float nodes. Such as, 'AcquisitionFrameRate' etc.
            CameraHkSdk.MVCC_FLOATVALUE stFloatVal = new CameraHkSdk.MVCC_FLOATVALUE();
            nRet = myCamera.MV_CC_GetFloatValue_NET("AcquisitionFrameRate", ref stFloatVal);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Get AcquisitionFrameRate failed:{0:x8}", nRet);
                return false;
            }
            Console.WriteLine("Current AcquisitionFrameRate:{0:f}Fps", stFloatVal.fCurValue);

            // Get value of bool nodes. Such as, 'AcquisitionFrameRateEnable' etc.
            bool bBoolVal = false;
            nRet = myCamera.MV_CC_GetBoolValue_NET("AcquisitionFrameRateEnable", ref bBoolVal);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Get AcquisitionFrameRateEnable failed:{0:x8}", nRet);
                return false;
            }
            Console.WriteLine("Current AcquisitionFrameRateEnable:{0:d}", bBoolVal);

            // Get value of String nodes. Such as, 'DeviceUserID' etc.
            CameraHkSdk.MVCC_STRINGVALUE stStrVal = new CameraHkSdk.MVCC_STRINGVALUE();
            nRet = myCamera.MV_CC_GetStringValue_NET("DeviceUserID", ref stStrVal);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Get DeviceUserID failed:{0:x8}", nRet);
                return false;
            }
            Console.WriteLine("Current DeviceUserID:{0:s}", stStrVal.chCurValue);

            return true;
        }

        public bool CameraSetParams()
        {
            if (null == myCamera)
            {
                Debug.WriteLine("相机参数设置出错，相机未实例化！");
                return false;
            }

            int nRet = CameraHkSdk.MV_OK;

            // Set value of Integer nodes. Such as, 'width' etc.
            nRet = myCamera.MV_CC_SetIntValueEx_NET("Width", 200);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Set Width failed:{0:x8}", nRet);
                return false;
            }

            // Set value of float nodes. Such as, 'AcquisitionFrameRate' etc.
            nRet = myCamera.MV_CC_SetFloatValue_NET("AcquisitionFrameRate", 8.8f);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Set AcquisitionFrameRate failed:{0:x8}", nRet);
                return false;
            }

            // Set value of bool nodes. Such as, 'AcquisitionFrameRateEnable' etc.
            nRet = myCamera.MV_CC_SetBoolValue_NET("AcquisitionFrameRateEnable", true);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Set AcquisitionFrameRateEnable failed:{0:x8}", nRet);
                return false;
            }

            // Set value of String nodes. Such as, 'DeviceUserID' etc.
            nRet = myCamera.MV_CC_SetStringValue_NET("DeviceUserID", "UserIDChanged");
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Set DeviceUserID failed:{0:x8}", nRet);
                return false;
            }

            // Execute Command nodes. Such as, 'TriggerSoftware' etc.
            // precondition
            // Set value of Enum nodes. Such as, 'TriggerMode' etc.
            nRet = myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)CameraHkSdk.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Set TriggerMode failed:{0:x8}", nRet);
                return false;
            }
            nRet = myCamera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)CameraHkSdk.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Set TriggerSource failed:{0:x8}", nRet);
                return false;
            }
            // execute command
            nRet = myCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            if (CameraHkSdk.MV_OK != nRet)
            {
                Console.WriteLine("Execute TriggerSoftware failed:{0:x8}", nRet);
                return false;
            }
            return true;
        }

        public void ResetCamera()
        {
            System.GC.Collect();

            int nRet = CameraHkSdk.MV_CC_EnumDevices_NET(CameraHkSdk.MV_GIGE_DEVICE | CameraHkSdk.MV_USB_DEVICE, ref myDeviceList);

            callbackImage = new CameraHkSdk.cbOutputExdelegate(ImageCallBack);
        }
        /************************************************************************
         *  @fn     IsColorData()
         *  @brief  判断是否是彩色数据
         *  @param  enGvspPixelType         [IN]           像素格式
         *  @return 成功，返回0；错误，返回-1 
         ************************************************************************/
        private static Boolean IsMono(MvGvspPixelType enPixelType)
        {
            switch (enPixelType)
            {
                case MvGvspPixelType.PixelType_Gvsp_Mono1p:
                case MvGvspPixelType.PixelType_Gvsp_Mono2p:
                case MvGvspPixelType.PixelType_Gvsp_Mono4p:
                case MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MvGvspPixelType.PixelType_Gvsp_Mono8_Signed:
                case MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                case MvGvspPixelType.PixelType_Gvsp_Mono14:
                case MvGvspPixelType.PixelType_Gvsp_Mono16:
               
                    return true;
                default:
                    return false;
            }
        }
        private static Boolean IsColor(MvGvspPixelType enGvspPixelType)
        {
            switch (enGvspPixelType)
            {
                case MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                case MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                    return true;

                default:
                    return false;
            }
        }
    }
}

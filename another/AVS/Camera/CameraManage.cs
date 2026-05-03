using Basler.Pylon; // Basler工业相机SDK
using HalconDotNet;
using Lmi3d.Zen.Io;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisionUserControls.CameraClass; // 自定义相机控件

namespace AVS
{
    public class CameraManage
    {    
        public static ICamera camera; 
        public static HImage hImageA = new HImage();

      
        public static HObject imageA;

        public static LMIFactory LMIfactory; // LMI 3D相机工厂
        public static LMISingle LMISideFace; // LMI 3D相机实例
        public static HObject imageUniform; // 3D统一图像对象

        public static bool isLMICamConnect = false;  // 3D相机连接状态
        public static string LMISideIP; // 3D相机IP地址

        //public static Mat CVImage;

        public static int camType = 0;
        public static int i = 0;

        public delegate void ImgInspectCallBackFunc2D(HObject img); // 声明委托类型
        public static ImgInspectCallBackFunc2D imgInspectCallBack2D;

        public delegate void ImgInspectCallBackFunc3D(HObject img);
        public static ImgInspectCallBackFunc3D imgInspectCallBack3D;

        //构造函数
        public static bool CameraManageInit()
        {
            try
            {
                if(Global.myParams.sideParamA.isNormalCheck || Global.myParams.sideParamA.isAiCheck)
                {
                    if(Global.myParams.sideParamA.camParam.CameraKind == 0)
                        camera = new CameraHk();    // 海康
                    else if (Global.myParams.sideParamA.camParam.CameraKind == 1)
                        camera = new BaslerCamera(); // Basler

                    camera.IpAddress = Global.myParams.sideParamA.camParam.addressIp; 
                    camera.IsConnect = false;
                    camera.TriggerSource = 1;
                    camera.IsTigger = true;
                    camera.callBackFunction = new ICamera.ImgHandleCallBackFunc(HalconImageHandle); // 注册图像回调

                    imageA = new HObject(); // 初始化图像对象
                }
                if(Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck)
                {
                    //2号相机初始化
                    LMIfactory = new LMIFactory();
                    LMISideFace = new LMISingle();
                    //lMISideIP = "192.168.1.10";
                    LMISideIP = Global.myParams.sideParamB.camParam.addressIp;
                    LMISideFace.CallBackImageDeep = new LMISingle.LMICallBackFunc(LmiHalconImageHandleB); // 注册3D图像回调
                    isLMICamConnect = false;
                    HOperatorSet.GenEmptyObj(out imageUniform);
                }
                return true;
            }
            catch (Exception ex)
            {
                Global.AddLog("相机初始化出错：\r\n" + ex.Message.ToString());
                return false;
            }
        }

        //连接相机
        public static bool ConnectCameras(string id)
        {
            try
            {
                switch (id)
                {
                    case "A":
                        if (camera == null)
                        {
                            Global.AddLog("相机A未实例化或未找到!");
                            return false;
                        }
                        else if ((Global.myParams.sideParamA.isNormalCheck || Global.myParams.sideParamA.isAiCheck) && (camera.Connect()>0))
                        {
                            Global.AddLog("相机A连接成功，IP：" + camera.IpAddress);
                            //camera.SetExposure(Global.myParams.sideParamA.camParam.exporsure);
                            //basCamA.StartGrab();
                            return true;
                        }
                        else
                        {
                            Global.AddLog("相机A连接失败，IP：" + camera.IpAddress);
                            return false;
                        }
                    case "B":
                        if (LMISideIP == null || LMISideFace == null)
                        {
                            return false;
                        }
                        else if ((Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck) && LMISideFace.Initialization(LMISideIP, LMIfactory, LMISideFace) > 0)
                        {
                            isLMICamConnect = ConnectCamera("LMI", true, "D:\\CameraJob3D\\LMI.job");//*******************************************
                            return isLMICamConnect;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        return false;
                }
            }
            catch(Exception ex)
            {
                string msg = "相机" + id + "连接出错！" + ex.Message.ToString();
                Global.AddLog(msg);
                return false;
            }
        }

        //断开相机
        public static bool DisConnectCameras(string id)
        {
            try
            {
                switch (id)
                {
                    case "A": // 断开2D相机
                        if (camera == null)
                        {
                            Global.AddLog("相机A断开失败，其未初始化或未找到!");
                            return false;
                        }
                        else if ((Global.myParams.sideParamA.isNormalCheck || Global.myParams.sideParamA.isAiCheck) && (camera.DisConnect()>0))
                        {
                            Global.AddLog("相机A断开成功，相机：" + camera.IpAddress);
                            return true;
                        }
                        else
                        {
                            Global.AddLog("相机A连接失败，相机：" + camera.IpAddress);
                            return false;
                        }
                    case "B":
                        if ((Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck) && isLMICamConnect && (LMISideFace.DisConnect()>0))
                        {
                            isLMICamConnect = false;
                            string inf = "3D相机断开成功！";
                            Global.AddLog(inf);
                            return true;
                        }
                        else
                        {
                            isLMICamConnect = false;
                            string inf = "3D相机断开失败！";
                            Global.AddLog(inf);
                            return false;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                string msg = "相机" + id + "断开出错！" + ex.Message.ToString();
                Global.AddLog(msg);
                return false;
            }
        }

        public static bool ConnectCamera(string id, bool useStartTrigger, string path)
        {
            if (LMISideIP != "")
            {
                if (isLMICamConnect == false)
                {
                    KIpAddress SideIP = KIpAddress.Parse(LMISideIP);
                    LMISideFace.Mysensor = LMIfactory.system.FindSensorByIpAddress(SideIP);
                    LMISideFace.pointCloudCalibIndex = 1;
                    //LMISideFace.cameraIndex = 1;

                    if (LMISideFace.Connect(path, id) > 0)
                    {
                        LMISideFace.IsUseTrigger = useStartTrigger;
                        isLMICamConnect = true;
                        string inf = "3D相机连接成功!";
                        Global.AddLog(inf);
                        return true;
                    }
                    else
                    {
                        string inf = "警告:3D相机连接失败!";
                        Global.AddLog(inf);
                        return false;
                    }
                }
                else
                {
                    string inf = "警告:3D相机已连接!";
                    Global.AddLog(inf);
                    return false;
                }
            }
            else
            {
                string inf = "警告:3D相机IP未设置!";
                Global.AddLog(inf);
                return false;
            }
        }

        #region 图像采集函数：根据相机ID采集图像，返回Halcon图像对象。
        public static HObject GrabImage(string id)
        {
            HObject img = null;
            switch (id)
            {
                case "A":
                    if (camera.GrabImage() > 0)
                    {
                        img = imageA;
                    }
                    else if (camera == null)
                    {
                        Global.AddLog("相机A取像失败，其未实例化！");
                    }
                    else
                    {
                        Global.AddLog("相机A取像失败:"/* + camera.ReturnString*/);
                    }
                    break;
                case "B":
                    if (LMISideFace.GrabImage() == 0)
                    {
                        img = LMISideFace.ImageObjectUniform;
                    }
                    else
                    {
                        Global.AddLog("3D图像采集失败！");
                    }
                    break;
                default:
                    break;
            }
            return img;

        }
        #endregion

        //public static void SetExposure(string id, long exposureTime)
        //{
        //    switch (id)
        //    {
        //        case "A":
        //            if (camera == null)
        //            {
        //                Global.AddLog("相机A未实例化或未找到！");
        //            }
        //            else if (camera.SetExposure(exposureTime))
        //            {
        //                Global.AddLog("相机A曝光设置成功，IP：" + camera.IpAddress);
        //            }
        //            else
        //            {
        //                Global.AddLog("相机A曝光设置失败！");
        //            }
        //            break;
        //        case "B":
        //            //
        //            break;
        //        default:
        //            break;
        //    }
        //}

        #region 将图像数据转成Halcon图像格式
        /// <summary>
        /// 图像格式转换的回调函数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="RedPtr"></param>
        /// <param name="GreenPtr"></param>
        /// <param name="BluePtr"></param>
        /// <param name="frameInfo"></param>
        public static void HalconImageHandle(HObject image)
        {
            //imageA = image.Clone();
            HObject img2d;
            HOperatorSet.GenEmptyObj(out img2d);
            img2d = image.Clone();
            image.Dispose();
            imgInspectCallBack2D(img2d);
        }
        /// <summary>
        /// 将原始图像数据转换成Halcon图像对象HImage
        /// </summary>
        /// <param name="image"></param>
        /// <param name="data"></param>
        /// <param name="RedPtr"></param>
        /// <param name="GreenPtr"></param>
        /// <param name="BluePtr"></param>
        /// <param name="frameInfo"></param>
        public static void GenImage(HImage image, IntPtr data, IntPtr RedPtr, IntPtr GreenPtr, IntPtr BluePtr, MyCamera.MV_FRAME_OUT_INFO_EX frameInfo)
        {
            image = new HImage();
            image.Dispose();
            if (frameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
            {
                IntPtr pTemp = data;
                image.GenImage1("byte", frameInfo.nWidth, frameInfo.nHeight, pTemp);
            }
            else if (frameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8)//图像格式需与相机设置的格式相同
            {
                IntPtr pRed = RedPtr, pGreen = GreenPtr, pBlue = BluePtr;
                image.GenImage3("byte", frameInfo.nWidth, frameInfo.nHeight, pRed, pGreen, pBlue);
            }
            hImageA = image.Clone();
            HObject img2d;
            HOperatorSet.GenEmptyObj(out img2d);
            img2d = image.Clone();
            hImageA.Dispose();
            imgInspectCallBack2D(img2d);

        }
        #endregion
        #region 3D图像处理函数
        public static void LmiHalconImageHandleB(HObject image)
        {
            //imageUniform = LMISideFace.ImageObjectUniform;
            //HOperatorSet.TestEqualObj(LMISideFace.emptyImageUniform, imageUniform, out LMISideFace.isEmptyImageUniform);
            //HObject imageIntensity = LMISideFace.ImageObjectIntensity;
            //HOperatorSet.TestEqualObj(LMISideFace.emptyImageIntensity, imageIntensity, out LMISideFace.isEmptyImageIntensity);

            //if (LMISideFace.isEmptyImageUniform == 0 || LMISideFace.isEmptyImageIntensity == 0)
            //{

            //}
            //else
            //{
            //    imageUniform.Dispose();
            //    imageIntensity.Dispose();
            //}
            HObject img3d;
            HOperatorSet.GenEmptyObj(out img3d);
            img3d = image.Clone();
            image.Dispose();
            imgInspectCallBack3D(img3d);
        }
        #endregion
        //使用OpenCV处理图像
        //public static void OpenCVImageHandle(IntPtr data, MyCamera.MV_FRAME_OUT_INFO_EX frameInfo)
        //{
        //    if (frameInfo.enPixelType != MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
        //        return;
        //    IntPtr pTemp = data;
        //    CVImage = new Mat(frameInfo.nHeight, frameInfo.nWidth, DepthType.Cv8U, 1, data, frameInfo.nWidth);

        //    pTemp = IntPtr.Zero;
        //}


        /// <summary>
        /// Halcon图像是否无效
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static bool IsEmptyObject(HObject image)
        {
            if (image == null || !image.IsInitialized())
                return true;
            HTuple isEmptyImage = -1;//图像是否为空,-1为空，1非空
            HOperatorSet.GenEmptyObj(out HObject emptyImage);
            //判断 图像是否为空
            try
            {
                HOperatorSet.TestEqualObj(image, emptyImage, out isEmptyImage);
            }
            catch (HOperatorException)
            {
                isEmptyImage = -1;
            }
            return isEmptyImage == 1;
        }
    }
}



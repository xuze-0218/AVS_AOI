using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HalconDotNet;
using Microsoft.SqlServer.Server;
using VisionDevelopLibrary.CameraClass;
using VisionDevelopLibrary.PrimaryClass;
using VisionDevelopLibrary.SocketCommunication;
using System.Windows.Forms;
using System.Threading;
using System.Collections;
using DevComponents.AdvTree;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using AVS.DataSave;
using OpenCvSharp;
using DevComponents.WinForms.Drawing;
using DevComponents.DotNetBar;

namespace AVS
{
    public static class ImgInspect3D
    {
        //创建接受图像队列，可以为泛型，可以指定容量,可以通过集合赋值
        //static List<HObject> rcvImgs = new List<HObject>();
        //也可以不指定类型,如果不指定类型，可以添加任何类型元素
        static Queue imgQueue = new Queue();
        public static double score;
        public static List<string[]> reader;
        public static DataSaveDelegate3D DataSaveCallBack;
        private static int[] inspectOrder;
        private static string _sideStr;
        public static string SideStr
        {
            get { return _sideStr; }
            set
            {
                _sideStr = value;
            }
        }
        //该实列类对应参数
        private static ParamsSide _sideParam;
        public static ParamsSide SideParam
        {
            get { return _sideParam; }
            set
            {
                _sideParam = value;
            }
        }
        //该实例类的传入图像变量
        private static HObject _imgSource;
        public static HObject ImageSource
        {
            get { return _imgSource; }
            set
            {
                HOperatorSet.GenEmptyObj(out _imgSource);
                _imgSource = value;
            }
        }

        //图像窗口01,用于接收图像显示
        private static HWindowControl _hWindow01;
        public static HWindowControl HWindow01
        {
            get { return _hWindow01; }
            set { _hWindow01 = value; }
        }
        //图像窗口02,用于处理图像显示
        private static HWindowControl _hWindow02;
        public static HWindowControl HWindow02
        {
            get { return _hWindow02; }
            set { _hWindow02 = value; }
        }

        private static HDevProcedureCall hCall01;
        private static HDevProcedureCall hCall02;
        private static HDevProcedureCall hCall03;
        private static HDevProcedureCall hCall04;
        private static HDevProcedure hStep01;
        private static HDevProcedure hStep02;
        private static HDevProcedure hStep03;
        private static HDevProcedure hStep04;

        private static string imgWorkType = "01";                   //图像处理类型，01-检测，02-标定块标定，03-标定块验证
        private static int imgInspectNum = 100;                     //图像检测完成数量
        private static int imgReceiveNum = 100;                     //图像接受完成数量
        private static int inspectStNum = 0;                        //图像检测开始序号
        private static int inspectEdNum = 0;                        //图像检测结束序号
        private static int numForInspect = 0;                       //图像需要检测个数
        private static string[] inspectResult = new string[90];     //图像检测结果保存数组
        private static int msgPoleCapacity = 25;                    //报文每次发送极柱检测个数

        private static string moduleName = "";                      //检测初始化时获取电信模组名称用于图片保存
        private static string timeLabel = "";

        private static string calibrateResult = "";                 //标准快标定结果
        private static string calibrateData = "";                   //标定块标定数据

        static ImgInspect3D()
        {
            CameraManage.imgInspectCallBack3D = new CameraManage.ImgInspectCallBackFunc3D(ImageReceive);
        }
        //参数初始化
        public static void ParamInitial()
        {
            SideStr = "B";
            if (Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck)
            {
                InspectDataInitial();
            }
        }

        private static void InspectDataInitial()
        {
            try
            {
                string paramDir = null;
                if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true) paramDir = (Global.RecipePath + "SBProductParamB.json").Replace("\\", "/");
                else paramDir = (Global.RecipePath + "CircProductParamB.json").Replace("\\", "/");
                //string paramDir = Global.RecipePath.Replace("\\", "/");

                hStep01 = new HDevProcedure();
                hStep01.LoadProcedure("LoadParam");
                hCall01 = new HDevProcedureCall(hStep01);

                hCall01.SetInputCtrlParamTuple("WindowHandle", HWindow01.HalconWindow);
                hCall01.SetInputCtrlParamTuple("ParamDir", paramDir);
                hCall01.SetInputCtrlParamTuple("ParamSide", SideStr);
                hCall01.Execute();
                hCall01.Dispose();
                hStep01.Dispose();

                hStep02 = new HDevProcedure();
                hStep02.LoadProcedure("Crop3d");
                hCall02 = new HDevProcedureCall(hStep02);

                //if (hStep03 != null)
                //{
                //    hStep03.Dispose();
                //}

                //hStep03 = new HDevProcedure();
                //hStep03.LoadProcedure("Measure3d");
                //hCall03 = new HDevProcedureCall(hStep03);

                if (hStep03 == null)
                {
                    hStep03 = new HDevProcedure();

                    if (Global.myParams.sideParamB.imgSaveParam.isPlanecheck == true)
                    {
                        // 平面拟合模式
                        if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true)
                        {
                            // 方形+条形焊缝
                            hStep03.LoadProcedure("PlaneFitSB3D");
                        }
                        else
                        {
                            // 圆形焊缝
                            hStep03.LoadProcedure("PlaneFit3D");
                        }
                    }
                    else
                    {
                        // 非平面拟合模式（3D测量模式）
                        if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true)
                        {
                            // 方形电池的3D测量
                            hStep03.LoadProcedure("MeasureSB3d");
                        }
                        else
                        {
                            // 圆柱电池的3D测量
                            hStep03.LoadProcedure("Measure3d");
                        }
                    }

                    hCall03 = new HDevProcedureCall(hStep03);
                    score = Global.myParams.sideParamB.score.scoreValue;
                }

            }
            catch (Exception ex)
            {
                string inf = SideStr + "_检测参数初始化" + ex.Message.ToString();
                MessageBox.Show(inf);
                Global.AddLog(inf);
            }
        }

        //标定块标定
        public static void BoardCalibrateInitial(PlcMsg plcMsg, out string result, out string resultData, out string returnMsg)
        {
            try
            {
                result = "01";
                resultData = "+0000000+0000000";
                returnMsg = "";
                calibrateResult = "01";
                calibrateData = "+0000000+0000000";

                timeLabel = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");//赋值时间标记，用作图像保存路径命名

                imgReceiveNum = 0;                                  //初始化接受图像个数
                imgInspectNum = 0;                                  //初始化极柱检测序号
                numForInspect = 1;                                  //
                HOperatorSet.ClearWindow(HWindow01.HalconWindow);   //窗口清除
                inspectOrder = new int[1];
                inspectOrder[0] = 0;
                string msgShow = "";
                switch (plcMsg.calibMsg.type)
                {
                    case "03":
                        imgWorkType = "02";
                        moduleName = "标定块标定";
                        msgShow = "标定块标定初始化完成！";
                        DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "green");
                        break;
                    case "04":
                        imgWorkType = "03";
                        moduleName = "标定块点检";
                        msgShow = "标定块点检初始化完成！";
                        DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "green");
                        break;
                    default:
                        break;
                }
                imgQueue.Clear();                                               //图像队列清零
                ThreadProcess.thread3d.Set();
            }
            catch (Exception ex)
            {
                result = "02";
                resultData = "+0000000+0000000";
                returnMsg = "";
                string msgShow = "2D检测数据初始化出错!\r\n" + ex.Message.ToString();
                Global.AddLog(msgShow);
                DisplayMessage(HWindow01.HalconWindow, returnMsg, 0, 0, 100, "green");
            }
        }

        public static void BoardCalibrateImgInQueue()
        {
            try
            {
                calibrateResult = "01";                                 //初始化标定结果
                calibrateData = "+0000000+0000000";                     //初始化标定结果

                int imgNum = imgQueue.Count;                            //检查图像队列中图像数量
                int timeCount = 0;
                int timeCountMax = 1000;
                while (true)
                {
                    imgNum = imgQueue.Count;                            //检查图像队列中图像数量
                    if (imgNum > 0)
                    {
                        break;
                    }
                    timeCount++;
                    Thread.Sleep(100);
                    if (timeCount > timeCountMax)
                    {
                        break;
                    }
                }

                if (imgNum <= 0)                                        //如果图像队列中没有图像，则返回
                {
                    calibrateResult = "02";                             //标定初始化结果初始化为01
                    calibrateData = "+0000000+0000000";                 //初始化标定结果
                    string logData = "相机" + SideStr + "未收到标定图像！";
                    Global.AddLog(logData);
                    HOperatorSet.ClearWindow(HWindow01.HalconWindow);
                    DisplayMessage(HWindow01.HalconWindow, logData, 0, 0, 200, "green");
                    return;
                }


                //从图像队列中获取图像
                HObject imgNow = (HObject)imgQueue.Dequeue();
                //HTuple matchModel;//标定匹配模型
                //HTuple metroModel;//标定测量模型
                HObject roiRegionA;//标定检测区域
                HObject roiRegionB;//标定检测区域
                //HOperatorSet.GenEmptyObj(out roiRegionA);
                //HOperatorSet.GenEmptyObj(out roiRegionB);
                //string modelNameStr = Global.modelPath + sideStr + "\\" + "model_" + (4).ToString() + ".shm";
                //string metroNameStr = Global.metroPath + sideStr + "\\" + "metro_" + (4).ToString() + ".mtr";
                string regionNameStrA = Global.RecipePath + "Region" + SideStr + "_" + (4).ToString() + ".hobj";
                string regionNameStrB = Global.RecipePath + "Region" + SideStr + "_" + (5).ToString() + ".hobj";
                ////HOperatorSet.ReadShapeModel(modelNameStr, out matchModel);
                ////HOperatorSet.ReadMetrologyModel(metroNameStr, out metroModel);
                HOperatorSet.ReadRegion(out roiRegionA, regionNameStrA);
                HOperatorSet.ReadRegion(out roiRegionB, regionNameStrB);
                DispRegionXld(HWindow01.HalconWindow, roiRegionA, "blue");
                DispRegionXld(HWindow01.HalconWindow, roiRegionB, "blue");

                double resoX = Global.myParams.sideParamB.calParam.fx;
                double resoY = Global.myParams.sideParamB.calParam.fy;
                double resoZ = Global.myParams.sideParamB.calParam.fz;

                string returnMsg = null;

                if (imgWorkType == "02")
                {

                    BoardCalibrate(imgNow, roiRegionA, roiRegionB, resoX, resoY, resoZ, out calibrateResult, out calibrateData, out returnMsg);

                    //HDevProcedure hStep = new HDevProcedure();
                    //hStep.LoadProcedure("BoardCalibrate");
                    //HDevProcedureCall hCall = new HDevProcedureCall(hStep);

                    //hCall.SetInputCtrlParamTuple("WindowHandle", HWindow02.HalconWindow);
                    //hCall.SetInputCtrlParamTuple("ParamSide", SideStr);
                    //hCall.SetInputCtrlParamTuple("SearchParam", matchParam);
                    //hCall.SetInputCtrlParamTuple("ParamDir", Global.RecipePath);
                    //hCall.SetInputIconicParamObject("Image", imgNow);

                    //hCall.Execute();

                    //HTuple resultArray = hCall.GetOutputCtrlParamTuple("ResultArray");

                    //hCall.Dispose();
                    //hStep.Dispose();
                }
                if (imgWorkType == "03")
                {
                    BoardCalibrate(imgNow, roiRegionA, roiRegionB, resoX, resoY, resoZ, out calibrateResult, out calibrateData, out returnMsg);
                    //HDevProcedure hStep = new HDevProcedure();
                    //hStep.LoadProcedure("BoardCheck");
                    //HDevProcedureCall hCall = new HDevProcedureCall(hStep);

                    //hCall.SetInputCtrlParamTuple("WindowHandle", HWindow02.HalconWindow);
                    //hCall.SetInputCtrlParamTuple("ParamSide", SideStr);
                    //hCall.SetInputIconicParamObject("Image", imgNow);
                    //hCall.Execute();

                    //HTuple resultArray = hCall.GetOutputCtrlParamTuple("ResultArray");

                    //hCall.Dispose();
                    //hStep.Dispose();
                }
                Global.AddLog(returnMsg);

                imgNow.Dispose();
            }
            catch (Exception ex)
            {
                calibrateResult = "02";                                                                 //初始化标定结果
                calibrateData = "+0000000+0000000";                                                     //初始化标定结果
                string msgShow = "相机" + SideStr + "标准块标定初始化出错！" + ex.Message.ToString();
                Global.AddLog(msgShow);
                HOperatorSet.ClearWindow(HWindow01.HalconWindow);
                DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 200, "green");
            }
        }

        public static void BoardCalibrateResultOut(PlcMsg plcMsg, out string result, out string resultData, out string returnMsg)
        {
            try
            {
                result = calibrateResult;
                resultData = calibrateData;
                returnMsg = "";
            }
            catch (Exception ex)
            {
                result = "02";
                resultData = "+0000000+0000000";
                returnMsg = "";
                string msgShow = "2D相机标准块标定初始化出错！" + ex.Message.ToString();
                Global.AddLog(msgShow);
                HOperatorSet.ClearWindow(HWindow01.HalconWindow);
                DisplayMessage(HWindow01.HalconWindow, returnMsg, 0, 0, 200, "green");
            }
        }

        //检测数据初始化
        public static void InspecteInitial(PlcMsg plcMsg, out string result, out string resultData, out string returnMsg)
        {
            try
            {
                result = "01";
                resultData = "";
                returnMsg = "";
                calibrateResult = "00";
                calibrateData = new string('0', 1400);

                timeLabel = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");            //赋值时间标记，用作图像保存路径命名
                int nameLength = int.Parse(plcMsg.inspectMsg.nameLength);                   //获取模组码名称长度
                if (nameLength == 0)
                    moduleName = "withnoModuleName";
                else
                    moduleName = plcMsg.inspectMsg.nameText.Substring(0, nameLength);           //赋值模组码值，用作图像保存路径命名
                inspectStNum = Convert.ToInt32(plcMsg.inspectMsg.backup02.Substring(0, 2)); //获取检测极柱开始序号
                inspectEdNum = Convert.ToInt32(plcMsg.inspectMsg.backup02.Substring(2, 2)); //获取检测极柱结束序号
                numForInspect = inspectEdNum - inspectStNum + 1;                            //获取需要检测的极柱总个数
                inspectResult = new string[90];                                             //初始化所有极柱检测结果

                if (int.Parse(plcMsg.inspectMsg.proVersion) == 1) msgPoleCapacity = 10;
                else if (int.Parse(plcMsg.inspectMsg.proVersion) == 2) msgPoleCapacity = 25;



                InspectOrder order = Global.myParams.sideParamA.inspectOrders[int.Parse(plcMsg.inspectMsg.project) - 1];
                inspectOrder = new int[90];

                //if (order.row * order.col != numForInspect)
                //{
                //    result = "02";
                //    resultData = "";
                //    returnMsg = "";
                //    for (int i = 0; i < 90; i++)
                //    {
                //        inspectResult[i] = new string('0', 50);
                //        if (i < msgPoleCapacity)
                //        {
                //            resultData += "02" + new string('0', 48);
                //        }
                //    }
                //    string msg = "请检查配方设置是否有误!\r\n";
                //    DisplayMessage(HWindow01.HalconWindow, msg, 0, 0, 100, "green");
                //}


                for (int j = 0; j < order.row; j++)
                {
                    int mdiff = (int)(Math.Abs(order.end[j] - order.start[j])) / (order.col - 1);
                    if (order.end[j] - order.start[j] < 0)
                        mdiff = -mdiff;

                    for (int i = 0; i < order.col; i++)
                    {
                        inspectOrder[i + j * order.col] = (int)order.start[j] + mdiff * i;
                    }
                }

                for (int i = 0; i < 90; i++)
                {
                    inspectResult[i] = new string('0', 50);                                 //所有极柱的检测结果初始化为0
                    if (i < msgPoleCapacity)                                                //电文一次容量为28个极柱检测结果
                    {
                        resultData += "01" + new string('0', 48);
                    }
                }

                imgReceiveNum = 0;                                                          //初始化接受图像个数
                imgInspectNum = 0;                                                          //初始化极柱检测序号
                imgWorkType = "01";                                                         //图像处理类型，01-检测
                imgQueue.Clear();                                                           //图像队列清零
                HOperatorSet.ClearWindow(HWindow01.HalconWindow);                           //窗口清除
                string msgShow = "检测数据初始化完成!";
                DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "green");
                ThreadProcess.thread3d.Set();
            }
            catch (Exception ex)
            {
                result = "02";
                resultData = "";
                returnMsg = "";
                for (int i = 0; i < 90; i++)
                {
                    inspectResult[i] = new string('0', 50);
                    if (i < msgPoleCapacity)
                    {
                        resultData += "02" + new string('0', 48);
                    }
                }
                string msgShow = "检测数据初始化出错!\r\n" + ex.Message.ToString();
                DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "green");
            }
        }

        public static void InspecteInitial(string label, int length)
        {
            try
            {
                calibrateResult = "00";
                calibrateData = new string('0', 1400);

                timeLabel = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");            //赋值时间标记，用作图像保存路径命名

                moduleName = label;           //赋值模组码值，用作图像保存路径命名
                inspectStNum = 1; //获取检测极柱开始序号
                inspectEdNum = length; //获取检测极柱结束序号
                numForInspect = inspectEdNum - inspectStNum + 1;                            //获取需要检测的极柱总个数
                inspectResult = new string[90];                                             //初始化所有极柱检测结果

                inspectOrder = new int[length];
                for (int i = 0; i < length; i++)
                {
                    inspectOrder[i] = i + 1;
                }

                imgReceiveNum = 0;                                                          //初始化接受图像个数
                imgInspectNum = 0;                                                          //初始化极柱检测序号
                imgWorkType = "01";                                                         //图像处理类型，01-检测
                imgQueue.Clear();                                                           //图像队列清零
                HOperatorSet.ClearWindow(HWindow01.HalconWindow);                           //窗口清除
                string msgShow = "检测数据初始化完成!";
                DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "green");
                ThreadProcess.thread3d.Set();
            }
            catch (Exception ex)
            {

                string msgShow = "检测数据初始化出错!\r\n" + ex.Message.ToString();
                DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "green");
            }
        }


        //图像检测处理
        private static void InspectImgInQueue()
        {
            int imgNum = imgQueue.Count;
            //-如果图像队列没有图像则返回
            if (imgNum <= 0)
            {
                Thread.Sleep(200);
                ThreadProcess.thread3d.Set();
                return;
            }
            //-如果图像队列有图像则进行检测
            if (imgNum > 0)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                HOperatorSet.ClearWindow(HWindow02.HalconWindow);
                //获取极柱号码
                int poleNum = inspectOrder[imgInspectNum];
                string result = "01";

                InspectResult3DData InspectData = new InspectResult3DData();

                HObject DumpImage = new HObject();
                HObject SaveImage = new HObject();
                HOperatorSet.GenEmptyObj(out DumpImage);
                HOperatorSet.GenEmptyObj(out SaveImage);
                HTuple resultArray = new HTuple();
                HTuple beadRect = new HTuple();
                //获取队列图像
                HObject imgNow = (HObject)imgQueue.Dequeue();
                //检测序号累加
                imgInspectNum++;
                if (imgInspectNum > numForInspect)
                {
                    ShowImage(imgNow, HWindow02);
                    string msgShow = "收到图像个数超出需要检测的总数！";
                    DisplayMessage(HWindow02.HalconWindow, msgShow, 0, 0, 60, "red");
                    return;
                }
                if (Global.myParams.sideParamB.isAiCheck == true)
                {
                    try
                    {
                        //先将深度图转换为字节图
                        //-焊缝图像裁切
                        hCall02.SetInputCtrlParamTuple("WindowHandle", HWindow02.HalconWindow);
                        hCall02.SetInputCtrlParamTuple("ParamSide", SideStr);
                        hCall02.SetInputIconicParamObject("Image", imgNow);
                        //hCall02.SetWaitForDebugConnection(true);
                        hCall02.Execute();
                        HObject imgByte = hCall02.GetOutputIconicParamObject("ImageByte");

                        //HOperatorSet.WriteImage(imgByte, "bmp", 0, "imgByte");

                        //-目标检测定位出焊缝位置 
                        if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true) 
                        { AiDrive.DetectImages(SideStr, 0, imgByte, score, out int[] beadType, out beadRect); hCall03.SetInputCtrlParamTuple("BeadType", beadType); 
                        }

                        else { AiDrive.DetectImage(SideStr, 0, imgByte, score, out int beadType, out beadRect); hCall03.SetInputCtrlParamTuple("BeadType", beadType); }
                        //AiDrive.DetectImage(SideStr, 0, imgByte, score, out int beadType, out HTuple beadRect);
                        //AiDrive.DetectImages(SideStr, 0, imgByte, score, out int[] beadType, out HTuple beadRect);
                        if (beadRect.Length < 4)
                        {
                            ShowImage(imgByte, HWindow02);
                            string msgShow = "未检测到焊缝位置！";
                            Global.AddLog("3D未检测到焊缝位置！");
                            DisplayMessage(HWindow02.HalconWindow, msgShow, 0, 0, 60, "red");
                            string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                            string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
                            string savaResult = "DetectFailed";
                            string type = "NG";
                            string saveDir = $"{Global.myParams.ImageSaveDir}\\{dateStr}\\{type}\\{moduleName}\\3D\\{savaResult}";
                            string saveDir01 = saveDir + "\\Originallmage";


                            if (!Directory.Exists(saveDir01))
                            {
                                Directory.CreateDirectory(saveDir01);
                            }
                            string savePath = saveDir01 + "\\" + timeStr + "_" + moduleName + "_Pole_" + poleNum.ToString() + "_Error.bmp";

                            // 保存图像
                            HOperatorSet.WriteImage(imgNow, "bmp", 0, savePath);
                            HOperatorSet.TupleGenConst(13, 2, out resultArray);
                            //临时更改 反馈OK                          
                            string result01 = DoubleToString(resultArray[2].D, 8);//方形余高
                            string result02 = DoubleToString(resultArray[4].D, 8);//方形下塌
                            string result03 = DoubleToString(resultArray[6].D, 8);//条形余高
                            string result04 = DoubleToString(resultArray[8].D, 8);//条形下塌
                            string result05 = DoubleToString(resultArray[10].D, 8);//
                            string result06 = DoubleToString(resultArray[12].D, 8);//
                            string measureResults = "02" + result01 + result02 + result03 + result04 + result05 + result06;
                            inspectResult[poleNum - 1] = measureResults;
                            Global.AddLog("3D焊缝未检测到！");
                            SaveWindowImg(imgNow, result, poleNum, timeLabel, moduleName);
                            SaveOriginImg(imgNow, poleNum, timeLabel, moduleName);
                            ThreadProcess.thread3d.Set();

                        }
                        else
                        {
                            ShowImage(imgByte, HWindow02);
                            //-检测出焊缝位置后，对焊缝进行测量
                            hCall03.SetInputCtrlParamTuple("WindowHandle", HWindow02.HalconWindow);
                            hCall03.SetInputCtrlParamTuple("ParamSide", SideStr);
                            hCall03.SetInputCtrlParamTuple("TargetRect", beadRect);                            
                            hCall03.SetInputIconicParamObject("Image", imgNow);
                            // hCall03.SetWaitForDebugConnection(true);
                            hCall03.Execute();
                            resultArray = hCall03.GetOutputCtrlParamTuple("ResultArray");
                            imgByte.Dispose();
                            string data01 = DoubleToString(resultArray[2].D, 8);  //方形余高
                            string data02 = DoubleToString(resultArray[4].D, 8);  //方形下塌
                            string data03 = DoubleToString(resultArray[6].D, 8);  // 条形余高
                            string data04 = DoubleToString(resultArray[8].D, 8);  // 条形下塌
                            string data05 = DoubleToString(0, 8);
                            string data06 = DoubleToString(0, 8);
                            if (resultArray[0].D != 0)
                            {
                                result = "02";
                            }
                            string measureResult = result + data01 + data02 + data03 + data04 + data05 + data06;
                            inspectResult[poleNum - 1] = measureResult;
                            stopwatch.Stop();
                            long time = stopwatch.ElapsedMilliseconds;
                            Global.AddLog("3D单个极柱算法时间：\r\n" + time.ToString());

                        }
                    }
                    catch (Exception ex)
                    {
                        result = "02";
                        resultArray = new HTuple();
                        HOperatorSet.TupleGenConst(13, 2, out resultArray);

                        string data01 = DoubleToString(resultArray[2].D, 8);//下榻
                        string data02 = DoubleToString(resultArray[4].D, 8);//余高
                        string data03 = DoubleToString(resultArray[6].D, 8);//
                        string data04 = DoubleToString(resultArray[8].D, 8);//
                        string data05 = DoubleToString(resultArray[10].D, 8);//
                        string data06 = DoubleToString(resultArray[12].D, 8);//
                        string measureResult = result + data01 + data02 + data03 + data04 + data05 + data06;
                        inspectResult[poleNum - 1] = measureResult;

                        Global.AddLog("3D智能检测方法出错：\r\n" + ex.Message.ToString());
                    }
                    string msg01 = "极柱" + poleNum.ToString("D2");
                    DisplayMessage(HWindow02.HalconWindow, msg01, 0, 0, 50, "green");

                    SaveImage = imgNow.Clone();
                    HOperatorSet.DumpWindowImage(out DumpImage, HWindow02.HalconWindow);
                    SaveImgData(resultArray, SaveImage, DumpImage, poleNum, moduleName, timeLabel);
                    SaveCsvData(poleNum, resultArray);

                }
                else
                {
                    try
                    {
                        ShowImage(imgNow, HWindow02);
                        inspectResult[poleNum - 1] = new string('0', 50);
                    }
                    catch (Exception ex)
                    {
                        result = "02";
                        resultArray = new HTuple();
                        HOperatorSet.TupleGenConst(13, 2, out resultArray);

                        string data01 = DoubleToString(resultArray[2].D, 8);//下榻
                        string data02 = DoubleToString(resultArray[4].D, 8);//余高
                        string data03 = DoubleToString(resultArray[6].D, 8);//
                        string data04 = DoubleToString(resultArray[8].D, 8);//
                        string data05 = DoubleToString(resultArray[10].D, 8);//
                        string data06 = DoubleToString(resultArray[12].D, 8);//
                        string measureResult = result + data01 + data02 + data03 + data04 + data05 + data06;
                        inspectResult[poleNum - 1] = measureResult;
                        SaveCsvData(poleNum, resultArray);
                        Global.AddLog("3D传统检测方法出错：\r\n" + ex.Message.ToString());
                    }
                }
                imgNow.Dispose();
                ThreadProcess.thread3d.Set();

            }
        }

        //PLC获取检测结果
        public static void InspecteResultOut(PlcMsg plcMsg, out string result, out string resultData, out string returnMsg)
        {
            try
            {
                result = "01";
                resultData = new string('0', 50 * msgPoleCapacity);
                returnMsg = "";
                string resultForPlc = "";
                int resultNumS = Convert.ToInt32(plcMsg.inspectMsg.backup02.Substring(0, 2)); //检测结果传送开始序号
                int resultNumE = Convert.ToInt32(plcMsg.inspectMsg.backup02.Substring(2, 2));   //检测结果传送结束序号 

                //int msgCapacity = 10;
                //if ((resultNumEnd - resultNumStart + 1) > msgCapacity)
                //{
                //    result = "02";
                //    returnMsg = "获取结果范围超出电文容量！";
                //    return;
                //}

                int timeCount = 0, timeCountMax = 100;
                for (int i = resultNumS - 1; i < resultNumE; i++)
                {
                    while (true)//
                    {
                        if (inspectResult[i].Substring(0, 2) == "00")
                        {
                            Thread.Sleep(1000);
                            timeCount++;
                            if (timeCount > timeCountMax)
                            {

                                break;
                            }
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    resultForPlc += inspectResult[i];
                }
                for (int i = 0; i < msgPoleCapacity - (resultNumE - resultNumS) - 1; i++)
                {
                    string data01 = DoubleToString(0, 8);
                    string data02 = DoubleToString(0, 8);
                    string data03 = DoubleToString(0, 8);
                    string data04 = DoubleToString(0, 8);
                    string data05 = DoubleToString(0, 8);
                    string data06 = DoubleToString(0, 8);
                    resultForPlc += "00" + data01 + data02 + data03 + data04 + data05 + data06;
                }
                resultData = resultForPlc;
                HOperatorSet.ClearWindow(HWindow01.HalconWindow);

                if (imgInspectNum == numForInspect)
                {
                    string msgShow = "检测极柱数量准确，应检：" + numForInspect.ToString() + ", 实检：" + imgInspectNum.ToString();
                    DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "green");
                }
                else
                {
                    string msgShow = "检测极柱数量有误，应检：" + numForInspect.ToString() + ", 实检：" + imgInspectNum.ToString();
                    DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "red");
                }
            }
            catch (Exception ex)
            {
                result = "02";
                resultData = "";
                for (int i = 0; i < 52; i++)
                {
                    resultData += "02" + new string('0', 48);
                }
                returnMsg = "export result error!\r\n" + ex.Message.ToString();
            }
        }

        public static void ImageReceive(HObject imgRcv)
        {
            try
            {
                string msgShow = "";
                //-接收到图像个数超出需要检测个数时报错返回
                if (imgReceiveNum >= numForInspect)
                {
                    HOperatorSet.ClearWindow(HWindow01.HalconWindow);
                    ShowImage(imgRcv, HWindow01);
                    msgShow = "收到未知图像，请检查确认！";
                    DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "red");
                    imgRcv.Dispose();
                    return;
                }
                //-获取对应极柱号
                int poleNum = inspectOrder[imgReceiveNum];
                HOperatorSet.GenEmptyObj(out HObject img);
                HOperatorSet.GenEmptyObj(out HObject imgSave);
                img = imgRcv.Clone();
                imgSave = imgRcv.Clone();
                imgRcv.Dispose();
                //-将图像添加进队列
                imgQueue.Enqueue(img);
                imgReceiveNum++;                                                            //接收图像个数累加
                HOperatorSet.ClearWindow(HWindow01.HalconWindow);                           //清除窗口
                ShowImage(imgSave, HWindow01);                                              //显示图像
                if (imgWorkType == "01")
                {
                    msgShow = "极柱" + poleNum.ToString("D2");
                }
                else
                {
                    msgShow = "标块" + imgReceiveNum.ToString("D2");
                }
                DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 200, "green");

            }
            catch
            {
                HOperatorSet.ClearWindow(HWindow01.HalconWindow);                           //清除窗口
                string msgShow = "2D图像接收出错！";
                DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 200, "green");
                Global.AddLog(msgShow);
            }
        }

        public static void ProcessImgInQueue()
        {
            if (imgWorkType == "02" || imgWorkType == "03")
            {
                BoardCalibrateImgInQueue();
            }
            else if (imgWorkType == "01")
            {
                InspectImgInQueue();
            }
        }


        private static void SaveImgData(HTuple resultArray, HObject OrnImage, HObject DumpImage, int poleNum, string moduleName, string timeLabel)
        {
            InspectResult3DData InspectData = new InspectResult3DData();
            InspectData.WorkType = "Check";
            InspectData.ModuleName = moduleName;
            InspectData.PoleNum = poleNum;
            InspectData.Result3D = (Result)resultArray[0].D;
            InspectData.ResultBeadHump = (Result)resultArray[1].D;
            InspectData.ResultBeadSag = (Result)resultArray[3].D;

            InspectData.BeadHump = resultArray[2].D;
            InspectData.BeadSag = resultArray[4].D;
            try
            {
                string type = "OK";
                string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
                string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                string saveDir = null;

                List<String> savaResult = new List<string>();
                if (InspectData.Result3D == Result.NG)
                {
                    type = "NG";
                    if (InspectData.ResultBeadHump == Result.NG) savaResult.Add("BeadHump");
                    if (InspectData.ResultBeadSag == Result.NG) savaResult.Add("BeadSag");
                    if (InspectData.ResultBeadHump == Result.None && InspectData.ResultBeadSag == Result.None)
                        savaResult.Add("DetectFail");
                }
                else if (InspectData.Result3D == Result.OK)
                {
                    type = "OK";
                    savaResult.Add("OK");
                }
                else if (InspectData.Result3D == Result.None)
                {
                    type = "NG";
                    savaResult.Add("DetectFail");
                }

                for (int i = 0; i < savaResult.Count; i++)
                {
                    //创建图像保存路径*************************************************************************************                   
                    if (type == "OK")
                    {
                        saveDir = $"{Global.myParams.ImageSaveDir}\\{dateStr}\\{type}\\{moduleName}\\3D";

                    }
                    else
                    {
                        saveDir = $"{Global.myParams.ImageSaveDir}\\{dateStr}\\{type}\\{moduleName}\\3D\\{savaResult[i]}";
                    }
                    string saveDir01 = saveDir + "\\Originallmage"; ;//原始图像保存路径
                    string saveDir02 = saveDir + "\\ResultImage\\Dumplmage"; ;//结果图像保存路径
                    //string saveDir03 = saveDir + "\\ResultImage\\Masklmage"; ;//Mask图像保存路径
                    if (!Directory.Exists(saveDir01))
                    {
                        Directory.CreateDirectory(saveDir01);
                    }
                    if (!Directory.Exists(saveDir02))
                    {
                        Directory.CreateDirectory(saveDir02);
                    }
                    //if (!Directory.Exists(saveDir03))
                    //{
                    //    Directory.CreateDirectory(saveDir03);
                    //}

                    //保存原始图像****************************************************************************************
                    string imgSavePath01 = saveDir01 + "\\" + timeStr + "_" + moduleName + "_Pole_" + poleNum.ToString() + "_O";
                    imgSavePath01 = imgSavePath01.Replace("\\", "/");
                    InspectData.Orn3DPath = imgSavePath01;
                    HObject OImage = new HObject();
                    HOperatorSet.GenEmptyObj(out OImage);
                    OImage = OrnImage.Clone();
                    Task.Run(() => SaveOriginImg(OImage, imgSavePath01));

                    if (DumpImage != null || IsObjectEmpty(DumpImage))
                    {
                        string imgSavePath02 = saveDir02 + "\\" + timeStr + "_" + moduleName + "_Pole_" + poleNum.ToString() + "_W";
                        HOperatorSet.WriteImage(DumpImage, "jpg 100", 0, imgSavePath02);
                        InspectData.Dump3DPath = imgSavePath02;
                    }
                }
                DataSaveCallBack(InspectData);
                OrnImage.Dispose();
                DumpImage.Dispose();
            }
            catch (Exception ex)
            {
                string exMsg = "3D原始图像保存出错：\r\n" + ex.Message.ToString();
                Global.AddLog(exMsg);
            }
        }

        private static void SaveOriginImg(HObject img, int poleNum, string timeMark, string moduleMark)
        {
            //图像保存**********************************************************************************************
            //string type = (result == "01") ? "OK" : "NG";
            string type = "OK";
            string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
            string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            string saveDir = Global.myParams.ImageSaveDir + "\\" + SideStr + "\\Inspect\\" + type + "\\" + dateStr;
            saveDir += "\\" + timeMark + "_Module_" + moduleMark;
            //创建图像保存路径*************************************************************************************
            string saveDir01 = saveDir + "_O";//原始图像保存路径
            if (!Directory.Exists(saveDir01))
            {
                Directory.CreateDirectory(saveDir01);
            }
            //保存原始图像****************************************************************************************
            string imgSavePath01 = saveDir01 + "\\" + timeStr + "_" + moduleMark + "_Pole_" + poleNum.ToString() + "_O";
            imgSavePath01 = imgSavePath01.Replace("\\", "/");
            if (img != null || IsObjectEmpty(img))
            {
                HOperatorSet.WriteImage(img, "png", 0, imgSavePath01);
            }
            img.Dispose();
        }

        private static void SaveOriginImg(HObject img, string path)
        {
            try
            {

                if (img != null || IsObjectEmpty(img))
                {


                    HOperatorSet.WriteImage(img, "png", 0, path);

                }
                img.Dispose();
            }
            catch (Exception ex)
            {
                string exMsg = "2D原始图像保存出错：\r\n" + ex.Message.ToString();
                Global.AddLog(exMsg);
            }
        }

        private static void SaveWindowImg(HObject img, string result, int poleNum, string timeMark, string moduleMark)
        {
            //图像保存**********************************************************************************************
            string type = (result == "01") ? "OK" : "NG";
            string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
            string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            string saveDir = Global.myParams.ImageSaveDir + "\\" + SideStr + "\\Inspect\\" + type + "\\" + dateStr;
            saveDir += "\\" + timeMark + "_Module_" + moduleMark;
            //创建图像保存路径*************************************************************************************
            string saveDir02 = saveDir + "_W";//窗口截图保存路径
            if (!Directory.Exists(saveDir02))
            {
                Directory.CreateDirectory(saveDir02);
            }
            //保存窗口截图****************************************************************************************
            string imgSavePath02 = saveDir02 + "\\" + timeStr + "_" + moduleMark + "_Pole_" + poleNum.ToString() + "_W";
            imgSavePath02 = imgSavePath02.Replace("\\", "/");
            HObject image = new HObject();
            HOperatorSet.GenEmptyObj(out image);
            HOperatorSet.DumpWindowImage(out image, HWindow02.HalconWindow);
            HOperatorSet.WriteImage(image, "jpg", 0, imgSavePath02);
            image.Dispose();
        }


        //-保存图像检测数据
        private static void SaveCsvData(int poleNum, HTuple resultArray)
        {
            try
            {
                //string LocalSaveImage = "C:\\本地的CSV文件\\";
                //测量数据保存**********************************************************************************************
                //string type = (resultArray[0] == "01") ? "OK" : "NG";
                //string type = "OK";
                string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
                //string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");

                //string saveDir = $"{LocalSaveImage}\\{moduleName}\\焊后C0312\\{type}\\2D检测\\{dateStr}";
                string saveDir = $"{Global.myParams.ImageSaveDir}";
                //string saveDir = Global.myParams.ImageSaveDir + "\\" + SideStr + "\\Inspect\\" + type + "\\" + dateStr;
                //saveDir += "\\" + timeLabel + "_Module_" + moduleName;
                //测量数据保存路径创建*************************************************************************************
                string saveDir01 = saveDir+ "\\3D检测\\"+dateStr;//保存路径




                //测量数据保存**********************************************************************************************
                //string type = (resultArray[0] == "01") ? "OK" : "NG";
                //string type = "OK";
                //string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
                //string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                //string saveDir = Global.myParams.ImageSaveDir + "\\" + SideStr + "\\Inspect\\" + type + "\\" + dateStr;
                //saveDir += "\\" + timeLabel + "_Module_" + moduleName;
                ////测量数据保存路径创建*************************************************************************************
                //string saveDir01 = saveDir + "_Data";//原始图像保存路径
                if (!Directory.Exists(saveDir01))
                {
                    Directory.CreateDirectory(saveDir01);
                }
                //测量数据保存****************************************************************************************
                //****************************************************************************************************
                //string data01 = DoubleToString(resultArray[1].D, 8);//焊缝长度
                //string data02 = DoubleToString(resultArray[2].D, 8);//焊缝宽度
                //string data03 = DoubleToString(resultArray[3].D, 8);//焊缝偏移
                //string data04 = DoubleToString(resultArray[4].D, 8);//爆孔尺寸
                //string data05 = DoubleToString(resultArray[5].D, 8);//
                //string data06 = DoubleToString(resultArray[6].D, 8);//焊缝外径
                string csvPath = saveDir01 + "\\" + "_DataRecord.csv";
                csvPath = csvPath.Replace("\\", "/");
                //当文件不存在时添加标题栏
                if (!File.Exists(csvPath))
                {
                    string[] rowHead = new string[15];
                    rowHead[0] = "模组码值";
                    rowHead[1] = "极柱序号";
                    rowHead[2] = "检测结果";
                    rowHead[3] = "下塌结果";
                    rowHead[4] = "下塌";
                    rowHead[5] = "余高结果";
                    rowHead[6] = "余高";
                    rowHead[7] = "";
                    rowHead[8] = "";
                    rowHead[9] = "";
                    rowHead[10] = "";
                    rowHead[11] = "";
                    rowHead[12] = "";
                    rowHead[13] = "";
                    rowHead[14] = "";
                    //创建一个List 将所有的内容都装入其中
                    List<string[]> dataHead = new List<string[]>();
                    //添加每一行的内容
                    dataHead.Add(rowHead);
                    //保存到CSV当中
                    CSV_RW.WriteCSV(csvPath, dataHead, true);
                }
                string[] rowContent = new string[15];
                rowContent[0] = moduleName;
                rowContent[1] = poleNum.ToString().PadLeft(2, '0');
                rowContent[2] = resultArray[0].D == 0 ? "OK" : "NG";
                rowContent[3] = resultArray[1].D == 0 ? "OK" : "NG";
                rowContent[4] = string.Format("{0:000.00}", resultArray[2].D);
                rowContent[5] = resultArray[3].D == 0 ? "OK" : "NG";
                rowContent[6] = string.Format("{0:000.00}", resultArray[4].D);
                rowContent[7] = resultArray[5].D == 0 ? "OK" : "NG";
                rowContent[8] = string.Format("{0:000.00}", resultArray[6].D);
                rowContent[9] = resultArray[7].D == 0 ? "OK" : "NG";
                rowContent[10] = string.Format("{0:000.00}", resultArray[8].D);
                rowContent[11] = resultArray[9].D == 0 ? "OK" : "NG";
                rowContent[12] = string.Format("{0:000.00}", resultArray[10].D);
                rowContent[13] = resultArray[11].D == 0 ? "OK" : "NG";
                rowContent[14] = string.Format("{0:000.00}", resultArray[12].D);
                //创建一个List 将所有的内容都装入其中
                List<string[]> dataRows = new List<string[]>();
                //添加每一行的内容
                dataRows.Add(rowContent);
                //保存到CSV当中
                CSV_RW.WriteCSV(csvPath, dataRows, true);
            }
            catch (Exception ex)
            {
                string exMsg = "3D测量数据保存出错：\r\n" + ex.Message.ToString();
                Global.AddLog(exMsg);
            }
        }

        //将Double数字转换为一定长度的字符串
        public static string DoubleToString(double detectValue, int len)
        {
            //len   生成字符的总长度 
            string standZero = "00000000";
            string valueStr = "";
            int valueInt = Math.Abs(Convert.ToInt32(detectValue * 1000));
            valueStr = standZero.Substring(0, 8) + valueInt.ToString();
            valueStr = valueStr.Substring(valueStr.Length - (len - 1), len - 1);
            if (detectValue >= 0)
            {
                valueStr = "+" + valueStr;
            }
            else
            {
                valueStr = "-" + valueStr;
            }
            return valueStr;
        }

        //判断HObject对象是否为空
        public static bool IsObjectEmpty(HObject image)
        {
            try
            {
                if (image == null)//判断 图像是否为空, 为空时返回---true
                {
                    return true;
                }
                HObject emptyImg = new HObject();
                HTuple isEqual = new HTuple();
                HOperatorSet.GenEmptyObj(out emptyImg);
                HOperatorSet.TestEqualObj(image, emptyImg, out isEqual);
                return (bool)isEqual;
            }
            catch
            {
                return true;
            }
        }

        private static void DisplayMessage(HTuple windowId, string message, int row, int col, int rowStep, string color)
        {
            string[] inf = message.Split('\n');
            int msgNum = inf.Length;
            for (int i = 0; i < msgNum; i++)
            {
                HOperatorSet.SetColor(windowId, color);
                HOperatorSet.SetTposition(windowId, row + rowStep * i, col);
                HOperatorSet.WriteString(windowId, inf[i]);
            }
        }

        public static void ShowImage(HObject img, HWindowControl hWindow)
        {
            if (IsObjectEmpty(img))
                return;

            HTuple row01, col01, row02, col02;
            HTuple imgW = new HTuple(), imgH = new HTuple();
            HOperatorSet.GetImageSize(img, out imgW, out imgH);

            HTuple winW = hWindow.Width;
            HTuple winH = hWindow.Height;

            HTuple ScaleW = imgW / (winW * 1.0);
            HTuple ScaleH = imgH / (winH * 1.0);

            if (ScaleW >= ScaleH)
            {
                row01 = -(1.0) * ((winH * ScaleW) - imgH) / 2;
                col01 = 0;
                row02 = row01 + winH * ScaleW;
                col02 = col01 + winW * ScaleW;
            }
            else
            {
                row01 = 0;
                col01 = -(1.0) * ((winW * ScaleH) - imgW) / 2;
                row02 = row01 + winH * ScaleH;
                col02 = col01 + winW * ScaleH;
            }

            HOperatorSet.SetPart(hWindow.HalconWindow, row01, col01, row02, col02);
            HOperatorSet.ClearWindow(hWindow.HalconWindow);
            HOperatorSet.DispObj(img, hWindow.HalconWindow);
        }

        private static void DispRegionXld(HTuple windowHandle, HObject region, HTuple color)
        {
            HObject regionXld;
            HOperatorSet.GenEmptyObj(out regionXld);
            HOperatorSet.GenContourRegionXld(region, out regionXld, "border_holes");
            HOperatorSet.SetColor(windowHandle, color);
            HOperatorSet.DispObj(regionXld, windowHandle);
            regionXld.Dispose();
        }

        private static void BoardCalibrate(HObject img, HObject roiA, HObject roiB, double resoX, double resoY, double resoZ, out string result, out string resultData, out string returnMsg)
        {
            result = "01";
            resultData = "";
            returnMsg = "";

            HTuple highDiff = null;
            HeightMeasure(img, roiA, roiB, resoX, resoY, resoZ, out highDiff);
            resultData = DoubleToString(highDiff.D, 8) + "+0000000";
        }
        private static void BoardTest()
        {

        }
        public static void HeightMeasure(HObject ho_img, HObject ho_roi01, HObject ho_roi02, HTuple hv_resoX, HTuple hv_resoY, HTuple hv_resoZ, out HTuple hv_highDiff)
        {
            // Local iconic variables 

            HObject ho_imgMean = null, ho_imgReal = null, ho_imgZ = null;
            HObject ho_imgX = null, ho_imgY = null, ho_ReducedXa = null, ho_ReducedYa = null;
            HObject ho_ReducedZa = null, ho_X = null, ho_Y = null, ho_Za = null;
            HObject ho_ReducedXb = null, ho_ReducedYb = null, ho_ReducedZb = null;
            HObject ho_Zb = null;

            // Local control variables 

            HTuple hv_imgW = new HTuple(), hv_imgH = new HTuple();
            HTuple hv_ms3Da00 = new HTuple(), hv_fit3Da = new HTuple();
            HTuple hv_poseParam = new HTuple(), hv_PoseInvert = new HTuple();
            HTuple hv_HomMat3D = new HTuple(), hv_PoseRotate = new HTuple();
            HTuple hv_ms3Da01 = new HTuple(), hv_ms3Da02 = new HTuple();
            HTuple hv_Mina = new HTuple(), hv_Maxa = new HTuple();
            HTuple hv_Rangea = new HTuple(), hv_ms3Db00 = new HTuple();
            HTuple hv_ms3Db01 = new HTuple(), hv_ms3Db02 = new HTuple();
            HTuple hv_Minb = new HTuple(), hv_Maxb = new HTuple();
            HTuple hv_Rangeb = new HTuple(), hv_Exception = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_imgMean);
            HOperatorSet.GenEmptyObj(out ho_imgReal);
            HOperatorSet.GenEmptyObj(out ho_imgZ);
            HOperatorSet.GenEmptyObj(out ho_imgX);
            HOperatorSet.GenEmptyObj(out ho_imgY);
            HOperatorSet.GenEmptyObj(out ho_ReducedXa);
            HOperatorSet.GenEmptyObj(out ho_ReducedYa);
            HOperatorSet.GenEmptyObj(out ho_ReducedZa);
            HOperatorSet.GenEmptyObj(out ho_X);
            HOperatorSet.GenEmptyObj(out ho_Y);
            HOperatorSet.GenEmptyObj(out ho_Za);
            HOperatorSet.GenEmptyObj(out ho_ReducedXb);
            HOperatorSet.GenEmptyObj(out ho_ReducedYb);
            HOperatorSet.GenEmptyObj(out ho_ReducedZb);
            HOperatorSet.GenEmptyObj(out ho_Zb);
            try
            {
                //**输入参数***

                //**输出参数***
                hv_highDiff = 0;
                //*************************************************************************************************
                try
                {
                    ho_imgMean.Dispose();
                    HOperatorSet.MeanImage(ho_img, out ho_imgMean, 11, 11);
                    ho_imgReal.Dispose();
                    HOperatorSet.ConvertImageType(ho_imgMean, out ho_imgReal, "real");
                    ho_imgZ.Dispose();
                    HOperatorSet.ScaleImage(ho_imgReal, out ho_imgZ, hv_resoZ, 0);
                    HOperatorSet.GetImageSize(ho_img, out hv_imgW, out hv_imgH);
                    ho_imgX.Dispose();
                    HOperatorSet.GenImageSurfaceFirstOrder(out ho_imgX, "real", hv_resoX, 0,
                        0, 0, 0, hv_imgW, hv_imgH);
                    ho_imgY.Dispose();
                    HOperatorSet.GenImageSurfaceFirstOrder(out ho_imgY, "real", 0, hv_resoY,
                        0, 0, 0, hv_imgW, hv_imgH);
                    //xyz_to_object_model_3d (imgX, imgY, imgZ, img3D)
                    //prepare_object_model_3d (img3D, 'segmentation', 'true', [], [])
                    //para := ['lut','color_attrib','point_size', 'disp_pose']
                    //value := ['color1','coord_z', 1,'true']
                    //visualize_object_model_3d (window, img3D, [], [], para, value, [], [], [], PoseOut)
                    //
                    ho_ReducedXa.Dispose();
                    HOperatorSet.ReduceDomain(ho_imgX, ho_roi01, out ho_ReducedXa);
                    ho_ReducedYa.Dispose();
                    HOperatorSet.ReduceDomain(ho_imgY, ho_roi01, out ho_ReducedYa);
                    ho_ReducedZa.Dispose();
                    HOperatorSet.ReduceDomain(ho_imgZ, ho_roi01, out ho_ReducedZa);
                    //
                    HOperatorSet.XyzToObjectModel3d(ho_ReducedXa, ho_ReducedYa, ho_ReducedZa,
                        out hv_ms3Da00);
                    //拟合区域一测量平面
                    HOperatorSet.FitPrimitivesObjectModel3d(hv_ms3Da00, (new HTuple("primitive_type")).TupleConcat(
                        "fitting_algorithm"), (new HTuple("plane")).TupleConcat("least_squares_tukey"),
                        out hv_fit3Da);
                    //para := ['lut','color_attrib','point_size', 'disp_pose']
                    //value := ['color1','coord_z', 1,'true']
                    //visualize_object_model_3d (window, fit3D, [], [], para, value, [], [], [], CamPose)
                    //获取这平面的单位法向量和这个平面距离原点的距离，4个参数
                    //get_object_model_3d_params (fit3D, 'primitive_parameter', ParamValueA1)
                    //get_object_model_3d_params (fit3D, 'primitive_rms', ParamValueA2)
                    HOperatorSet.GetObjectModel3dParams(hv_fit3Da, "primitive_pose", out hv_poseParam);
                    HOperatorSet.ClearObjectModel3d(hv_fit3Da);
                    //get_object_model_3d_params (fit3D, 'point_coord_z', deepZn)
                    //将拟合平面位姿矩阵
                    HOperatorSet.PoseInvert(hv_poseParam, out hv_PoseInvert);
                    HOperatorSet.PoseToHomMat3d(hv_PoseInvert, out hv_HomMat3D);
                    HOperatorSet.CreatePose(0, 0, 0, 180, 0, 0, "Rp+T", "gba", "coordinate_system",
                        out hv_PoseRotate);

                    HOperatorSet.AffineTransObjectModel3d(hv_ms3Da00, hv_HomMat3D, out hv_ms3Da01);
                    HOperatorSet.ClearObjectModel3d(hv_ms3Da00);
                    HOperatorSet.RigidTransObjectModel3d(hv_ms3Da01, hv_PoseRotate, out hv_ms3Da02);
                    HOperatorSet.ClearObjectModel3d(hv_ms3Da01);
                    ho_X.Dispose(); ho_Y.Dispose(); ho_Za.Dispose();
                    HOperatorSet.ObjectModel3dToXyz(out ho_X, out ho_Y, out ho_Za, hv_ms3Da02,
                        "from_xyz_map", new HTuple(), new HTuple());
                    HOperatorSet.ClearObjectModel3d(hv_ms3Da02);
                    HOperatorSet.MinMaxGray(ho_Za, ho_Za, 40, out hv_Mina, out hv_Maxa, out hv_Rangea);
                    ho_ReducedXb.Dispose();
                    HOperatorSet.ReduceDomain(ho_imgX, ho_roi02, out ho_ReducedXb);
                    ho_ReducedYb.Dispose();
                    HOperatorSet.ReduceDomain(ho_imgY, ho_roi02, out ho_ReducedYb);
                    ho_ReducedZb.Dispose();
                    HOperatorSet.ReduceDomain(ho_imgZ, ho_roi02, out ho_ReducedZb);
                    //
                    HOperatorSet.XyzToObjectModel3d(ho_ReducedXb, ho_ReducedYb, ho_ReducedZb,out hv_ms3Db00);
                    //para := ['lut','color_attrib','point_size', 'disp_pose']
                    //value := ['color1','coord_z', 1,'true']
                    //*     visualize_object_model_3d (window, ms3Db00, [], [], para, value, [], [], [], CamPose)
                    HOperatorSet.AffineTransObjectModel3d(hv_ms3Db00, hv_HomMat3D, out hv_ms3Db01);
                    HOperatorSet.ClearObjectModel3d(hv_ms3Db00);
                    HOperatorSet.RigidTransObjectModel3d(hv_ms3Db01, hv_PoseRotate, out hv_ms3Db02);
                    HOperatorSet.ClearObjectModel3d(hv_ms3Db01);
                    ho_X.Dispose(); ho_Y.Dispose(); ho_Zb.Dispose();
                    HOperatorSet.ObjectModel3dToXyz(out ho_X, out ho_Y, out ho_Zb, hv_ms3Db02,
                        "from_xyz_map", new HTuple(), new HTuple());
                    HOperatorSet.ClearObjectModel3d(hv_ms3Db02);
                    HOperatorSet.MinMaxGray(ho_Zb, ho_Zb, 40, out hv_Minb, out hv_Maxb, out hv_Rangeb);
                    hv_highDiff = hv_Maxa - hv_Maxb;
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    hv_highDiff = 0;
                }

                ho_imgMean.Dispose();
                ho_imgReal.Dispose();
                ho_imgZ.Dispose();
                ho_imgX.Dispose();
                ho_imgY.Dispose();
                ho_ReducedXa.Dispose();
                ho_ReducedYa.Dispose();
                ho_ReducedZa.Dispose();
                ho_X.Dispose();
                ho_Y.Dispose();
                ho_Za.Dispose();
                ho_ReducedXb.Dispose();
                ho_ReducedYb.Dispose();
                ho_ReducedZb.Dispose();
                ho_Zb.Dispose();
                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_imgMean.Dispose();
                ho_imgReal.Dispose();
                ho_imgZ.Dispose();
                ho_imgX.Dispose();
                ho_imgY.Dispose();
                ho_ReducedXa.Dispose();
                ho_ReducedYa.Dispose();
                ho_ReducedZa.Dispose();
                ho_X.Dispose();
                ho_Y.Dispose();
                ho_Za.Dispose();
                ho_ReducedXb.Dispose();
                ho_ReducedYb.Dispose();
                ho_ReducedZb.Dispose();
                ho_Zb.Dispose();
                throw HDevExpDefaultException;
            }
        }
        public static void InspectImg(HObject imgNow, HWindowControl HWindow01)
        {
            ShowImage(imgNow, HWindow01);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            HOperatorSet.ClearWindow(HWindow01.HalconWindow);
            HTuple beadRect = new HTuple();
            if (Global.myParams.sideParamB.isAiCheck == true)
            {
                try
                {
                    //先将深度图转换为字节图
                    //-焊缝图像裁切
                    hCall02.SetInputCtrlParamTuple("WindowHandle", HWindow01.HalconWindow);
                    hCall02.SetInputCtrlParamTuple("ParamSide", SideStr);
                    hCall02.SetInputIconicParamObject("Image", imgNow);
                    //hCall02.SetWaitForDebugConnection(true);
                    hCall02.Execute();
                    HObject imgByte = hCall02.GetOutputIconicParamObject("ImageByte"); // 字节图
                    //-目标检测定位出焊缝位置
                    if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true) { AiDrive.DetectImages(SideStr, 0, imgByte, score, out int[] beadType, out beadRect); hCall03.SetInputCtrlParamTuple("BeadType", beadType); }
                    else { AiDrive.DetectImage(SideStr, 0, imgByte, score, out int beadType, out beadRect); hCall03.SetInputCtrlParamTuple("BeadType", beadType); }
                    //AiDrive.DetectImage(SideStr, 0, imgByte, score, out int beadType, out HTuple beadRect);
                    //AiDrive.DetectImages(SideStr, 0, imgByte, score, out int[] beadType, out HTuple beadRect);
                    if (beadRect.Length < 4)
                    {
                        ShowImage(imgNow, HWindow01);
                        string msgShow = "未检测到焊缝位置！";
                        Global.AddLog("2D未检测到焊缝位置！");
                        DateTime now = DateTime.Now;
                        string dateFolder = now.ToString("yyyyMMdd");
                        string basePath = @"E:\3D报错图";
                        // 创建完整目录路径
                        string datePath = Path.Combine(basePath, dateFolder);

                        // 如果目录不存在则创建
                        if (!Directory.Exists(datePath))
                        {
                            Directory.CreateDirectory(datePath);
                        }
                        string timestamp = now.ToString("yyyyMMdd_HHmmss_fff");
                        string savePath = Path.Combine(datePath, $"error_{timestamp}.bmp");

                        // 保存图像
                        HOperatorSet.WriteImage(imgNow, "bmp", 0, savePath);
                    }
                    else
                    {
                        ShowImage(imgByte, HWindow01); //imgByte
                        //-检测出焊缝位置后，对焊缝进行测量
                        hCall03.SetInputCtrlParamTuple("WindowHandle", HWindow01.HalconWindow);
                        hCall03.SetInputCtrlParamTuple("ParamSide", SideStr);
                        hCall03.SetInputCtrlParamTuple("TargetRect", beadRect);
                        //hCall03.SetInputCtrlParamTuple("BeadType", beadType);
                        hCall03.SetInputIconicParamObject("Image", imgNow);
                        //hCall03.SetWaitForDebugConnection(true);
                        hCall03.Execute();
                        //resultArray = hCall03.GetOutputCtrlParamTuple("ResultArray");
                        imgByte.Dispose();
                        stopwatch.Stop();
                        long time = stopwatch.ElapsedMilliseconds;
                        Global.AddLog("3D单个极柱算法时间：\r\n" + time.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Global.AddLog("3D智能检测方法出错：\r\n" + ex.Message.ToString());
                }
            }
            else
            {
                try
                {
                    ShowImage(imgNow, HWindow01);
                }
                catch (Exception ex)
                {
                    Global.AddLog("3D传统检测方法出错：\r\n" + ex.Message.ToString());
                }
            }
            imgNow.Dispose();
        }       
    }
}

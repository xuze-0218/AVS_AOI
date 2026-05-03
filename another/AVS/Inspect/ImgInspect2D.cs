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
using OpenCvSharp.Flann;
using File = System.IO.File;
using AVS.DataSave;
using OpenCvSharp;
using System.Drawing;
using System.Data;

namespace AVS
{
    public static class ImgInspect2D
    {
        //创建接收图像队列，可以为泛型，可以指定容量,可以通过集合赋值
        //static List<HObject> rcvImgs = new List<HObject>();
        //也可以不指定类型,如果不指定类型，可以添加任何类型元素
        static Queue imgQueue = new Queue();
        //-该实例类在主程序中对应的线程序号
        private static string _sideStr;
        public static double score;
        private static int[] inspectOrder;
        public static DataSaveDelegate2D DataSaveCallBack;
        public static List<string[]> reader;
        //public static int Index = 103;
        public static string SideStr
        {
            get { return _sideStr; }
            set
            {
                _sideStr = value;
            }
        }
        //-该实列类对应参数
        private static ParamsSide _sideParam;
        public static ParamsSide SideParam
        {
            get { return _sideParam; }
            set
            {
                _sideParam = value;
            }
        }

        //-该实例类的传入图像变量
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
        //-图像窗口01,用于接收图像显示
        private static HWindowControl _hWindow01;
        public static HWindowControl HWindow01
        {
            get { return _hWindow01; }
            set { _hWindow01 = value; }
        }
        //-图像窗口02,用于处理图像显示
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

        private static string imgWorkType = "01";                   //图像处理类型，01-检测，02-标定块标定，03-标定块点检
        private static int imgInspectNum = 0;                       //图像检测完成数量
        private static int imgReceiveNum = 0;                       //图像接收完成数量
        private static int inspectStNum = 0;                        //图像检测开始序号
        private static int inspectEdNum = 0;                        //图像检测结束序号
        private static int numForInspect = 0;                       //图像需要检测个数
        private static string[] inspectResult = new string[90];     //图像检测结果保存数组
        private static int msgPoleCapacity = 25;                    //报文每次发送极柱检测个数 

        private static string moduleName = "";                      //检测初始化时获取电信模组名称用于图片保存
        private static string timeLabel = "";                       //检测初始化时获取电信模组名称用于图片保存

        private static string calibrateResult = "00";               //标准快标定结果
        private static string calibrateData = "";                   //标定块标定数据

        static ImgInspect2D() // 静态构造函数
        {
            //-相机收到图像后调用图像接收函数，图像接收函数将图像放入队列并保存至本地
            CameraManage.imgInspectCallBack2D = new CameraManage.ImgInspectCallBackFunc2D(ImageReceive);
        }

        //参数初始化
        public static void ParamInitial()
        {
            SideStr = "A";  // 2D 
            string paramDir = null;//参数初始化
            try
            {
                if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true) paramDir = (Global.RecipePath + "SBProductParamA.json").Replace("\\", "/");
                else paramDir = (Global.RecipePath + "CircProductParamA.json").Replace("\\", "/");
                //paramDir = Global.RecipePath.Replace("\\", "/");
                //-加载该处理对应的参数
                hStep01 = new HDevProcedure(); // 在内存里“加载”一个 HDevelop 过程
                hStep01.LoadProcedure("LoadParam"); // 把 .hdev 文件读进来
                hCall01 = new HDevProcedureCall(hStep01); //HDevProcedureCall -> 生成一次可执行的调用实例。
                // SetInputCtrlParamTuple(string parName, object value) 往“控制型输入参数”里塞值。Halcon 大小写敏感，WindowHandle≠windowhandle。
                hCall01.SetInputCtrlParamTuple("WindowHandle", HWindow01.HalconWindow); 
                hCall01.SetInputCtrlParamTuple("ParamDir", paramDir);
                hCall01.SetInputCtrlParamTuple("ParamSide", SideStr);
                //hCall01.SetWaitForDebugConnection(true);
                // 
                hCall01.Execute(); // 调用
                hCall01.Dispose(); // 释放
                hStep01.Dispose();

                //-实例化图像
                hStep02 = new HDevProcedure();
                hStep02.LoadProcedure("Crop2d");
                hCall02 = new HDevProcedureCall(hStep02);
                hStep03 = new HDevProcedure();

                if (Global.myParams.sideParamA.imgSaveParam.isCirWeldMark == true) hStep03.LoadProcedure("Measure2d");
                else hStep03.LoadProcedure("MeasureSB2D");
                //hStep03.LoadProcedure("Measure2d"); 
                //hStep03.LoadProcedure("MeasureSB2D");
                hCall03 = new HDevProcedureCall(hStep03);
                score = Global.myParams.sideParamB.score.scoreValue;
            }
            catch (Exception ex)
            {
                string msgShow = SideStr + "_检测参数初始化" + ex.Message.ToString();
                Global.AddLog(msgShow);
                MessageBox.Show(msgShow);
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
                imgQueue.Clear();                                   //图像队列清零
                ThreadProcess.thread2d.Set();
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

                ThreadProcess.thread2d.Reset();

                calibrateResult = "01";                             //初始化标定结果
                calibrateData = "+0000000+0000000";                 //初始化标定结果

                int imgNum = imgQueue.Count;                        //检查图像队列中图像数量
                int timeCount = 0;
                int timeCountMax = 1000;
                while (true)
                {
                    imgNum = imgQueue.Count;                        //检查图像队列中图像数量
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

                if (imgNum <= 0)                                    //如果图像队列中没有图像，则返回
                {
                    calibrateResult = "02";                         //标定初始化结果初始化为01
                    calibrateData = "+0000000+0000000";             //初始化标定结果
                    string logData = "相机" + SideStr + "未收到标定图像！";
                    Global.AddLog(logData);
                    HOperatorSet.ClearWindow(HWindow01.HalconWindow);
                    DisplayMessage(HWindow01.HalconWindow, logData, 0, 0, 200, "green");
                    return;
                }
                HObject imgNow = (HObject)imgQueue.Dequeue();
                SaveOriginImg(imgNow, Global.myParams.sideParamA.camParam.addressImg);
                ModelSearch_Param searchParam = Global.myParams.sideParamA.searchParam[3];
                HTuple angleStart = (HTuple)searchParam.angleStart;
                HTuple angleExtent = (HTuple)searchParam.angleExtent;
                HTuple scaleMin = (HTuple)searchParam.minScale;
                HTuple scaleMax = (HTuple)searchParam.maxScale;
                HTuple minScore = (HTuple)searchParam.minScore;
                HTuple numMatch = (HTuple)searchParam.maxMatchNum;
                HTuple maxOverlap = (HTuple)searchParam.maxOverlap;
                HTuple subPixel = (HTuple)searchParam.subPixel;
                HTuple numLevel = (HTuple)searchParam.numLevel;
                HTuple greediness = (HTuple)searchParam.greediness;

                HTuple matchParam = new HTuple();
                HOperatorSet.TupleConcat(matchParam, angleStart, out matchParam);
                HOperatorSet.TupleConcat(matchParam, angleExtent, out matchParam);
                HOperatorSet.TupleConcat(matchParam, scaleMin, out matchParam);
                HOperatorSet.TupleConcat(matchParam, scaleMax, out matchParam);
                HOperatorSet.TupleConcat(matchParam, minScore, out matchParam);

                HOperatorSet.TupleConcat(matchParam, numMatch, out matchParam);
                HOperatorSet.TupleConcat(matchParam, maxOverlap, out matchParam);
                HOperatorSet.TupleConcat(matchParam, subPixel, out matchParam);
                HOperatorSet.TupleConcat(matchParam, numLevel, out matchParam);
                HOperatorSet.TupleConcat(matchParam, greediness, out matchParam);

                HOperatorSet.TupleConcat(matchParam, Global.myParams.sideParamA.calParam.fx, out matchParam);
                HOperatorSet.TupleConcat(matchParam, Global.myParams.sideParamA.calParam.fy, out matchParam);


                if (imgWorkType == "02")
                {
                    HDevProcedure hStep = new HDevProcedure();
                    hStep.LoadProcedure("Cali2d");
                    HDevProcedureCall hCall = new HDevProcedureCall(hStep);

                    hCall.SetInputCtrlParamTuple("WindowHandle", HWindow02.HalconWindow);
                    hCall.SetInputCtrlParamTuple("ParamSide", SideStr);
                    hCall.SetInputCtrlParamTuple("SearchParam", matchParam);
                    hCall.SetInputCtrlParamTuple("ParamDir", Global.RecipePath);
                    hCall.SetInputIconicParamObject("Image", imgNow);

                    hCall.Execute();

                    HTuple resultArray = hCall.GetOutputCtrlParamTuple("ResultArray");

                    hCall.Dispose();
                    hStep.Dispose();

                    calibrateResult = resultArray[0].D == 1 ? "01" : "02";
                    string boardCenterX = DoubleToString(resultArray[1].D, 8);
                    string boardCenterY = DoubleToString(resultArray[2].D, 8);
                    Global.myParams.sideParamA.calParam.fx = resultArray[3].D;
                    Global.myParams.sideParamA.calParam.fy = resultArray[4].D;
                    if (Global.WriteGlobalParams())
                    {
                        Global.AddLog("参数保存成功！");
                        Thread.Sleep(300);
                        Global.ReadGlobalParams();
                    }
                    else
                    {
                        Global.AddLog("参数保存失败！");
                    }

                    calibrateData = boardCenterX + boardCenterY;
                }
                if (imgWorkType == "03")
                {
                    HDevProcedure hStep = new HDevProcedure();
                    hStep.LoadProcedure("Check2d");
                    HDevProcedureCall hCall = new HDevProcedureCall(hStep);

                    hCall.SetInputCtrlParamTuple("WindowHandle", HWindow02.HalconWindow);
                    hCall.SetInputCtrlParamTuple("ParamSide", SideStr);
                    hCall.SetInputCtrlParamTuple("SearchParam", matchParam);
                    hCall.SetInputCtrlParamTuple("ParamDir", Global.RecipePath);
                    hCall.SetInputIconicParamObject("Image", imgNow);
                    hCall.Execute();

                    HTuple resultArray = hCall.GetOutputCtrlParamTuple("ResultArray");

                    hCall.Dispose();
                    hStep.Dispose();

                    calibrateResult = resultArray[0].D == 1 ? "01" : "02";
                    string boardCenterX = DoubleToString(resultArray[1].D, 8);
                    string boardCenterY = DoubleToString(resultArray[2].D, 8);
                    calibrateData = boardCenterX + boardCenterY;
                }
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

        //检测数据初始化 解析PLC报文拿配方 输出result→ 01 成功 / 02 失败 resultData  → 给 PLC 回写的 01/02 + 48 位 ‘0’ 的固定长度报文
        public static void InspecteInitial(PlcMsg plcMsg, out string result, out string resultData, out string returnMsg)
        {
            try
            {
                result = "01";
                resultData = "";
                returnMsg = "";
                calibrateResult = "02";
                calibrateData = "+0000000+0000000";

                timeLabel = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");            //赋值时间标记，用作图像保存路径命名
                int nameLength = int.Parse(plcMsg.inspectMsg.nameLength);                   //获取模组码名称长度
                if (nameLength == 0)
                    moduleName = "withnoModuleName";
                else
                    moduleName = plcMsg.inspectMsg.nameText.Substring(0, nameLength);           //赋值模组码值，用作图像保存路径命名
                inspectStNum = Convert.ToInt32(plcMsg.inspectMsg.backup02.Substring(0, 2)); //获取检测极柱开始序号
                inspectEdNum = Convert.ToInt32(plcMsg.inspectMsg.backup02.Substring(2, 2)); //获取检测极柱结束序号
                numForInspect = inspectEdNum - inspectStNum + 1;                            //获取需要检测的极柱总个数
                inspectResult = new string[900];                                             //初始化所有极柱检测结果

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
                    if (i < msgPoleCapacity)                                                 //电文一次容量为XX个极柱检测结果
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

                ThreadProcess.thread2d.Set();
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
                calibrateResult = "02";
                calibrateData = "+0000000+0000000";

                timeLabel = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");            //赋值时间标记，用作图像保存路径命名

                moduleName = label;           //赋值模组码值，用作图像保存路径命名
                inspectStNum = 1; //获取检测极柱开始序号
                inspectEdNum = length; //获取检测极柱结束序号
                numForInspect = inspectEdNum - inspectStNum + 1;                            //获取需要检测的极柱总个数
                inspectResult = new string[900];                                             //初始化所有极柱检测结果

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

                ThreadProcess.thread2d.Set();
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
                ThreadProcess.thread2d.Set();
                return;
            }
            //-如果图像队列有图像则进行检测
            if (imgNum > 0)
            {
                Stopwatch stopwatch = new Stopwatch(); // Stopwatch计时器
                stopwatch.Start();
                HOperatorSet.ClearWindow(HWindow02.HalconWindow); //清空Halcon显示窗口
                //获取极柱号码
                int poleNum = inspectOrder[imgInspectNum];
                string result = "01";
                InspectResult2DData InspectData = new InspectResult2DData(); //定义2DData类型

                //-检测出焊缝位置后，对焊缝进行裁切，然后应用分割模型
                HOperatorSet.GenEmptyObj(out HObject mask01);
                HOperatorSet.GenEmptyObj(out HObject mask02);
                HOperatorSet.GenEmptyObj(out HObject mask03);
                HObject DumpImage = new HObject();
                HObject SaveImage = new HObject();
                HOperatorSet.GenEmptyObj(out DumpImage);
                HOperatorSet.GenEmptyObj(out SaveImage);
                HTuple resultArray = new HTuple();
                HTuple beadRect01 = new HTuple();
                HTuple beadRect02 = new HTuple();
                //获取队列图像
                HObject imgNow = (HObject)imgQueue.Dequeue();
                //检测序号累加
                imgInspectNum++;
                if (imgInspectNum > numForInspect)
                {
                    ShowImage(imgNow, HWindow02);
                    string msgShow = "收到图像个数超出需要检测的总数！";
                    DisplayMessage(HWindow02.HalconWindow, msgShow, 0, 0, 60, "red");
                    ThreadProcess.thread2d.Set();
                    return;
                }
                if (Global.myParams.sideParamA.isAiCheck == true)
                {
                    try
                    {
                        //-目标检测定位出焊缝位置 输出beadType01: int[] - 检测到的目标类别ID数组 beadRect01: HTuple - 检测框坐标数组 [x1,y1,x2,y2, x1,y1,x2,y2, ...]
                        if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true) { AiDrive.DetectImages(SideStr, 0, imgNow, score, out int[] beadType01, out beadRect01); }
                        else AiDrive.DetectImage(SideStr, 0, imgNow, score, out int beadType01, out beadRect01);
                        // 检测失败
                        if (beadRect01.Length < 4)
                        {
                            ShowImage(imgNow, HWindow02);
                            string msgShow = "未检测到焊缝位置！";
                            Global.AddLog("2D未检测到焊缝位置！");
                            DisplayMessage(HWindow02.HalconWindow, msgShow, 0, 0, 60, "red");
                            string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                            string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
                            string savaResult = "DetectFailed";
                            string type = "NG";
                            string saveDir = $"{Global.myParams.ImageSaveDir}\\{dateStr}\\{type}\\{moduleName}\\2D\\{savaResult}";
                            string saveDir01 = saveDir + "\\Originallmage";

                            if (!Directory.Exists(saveDir01))
                            {
                                Directory.CreateDirectory(saveDir01);
                            }
                            string savePath = saveDir01 + "\\" + timeStr + "_" + moduleName + "_Pole_" + poleNum.ToString() + "_Error.bmp";

                            // 保存图像
                            HOperatorSet.WriteImage(imgNow, "bmp", 0, savePath);

                            HOperatorSet.TupleGenConst(13, 2, out resultArray);
                            string result01 = DoubleToString(resultArray[2].D, 8);//焊缝长度
                            string result02 = DoubleToString(resultArray[4].D, 8);//焊缝宽度
                            string result03 = DoubleToString(resultArray[6].D, 8);//焊缝偏移
                            string result04 = DoubleToString(resultArray[8].D, 8);//爆孔面积
                            string result05 = DoubleToString(resultArray[10].D, 8);//焊缝外径
                            string result06 = DoubleToString(resultArray[12].D, 8);//虚焊尺寸
                            string measureResults = "02" + result01 + result02 + result03 + result04 + result05 + result06;
                            inspectResult[poleNum - 1] = measureResults;

                            Global.AddLog("2D焊缝未检测到！");
                        }
                        // 处理2D图像
                        else
                        {
                            //-焊缝图像裁切
                            hCall02.SetInputCtrlParamTuple("WindowHandle", HWindow02.HalconWindow);
                            hCall02.SetInputCtrlParamTuple("ParamSide", SideStr);
                            hCall02.SetInputCtrlParamTuple("TargetRect", beadRect01); // 这里拿到检测框坐标数组
                            hCall02.SetInputIconicParamObject("Image", imgNow);
                            hCall02.Execute();
                            HObject imgBead = hCall02.GetOutputIconicParamObject("ImageRoi"); // 根据检测框裁切ROI
                            //-分割模型应用
                            if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true)
                            {   // 检测方条焊缝
                                AiDrive.DetectImages(SideStr, 0, imgBead, score, out int[] beadType02, out beadRect02); // imgBead—>裁切ROI
                                hCall03.SetInputCtrlParamTuple("BeadType", beadType02); // 数组类型
                            }
                            else
                            { // 检测单个焊缝
                                AiDrive.DetectImage(SideStr, 0, imgBead, score, out int beadType02, out beadRect02);
                                hCall03.SetInputCtrlParamTuple("BeadType", beadType02);
                            }
                            AiDrive.PredictImage(SideStr, 0, imgBead, out mask01);
                            AiDrive.PredictImage(SideStr, 1, imgBead, out mask02); // 缺陷检测 识别爆孔、裂纹等缺陷
                            AiDrive.PredictImage(SideStr, 2, imgBead, out mask03);

                            //-焊缝尺度测量
                            hCall03.SetInputCtrlParamTuple("WindowHandle", HWindow02.HalconWindow);
                            hCall03.SetInputCtrlParamTuple("ParamSide", SideStr);
                            hCall03.SetInputIconicParamObject("Image", imgBead);
                            hCall03.SetInputIconicParamObject("Mask01", mask01);
                            hCall03.SetInputIconicParamObject("Mask02", mask02);
                            hCall03.SetInputIconicParamObject("Mask03", mask03);
                            //hCall03.SetWaitForDebugConnection(true);
                            hCall03.Execute();
                            resultArray = hCall03.GetOutputCtrlParamTuple("ResultArray");
                            imgBead.Dispose();
                            //方形数据格式位  焊缝长度 - 方形焊缝宽度 - 条形焊缝宽度 -焊缝间距- 爆孔数量
                            //圆形数据格式位  焊缝长度 - 焊缝宽度 - 焊缝偏移 -爆孔数量- 焊缝外径
                            string data01 = DoubleToString(resultArray[2].D, 8);//焊缝长度
                            string data02 = DoubleToString(resultArray[4].D, 8);//焊缝宽度
                            string data03 = DoubleToString(resultArray[6].D, 8);//焊缝偏移
                            string data04 = DoubleToString(resultArray[8].D, 8);//爆孔数量
                            string data05 = DoubleToString(resultArray[10].D, 8);//焊缝外径
                            string data06 = DoubleToString(resultArray[12].D, 8);//虚焊面积
                            string measureResult = result + data01 + data02 + data03 + data04 + data05 + data06;
                            inspectResult[poleNum - 1] = measureResult;
                            stopwatch.Stop();
                            long time = stopwatch.ElapsedMilliseconds;
                            Global.AddLog("2D单个极柱算法时间：\r\n" + time.ToString());


                        }
                    }
                    catch (Exception ex)
                    {
                        bool ss = true;
                        result = "02";
                        resultArray = new HTuple();
                        HOperatorSet.TupleGenConst(13, 2, out resultArray);
                        string result01 = DoubleToString(resultArray[2].D, 8);//焊缝长度
                        string result02 = DoubleToString(resultArray[4].D, 8);//焊缝宽度
                        string result03 = DoubleToString(resultArray[6].D, 8);//焊缝偏移
                        string result04 = DoubleToString(resultArray[8].D, 8);//爆孔面积
                        string result05 = DoubleToString(resultArray[10].D, 8);//焊缝外径
                        string result06 = DoubleToString(resultArray[12].D, 8);//虚焊面积
                        string measureResults = "02" + result01 + result02 + result03 + result04 + result05 + result06;
                        inspectResult[poleNum - 1] = measureResults;
                        Global.AddLog("2D智能检测方法出错：\r\n" + ex.Message.ToString());
                    }
                    string msg01 = "极柱" + poleNum.ToString("D2");
                    DisplayMessage(HWindow02.HalconWindow, msg01, 0, 0, 50, "green");

                    SaveImage = imgNow.Clone();
                    HOperatorSet.DumpWindowImage(out DumpImage, HWindow02.HalconWindow);
                    SaveImgData(resultArray, SaveImage, DumpImage, mask01, mask02, mask03, poleNum, moduleName, timeLabel);
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
                        HOperatorSet.TupleGenConst(13, 0, out resultArray);
                        resultArray[0] = 02;
                        string data01 = DoubleToString(resultArray[2].D, 8);//焊缝长度
                        string data02 = DoubleToString(resultArray[4].D, 8);//焊缝宽度
                        string data03 = DoubleToString(resultArray[6].D, 8);//焊缝偏移
                        string data04 = DoubleToString(resultArray[8].D, 8);//爆孔尺寸
                        string data05 = DoubleToString(resultArray[10].D, 8);//焊缝外径
                        string data06 = DoubleToString(resultArray[12].D, 8);//虚焊面积
                        string measureResult = result + data01 + data02 + data03 + data04 + data05 + data06;
                        inspectResult[poleNum - 1] = measureResult;
                        Global.AddLog("2D传统检测方法出错：\r\n" + ex.Message.ToString());
                    }
                }

                imgNow.Dispose();
                ThreadProcess.thread2d.Set();
            }
        }

        private static void SaveImgData(HTuple resultArray, HObject OrnImage, HObject DumpImage, HObject Mask01, HObject Mask02, HObject Mask03, int poleNum, string moduleName, string timeLabel)
        {
            InspectResult2DData InspectData = new InspectResult2DData();
            InspectData.WorkType = "Check";
            InspectData.ModuleName = moduleName;
            InspectData.DateTime = DateTime.Now;
            InspectData.PoleNum = poleNum;
            InspectData.Result2D = (Result)resultArray[0].D;
            InspectData.ResultLength = (Result)resultArray[1].D;
            InspectData.ResultWidth = (Result)resultArray[3].D;
            InspectData.ResultOffset = (Result)resultArray[5].D;
            InspectData.ResultPoreBreak = (Result)resultArray[7].D;
            InspectData.ResultBeadDiameter = (Result)resultArray[9].D;
            InspectData.ResultfaultySol = (Result)resultArray[11].D;

            InspectData.Length = resultArray[2].D;
            InspectData.Width = resultArray[4].D;
            InspectData.Offset = resultArray[6].D;
            InspectData.PoreBreakArea = resultArray[8].D;
            InspectData.BeadDiameter = resultArray[10].D;
            InspectData.faultySol = resultArray[12].D;

            try
            {
                string type = "OK";
                string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
                string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                string saveDir = null;

                List<String> savaResult = new List<string>();
                if (InspectData.Result2D == Result.NG)
                {
                    type = "NG";
                    if (InspectData.ResultLength == Result.NG) savaResult.Add("Length");
                    if (InspectData.ResultWidth == Result.NG) savaResult.Add("Width");
                    if (InspectData.ResultOffset == Result.NG) savaResult.Add("Offset");
                    if (InspectData.ResultPoreBreak == Result.NG) savaResult.Add("PoreBreak"); // 爆孔
                    if (InspectData.ResultBeadDiameter == Result.NG) savaResult.Add("BeadDiameter");
                    if (InspectData.ResultfaultySol == Result.NG) savaResult.Add("faultySol");
                }
                else if (InspectData.Result2D == Result.OK)
                {
                    savaResult.Add("OK");
                    type = "OK";
                }
                else if (InspectData.Result2D == Result.None)
                {

                }

                for (int i = 0; i < savaResult.Count; i++)
                {
                    //创建图像保存路径*************************************************************************************
                    if (type == "OK")
                    {
                        saveDir = $"{Global.myParams.ImageSaveDir}\\{dateStr}\\{type}\\{moduleName}\\2D";

                    }
                    else
                    {
                        saveDir = $"{Global.myParams.ImageSaveDir}\\{dateStr}\\{type}\\{moduleName}\\2D\\{savaResult[i]}";
                    }

                    string saveDir01 = saveDir + "\\Originallmage"; ;//原始图像保存路径
                    string saveDir02 = saveDir + "\\ResultImage\\Dumplmage"; ;//结果图像保存路径
                    string saveDir03 = saveDir + "\\ResultImage\\Masklmage"; ;//Mask图像保存路径
                    if (!Directory.Exists(saveDir01))
                    {
                        Directory.CreateDirectory(saveDir01);
                    }
                    if (!Directory.Exists(saveDir02))
                    {
                        Directory.CreateDirectory(saveDir02);
                    }
                    if (!Directory.Exists(saveDir03))
                    {
                        Directory.CreateDirectory(saveDir03);
                    }

                    //保存原始图像****************************************************************************************
                    string imgSavePath01 = saveDir01 + "\\" + timeStr + "_" + moduleName + "_Pole_" + poleNum.ToString() + "_O";
                    //imgSavePath01 = imgSavePath01.Replace("\\", "/");
                    InspectData.Orn2DPath = imgSavePath01;
                    HObject OImage = new HObject();
                    HOperatorSet.GenEmptyObj(out OImage);
                    OImage = OrnImage.Clone();
                    Task.Run(() => SaveOriginImg(OImage, imgSavePath01));

                    if (DumpImage != null || IsObjectEmpty(DumpImage))
                    {
                        string imgSavePath02 = saveDir02 + "\\" + timeStr + "_" + moduleName + "_Pole_" + poleNum.ToString() + "_W";
                        HOperatorSet.WriteImage(DumpImage, "jpg 100", 0, imgSavePath02);
                        InspectData.Dump2DPath = imgSavePath02;
                    }

                    if (Mask01 != null || IsObjectEmpty(Mask01))
                    {
                        string imgSavePath03 = saveDir03 + "\\" + timeStr + "_" + moduleName + "_Pole_" + poleNum.ToString() + "_Mask01";
                        HOperatorSet.WriteImage(Mask01, "jpg 100", 0, imgSavePath03);
                    }

                    if (Mask02 != null || IsObjectEmpty(Mask02))
                    {
                        string imgSavePath04 = saveDir03 + "\\" + timeStr + "_" + moduleName + "_Pole_" + poleNum.ToString() + "_Mask02";
                        HOperatorSet.WriteImage(Mask02, "jpg 100", 0, imgSavePath04);
                    }

                    if (Mask03 != null || IsObjectEmpty(Mask03))
                    {
                        string imgSavePath05 = saveDir03 + "\\" + timeStr + "_" + moduleName + "_Pole_" + poleNum.ToString() + "_Mask03";
                        HOperatorSet.WriteImage(Mask03, "jpg 100", 0, imgSavePath05);
                    }
                }

                DataSaveCallBack(InspectData);
                Mask01.Dispose();
                Mask02.Dispose();
                Mask03.Dispose();
                OrnImage.Dispose();
                DumpImage.Dispose();
            }
            catch (Exception ex)
            {
                string exMsg = "2D原始图像保存出错：\r\n" + ex.Message.ToString();
                Global.AddLog(exMsg);
            }
        }

        //PLC获取检测结果
        public static void InspecteResultOut(PlcMsg plcMsg, out string result, out string resultData, out string returnMsg)
        {
            try
            {
                result = "01";
                resultData = "";
                returnMsg = "";
                string resultForPlc = "";
                int resultNumS = Convert.ToInt32(plcMsg.inspectMsg.backup02.Substring(0, 2)); //检测结果传送开始序号
                int resultNumE = Convert.ToInt32(plcMsg.inspectMsg.backup02.Substring(2, 2)); //检测结果传送结束序号 

                if (resultNumE - resultNumS + 1 > msgPoleCapacity)
                {
                    for (int i = 0; i < msgPoleCapacity; i++)
                    {
                        resultData += "02" + new string('0', msgPoleCapacity);
                    }
                    HOperatorSet.ClearWindow(HWindow01.HalconWindow);
                    string msgShow = "取结果极柱号有误，开始：" + resultNumS.ToString() + ", 结束：" + resultNumE.ToString();
                    DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "green");
                    return;
                }

                int timeCount = 0, timeCountMax = 80;

                int numMax = resultNumE > numForInspect ? numForInspect : resultNumE;

                for (int i = resultNumS - 1; i < numMax; i++)
                {
                    while (true)//
                    {
                        //当最后一个极柱检测完成说明全部极柱检测完成
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
                returnMsg = "";
                resultData = "";
                for (int i = 0; i < msgPoleCapacity; i++)
                {
                    resultData += "02" + new string('0', 48);
                }
                string msgShow = "获取极柱结果出错!\r\n" + ex.Message.ToString();
                DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 100, "red");
            }
        }

        //硬触发图像接收
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


                if (Global.myParams.sideParamA.isRotated)
                    HOperatorSet.RotateImage(imgRcv, out imgRcv, 180, "constant");
                img = imgRcv.Clone();

                //-将图像添加进队列
                imgQueue.Enqueue(img);
                imgReceiveNum++;                                                            //接收图像个数累加
                HOperatorSet.ClearWindow(HWindow01.HalconWindow);                           //清除窗口
                ShowImage(imgRcv, HWindow01);
                imgRcv.Dispose();//显示图像
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


        //线程监测图像队列进行处理
        public static void ProcessImgInQueue()
        {
            if (imgWorkType == "01")
            {
                InspectImgInQueue();
            }
            else if (imgWorkType == "02" || imgWorkType == "03")
            {
                BoardCalibrateImgInQueue();
            }
        }

        //-保存相机采集原图
        private static void SaveOriginImg(HObject img, string path)
        {
            try
            {

                if (img != null || IsObjectEmpty(img))
                {
                    string format = Global.myParams.sideParamA.imgSaveParam.format == "bmp" ? "bmp" : "jpeg " + Global.myParams.sideParamA.imgSaveParam.radio;

                    HOperatorSet.WriteImage(img, format, 0, path);

                }
                img.Dispose();
            }
            catch (Exception ex)
            {
                string exMsg = "2D原始图像保存出错：\r\n" + ex.Message.ToString();
                Global.AddLog(exMsg);
            }
        }

        //-保存处理窗口截图
        private static void SaveWindowImg(HObject img, string result, int poleNum, string timeMark, string moduleMark)
        {
            try
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
                HOperatorSet.WriteImage(image, "jpg 100", 0, imgSavePath02);
                image.Dispose();
            }
            catch (Exception ex)
            {
                string exMsg = "2D窗口图像保存出错：\r\n" + ex.Message.ToString();
                Global.AddLog(exMsg);
            }
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
                string saveDir01 = saveDir + "\\2D检测\\" + dateStr;//保存路径
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
                //string csvPath = saveDir01 + "\\" + timeLabel + "_" + moduleName + "_DataRecord.csv";
                string csvPath = saveDir01 + "\\" + "_DataRecord.csv";
                csvPath = csvPath.Replace("\\", "/");
                //当文件不存在时添加标题栏
                if (!File.Exists(csvPath))
                {
                    string[] rowHead = new string[15];
                    rowHead[0] = "模组码值";
                    rowHead[1] = "极柱序号";
                    rowHead[2] = "检测结果";
                    rowHead[3] = "长度结果";
                    rowHead[4] = "焊缝长度";
                    rowHead[5] = "宽度结果";
                    rowHead[6] = "焊缝宽度";
                    rowHead[7] = "偏移结果";
                    rowHead[8] = "焊缝偏移";
                    rowHead[9] = "爆孔结果";
                    rowHead[10] = "焊缝爆孔";
                    rowHead[11] = "外径结果";
                    rowHead[12] = "焊缝外径";
                    rowHead[13] = "虚焊结果";
                    rowHead[14] = "焊缝虚焊";
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

                ////测量数据保存**********************************************************************************************
                ////string type = (resultArray[0] == "01") ? "OK" : "NG";
                //string type = "OK";
                //string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
                //string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                //string saveDir = Global.myParams.ImageSaveDir + "\\" + SideStr + "\\Inspect\\" + type + "\\" + dateStr;
                //saveDir += "\\" + timeLabel + "_Module_" + moduleName;
                ////测量数据保存路径创建*************************************************************************************
                //string saveDir01 = saveDir + "_Data";//原始图像保存路径
                //if (!Directory.Exists(saveDir01))
                //{
                //    Directory.CreateDirectory(saveDir01);
                //}
                ////测量数据保存****************************************************************************************
                ////****************************************************************************************************
                ////string data01 = DoubleToString(resultArray[1].D, 8);//焊缝长度
                ////string data02 = DoubleToString(resultArray[2].D, 8);//焊缝宽度
                ////string data03 = DoubleToString(resultArray[3].D, 8);//焊缝偏移
                ////string data04 = DoubleToString(resultArray[4].D, 8);//爆孔尺寸
                ////string data05 = DoubleToString(resultArray[5].D, 8);//
                ////string data06 = DoubleToString(resultArray[6].D, 8);//焊缝外径
                //string csvPath = saveDir01 + "\\" + timeLabel + "_" + moduleName + "_DataRecord.csv";
                //csvPath = csvPath.Replace("\\", "/");
                ////当文件不存在时添加标题栏
                //if (!File.Exists(csvPath))
                //{
                //    string[] rowHead = new string[14];
                //    rowHead[0] = "极柱序号";
                //    rowHead[1] = "检测结果";
                //    rowHead[2] = "长度结果";
                //    rowHead[3] = "焊缝长度";
                //    rowHead[4] = "宽度结果";
                //    rowHead[5] = "焊缝宽度";
                //    rowHead[6] = "偏移结果";
                //    rowHead[7] = "焊缝偏移";
                //    rowHead[8] = "爆孔结果";
                //    rowHead[9] = "焊缝爆孔";
                //    rowHead[10] = "外径结果";
                //    rowHead[11] = "焊缝外径";
                //    rowHead[12] = "虚焊结果";
                //    rowHead[13] = "焊缝虚焊";
                //    //创建一个List 将所有的内容都装入其中
                //    List<string[]> dataHead = new List<string[]>();
                //    //添加每一行的内容
                //    dataHead.Add(rowHead);
                //    //保存到CSV当中
                //    CSV_RW.WriteCSV(csvPath, dataHead, true);
                //}
                //string[] rowContent = new string[14];
                //rowContent[0] = poleNum.ToString().PadLeft(2, '0');
                //rowContent[1] = resultArray[0].D == 0 ? "OK" : "NG";
                //rowContent[2] = string.Format("{0:000.00}", resultArray[1].D);
                //rowContent[3] = string.Format("{0:000.00}", resultArray[2].D);
                //rowContent[4] = string.Format("{0:000.00}", resultArray[3].D);
                //rowContent[5] = string.Format("{0:000.00}", resultArray[4].D);
                //rowContent[6] = string.Format("{0:000.00}", resultArray[5].D);
                //rowContent[7] = string.Format("{0:000.00}", resultArray[6].D);
                //rowContent[8] = string.Format("{0:000.00}", resultArray[7].D);
                //rowContent[9] = string.Format("{0:000.00}", resultArray[8].D);
                //rowContent[10] = string.Format("{0:000.00}", resultArray[9].D);
                //rowContent[11] = string.Format("{0:000.00}", resultArray[10].D);
                //rowContent[12] = string.Format("{0:000.00}", resultArray[11].D);
                //rowContent[13] = string.Format("{0:000.00}", resultArray[12].D);
                ////创建一个List 将所有的内容都装入其中
                //List<string[]> dataRows = new List<string[]>();
                ////添加每一行的内容
                //dataRows.Add(rowContent);
                ////保存到CSV当中
                //CSV_RW.WriteCSV(csvPath, dataRows, true);
            }
            catch (Exception ex)
            {
                string exMsg = "2D测量数据保存出错：\r\n" + ex.Message.ToString();
                Global.AddLog(exMsg);
            }
        }

        //-将Double数字转换为一定长度的字符串
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

        //-判断HObject对象是否为空
        public static bool IsObjectEmpty(HObject image)
        {
            //判断 图像是否为空, 为空时返回---true
            if (image == null)
                return true;
            try
            {
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

        //-窗口消息显示
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

        //-窗口图像显示
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

        static public void DispMessageUserDefine(HTuple hv_WindowHandle, HTuple hv_MessageInfo, HTuple hv_Row, HTuple hv_Col, HTuple hv_RowHeight, HTuple hv_Color, HTuple hv_Front, HTuple hv_FrontSize)
        {
            // Local control variables 

            HTuple hv_Substrings = null, hv_Length = null;
            HTuple hv_I = null;
            // Initialize local and output iconic variables 

            HOperatorSet.SetColor(hv_WindowHandle, hv_Color);
            HOperatorSet.SetFont(hv_WindowHandle, "-" + hv_Front + hv_FrontSize + "-");

            HOperatorSet.TupleSplit(hv_MessageInfo, "\r\n", out hv_Substrings);

            HOperatorSet.TupleLength(hv_Substrings, out hv_Length);
            HTuple end_val7 = hv_Length - 1;
            HTuple step_val7 = 1;
            for (hv_I = 0; hv_I.Continue(end_val7, step_val7); hv_I = hv_I.TupleAdd(step_val7))
            {
                HOperatorSet.SetTposition(hv_WindowHandle, hv_Row + (hv_I * hv_RowHeight), hv_Col);
                HOperatorSet.WriteString(hv_WindowHandle, hv_Substrings.TupleSelect(hv_I));
            }

            //显示完设置字体默认颜色
            HOperatorSet.SetColor(hv_WindowHandle, "green");

            return;
        }
        public static void InspectImg(HObject imgNow, HWindowControl HWindow01)
        {
            ShowImage(imgNow, HWindow01);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            HOperatorSet.ClearWindow(HWindow01.HalconWindow);
            //-检测出焊缝位置后，对焊缝进行裁切，然后应用分割模型
            HOperatorSet.GenEmptyObj(out HObject mask01);
            HOperatorSet.GenEmptyObj(out HObject mask02);
            HOperatorSet.GenEmptyObj(out HObject mask03);
            HTuple resultArray = new HTuple();
            HTuple beadRect01 = new HTuple();
            HTuple beadRect02 = new HTuple();
            if (Global.myParams.sideParamA.isAiCheck == true)
            {
                try
                {
                    //-目标检测定位出焊缝位置 
                    if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true) { AiDrive.DetectImages(SideStr, 0, imgNow, score, out int[] beadType01, out beadRect01); }
                    else AiDrive.DetectImage(SideStr, 0, imgNow, score, out int beadType01, out beadRect01);
                    if (beadRect01.Length < 3)
                    {
                        ShowImage(imgNow, HWindow02);
                        string msgShow = "未检测到焊缝位置！";
                        DisplayMessage(HWindow01.HalconWindow, msgShow, 0, 0, 60, "red");
                        DateTime now = DateTime.Now;
                        string dateFolder = now.ToString("yyyyMMdd");
                        string basePath = @"E:\2D报错图";
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
                        //-焊缝图像裁切生成ROI区域 无论原始检测框多大，都裁剪为标准尺寸 只保留焊缝相关区域
                        hCall02.SetInputCtrlParamTuple("WindowHandle", HWindow01.HalconWindow);
                        hCall02.SetInputCtrlParamTuple("ParamSide", SideStr);
                        hCall02.SetInputCtrlParamTuple("TargetRect", beadRect01);
                        hCall02.SetInputIconicParamObject("Image", imgNow);
                        //hCall02.SetWaitForDebugConnection(true);
                        hCall02.Execute();
                        HObject imgBead = hCall02.GetOutputIconicParamObject("ImageRoi"); 
                        //-分割模型应用
                        if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark == true)
                        {
                            AiDrive.DetectImages(SideStr, 0, imgBead, score, out int[] beadType02, out beadRect02);
                            hCall03.SetInputCtrlParamTuple("BeadType", beadType02);
                        }
                        else
                        {
                            AiDrive.DetectImage(SideStr, 0, imgBead, score, out int beadType02, out beadRect02);
                            hCall03.SetInputCtrlParamTuple("BeadType", beadType02);
                        }
                        // 
                        AiDrive.PredictImage(SideStr, 0, imgBead, out mask01);
                        // HOperatorSet.WriteImage(mask01, "jpg 100", 0, "C:\\Users\\dyc\\Desktop\\610A\\mask01");
                        AiDrive.PredictImage(SideStr, 1, imgBead, out mask02);
                        AiDrive.PredictImage(SideStr, 2, imgBead, out mask03);
                        //-焊缝尺度测量
                        hCall03.SetInputCtrlParamTuple("WindowHandle", HWindow01.HalconWindow);
                        hCall03.SetInputCtrlParamTuple("ParamSide", SideStr);
                        hCall03.SetInputIconicParamObject("Image", imgBead);
                        hCall03.SetInputIconicParamObject("Mask01", mask01);
                        hCall03.SetInputIconicParamObject("Mask02", mask02);
                        hCall03.SetInputIconicParamObject("Mask03", mask03);
                        //hCall03.SetWaitForDebugConnection(true);
                        hCall03.Execute();
                        imgBead.Dispose();
                        stopwatch.Stop();
                        long time = stopwatch.ElapsedMilliseconds;
                        Global.AddLog("2D单个极柱算法时间：\r\n" + time.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Global.AddLog("2D智能检测方法出错：\r\n" + ex.Message.ToString());
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
                    Global.AddLog("2D传统检测方法出错：\r\n" + ex.Message.ToString());
                }
            }
            imgNow.Dispose();
        }
    }
}

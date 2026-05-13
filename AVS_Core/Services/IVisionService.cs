using AVS_Core.Models;
using AVS_Service;
using HalconDotNet;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Services
{
    public interface IVisionService
    {
        /// <summary>
        /// 初始化指定工位的视觉资源（加载HDevelop过程、AI模型）
        /// </summary>
        Task InitializeAsync(string stationId);

        /// <summary>
        /// 执行2D极柱检测
        /// </summary>
        Task<HTuple> Execute2DInspectAsync(HObject image, int poleNumber, InspectionParams param);

        /// <summary>
        /// 执行3D极柱检测
        /// </summary>
        Task<HTuple> Execute3DInspectAsync(HObject image, int poleNumber, InspectionParams param);

        /// <summary>
        /// 执行标定（返回标定结果数组）
        /// </summary>
        Task<HTuple> ExecuteCalibrationAsync(HObject image, CalibrationParams param);

        /// <summary>
        /// 执行点检（返回点检结果数组）
        /// </summary>
        Task<HTuple> ExecuteVerificationAsync(HObject image, CalibrationParams param);
    }

    public class VisionService : IVisionService
    {
        private readonly ILogger _logger;
        private readonly IParametersConfigService _parametersConfig;
        private string paramDir = string.Empty;
        private HDevEngine _engine;
        private HDevProcedure _proc2DLoadParam, _proc2DMeasure, _proc2DCrop;
        private HDevProcedure _proc3DLoadParam, _proc3DMeasure, _proc3DCrop, _procPlaneFit3D;
        private HDevProcedureCall hCall01;
        // ... 其他过程变量

        public VisionService(ILogger logger, IParametersConfigService parametersConfig)
        {
            _logger = logger;
            _parametersConfig = parametersConfig;
            //程序运行后卡住主窗体，耗时长
            Task.Run(() => InitializeEngine());

        }

        private void InitializeEngine()
        {
            _engine = new HDevEngine();
            _engine.SetProcedurePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "HalconEngine.hdpl")); // 从配置读取
            _engine.StartDebugServer();
        }

        /// <summary>
        /// 这里硬编码了工位A和B的区别，实际可以根据工位ID加载不同的配置文件来决定加载哪些过程
        /// 这里仅是halcon引擎初始化和过程加载的示例，实际还需要加载AI模型等资源
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        public async Task InitializeAsync(string stationId)
        {

            bool isSquareBarWeldMark = _parametersConfig.GetBool("ProductParam", "isSquareBarWeldMark");
            if (stationId == "A")
            {
                bool isCirWeldMark = _parametersConfig.GetBool("ProductParam", "isCirWeldMark");
                if (isSquareBarWeldMark)
                    paramDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SBProductParamA.json");
                else
                    paramDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CircProductParamA.json");
                _proc2DLoadParam = new HDevProcedure("LoadParam");
                hCall01 = new HDevProcedureCall(_proc2DLoadParam);

                //不应该在这里设置窗口,这样增加耦合,而且这里也无法获取窗口句柄。
                //如果要在窗口中显示其他内容，而不仅仅是图像，该在处理？
                hCall01.SetInputCtrlParamTuple("WindowHandle", "");
                hCall01.SetInputCtrlParamTuple("ParamDir", paramDir);
                hCall01.SetInputCtrlParamTuple("ParamSide", stationId);
                hCall01.Execute(); // 调用
                hCall01.Dispose(); // 释放
                _proc2DLoadParam.Dispose();

                _proc2DCrop = new HDevProcedure("Crop2d");
                if (isCirWeldMark)
                    _proc2DMeasure = new HDevProcedure("Measure2d");
                else
                    _proc2DMeasure = new HDevProcedure("MeasureSB2D");
            }
            else // B (3D)
            {
                bool isNormalCheck = _parametersConfig.GetBool("ProductParam", "isNormalCheck");
                bool isAiCheck = _parametersConfig.GetBool("ProductParam", "isAiCheck");
                bool isPlanecheck = _parametersConfig.GetBool("ProductParam", "isPlanecheck");
                if (isNormalCheck || isAiCheck)
                {
                    if (isSquareBarWeldMark)
                        paramDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SBProductParamB.json");
                    else
                        paramDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CircProductParamB.json");
                    _proc2DLoadParam = new HDevProcedure("LoadParam");
                    //hCall01.SetInputCtrlParamTuple("WindowHandle", HWindow01.HalconWindow);//不应该在这里设置窗口,这样增加耦合
                    hCall01.SetInputCtrlParamTuple("ParamDir", paramDir);
                    hCall01.SetInputCtrlParamTuple("ParamSide", stationId);
                    hCall01.Execute(); // 调用
                    hCall01.Dispose(); // 释放
                    _proc3DLoadParam.Dispose();

                    _proc3DCrop = new HDevProcedure("Crop3d");
                    if (isPlanecheck)    // 平面拟合模式
                    {
                        // 方形+条形焊缝
                        if (isSquareBarWeldMark)
                            _procPlaneFit3D = new HDevProcedure("PlaneFitSB3D");
                        else
                            // // 圆形焊缝
                            _procPlaneFit3D = new HDevProcedure("PlaneFit3D");
                    }
                    else  // 非平面拟合模式（3D测量模式）
                    {
                        if (isSquareBarWeldMark)   // 方形电池的3D测量
                            _proc3DMeasure = new HDevProcedure("MeasureSB3d");
                        else   // 圆柱电池的3D测量
                            _proc3DMeasure = new HDevProcedure("Measure3d");
                    }
                }
            }
            await Task.CompletedTask;
            _logger.Information("Vision engine initialized for {StationId}", stationId);
        }

        public Task<HTuple> Execute2DInspectAsync(HObject image, int poleNumber, InspectionParams param)
        {
            throw new NotImplementedException();
        }

        public Task<HTuple> Execute3DInspectAsync(HObject image, int poleNumber, InspectionParams param)
        {
            throw new NotImplementedException();
        }

        public Task<HTuple> ExecuteCalibrationAsync(HObject image, CalibrationParams param)
        {
            HDevProcedure hStep = new HDevProcedure();
            hStep.LoadProcedure("Cali2d");
            var procCall = new HDevProcedureCall(hStep);
            HTuple angleStart = _parametersConfig.GetDouble("TemplateMatch", "angleStart");
            HTuple angleExtent = _parametersConfig.GetDouble("TemplateMatch", "angleExtent");
            HTuple scaleMin = _parametersConfig.GetDouble("TemplateMatch", "minScale");
            HTuple scaleMax = _parametersConfig.GetDouble("TemplateMatch", "maxScale");
            HTuple minScore = _parametersConfig.GetDouble("TemplateMatch", "minScore");
            HTuple numMatch = _parametersConfig.GetInt("TemplateMatch", "maxMatchNum");
            HTuple maxOverlap = _parametersConfig.GetDouble("TemplateMatch", "maxOverlap");
            HTuple subPixel = _parametersConfig.GetDouble("TemplateMatch", "subPixel");
            HTuple numLevel = _parametersConfig.GetInt("TemplateMatch", "numLevel");
            HTuple greediness = _parametersConfig.GetDouble("TemplateMatch", "greediness");


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

            //HOperatorSet.TupleConcat(matchParam, Global.myParams.sideParamA.calParam.fx, out matchParam);
            //HOperatorSet.TupleConcat(matchParam, Global.myParams.sideParamA.calParam.fy, out matchParam);

            //procCall.SetInputCtrlParamTuple("SearchParam", matchParam);//从配置读取匹配参数
            //procCall.SetInputCtrlParamTuple("ParamDir", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));
            //procCall.SetInputIconicParamObject("Image", image);
            //procCall.Execute();
            HTuple result = procCall.GetOutputCtrlParamTuple("ResultArray");
            //procCall.Dispose();
            //hStep.Dispose();

            //string calibrateResult = result[0].D == 1 ? "01" : "02";
            //string boardCenterX = DoubleToString(result[1].D, 8);
            //string boardCenterY = DoubleToString(result[2].D, 8);
            //_parametersConfig.UpdateParam();
            //Global.myParams.sideParamA.calParam.fx = result[3].D;
            //Global.myParams.sideParamA.calParam.fy = result[4].D;

            //calibrateData = boardCenterX + boardCenterY;

            return Task.FromResult(result);
        }

        public Task<HTuple> ExecuteVerificationAsync(HObject image, CalibrationParams param)
        {
            var procCall = new HDevProcedureCall(_proc2DMeasure);
            procCall.SetInputIconicParamObject("Image", image);
            procCall.Execute();
            HTuple result = procCall.GetOutputCtrlParamTuple("ResultArray");
            return Task.FromResult(result);
        }


        private string DoubleToString(double detectValue, int len)
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
        private bool IsObjectEmpty(HObject image)
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
    }
}

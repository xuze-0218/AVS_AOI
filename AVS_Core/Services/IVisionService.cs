using AVS_Common.Services;
using AVS_Core.Models;
using AVS_Service;
using AVS_Service.Models;
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
        /// 负责Halcon引擎、参数文件、AI模型等配置加载，这些资源在整个应用程序生命周期中只初始化一次（每个工位）
        /// </summary>
        Task InitializeAsync(string stationId);

        /// <summary>
        /// 执行2D极柱检测
        /// </summary>
        Task<string> Execute2DInspectAsync(HObject image, int poleNumber, InspectionParams param);

        /// <summary>
        /// 执行3D极柱检测
        /// </summary>
        Task<HTuple> Execute3DInspectAsync(HObject image, int poleNumber, InspectionParams param);

        /// <summary>
        /// 执行标定（返回标定结果数组）
        /// </summary>
        Task<string> ExecuteCalibrationAsync(HObject image, CalibrationParams param);

        /// <summary>
        /// 执行点检（返回点检结果数组）
        /// </summary>
        Task<string> ExecuteVerificationAsync(HObject image, CalibrationParams param);
    }

    public class VisionService : IVisionService
    {
        private string _stationId;
        private readonly IHalconEngineProvider _engineProvider;
        private readonly IAiDriveService _aiDrive;
        private HWindow handle;

        private readonly ILogger _logger;
        private readonly IParametersConfigService _parametersConfig;
        private readonly IStationConfigService _stationConfig;
        private readonly IWindowHandleRegistry _windowHandleRegistry;
        private string paramDir = string.Empty;
        private HDevProcedure _proc2DLoadParam, _proc2DMeasure, _proc2DCrop;
        private HDevProcedure _proc3DLoadParam, _proc3DMeasure, _proc3DCrop, _procPlaneFit3D;
        private HDevProcedureCall hCall01, hCall02, hCall03;

        public VisionService(ILogger logger,
            IStationConfigService stationConfig,
            IParametersConfigService parametersConfig,
            IHalconEngineProvider engineProvider,
            IWindowHandleRegistry windowHandleRegistry,
            IAiDriveService aiDrive)
        {
            _logger = logger;
            _stationConfig = stationConfig;
            _parametersConfig = parametersConfig;
            _engineProvider = engineProvider;
            _windowHandleRegistry = windowHandleRegistry;
            _aiDrive = aiDrive;
        }

        /// <summary>
        /// 这里仅是halcon引擎初始化和过程加载，实际还需要加载AI模型等资源
        /// 这里硬编码很烂，增加了工位和视觉算法的耦合，换一个现场需要先配置好参数，不然程序执行到这里直接退出
        /// 理想情况下应该有一个更灵活的机制来根据工位配置动态加载资源，而不是在代码里写死工位ID和过程名。
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        public async Task InitializeAsync(string stationId)
        {
            _stationId = stationId;
            _engineProvider.GetEngine(); // 确保Halcon引擎已初始化
            //await InitializeEngineAsync();
            string sn = _stationConfig.GetStation(stationId).CameraRole;
            //这里获取窗口句柄绕了很大的圈，Halcon处理需要窗口句柄作为输入参数，
            //但服务层不应该直接依赖UI组件来获取这个句柄，所以通过IWindowHandleRegistry接口来获取。
            //handle = _windowHandleRegistry.GetHandle(sn);
            handle = await _windowHandleRegistry.WaitForHandleAsync(sn).ConfigureAwait(false);
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

                //不应该在这里设置窗口,这样增加耦合
                hCall01.SetInputCtrlParamTuple("WindowHandle", handle);
                hCall01.SetInputCtrlParamTuple("ParamDir", paramDir);
                hCall01.SetInputCtrlParamTuple("ParamSide", stationId);
                //这里调用报错，halcon里解析路径失败，待调试
                //hCall01.Execute(); // 调用
                //hCall01.Dispose(); // 释放 
                _proc2DLoadParam.Dispose();
                _proc2DCrop = new HDevProcedure("Crop2d");
                hCall02 = new HDevProcedureCall(_proc2DCrop);
                if (isCirWeldMark)
                    _proc2DMeasure = new HDevProcedure("Measure2d");
                else
                    _proc2DMeasure = new HDevProcedure("MeasureSB2D");
                hCall03 = new HDevProcedureCall(_proc2DMeasure);
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
                    _proc3DLoadParam = new HDevProcedure("LoadParam");
                    hCall01 = new HDevProcedureCall(_proc3DLoadParam);
                    hCall01.SetInputCtrlParamTuple("WindowHandle", handle);
                    hCall01.SetInputCtrlParamTuple("ParamDir", paramDir);
                    hCall01.SetInputCtrlParamTuple("ParamSide", _stationId);
                    //hCall01.Execute(); // 调用
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
            _logger.Information("{StationId}工位视觉初始化成功", stationId);
        }

        public async Task<string> Execute2DInspectAsync(HObject image, int poleNumber, InspectionParams param)
        {
            string result = "01";
            string measureResults = string.Empty;
            HTuple resultArray = new HTuple();
            HTuple beadRect01 = new HTuple();
            HTuple beadRect02 = new HTuple();
            HOperatorSet.GenEmptyObj(out HObject mask01);
            HOperatorSet.GenEmptyObj(out HObject mask02);
            HOperatorSet.GenEmptyObj(out HObject mask03);
            bool isAiCheck = _parametersConfig.GetBool("ProductParam", "isAiCheck");
            string sn = _stationConfig.GetStation(_stationId).CameraRole;
            handle = await _windowHandleRegistry.WaitForHandleAsync(sn).ConfigureAwait(false);
            if (isAiCheck)
            {
                bool isSquareBarWeldMark = _parametersConfig.GetBool("ProductParam", "isSquareBarWeldMark");
                if (isSquareBarWeldMark)
                    //这里score要从本地配置里读取 先写死
                    _aiDrive.DetectMulti(_stationId, 0, image, 0.8, out int[] beadType01, out beadRect01);
                else
                    _aiDrive.Detect(_stationId, 0, image, 0.8, out int beadType01, out beadRect01);
                if (beadRect01.Length < 4)
                {
                    HOperatorSet.TupleGenConst(13, 2, out resultArray);
                    string result01 = DoubleToString(resultArray[2].D, 8);//焊缝长度
                    string result02 = DoubleToString(resultArray[4].D, 8);//焊缝宽度
                    string result03 = DoubleToString(resultArray[6].D, 8);//焊缝偏移
                    string result04 = DoubleToString(resultArray[8].D, 8);//爆孔面积
                    string result05 = DoubleToString(resultArray[10].D, 8);//焊缝外径
                    string result06 = DoubleToString(resultArray[12].D, 8);//虚焊尺寸
                    measureResults = "02" + result01 + result02 + result03 + result04 + result05 + result06;
                    return measureResults;
                }
                else
                {
                    hCall02.SetInputCtrlParamTuple("WindowHandle", handle);
                    hCall02.SetInputCtrlParamTuple("ParamSide", _stationId);
                    hCall02.SetInputCtrlParamTuple("TargetRect", beadRect01); // 这里拿到检测框坐标数组
                    hCall02.SetInputIconicParamObject("Image", image);
                    hCall02.Execute();
                    HObject imgBead = hCall02.GetOutputIconicParamObject("ImageRoi"); // 根据检测框裁切ROI
                                                                                      //-分割模型应用
                    if (isSquareBarWeldMark)
                    {   // 检测方条焊缝
                        _aiDrive.DetectMulti(_stationId, 0, imgBead, 0.8, out int[] beadType02, out beadRect02); // imgBead—>裁切ROI
                        hCall03.SetInputCtrlParamTuple("BeadType", beadType02); // 数组类型
                    }
                    else
                    { // 检测单个焊缝
                        _aiDrive.Detect(_stationId, 0, imgBead, 0.8, out int beadType02, out beadRect02);
                        hCall03.SetInputCtrlParamTuple("BeadType", beadType02);
                    }
                    _aiDrive.Predict(_stationId, 0, imgBead, out mask01);
                    _aiDrive.Predict(_stationId, 1, imgBead, out mask02); // 缺陷检测 识别爆孔、裂纹等缺陷
                    _aiDrive.Predict(_stationId, 2, imgBead, out mask03);

                    //-焊缝尺度测量
                    hCall03.SetInputCtrlParamTuple("WindowHandle", handle);
                    hCall03.SetInputCtrlParamTuple("ParamSide", _stationId);
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
                    measureResults = result + data01 + data02 + data03 + data04 + data05 + data06;
                    return measureResults;
                }
            }
            else
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
                measureResults = result + data01 + data02 + data03 + data04 + data05 + data06;
                await Task.FromResult(measureResults);
            }
            return measureResults;
        }

        public Task<HTuple> Execute3DInspectAsync(HObject image, int poleNumber, InspectionParams param)
        {
            throw new NotImplementedException();
        }

        public Task<string> ExecuteCalibrationAsync(HObject image, CalibrationParams param)
        {
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
            HTuple fx = _parametersConfig.GetDouble("CalibrateParam", "fx");
            HTuple fy = _parametersConfig.GetDouble("CalibrateParam", "fy");
            HOperatorSet.TupleConcat(matchParam, fx, out matchParam);
            HOperatorSet.TupleConcat(matchParam, fy, out matchParam);

            HDevProcedure hStep = new HDevProcedure();
            hStep.LoadProcedure("Cali2d");
            var procCall = new HDevProcedureCall(hStep);
            procCall.SetInputCtrlParamTuple("WindowHandle", handle);
            procCall.SetInputCtrlParamTuple("ParamSide", _stationId);
            procCall.SetInputCtrlParamTuple("SearchParam", matchParam);
            procCall.SetInputCtrlParamTuple("ParamDir", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));
            procCall.SetInputIconicParamObject("Image", image);
            procCall.Execute();
            HTuple result = procCall.GetOutputCtrlParamTuple("ResultArray");
            procCall.Dispose();
            hStep.Dispose();

            string calibrateResult = result[0].D == 1 ? "01" : "02";
            string boardCenterX = DoubleToString(result[1].D, 8);
            string boardCenterY = DoubleToString(result[2].D, 8);
            _parametersConfig.UpdateParam("CalibrateParam", "fx", result[3].D.ToString(), ParamOutputType.FLOAT);
            _parametersConfig.UpdateParam("CalibrateParam", "fy", result[4].D.ToString(), ParamOutputType.FLOAT);
            string calibrateData = boardCenterX + boardCenterY;
            return Task.FromResult(calibrateResult + "," + calibrateData);
        }

        public Task<string> ExecuteVerificationAsync(HObject image, CalibrationParams param)
        {
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
            HTuple fx = _parametersConfig.GetDouble("CalibrateParam", "fx");
            HTuple fy = _parametersConfig.GetDouble("CalibrateParam", "fy");
            HOperatorSet.TupleConcat(matchParam, fx, out matchParam);
            HOperatorSet.TupleConcat(matchParam, fy, out matchParam);
            HDevProcedure hStep = new HDevProcedure();
            hStep.LoadProcedure("Check2d");
            var procCall = new HDevProcedureCall(hStep);

            procCall.SetInputCtrlParamTuple("WindowHandle", handle);
            procCall.SetInputCtrlParamTuple("ParamSide", _stationId);
            procCall.SetInputCtrlParamTuple("SearchParam", matchParam);
            procCall.SetInputCtrlParamTuple("ParamDir", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));
            procCall.SetInputIconicParamObject("Image", image);
            procCall.Execute();
            HTuple result = procCall.GetOutputCtrlParamTuple("ResultArray");
            procCall.Dispose();
            hStep.Dispose();

            string calibrateResult = result[0].D == 1 ? "01" : "02";
            string boardCenterX = DoubleToString(result[1].D, 8);
            string boardCenterY = DoubleToString(result[2].D, 8);
            string calibrateData = boardCenterX + boardCenterY;

            return Task.FromResult(calibrateResult + "," + calibrateData);
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

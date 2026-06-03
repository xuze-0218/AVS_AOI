using AVS_Common.Services;
using AVS_Core.Models;
using AVS_Service;
using AVS_Service.Models;
using HalconDotNet;
using OpenCvSharp.LineDescriptor;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Services
{
    public interface IVisionProvider
    {
        Task InitializeAsync(StationConfig stationConfig);
    }

    public interface I2DVisionProvider : IVisionProvider
    {
        Task<string> ExecuteInspectAsync(HObject image, int poleNum, InspectionParams param);
        Task<string> ExecuteCalibrationAsync(HObject image, CalibrationParams param);
        Task<string> ExecuteVerificationAsync(HObject image, CalibrationParams param);
    }

    public interface I3DVisionProvider : IVisionProvider
    {
        Task<string> ExecuteInspectAsync(HObject image, int poleNum, InspectionParams param);
        Task<string> ExecuteCalibrationAsync(HObject image, CalibrationParams param);
        //Task<string> ExecuteVerificationAsync(HObject image, CalibrationParams param);
    }


    public class TwoDVisionProvider : I2DVisionProvider
    {
        private string _stationId;
        private string _aiModelId;
        private HWindow _windowHandle;
        private readonly IHalconEngineProvider _engineProvider;
        private readonly IAiDriveService _aiDrive;
        private readonly ILogger _logger;
        private readonly IParametersConfigService _paramService;
        private readonly IStationConfigService _stationConfig;
        private readonly IWindowHandleRegistry _handleRegistry;
        private string paramDir = string.Empty;
        private HDevProcedureCall _cropCall, _measureCall;

        public TwoDVisionProvider(ILogger logger,
            IStationConfigService stationConfig,
            IParametersConfigService parametersConfig,
            IHalconEngineProvider engineProvider,
            IWindowHandleRegistry windowHandleRegistry,
            IAiDriveService aiDrive)
        {
            _logger = logger;
            _stationConfig = stationConfig;
            _paramService = parametersConfig;
            _engineProvider = engineProvider;
            _handleRegistry = windowHandleRegistry;
            _aiDrive = aiDrive;
        }


        public Task<string> ExecuteCalibrationAsync(HObject image, CalibrationParams param)
        {
            HTuple angleStart = _paramService.GetDouble("TemplateMatch", "angleStart");
            HTuple angleExtent = _paramService.GetDouble("TemplateMatch", "angleExtent");
            HTuple scaleMin = _paramService.GetDouble("TemplateMatch", "minScale");
            HTuple scaleMax = _paramService.GetDouble("TemplateMatch", "maxScale");
            HTuple minScore = _paramService.GetDouble("TemplateMatch", "minScore");
            HTuple numMatch = _paramService.GetInt("TemplateMatch", "maxMatchNum");
            HTuple maxOverlap = _paramService.GetDouble("TemplateMatch", "maxOverlap");
            HTuple subPixel = _paramService.GetDouble("TemplateMatch", "subPixel");
            HTuple numLevel = _paramService.GetInt("TemplateMatch", "numLevel");
            HTuple greediness = _paramService.GetDouble("TemplateMatch", "greediness");


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
            HTuple fx = _paramService.GetDouble("CalibrateParam", "fx");
            HTuple fy = _paramService.GetDouble("CalibrateParam", "fy");
            HOperatorSet.TupleConcat(matchParam, fx, out matchParam);
            HOperatorSet.TupleConcat(matchParam, fy, out matchParam);

            HDevProcedure hStep = new HDevProcedure();
            hStep.LoadProcedure("Cali2d");
            var procCall = new HDevProcedureCall(hStep);

            procCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
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
            _paramService.UpdateParam("CalibrateParam", "fx", result[3].D.ToString(), ParamOutputType.FLOAT);
            _paramService.UpdateParam("CalibrateParam", "fy", result[4].D.ToString(), ParamOutputType.FLOAT);
            string calibrateData = boardCenterX + boardCenterY;
            return Task.FromResult(calibrateResult + "," + calibrateData);
        }

        public async Task<string> ExecuteInspectAsync(HObject image, int poleNum, InspectionParams param)
        {
            string result = "01";
            string measureResults = string.Empty;
            HTuple resultArray = new HTuple();
            HTuple beadRect01 = new HTuple();
            HTuple beadRect02 = new HTuple();
            HOperatorSet.GenEmptyObj(out HObject mask01);
            HOperatorSet.GenEmptyObj(out HObject mask02);
            HOperatorSet.GenEmptyObj(out HObject mask03);
            bool isAiCheck = _paramService.GetBool("ProductParam", "isAiCheck");
            string sn = _stationConfig.GetStation(_stationId).CameraRole;
            //忘了需不需要再加个超时重试机制
            //_windowHandle = await _handleRegistry.WaitForHandleAsync(sn).ConfigureAwait(false);
            //目前的逻辑是深度学习一定勾选
            if (isAiCheck)
            {
                bool isSquareBarWeldMark = _paramService.GetBool("ProductParam", "isSquareBarWeldMark");
                if (isSquareBarWeldMark)
                    //这里score要从本地配置里读取 先写死
                    _aiDrive.DetectMulti(_aiModelId, 0, image, 0.8, out int[] beadType01, out beadRect01);
                else
                    _aiDrive.Detect(_aiModelId, 0, image, 0.8, out int beadType01, out beadRect01);
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
                    _cropCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
                    _cropCall.SetInputCtrlParamTuple("ParamSide", _stationId);
                    _cropCall.SetInputCtrlParamTuple("TargetRect", beadRect01); // 这里拿到检测框坐标数组
                    _cropCall.SetInputIconicParamObject("Image", image);
                    _cropCall.Execute();
                    HObject imgBead = _cropCall.GetOutputIconicParamObject("ImageRoi"); // 根据检测框裁切ROI
                                                                                        //-分割模型应用
                    if (isSquareBarWeldMark)
                    {   // 检测方条焊缝
                        _aiDrive.DetectMulti(_aiModelId, 0, imgBead, 0.8, out int[] beadType02, out beadRect02); // imgBead—>裁切ROI
                        _measureCall.SetInputCtrlParamTuple("BeadType", beadType02); // 数组类型
                    }
                    else
                    { // 检测单个焊缝
                        _aiDrive.Detect(_stationId, 0, imgBead, 0.8, out int beadType02, out beadRect02);
                        _measureCall.SetInputCtrlParamTuple("BeadType", beadType02);
                    }
                    _aiDrive.Predict(_aiModelId, 0, imgBead, out mask01);
                    _aiDrive.Predict(_aiModelId, 1, imgBead, out mask02); // 缺陷检测 识别爆孔、裂纹等缺陷
                    _aiDrive.Predict(_aiModelId, 2, imgBead, out mask03);

                    //-焊缝尺度测量
                    _measureCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
                    _measureCall.SetInputCtrlParamTuple("ParamSide", _stationId);
                    _measureCall.SetInputIconicParamObject("Image", imgBead);
                    _measureCall.SetInputIconicParamObject("Mask01", mask01);
                    _measureCall.SetInputIconicParamObject("Mask02", mask02);
                    _measureCall.SetInputIconicParamObject("Mask03", mask03);
                    //hCall03.SetWaitForDebugConnection(true);
                    _measureCall.Execute();
                    resultArray = _measureCall.GetOutputCtrlParamTuple("ResultArray");
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

        public Task<string> ExecuteVerificationAsync(HObject image, CalibrationParams param)
        {
            HTuple angleStart = _paramService.GetDouble("TemplateMatch", "angleStart");
            HTuple angleExtent = _paramService.GetDouble("TemplateMatch", "angleExtent");
            HTuple scaleMin = _paramService.GetDouble("TemplateMatch", "minScale");
            HTuple scaleMax = _paramService.GetDouble("TemplateMatch", "maxScale");
            HTuple minScore = _paramService.GetDouble("TemplateMatch", "minScore");
            HTuple numMatch = _paramService.GetInt("TemplateMatch", "maxMatchNum");
            HTuple maxOverlap = _paramService.GetDouble("TemplateMatch", "maxOverlap");
            HTuple subPixel = _paramService.GetDouble("TemplateMatch", "subPixel");
            HTuple numLevel = _paramService.GetInt("TemplateMatch", "numLevel");
            HTuple greediness = _paramService.GetDouble("TemplateMatch", "greediness");
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
            HTuple fx = _paramService.GetDouble("CalibrateParam", "fx");
            HTuple fy = _paramService.GetDouble("CalibrateParam", "fy");
            HOperatorSet.TupleConcat(matchParam, fx, out matchParam);
            HOperatorSet.TupleConcat(matchParam, fy, out matchParam);
            HDevProcedure hStep = new HDevProcedure();
            hStep.LoadProcedure("Check2d");
            var procCall = new HDevProcedureCall(hStep);

            procCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
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

        public async Task InitializeAsync(StationConfig config)
        {
            _stationId = config.StationId;
            _aiModelId = string.IsNullOrEmpty(config.AiModelStationId) ? config.StationId : config.AiModelStationId;
            _windowHandle = await _handleRegistry.WaitForHandleAsync(config.CameraRole)/*.ConfigureAwait(false)*/;
            _engineProvider.GetEngine(); // 确保Halcon引擎已初始化
            bool isSquareBarWeldMark = _paramService.GetBool("ProductParam", "isSquareBarWeldMark");
            if (isSquareBarWeldMark)
                paramDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SBProductParamA.json");
            else
                paramDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CircProductParamA.json");
            //加载并执行 LoadParam
            var loadProc = new HDevProcedure("LoadParam");
            var loadCall = new HDevProcedureCall(loadProc);
            loadCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
            loadCall.SetInputCtrlParamTuple("ParamDir", paramDir);
            loadCall.SetInputCtrlParamTuple("ParamSide", _stationId);
            //loadCall.Execute();
            loadCall.Dispose();
            loadProc.Dispose();
            //初始化Crop2d
            var cropProc = new HDevProcedure("Crop2d");
            _cropCall = new HDevProcedureCall(cropProc);
            //bool isCirWeldMark = _paramService.GetBool("ProductParam", "isCirWeldMark");
            //初始化Measure（根据产品类型选择）
            bool isCirWeldMark = _paramService.GetBool(config.ProductConfigSection, "isCirWeldMark");
            var measureProcName = isCirWeldMark ? "Measure2d" : "MeasureSB2D";
            var measureProc = new HDevProcedure(measureProcName);
            _measureCall = new HDevProcedureCall(measureProc);
            await Task.CompletedTask;
            _logger.Information("{StationId}工位2D视觉初始化成功", _stationId);
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
    }

    public class ThreeDVisionProvider : I3DVisionProvider
    {
        private string _stationId;
        private string _aiModelId;
        private string paramDir = string.Empty;
        private HWindow _windowHandle;
        private readonly IHalconEngineProvider _engineProvider;
        private readonly IAiDriveService _aiDrive;
        private readonly ILogger _logger;
        private readonly IParametersConfigService _paramService;
        private readonly IStationConfigService _stationConfig;
        private readonly IWindowHandleRegistry _handleRegistry;
        private HDevProcedureCall _cropCall, _measureCall, _planeFitCall;

        public ThreeDVisionProvider(ILogger logger,
            IStationConfigService stationConfig,
            IParametersConfigService parametersConfig,
            IHalconEngineProvider engineProvider,
            IWindowHandleRegistry windowHandleRegistry,
            IAiDriveService aiDrive)
        {
            _logger = logger;
            _stationConfig = stationConfig;
            _paramService = parametersConfig;
            _engineProvider = engineProvider;
            _handleRegistry = windowHandleRegistry;
            _aiDrive = aiDrive;
        }

        public Task<string> ExecuteCalibrationAsync(HObject image, CalibrationParams param)
        {
            //读取ROI区域
            string regionNameStrA = "Region" + "_" + (4).ToString() + ".hobj";
            string regionNameStrB = "Region" + "_" + (5).ToString() + ".hobj";
            HOperatorSet.ReadRegion(out HObject roiRegionA, regionNameStrA);
            HOperatorSet.ReadRegion(out HObject roiRegionB, regionNameStrB);
            //参数未配置好之前先写死，后续改成从配置里读
            double resoX = 0;
            double resoY = 0;
            double resoZ = 0;
            BoardCalibrate(image, roiRegionA, roiRegionB, resoX, resoY, resoZ, out string calibrateResult, out string calibrateData);
            return Task.FromResult(calibrateResult + "," + calibrateData);

        }
        public async Task<string> ExecuteInspectAsync(HObject image, int poleNum, InspectionParams param)
        {
            string result = "01";
            string measureResults = string.Empty;
            HTuple resultArray = new HTuple();
            HTuple beadRect = new HTuple();
            HOperatorSet.GenEmptyObj(out HObject mask01);
            HOperatorSet.GenEmptyObj(out HObject mask02);
            HOperatorSet.GenEmptyObj(out HObject mask03);
            bool isAiCheck = _paramService.GetBool("ProductParam", "isAiCheck");
            string sn = _stationConfig.GetStation(_stationId).CameraRole;
            //忘了需不需要再加个超时重试机制
            //_windowHandle = await _handleRegistry.WaitForHandleAsync(sn).ConfigureAwait(false);
            //目前的逻辑是深度学习一定勾选
            if (isAiCheck)
            {
                _cropCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
                _cropCall.SetInputCtrlParamTuple("ParamSide", _stationId);
                _cropCall.SetInputIconicParamObject("Image", image);
                _cropCall.Execute();
                HObject imgByte = _cropCall.GetOutputIconicParamObject("ImageByte");
                bool isSquareBarWeldMark = _paramService.GetBool("ProductParam", "isSquareBarWeldMark");
                if (isSquareBarWeldMark)
                    //这里score要从本地配置里读取 先写死
                    _aiDrive.DetectMulti(_aiModelId, 0, imgByte, 0.8, out int[] beadType01, out beadRect);
                else
                    _aiDrive.Detect(_aiModelId, 0, imgByte, 0.8, out int beadType01, out beadRect);
                var call = _measureCall != null ? _measureCall : _planeFitCall;
                call.SetInputCtrlParamTuple("BeadType", beadRect);
                if (beadRect.Length < 4)
                {
                    HOperatorSet.TupleGenConst(13, 2, out resultArray);
                    string result01 = DoubleToString(resultArray[2].D, 8);//方形余高
                    string result02 = DoubleToString(resultArray[4].D, 8);//方形下塌
                    string result03 = DoubleToString(resultArray[6].D, 8);//条形余高
                    string result04 = DoubleToString(resultArray[8].D, 8);//条形下塌
                    string result05 = DoubleToString(resultArray[10].D, 8);//
                    string result06 = DoubleToString(resultArray[12].D, 8);//
                    measureResults = "02" + result01 + result02 + result03 + result04 + result05 + result06;
                    return measureResults;
                }
                else
                {


                    call.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
                    call.SetInputCtrlParamTuple("ParamSide", _stationId);
                    call.SetInputCtrlParamTuple("TargetRect", beadRect);
                    call.SetInputIconicParamObject("Image", image);

                    call.Execute();
                    resultArray = call.GetOutputCtrlParamTuple("ResultArray");
                    imgByte.Dispose();
                    string data01 = DoubleToString(resultArray[2].D, 8);  //方形余高
                    string data02 = DoubleToString(resultArray[4].D, 8);  //方形下塌
                    string data03 = DoubleToString(resultArray[6].D, 8);  // 条形余高
                    string data04 = DoubleToString(resultArray[8].D, 8);  // 条形下塌
                    string data05 = DoubleToString(0, 8);
                    string data06 = DoubleToString(0, 8);
                    measureResults = result + data01 + data02 + data03 + data04 + data05 + data06;
                    await Task.FromResult( measureResults);
                }
            }
            else
            {
                result = "02";
                resultArray = new HTuple();
                HOperatorSet.TupleGenConst(13, 2, out resultArray);
                resultArray[0] = 02;
                string data01 = DoubleToString(resultArray[2].D, 8);//下榻
                string data02 = DoubleToString(resultArray[4].D, 8);//余高
                string data03 = DoubleToString(resultArray[6].D, 8);//
                string data04 = DoubleToString(resultArray[8].D, 8);//
                string data05 = DoubleToString(resultArray[10].D, 8);//
                string data06 = DoubleToString(resultArray[12].D, 8);//
                measureResults = result + data01 + data02 + data03 + data04 + data05 + data06;
                await Task.FromResult(measureResults);
            }
            return measureResults;
        }
       
        public async Task InitializeAsync(StationConfig config)
        {
            _stationId = config.StationId;
            _aiModelId = string.IsNullOrEmpty(config.AiModelStationId) ? config.StationId : config.AiModelStationId;
            var handle = await _handleRegistry.WaitForHandleAsync(config.CameraRole);
            _engineProvider.GetEngine(); // 确保Halcon引擎已初始化
            bool isSquareBarWeldMark = _paramService.GetBool("ProductParam", "isSquareBarWeldMark");
            if (isSquareBarWeldMark)
                paramDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SBProductParamB.json");
            else
                paramDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CircProductParamB.json");
            //加载并执行 LoadParam
            var loadProc = new HDevProcedure("LoadParam");
            var loadCall = new HDevProcedureCall(loadProc);
            loadCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
            loadCall.SetInputCtrlParamTuple("ParamDir", paramDir);
            loadCall.SetInputCtrlParamTuple("ParamSide", _stationId);
            loadCall.Execute();
            loadCall.Dispose();
            loadProc.Dispose();
            //初始化Crop3d
            var cropProc = new HDevProcedure("Crop3d");
            _cropCall = new HDevProcedureCall(cropProc);
            bool isPlanecheck = _paramService.GetBool(config.ProductConfigSection, "isPlanecheck");
            if (isPlanecheck)
            {
                bool isSquare = _paramService.GetBool(config.ProductConfigSection, "isSquareBarWeldMark");
                var proc = isSquare ? new HDevProcedure("PlaneFitSB3D") : new HDevProcedure("PlaneFit3D");
                _planeFitCall = new HDevProcedureCall(proc);
            }
            else
            {
                bool isSquare = _paramService.GetBool(config.ProductConfigSection, "isSquareBarWeldMark");
                var proc = isSquare ? new HDevProcedure("MeasureSB3d") : new HDevProcedure("Measure3d");
                _measureCall = new HDevProcedureCall(proc);
            }
        }

        private void BoardCalibrate(HObject img, HObject roiA, HObject roiB, double resoX, double resoY, double resoZ, out string result, out string resultData)
        {

            HTuple highDiff = null;
            try
            {
                HeightMeasure(img, roiA, roiB, resoX, resoY, resoZ, out highDiff);
            }
            catch (Exception)
            {
                result = "02";
                resultData = "";
                throw;
            }
            result = "01";
            resultData = DoubleToString(highDiff.D, 8) + "+0000000";
        }

        private void HeightMeasure(HObject ho_img, HObject ho_roi01, HObject ho_roi02, HTuple hv_resoX, HTuple hv_resoY, HTuple hv_resoZ, out HTuple hv_highDiff)
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
                    HOperatorSet.XyzToObjectModel3d(ho_ReducedXb, ho_ReducedYb, ho_ReducedZb, out hv_ms3Db00);
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
    }
}

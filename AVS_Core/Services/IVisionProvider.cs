using AVS_Common.Events;
using AVS_Common.Services;
using AVS_Core.Models;
using AVS_Drivers.Camera.Common.Enum;
using AVS_Service;
using AVS_Service.Models;
using AVS_Service.Services;
using HalconDotNet;
using Prism.Events;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
        private HWindow _backgroundWindow;      // 后台窗口，替代 _windowHandle
        private string _cameraSN;               // 用于发布事件时标识相机
        private HWindow _windowHandle;
        private readonly IHalconEngineProvider _engineProvider;
        private readonly IAiDriveService _aiDrive;
        private readonly IInspectionCsvService _csvService;
        private readonly ILogger _logger;
        private readonly IParametersConfigService _paramService;
        private readonly IStationConfigService _stationConfig;
        private readonly IWindowHandleRegistry _handleRegistry;
        //private readonly IWindowHandleManager _handleManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly IImageSaveService _imageSaveService;
        private string paramDir = string.Empty;
        private HDevProcedureCall _cropCall, _measureCall;

        public TwoDVisionProvider(ILogger logger,
            IStationConfigService stationConfig,
            IParametersConfigService parametersConfig,
            IHalconEngineProvider engineProvider,
            IWindowHandleRegistry windowHandleRegistry,
            IEventAggregator eventAggregator,
            //IWindowHandleManager handleManager,
            ICameraConfigService cameraConfigService,
            IImageSaveService imageSaveService,
            IInspectionCsvService csvService,
            IAiDriveService aiDrive)
        {
            _logger = logger;
            _stationConfig = stationConfig;
            _paramService = parametersConfig;
            _engineProvider = engineProvider;
            _eventAggregator = eventAggregator;
            _imageSaveService = imageSaveService;
            //_handleRegistry = handleManager;
            _handleRegistry = windowHandleRegistry;
            _cameraConfigService = cameraConfigService;
            _csvService = csvService;
            _aiDrive = aiDrive;
        }


        public Task<string> ExecuteCalibrationAsync(HObject image, CalibrationParams param)
        {
            var p = _paramService.GetStationParams(_stationId);
            HTuple matchParam = new HTuple();
            HOperatorSet.TupleConcat(matchParam, p.AngleStart, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.AngleExtent, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MinScale, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MaxScale, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MinScore, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MaxMatchNum, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MaxOverlap, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.SubPixel, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.NumLevel, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.Greediness, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.Fx, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.Fy, out matchParam);

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
            _paramService.UpdateParam(_stationId, "Fx", result[3].D.ToString(), ParamOutputType.FLOAT);
            _paramService.UpdateParam(_stationId, "Fy", result[4].D.ToString(), ParamOutputType.FLOAT);
            string calibrateData = boardCenterX + boardCenterY;
            return Task.FromResult(calibrateResult + "," + calibrateData);
        }

        /// <summary>
        /// 检查逻辑
        /// </summary>
        /// <param name="image"></param>
        /// <param name="poleNum"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Task<string> ExecuteInspectAsync(HObject image, int poleNum, InspectionParams param)
        {
            var p = _paramService.GetStationParams(_stationId);
            string result = "01";
            string measureResults = string.Empty;
            HTuple resultArray = new HTuple();
            HTuple beadRect01 = new HTuple();
            HTuple beadRect02 = new HTuple();
            HOperatorSet.GenEmptyObj(out HObject mask01);
            HOperatorSet.GenEmptyObj(out HObject mask02);
            HOperatorSet.GenEmptyObj(out HObject mask03);

            //HOperatorSet.GetImageSize(image, out HTuple imgWidth, out HTuple imgHeight);
            //EnsureBackgroundWindow((int)imgWidth, (int)imgHeight);

            string sn = _stationConfig.GetStation(_stationId).CameraRole;
            if (p.IsAiCheck)
            {

                if (p.IsSquareBarWeldMark)
                    _aiDrive.DetectMulti(_aiModelId, 0, image, p.ScoreValue, out int[] beadType01, out beadRect01);
                else
                    _aiDrive.Detect(_aiModelId, 0, image, p.ScoreValue, out int beadType01, out beadRect01);
                if (beadRect01.Length < 4)
                {
                    mask01.Dispose();
                    mask02.Dispose();
                    mask03.Dispose();
                    HOperatorSet.TupleGenConst(13, 2, out resultArray);
                    string result01 = DoubleToString(resultArray[2].D, 8);//焊缝长度
                    string result02 = DoubleToString(resultArray[4].D, 8);//焊缝宽度
                    string result03 = DoubleToString(resultArray[6].D, 8);//焊缝偏移
                    string result04 = DoubleToString(resultArray[8].D, 8);//爆孔面积
                    string result05 = DoubleToString(resultArray[10].D, 8);//焊缝外径
                    string result06 = DoubleToString(resultArray[12].D, 8);//虚焊尺寸
                    measureResults = "02" + result01 + result02 + result03 + result04 + result05 + result06;
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
                    if (p.IsSquareBarWeldMark)
                    {   // 检测方条焊缝
                        _aiDrive.DetectMulti(_aiModelId, 0, imgBead, p.ScoreValue, out int[] beadType02, out beadRect02); // imgBead—>裁切ROI
                        _measureCall.SetInputCtrlParamTuple("BeadType", beadType02); // 数组类型
                    }
                    else
                    { // 检测单个焊缝
                        _aiDrive.Detect(_aiModelId, 0, imgBead, p.ScoreValue, out int beadType02, out beadRect02);
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

                }
            }
            else
            {
                try
                {
                    measureResults = new string('0', 50);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "2D传统检测出错");
                    measureResults = "02" + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8)
                        + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8);
                }

            }

            //HObject processedImage = new HObject();
            //HOperatorSet.DumpWindowImage(out processedImage, _backgroundWindow);
            //_eventAggregator.GetEvent<HImageDisplayEvent>().Publish(new CameraImagePayload
            //{
            //    CameraSN = _cameraSN,
            //    Image = processedImage,
            //    ImageType = CameraImageType.Processed
            //});
            if (resultArray != null && resultArray.Length >= 13)
            {
                var inspect2D = new InspectResult2DData
                {
                    WorkType = param.WorkType,
                    ModuleName = param.ModuleName,
                    PoleNum = poleNum,
                    DateTime = DateTime.Now,
                    Result2D = (Result)resultArray[0].I,
                    ResultLength = (Result)resultArray[1].I,
                    Length = resultArray[2].D,
                    ResultWidth = (Result)resultArray[3].I,
                    Width = resultArray[4].D,
                    ResultOffset = (Result)resultArray[5].I,
                    Offset = resultArray[6].D,
                    ResultPoreBreak = (Result)resultArray[7].I,
                    PoreBreakArea = resultArray[8].D,
                    ResultBeadDiameter = (Result)resultArray[9].I,
                    BeadDiameter = resultArray[10].D,
                    ResultfaultySol = (Result)resultArray[11].I,
                    faultySol = resultArray[12].D,
                    IsSquareBar = p.IsSquareBarWeldMark
                };
                _csvService.Report2D(inspect2D);
                HObject resultImage = null;
                try { HOperatorSet.DumpWindowImage(out resultImage, _windowHandle); } catch { }
                _imageSaveService.Save2DImages(
                    inspect2D,
                    image.Clone(),
                    resultImage,
                    mask01,
                    mask02,
                    mask03);
                mask01.Dispose();
                mask02.Dispose();
                mask03.Dispose();
                resultImage?.Dispose();
            }
            _logger.Information("[2D检测] 工位={StationId} 极柱={Pole} 检测结果: {Result}", _stationId, poleNum, measureResults);
            return Task.FromResult(measureResults);

        }

        private void EnsureBackgroundWindow(int width, int height)
        {
            if (_backgroundWindow == null)
            {
                _backgroundWindow = new HWindow(0, 0, width, height, 0, "buffer", "localhost");
            }
            else
            {
                _backgroundWindow.Dispose();
                _backgroundWindow = new HWindow(0, 0, width, height, 0, "buffer", "localhost");
            }
            _backgroundWindow.SetPart(0, 0, height - 1, width - 1);
        }

        public Task<string> ExecuteVerificationAsync(HObject image, CalibrationParams param)
        {

            var p = _paramService.GetStationParams(_stationId);
            HTuple matchParam = new HTuple();
            HOperatorSet.TupleConcat(matchParam, p.AngleStart, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.AngleExtent, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MinScale, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MaxScale, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MinScore, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MaxMatchNum, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.MaxOverlap, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.SubPixel, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.NumLevel, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.Greediness, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.Fx, out matchParam);
            HOperatorSet.TupleConcat(matchParam, p.Fy, out matchParam);
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
            //这里注释掉，因为后台halcon代码会showimg，导致前端view的渲染\原图按钮无效
            _windowHandle = await _handleRegistry.WaitForHandleAsync(config.CameraRole);
            var cameraSetting = _cameraConfigService.AllSettings.FirstOrDefault(c => c.CameraRole == config.CameraRole);
            _cameraSN = cameraSetting?.SerilalNum;
            if (string.IsNullOrEmpty(_cameraSN))
                _logger.Warning("未找到相机角色 {Role} 对应的相机SN，处理图事件可能无法匹配UI", config.CameraRole);

            // 创建后台窗口（初始大小可任意,之后会按图像调整,这里传入localhost可以为任意非空字符）
            //_backgroundWindow = new HWindow(0, 0, 512, 512, 0, "buffer", "localhost");
            //_backgroundWindow.SetColor("green");
            //_backgroundWindow.SetLineWidth(2);

            _engineProvider.GetEngine(); // 确保Halcon引擎已初始化
            var p = _paramService.GetStationParams(_stationId);

            paramDir = p.IsSquareBarWeldMark
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "SBProductParamA.json")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "CircProductParamA.json");
            //加载并执行 LoadParam
            var loadProc = new HDevProcedure("LoadParam");
            var loadCall = new HDevProcedureCall(loadProc);
            loadCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
            loadCall.SetInputCtrlParamTuple("ParamDir", paramDir);
            loadCall.SetInputCtrlParamTuple("ParamSide", _stationId);
            //这里先注释，报错了
            loadCall.Execute();
            loadCall.Dispose();
            loadProc.Dispose();
            //初始化Crop2d
            var cropProc = new HDevProcedure("Crop2d");
            _cropCall = new HDevProcedureCall(cropProc);
            //bool isCirWeldMark = _paramService.GetBool("ProductParam", "isCirWeldMark");
            //初始化Measure（根据产品类型选择）
            var measureProcName = p.IsCirWeldMark ? "Measure2d" : "MeasureSB2D";
            var measureProc = new HDevProcedure(measureProcName);
            _measureCall = new HDevProcedureCall(measureProc);
            await Task.CompletedTask;
            _logger.Information("{StationId}工位2D视觉初始化成功", _stationId);
        }

        private string DoubleToString(double detectValue, int len)
        {
            // NaN/Infinity 保护
            if (double.IsNaN(detectValue) || double.IsInfinity(detectValue))
            {
                return "+" + new string('0', len - 1);
            }

            // 放大1000倍，限制范围
            double scaled = Math.Max(-99999999, Math.Min(99999999, detectValue * 1000));
            int valueInt = Math.Abs((int)Math.Round(scaled));

            // 截断到 len-1 位
            string valueStr = valueInt.ToString().PadLeft(len - 1, '0');
            if (valueStr.Length > len - 1)
                valueStr = valueStr.Substring(valueStr.Length - (len - 1));

            return (detectValue >= 0 ? "+" : "-") + valueStr;
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
        private readonly IInspectionCsvService _csvService;
        private readonly ILogger _logger;
        private readonly IParametersConfigService _paramService;
        private readonly IStationConfigService _stationConfig;
        private readonly IWindowHandleRegistry _handleRegistry;
        private readonly IImageSaveService _imageSaveService;
        private readonly ICameraConfigService _cameraConfigService;
        //private readonly IWindowHandleManager _handleManager;
        private HDevProcedure _cropProc, _measureProc, _planeFitProc;
        private HDevProcedureCall _cropCall, _measureCall, _planeFitCall;

        public ThreeDVisionProvider(ILogger logger,
            IStationConfigService stationConfig,
            IParametersConfigService parametersConfig,
            IHalconEngineProvider engineProvider,
            IWindowHandleRegistry windowHandleRegistry,
            //IWindowHandleManager handleManager,
            IInspectionCsvService csvService,
            IImageSaveService imageSaveService,
            ICameraConfigService cameraConfigService,
            IAiDriveService aiDrive)
        {
            _logger = logger;
            _stationConfig = stationConfig;
            _paramService = parametersConfig;
            _engineProvider = engineProvider;
            //_handleManager = handleManager;
            _imageSaveService = imageSaveService;
            _handleRegistry = windowHandleRegistry;
            _cameraConfigService = cameraConfigService;
            _csvService = csvService;
            _aiDrive = aiDrive;
        }

        public Task<string> ExecuteCalibrationAsync(HObject image, CalibrationParams param)
        {
            //读取ROI区域
            var p = _paramService.GetStationParams(_stationId);
            string recipePath = p.RecipePath;
            if (string.IsNullOrEmpty(recipePath))
                recipePath = AppDomain.CurrentDomain.BaseDirectory;

            string regionNameStrA = Path.Combine(recipePath, $"Region{_stationId}_4.hobj");
            string regionNameStrB = Path.Combine(recipePath, $"Region{_stationId}_5.hobj");

            HOperatorSet.ReadRegion(out HObject roiRegionA, regionNameStrA);
            HOperatorSet.ReadRegion(out HObject roiRegionB, regionNameStrB);
            double resoX = p.Fx;
            double resoY = p.Fy;
            double resoZ = p.Fz;
            BoardCalibrate(image, roiRegionA, roiRegionB, resoX, resoY, resoZ, out string calibrateResult, out string calibrateData);
            roiRegionA.Dispose();
            roiRegionB.Dispose();
            return Task.FromResult(calibrateResult + "," + calibrateData);

        }

        public Task<string> ExecuteInspectAsync(HObject image, int poleNum, InspectionParams param)
        {
            var p = _paramService.GetStationParams(_stationId);
            string result = "01";
            string measureResults = string.Empty;
            HTuple resultArray = new HTuple();
            HTuple beadRect = new HTuple();
            HObject mask01 = null, mask02 = null, mask03 = null;
            HObject mask01Img = null, mask02Img = null, mask03Img = null;
            HObject resultImage = null;
            bool canMeasure = true;

            try
            {
                HOperatorSet.GenEmptyObj(out mask01);
                HOperatorSet.GenEmptyObj(out mask02);
                HOperatorSet.GenEmptyObj(out mask03);
                HOperatorSet.GenEmptyObj(out mask01Img);
                HOperatorSet.GenEmptyObj(out mask02Img);
                HOperatorSet.GenEmptyObj(out mask03Img);

                // 获取当前使用的测量过程（平面拟合或测量）
                var call = _measureCall != null ? _measureCall : _planeFitCall;

                if (p.IsAiCheck)
                {
                    // ===== AI 检测 =====
                    // 执行裁剪过程
                    _cropCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
                    _cropCall.SetInputCtrlParamTuple("ParamSide", _stationId);
                    _cropCall.SetInputIconicParamObject("Image", image);
                    _cropCall.Execute();
                    HObject imgByte = _cropCall.GetOutputIconicParamObject("ImageByte");

                    bool use3DSegmentation = p.IsAiCheck && !p.IsSquareBarWeldMark && p.Is3DSegmentation;
                    if (use3DSegmentation)
                    {
                        // ===== 3D 分割模式 =====
                        // 调用 AI 分割模型获取三个 Mask
                        _aiDrive.Predict3DImage(_stationId, 0, imgByte, out HObject masks, out mask01, out mask02, out mask03);
                        masks?.Dispose(); // 父对象可释放，Mask 已单独取出

                        // 转换 Mask 为图像格式，供 Halcon 过程使用
                        HTuple width, height;
                        HOperatorSet.GetImageSize(imgByte, out width, out height);
                        HOperatorSet.RegionToBin(mask01, out mask01Img, 255, 0, width, height);
                        HOperatorSet.RegionToBin(mask02, out mask02Img, 255, 0, width, height);
                        HOperatorSet.RegionToBin(mask03, out mask03Img, 255, 0, width, height);

                        // 传入 Mask 图像
                        call.SetInputIconicParamObject("Mask01", mask01Img);
                        call.SetInputIconicParamObject("Mask02", mask02Img);
                        call.SetInputIconicParamObject("Mask03", mask03Img);
                    }
                    else
                    {
                        // ===== AI 定位模式 =====
                        if (p.IsSquareBarWeldMark)
                        {
                            _aiDrive.DetectMulti(_aiModelId, 0, imgByte, p.ScoreValue, out int[] beadType, out beadRect);
                            call.SetInputCtrlParamTuple("BeadType", beadType);
                        }
                        else
                        {
                            _aiDrive.Detect(_aiModelId, 0, imgByte, p.ScoreValue, out int beadType, out beadRect);
                            call.SetInputCtrlParamTuple("BeadType", beadType);
                        }

                        int requiredRectLength = p.IsSquareBarWeldMark ? 8 : 4;
                        if (beadRect.Length < requiredRectLength)
                        {
                            // 定位失败，生成失败结果
                            canMeasure = false;
                            resultArray = new HTuple();
                            HOperatorSet.TupleGenConst(13, 2, out resultArray);
                            _logger.Warning("3D定位焊缝框数量不足，期望 {Expected}，实际 {Actual}", requiredRectLength, beadRect.Length);
                        }
                        else
                        {
                            call.SetInputCtrlParamTuple("TargetRect", beadRect);
                        }
                    }

                    if (canMeasure)
                    {
                        // 执行测量/平面拟合过程
                        call.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
                        call.SetInputCtrlParamTuple("ParamSide", _stationId);
                        call.SetInputIconicParamObject("Image", image);
                        call.Execute();
                        resultArray = call.GetOutputCtrlParamTuple("ResultArray");
                    }

                    imgByte?.Dispose();
                }
                else
                {
                    // ===== 传统检测（非AI）=====
                    // 当前无实际测量，结果全0
                    resultArray = new HTuple();
                    HOperatorSet.TupleGenConst(13, 0, out resultArray); // 生成13个0
                    _logger.Information("3D传统检测模式，未执行AI算法");
                }

                // ===== 构造结果字符串（固定50字符） =====
                if (resultArray.Length >= 9)
                {
                    string data01 = DoubleToString(resultArray[2].D, 8);  // 方形余高
                    string data02 = DoubleToString(resultArray[4].D, 8);  // 方形下塌
                    string data03 = DoubleToString(resultArray[6].D, 8);  // 条形余高
                    string data04 = DoubleToString(resultArray[8].D, 8);  // 条形下塌
                    string data05 = DoubleToString(0, 8);
                    string data06 = DoubleToString(0, 8);
                    result = resultArray[0].D == 0 ? "01" : "02";
                    measureResults = result + data01 + data02 + data03 + data04 + data05 + data06;
                }
                else
                {
                    // 结果数组异常，返回失败
                    measureResults = "02" + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8);
                }

                // ===== 保存 CSV 和图像 =====
                if (resultArray != null && resultArray.Length >= 9)
                {
                    var inspect3D = new InspectResult3DData
                    {
                        WorkType = param.WorkType,
                        ModuleName = param.ModuleName,
                        PoleNum = poleNum,
                        DateTime = DateTime.Now,
                        Result3D = (Result)resultArray[0].I,
                        ResultBeadHump = (Result)resultArray[1].I,
                        BeadHump = resultArray[2].D,
                        ResultBeadSag = (Result)resultArray[3].I,
                        BeadSag = resultArray[4].D,
                        ResultBarBeadHump = (Result)resultArray[5].I,
                        BarBeadHump = resultArray[6].D,
                        ResultBarBeadSag = (Result)resultArray[7].I,
                        BarBeadSag = resultArray[8].D
                    };
                    _csvService.Report3D(inspect3D);

                    // 获取窗口截图
                    try { HOperatorSet.DumpWindowImage(out resultImage, _windowHandle); } catch { }

                    _imageSaveService.Save3DImages(
                        inspect3D,
                        image.Clone(),          // 深度图（克隆后由保存服务负责释放）
                        null,                   // 亮度图，当前未传入
                        resultImage,            // 结果图
                        mask01,                /*mask01Img?mask01*/
                        mask02,
                        mask03);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "3D检测执行异常，工位={StationId} 极柱={Pole}", _stationId, poleNum);
                measureResults = "02" + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8) + DoubleToString(0, 8);
            }
            finally
            {
                // 确保所有临时对象释放
                mask01?.Dispose();
                mask02?.Dispose();
                mask03?.Dispose();
                mask01Img.Dispose();
                mask02Img.Dispose();
                mask03Img.Dispose();
                resultImage?.Dispose();
            }

            _logger.Information("[3D检测] 工位={StationId} 极柱={Pole} 检测结果: {Result}", _stationId, poleNum, measureResults);
            return Task.FromResult(measureResults);
        }

        public async Task InitializeAsync(StationConfig config)
        {
            _stationId = config.StationId;
            _aiModelId = string.IsNullOrEmpty(config.AiModelStationId) ? config.StationId : config.AiModelStationId;
            _windowHandle = await _handleRegistry.WaitForHandleAsync(config.CameraRole);
            _engineProvider.GetEngine();

            var p = _paramService.GetStationParams(_stationId);
            paramDir = p.IsSquareBarWeldMark
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SBProductParamB.json")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CircProductParamB.json");

            var cameraSetting = _cameraConfigService.AllSettings.FirstOrDefault(s => s.CameraRole == config.CameraRole);
            if (cameraSetting == null)
            {
                _logger.Error("未找到相机角色 {Role} 的配置", config.CameraRole);
                throw new InvalidOperationException($"未找到相机角色 {config.CameraRole} 的配置");
            }
            CameraBrand cameraBrand = (CameraBrand)cameraSetting.CameraType;
            bool isLmi = cameraBrand == CameraBrand.LMI3D;

            try
            {
                // 释放旧的过程对象
                _cropCall?.Dispose();
                _cropProc?.Dispose();
                _planeFitCall?.Dispose();
                _planeFitProc?.Dispose();
                _measureCall?.Dispose();
                _measureProc?.Dispose();

                // 加载 LoadParam
                using (var loadProc = new HDevProcedure("LoadParam"))
                using (var loadCall = new HDevProcedureCall(loadProc))
                {
                    loadCall.SetInputCtrlParamTuple("WindowHandle", _windowHandle);
                    loadCall.SetInputCtrlParamTuple("ParamDir", paramDir);
                    loadCall.SetInputCtrlParamTuple("ParamSide", _stationId);
                    loadCall.Execute();
                }

                // 初始化 Crop3d
                string cropProcName = isLmi ? "Crop3d" : "Crop3d_HK";
                _cropProc = new HDevProcedure(cropProcName);
                _cropCall = new HDevProcedureCall(_cropProc);

                bool use3DSegmentation = p.IsAiCheck && !p.IsSquareBarWeldMark && p.Is3DSegmentation;

                if (p.IsPlaneCheck)
                {
                    string procName;
                    if (p.IsSquareBarWeldMark)
                        procName = "PlaneFitSB3D";
                    else if (!isLmi) // HK
                        procName = use3DSegmentation ? "PlaneFit3DHKML" : "PlaneFit3DHK";
                    else // LMI
                        procName = use3DSegmentation ? "PlaneFit3DML" : "PlaneFit3D";

                    _planeFitProc = new HDevProcedure(procName);
                    _planeFitCall = new HDevProcedureCall(_planeFitProc);
                }
                else
                {
                    string procName = p.IsSquareBarWeldMark ? "MeasureSB3d" : "Measure3d";
                    _measureProc = new HDevProcedure(procName);
                    _measureCall = new HDevProcedureCall(_measureProc);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "3D视觉提供者初始化失败，工位：{StationId}", _stationId);
                throw;
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
            // NaN/Infinity 保护
            if (double.IsNaN(detectValue) || double.IsInfinity(detectValue))
            {
                return "+" + new string('0', len - 1);
            }

            // 放大1000倍，限制范围
            double scaled = Math.Max(-99999999, Math.Min(99999999, detectValue * 1000));
            int valueInt = Math.Abs((int)Math.Round(scaled));

            // 截断到 len-1 位
            string valueStr = valueInt.ToString().PadLeft(len - 1, '0');
            if (valueStr.Length > len - 1)
                valueStr = valueStr.Substring(valueStr.Length - (len - 1));

            return (detectValue >= 0 ? "+" : "-") + valueStr;
        }
    }
}

using AVS_Core.Models;
using HalconDotNet;
using Serilog;
using System;
using System.Collections.Generic;
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
        private HDevEngine _engine;
        private HDevProcedure _proc2DInspect, _proc2DCalib, _proc2DVerify;
        // ... 其他过程变量

        public VisionService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task InitializeAsync(string stationId)
        {
            _engine = new HDevEngine();
            _engine.SetProcedurePath(@"C:\RecipePath"); // 从配置读取
            _engine.StartDebugServer();

            if (stationId == "A")
            {
                _proc2DInspect = new HDevProcedure("Measure2d");
                _proc2DCalib = new HDevProcedure("Cali2d");
                _proc2DVerify = new HDevProcedure("Check2d");
                // 加载 AI 模型等
            }
            else // B (3D)
            {
                // 加载 3D 相关过程
            }
            await Task.CompletedTask;
            _logger.Information("Vision engine initialized for {StationId}", stationId);
        }

        public Task<HTuple> Execute2DInspectAsync(HObject image, int poleNumber, InspectionParams param)
        {
            var procCall = new HDevProcedureCall(_proc2DInspect);
            procCall.SetInputIconicParamObject("Image", image);
            // 设置其他输入参数...
            procCall.Execute();
            HTuple result = procCall.GetOutputCtrlParamTuple("ResultArray");
            return Task.FromResult(result);
        }

        public Task<HTuple> Execute3DInspectAsync(HObject image, int poleNumber, InspectionParams param)
        {
            // 类似 2D，调用 3D 过程
            throw new NotImplementedException();
        }

        public Task<HTuple> ExecuteCalibrationAsync(HObject image, CalibrationParams param)
        {
            var procCall = new HDevProcedureCall(_proc2DCalib);
            procCall.SetInputIconicParamObject("Image", image);
            procCall.Execute();
            HTuple result = procCall.GetOutputCtrlParamTuple("ResultArray");
            return Task.FromResult(result);
        }

        public Task<HTuple> ExecuteVerificationAsync(HObject image, CalibrationParams param)
        {
            var procCall = new HDevProcedureCall(_proc2DVerify);
            procCall.SetInputIconicParamObject("Image", image);
            procCall.Execute();
            HTuple result = procCall.GetOutputCtrlParamTuple("ResultArray");
            return Task.FromResult(result);
        }
    }
}

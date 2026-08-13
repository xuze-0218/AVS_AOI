using AVS_Core.Models;
using AVS_Service;
using AVS_Service.Models;
using HalconDotNet;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AVS_Core.Services
{
    /// <summary>
    /// 离线测试服务接口
    /// </summary>
    public interface ILocalTestService
    {
        /// <summary>
        /// 执行本地检测测试
        /// </summary>
        Task<LocalTestResult> RunInspectTestAsync(string stationId, List<string> imagePaths, CancellationToken ct = default);

        /// <summary>
        /// 执行本地标定测试
        /// </summary>
        Task<LocalTestResult> RunCalibrationTestAsync(string stationId, List<string> imagePaths, bool isVerify = false, CancellationToken ct = default);
    }

    public class LocalTestService : ILocalTestService
    {
        private readonly IStationSessionService _sessionService;
        private readonly IParametersConfigService _paramService;
        private readonly ILogger _logger;

        public LocalTestService(
            IStationSessionService sessionService,
            IParametersConfigService paramService,
            ILogger logger)
        {
            _sessionService = sessionService;
            _paramService = paramService;
            _logger = logger;
        }

        public async Task<LocalTestResult> RunInspectTestAsync(string stationId, List<string> imagePaths, CancellationToken ct = default)
        {
            try
            {
                if (imagePaths == null || imagePaths.Count == 0)
                    return new LocalTestResult { Success = false, Message = "未选择图片" };

                int[] poleOrder = LoadPoleOrderFromRecipe();
                if (poleOrder == null)
                    return new LocalTestResult { Success = false, Message = "未找到配方检测顺序" };

                if (imagePaths.Count > poleOrder.Length)
                    return new LocalTestResult { Success = false, Message = $"图片数量({imagePaths.Count})超过极柱数({poleOrder.Length})" };
                //初始化检测会话
                var initParams = new InspectionInitParams
                {
                    ImageName = "LocalTest",
                    MsgPoleCapacity = 25,
                    PoleOrder = poleOrder
                };
                await _sessionService.InitializeSession(stationId, SessionWorkType.Inspect, initParams);
                foreach (var file in imagePaths)
                {
                    HObject img = new HObject();
                    HOperatorSet.GenEmptyObj(out img);
                    HOperatorSet.ReadImage(out img, file);
                    _sessionService.EnqueueImage(stationId, img);
                    // 可以加小延迟模拟真实节拍，也可以不加
                    await Task.Delay(100, ct);
                }

                _logger.Information("本地检测测试已提交 {Count} 张图片", imagePaths.Count);

                return new LocalTestResult
                {
                    Success = true,
                    Message = $"已提交 {imagePaths.Count} 张本地图片，结果等待 PLC 查询或直接查看日志"
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "本地检测测试失败");
                return new LocalTestResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<LocalTestResult> RunCalibrationTestAsync(string stationId, List<string> imagePaths, bool isVerify = false, CancellationToken ct = default)
        {
            try
            {
                if (imagePaths == null || imagePaths.Count == 0)
                    return new LocalTestResult { Success = false, Message = "未选择图片" };
                //初始化标定会话
                await _sessionService.InitializeSession(stationId,
                    isVerify ? SessionWorkType.Verify : SessionWorkType.Calibrate);

                //只取第一张图片
                HObject img = new HObject();
                HOperatorSet.GenEmptyObj(out img);
                HOperatorSet.ReadImage(out img, imagePaths[0]);
                _sessionService.EnqueueImage(stationId, img);
                //等待标定完成（最多等10秒）
                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(500, ct);
                    string data = _sessionService.GetResultData(stationId);
                    if (data.Length >= 16 && data.Substring(0, 2) != "00")
                    {
                        return new LocalTestResult
                        {
                            Success = true,
                            Message = "标定完成",
                            ResultData = data
                        };
                    }
                }

                return new LocalTestResult { Success = false, Message = "标定超时" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "本地标定测试失败");
                return new LocalTestResult { Success = false, Message = ex.Message };
            }
        }


        private int[] LoadPoleOrderFromRecipe()
        {
            string json = _paramService.GetString("Recipe", "InspectOrders", "");
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                var orders = JsonConvert.DeserializeObject<InspectOrder[]>(json);
                if (orders == null || orders.Length == 0) return null;

                var order = orders[0]; //使用配方1
                int total = order.Row * order.Col;
                int[] poleOrder = new int[total];

                for (int j = 0; j < order.Row; j++)
                {
                    int mdiff = Math.Abs(order.End[j] - order.Start[j]) / (order.Col - 1);
                    if (order.End[j] - order.Start[j] < 0) mdiff = -mdiff;
                    for (int i = 0; i < order.Col; i++)
                    {
                        poleOrder[j * order.Col + i] = order.Start[j] + mdiff * i;
                    }
                }
                return poleOrder;
            }
            catch
            {
                return null;
            }
        }
    }


    public class LocalTestResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ResultData { get; set; }   // 标定结果数据或检测结果汇总
    }
}

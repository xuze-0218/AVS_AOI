using AVS_Core.Models;
using AVS_Service;
using HalconDotNet;
using Serilog;
using System.Collections.Concurrent;


namespace AVS_Core.Services
{
    /// <summary>
    /// 会话服务,根据报文指令执行不同逻辑
    /// </summary>
    public interface IStationSessionService
    {
        /// <summary>
        /// 初始化会话
        /// </summary>
        /// <param name="stationId">工位（"左侧2D"/"右侧3D"）</param>
        /// <param name="workType">会话类型（检测、标定、点检）</param>
        /// <param name="parameters">可选的初始化参数，如检测的 inspectOrder，标定所需的配置等</param>
        void InitializeSession(string stationId, SessionWorkType workType, object parameters = null);

        /// <summary>
        /// 相机采集到图像后，将其放入对应工位的处理队列
        /// </summary>
        void EnqueueImage(string stationId, HObject image);

        /// <summary>
        /// 获取检测或标定的结果数据（用于PLC回复）
        /// 检测模式需要提供极柱开始和结束序号
        /// </summary>
        string GetResultData(string stationId, int? startIndex = null, int? endIndex = null);

        /// <summary>
        /// 重置指定工位的会话
        /// </summary>
        void Reset(string stationId);
    }

    public class StationSessionService : IStationSessionService
    {
        private readonly IProtocolEngineService _protocolEngine;
        private readonly IVisionService _visionService;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SessionState> _sessions = new();

        public StationSessionService(IProtocolEngineService protocolEngine, IVisionService visionService, ILogger logger)
        {
            _protocolEngine = protocolEngine;
            _visionService = visionService;
            _logger = logger;
        }


        public void InitializeSession(string stationId, SessionWorkType workType, object parameters = null)
        {
            // 清理旧会话
            if (_sessions.TryRemove(stationId, out var oldState))
                oldState.Dispose();

            var state = new SessionState
            {
                WorkType = workType,
                Cts = new CancellationTokenSource(),
                ImageQueue = new BlockingCollection<HObject>()
            };


            // 根据类型初始化内部数据结构
            switch (workType)
            {
                case SessionWorkType.Inspect:

                    int inspectStNum = Convert.ToInt32(_protocolEngine.GetVariable("backup2").Substring(0, 2)); //获取检测极柱开始序号
                    int inspectEdNum = Convert.ToInt32(_protocolEngine.GetVariable("backup2").Substring(2, 2)); //获取检测极柱结束序号
                    int numForInspect = inspectEdNum - inspectStNum + 1;                                        //获取需要检测的极柱总个数
                    state.resultData = "";
                    var inspectResult = new string[numForInspect];
                    int[] inspectOrder = new int[numForInspect];


                    int msgPoleCapacity = Convert.ToInt32(_protocolEngine.GetVariable("version")) == 1 ? 10 : 25;//版本号为1：10；为2：25
                    int orderIndex = int.Parse(_protocolEngine.GetVariable("inspectType"));
                    //inspectOrders是什么？
                    //不理解电芯类型减1是什么鬼东西，索引默认取0？
                    //这里是空值，还没赋值，后续本地读取
                    //InspectOrder order = new ParamsSide().inspectOrders[orderIndex - 1];
                    InspectOrder order = new InspectOrder() { row = 2, col = 13, start = [1, 26], end = [25, 2] };

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
                    state.resultData = string.Concat(Enumerable.Repeat("01" + new string('0', 48), msgPoleCapacity));
                    break;
                case SessionWorkType.Calibrate:
                case SessionWorkType.Verify:
                    state.CalibResult = "97";//测试用
                    state.CalibData = "+8989333+1212666";
                    break;
            }

            _sessions[stationId] = state;
            //启动后台任务
            state.ProcessTask = Task.Run(() => ProcessLoop(stationId, state, state.Cts.Token));
            _logger.Information("会话启动: {StationId}, 类型: {WorkType}", stationId, workType);
        }

        public void EnqueueImage(string stationId, HObject image)
        {
            if (_sessions.TryGetValue(stationId, out var state) && state.IsActive)
            {
                state.ImageQueue.Add(image.Clone());
            }
            else
            {
                _logger.Warning("No active session for {StationId}, discarding image", stationId);
                image?.Dispose();
            }
        }

        public string GetResultData(string stationId, int? startIndex = null, int? endIndex = null)
        {
            if (!_sessions.TryGetValue(stationId, out var state) || !state.IsActive)
            {
                _logger.Warning("No active session for {StationId} when querying results", stationId);
                return GenerateErrorResult(state?.WorkType ?? SessionWorkType.Inspect);
            }

            return state.WorkType switch
            {
                SessionWorkType.Inspect => state.resultData,
                SessionWorkType.Calibrate => state.CalibResult + state.CalibData,
                SessionWorkType.Verify => state.CalibResult + state.CalibData,
                _ => string.Empty
            };
        }

        public void Reset(string stationId)
        {
            if (_sessions.TryRemove(stationId, out var state))
            {
                state.Dispose();
                _logger.Information("Session on {StationId} reset", stationId);
            }
        }

        /// <summary>
        /// 图像处理
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="state"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ProcessLoop(string stationId, SessionState state, CancellationToken token)
        {
            try
            {
                foreach (var img in state.ImageQueue.GetConsumingEnumerable(token))
                {
                    try
                    {
                        switch (state.WorkType)
                        {
                            case SessionWorkType.Inspect:
                                await ProcessInspectImage(stationId, state, img);
                                break;
                            case SessionWorkType.Calibrate:
                                await ProcessCalibrationImage(state, img, false);
                                state.ImageQueue.CompleteAdding(); // 标定只需一张图
                                break;
                            case SessionWorkType.Verify:
                                await ProcessCalibrationImage(state, img, true);
                                state.ImageQueue.CompleteAdding();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error processing image on {StationId}", stationId);
                    }
                    finally
                    {
                        img.Dispose();
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _logger.Information("Processing loop ended for {StationId}", stationId);
            }
        }

        private async Task ProcessInspectImage(string stationId, SessionState state, HObject image)
        {
            string result;
            result = await _visionService.Execute2DInspectAsync(image, 10, new InspectionParams());
            //结果汇总时用','连接
            state.CalibResult = result.Split(',')[0];
            state.CalibData = result.Split(',')[1];
        }

        private async Task ProcessCalibrationImage(SessionState state, HObject image, bool isVerification)
        {
            string result;
            if (isVerification)
                result = await _visionService.ExecuteVerificationAsync(image, new CalibrationParams());
            else
                result = await _visionService.ExecuteCalibrationAsync(image, new CalibrationParams());
            //结果汇总时用','连接
            state.CalibResult = result.Split(',')[0];
            state.CalibData = result.Split(',')[1];
        }

        private string GenerateErrorResult(SessionWorkType type)
        {
            if (type == SessionWorkType.Inspect)
                return string.Join("", Enumerable.Repeat("02" + new string('0', 48), 25)); // 默认容量
            else
                return "02+0000000+0000000";
        }
    }

    // 内部状态类
    internal class SessionState
    {
        public SessionWorkType WorkType { get; set; }
        public CancellationTokenSource Cts { get; set; }
        public Task ProcessTask { get; set; }
        public BlockingCollection<HObject> ImageQueue { get; set; }
        public bool IsActive => Cts != null && !Cts.IsCancellationRequested;

        // —— 检测相关 ——
        public int[] InspectOrder { get; set; }          // 极柱拍照顺序
        public string[] PoleResults { get; set; }        // 每个极柱的结果字符串
        public int ProcessedCount { get; set; }           // 已处理的极柱数量
        public int MsgPoleCapacity { get; set; }          // 单次报文容量
        public string resultData { get; set; }          // 结果数据字符串

        // —— 标定/点检相关 ——
        public string CalibResult { get; set; } = "00";  // 结果 01 ok/02 ng
        public string CalibData { get; set; } = "+0000000+0000000"; // X+Y 坐标

        public void Dispose()
        {
            Cts?.Cancel();
            ImageQueue?.CompleteAdding();
            ProcessTask?.Wait(TimeSpan.FromSeconds(3));
            Cts?.Dispose();
            ImageQueue?.Dispose();
        }
    }
}

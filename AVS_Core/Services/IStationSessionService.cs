using AVS_Core.Models;
using AVS_Service;
using HalconDotNet;
using Prism.Ioc;
using Serilog;
using System;
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
        /// 获取标定\点检的结果数据（用于PLC回复）
        /// </summary>
        string GetResultData(string stationId, int? startIndex = null, int? endIndex = null);

        /// <summary>
        /// 获取检测的结果数据
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="startPole"></param>
        /// <param name="endPole"></param>
        /// <param name="msgPoleCapacity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<string> GetResultDataAsync(string stationId, int startPole, int endPole, int msgPoleCapacity, CancellationToken ct = default);

        /// <summary>
        /// 重置指定工位的会话
        /// </summary>
        void Reset(string stationId);
    }

    public class StationSessionService : IStationSessionService
    {
        private readonly IContainerProvider _containerProvider;
        //private readonly IProtocolEngineService _protocolEngine;
        //private readonly IVisionService _visionService;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SessionState> _sessions = new();

        public StationSessionService(
            IProtocolEngineService protocolEngine, 
            //IVisionService visionService,
            IContainerProvider containerProvider,
            ILogger logger)
        {
            _containerProvider = containerProvider;
            //_protocolEngine = protocolEngine;
            //_visionService = visionService;
            _logger = logger;
        }


        public void InitializeSession(string stationId, SessionWorkType workType, object parameters = null)
        {
            // 清理旧会话
            if (_sessions.TryRemove(stationId, out var oldState))
                oldState.Dispose();

            // 为这个工位新建视觉服务实例

            var visionService = _containerProvider.Resolve<IVisionService>();
            visionService.InitializeAsync(stationId).GetAwaiter().GetResult();

            var state = new SessionState
            {
                WorkType = workType,
                visionService = visionService,
                Cts = new CancellationTokenSource(),
                ImageQueue = new BlockingCollection<HObject>(),
                MsgPoleCapacity = 0,
                ReceivedCount = 0,

            };
            // 根据类型初始化内部数据结构
            switch (workType)
            {
                case SessionWorkType.Inspect:

                    var p = (InspectionInitParams)parameters;
                    int maxPole = p.PoleOrder.Max();

                    state.PoleOrder = p.PoleOrder;
                    state.MsgPoleCapacity = p.MsgPoleCapacity;
                    state.ProcessIndex = 0;
                    state.PoleResults = new string[maxPole + 1];
                    state.ResultSources = new TaskCompletionSource<string>[maxPole + 1];
                    for (int i = 0; i <= maxPole; i++)
                        state.ResultSources[i] = new TaskCompletionSource<string>();
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
                if (state.WorkType == SessionWorkType.Inspect)
                {
                    if (state.ReceivedCount >= state.PoleOrder.Length)
                    {
                        _logger.Warning("接收图像数超出预期，工位{StationId}，已接收{Received}，预期{Total}",
                            stationId, state.ReceivedCount, state.PoleOrder.Length);
                        image.Dispose();
                        return;
                    }
                    state.ReceivedCount++;
                }
                state.ImageQueue.Add(image.Clone());
            }
            else
            {
                _logger.Warning("没有激活的{StationId}对话", stationId);
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
            return state.CalibResult + state.CalibData;
        }

        public async Task<string> GetResultDataAsync(string stationId, int startPole, int endPole, int msgPoleCapacity, CancellationToken ct = default)
        {
            if (!_sessions.TryGetValue(stationId, out var state) || state.WorkType != SessionWorkType.Inspect)
            {
                _logger.Warning("无效会话或非检测模式，工位 {StationId}", stationId);
                return GenerateEmptyResult(msgPoleCapacity);
            }

            int requestedCount = endPole - startPole + 1;
            if (requestedCount > msgPoleCapacity)
            {
                _logger.Warning("请求极柱数 {Requested} 超过单次容量 {Capacity}", requestedCount, msgPoleCapacity);
                requestedCount = msgPoleCapacity;
            }

            var results = new List<string>();
            for (int pole = startPole; pole <= endPole; pole++)
            {
                if (pole < 0 || pole >= state.ResultSources.Length)
                {
                    results.Add("00" + new string('0', 48));
                    continue;
                }

                var tcs = state.ResultSources[pole];
                // 等待结果或超时（5秒）
                var timeoutTask = Task.Delay(5000, ct);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);
                if (completedTask == tcs.Task)
                {
                    results.Add(await tcs.Task);
                }
                else
                {
                    _logger.Warning("极柱 {Pole} 检测超时", pole);
                    results.Add("00" + new string('0', 48));
                }
            }

            while (results.Count < msgPoleCapacity)
            {
                results.Add("00" + new string('0', 48));
            }

            return string.Concat(results);
        }

        private string GenerateEmptyResult(int capacity)
        {
            return string.Concat(Enumerable.Repeat("00" + new string('0', 48), capacity));
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
                                await ProcessInspectImage(state, img);
                                break;
                            case SessionWorkType.Calibrate:
                                await ProcessCalibrationImage(state, img, false);
                                state.ImageQueue.CompleteAdding();
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

        /// <summary>
        /// 检测图片
        /// </summary>
        /// <param name="state"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        private async Task ProcessInspectImage(SessionState state, HObject image)
        {
            string result = string.Empty;
            int idx = state.ProcessIndex;
            if (idx >= state.PoleOrder.Length)
            {
                _logger.Error("处理序号超出极柱总数");
                return;
            }
            int poleNum = state.PoleOrder[idx];
            state.ProcessIndex++;
            result = await state.visionService.Execute2DInspectAsync(image, poleNum, new InspectionParams());
            state.PoleResults[poleNum] = result;
            state.ResultSources[poleNum].TrySetResult(result);
        }

        /// <summary>
        /// 标定图像
        /// </summary>
        /// <param name="state"></param>
        /// <param name="image"></param>
        /// <param name="isVerification"></param>
        /// <returns></returns>
        private async Task ProcessCalibrationImage(SessionState state, HObject image, bool isVerification)
        {
            string result;
            if (isVerification)
                result = await state.visionService.ExecuteVerificationAsync(image, new CalibrationParams());
            else
                result = await state.visionService.ExecuteCalibrationAsync(image, new CalibrationParams());
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
        public IVisionService visionService { get; set; }
        public SessionWorkType WorkType { get; set; }
        public CancellationTokenSource Cts { get; set; }
        public Task ProcessTask { get; set; }
        public BlockingCollection<HObject> ImageQueue { get; set; }
        public bool IsActive => Cts != null && !Cts.IsCancellationRequested;

        //检测相关
        public int[] PoleOrder { get; set; }        // 极柱拍照顺序（物理编号数组）
        public string[] PoleResults { get; set; }   // 按物理编号存储每个极柱的结果字符串 "01+0001234+0005678..."
        public string resultData { get; set; }      // 结果数据字符串
        public int ReceivedCount { get; set; }      // 已入队图像数量（用于校验）
        /// <summary>
        /// 当前处理的极柱在 PoleOrder中的索引
        /// </summary>
        public int ProcessIndex { get; set; }
        //为每个物理编号提供一个 TaskCompletionSource，用于异步等待该极柱的结果
        public TaskCompletionSource<string>[] ResultSources { get; set; } // 索引 = 物理编号；
        public int MsgPoleCapacity { get; set; }    // 单次报文最大极柱数（10 或 25）


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

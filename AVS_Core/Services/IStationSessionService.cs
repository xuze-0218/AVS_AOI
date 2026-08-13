using AVS_Core.Models;
using AVS_Service;
using AVS_Service.Models;
using HalconDotNet;
using Prism.Ioc;
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
        Task InitializeSession(string stationId, SessionWorkType workType, object parameters = null);

        /// <summary>
        /// 相机采集到图像后，将其放入对应工位的处理队列
        /// </summary>
        void EnqueueImage(string stationId, HObject image);

        /// <summary>
        /// 获取标定\点检的结果数据（用于PLC回复）
        /// </summary>
        string GetResultData(string stationId);

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
        /// <summary>
        /// 预加载参数避免首轮检测/标定时的延迟
        /// </summary>
        /// <returns></returns>
        Task PreloadAllStationsAsync();
    }

    public class StationSessionService : IStationSessionService
    {
        private readonly IContainerProvider _container;
        private readonly IAiDriveService _aiDriveService;
        private readonly IParametersConfigService _paramService;
        private readonly IStationConfigService _stationConfigService;
        private readonly ILogger _logger;
        private readonly object _preloadLock = new object();
        private Task _preloadTask; // 后台预加载任务


        private readonly ConcurrentDictionary<string, SessionState> _sessions = new();
        /// <summary>
        /// 视觉服务缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, IVisionProvider> _providers = new();
        public StationSessionService(IContainerProvider containerProvider, IStationConfigService stationConfigService,
            IParametersConfigService paramService, ILogger logger, IAiDriveService aiDriveService)
        {
            _paramService = paramService;
            _container = containerProvider;
            _stationConfigService = stationConfigService;
            _logger = logger;
            _aiDriveService = aiDriveService;
        }

        public async Task InitializeSession(string stationId, SessionWorkType workType, object parameters = null)
        {
            //await EnsurePreloadCompletedAsync();
            // 清理旧会话
            if (_sessions.TryRemove(stationId, out var oldState))
                oldState.Dispose();
            if (!_providers.TryGetValue(stationId, out var provider))
            {
                var stationCfg = _stationConfigService.GetStation(stationId);
                provider = stationCfg.Dimension switch
                {
                    VisionDimension.TwoD => _container.Resolve<I2DVisionProvider>(),
                    VisionDimension.ThreeD => _container.Resolve<I3DVisionProvider>(),
                    _ => throw new NotSupportedException()
                };
                await provider.InitializeAsync(stationCfg);
                _providers.TryAdd(stationId, provider);
            }

            var state = new SessionState
            {
                WorkType = workType,
                Provider = provider,  //复用缓存
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
                    state.CalibResult = "00";
                    state.CalibData = "+0000000+0000000";
                    break;
            }
            _sessions[stationId] = state;
            //启动后台任务
            state.ProcessTask = Task.Run(() => ProcessLoop(stationId, state, state.Cts.Token));
            _logger.Information("会话启动: {StationId}, 类型: {WorkType}", stationId, workType);
        }

        public void EnqueueImage(string stationId, HObject image)
        {
            if (image == null) return;
            try
            {
                if (_sessions.TryGetValue(stationId, out var state) && state.IsActive)
                {
                    if (state.WorkType == SessionWorkType.Inspect)
                    {
                        if (state.ReceivedCount >= state.PoleOrder.Length)
                        {
                            _logger.Warning("接收图像数超出预期，工位{StationId}，已接收{Received}，预期{Total}",
                                stationId, state.ReceivedCount, state.PoleOrder.Length);
                            return;
                        }
                        state.ReceivedCount++;
                    }
                    state.ImageQueue.Add(image.Clone());
                }
                else
                    _logger.Warning("没有激活的{StationId}对话", stationId);
            }
            finally
            {
                image.Dispose();
            }
        }

        public void Reset(string stationId)
        {
            if (_sessions.TryRemove(stationId, out var state))
            {
                state.Dispose();
                _logger.Information("Session on {StationId} reset", stationId);
            }
        }

        public string GetResultData(string stationId)
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

        private string GenerateEmptyResult(int capacity) => string.Concat(Enumerable.Repeat("00" + new string('0', 48), capacity));

        public async Task PreloadAllStationsAsync()
        {
            if (_preloadTask != null) return;
            lock (_preloadLock)
            {
                if (_preloadTask != null) return;

                _preloadTask = Task.Run(async () =>
                {
                    var tasks = _stationConfigService.Stations.Select(async station =>
                    {
                        //初始化视觉服务并缓存，加载halcon引擎参数等
                        if (!_providers.ContainsKey(station.StationId))
                        {
                            IVisionProvider provider = station.Dimension switch
                            {
                                VisionDimension.TwoD => _container.Resolve<I2DVisionProvider>(),
                                VisionDimension.ThreeD => _container.Resolve<I3DVisionProvider>(),
                                _ => throw new NotSupportedException()
                            };
                            await provider.InitializeAsync(station);
                            _providers.TryAdd(station.StationId, provider);
                            _logger.Information("预加载工位 {Id} 视觉完成", station.StationId);
                        }
                        //加载AI模型
                        string moduleName = station.StationId;
                        var p = _paramService.GetStationParams(moduleName);
                        if (p.IsAiCheck)
                        {
                            // 确定 AI 模型键：优先使用 AiModelStationId，否则用 StationId
                            string aiKey = string.IsNullOrEmpty(station.AiModelStationId) ? station.StationId : station.AiModelStationId;
                            // 如果该键的模型尚未加载，则加载
                            if (!_aiDriveService.IsModelLoaded(aiKey))
                            {
                                // 模型路径从参数配置中读取（也可以硬编码或从 station 配置中获取）
                                string detModelPath = _paramService.GetString(moduleName, "DetModelPath", "");
                                string segModelPathsStr = _paramService.GetString(moduleName, "SegModelPaths", "");
                                if (!string.IsNullOrEmpty(detModelPath))
                                    _aiDriveService.LoadDetModel(aiKey, new[] { detModelPath });
                                if (!string.IsNullOrEmpty(segModelPathsStr))
                                {
                                    var segPaths = segModelPathsStr.Split(';');
                                    _aiDriveService.LoadSegModel(aiKey, segPaths);
                                }
                            }
                            _logger.Information("工位 {StationId} AI 模型加载完成（键: {AiKey}）", station.StationId, aiKey);

                        }
                    });
                    await Task.WhenAll(tasks);
                    _logger.Information("所有工位视觉服务预加载完成");
                });
            }
            await _preloadTask;
        }

        /// <summary>
        /// 图像处理
        /// </summary>
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
            int idx = state.ProcessIndex;     //当前拍照序号
            if (idx >= state.PoleOrder.Length)
            {
                _logger.Error("处理序号超出极柱总数");
                return;
            }
            int poleNum = state.PoleOrder[idx];// 映射为物理极柱号
            state.ProcessIndex++;
            switch (state.Provider)
            {
                case I2DVisionProvider p2D:
                    result = await p2D.ExecuteInspectAsync(image, poleNum, new InspectionParams());
                    break;
                case I3DVisionProvider p3D:
                    result = await p3D.ExecuteInspectAsync(image, poleNum, new InspectionParams());
                    break;
                default:
                    throw new InvalidOperationException($"未知的视觉提供者类型: {state.Provider.GetType()}");
            }
            //result = await state.visionService.Execute2DInspectAsync(image, poleNum, new InspectionParams());
            state.PoleResults[poleNum] = result;  // 按物理编号存储
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
            switch (state.Provider)
            {
                case I2DVisionProvider p2D:
                    result = isVerification
                        ? await p2D.ExecuteVerificationAsync(image, new CalibrationParams())
                        : await p2D.ExecuteCalibrationAsync(image, new CalibrationParams());
                    break;
                case I3DVisionProvider p3D:
                    result = await p3D.ExecuteCalibrationAsync(image, new CalibrationParams());
                    break;
                default:
                    throw new InvalidOperationException($"未知类型: {state.Provider.GetType()}");
            }
            if (string.IsNullOrEmpty(result))
            {
                _logger.Error("标定结果为空");
                state.CalibResult = "02";
                state.CalibData = "+0000000+0000000";
                return;
            }

            var parts = result.Split(',');
            if (parts.Length >= 2)
            {
                state.CalibResult = parts[0];
                state.CalibData = parts[1];
            }
            else
            {
                _logger.Error("标定结果格式无效: {Result}", result);
                state.CalibResult = "02";
                state.CalibData = "+0000000+0000000";
            }
        }

        private string GenerateErrorResult(SessionWorkType type)
        {
            if (type == SessionWorkType.Inspect)
                return string.Join("", Enumerable.Repeat("02" + new string('0', 48), 25)); // 默认容量
            else
                return "02+0000000+0000000";
        }
    }

    // 会话状态
    /// <summary>
    /// 记录一次检测任务（一批产品）的临时数据，例如极柱拍照顺序PoleOrder、结果数组PoleResults、等待句柄ResultSources等。
    /// 每批产品都会创建新的会话状态，任务结束后销毁。
    /// </summary>
    internal class SessionState
    {
        public IVisionProvider Provider { get; set; }
        public SessionWorkType WorkType { get; set; }
        public CancellationTokenSource Cts { get; set; }
        public Task ProcessTask { get; set; }
        public BlockingCollection<HObject> ImageQueue { get; set; }
        public bool IsActive => Cts != null && !Cts.IsCancellationRequested;

        //检测相关
        public int[] PoleOrder { get; set; }        // 极柱拍照顺序（物理编号）假设4行13列共52个极柱，拍照顺序可能是 [1~13 26~14 27~39 52~40],索引0-51
        /// <summary>
        ///按物理编号存储每个极柱的结果字符串 "01+0001234+0005678..." 索引 = 物理极柱号，PoleResults[1]存储1号极柱结果
        /// </summary>
        public string[] PoleResults { get; set; }
        public int ReceivedCount { get; set; }      // 已入队图像数量,防越界，超过 PoleOrder.Length 则丢弃
        /// <summary>
        /// 当前处理的极柱在PoleOrder中的索引,初始0，每处理一张图像自增1
        /// 取值顺序PoleOrder[ProcessIndex]得到本次极柱号
        /// </summary>
        public int ProcessIndex { get; set; }
        /// <summary>
        /// 为每个物理编号提供一个 TaskCompletionSource，用于异步等待该极柱的结果
        /// </summary>
        public TaskCompletionSource<string>[] ResultSources { get; set; } // 索引 = 物理编号；
        public int MsgPoleCapacity { get; set; }    // 单次报文最大极柱数（10 或 25）


        /// <summary>
        /// —— 标定/点检相关 ——
        /// </summary>
        public string CalibResult { get; set; } = "00";  // 结果 01 ok/02 ng
        public string CalibData { get; set; } = "+0000000+0000000"; // X+Y 坐标

        public void Dispose()
        {
            Cts?.Cancel();
            ImageQueue?.CompleteAdding();
            ProcessTask?.Wait(TimeSpan.FromSeconds(3));
            while (ImageQueue?.TryTake(out var img) == true)
            {
                img?.Dispose();
            }
            Cts?.Dispose();
            ImageQueue?.Dispose();
        }
    }
}

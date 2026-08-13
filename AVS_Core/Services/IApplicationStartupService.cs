using AVS_Common.Events;
using AVS_Service;
using Prism.Events;
using Serilog;

namespace AVS_Core.Services
{
    public interface IApplicationStartupService
    {
        Task InitializeAsync();
        Task ShutdownAsync();
    }

    public class ApplicationStartupService : IApplicationStartupService
    {
        /// <summary>
        /// 记录后台初始化任务，确保在应用关闭时可以等待其完成或安全取消
        /// </summary>
        private SubscriptionToken _imageEventToken;
        private Task _backgroundInitializationTask;
        private readonly IEventAggregator _eventAggregator;
        private readonly IStationSessionService _sessionService;
        private readonly IPlcMessageRouter _messageRouter;
        private readonly ICommunicationService _communicationService;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly IStationConfigService _stationConfigService;
        private readonly ILogger _logger;

        public ApplicationStartupService(
            ILogger logger,
            IPlcMessageRouter messageRouter,
            IEventAggregator eventAggregator,
            IStationSessionService sessionService,
            ICommunicationService communicationService,
            IStationConfigService stationConfigService,
            //IParametersConfigService parametersConfigService,
            ICameraConfigService cameraConfigService)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _sessionService = sessionService;
            _messageRouter = messageRouter;
            _stationConfigService = stationConfigService;
            //_parametersConfigService = parametersConfigService;
            _communicationService = communicationService;
            _cameraConfigService = cameraConfigService;
        }

        public async Task InitializeAsync()
        {
            _logger.Information("开始应用初始化");

            try
            {
                //加载通讯服务
                await InitializeCommunicationAsync();
                ///避免单例冲突和重复初始化
                //foreach (var item in _stationConfigService.Stations)
                //{
                //    await _visionService.InitializeAsync(item.StationId);
                //}
                _imageEventToken = _eventAggregator.GetEvent<HImageDisplayEvent>()
                    .Subscribe(OnImageCaptured, ThreadOption.BackgroundThread, false);
                _backgroundInitializationTask = Task.Run(async () =>
                {
                    try
                    {
                        await _cameraConfigService.InitializeAllCameras();
                        _logger.Information("后台初始化完成");
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex, "后台初始化失败");
                    }
                });
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "应用初始化失败");
                throw;
            }

        }

        private void OnImageCaptured(CameraImagePayload payload)
        {
            try
            {
                var camSetting = _cameraConfigService.AllSettings
                    .FirstOrDefault(c => c.SerilalNum == payload.CameraSN);

                if (camSetting == null)
                {
                    _logger.Warning("未配置的相机SN: {SN}，图像丢弃", payload.CameraSN);
                    return;
                }

                var station = _stationConfigService.GetStationByCameraRole(camSetting.CameraRole);
                if (station == null || string.IsNullOrEmpty(station.StationId))
                {
                    _logger.Warning("未找到相机角色 {Role} 对应的工位", camSetting.CameraRole);
                    return;
                }

                _sessionService.EnqueueImage(station.StationId, payload.Image);
            }
            finally
            {
                payload.Image?.Dispose();  // 无论是否匹配成功，都释放
            }
        }

        private async Task InitializeCommunicationAsync()
        {
            _logger.Debug("初始化通讯服务");

            try
            {

                foreach (var station in _stationConfigService.Stations)
                {
                    _communicationService.Start(station.StationId, station.Protocol, station.Role, station.IP, station.Port);
                }

                _communicationService.MessageReceived += async (connectionPlcId, message) =>
                {
                    _logger.Information("接收来自 {Sender} 的消息: {Message}", connectionPlcId, message);
                    try
                    {
                        await _messageRouter.HandleMessageAsync(connectionPlcId, message);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "处理消息失败");
                    }
                };

                _logger.Information("通讯服务已启动，等待连接...");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "初始化通讯服务失败");
                _logger.Warning("通讯服务启动失败，但应用将继续运行");
            }
        }


        public async Task ShutdownAsync()
        {
            try
            {
                if (_imageEventToken != null)
                {
                    _eventAggregator.GetEvent<HImageDisplayEvent>().Unsubscribe(_imageEventToken);
                    _imageEventToken = null;
                }
                if (_backgroundInitializationTask != null && !_backgroundInitializationTask.IsCompleted)
                {
                    _logger.Debug("等待后台初始化任务完成...");
                    var timeoutTask = Task.Delay(5000);
                    var completedTask = await Task.WhenAny(_backgroundInitializationTask, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        _logger.Warning("后台初始化任务超时未完成，强制继续关闭流程");
                    }
                }
                _logger.Debug("开始释放所有相机资源...");
                var connectedSNs = _cameraConfigService.ConnectedCameras.Keys.ToList();
                foreach (var sn in connectedSNs)
                {
                    _cameraConfigService.DisconnectCamera(sn);
                    _logger.Information($"相机 {sn} 已断开连接");
                }

                _logger.Debug("停止通讯服务");
                foreach (var station in _stationConfigService.Stations)
                {
                    _communicationService?.Stop(station.StationId);
                }

                await Task.Delay(300);

                //确保Halcon非托管内存被回收
                GC.Collect();
                GC.WaitForPendingFinalizers();
                _logger.Information("应用关闭流程完成");

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "应用关闭过程中出错");
            }
        }
    }
}

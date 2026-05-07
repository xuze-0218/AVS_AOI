#define IsDebug       

using AVS_Common;
using AVS_Service;
using Newtonsoft.Json.Linq;
using Prism.Events;
using Serilog;
using System.ComponentModel;


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
        private Task _backgroundInitializationTask;
        private readonly IEventAggregator _eventAggregator;
        //private readonly IWorkflowService _workflowService;
        private readonly IStationSessionService _sessionService;
        private readonly IPlcMessageRouter _messageRouter;
        private readonly IVisionService _visionService;
        private readonly ICommunicationService _communicationService;
        private readonly IParametersConfigService _parametersConfigService;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly IStationConfigService _stationConfigService;
        private readonly ILogger _logger;

        public ApplicationStartupService(
            IPlcMessageRouter messageRouter,
           ILogger logger,
           IEventAggregator eventAggregator,
            IStationSessionService sessionService,
            IVisionService visionService,
           ICommunicationService communicationService,
           IStationConfigService stationConfigService,
           //IWorkflowService workflowService,
           IParametersConfigService parametersConfigService,
           ICameraConfigService cameraConfigService)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            //_workflowService = workflowService;
            _sessionService = sessionService;
            _visionService = visionService;
            _messageRouter = messageRouter;
            _stationConfigService = stationConfigService;
            _parametersConfigService = parametersConfigService;
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

#if !IsDebug
                await _visionService.InitializeAsync("A");
                await _visionService.InitializeAsync("B");

#endif

                _eventAggregator.GetEvent<HImageDisplayEvent>().Subscribe(OnImageCaptured);
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
            // 根据相机逻辑角色确定工位ID
            var camSetting = _cameraConfigService.AllSettings
                .FirstOrDefault(c => c.SerilalNum == payload.CameraSN);

            string stationId = camSetting?.CameraRole switch
            {
                "cam2d" => "A",
                "cam3d" => "B",
                _ => null
            };

            if (stationId != null)
            {
                _sessionService.EnqueueImage(stationId, payload.Image);
            }
            else
            {
                _logger.Warning("Unknown camera role for SN {SN}, image discarded", payload.CameraSN);
            }
        }

        private async Task InitializeCommunicationAsync()
        {
            _logger.Debug("初始化通讯服务");

            try
            {

                //foreach (var station in _stationConfigService.Stations)
                //{
                //    _communicationService.Start(station.StationId, station.Protocol, station.Role, station.IP, station.Port);
                //}

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
                    //_communicationService.Start(station.StationId, station.Protocol, station.Role, station.IP, station.Port);
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

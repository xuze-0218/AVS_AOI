using AVS_Common.Events;
using AVS_Service;
using Prism.Events;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
                _imageEventToken = _eventAggregator.GetEvent<HImageDisplayEvent>()
                    .Subscribe(OnImageCaptured, ThreadOption.PublisherThread, false);
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
                _logger.Information("收到相机图像事件: SN={SN}", payload.CameraSN);
                var camSetting = _cameraConfigService.AllSettings.FirstOrDefault(c => c.SerilalNum == payload.CameraSN);
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
                //var imageForQueue = payload.Image?.Clone();
                //if (imageForQueue != null)
                //{
                //    _sessionService.EnqueueImage(station.StationId, imageForQueue);
                //    imageForQueue.Dispose(); // EnqueueImage 内部会再次克隆，这个临时克隆可以释放
                //}
                _sessionService.EnqueueImage(station.StationId, payload.Image);
            }
            finally
            {
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
                // 取消事件订阅
                if (_imageEventToken != null)
                {
                    _logger.Debug("取消图像事件订阅");
                    _eventAggregator.GetEvent<HImageDisplayEvent>().Unsubscribe(_imageEventToken);
                    _imageEventToken = null;
                    _logger.Debug("图像事件订阅已取消");
                }

                // 等待后台初始化任务
                if (_backgroundInitializationTask != null && !_backgroundInitializationTask.IsCompleted)
                {
                    _logger.Debug("等待后台初始化任务完成...");
                    var timeoutTask = Task.Delay(5000);
                    var completedTask = await Task.WhenAny(_backgroundInitializationTask, timeoutTask).ConfigureAwait(false);
                    if (completedTask == timeoutTask)
                    {
                        _logger.Warning("后台初始化任务超时未完成，强制继续关闭流程");
                    }
                    else
                    {
                        _logger.Debug("后台初始化任务已完成");
                    }
                }

                // 释放所有相机
                _logger.Debug("开始释放所有相机资源...");
                var connectedSNs = _cameraConfigService.ConnectedCameras.Keys.ToList();
                _logger.Information("准备断开 {Count} 台相机: {SNs}", connectedSNs.Count, string.Join(", ", connectedSNs));
                foreach (var sn in connectedSNs)
                {
                    _logger.Information("开始断开相机 {SN} ...", sn);
                    try
                    {
                        _cameraConfigService.DisconnectCamera(sn);
                        _logger.Information("相机 {SN} 断开完成", sn);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "断开相机 {SN} 异常", sn);
                    }
                }
                _logger.Information("所有相机断开流程结束");

                // 停止通讯服务
                _logger.Debug("停止通讯服务");
                foreach (var station in _stationConfigService.Stations)
                {
                    _logger.Debug("停止工位 {StationId} 通讯", station.StationId);
                    try
                    {
                        _communicationService?.Stop(station.StationId);
                        _logger.Debug("工位 {StationId} 通讯已停止", station.StationId);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "停止工位 {StationId} 通讯异常", station.StationId);
                    }
                }
                _logger.Information("所有通讯服务已停止");

                // 延迟和GC
                await Task.Delay(300).ConfigureAwait(false);

                // 确保Halcon非托管内存被回收
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

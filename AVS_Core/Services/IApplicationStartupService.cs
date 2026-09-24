using AVS_Common.Events;
using AVS_Common.Services;
using AVS_Service;
using AVS_Service.Services;
using Prism.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using AVS_Service.Models;

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
        private readonly HashSet<string> _startedStations = new HashSet<string>();
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);
        private SubscriptionToken _stationChangedToken;
        private SubscriptionToken _aiChangedToken;
        private SubscriptionToken _imageEventToken;
        private Task _backgroundInitializationTask;
        /// <summary>
        /// PLC消息订阅
        /// </summary>
        private bool _messageHandlerSubscribed;

        private readonly IEventAggregator _eventAggregator;
        private readonly IAiDriveService _aiDriveService;
        private readonly IStationSessionService _sessionService;
        private readonly IWindowHandleRegistry _windowHandleRegistry;
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
            IAiDriveService aiDriveService,
            ICommunicationService communicationService,
            IStationConfigService stationConfigService,
            IWindowHandleRegistry windowHandleRegistry,
            ICameraConfigService cameraConfigService)
        {
            _logger = logger;
            _aiDriveService = aiDriveService;
            _eventAggregator = eventAggregator;
            _sessionService = sessionService;
            _messageRouter = messageRouter;
            _stationConfigService = stationConfigService;
            _windowHandleRegistry = windowHandleRegistry;
            _communicationService = communicationService;
            _cameraConfigService = cameraConfigService;
        }

        public async Task InitializeAsync()
        {
            _logger.Information("开始应用初始化");

            try
            {
                if (!_messageHandlerSubscribed)
                {
                    _communicationService.MessageReceived += OnMessageReceived;
                    _messageHandlerSubscribed = true;
                }
                //图像事件
                if (_imageEventToken == null)
                    _imageEventToken = _eventAggregator.GetEvent<HImageDisplayEvent>()
                        .Subscribe(OnImageCaptured, ThreadOption.PublisherThread, false);
                // 工位配置变更事件
                if (_stationChangedToken == null)
                    _stationChangedToken = _eventAggregator.GetEvent<StationConfigChangedEvent>()
                        .Subscribe(OnStationConfigChanged, ThreadOption.UIThread, false);
                // 模型配置变更事件
                if (_aiChangedToken == null)
                    _aiChangedToken = _eventAggregator.GetEvent<AiModelConfigChangedEvent>()
                        .Subscribe(OnAiModelConfigChanged, ThreadOption.UIThread, false);

                var rolesSnapshot = _stationConfigService.Stations.Select(s => s.CameraRole).Where(r => !string.IsNullOrEmpty(r)).Distinct().ToList();
                var stationsSnapshot = _stationConfigService.Stations.ToList();
                await Task.Run(async () =>
                {
                    await WaitForCameraHandlesAsync(rolesSnapshot).ConfigureAwait(false);
                    await StartStationsAsync(stationsSnapshot).ConfigureAwait(false);
                }).ConfigureAwait(false);
                StartCameraInitialization();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (_backgroundInitializationTask != null)
                            await _backgroundInitializationTask.ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "等待相机初始化完成失败");
                    }

                    int camCount = _cameraConfigService.ConnectedCameras.Count;
                    bool isReady = camCount > 0;
                    _logger.Information("应用初始化全部完成，已连接相机 {Count} 台，isReady={Ready}",
                        camCount, isReady);

                    var dispatcher = System.Windows.Application.Current?.Dispatcher;
                    if (dispatcher != null)
                    {
                        await dispatcher.InvokeAsync(() =>
                        {
                            _eventAggregator.GetEvent<ApplicationStartupCompletedEvent>().Publish(isReady);
                        }).Task;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "应用初始化失败");
                throw;
            }

        }

        private async void OnMessageReceived(string connectionPlcId, string message)
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
        }

        private void OnStationConfigChanged()
        {
            // 当前在UI线程（ThreadOption.UIThread 回调），先做快照
            var rolesSnapshot = _stationConfigService.Stations
                .Select(s => s.CameraRole)
                .Where(r => !string.IsNullOrEmpty(r))
                .Distinct()
                .ToList();

            var stationsSnapshot = _stationConfigService.Stations.ToList();

            _ = Task.Run(async () =>
            {
                try
                {
                    await StartStationsAsync(stationsSnapshot).ConfigureAwait(false);
                    StartCameraInitialization();
                    await WaitForCameraHandlesAsync(rolesSnapshot).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "工位配置变更后重建服务失败");
                }
            });
        }

        private void OnAiModelConfigChanged(string section)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(section))
                    {
                        var station = _stationConfigService.Stations.FirstOrDefault(s => s.StationId == section);
                        string aiKey = station == null
                            ? section
                            : (string.IsNullOrEmpty(station.AiModelStationId)
                                ? station.StationId
                                : station.AiModelStationId);
                        _aiDriveService.UnloadStation(aiKey);
                        _logger.Information("工位 {Section} 的 AI 模型已卸载，准备重新加载", section);
                    }
                    else
                    {
                        _logger.Warning("AiModelConfigChanged 未携带 Section，执行全量 Preload");
                    }
                    await _sessionService.PreloadAllStationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "AI 模型重载失败");
                }
            });
        }
        private async Task StartStationsAsync(List<StationConfig> stationsSnapshot)
        {
            await _initLock.WaitAsync().ConfigureAwait(false);
            try
            {
                var currentIds = stationsSnapshot.Select(s => s.StationId).ToHashSet();

                var removed = _startedStations.Where(id => !currentIds.Contains(id)).ToList();
                foreach (var id in removed)
                {
                    try { _communicationService.Stop(id); }
                    catch (Exception ex) { _logger.Error(ex, "停止工位 {Id} 通讯失败", id); }
                    _startedStations.Remove(id);
                    _logger.Information("工位 {Id} 已从运行中移除", id);
                }

                var newStations = new List<string>();
                foreach (var station in stationsSnapshot)
                {
                    if (_startedStations.Contains(station.StationId)) continue;
                    _communicationService.Start(
                        station.StationId, station.Protocol, station.Role, station.IP, station.Port);
                    _startedStations.Add(station.StationId);
                    newStations.Add(station.StationId);
                }
                if (newStations.Count > 0)
                    _logger.Information("已启动工位通讯: {Stations}", string.Join(", ", newStations));

                await _sessionService.PreloadAllStationsAsync().ConfigureAwait(false);
            }
            finally { _initLock.Release(); }
        }

        private void StartCameraInitialization()
        {
            // 若已有初始化任务在跑，不重复
            if (_backgroundInitializationTask != null && !_backgroundInitializationTask.IsCompleted)
                return;

            _backgroundInitializationTask = Task.Run(async () =>
            {
                try { await _cameraConfigService.InitializeAllCameras(); }
                catch (Exception ex) { _logger.Error(ex, "相机初始化失败"); }
            });
        }

        private async Task WaitForCameraHandlesAsync(List<string> roles)
        {
            foreach (var role in roles)
            {
                var start = DateTime.Now;
                while (_windowHandleRegistry.GetHandle(role) == null && DateTime.Now - start < TimeSpan.FromSeconds(5))
                {
                    await Task.Delay(50).ConfigureAwait(false);
                }
                if (_windowHandleRegistry.GetHandle(role) == null)
                    _logger.Warning("相机角色 {Role} 窗口句柄等待超时", role);
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
                // 发布者发布后即 Dispose 原始 HObject，必须同步 Clone 后再入队
                var clonedImage = payload.Image?.Clone();
                if (clonedImage != null && clonedImage.IsInitialized())
                {
                    _sessionService.EnqueueImage(station.StationId, clonedImage);
                }
                else
                {
                    clonedImage?.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "处理图像事件异常");
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                //取消消息订阅
                if (_messageHandlerSubscribed)
                {
                    _communicationService.MessageReceived -= OnMessageReceived;
                    _messageHandlerSubscribed = false;
                }

                //取消图像订阅
                if (_imageEventToken != null)
                {
                    _logger.Debug("取消图像事件订阅");
                    _eventAggregator.GetEvent<HImageDisplayEvent>().Unsubscribe(_imageEventToken);
                    _imageEventToken = null;
                    _logger.Debug("图像事件订阅已取消");
                }

                if (_stationChangedToken != null)
                {
                    _eventAggregator.GetEvent<StationConfigChangedEvent>().Unsubscribe(_stationChangedToken);
                    _stationChangedToken = null;
                }
                if (_aiChangedToken != null)
                {
                    _eventAggregator.GetEvent<AiModelConfigChangedEvent>().Unsubscribe(_aiChangedToken);
                    _aiChangedToken = null;
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
                // 清理所有会话队列，释放残留图像
                foreach (var station in _stationConfigService.Stations)
                {
                    try
                    {
                        _sessionService.Reset(station.StationId);
                        _logger.Information("工位 {StationId} 会话已重置", station.StationId);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "重置工位 {StationId} 会话失败", station.StationId);
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

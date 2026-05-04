using AVS_Service;
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
        private Task _backgroundInitializationTask;
        private readonly IWorkflowService _workflowService;
        private readonly ICommunicationService _communicationService;
        private readonly IParametersConfigService _parametersConfigService;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly ILogger _logger;

        public ApplicationStartupService(
           ILogger logger,
           ICommunicationService communicationService,
           IWorkflowService workflowService,
           IParametersConfigService parametersConfigService,
           ICameraConfigService cameraConfigService)
        {
            _logger = logger;
            _workflowService = workflowService;
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

        private async Task InitializeCommunicationAsync()
        {
            _logger.Debug("初始化通讯服务");

            try
            {
                _communicationService.Start();
                _communicationService.MessageReceived += async (sender, message) =>
                {
                    _logger.Information("接收来自 {Sender} 的消息: {Message}", sender, message);
                    try
                    {
                        _cameraConfigService.AllSettings.ForEach(async cam =>
                        {

                            _logger.Debug($"触发相机{cam.SerilalNum}拍照");
                            //_cameraConfigService.ExecuteSoftTrigger(cam.SerilalNum);//调用业务处理流程,2d和3d相机可能不是同时触发，要更改为在业务流程中根据配置触发对应相机
                            await _workflowService.ProcessPlcTriggerAsync(message);

                        });
                        await Task.Delay(100);

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
                _communicationService?.Stop();
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

using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service
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
        private readonly ICommunicationService _communicationService;
        private readonly IParametersConfigService _parametersConfigService;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly ILogger _logger;

        public ApplicationStartupService(
           ILogger logger,
           ICommunicationService communicationService,
           IParametersConfigService parametersConfigService,
           ICameraConfigService cameraConfigService)
        {
            _logger = logger;
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
                    _logger.Debug("接收来自 {Sender} 的消息: {Message}", sender, message);
                    try
                    {
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
                    try
                    {
                        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                        {
                            await Task.WhenAll(
                                _backgroundInitializationTask,
                                Task.Delay(Timeout.Infinite, cts.Token)
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "等待后台初始化任务失败");
                    }
                }

                //停止通讯
                _logger.Debug("停止通讯服务");
                _communicationService?.Stop();
                await Task.Delay(300);

                _logger.Information("应用关闭流程完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "应用关闭过程中出错");
            }
        }
    }
}

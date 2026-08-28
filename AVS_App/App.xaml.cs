using AVS_App.Views;
using AVS_Common;
using AVS_Common.Events;
using AVS_Common.Services;
using AVS_Core.Models;
using AVS_Core.Services;
using AVS_Modules_Settings.ViewModels;
using AVS_Modules_Settings.Views;
using AVS_Service;
using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Serilog;
using Serilog.Core;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AVS_App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private static Mutex _singleInstanceMutex;
        private const string MutexName = "aoi_common_mutex";
        public static UserRole CurrentUserRole { get; private set; } = UserRole.Operator;
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell(Window shell)
        {
           
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //注册导航
            containerRegistry.RegisterForNavigation<InspectionView>();
            containerRegistry.RegisterForNavigation<StationConfigView>();
            containerRegistry.RegisterForNavigation<ParameterConfigView>();
            containerRegistry.RegisterForNavigation<TempAndCaliDebugView>();
            containerRegistry.RegisterForNavigation<ProtocolConfigView>();
            containerRegistry.RegisterForNavigation<CameraDebugView>();
            containerRegistry.RegisterForNavigation<PlcDebugView>();
            containerRegistry.RegisterForNavigation<MetrologyView>();
            containerRegistry.RegisterForNavigation<DebugCenterView>();



            containerRegistry.RegisterSingleton<ICameraConfigService, CameraConfigService>();
            containerRegistry.RegisterSingleton<IWindowHandleRegistry, WindowHandleRegistry>();
            containerRegistry.RegisterSingleton<IAiDriveService, AiDriveService>();
            containerRegistry.Register<I2DVisionProvider, TwoDVisionProvider>();
            containerRegistry.Register<I3DVisionProvider, ThreeDVisionProvider>();
            containerRegistry.RegisterSingleton<IHalconEngineProvider, HalconEngineProvider>();
            containerRegistry.RegisterSingleton<IStationSessionService, StationSessionService>();
            containerRegistry.RegisterSingleton<IPlcMessageRouter, PlcMessageRouter>();
            containerRegistry.RegisterSingleton<IParametersConfigService, ParametersConfigService>();
            containerRegistry.RegisterSingleton<IProtocolConfigRepository, ProtocolConfigRepository>();
            containerRegistry.RegisterSingleton<IProtocolEngineService, ProtocolEngineService>();
            containerRegistry.RegisterSingleton<ICommunicationService, CommunicationService>();
            containerRegistry.RegisterSingleton<IStationConfigService, StationConfigService>();
            containerRegistry.RegisterSingleton<ITemplateMatchingService, TemplateMatchingService>();
            containerRegistry.RegisterSingleton<ILocalTestService, LocalTestService>();
            //containerRegistry.RegisterSingleton<ICaliperService, CaliperService>();
            containerRegistry.RegisterSingleton<IMetrologyService, MetrologyService>();
            containerRegistry.RegisterSingleton<IApplicationStartupService, ApplicationStartupService>();

            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext()
                    .WriteTo.Async(a => a.File("Logs/log_.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 30)).WriteTo.Sink(new UiLogSink()).CreateLogger();

            containerRegistry.RegisterInstance<ILogger>(Log.Logger);
        }

        protected override async void OnInitialized()
        {
            try
            {
                base.OnInitialized();
                //注册窗口句柄事件
                WindowHandleEvent.HandleRegistered += (rn, handle) => Container.Resolve<IWindowHandleRegistry>().Register(rn, handle);
                WindowHandleEvent.HandleUnregistered += (rn) => Container.Resolve<IWindowHandleRegistry>().Unregister(rn);
                //导航到InspectionView
                var regionManager = Container.Resolve<IRegionManager>();
                regionManager.RequestNavigate("MainContentRegion", "InspectionView");
                // 等待 InspectionView 完全加载并注册所有窗口句柄
                await WaitForCameraHandlesAsync();
                //预加载所有工位的视觉服务
                var stationSessionService = Container.Resolve<IStationSessionService>();
                await stationSessionService.PreloadAllStationsAsync();  // 等待预加载完成

                var startupService = Container.Resolve<IApplicationStartupService>();
                await startupService.InitializeAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "应用启动失败");
                MessageBox.Show("应用初始化失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        /// <summary>
        /// 等待所有相机的窗口句柄注册完成,因为视觉服务初始化需要窗口句柄
        /// </summary>
        /// <returns></returns>
        private async Task WaitForCameraHandlesAsync()
        {
            var registry = Container.Resolve<IWindowHandleRegistry>();
            var cameraRoles = Container.Resolve<IStationConfigService>().Stations.Select(s => s.CameraRole).Distinct().ToList();
            var timeout = TimeSpan.FromSeconds(5);
            var start = DateTime.Now;
            foreach (var role in cameraRoles)
            {
                while (registry.GetHandle(role) == null && DateTime.Now - start < timeout)
                {
                    await Task.Delay(50);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            try
            {
                var startupService = Container.Resolve<IApplicationStartupService>();
                //同步等待关闭相机
                startupService.ShutdownAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "应用关闭时保存配置失败");
            }
            if (_singleInstanceMutex != null)
            {
                try
                {
                    _singleInstanceMutex.ReleaseMutex();
                    _singleInstanceMutex.Dispose();
                    _singleInstanceMutex = null;
                }
                catch { }
            }

            Log.CloseAndFlush();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isNewInstance = false;
            _singleInstanceMutex = new Mutex(true, MutexName, out isNewInstance);

            if (!isNewInstance)
            {
                MessageBox.Show("应用已在运行，无法启动新实例！",
                    "警告",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);
            var shell = this.MainWindow;
            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();

            if (!loginWindow.LoginSuccess)
            {
                Shutdown();
                return;
            }
            CurrentUserRole = loginWindow.SelectedRole;
            shell.Show();

        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is NullReferenceException &&
                e.Exception.Source == "halcondotnet")
            {
                e.Handled = true;
                Environment.Exit(0);
            }
        }
    }
}

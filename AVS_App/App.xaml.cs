using AVS_App.Views;
using AVS_Common;
using AVS_Service;
using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Serilog;
using System.Configuration;
using System.Data;
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
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<InspectionView>();
            containerRegistry.RegisterSingleton<ICameraConfigService, CameraConfigService>();
            containerRegistry.RegisterSingleton<IParametersConfigService, ParametersConfigService>();
            containerRegistry.RegisterSingleton<ICommunicationService, CommunicationService>();
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
            base.OnInitialized();
            try
            {
                var startupService = Container.Resolve<IApplicationStartupService>();
                await startupService.InitializeAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "应用启动失败");
                MessageBox.Show("应用初始化失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate("MainContentRegion", "InspectionView");
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = @".\Modules" };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            try
            {
                var startupService = Container.Resolve<IApplicationStartupService>();
                startupService.ShutdownAsync().Wait();
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

            base.OnStartup(e);
        }
    }



}

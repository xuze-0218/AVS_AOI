using AVS_Common;
using AVS_VisionService;
using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
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
            //相机配置
            containerRegistry.RegisterSingleton<ICameraConfigService, CameraConfigService>();

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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
           
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

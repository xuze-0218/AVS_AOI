using AVS_App.Views;
using AVS_Common;
using AVS_Common.Events;
using AVS_Common.Model;
using AVS_Service;
using AVS_Service.Events;
using AVS_Service.Models;
using AVS_Service.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AVS_App.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ILogger _logger;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICommunicationService _communicationService;
        private readonly IStationConfigService _stationConfigService;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly ILoginCredentialService _loginCredentialService;


        private bool _isEngineer;
        public bool IsEngineer
        {
            get => _isEngineer;
            private set => SetProperty(ref _isEngineer, value);
        }

        private string _currentUser = "未登录";
        public string CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        private bool _isAnyCameraGrabbing;
        public bool IsAnyCameraGrabbing
        {
            get => _isAnyCameraGrabbing;
            set
            {
                if (SetProperty(ref _isAnyCameraGrabbing, value))
                {
                    RefreshCommandStatus();
                }
            }
        }

        private bool _isPlcConnected;
        public bool IsPlcConnected
        {
            get => _isPlcConnected;
            set => SetProperty(ref _isPlcConnected, value);
        }

        private bool _isCameraConnected;
        public bool IsCameraConnected
        {
            get => _isCameraConnected;
            set
            {
                if (SetProperty(ref _isCameraConnected, value))
                {
                    RefreshCommandStatus();
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    RefreshCommandStatus();
                }
            }
        }

        private bool _isAutoRunning;
        public bool IsAutoRunning
        {
            get => _isAutoRunning;
            set
            {
                if (SetProperty(ref _isAutoRunning, value))
                {
                    RefreshCommandStatus();
                }
            }
        }

        private int _totalCount;
        private int _okCount;
        private int _ngCount;
        private double _yieldRate;

        public int TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }
        public int OkCount { get => _okCount; set => SetProperty(ref _okCount, value); }
        public int NgCount { get => _ngCount; set => SetProperty(ref _ngCount, value); }
        public double YieldRate { get => _yieldRate; set => SetProperty(ref _yieldRate, value); }

        public DelegateCommand<string> NavigateCommand { get; set; }
        public DelegateCommand StartAllCommand { get; set; }
        public DelegateCommand StopAllCommand { get; set; }
        public DelegateCommand LoginCommand { get; set; }
        public ObservableCollection<LogEventModel> LogSource => UiLogSink.LogCollection;
        public MainWindowViewModel(
            ILogger logger,
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            ICameraConfigService cameraConfigService,
            ICommunicationService communicationService,
            IStationConfigService stationConfigService,
            ILoginCredentialService loginCredentialService)
        {
            _logger = logger;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _cameraConfigService = cameraConfigService;
            _communicationService = communicationService;
            _stationConfigService = stationConfigService;
            _loginCredentialService = loginCredentialService;
            _eventAggregator.GetEvent<LoginSuccessEvent>().Subscribe(OnLoginSuccess);
            NavigateCommand = new DelegateCommand<string>(Navigate);
            StartAllCommand = new DelegateCommand(async () => await ExecuteStartAllAsync(), CanStartAll);
            StopAllCommand = new DelegateCommand(async () => await ExecuteStopAllAsync(), CanStopAll);
            LoginCommand = new DelegateCommand(ShowLoginWindow);
            RefreshPlcStatus();
            _communicationService.ConnectionStatusChanged += (id, connected) =>
            {
                Application.Current?.Dispatcher.Invoke(RefreshPlcStatus);
            };

            RefreshCameraStatus();
            _cameraConfigService.CameraStatusChanged += (sn, connected) =>
            {
                Application.Current?.Dispatcher.Invoke(RefreshCameraStatus);
            };
            _cameraConfigService.CameraGrabbingStatusChanged += (sn, isGrabbing) =>
            {
                Application.Current?.Dispatcher.Invoke(RefreshCameraGrabbingStatus);
            };
            eventAggregator.GetEvent<PoleResultEvent>().Subscribe(OnPoleResult, ThreadOption.UIThread);
            _eventAggregator.GetEvent<ApplicationStartupCompletedEvent>().Subscribe(OnStartupCompleted, ThreadOption.UIThread);
        }

        private void OnPoleResult(PoleResultPayload payload)
        {
            TotalCount++;
            if (payload.IsOK) OkCount++; else NgCount++;
            YieldRate = TotalCount == 0 ? 0 : Math.Round((double)OkCount / TotalCount * 100, 1);
        }

        /// <summary>
        /// 应用初始化完成后自动启动采集
        /// </summary>
        private async void OnStartupCompleted(bool isReady)
        {
            try
            {
                if (!isReady)
                {
                    MessageBox.Show(
                        "应用初始化完成，但未检测到已连接的相机。\n请检查相机连接后手动点击「启动」。",
                        "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (IsAutoRunning) return;  // 已经在跑，不重复

                //UI渲染等待时间（相机状态刚变更，命令状态需要刷新）
                await Task.Delay(800);

                // 主动刷新一次状态
                RefreshCameraStatus();
                RefreshCameraGrabbingStatus();
                RefreshCommandStatus();

                if (!CanStartAll())
                {
                    _logger.Warning($"CanStartAll=false (IsCameraConnected={IsCameraConnected}, IsBusy={IsBusy})");
                    return;
                }

                await ExecuteStartAllAsync();
                _logger.Information("自动启动完成");
            }
            catch (Exception ex)
            {
                _logger.Error($"[AutoStart] 自动启动异常: {ex.Message}");
            }
        }
        private void Navigate(string navigatePath)
        {
            if (!string.IsNullOrEmpty(navigatePath))
            {
                _regionManager.RequestNavigate("MainContentRegion", navigatePath);
            }
        }
        private void RefreshCameraGrabbingStatus()
        {
            IsAnyCameraGrabbing = _cameraConfigService.ConnectedCameras.Keys
                .Any(sn => _cameraConfigService.IsCameraGrabbing(sn));
        }
        private void RefreshPlcStatus()
        {
            var stations = _stationConfigService.Stations;
            IsPlcConnected = stations.Any() &&
                             stations.All(s => _communicationService.IsActive(s.StationId));
        }
        private void RefreshCameraStatus()
        {
            // 修改为：至少有一台相机连接即可认为相机可用
            IsCameraConnected = _cameraConfigService.ConnectedCameras.Count > 0;

            // 如果没有任何相机连接，且之前是自动运行状态，则强制退出自动运行
            if (!IsCameraConnected && IsAutoRunning)
            {
                IsAutoRunning = false;
            }
        }
        // 启动按钮：只要有相机连接且不忙碌即可点击
        private bool CanStartAll() => IsCameraConnected && !IsBusy;
        // 停止按钮：只要有相机在采集且不忙碌即可点击
        private bool CanStopAll() => IsAnyCameraGrabbing && !IsBusy;
        private void RefreshCommandStatus()
        {
            StartAllCommand?.RaiseCanExecuteChanged();
            StopAllCommand?.RaiseCanExecuteChanged();
        }
        private async Task ExecuteStartAllAsync()
        {
            if (!CanStartAll()) return;
            IsBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    // 如果已有相机在采集（例如调试界面启动了预览），先全部停止
                    if (_cameraConfigService.ConnectedCameras.Keys.Any(sn => _cameraConfigService.IsCameraGrabbing(sn)))
                    {
                        _cameraConfigService.StopAllCameras();
                    }
                    // 启动所有相机（应用配置、设置触发模式、启动采集）
                    _cameraConfigService.StartAllCameras();
                });

                IsAutoRunning = true;
            }
            catch (Exception ex)
            {
                IsAutoRunning = false;
            }
            finally
            {
                IsBusy = false;
                RefreshCameraGrabbingStatus();
            }
        }
        private async Task ExecuteStopAllAsync()
        {
            if (!CanStopAll()) return;
            IsBusy = true;
            try
            {
                await Task.Run(() => _cameraConfigService.StopAllCameras());
                IsAutoRunning = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            finally
            {
                IsBusy = false;
                RefreshCameraGrabbingStatus();
            }
        }
        private void OnLoginSuccess(LoginSuccessInfo info)
        {
            CurrentUser = info.CurrentUser;
            IsEngineer = info.IsEngineer;
        }
        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            var loginViewModel = new LoginWindowViewModel(_loginCredentialService, _eventAggregator);
            loginWindow.DataContext = loginViewModel;
            loginViewModel.CloseAction = () => loginWindow.Close();
            loginWindow.ShowDialog();

            if (loginViewModel.LoginSuccess)
            {
                MessageBox.Show($"登录成功: {loginViewModel.LoginParams.UserName}, 角色: {loginViewModel.SelectedRole}");
                string roleText = loginViewModel.SelectedRole == UserRole.Operator ? "操作员" : "工程师";
                CurrentUser = $"{loginViewModel.LoginParams.UserName}（{roleText}）";
                IsEngineer = loginViewModel.SelectedRole == UserRole.Engineer;
            }
            else
            {
                MessageBox.Show("登录失败或取消");
            }
        }
    }
}
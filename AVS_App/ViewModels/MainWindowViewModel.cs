using AVS_Common;
using AVS_Common.Model;
using AVS_Service;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AVS_App.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ICommunicationService _communicationService;
        private readonly IStationConfigService _stationConfigService;
        private readonly ICameraConfigService _cameraConfigService;

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

        public DelegateCommand<string> NavigateCommand { get; set; }
        public DelegateCommand StartAllCommand { get; set; }
        public DelegateCommand StopAllCommand { get; set; }

        public ObservableCollection<LogEventModel> LogSource => UiLogSink.LogCollection;

        public MainWindowViewModel(IRegionManager regionManager,
            ICommunicationService communicationService,
            IStationConfigService stationConfigService,
            ICameraConfigService cameraConfigService)
        {
            _regionManager = regionManager;
            _cameraConfigService = cameraConfigService;
            _communicationService = communicationService;
            _stationConfigService = stationConfigService;

            NavigateCommand = new DelegateCommand<string>(Navigate);
            StartAllCommand = new DelegateCommand(async () => await ExecuteStartAllAsync(), CanStartAll);
            StopAllCommand = new DelegateCommand(async () => await ExecuteStopAllAsync(), CanStopAll);

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
                // 建议记录日志或提示
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
                // 建议记录日志
            }
            finally
            {
                IsBusy = false;
                RefreshCameraGrabbingStatus();
            }
        }
    }
}
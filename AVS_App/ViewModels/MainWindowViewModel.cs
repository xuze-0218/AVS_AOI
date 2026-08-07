using AVS_Common;
using AVS_Common.Model;
using AVS_Service;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
            set => SetProperty(ref _isCameraConnected, value);
        }


        public DelegateCommand<string> NavigateCommand { get; set; }
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
            RefreshPlcStatus();
            _communicationService.ConnectionStatusChanged += (id, connected) =>
            {
                Application.Current?.Dispatcher.Invoke(() => RefreshPlcStatus());
            };

            // === 相机状态 ===
            RefreshCameraStatus();
            _cameraConfigService.CameraStatusChanged += (sn, connected) =>
            {
                Application.Current?.Dispatcher.Invoke(() => RefreshCameraStatus());
            };
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string navigatePath)
        {
            if (!string.IsNullOrEmpty(navigatePath))
            {
                _regionManager.RequestNavigate("MainContentRegion", navigatePath);
            }
        }

        private void RefreshPlcStatus()
        {
            var stations = _stationConfigService.Stations;
            IsPlcConnected = stations.Any() &&
                             stations.All(s => _communicationService.IsActive(s.StationId));
        }

        private void RefreshCameraStatus()
        {
            var configuredSns = _cameraConfigService.AllSettings
                .Where(x => !string.IsNullOrEmpty(x.SerilalNum))
                .Select(x => x.SerilalNum)
                .ToList();

            IsCameraConnected = configuredSns.Any() &&
                                configuredSns.All(sn =>
                                    _cameraConfigService.ConnectedCameras.ContainsKey(sn));
        }
    }
}

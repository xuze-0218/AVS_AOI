using AVS_Common;
using AVS_Drivers.Camera;
using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.Camera.Common.Model;
using AVS_Service;
using AVS_Service.Models;
using HalconDotNet;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace AVS_Modules_Settings.ViewModels
{
    public class CameraDebugViewModel : BindableBase
    {
        private ICamera _camera;
        private ILogger _logger;
        private ICameraConfigService _cameraConfigService;
        private CameraSettingModel _currentConfig;
        private IEventAggregator _eventAggregator;
        private bool _isBorrowedCamera = false;

        #region 状态控制属性
        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set { SetProperty(ref _isConnected, value); RaisePropertyChanged(nameof(IsNotConnected)); RaisePropertyChanged(nameof(CanStartGrab)); RaisePropertyChanged(nameof(CanSoftTrigger)); }
        }
        public bool IsNotConnected => !IsConnected;

        private bool _isGrabbing = false;
        public bool IsGrabbing
        {
            get => _isGrabbing;
            set { SetProperty(ref _isGrabbing, value); RaisePropertyChanged(nameof(CanStartGrab)); RaisePropertyChanged(nameof(CanSoftTrigger)); }
        }

        public bool CanStartGrab => IsConnected && !IsGrabbing;
        public bool CanSoftTrigger => IsConnected && IsTriggerMode && (!IsGrabbing || SelectedTriggerSource == TriggerSource.Software);
        #endregion

        #region 绑定的属性
        private List<string> _deviceList;
        public List<string> DeviceList { get => _deviceList; set => SetProperty(ref _deviceList, value); }

        private string _selectedDevice;
        public string SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (SetProperty(ref _selectedDevice, value))
                {
                    int idx = DeviceList.IndexOf(value);
                    _currentConfig = _cameraConfigService.GetCameraSettingBySnOrIndex(null, idx);
                    SaveImagePath = _currentConfig.imgpath;
                    SyncConfigToUI();
                }
            }
        }

        private string _ip;
        public string IP { get => _ip; set => SetProperty(ref _ip, value); }

        private int _port;
        public int Port { get => _port; set => SetProperty(ref _port, value); }

        private bool _isContinuousMode = true;
        public bool IsContinuousMode { get => _isContinuousMode; set { if (SetProperty(ref _isContinuousMode, value) && value) IsTriggerMode = false; } }

        private bool _isTriggerMode;
        public bool IsTriggerMode { get => _isTriggerMode; set { if (SetProperty(ref _isTriggerMode, value) && value) IsContinuousMode = false; RaisePropertyChanged(nameof(CanSoftTrigger)); } }

        public List<TriggerSource> TriggerSources { get; }
        private TriggerSource _selectedTriggerSource;
        public TriggerSource SelectedTriggerSource { get => _selectedTriggerSource; set => SetProperty(ref _selectedTriggerSource, value); }

        private short _exposureTime;
        public short ExposureTime { get => _exposureTime; set => SetProperty(ref _exposureTime, value); }

        private short _gain;
        public short Gain { get => _gain; set => SetProperty(ref _gain, value); }

        public List<CameraBrand> CameraBrands { get; set; }
        private CameraBrand _selectedBrand;
        public CameraBrand SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (SetProperty(ref _selectedBrand, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.CameraType = (int)value;
                        _cameraConfigService.UpdateCameraSetting(_currentConfig);
                    }
                }
            }
        }

        private string _saveImagePath;
        public string SaveImagePath { get => _saveImagePath; set => SetProperty(ref _saveImagePath, value); }

        private string _statusMessage = "请先查找设备...";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        #endregion


        public CameraDebugViewModel(IEventAggregator eventAggregator, ICameraConfigService cameraConfigService, ILogger logger)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _cameraConfigService = cameraConfigService;

            SearchCommand = new DelegateCommand(ExecuteSearch).ObservesCanExecute(() => IsNotConnected);
            InitCommand = new DelegateCommand(ExecuteInit).ObservesCanExecute(() => IsNotConnected);
            CloseCommand = new DelegateCommand(ExecuteClose).ObservesCanExecute(() => IsConnected);
            StartGrabCommand = new DelegateCommand(ExecuteStartGrab).ObservesCanExecute(() => CanStartGrab);
            StopGrabCommand = new DelegateCommand(ExecuteStopGrab).ObservesCanExecute(() => IsGrabbing);
            SoftTriggerCommand = new DelegateCommand(ExecuteSoftTrigger).ObservesCanExecute(() => CanSoftTrigger);
            GetParamCommand = new DelegateCommand(ExecuteGetParam).ObservesCanExecute(() => IsConnected);
            SetParamCommand = new DelegateCommand(ExecuteSetParam).ObservesCanExecute(() => IsConnected);
            SaveImageCommand = new DelegateCommand<string>(ExecuteSaveImage).ObservesCanExecute(() => IsConnected);

            TriggerSources = new List<TriggerSource>((TriggerSource[])Enum.GetValues(typeof(TriggerSource)));
            SelectedTriggerSource = TriggerSource.Software;
            CameraBrands = Enum.GetValues(typeof(CameraBrand)).Cast<CameraBrand>().ToList();

            _currentConfig = _cameraConfigService.GetCameraSettingBySnOrIndex(null, 0);
            SyncConfigToUI();

            // 订阅底层图像事件用于显示
            _cameraConfigService.OnImageCaptured += OnImageCaptured;
        }


        /// <summary>
        /// 窗体显示
        /// </summary>
        /// <param name="camSN"></param>
        /// <param name="cogImg"></param>
        private void OnImageCaptured(string camSN, HObject img)
        {
            if (camSN == SelectedDevice || _currentConfig?.SerilalNum == camSN)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {

                    _eventAggregator.GetEvent<HImageDisplayEvent>().Publish(new CameraImagePayload()
                    {
                        CameraSN = camSN,
                        Image = img
                    });
                });
            }
        }

        #region Commands
        public DelegateCommand SearchCommand { get; }
        public DelegateCommand InitCommand { get; }
        public DelegateCommand CloseCommand { get; }
        public DelegateCommand StartGrabCommand { get; }
        public DelegateCommand StopGrabCommand { get; }
        public DelegateCommand SoftTriggerCommand { get; }
        public DelegateCommand GetParamCommand { get; }
        public DelegateCommand SetParamCommand { get; }
        public DelegateCommand<string> SaveImageCommand { get; }
        #endregion


        #region 执行逻辑
        private void ExecuteSearch()
        {
            try
            {
                DeviceList = CamFactory.GetDeviceEnum(SelectedBrand);
                StatusMessage = (DeviceList != null && DeviceList.Count > 0) ? $"[{SelectedBrand}] 查找成功" : $"未发现 [{SelectedBrand}] 设备";
                if (DeviceList != null && DeviceList.Count > 0) SelectedDevice = DeviceList[0];
            }
            catch (Exception ex) { StatusMessage = $"搜索异常: {ex.Message}"; }
        }

        private void ExecuteInit()
        {
            _logger.Information("初始化相机");
            if (string.IsNullOrEmpty(SelectedDevice)) return;
            _camera = _cameraConfigService.GetCameraInstance(SelectedDevice);
            _isBorrowedCamera = (_camera != null);
            if (!_isBorrowedCamera)
            {
                bool isSuccess = _cameraConfigService.ConnectAndStartCamera(SelectedDevice, (int)SelectedBrand);
                if (!isSuccess)
                {
                    StatusMessage = "连接失败";
                    return;
                }
                _camera = _cameraConfigService.GetCameraInstance(SelectedDevice);
            }

            IsConnected = true;
            _camera.GetCamConfig(out CamConfig hwConfig);
            string realSn = hwConfig?.SerilalNum;
            if (!string.IsNullOrEmpty(realSn))
            {
                _currentConfig = _cameraConfigService.GetCameraSettingBySnOrIndex(realSn, DeviceList.IndexOf(SelectedDevice));
                _currentConfig.SerilalNum = realSn;
            }
            StatusMessage = $"相机已连接: {realSn}";
            ExecuteGetParam();
        }

        private void ExecuteClose()
        {
            if (_camera != null)
            {
                ExecuteStopGrab(); // 确保退出时恢复模式
                if (!_isBorrowedCamera)
                {
                    _cameraConfigService.DisconnectCamera(SelectedDevice);
                }
                IsConnected = false;
                _camera = null;
                StatusMessage = "调试连接已断开";
            }
        }

        private void ExecuteStartGrab()
        {
            // 委托服务切换模式
            var mode = IsContinuousMode ? AcquisitionMode.Continuous : AcquisitionMode.SoftTrigger;
            _cameraConfigService.SetCameraAcquisitionMode(SelectedDevice, mode);
            IsGrabbing = true;
            StatusMessage = IsContinuousMode ? "连续采图中..." : "等待触发中...";
        }

        private void ExecuteStopGrab()
        {
            if (_camera == null || !IsGrabbing) return;
            // 退出抓图时，强制恢复为主程序需要的软触发状态
            _cameraConfigService.SetCameraAcquisitionMode(SelectedDevice, AcquisitionMode.SoftTrigger);
            IsGrabbing = false;
            StatusMessage = "采集已停止，已恢复软触发";
        }

        private void ExecuteSoftTrigger()
        {
            if (!IsGrabbing) return;
            if (IsConnected && _camera != null) _camera.SoftTrigger();
        }

        private void ExecuteGetParam()
        {
            if (_camera != null && IsConnected)
            {
                _camera.GetCamConfig(out CamConfig config);
                if (config != null) { ExposureTime = (short)config.ExpouseTime; Gain = config.Gain; StatusMessage = "参数读取成功"; }
            }
        }

        private void ExecuteSetParam()
        {
            if (_camera != null && IsConnected)
            {
                _camera.SetExpouseTime((ushort)ExposureTime);
                _camera.SetGain(Gain);
                if (_currentConfig != null)
                {
                    _currentConfig.ExposureTime = ExposureTime;
                    _currentConfig.Gain = Gain;
                    _currentConfig.imgpath = SaveImagePath;
                    _currentConfig.CameraType = (int)SelectedBrand;
                    _currentConfig.SerilalNum = SelectedDevice;
                    _currentConfig.IP = IP;
                    _currentConfig.Port = Port;
                    _cameraConfigService.UpdateCameraSetting(_currentConfig);
                }
                StatusMessage = "参数已保存到本地";
            }
        }

        private void ExecuteSaveImage(string format)
        {
            // 注意：此功能由于未在调试界面截取 Bitmap，需根据实际情况处理，此处保留原框架逻辑
            StatusMessage = "调试模式暂不支持直接保存，请通过主程序保存";
        }

        private void SyncConfigToUI()
        {
            if (_currentConfig == null) return;
            SelectedBrand = (CameraBrand)_currentConfig.CameraType;
            SaveImagePath = _currentConfig.imgpath;
            if (ushort.TryParse(_currentConfig.ExposureTime.ToString(), out ushort exp)) ExposureTime = (short)exp;
            if (short.TryParse(_currentConfig.Gain.ToString(), out short gn)) Gain = gn;
            IP = _currentConfig.IP;
            Port = _currentConfig.Port;
        }

        #endregion

    }
}

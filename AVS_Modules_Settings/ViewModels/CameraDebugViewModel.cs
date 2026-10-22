using AVS_Common.Events;
using AVS_Drivers.Camera;
using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.Camera.Common.Model;
using AVS_Service.Models;
using AVS_Service.Services;
using HalconDotNet;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AVS_Modules_Settings.ViewModels
{
    public class CameraDebugViewModel : BindableBase, INavigationAware, IDisposable
    {
        private ICamera _camera;
        private ILogger _logger;
        private CameraSettingModel _currentConfig;
        private IEventAggregator _eventAggregator;
        private ICameraConfigService _cameraConfigService;
        private readonly IStationConfigService _stationConfigService;
        private readonly IParametersConfigService _paramService;
        private bool _isBorrowedCamera = false;
        private bool _isActiveView = false; // 标记当前页面是否处于激活显示状态
        /// <summary>
        /// token for subscribing to image display events. Used to unsubscribe when the view is deactivated.
        /// </summary>
        private SubscriptionToken _imageSubToken;
        private bool _disposed = false; // 防止重复释放
        private bool _isSubscribed = false;

        #region 状态控制属性
        public bool IsCurrentCameraGrabbing => !string.IsNullOrEmpty(SelectedDevice) && _cameraConfigService.IsCameraGrabbing(SelectedDevice);
        public string PreviewButtonText => IsCurrentCameraGrabbing ? "停止采集" : "开始采集";
        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                SetProperty(ref _isConnected, value);
                RaisePropertyChanged(nameof(IsNotConnected));
                RaisePropertyChanged(nameof(CanSoftTrigger));
                TogglePreviewCommand?.RaiseCanExecuteChanged();
            }
        }
        public bool IsNotConnected => !IsConnected;
        public bool CanSoftTrigger => IsConnected && IsTriggerMode && IsCurrentCameraGrabbing;

        public bool HasDebugImage => _currentDebugImage != null && _currentDebugImage.IsInitialized();
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
                    if (string.IsNullOrEmpty(value))
                    {
                        _currentConfig = null;
                        _camera = null;
                        IsConnected = false;
                        StatusMessage = "请搜索并选择设备";
                        return;
                    }

                    int idx = DeviceList.IndexOf(value);
                    _currentConfig = _cameraConfigService.GetCameraSettingBySnOrIndex(value, idx);
                    if (_currentConfig == null)
                    {
                        //临时新建,点击"设置参数"才保存）
                        _currentConfig = new CameraSettingModel
                        {
                            SerilalNum = value,
                            CamSelectIndex = idx
                        };
                    }
                    _currentConfig.CameraType = (int)SelectedBrand;
                    SyncConfigToUI();

                    bool isActuallyConnected = _cameraConfigService.ConnectedCameras != null &&
                                               _cameraConfigService.ConnectedCameras.ContainsKey(value);

                    if (isActuallyConnected)
                    {
                        _camera = _cameraConfigService.GetCameraInstance(value);
                        _isBorrowedCamera = true;
                        IsConnected = true;
                        StatusMessage = $"已连接(后台驻留): {value}";
                        ExecuteGetParam();
                    }
                    else
                    {
                        _camera = null;
                        _isBorrowedCamera = false;
                        IsConnected = false;
                        StatusMessage = "未连接，请点击初始化";
                    }
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

        private string _cameraRoleName = "cam1";
        public string CameraRoleName
        {
            get => _cameraRoleName;
            set => SetProperty(ref _cameraRoleName, value);
        }

        public List<CameraBrand> CameraBrands { get; set; }
        //private CameraBrand _selectedBrand;
        //public CameraBrand SelectedBrand
        //{
        //    get => _selectedBrand;
        //    set
        //    {
        //        if (SetProperty(ref _selectedBrand, value))
        //        {
        //            if (_currentConfig != null)
        //            {
        //                _currentConfig.CameraType = (int)value;
        //                _cameraConfigService.UpdateCameraSetting(_currentConfig);
        //            }
        //        }
        //    }
        //}

        private CameraBrand _selectedBrand;
        public CameraBrand SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (SetProperty(ref _selectedBrand, value))
                {
                    if (IsConnected && CloseCommand.CanExecute())
                    {
                        ExecuteClose();
                    }
                    if (DeviceList != null && DeviceList.Count > 0)
                    {
                        DeviceList = new List<string>(); // 赋新实例触发 UI 刷新
                    }
                    SelectedDevice = null;
                }
            }
        }

        private string _statusMessage = "请先查找设备...";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        private HObject _currentDebugImage;
        public HObject CurrentDebugImage
        {
            get => _currentDebugImage;
            set
            {
                if (_currentDebugImage != null && _currentDebugImage.IsInitialized())
                {
                    _currentDebugImage.Dispose();
                }
                if (SetProperty(ref _currentDebugImage, value))
                {
                    RaisePropertyChanged(nameof(HasDebugImage));
                    SaveImageCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion
        public CameraDebugViewModel(IEventAggregator eventAggregator,
            ICameraConfigService cameraConfigService,
            IStationConfigService stationConfigService,
            IParametersConfigService paramService,
            ILogger logger)
        {
            _logger = logger;
            _paramService = paramService;
            _eventAggregator = eventAggregator;
            _stationConfigService = stationConfigService;
            _cameraConfigService = cameraConfigService;

            SearchCommand = new DelegateCommand(ExecuteSearch).ObservesCanExecute(() => IsNotConnected);
            InitCommand = new DelegateCommand(ExecuteInit).ObservesCanExecute(() => IsNotConnected);
            CloseCommand = new DelegateCommand(ExecuteClose).ObservesCanExecute(() => IsConnected);
            SaveImageCommand = new DelegateCommand(ExecuteSaveImage).ObservesCanExecute(() => HasDebugImage);
            SoftTriggerCommand = new DelegateCommand(ExecuteSoftTrigger).ObservesCanExecute(() => CanSoftTrigger);
            GetParamCommand = new DelegateCommand(ExecuteGetParam).ObservesCanExecute(() => IsConnected);
            SetParamCommand = new DelegateCommand(ExecuteSetParam).ObservesCanExecute(() => IsConnected);
            TogglePreviewCommand = new DelegateCommand(ExecuteTogglePreview, () => IsConnected).ObservesCanExecute(() => IsConnected);
            DeleteCommand = new DelegateCommand(ExecuteDeleteConfig);
            TriggerSources = new List<TriggerSource>((TriggerSource[])Enum.GetValues(typeof(TriggerSource)));
            SelectedTriggerSource = TriggerSource.Software;
            CameraBrands = Enum.GetValues(typeof(CameraBrand)).Cast<CameraBrand>().ToList();

            _currentConfig = _cameraConfigService.GetCameraSettingBySnOrIndex(null, 0);
            SyncConfigToUI();
            SubscribeToImageEvent();
            _cameraConfigService.CameraGrabbingStatusChanged += (sn, isGrabbing) =>
            {
                if (sn == SelectedDevice)
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        RaisePropertyChanged(nameof(IsCurrentCameraGrabbing));
                        RaisePropertyChanged(nameof(PreviewButtonText));
                        SoftTriggerCommand?.RaiseCanExecuteChanged();
                        TogglePreviewCommand?.RaiseCanExecuteChanged();
                    });
                }
            };
        }

        private void SubscribeToImageEvent()
        {
            if (_isSubscribed) return;

            _imageSubToken = _eventAggregator.GetEvent<HImageDisplayEvent>().Subscribe(
                payload =>
                {
                    if (!_isActiveView || !IsCurrentCameraGrabbing || payload.CameraSN != SelectedDevice)
                        return;

                    // 发布者发布后即 Dispose 原始 HObject，必须同步 Clone 后再跨线程使用
                    var image = payload.Image?.Clone();
                    if (image == null || !image.IsInitialized()) return;

                    try
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            CurrentDebugImage = image;
                        }));
                    }
                    catch (Exception ex)
                    {
                        image?.Dispose();
                        _logger.Error(ex, "更新调试图像异常");
                    }
                    //finally
                    //{
                    //    image?.Dispose();
                    //}
                },
                ThreadOption.PublisherThread,
                true, // keepSubscriberReferenceWeak = true 防止强引用
                null
            );

            _isSubscribed = true;
        }

        #region Commands
        public DelegateCommand SearchCommand { get; }
        public DelegateCommand InitCommand { get; }
        public DelegateCommand CloseCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand StartGrabCommand { get; }
        public DelegateCommand StopGrabCommand { get; }
        public DelegateCommand SoftTriggerCommand { get; }
        public DelegateCommand GetParamCommand { get; }
        public DelegateCommand SetParamCommand { get; }
        public DelegateCommand SaveImageCommand { get; }
        public DelegateCommand TogglePreviewCommand { get; set; }
        #endregion

        #region 执行逻辑
        private void ExecuteSearch()
        {
            try
            {

                SelectedDevice = null;
                DeviceList = CamFactory.GetDeviceEnum(SelectedBrand);
                StatusMessage = (DeviceList != null && DeviceList.Count > 0) ? $"[{SelectedBrand}] 查找成功" : $"未发现 [{SelectedBrand}] 设备";
                if (DeviceList != null && DeviceList.Count > 0)
                {
                    SelectedDevice = DeviceList[0];
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"搜索异常: {ex.Message}";
            }
        }
        private void ExecuteInit()
        {
            _logger.Information("初始化相机");
            if (string.IsNullOrEmpty(SelectedDevice)) return;
            _camera = _cameraConfigService.GetCameraInstance(SelectedDevice);
            _isBorrowedCamera = (_camera != null);
            if (!_isBorrowedCamera)
            {
                bool isSuccess = _cameraConfigService.ConnectCamera(SelectedDevice, (int)SelectedBrand, false);
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
                if (_currentConfig == null)
                {
                    _currentConfig = new CameraSettingModel
                    {
                        SerilalNum = realSn,
                        CamSelectIndex = DeviceList.IndexOf(SelectedDevice)
                    };
                }
                _currentConfig.SerilalNum = realSn;
            }
            StatusMessage = $"相机已连接: {realSn}";
            ExecuteGetParam();
        }
        private void ExecuteClose()
        {
            if (_camera != null)
            {
                if (!_isBorrowedCamera)
                {
                    _cameraConfigService.DisconnectCamera(SelectedDevice);
                }
                IsConnected = false;
                _camera = null;
                StatusMessage = "调试连接已断开";
            }
        }
        private void ExecuteSoftTrigger()
        {
            if (!IsCurrentCameraGrabbing) return;
            if (IsConnected && _camera != null) _camera.SoftTrigger();
        }
        private void ExecuteGetParam()
        {
            if (_camera != null && IsConnected)
            {
                _camera.GetCamConfig(out CamConfig config);
                CameraRoleName = _currentConfig.CameraRole;
                if (config != null) { ExposureTime = (short)config.ExpouseTime; Gain = config.Gain; StatusMessage = "参数读取成功"; }
            }
        }
        private void ExecuteSetParam()
        {
            if (_camera == null || !IsConnected) return;

            if (_currentConfig == null)
            {
                _currentConfig = new CameraSettingModel
                {
                    SerilalNum = SelectedDevice,
                    CamSelectIndex = DeviceList.IndexOf(SelectedDevice)
                };
            }

            _camera.SetExpouseTime((ushort)ExposureTime);
            _camera.SetGain(Gain);

            _currentConfig.ExposureTime = ExposureTime;
            _currentConfig.Gain = Gain;
            _currentConfig.SerilalNum = SelectedDevice;
            _currentConfig.IP = IP;
            _currentConfig.Port = Port;
            _currentConfig.CameraRole = CameraRoleName;
            _currentConfig.TriggerMode = IsTriggerMode ? TriggerMode.On : TriggerMode.Off;
            _currentConfig.TriggerSource = SelectedTriggerSource;
            _cameraConfigService.UpdateCameraSetting(_currentConfig);   // 内部 Add + SaveSettings
            StatusMessage = "参数已保存到本地";
        }

        private void ExecuteSaveImage()
        {
            var image = _currentDebugImage;
            if (image == null || !image.IsInitialized())
            {
                StatusMessage = "当前没有可保存的图像";
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "保存当前图像",
                Filter = "PNG 图像|*.png|BMP 图像|*.bmp|JPEG 图像|*.jpg;*.jpeg|TIFF 图像|*.tif;*.tiff",
                FileName = $"{CameraRoleName}_{DateTime.Now:yyyyMMdd_HHmmss_fff}",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                DefaultExt = ".png",
                AddExtension = true,
                OverwritePrompt = true
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                string ext = System.IO.Path.GetExtension(dlg.FileName);
                ext = ext == null ? "" : ext.ToLowerInvariant();

                // ★ 传统 switch 语句
                string format;
                switch (ext)
                {
                    case ".bmp":
                        format = "bmp";
                        break;
                    case ".jpg":
                    case ".jpeg":
                        format = "jpeg 90";
                        break;
                    case ".tif":
                    case ".tiff":
                        format = "tiff";
                        break;
                    default:
                        format = "png";
                        break;
                }

                HOperatorSet.WriteImage(image, format, 0, dlg.FileName);
                StatusMessage = $"图像已保存: {System.IO.Path.GetFileName(dlg.FileName)}";
                _logger.Information("调试图像已保存: {Path}", dlg.FileName);
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
                _logger.Error(ex, "调试图像保存失败");
                MessageBox.Show($"保存图像失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SyncConfigToUI()
        {
            if (_currentConfig == null) return;
            SelectedBrand = (CameraBrand)_currentConfig.CameraType;
            if (ushort.TryParse(_currentConfig.ExposureTime.ToString(), out ushort exp)) ExposureTime = (short)exp;
            if (short.TryParse(_currentConfig.Gain.ToString(), out short gn)) Gain = gn;
            IP = _currentConfig.IP;
            Port = _currentConfig.Port;
            CameraRoleName = _currentConfig.CameraRole;
            if (_currentConfig.TriggerMode == TriggerMode.On)
            {
                IsTriggerMode = true;
                IsContinuousMode = false;
            }
            else
            {
                IsTriggerMode = false;
                IsContinuousMode = true;
            }
            SelectedTriggerSource = _currentConfig.TriggerSource;
        }
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _isActiveView = true; // 页面切入时激活
            SyncDeviceStatusFromService();
            if (!_isSubscribed)
            {
                SubscribeToImageEvent();
            }
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _isActiveView = false;
            if (_imageSubToken != null)
            {
                _eventAggregator.GetEvent<HImageDisplayEvent>().Unsubscribe(_imageSubToken);
                _imageSubToken = null;
                _isSubscribed = false;
            }
        }
        private void SyncDeviceStatusFromService()
        {
            if (_cameraConfigService.ConnectedCameras.Count > 0)
            {
                var connectedSns = _cameraConfigService.ConnectedCameras.Keys.ToList();
                DeviceList = connectedSns;

                if (string.IsNullOrEmpty(SelectedDevice) || !connectedSns.Contains(SelectedDevice))
                {
                    SelectedDevice = connectedSns[0];
                }

                IsConnected = true;
                _camera = _cameraConfigService.GetCameraInstance(SelectedDevice);
                _isBorrowedCamera = true; // 标记为借用后台已连接的实例
                StatusMessage = $"已自动绑定后台运行相机: {SelectedDevice}";

                ExecuteGetParam();
            }
        }
        private void ExecuteTogglePreview()
        {
            if (string.IsNullOrEmpty(SelectedDevice)) return;

            if (IsCurrentCameraGrabbing)
            {
                _cameraConfigService.StopCameraGrabbing(SelectedDevice);
                StatusMessage = "采集已停止";
            }
            else
            {
                // 先停止当前采集
                _cameraConfigService.StopCameraGrabbing(SelectedDevice);
                AcquisitionMode mode = IsContinuousMode ? AcquisitionMode.Continuous : AcquisitionMode.SoftTrigger;
                _cameraConfigService.StartCameraGrabbing(SelectedDevice, mode, isFromDebug: true);
                StatusMessage = IsContinuousMode ? "连续采集中..." : "触发采集中，等待触发...";
            }
        }

        private void ExecuteDeleteConfig()
        {
            if (string.IsNullOrEmpty(SelectedDevice))
            {
                MessageBox.Show("请先选择要删除的相机。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string sn = SelectedDevice;
            string role = _currentConfig?.CameraRole;
            var stationIds = string.IsNullOrEmpty(role)
                ? new List<string>()
                : _stationConfigService.Stations
                    .Where(s => s.CameraRole == role)
                    .Select(s => s.StationId)
                    .ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"确定要删除相机 [{sn}] 的本地配置吗？");
            sb.AppendLine();
            sb.AppendLine("将删除：");
            sb.AppendLine($"  1. CameraSettings.json 中 SN = {sn} 的配置");
            if (!string.IsNullOrEmpty(role))
                sb.AppendLine($"  2. StationConfigs.json 中 CameraRole = {role} 的 {stationIds.Count} 个工位");
            if (stationIds.Count > 0)
                sb.AppendLine($"  3. 参数配置中 Section = {string.Join(", ", stationIds)} 的所有参数");
            sb.AppendLine("  4. 实时检测界面中该相机的显示窗体");
            sb.AppendLine();
            sb.AppendLine("此操作不可恢复！");

            if (MessageBox.Show(sb.ToString(), "删除确认",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                // 断开相机
                if (_cameraConfigService.ConnectedCameras.ContainsKey(sn))
                {
                    _cameraConfigService.DisconnectCamera(sn);
                    _logger.Information("删除配置前已断开相机 {SN}", sn);
                }

                //CameraSettings.json
                _cameraConfigService.RemoveCameraSetting(sn);

                //StationConfigs.json
                if (!string.IsNullOrEmpty(role))
                {
                    var stationsToRemove = _stationConfigService.Stations
                        .Where(s => s.CameraRole == role)
                        .ToList();
                    foreach (var s in stationsToRemove)
                        _stationConfigService.Stations.Remove(s);
                    if (stationsToRemove.Count > 0)
                    {
                        _stationConfigService.Save();
                        _logger.Information("已从 StationConfigs 删除 {Count} 个角色为 {Role} 的工位",
                            stationsToRemove.Count, role);
                    }
                }

                //参数配置 Section
                foreach (var sid in stationIds)
                {
                    var paramsToRemove = _paramService.ConfigParams
                        .Where(p => p.ModuleName == sid)
                        .ToList();
                    if (paramsToRemove.Count == 0) continue;
                    foreach (var p in paramsToRemove)
                        _paramService.ConfigParams.Remove(p);
                    _logger.Information("已删除工位 {StationId} 的 {Count} 个参数", sid, paramsToRemove.Count);
                }
                if (stationIds.Count > 0)
                {
                    _paramService.SaveConfig();
                    _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
                }

                // 通知InspectionView刷新
                _eventAggregator.GetEvent<StationConfigChangedEvent>().Publish();

                //刷新当前界面
                DeviceList = new List<string>();
                SelectedDevice = null;
                _currentConfig = null;
                _camera = null;
                _isBorrowedCamera = false;
                IsConnected = false;
                StatusMessage = "本地配置已删除";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "删除本地配置失败");
                MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 从 CameraSettings.json 删除指定 SN 的相机配置
        /// </summary>
        private void RemoveCameraSetting(string sn)
        {
            var target = _cameraConfigService.AllSettings.FirstOrDefault(s => s.SerilalNum == sn);
            if (target != null)
            {
                _cameraConfigService.AllSettings.Remove(target);
                _cameraConfigService.SaveSettings();
                _logger.Information("已从 CameraSettings 删除相机 {SN}", sn);
            }
            else
            {
                _logger.Warning("CameraSettings 中未找到相机 {SN}", sn);
            }
        }

        /// <summary>
        /// 从 StationConfigs.json 删除 CameraRole 对应的工位
        /// </summary>
        private StationConfig RemoveStationByCameraRole(string role)
        {
            var target = _stationConfigService.GetStationByCameraRole(role);
            if (target != null)
            {
                _stationConfigService.Stations.Remove(target);
                _stationConfigService.Save();
                _logger.Information("已从 StationConfigs 删除工位 {StationId}（角色 {Role}）", target.StationId, role);
            }
            else
            {
                _logger.Warning("StationConfigs 中未找到角色 {Role} 的工位", role);
            }
            return target;
        }

        /// <summary>
        /// 从参数配置删除指定 Section 的所有参数
        /// </summary>
        private void RemoveParamSection(string stationId)
        {
            var paramsToRemove = _paramService.ConfigParams
                .Where(p => p.ModuleName == stationId)
                .ToList();
            if (paramsToRemove.Count == 0)
            {
                _logger.Warning("参数配置中未找到 Section {StationId}", stationId);
                return;
            }
            foreach (var p in paramsToRemove)
                _paramService.ConfigParams.Remove(p);
            _paramService.SaveConfig();
            _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
            _logger.Information("已删除工位 {StationId} 的 {Count} 个参数", stationId, paramsToRemove.Count);
        }
        #endregion
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // 取消事件订阅
            if (_imageSubToken != null)
            {
                _eventAggregator.GetEvent<HImageDisplayEvent>().Unsubscribe(_imageSubToken);
                _imageSubToken = null;
                _isSubscribed = false;
            }

            // 释放当前图像资源
            if (_currentDebugImage != null && _currentDebugImage.IsInitialized())
            {
                _currentDebugImage.Dispose();
                _currentDebugImage = null;
            }
        }
    }
}
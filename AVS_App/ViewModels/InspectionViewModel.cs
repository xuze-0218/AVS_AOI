using AVS_Common.Events;
using AVS_Common.Model;
using AVS_Core.Services;
using AVS_Service.Models;
using AVS_Service.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace AVS_App.ViewModels
{
    public class InspectionViewModel : BindableBase, INavigationAware
    {
        /// <summary>
        /// 导航日志，记录页面内的导航历史，支持前进后退
        /// </summary>
        private IRegionNavigationJournal _journal;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly IStationConfigService _stationConfigService;
        private readonly ILocalTestService _localTestService;
        private readonly ILogger _logger;
        private int _layoutColumns = 2;
        public int LayoutColumns { get => _layoutColumns; set => SetProperty(ref _layoutColumns, value); }
        //相机数据集合
        public ObservableCollection<CameraDisplayItem> CameraDisplayList { get; set; }
        public DelegateCommand GoBackCommand => new DelegateCommand(() =>
        {
            if (_journal != null && _journal.CanGoBack)
                _journal.GoBack();
        });

        public InspectionViewModel(
            IEventAggregator eventAggregator,
            IStationConfigService stationConfigService,
            ICameraConfigService cameraConfigService,
            ILocalTestService localTestService,
            ILogger logger)
        {
            _eventAggregator = eventAggregator;
            _cameraConfigService = cameraConfigService;
            _stationConfigService = stationConfigService;
            _localTestService = localTestService;
            _logger = logger;
            CameraDisplayList = new ObservableCollection<CameraDisplayItem>();

            //根据配置加载相机窗体数量
            InitializeLayout(_cameraConfigService.AllSettings);
            _eventAggregator.GetEvent<HImageDisplayEvent>().Subscribe(OnImageReceived, ThreadOption.PublisherThread);
        }

        private void OnImageReceived(CameraImagePayload payload)
        {
            var targetCam = CameraDisplayList.FirstOrDefault(x => x.PhysicalSN == payload.CameraSN);
            if (targetCam == null)
                return;

            if (payload.Image == null || !payload.Image.IsInitialized())
                return;
            // 发布者(ICameraConfigService)发布后即 Dispose 原始 HObject，必须同步 Clone 后再使用
            var image = payload.Image.Clone();
            if (payload.ImageType == CameraImageType.Processed)
            {
                targetCam.ProcessedImage = image;
            }
            else
            {
                targetCam.RawImage = image;
            }
        }

        private void InitializeLayout(List<CameraSettingModel> cameraSettings)
        {
            CameraDisplayList.Clear();

            //有几个相机就生成几个窗体
            foreach (var cam in cameraSettings)
            {
                var item = new CameraDisplayItem
                {
                    CameraRoleName = cam.CameraRole,
                    PhysicalSN = cam.SerilalNum
                };
                item.InspectTestCommand = new DelegateCommand(() => OnTestRequested(item, false));
                item.CalibTestCommand = new DelegateCommand(() => OnTestRequested(item, true));
                CameraDisplayList.Add(item);
            }

            // 动态计算列数
            LayoutColumns = CameraDisplayList.Count <= 1 ? 1 :
                            CameraDisplayList.Count <= 4 ? 2 : 3;
        }

        private async void OnTestRequested(CameraDisplayItem item, bool isCalib)
        {
            if (item == null || string.IsNullOrEmpty(item.CameraRoleName))
                return;
            var station = _stationConfigService.GetStationByCameraRole(item.CameraRoleName);
            if (station == null)
            {
                _logger.Warning("未找到相机角色 {Role} 对应的工位配置", item.CameraRoleName);
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = isCalib ? "选择标定图片（一张）" : "选择检测图片（按拍照顺序多选）",
                Multiselect = !isCalib,
                Filter = "图像文件|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|所有文件|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                var paths = dialog.FileNames.ToList();
                LocalTestResult result = isCalib
                    ? await _localTestService.RunCalibrationTestAsync(station.StationId, paths)
                    : await _localTestService.RunInspectTestAsync(station.StationId, paths);
                if (result.Success)
                {
                    _logger.Information("本地测试成功：工位={StationId}, 类型={Type}, 消息={Message}, 结果数据={ResultData}",
                        station.StationId, isCalib ? "标定" : "检测", result.Message, result.ResultData ?? "无");
                }
                else
                {
                    _logger.Warning("本地测试失败：工位={StationId}, 类型={Type}, 消息={Message}",
                        station.StationId, isCalib ? "标定" : "检测", result.Message);
                }
            }

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //获取导航日志
            _journal = navigationContext.NavigationService.Journal;
            //刷新命令的状态
            GoBackCommand.RaiseCanExecuteChanged();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;


        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}

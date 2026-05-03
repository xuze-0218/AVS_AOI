using AVS_Common;
using AVS_Service;
using AVS_Service.Models;
using HalconDotNet;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private int _layoutColumns = 2;
        public int LayoutColumns { get => _layoutColumns; set => SetProperty(ref _layoutColumns, value); }
        // 绑定给 ItemsControl 的相机数据集合
        public ObservableCollection<CameraDisplayItem> CameraDisplayList { get; set; }
        public DelegateCommand GoBackCommand => new DelegateCommand(() =>
        {
            if (_journal != null && _journal.CanGoBack)
                _journal.GoBack();
        });



        public InspectionViewModel(IEventAggregator eventAggregator, ICameraConfigService cameraService, ICameraConfigService cameraConfigService)
        {
            _eventAggregator = eventAggregator;
            _cameraConfigService = cameraConfigService;
            CameraDisplayList = new ObservableCollection<CameraDisplayItem>();

            //根据配置加载相机窗体数量
            InitializeLayout(cameraService.AllSettings);
            //_cameraConfigService.OnImageCaptured += (sn, img) =>
            //{ OnImageReceived(new CameraImagePayload() { CameraSN = sn, Image = img }); };


            //订阅图像到达事件
            _eventAggregator.GetEvent<HImageDisplayEvent>().Subscribe(OnImageReceived);
        }

        private void OnImageReceived(CameraImagePayload payload)
        {
            var targetCam = CameraDisplayList.FirstOrDefault(x => x.PhysicalSN == payload.CameraSN);
            if (targetCam != null)
            {
                targetCam.CurrentImage = payload.Image;
            }
        }

        private void InitializeLayout(List<CameraSettingModel> cameraSettings)
        {
            CameraDisplayList.Clear();

            //有几个相机就生成几个窗体
            foreach (var cam in cameraSettings)
            {
                CameraDisplayList.Add(new CameraDisplayItem
                {
                    CameraRoleName = cam.CameraRole ?? cam.SerilalNum, //使用逻辑角色名
                    PhysicalSN = cam.SerilalNum
                });
            }

            // 动态计算列数
            LayoutColumns = CameraDisplayList.Count <= 1 ? 1 :
                            CameraDisplayList.Count <= 4 ? 2 : 3;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //获取导航日志
            _journal = navigationContext.NavigationService.Journal;
            //刷新命令的状态
            GoBackCommand.RaiseCanExecuteChanged();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

    }

    public class CameraDisplayItem : BindableBase
    {
        public string CameraRoleName { get; set; }
        public string PhysicalSN { get; set; }

        private HObject _currentImage;
        public HObject CurrentImage
        {
            get => _currentImage;
            set
            {
                if (_currentImage != null)
                    _currentImage.Dispose();
                HObject newImage = value.Clone();
                SetProperty(ref _currentImage, newImage);
            }
        }
    }
}

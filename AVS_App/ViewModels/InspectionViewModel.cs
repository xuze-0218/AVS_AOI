using AVS_Common;
using AVS_Service;
using AVS_Service.Models;
using HalconDotNet;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_App.ViewModels
{
    public class InspectionViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private int _layoutColumns = 2;
        public int LayoutColumns { get => _layoutColumns; set => SetProperty(ref _layoutColumns, value); }
        // 绑定给 ItemsControl 的相机数据集合
        public ObservableCollection<CameraDisplayItem> CameraDisplayList { get; set; }

        public InspectionViewModel(IEventAggregator eventAggregator, ICameraConfigService cameraService)
        {
            _eventAggregator = eventAggregator;
            CameraDisplayList = new ObservableCollection<CameraDisplayItem>();

            //根据配置加载相机窗体数量
            InitializeLayout(cameraService.AllSettings);

            //订阅图像到达事件
            _eventAggregator.GetEvent<HImageDisplayEvent>().Subscribe(OnImageReceived);
        }

        private void OnImageReceived(CameraImagePayload payload)
        {
            var targetCam = CameraDisplayList.FirstOrDefault(x => x.PhysicalSN == payload.CameraSN);
            if (targetCam != null)
            {
                targetCam.UpdateImage(payload.Image);
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
    }

    public class CameraDisplayItem : BindableBase
    {
        public string CameraRoleName { get; set; }
        public string PhysicalSN { get; set; }

        public void UpdateImage(HObject img)
        {

        }
    }
}

using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Modules_Settings.ViewModels
{
    internal class DebugCenterViewModel: BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private bool _isLoaded = false;

        public DebugCenterViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 第一次进入时初始化所有子页面
            if (_isLoaded) return;
            _isLoaded = true;
            _regionManager.RequestNavigate("CameraDebugRegion", "CameraDebugView");
            _regionManager.RequestNavigate("PlcDebugRegion", "PlcDebugView");
            _regionManager.RequestNavigate("ProtocolConfigRegion", "ProtocolConfigView");
            _regionManager.RequestNavigate("StationConfigRegion", "StationConfigView");
            _regionManager.RequestNavigate("TempAndCaliDebugRegion", "TempAndCaliDebugView");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}

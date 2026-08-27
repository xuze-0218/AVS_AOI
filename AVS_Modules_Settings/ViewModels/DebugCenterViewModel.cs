using Prism.Mvvm;
using Prism.Regions;
using System;

namespace AVS_Modules_Settings.ViewModels
{
    public class DebugCenterViewModel: BindableBase, INavigationAware
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

            // 使用 Application.Current.Dispatcher 延迟执行导航，确保界面的 Region 已经挂载完毕
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _regionManager.RequestNavigate("CameraDebugRegion", "CameraDebugView");
                _regionManager.RequestNavigate("PlcDebugRegion", "PlcDebugView");
                _regionManager.RequestNavigate("ProtocolConfigRegion", "ProtocolConfigView");
                _regionManager.RequestNavigate("StationConfigRegion", "StationConfigView");
                _regionManager.RequestNavigate("TempAndCaliDebugRegion", "TempAndCaliDebugView");
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}

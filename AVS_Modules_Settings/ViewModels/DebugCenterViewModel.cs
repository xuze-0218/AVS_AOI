using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace AVS_Modules_Settings.ViewModels
{
    /// <summary>
    /// 调试中心 ViewModel
    /// 负责根据用户选中的 Tab，按需导航到对应的子 Region。
    /// 只有被选中的 Tab 才会被创建 View，避免未显示的 HALCON 控件被提前实例化。
    /// </summary>
    public class DebugCenterViewModel :
        BindableBase,
        INavigationAware,
        IRegionMemberLifetime
    {
        private readonly IRegionManager _regionManager;

        /// <summary>
        /// DebugCenter 自身保持存活。
        /// 当从外部主导航离开时，不希望整个调试中心被重新创建。
        /// </summary>
        public bool KeepAlive => true;

        private bool _isLoaded;

        /// <summary>
        /// 防止同一个 Tab 被重复安排导航。
        /// </summary>
        private bool _navigationScheduled;

        private int _pendingTabIndex = -1;

        private readonly HashSet<int> _initializedTabs = new HashSet<int>();

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;

            set
            {
                if (!SetProperty(ref _selectedTabIndex, value))
                    return;

                Debug.WriteLine(
                    $"[DebugCenter] SelectedTabIndex -> {_selectedTabIndex}");

                // DebugCenter 已经完成首次导航后，
                // 用户切换 Tab 时才进行对应的 RequestNavigate。
                ScheduleNavigateSelectedTab();
            }
        }

        public DebugCenterViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            // 默认显示第一个 Tab：相机调试
            _selectedTabIndex = 0;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Debug.WriteLine("[DebugCenter] OnNavigatedTo");

            if (_isLoaded)
            {
                ScheduleNavigateSelectedTab();
                return;
            }

            _isLoaded = true;
            ScheduleNavigateSelectedTab();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Debug.WriteLine("[DebugCenter] OnNavigatedFrom");
        }

        private void ScheduleNavigateSelectedTab()
        {
            if (!_isLoaded)
                return;

            _pendingTabIndex = _selectedTabIndex;

            if (_navigationScheduled)
                return;

            _navigationScheduled = true;

            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    _navigationScheduled = false;
                    int target = _pendingTabIndex;
                    _pendingTabIndex = -1;

                    if (target >= 0)
                        NavigateTab(target);
                }),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }


        private void NavigateTab(int tabIndex)
        {
            if (!_isLoaded)
                return;

            if (_initializedTabs.Contains(tabIndex))
            {
                Debug.WriteLine(
                    $"[DebugCenter] Tab {tabIndex} 已初始化，跳过导航");
                return;
            }

            string regionName;
            string viewName;

            switch (tabIndex)
            {
                case 0:
                    regionName = "CameraDebugRegion";
                    viewName = "CameraDebugView";
                    break;

                case 1:
                    regionName = "PlcDebugRegion";
                    viewName = "PlcDebugView";
                    break;

                case 2:
                    regionName = "ProtocolConfigRegion";
                    viewName = "ProtocolConfigView";
                    break;

                case 3:
                    regionName = "StationConfigRegion";
                    viewName = "StationConfigView";
                    break;

                case 4:
                    regionName = "TempAndCaliDebugRegion";
                    viewName = "TempAndCaliDebugView";
                    break;

                default:
                    Debug.WriteLine(
                        $"[DebugCenter] 未知 TabIndex={tabIndex}");
                    return;
            }

            Debug.WriteLine(
                $"[DebugCenter] 开始导航: " +
                $"Tab={tabIndex}, Region={regionName}, View={viewName}");

            try
            {
                _regionManager.RequestNavigate(
                    regionName,
                    viewName,
                    result =>
                    {
                        if (result.Result.Value)
                        {
                            _initializedTabs.Add(tabIndex);

                            Debug.WriteLine(
                                $"[DebugCenter] 导航成功: " +
                                $"Tab={tabIndex}, View={viewName}");
                        }
                        else
                        {
                            Debug.WriteLine(
                                $"[DebugCenter] 导航失败: " +
                                $"Tab={tabIndex}, View={viewName}, " +
                                $"Error={result.Error}");

                            // 导航失败允许下次重新尝试
                            _initializedTabs.Remove(tabIndex);
                        }
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[DebugCenter] RequestNavigate异常: " +
                    $"Tab={tabIndex}, " +
                    $"Region={regionName}, " +
                    $"View={viewName}, " +
                    $"Exception={ex}");

                _initializedTabs.Remove(tabIndex);
            }
        }
    }
}
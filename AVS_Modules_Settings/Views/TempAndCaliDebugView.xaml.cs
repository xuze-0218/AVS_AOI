using AVS_Modules_Settings.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AVS_Modules_Settings.Views
{
    /// <summary>
    /// TempAndCaliDebugView.xaml 容器代码后置
    /// ROI 绘制升级到协调器（TempAndCaliDebugViewModel），
    /// 掩膜编辑仍由 TemplateMatchingViewModel 负责。
    /// </summary>
    public partial class TempAndCaliDebugView : UserControl
    {
        private TempAndCaliDebugViewModel _viewModel;
        private TemplateMatchingViewModel _tmVM => _viewModel?.TemplateMatchingVM;

        public TempAndCaliDebugView()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is TempAndCaliDebugViewModel oldVm)
                UnsubscribeEvents(oldVm);
            if (e.NewValue is TempAndCaliDebugViewModel vm)
            {
                _viewModel = vm;
                _viewModel.SetHalconWindow(CameraDisplay.HalconWindow);
                SubscribeEvents(vm);
                UpdateMoveContentState();
            }
        }

        private void SubscribeEvents(TempAndCaliDebugViewModel vm)
        {
            CameraDisplay.MouseLeftButtonDown += OnMouseLeftDown;
            CameraDisplay.MouseLeftButtonUp += OnMouseLeftUp;
            CameraDisplay.MouseMove += OnMouseMove;
            CameraDisplay.MouseRightButtonDown += OnMouseRightDown;
            // 监听协调器的 IsCustomMode 变化（多边形绘制）
            vm.PropertyChanged += OnCoordinatorPropertyChanged;
            // 也监听子 VM 的 IsCustomMode 变化（掩膜编辑）
            if (_tmVM != null)
                _tmVM.PropertyChanged += OnTmPropertyChanged;
        }

        private void UnsubscribeEvents(TempAndCaliDebugViewModel vm)
        {
            CameraDisplay.MouseLeftButtonDown -= OnMouseLeftDown;
            CameraDisplay.MouseLeftButtonUp -= OnMouseLeftUp;
            CameraDisplay.MouseMove -= OnMouseMove;
            CameraDisplay.MouseRightButtonDown -= OnMouseRightDown;
            if (vm != null)
                vm.PropertyChanged -= OnCoordinatorPropertyChanged;
            if (_tmVM != null)
                _tmVM.PropertyChanged -= OnTmPropertyChanged;
        }

        private void OnCoordinatorPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TempAndCaliDebugViewModel.IsCustomMode))
                UpdateMoveContentState();
        }

        private void OnTmPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TemplateMatchingViewModel.IsCustomMode))
                UpdateMoveContentState();
        }

        private void UpdateMoveContentState()
        {
            if (_viewModel == null) return;
            // 多边形绘制 或 掩膜编辑 → 禁用平移
            CameraDisplay.HMoveContent = !_viewModel.IsCustomMode;
        }

        private void CameraDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.SetHalconWindow(CameraDisplay.HalconWindow);
            UpdateMoveContentState();
        }

        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            ConvertToImageCoords(e, out double row, out double col);
            UpdateMouseCoords(row, col);

            if (_viewModel.IsDrawingPolygon)
            {
                // 多边形绘制模式：加点
                _viewModel.AddPolygonPoint(row, col);
                e.Handled = true;
            }
            else if (_tmVM != null && _tmVM.IsCustomMode)
            {
                // 掩膜编辑模式：路由到 TemplateMatchingVM
                _tmVM.OnMouseDown(row, col);
                e.Handled = true;
            }
        }

        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (_tmVM == null) return;
            if (_tmVM.IsCustomMode)
            {
                _tmVM.OnMouseUp();
                e.Handled = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel == null) return;
            ConvertToImageCoords(e, out double row, out double col);
            UpdateMouseCoords(row, col);

            if (_viewModel.IsDrawingPolygon)
            {
                // 多边形模式不处理移动
            }
            else if (_tmVM != null && _tmVM.IsCustomMode && e.LeftButton == MouseButtonState.Pressed)
            {
                _tmVM.OnMouseMove(row, col);
                e.Handled = true;
            }
        }

        private void OnMouseRightDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            ConvertToImageCoords(e, out double row, out double col);
            UpdateMouseCoords(row, col);

            if (_viewModel.IsDrawingPolygon)
            {
                // 右键闭合多边形
                _viewModel.FinishPolygon();
                e.Handled = true;
            }
            // 否则弹出 ContextMenu（右键选择ROI形状），不需要额外处理
        }

        private void UpdateMouseCoords(double row, double col)
        {
            _viewModel.CurrentMouseRow = row;
            _viewModel.CurrentMouseCol = col;
        }

        private void ConvertToImageCoords(MouseEventArgs e, out double row, out double col)
        {
            var pos = e.GetPosition(CameraDisplay.HsmartWindowControl);
            CameraDisplay.HalconWindow.ConvertCoordinatesWindowToImage(pos.Y, pos.X, out row, out col);
        }
    }
}
using AVS_Modules_Settings.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AVS_Modules_Settings.Views
{
    /// <summary>
    /// TempAndCaliDebugView.xaml 容器代码后置
    /// 将图像区域的鼠标事件路由到 TemplateMatchingViewModel，
    /// 由 TemplateMatchingVM 统一处理 ROI 绘制、多边形和掩膜编辑。
    /// 参照 PreviousTempAndCaliDebugViewModel 的设计方案。
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
            {
                UnsubscribeEvents();
            }
            if (e.NewValue is TempAndCaliDebugViewModel vm)
            {
                _viewModel = vm;
                _viewModel.SetHalconWindow(CameraDisplay.HalconWindow);
                SubscribeEvents();
                UpdateMoveContentState();
            }
        }

        private void SubscribeEvents()
        {
            CameraDisplay.MouseLeftButtonDown += OnMouseLeftDown;
            CameraDisplay.MouseLeftButtonUp += OnMouseLeftUp;
            CameraDisplay.MouseMove += OnMouseMove;
            CameraDisplay.MouseRightButtonDown += OnMouseRightDown;
            if (_tmVM != null)
                _tmVM.PropertyChanged += OnTmPropertyChanged;
        }

        private void UnsubscribeEvents()
        {
            CameraDisplay.MouseLeftButtonDown -= OnMouseLeftDown;
            CameraDisplay.MouseLeftButtonUp -= OnMouseLeftUp;
            CameraDisplay.MouseMove -= OnMouseMove;
            CameraDisplay.MouseRightButtonDown -= OnMouseRightDown;
            if (_tmVM != null)
                _tmVM.PropertyChanged -= OnTmPropertyChanged;
        }

        private void OnTmPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TemplateMatchingViewModel.IsCustomMode))
            {
                UpdateMoveContentState();
            }
        }

        private void UpdateMoveContentState()
        {
            if (_tmVM == null) return;
            CameraDisplay.HMoveContent = !_tmVM.IsCustomMode;
        }

        private void CameraDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.SetHalconWindow(CameraDisplay.HalconWindow);
            UpdateMoveContentState();
        }

        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (_tmVM == null || !_tmVM.IsCustomMode) return;
            ConvertToImageCoords(e, out double row, out double col);
            // 更新坐标
            _tmVM.CurrentMouseRow = row;
            _tmVM.CurrentMouseCol = col;
            _viewModel.CurrentMouseRow = row;
            _viewModel.CurrentMouseCol = col;
            if (_tmVM.IsDrawingPolygon)
            {
                _tmVM.AddPolygonPoint(row, col);
            }
            else
            {
                _tmVM.OnMouseDown(row, col);
            }
            e.Handled = true;
        }

        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (_tmVM == null || !_tmVM.IsCustomMode) return;
            _tmVM.OnMouseUp();
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_tmVM == null || !_tmVM.IsCustomMode) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;
            ConvertToImageCoords(e, out double row, out double col);
            _tmVM.CurrentMouseRow = row;
            _tmVM.CurrentMouseCol = col;
            _viewModel.CurrentMouseRow = row;
            _viewModel.CurrentMouseCol = col;
            if (_tmVM.IsDrawingPolygon)
            {
                // 多边形模式下左键拖动不处理，只在按下时加点
            }
            else
            {
                _tmVM.OnMouseMove(row, col);
            }
            e.Handled = true;
        }

        private void OnMouseRightDown(object sender, MouseButtonEventArgs e)
        {
            if (_tmVM == null) return;
            ConvertToImageCoords(e, out double row, out double col);
            _tmVM.CurrentMouseRow = row;
            _tmVM.CurrentMouseCol = col;
            _viewModel.CurrentMouseRow = row;
            _viewModel.CurrentMouseCol = col;
            if (_tmVM.IsDrawingPolygon)
            {
                _tmVM.FinishPolygon();
                e.Handled = true;
            }
            // 右键不再弹出 ContextMenu
            // ROI 绘制命令通过 TemplateMatchingView 右侧面板的按钮触发
        }

        private void ConvertToImageCoords(MouseEventArgs e, out double row, out double col)
        {
            var pos = e.GetPosition(CameraDisplay.HsmartWindowControl);
            CameraDisplay.HalconWindow.ConvertCoordinatesWindowToImage(pos.Y, pos.X, out row, out col);
        }
    }
}
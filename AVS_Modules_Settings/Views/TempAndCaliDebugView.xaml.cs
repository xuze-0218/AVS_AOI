using AVS_Modules_Settings.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AVS_Modules_Settings.Views
{
    /// <summary>
    /// TempAndCaliDebugView.xaml 的交互逻辑
    /// </summary>
    public partial class TempAndCaliDebugView : UserControl
    {
        private double _lastRightClickRow;
        private double _lastRightClickCol;
        private TempAndCaliDebugViewModel _viewModel;
        public TempAndCaliDebugView()
        {
            InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TempAndCaliDebugViewModel vm)
            {
                if (_viewModel != null)
                    _viewModel.PropertyChanged -= _viewModel_PropertyChanged;
                _viewModel = vm;
                _viewModel.PropertyChanged += _viewModel_PropertyChanged;
                UpdateMoveContentState();
                CameraDisplay.MouseLeftButtonDown += OnMouseLeftDown;
                CameraDisplay.MouseLeftButtonUp += OnMouseLeftUp;
                CameraDisplay.MouseMove += CameraDisplay_MouseMove;
                CameraDisplay.MouseRightButtonDown += OnMouseRightDown;
            }
            if (e.OldValue is TempAndCaliDebugViewModel oldVm)
            {
                oldVm.PropertyChanged -= _viewModel_PropertyChanged;
                CameraDisplay.MouseLeftButtonDown -= OnMouseLeftDown;
                CameraDisplay.MouseLeftButtonUp -= OnMouseLeftUp;
                CameraDisplay.MouseMove -= CameraDisplay_MouseMove;
                CameraDisplay.MouseRightButtonDown -= OnMouseRightDown;
            }
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TempAndCaliDebugViewModel.IsMaskEditing) ||
            e.PropertyName == nameof(TempAndCaliDebugViewModel.IsDrawingPolygon))
            {
                UpdateMoveContentState();
            }
        }

        private void UpdateMoveContentState()
        {
            if (_viewModel == null) return;
            bool customMode = _viewModel.IsMaskEditing || _viewModel.IsDrawingPolygon;
            CameraDisplay.HMoveContent = !customMode;
        }

        private void CameraDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel?.IsMaskEditing == true && e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(CameraDisplay.HsmartWindowControl);
                CameraDisplay.HalconWindow.ConvertCoordinatesWindowToImage(pos.Y, pos.X, out double row, out double col);
                _viewModel.OnMouseMove(row, col);
                e.Handled = true;
            }
        }
        private void CameraDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.SetHalconWindow(CameraDisplay.HalconWindow);
        }
        private void OnMouseRightDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(CameraDisplay.HsmartWindowControl);
            CameraDisplay.HalconWindow.ConvertCoordinatesWindowToImage(pos.Y, pos.X, out _lastRightClickRow, out _lastRightClickCol);
            if (_viewModel != null)
            {
                _viewModel.CurrentMouseRow = _lastRightClickRow;
                _viewModel.CurrentMouseCol = _lastRightClickCol;
            }
            if (_viewModel?.IsDrawingPolygon == true)
            {
                _viewModel.FinishPolygon();
                e.Handled = true;
            }          
        }
        private void HalconWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TempAndCaliDebugViewModel vm)
            {
                _viewModel?.SetHalconWindow(CameraDisplay.HalconWindow);
            }
        }
        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            var pos = e.GetPosition(CameraDisplay.HsmartWindowControl);
            CameraDisplay.HalconWindow.ConvertCoordinatesWindowToImage(pos.Y, pos.X, out double row, out double col);
            if (_viewModel.IsMaskEditing)
            {
                _viewModel.OnMouseDown(row, col);
                e.Handled = true;
            }
            else if (_viewModel.IsDrawingPolygon)
            {
                _viewModel.AddPolygonPoint(row, col);
                e.Handled = true;
            }
        }
        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel?.IsMaskEditing == true)
            {
                _viewModel.OnMouseUp();
                e.Handled = true;
            }
        }     
    }
}

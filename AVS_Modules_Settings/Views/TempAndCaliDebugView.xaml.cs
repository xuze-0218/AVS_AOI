using AVS_Modules_Settings.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AVS_Modules_Settings.Views
{
    /// <summary>
    /// TempAndCaliDebugView.xaml 的交互逻辑
    /// </summary>
    public partial class TempAndCaliDebugView : UserControl
    {
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
                _viewModel = vm;
                CameraDisplay.MouseLeftButtonDown += OnMouseLeftDown;
                CameraDisplay.MouseLeftButtonUp += OnMouseLeftUp;
                CameraDisplay.MouseMove += CameraDisplay_MouseMove;
                CameraDisplay.MouseRightButtonDown += OnMouseRightDown;
            }
            if (e.OldValue is TempAndCaliDebugViewModel oldVm)
            {
                CameraDisplay.MouseLeftButtonDown -= OnMouseLeftDown;
                CameraDisplay.MouseLeftButtonUp -= OnMouseLeftUp;
                CameraDisplay.MouseMove -= CameraDisplay_MouseMove;
                CameraDisplay.MouseRightButtonDown -= OnMouseRightDown;
            }
        }

        private void OnMouseRightDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel?.IsDrawingPolygon == true)
            {
                _viewModel.FinishPolygon();
                e.Handled = true;
            }
        }

        private void CameraDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel?.IsMaskEditing == true && e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(CameraDisplay);
                CameraDisplay.HalconWindow.ConvertCoordinatesWindowToImage(pos.Y, pos.X, out double row, out double col);
                _viewModel.OnMouseMove(row, col);
                e.Handled = true;
            }
        }

        private void CameraDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.SetHalconWindow(CameraDisplay.HalconWindow);
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel?.IsDrawingPolygon == true)
            {
                _viewModel.FinishPolygon();
                e.Handled = true;
            }
        }

        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel?.IsMaskEditing == true)
            {
                var pos = e.GetPosition(CameraDisplay);
                CameraDisplay.HalconWindow.ConvertCoordinatesWindowToImage(pos.Y, pos.X, out double row, out double col);
                _viewModel.OnMouseDown(row, col);
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


        private void HalconWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TempAndCaliDebugViewModel vm)
            {
                _viewModel?.SetHalconWindow(CameraDisplay.HalconWindow);
            }
        }
    }
}

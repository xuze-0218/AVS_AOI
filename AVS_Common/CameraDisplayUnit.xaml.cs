using AVS_Common.Model;
using HalconDotNet;
using System.Windows;
using System.Windows.Controls;
namespace AVS_Common
{
    /// <summary>
    /// CameraDisplayUnit.xaml 的交互逻辑
    /// </summary>
    public partial class CameraDisplayUnit : UserControl
    {
        //private readonly IWindowHandleRegistry _windowHandleRegistry;
        private bool _isRegistered = false;
        public HWindow HalconWindow => HsmartWindow.HalconWindow;
        public CameraDisplayUnit()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
                TryRegister();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TryRegister();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //if (DataContext is CameraDisplayItem item && !string.IsNullOrEmpty(item.CameraRoleName))
            //{
            //    WindowHandleEvent.RaiseHandleUnregistered(item.CameraRoleName);
            //}
            _isRegistered = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //TryRegister();
        }

        private void TryRegister()
        {
            if (_isRegistered) return;
            if (DataContext is CameraDisplayItem item && !string.IsNullOrEmpty(item.CameraRoleName))
            {
                if (HsmartWindow.ActualWidth <= 0 || HsmartWindow.ActualHeight <= 0)
                    return;
                try
                {
                    var hWindow = HsmartWindow.HalconWindow;
                    WindowHandleEvent.RaiseHandleRegistered(item.CameraRoleName, hWindow);
                    _isRegistered = true;
                }
                catch (Exception ex)
                {
                }
            }
        }

        public HObject DispImage
        {
            get { return (HObject)GetValue(DispImageProperty); }
            set { SetValue(DispImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DispImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DispImageProperty =
            DependencyProperty.Register("DispImage", typeof(HObject), typeof(CameraDisplayUnit), new PropertyMetadata(null, OnHObjectChanged));


        private static void OnHObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CameraDisplayUnit;
            control.UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (HalconWindow == null) return;
            HalconWindow.ClearWindow();

            // 显示图像
            if (DispImage != null && DispImage.IsInitialized())
            {
                HOperatorSet.GetImageSize(DispImage, out HTuple width, out HTuple height);
                HalconWindow.SetPart(0, 0, (int)height - 1, (int)width - 1);
                HalconWindow.DispObj(DispImage);
            }
            // 叠加显示区域
            if (DispRegion != null && DispRegion.IsInitialized() && DispRegion.CountObj() > 0)
            {
                HalconWindow.SetColor("green");
                HalconWindow.SetLineWidth(2);
                HalconWindow.SetDraw("margin");
                HalconWindow.DispObj(DispRegion);
            }
        }


        public HObject DispRegion
        {
            get { return (HObject)GetValue(DispRegionProperty); }
            set { SetValue(DispRegionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DispRegion.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DispRegionProperty =
            DependencyProperty.Register("DispRegion", typeof(HObject), typeof(CameraDisplayUnit), new PropertyMetadata(null, OnHObjectChanged));


    }
}

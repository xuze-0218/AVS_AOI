using AVS_Common.Model;
using AVS_Common.Services;
using DryIoc;
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
        private readonly IWindowHandleRegistry _windowHandleRegistry;
        public CameraDisplayUnit(IWindowHandleRegistry windowHandleRegistry) : this()
        {
            _windowHandleRegistry = windowHandleRegistry;
        }

        public CameraDisplayUnit()
        {
            InitializeComponent();
            //Loaded += OnLoaded;
            //Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CameraDisplayItem item && !string.IsNullOrEmpty(item.PhysicalSN))
            {
                _windowHandleRegistry.Unregister(item.PhysicalSN);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CameraDisplayItem item && !string.IsNullOrEmpty(item.PhysicalSN))
            {
                HWindow hWindow = HsmartWindow.HalconWindow;  // HSmartWindowControlWPF 的 HalconWindow 属性
                _windowHandleRegistry.Register(item.PhysicalSN, hWindow);
            }
        }

        public HObject DispImage
        {
            get { return (HObject)GetValue(DispImageProperty); }
            set { SetValue(DispImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DispImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DispImageProperty =
            DependencyProperty.Register("DispImage", typeof(HObject), typeof(CameraDisplayUnit), new PropertyMetadata(null, OnHImageChanged));


        private static void OnHImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CameraDisplayUnit;
            var img = e.NewValue as HObject;
            if (control != null && img != null && img.IsInitialized())
            {
                HOperatorSet.GetImageSize(img, out HTuple width, out HTuple height);
                control.HsmartWindow.HalconWindow.SetPart(0, 0, (int)height - 1, (int)width - 1);
                control.HsmartWindow.HalconWindow.DispObj(img);
            }
        }



    }
}

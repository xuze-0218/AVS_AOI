using AVS_Common.Model;
using AVS_Common.Services;
using DryIoc;
using HalconDotNet;
using Prism.Events;
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

        public CameraDisplayUnit()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TryRegister();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CameraDisplayItem item && !string.IsNullOrEmpty(item.PhysicalSN))
            {
                WindowHandleEvent.RaiseHandleUnregistered(item.PhysicalSN);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TryRegister();
        }

        private void TryRegister()
        {
            if (DataContext is CameraDisplayItem item && !string.IsNullOrEmpty(item.PhysicalSN))
            {
                var hWindow = HsmartWindow.HalconWindow;
                WindowHandleEvent.RaiseHandleRegistered(item.PhysicalSN, hWindow);
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

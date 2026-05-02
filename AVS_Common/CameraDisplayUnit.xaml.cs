using HalconDotNet;
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

namespace AVS_Common
{
    /// <summary>
    /// CameraDisplayUnit.xaml 的交互逻辑
    /// </summary>
    public partial class CameraDisplayUnit : UserControl
    {
        public CameraDisplayUnit()
        {
            InitializeComponent();
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
            var img =e.NewValue as HObject;
            if (control!=null && img!=null && img.IsInitialized())
            {
                HOperatorSet.GetImageSize(img, out HTuple width, out HTuple height);
                control.HWindow.HalconWindow.SetPart(0, 0, (int)height - 1, (int)width - 1);
                control.HWindow.HalconWindow.DispObj(img);
            }
        }



    }
}

using AVS_Common;
using HalconDotNet;
using Prism.Events;
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
    /// CameraDebugView.xaml 的交互逻辑
    /// </summary>
    public partial class CameraDebugView : UserControl
    {
        public CameraDebugView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            //界面显示 
            eventAggregator.GetEvent<HImageDisplayEvent>().Subscribe(payload =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (payload.Image != null)
                    {
                        HOperatorSet.GetImageSize(payload.Image, out HTuple width, out HTuple height);
                        HOperatorSet.SetPart(CameraDebugDisplay.HalconWindow, 0, 0, height - 1, width - 1);
                        HOperatorSet.DispObj(payload.Image,CameraDebugDisplay.HalconWindow);
                        //DispImage有bug，只显示灰色图
                        //HOperatorSet.DispImage(payload.Image, CameraDebugDisplay.HalconWindow);
                        //payload.Image.Dispose();
                    }
                });
            });
        }
    }
}

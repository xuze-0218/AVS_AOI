using AVS_Common.Events;
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
        private bool _isRegistered = false;
        public HWindow HalconWindow { get; private set; }

        /// <summary>
        /// 暴露 HSmartWindowControlWPF 控件引用，以便外部进行精确的窗口坐标到图像坐标转换
        /// </summary>
        public HSmartWindowControlWPF HsmartWindowControl => HsmartWindow;

        public bool HMoveContent
        {
            get => HsmartWindow.HMoveContent;
            set => HsmartWindow.HMoveContent = value;
        }

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
            {
                TryRegister();
                UpdateDisplay();
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TryRegister();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isRegistered = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //// 确保 HalconWindow 始终可用，不依赖 DataContext 类型
            //if (HalconWindow == null)
            //{
            //    try
            //    {
            //        if (HsmartWindow.ActualWidth > 0 && HsmartWindow.ActualHeight > 0)
            //            HalconWindow = HsmartWindow.HalconWindow;
            //    }
            //    catch (HalconException)
            //    {
            //        // 尺寸为 0 时 HALCON 初始化会失败，等下次 SizeChanged 触发
            //    }
            //}
            TryRegister();
            UpdateDisplay();
        }


        private void TryRegister()
        {
            if (_isRegistered) return;

            string roleName = CameraRoleName;
            if (string.IsNullOrEmpty(roleName) && DataContext is CameraDisplayItem item)
            {
                roleName = item.CameraRoleName;
            }

            if (HsmartWindow.ActualWidth <= 0 || HsmartWindow.ActualHeight <= 0)
                return;

            try
            {
                var hWindow = HsmartWindow.HalconWindow;
                if (hWindow == null || !hWindow.IsInitialized()) return;

                HalconWindow = hWindow;

                // 只有角色名有效时才注册
                if (!string.IsNullOrEmpty(roleName))
                {
                    WindowHandleEvent.RaiseHandleRegistered(roleName, hWindow);
                    _isRegistered = true;  // 仅在注册成功后标记
                }
                else
                {
                    // 角色名为空时，仅缓存句柄，但不标记为已注册，以便后续角色名有效时再次尝试
                    _isRegistered = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TryRegister failed: {ex.Message}");
                _isRegistered = false;
            }
        }

        public HObject DispImage
        {
            get { return (HObject)GetValue(DispImageProperty); }
            set { SetValue(DispImageProperty, value); }
        }

        public static readonly DependencyProperty DispImageProperty =
            DependencyProperty.Register("DispImage", typeof(HObject), typeof(CameraDisplayUnit),
                new PropertyMetadata(null, OnHObjectChanged));

        public HObject DispRegion
        {
            get { return (HObject)GetValue(DispRegionProperty); }
            set { SetValue(DispRegionProperty, value); }
        }

        public static readonly DependencyProperty DispRegionProperty =
            DependencyProperty.Register("DispRegion", typeof(HObject), typeof(CameraDisplayUnit),
                new PropertyMetadata(null, OnHObjectChanged));

        public string CameraRoleName
        {
            get => (string)GetValue(CameraRoleNameProperty);
            set => SetValue(CameraRoleNameProperty, value);
        }

        public static readonly DependencyProperty CameraRoleNameProperty =
    DependencyProperty.Register(
        nameof(CameraRoleName),
        typeof(string),
        typeof(CameraDisplayUnit),
        new PropertyMetadata(null, OnCameraRoleNameChanged));

        private static void OnCameraRoleNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CameraDisplayUnit)d;
            if (!string.IsNullOrEmpty(control.CameraRoleName) && control.IsLoaded &&
                control.HsmartWindow.ActualWidth > 0 && control.HsmartWindow.ActualHeight > 0)
            {
                // 角色名已变化，重新注册窗口句柄
                control._isRegistered = false;
                control.TryRegister();
            }
        }

        private static void OnHObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CameraDisplayUnit;
            control.UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            // 窗口尚未完成布局或尺寸无效时，直接访问 HsmartWindow.HalconWindow
            // 会触发 HALCON 内部 HInitializeWindow → open_window，若 size=0 则抛出
            if (!IsLoaded || HsmartWindow.ActualWidth <= 0 || HsmartWindow.ActualHeight <= 0)
                return;
            // 如果缓存句柄为空，尝试获取（内部会检查尺寸并安全获取）
            if (HalconWindow == null)
            {
                TryRegister();
            }
            HWindow hw = HalconWindow;
            if (hw == null || !hw.IsInitialized())
                return;

            try
            {
                hw.ClearWindow();

                if (DispImage != null && DispImage.IsInitialized())
                {
                    HOperatorSet.GetImageSize(DispImage, out HTuple width, out HTuple height);
                    SetPartKeepAspectRatio(hw, (int)width, (int)height);
                    hw.DispObj(DispImage);
                }

                if (DispRegion != null && DispRegion.IsInitialized() && DispRegion.CountObj() > 0)
                {
                    hw.SetColor("green");
                    hw.SetLineWidth(2);
                    hw.SetDraw("margin");
                    hw.DispObj(DispRegion);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateDisplay failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据窗口实际尺寸和图像尺寸，计算等比例显示的 SetPart 区域，
        /// 使图像始终等比例居中显示
        /// </summary>
        private void SetPartKeepAspectRatio(HWindow hw, int imageWidth, int imageHeight)
        {
            double winWidth = HsmartWindow.ActualWidth;
            double winHeight = HsmartWindow.ActualHeight;
            double imgRatio = (double)imageWidth / imageHeight;
            double winRatio = winWidth / winHeight;

            double row1, col1, row2, col2;

            if (imgRatio > winRatio)
            {
                // 图像比窗口更宽：宽度填满，上下留黑边
                double dispHeight = imageWidth / winRatio;
                double offset = (dispHeight - imageHeight) / 2.0;
                row1 = -offset;
                col1 = 0;
                row2 = imageHeight - 1 + offset;
                col2 = imageWidth - 1;
            }
            else
            {
                // 图像比窗口更高（或相等）：高度填满，左右留黑边
                double dispWidth = imageHeight * winRatio;
                double offset = (dispWidth - imageWidth) / 2.0;
                row1 = 0;
                col1 = -offset;
                row2 = imageHeight - 1;
                col2 = imageWidth - 1 + offset;
            }

            hw.SetPart((int)row1, (int)col1, (int)row2, (int)col2);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CameraDisplayItem item)
            {
                var menu = new ContextMenu();

                var inspectItem = new MenuItem { Header = "检测测试" };
                inspectItem.Command = item.InspectTestCommand;
                menu.Items.Add(inspectItem);
                var calibItem = new MenuItem { Header = "标定测试" };
                calibItem.Command = item.CalibTestCommand;
                menu.Items.Add(calibItem);
                menu.PlacementTarget = TestButton;
                menu.IsOpen = true;
            }
        }
    }
}
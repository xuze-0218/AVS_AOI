using AVS_Common.Events;
using AVS_Common.Model;
using HalconDotNet;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AVS_Common
{
    /// <summary>
    /// CameraDisplayUnit.xaml 的交互逻辑
    /// </summary>
    public partial class CameraDisplayUnit : UserControl
    {
        public HWindow HalconWindow { get; private set; }

        public event Action<HWindow> HalconWindowReady;
        public HSmartWindowControlWPF HsmartWindowControl => HsmartWindow;

        public bool HMoveContent
        {
            get => HsmartWindow.HMoveContent;
            set => HsmartWindow.HMoveContent = value;
        }

        public CameraDisplayUnit()
        {
            InitializeComponent();
            HsmartWindow.HInitWindow += OnHInitWindow;
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
            DataContextChanged += OnDataContextChanged;
        }

        #region 触发入口

        private void OnHInitWindow(object sender, EventArgs e) => AcquireHalconWindow();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AcquireHalconWindow();
            UpdateDisplay();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                AcquireHalconWindow();   // 尺寸就绪时补一次，兜底 HInitWindow 早触发的场景
                UpdateDisplay();
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => AcquireHalconWindow();

        #endregion

        #region 窗口获取与注册
        /// <summary>
        ///访问HsmartWindow.HalconWindow, 只在IsLoaded且尺寸有效时才真正访问
        /// 已获取到有效窗口后直接返回。
        /// </summary>
        private void AcquireHalconWindow()
        {
            if (!IsLoaded) return;
            if (HsmartWindow == null) return;
            if (HsmartWindow.ActualWidth <= 0 || HsmartWindow.ActualHeight <= 0) return;

            try
            {
                var hw = HsmartWindow.HalconWindow;
                if (hw == null || !hw.IsInitialized()) return;

                //句柄没变就不广播
                if (ReferenceEquals(HalconWindow, hw)) return;

                HalconWindow = hw;
                TryRegisterHandle();
                HalconWindowReady?.Invoke(hw);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AcquireHalconWindow failed: {ex.Message}");
            }
        }

        private void TryRegisterHandle()
        {
            if (HalconWindow == null || !HalconWindow.IsInitialized()) return;

            string roleName = CameraRoleName;
            if (string.IsNullOrEmpty(roleName) && DataContext is CameraDisplayItem item)
                roleName = item.CameraRoleName;
            if (string.IsNullOrEmpty(roleName)) return;

            WindowHandleEvent.RaiseHandleRegistered(roleName, HalconWindow);
        }

        #endregion

        #region 依赖属性

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
            DependencyProperty.Register(nameof(CameraRoleName), typeof(string), typeof(CameraDisplayUnit),
                new PropertyMetadata(null, OnCameraRoleNameChanged));

        private static void OnCameraRoleNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((CameraDisplayUnit)d).AcquireHalconWindow();
        private static void OnHObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((CameraDisplayUnit)d).UpdateDisplay();
        #endregion


        #region 图像显示
        private void UpdateDisplay()
        {
            if (HalconWindow == null || !HalconWindow.IsInitialized()) return;

            try
            {
                HalconWindow.ClearWindow();

                if (DispImage != null && DispImage.IsInitialized())
                {
                    HOperatorSet.GetImageSize(DispImage, out HTuple width, out HTuple height);
                    SetPartKeepAspectRatio(HalconWindow, (int)width, (int)height);
                    HalconWindow.DispObj(DispImage);
                }

                if (DispRegion != null && DispRegion.IsInitialized() && DispRegion.CountObj() > 0)
                {
                    HalconWindow.SetColor("green");
                    HalconWindow.SetLineWidth(2);
                    HalconWindow.SetDraw("margin");
                    HalconWindow.DispObj(DispRegion);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateDisplay failed: {ex.Message}");
            }
        }
        private void SetPartKeepAspectRatio(HWindow hw, int imageWidth, int imageHeight)
        {
            double winWidth = HsmartWindow.ActualWidth;
            double winHeight = HsmartWindow.ActualHeight;
            double imgRatio = (double)imageWidth / imageHeight;
            double winRatio = winWidth / winHeight;

            double row1, col1, row2, col2;

            if (imgRatio > winRatio)
            {
                double dispHeight = imageWidth / winRatio;
                double offset = (dispHeight - imageHeight) / 2.0;
                row1 = -offset;
                col1 = 0;
                row2 = imageHeight - 1 + offset;
                col2 = imageWidth - 1;
            }
            else
            {
                double dispWidth = imageHeight * winRatio;
                double offset = (dispWidth - imageWidth) / 2.0;
                row1 = 0;
                col1 = -offset;
                row2 = imageHeight - 1;
                col2 = imageWidth - 1 + offset;
            }

            hw.SetPart((int)row1, (int)col1, (int)row2, (int)col2);
        }
        #endregion

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
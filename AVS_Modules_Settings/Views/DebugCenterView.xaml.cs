using System.Windows;
using System.Windows.Controls;

namespace AVS_Modules_Settings.Views
{
    /// <summary>
    /// DebugCenterView.xaml 的交互逻辑
    /// </summary>
    public partial class DebugCenterView : UserControl
    {
        public DebugCenterView()
        {
            InitializeComponent();
        }

        private void DebugTabs_Loaded(object sender, RoutedEventArgs e)
        {
            // 防止重复触发
            if (DebugTabs.Items.Count == 0) return;

            // 保存当前选中的 Tab，以便之后恢复
            int originalIndex = DebugTabs.SelectedIndex;

            DebugTabs.BeginInit();
            for (int i = 0; i < DebugTabs.Items.Count; i++)
            {
                // 切换到每个 Tab 并强制布局更新
                DebugTabs.SelectedIndex = i;
                DebugTabs.UpdateLayout();
            }
            // 恢复初始选中的 Tab
            DebugTabs.SelectedIndex = originalIndex;
            DebugTabs.EndInit();

            System.Diagnostics.Debug.WriteLine("[DebugCenter] 所有 Tab 已强制初始化完成。");
        }
    }
}

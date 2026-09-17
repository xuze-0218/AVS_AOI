using AVS_Modules_Settings.Models;
using AVS_Modules_Settings.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace AVS_Modules_Settings.Views
{
    /// <summary>
    /// ParameterConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class ParameterConfigView : UserControl
    {
        public ParameterConfigView()
        {
            InitializeComponent();
        }

        private void PoleCircle_Click(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            PoleCircleItem item = null;
            while (element != null)
            {
                if (element.DataContext is PoleCircleItem poleItem)
                {
                    item = poleItem;
                    break;
                }
                element = element.Parent as FrameworkElement;
            }

            if (item == null)
                return;

            var vm = DataContext as ParameterConfigViewModel;
            if (vm == null)
                return;
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                vm.BeginEditPole(item);
                e.Handled = true;
                return;
            }
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                vm.SelectedPole = item;
                e.Handled = true;
            }
        }

        private void PoleTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.Focus();
                tb.SelectAll();
            }
        }

        private void PoleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is PoleCircleItem item)
            {
                var vm = DataContext as ParameterConfigViewModel;
                vm?.CommitEditPole(item, tb.Text);
            }
        }

        private void PoleTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is PoleCircleItem item)
            {
                var vm = DataContext as ParameterConfigViewModel;
                if (e.Key == Key.Enter)
                {
                    vm?.CommitEditPole(item, tb.Text);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    vm?.CancelEditPole(item);
                    e.Handled = true;
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl)
            {
                // 判断当前选中的是否是产品参数 Tab（根据 Header 或索引）
                if (tabControl.SelectedItem is TabItem tabItem && tabItem.Header?.ToString() == "产品参数")
                {
                    var vm = DataContext as ParameterConfigViewModel;
                    vm?.OnProductParamTabActivated();
                }
            }
        }

        private void SetAsStart_Click(object sender, RoutedEventArgs e)
        {
            var item = ResolveContextMenuPoleItem(sender);
            if (item == null) return;
            (DataContext as ParameterConfigViewModel)?.SetAsStartCommand.Execute(item);
        }

        private void SetAsEnd_Click(object sender, RoutedEventArgs e)
        {
            var item = ResolveContextMenuPoleItem(sender);
            if (item == null) return;
            (DataContext as ParameterConfigViewModel)?.SetAsEndCommand.Execute(item);
        }

        /// <summary>
        /// 从 ContextMenu 的 PlacementTarget（那个 Grid）拿它绑定的 PoleCircleItem。
        /// ContextMenu 是独立可视树，不能直接用它自己的 DataContext 走 VM。
        /// </summary>
        private PoleCircleItem ResolveContextMenuPoleItem(object sender)
        {
            if (sender is MenuItem mi
                && mi.Parent is ContextMenu cm
                && cm.PlacementTarget is FrameworkElement fe)
            {
                return fe.DataContext as PoleCircleItem;
            }
            return null;
        }
    }
}

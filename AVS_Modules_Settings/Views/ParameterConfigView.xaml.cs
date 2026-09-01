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
    }
}

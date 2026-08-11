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

        // ParameterConfigView.xaml.cs
        private void PoleCircle_Click(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid;
            if (grid?.DataContext is PoleCircleItem item)
            {
                // 从 View 中找到 ViewModel
                var vm = DataContext as ParameterConfigViewModel;
                if (vm == null) return;

                if (e.ChangedButton == MouseButton.Left)
                {
                    // 左键：选中该圆
                    vm.SelectedPole = item;
                    e.Handled = true;
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    // 右键：弹出上下文菜单
                    vm.SelectedPole = item;
                    ShowPoleContextMenu(grid, item, vm);
                    e.Handled = true;
                }
            }
        }

        private void ShowPoleContextMenu(FrameworkElement target, PoleCircleItem item, ParameterConfigViewModel vm)
        {
            var menu = new ContextMenu();

            var markStart = new MenuItem { Header = "标记为起点" };
            markStart.Click += (s, e) => vm.MarkAsStartCommand.Execute();
            menu.Items.Add(markStart);

            var markEnd = new MenuItem { Header = "标记为终点" };
            markEnd.Click += (s, e) => vm.MarkAsEndCommand.Execute();
            menu.Items.Add(markEnd);

            menu.Items.Add(new Separator());

            var setNumber = new MenuItem { Header = "手动输入序号..." };
            setNumber.Click += (s, e) =>
            {
                string result = Microsoft.VisualBasic.Interaction.InputBox(
                    "请输入该位置的极柱号:", "输入极柱号", item.PoleNumber?.ToString() ?? "");
                if (int.TryParse(result, out int num))
                {
                    item.PoleNumber = num;
                }
            };
            menu.Items.Add(setNumber);

            var clear = new MenuItem { Header = "清除该圆" };
            clear.Click += (s, e) => { item.PoleNumber = null; };
            menu.Items.Add(clear);

            menu.IsOpen = true;
        }
    }
}

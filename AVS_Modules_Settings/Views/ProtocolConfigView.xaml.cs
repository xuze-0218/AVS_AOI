using AVS_Common.Model;
using AVS_Modules_Settings.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AVS_Modules_Settings.Views
{
    /// <summary>
    /// ProtocolConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolConfigView : UserControl
    {
        public ProtocolConfigView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 删除输入字段
        /// </summary>
        private void OnDeleteInputField(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var field = button?.DataContext as ProtocolField;

                if (field != null && this.DataContext is ProtocolConfigViewModel viewModel)
                {
                    viewModel.InputFields.Remove(field);
                    viewModel.StatusMessage = "已删除输入字段";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 删除输出字段
        /// </summary>
        private void OnDeleteOutputField(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var field = button?.DataContext as ProtocolField;

                if (field != null && this.DataContext is ProtocolConfigViewModel viewModel)
                {
                    viewModel.OutputFields.Remove(field);
                    viewModel.StatusMessage = "已删除输出字段";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

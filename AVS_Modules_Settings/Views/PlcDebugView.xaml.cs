using System.Windows.Controls;

namespace AVS_Modules_Settings.Views
{
    /// <summary>
    /// PlcDebugView.xaml 的交互逻辑
    /// </summary>
    public partial class PlcDebugView : UserControl
    {
        public PlcDebugView()
        {
            InitializeComponent();
        }

        private void ReceivedTextLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.ScrollToEnd();
            }
        }
    }
}

using AVS_Common;
using AVS_Core.Models;
using System.Windows;

namespace AVS_App.Views
{
    public partial class LoginWindow : Window
    {
        public UserRole SelectedRole { get; private set; } = UserRole.Operator;
        public bool LoginSuccess { get; private set; } = false;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

            if (username == "engineer" && password == "123")
            {
                SelectedRole = UserRole.Engineer;
                LoginSuccess = true;
                Close();
            }
            else if (username == "operator" && password == "1")
            {
                SelectedRole = UserRole.Operator;
                LoginSuccess = true;
                Close();
            }
            else
            {
                MessageBox.Show("用户名或密码错误", "登录失败", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            LoginSuccess = false;
            Close();
        }
    }
}
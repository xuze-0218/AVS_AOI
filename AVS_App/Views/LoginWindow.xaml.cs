using AVS_App.ViewModels;
using AVS_Common;
using AVS_Core.Models;
using AVS_Service.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace AVS_App.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginWindowViewModel vm)
            {
                vm.CloseAction = Close;
                await vm.AutoLoginAsync();
            }
        }

        private void DragWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
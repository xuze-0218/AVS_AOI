using AVS_Common.Events;
using AVS_Service;
using AVS_Service.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AVS_App.ViewModels
{
    public class LoginWindowViewModel : BindableBase
    {
        private readonly ILoginCredentialService _credentialService;
        private readonly IEventAggregator _eventAggregator;
        private bool _isAutoLoginTried = false; // 防止重复自动登录
        public List<string> UserNames { get; } = new List<string> { "operator", "engineer" };

        private string _selectedUserName;
        public string SelectedUserName
        {
            get => _selectedUserName;
            set
            {
                if (SetProperty(ref _selectedUserName, value))
                {
                    LoginParams.UserName = value;
                    // 根据用户名自动确定角色
                    SelectedRole = value == "operator" ? UserRole.Operator : UserRole.Engineer;
                }
            }
        }

        private LoginParams _loginParams = new LoginParams();
        public LoginParams LoginParams
        {
            get => _loginParams;
            set => SetProperty(ref _loginParams, value);
        }

        private bool _isUIEnabled = true;
        public bool IsUIEnabled
        {
            get => _isUIEnabled;
            set => SetProperty(ref _isUIEnabled, value);
        }

        private string _loginButtonText = "登 录";
        public string LoginButtonText
        {
            get => _loginButtonText;
            set => SetProperty(ref _loginButtonText, value);
        }

        private bool _loginSuccess;
        public bool LoginSuccess
        {
            get => _loginSuccess;
            private set => SetProperty(ref _loginSuccess, value);
        }

        private UserRole _selectedRole;
        public UserRole SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public Action CloseAction { get; set; }

        public DelegateCommand LoginCommand { get; }
        public DelegateCommand ExitCommand { get; }

        public LoginWindowViewModel(ILoginCredentialService credentialService, IEventAggregator eventAggregator)
        {
            _credentialService = credentialService;
            _eventAggregator = eventAggregator;
            LoginCommand = new DelegateCommand(async () => await ExecuteLoginAsync());
            ExitCommand = new DelegateCommand(ExecuteExit);

            LoadSavedCredentials();
        }

        private void LoadSavedCredentials()
        {
            var creds = _credentialService.LoadCredentials();
            // 设置用户名下拉框选中项
            if (!string.IsNullOrEmpty(creds.Username) && UserNames.Contains(creds.Username))
            {
                SelectedUserName = creds.Username;
            }
            else
            {
                SelectedUserName = UserNames.FirstOrDefault();
            }
            LoginParams.Password = creds.Password;
            LoginParams.IsRememberPassword = creds.RememberPassword;
            LoginParams.IsAutoLogin = creds.AutoLogin;
        }

        public async Task AutoLoginAsync()
        {
            // 仅当用户勾选了自动登录且未尝试过时执行
            if (_isAutoLoginTried || !LoginParams.IsAutoLogin)
                return;

            _isAutoLoginTried = true;
            if (string.IsNullOrWhiteSpace(LoginParams.UserName) ||
                string.IsNullOrWhiteSpace(LoginParams.Password))
            {
                ShowError("自动登录失败：用户名或密码为空");
                return;
            }
            await ExecuteLoginAsync();
        }

        private async Task ExecuteLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(LoginParams.UserName) ||
                string.IsNullOrWhiteSpace(LoginParams.Password))
            {
                ShowError("用户名和密码不能为空");
                return;
            }

            IsUIEnabled = false;
            LoginButtonText = "登录中...";
            try
            {
                bool success = ValidateLogin(LoginParams.UserName, LoginParams.Password, SelectedRole);

                if (success)
                {
                    LoginSuccess = true;
                    string roleText = SelectedRole == UserRole.Operator ? "操作员" : "工程师";
                    string currentUser = $"{LoginParams.UserName}（{roleText}）";
                    bool isEngineer = SelectedRole == UserRole.Engineer;

                    // 更新静态会话
                    UserSession.CurrentUser = currentUser;
                    UserSession.IsEngineer = isEngineer;

                    // 发布事件，通知主窗体更新
                    _eventAggregator.GetEvent<LoginSuccessEvent>().Publish(new LoginSuccessInfo
                    {
                        CurrentUser = currentUser,
                        IsEngineer = isEngineer
                    });

                    // 保存或清除凭据
                    if (LoginParams.IsRememberPassword || LoginParams.IsAutoLogin)
                    {
                        _credentialService.SaveCredentials(
                            LoginParams.UserName,
                            LoginParams.Password,
                            SelectedRole,
                            LoginParams.IsRememberPassword,
                            LoginParams.IsAutoLogin);
                    }
                    else
                    {
                        _credentialService.ClearAllCredentials();
                    }

                    CloseAction?.Invoke(); // 关闭登录窗口
                }
                else
                {
                    LoginSuccess = false;
                    ShowError("用户名、密码或角色错误");
                }
            }
            finally
            {
                IsUIEnabled = true;
                LoginButtonText = "登 录";
            }
        }

        private void ExecuteExit()
        {
            CloseAction?.Invoke();
        }

        private bool ValidateLogin(string username, string password, UserRole role)
        {
            //目前硬编码
            if (role == UserRole.Operator)
                return username == "operator" && password == "1";
            else
                return username == "engineer" && password == "123";
        }

        private void ShowError(string message)
        {
            LoginParams.AlertInfo = message;
            LoginParams.IsAlertVisible = Visibility.Visible;
        }
    }

    public class LoginParams : BindableBase
    {
        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _isRememberPassword;
        public bool IsRememberPassword
        {
            get => _isRememberPassword;
            set => SetProperty(ref _isRememberPassword, value);
        }

        private bool _isAutoLogin;
        public bool IsAutoLogin
        {
            get => _isAutoLogin;
            set => SetProperty(ref _isAutoLogin, value);
        }

        private string _alertInfo;
        public string AlertInfo
        {
            get => _alertInfo;
            set => SetProperty(ref _alertInfo, value);
        }

        private Visibility _isAlertVisible = Visibility.Collapsed;
        public Visibility IsAlertVisible
        {
            get => _isAlertVisible;
            set => SetProperty(ref _isAlertVisible, value);
        }
    }
}
using AVS_Service.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Services
{
    /// <summary>
    /// 用户登录凭证服务
    /// </summary>
    public interface ILoginCredentialService
    {
        void SaveCredentials(string username, string password, UserRole role, bool rememberPassword, bool autoLogin);
        LoginCredentials LoadCredentials();
        void UpdatePassword(string newPassword);
        void DeletePassword();
        void ClearAllCredentials();
    }


    public class LoginCredentialService : ILoginCredentialService
    {
        private const string PasswordFileName = "cred.bin";

        public void SaveCredentials(string username, string password, UserRole role, bool rememberPassword, bool autoLogin)
        {
            Properties.Settings.Default.LastUsername = username;
            Properties.Settings.Default.LastRole = role.ToString();
            Properties.Settings.Default.RememberPassword = rememberPassword;
            Properties.Settings.Default.AutoLogin = autoLogin;
            Properties.Settings.Default.Save();

            if (!string.IsNullOrEmpty(password))
            {
                byte[] encrypted = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(password),
                    null,
                    DataProtectionScope.CurrentUser);
                File.WriteAllBytes(GetPasswordFilePath(), encrypted);
            }
            else
            {
                DeletePassword();
            }
        }

        public LoginCredentials LoadCredentials()
        {
            var creds = new LoginCredentials
            {
                Username = Properties.Settings.Default.LastUsername,
                Role = Enum.TryParse(Properties.Settings.Default.LastRole, out UserRole r) ? r : UserRole.Operator,
                RememberPassword = Properties.Settings.Default.RememberPassword,
                AutoLogin = Properties.Settings.Default.AutoLogin
            };

            string filePath = GetPasswordFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    byte[] encrypted = File.ReadAllBytes(filePath);
                    byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                    creds.Password = Encoding.UTF8.GetString(decrypted);
                }
                catch
                {
                    File.Delete(filePath);
                    creds.Password = null;
                }
            }

            return creds;
        }

        public void UpdatePassword(string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                DeletePassword();
                return;
            }
            byte[] encrypted = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(newPassword),
                null,
                DataProtectionScope.CurrentUser);
            File.WriteAllBytes(GetPasswordFilePath(), encrypted);
        }

        public void DeletePassword()
        {
            string filePath = GetPasswordFilePath();
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public void ClearAllCredentials()
        {
            DeletePassword();
            Properties.Settings.Default.LastUsername = string.Empty;
            Properties.Settings.Default.LastRole = UserRole.Operator.ToString();
            Properties.Settings.Default.RememberPassword = false;
            Properties.Settings.Default.AutoLogin = false;
            Properties.Settings.Default.Save();
        }

        private string GetPasswordFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, PasswordFileName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Models
{
    public class LoginCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public bool RememberPassword { get; set; }
        public bool AutoLogin { get; set; }
    }
}

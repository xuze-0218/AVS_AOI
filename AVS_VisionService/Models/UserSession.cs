using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Models
{
    public static class UserSession
    {
        public static string CurrentUser { get; set; } = "未登录";
        public static bool IsEngineer { get; set; }
    }
}

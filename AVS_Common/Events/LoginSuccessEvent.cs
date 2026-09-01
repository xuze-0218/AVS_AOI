using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Common.Events
{
    public class LoginSuccessEvent : PubSubEvent<LoginSuccessInfo>
    {
    }

    public class LoginSuccessInfo
    {
        public string CurrentUser { get; set; }
        public bool IsEngineer { get; set; }
    }
}

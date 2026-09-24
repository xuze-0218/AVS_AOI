using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Common.Events
{

    /// <summary>
    /// 应用初始化完成事件
    /// </summary>
    public class ApplicationStartupCompletedEvent : PubSubEvent<bool> { }
}


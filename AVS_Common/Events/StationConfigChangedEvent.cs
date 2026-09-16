using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Common.Events
{
    /// <summary>
    /// 工位配置变更事件，用来刷新显示界面上的相机布局和配置
    /// </summary>
    public class StationConfigChangedEvent: PubSubEvent
    {
    }

    /// <summary>
    /// AI模型配置变更事件，用来重新加载AI模型
    /// </summary>
    public class AiModelConfigChangedEvent : PubSubEvent<string> 
    {

    }


}

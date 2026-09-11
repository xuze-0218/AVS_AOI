using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Events
{
    public class PoleResultEvent : PubSubEvent<PoleResultPayload> { }

    /// <summary>
    /// 当前极柱检测结果的负载数据
    /// </summary>
    public class PoleResultPayload
    {
        public int PoleNum { get; set; }
        public bool IsOK { get; set; }
        public double DetectTimeMs { get; set; } 
    }
}

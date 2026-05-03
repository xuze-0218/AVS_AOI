using AVS_Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Models
{
    public class SessionConfig
    {
        public string FuncCode { get; set; }
        public string Description { get; set; }
        public List<ProtocolField> InputFields { get; set; } = new List<ProtocolField>();
        public List<ProtocolField> OutputFields { get; set; } = new List<ProtocolField>();
    }

    // 整个工位的配置集合
    public class StationConfig
    {
        /// <summary>
        /// 工位
        /// </summary>
        public string StationId { get; set; }
        public List<SessionConfig> Messages { get; set; } = new List<SessionConfig>();
    }
}

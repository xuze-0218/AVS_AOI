using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
namespace AVS_Common.Events
{
    public class InspectionResultEvent : PubSubEvent<InspectionResultPayload> { }

    public class InspectionResultPayload
    {
        public string StationId { get; set; }
        public int PoleNum { get; set; }
        public bool IsOK { get; set; }       
        public DateTime DetectTime { get; set; }
        public string WorkType { get; set; }
        public string ModuleName { get; set; }
    }
}

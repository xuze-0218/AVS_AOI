using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Models
{
    public class InspectionInitData
    {
        public string ModuleName { get; set; }
        public int StartPole { get; set; }
        public int EndPole { get; set; }
        public int[] InspectOrder { get; set; }
        public int MsgPoleCapacity { get; set; } = 25; // 报文每次发送极柱个数
    }

    public class CalibrationInitData
    {
        public string ModuleName { get; set; }
        public string Type { get; set; } // "03" 标定 / "04" 点检
    }
}

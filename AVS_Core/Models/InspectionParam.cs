using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Models
{
    public class InspectionParams
    {
        public double Score { get; set; }
        public bool IsAiCheck { get; set; }
        // 其他运行时参数
    }

    public class CalibrationParams
    {
        public string ParamDir { get; set; }
        public double ResoX { get; set; }
        public double ResoY { get; set; }
        // ...
    }
}

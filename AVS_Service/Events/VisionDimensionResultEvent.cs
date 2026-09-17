using AVS_Service.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Events
{
    public class VisionDimensionResultEvent : PubSubEvent<VisionDimensionResultPayload> { }

    /// <summary>
    /// 当前维度检测结果的负载数据
    /// </summary>
    public class VisionDimensionResultPayload
    {
        public int PoleNum { get; set; }
        public Result DimensionResult { get; set; }
        public double DetectTimeMs { get; set; }
        public VisionDimension Dimension { get; set; }
    }
}

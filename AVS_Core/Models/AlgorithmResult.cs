using HalconDotNet;

namespace AVS_Core.Models
{
    public enum InspectStatus { OK, NG, Error }

    /// <summary>
    /// 算法结果类，包含算法执行状态、消息、耗时，以及用于显示的Region/XLD和数值结果
    /// </summary>
    public class AlgorithmResult
    {
        public InspectStatus Status { get; set; } = InspectStatus.OK;
        public string Message { get; set; } = string.Empty;
        public double ElapsedTime { get; set; }
        /// <summary>
        /// 用于显示的Region\XLD结果
        /// </summary>
        public Dictionary<string, HObject> HDispRegions { get; set; } = new Dictionary<string, HObject>();
        /// <summary>
        /// 测量结果数值
        /// </summary>
        public Dictionary<string, double> NumericResults { get; set; } = new Dictionary<string, double>();

        public void Dispose()
        {
            foreach (var item in HDispRegions)
            {
                item.Value.Dispose();
            }
            HDispRegions.Clear();
            //NumericResults.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Models
{
    public sealed class RateStatistics
    {
        public string Name { get; set; }
        public int Total { get; set; }
        public int OkCount { get; set; }
        public int NgCount { get; set; }
        public bool IsSummary { get; set; }
        public double OkRate => Total == 0 ? 0 : OkCount * 100.0 / Total;
        public double NgRate => Total == 0 ? 0 : NgCount * 100.0 / Total;
    }

    public static class StatisticsCalculator
    {
        public static RateStatistics Compute<T>(string name, IEnumerable<T> rows,
            Func<T, Result> selector, bool isSummary)
        {
            int total = 0, ok = 0, ng = 0;
            foreach (var r in rows)
            {
                var res = selector(r);
                if (res == Result.None) continue;
                total++;
                if (res == Result.OK) ok++;
                else if (res == Result.NG) ng++;
            }
            return new RateStatistics
            {
                Name = name,
                Total = total,
                OkCount = ok,
                NgCount = ng,
                IsSummary = isSummary
            };
        }
    }
}

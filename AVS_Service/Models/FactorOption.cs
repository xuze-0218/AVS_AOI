using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Models
{
    /// <summary>
    /// 查询筛选因子
    /// </summary>
    public sealed class FactorOption
    {
        public string Name { get; private set; }
        public VisionDimension DataType { get; private set; }

        public BarShape Shape { get; private set; }
        public Func<InspectResult2DData, Result> Result2D { get; private set; }
        public Func<InspectResult2DData, double> Value2D { get; private set; }
        public Func<InspectResult3DData, Result> Result3D { get; private set; }
        public Func<InspectResult3DData, double> Value3D { get; private set; }

        private FactorOption() { }

        public static FactorOption For2D(string name, BarShape shape,
            Func<InspectResult2DData, Result> result,
            Func<InspectResult2DData, double> value)
            => new FactorOption
            {
                Name = name,
                DataType = VisionDimension.TwoD,
                Shape = shape,
                Result2D = result,
                Value2D = value
            };

        public static FactorOption For3D(string name, BarShape shape,
            Func<InspectResult3DData, Result> result,
            Func<InspectResult3DData, double> value)
            => new FactorOption
            {
                Name = name,
                DataType = VisionDimension.ThreeD,
                Shape = shape,
                Result3D = result,
                Value3D = value
            };
    }
}

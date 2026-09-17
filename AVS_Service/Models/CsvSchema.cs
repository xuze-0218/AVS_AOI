using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Models
{
    public sealed class CsvSchema
    {
        public string Name { get; set; }
        public VisionDimension DataType { get; set; }
        public BarShape Shape { get; set; }
        public string[] Headers { get; set; }
        public IReadOnlyList<FactorOption> Factors { get; set; }
    }

    public static class CsvSchemaRegistry
    {
        public static readonly CsvSchema TwoDCircle = new CsvSchema()
        {
            Name = "2D-圆形",
            DataType = VisionDimension.TwoD,
            Shape = BarShape.Circle,
            Headers = InspectionCsvHeaders.Get2DDetailHeader(false),
            Factors = new[]
            {
            FactorOption.For2D("2D 长度", BarShape.Circle, r => r.ResultLength,       r => r.Length),
            FactorOption.For2D("2D 宽度", BarShape.Circle, r => r.ResultWidth,        r => r.Width),
            FactorOption.For2D("2D 偏移", BarShape.Circle, r => r.ResultOffset,       r => r.Offset),
            FactorOption.For2D("2D 爆孔", BarShape.Circle, r => r.ResultPoreBreak,    r => r.PoreBreakArea),
            FactorOption.For2D("2D 外径", BarShape.Circle, r => r.ResultBeadDiameter, r => r.BeadDiameter),
            FactorOption.For2D("2D 虚焊", BarShape.Circle, r => r.ResultfaultySol,    r => r.faultySol)
        }
        };

        public static readonly CsvSchema TwoDSquare = new CsvSchema()
        {
            Name = "2D-方形",
            DataType = VisionDimension.TwoD,
            Shape = BarShape.SquareBar,
            Headers = InspectionCsvHeaders.Get2DDetailHeader(true),
            Factors = new[]
            {
            FactorOption.For2D("2D 长度",     BarShape.SquareBar, r => r.ResultLength,       r => r.Length),
            FactorOption.For2D("2D 方形宽度", BarShape.SquareBar, r => r.ResultWidth,        r => r.Width),
            FactorOption.For2D("2D 条形宽度", BarShape.SquareBar, r => r.ResultOffset,       r => r.Offset),
            FactorOption.For2D("2D 间距",     BarShape.SquareBar, r => r.ResultPoreBreak,    r => r.PoreBreakArea),
            FactorOption.For2D("2D 爆孔",     BarShape.SquareBar, r => r.ResultBeadDiameter, r => r.BeadDiameter),
            FactorOption.For2D("2D 虚焊",     BarShape.SquareBar, r => r.ResultfaultySol,    r => r.faultySol)
        }
        };

        public static readonly CsvSchema ThreeDCircle = new CsvSchema()
        {
            Name = "3D-圆形",
            DataType = VisionDimension.ThreeD,
            Shape = BarShape.Circle,
            Headers = InspectionCsvHeaders.Get3DDetailHeader(false),
            Factors = new[]
            {
            FactorOption.For3D("3D 余高", BarShape.Circle, r => r.ResultBeadHump, r => r.BeadHump),
            FactorOption.For3D("3D 下塌", BarShape.Circle, r => r.ResultBeadSag,  r => r.BeadSag)
        }
        };

        public static readonly CsvSchema ThreeDSquare = new CsvSchema()
        {
            Name = "3D-方形",
            DataType = VisionDimension.ThreeD,
            Shape = BarShape.SquareBar,
            Headers = InspectionCsvHeaders.Get3DDetailHeader(true),
            Factors = new[]
            {
            FactorOption.For3D("3D 方形余高", BarShape.SquareBar, r => r.ResultBeadHump,    r => r.BeadHump),
            FactorOption.For3D("3D 方形下塌", BarShape.SquareBar, r => r.ResultBeadSag,     r => r.BeadSag),
            FactorOption.For3D("3D 条形余高", BarShape.SquareBar, r => r.ResultBarBeadHump, r => r.BarBeadHump),
            FactorOption.For3D("3D 条形下塌", BarShape.SquareBar, r => r.ResultBarBeadSag,  r => r.BarBeadSag)
        }
        };

        public static readonly IReadOnlyList<CsvSchema> All = new[] { TwoDCircle, TwoDSquare, ThreeDCircle, ThreeDSquare };

        public static CsvSchema Get(VisionDimension type, BarShape shape)
            => All.FirstOrDefault(s => s.DataType == type && s.Shape == shape);

        public static CsvSchema MatchHeader(string[] header)
        {
            if (header == null || header.Length == 0) return null;
            foreach (var s in All)
            {
                if (header.Length < s.Headers.Length) continue;
                bool ok = true;
                for (int i = 0; i < s.Headers.Length; i++)
                {
                    string h = NormalizeCell(header[i], i == 0);
                    if (!string.Equals(h, s.Headers[i], StringComparison.Ordinal)) { ok = false; break; }
                }
                if (ok) return s;
            }
            return null;
        }

        private static string NormalizeCell(string v, bool trimBom)
        {
            string s = (v ?? string.Empty).Trim().Trim('"');
            return trimBom ? s.TrimStart('\uFEFF') : s;
        }
    }
}

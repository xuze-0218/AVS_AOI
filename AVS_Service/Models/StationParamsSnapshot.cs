using System.ComponentModel;

namespace AVS_Service.Models
{
    public class StationParamsSnapshot
    {
        [Description("是否开启传统算法检测")]
        public bool IsNormalCheck { get; set; }

        [Description("是否启用 AI 检测")]
        public bool IsAiCheck { get; set; } = true;

        [Description("图像是否旋转 180°")]
        public bool IsRotated { get; set; }

        // ===== 产品类型 =====
        [Description("方条焊缝标志")]
        public bool IsSquareBarWeldMark { get; set; }

        [Description("圆形焊缝标志")]
        public bool IsCirWeldMark { get; set; } = true;

        [Description("3D 分割模式")]
        public bool Is3DSegmentation { get; set; }

        [Description("AI 置信度阈值")]
        public double ScoreValue { get; set; } = 0.8;

        [Description("平面拟合模式")]
        public bool IsPlaneCheck { get; set; }

        [Description("X 方向像素物理系数")]
        public double Fx { get; set; }

        [Description("Y 方向像素物理系数")]
        public double Fy { get; set; }

        [Description("Z 方向物理系数")]
        public double Fz { get; set; }

        [Description("配方路径")]
        public string RecipePath { get; set; } = "";

        [Description("标定块基准中心 X")]
        public double CornerX01 { get; set; }

        [Description("标定块基准中心 Y")]
        public double CornerY01 { get; set; }

        [Description("标定块基准高度 Z")]
        public double CornerZ01 { get; set; }

        [Description("标定块点检位置偏差阈值（像素）")]
        public double PsnTolerance { get; set; } = 0.5;

        [Description("模板匹配起始角度")]
        public double AngleStart { get; set; } = 0;

        [Description("模板匹配角度范围")]
        public double AngleExtent { get; set; } = 360;

        [Description("最小缩放比例")]
        public double MinScale { get; set; } = 0.9;

        [Description("最大缩放比例")]
        public double MaxScale { get; set; } = 1.1;

        [Description("最低匹配分数")]
        public double MinScore { get; set; } = 0.5;

        [Description("最大匹配数量")]
        public int MaxMatchNum { get; set; } = 1;

        [Description("最大重叠率")]
        public double MaxOverlap { get; set; } = 0.5;

        [Description("金字塔层数")]
        public int NumLevel { get; set; }

        [Description("贪婪度")]
        public double Greediness { get; set; } = 0.9;

        [Description("亚像素精度模式")]
        public string SubPixel { get; set; } = "least_squares";

        [Description("检测模型路径")]
        public string DetModelPath { get; set; } = "";

        [Description("分割模型路径（多个用分号分隔）")]
        public string SegModelPaths { get; set; } = "";
    }


    public struct InspectOrder
    {
        public int Row;//排数
        public int Col;//列数
        public int[] Start;
        public int[] End;
        public bool[] RowReversed { get; set; }   //true = 本行从右往左（起点在右端）
    }
}

namespace AVS_Service.Models
{
    public class StationParamsSnapshot
    {
        // ===== 检测方法 =====
        public bool IsNormalCheck { get; set; }
        public bool IsAiCheck { get; set; }
        public bool IsRotated { get; set; }

        // ===== 产品类型 =====
        public bool IsSquareBarWeldMark { get; set; }
        public bool IsCirWeldMark { get; set; }

        // ===== AI 阈值 =====
        public double ScoreValue { get; set; } = 0.8;

        // ===== 3D 模式 =====
        public bool IsPlaneCheck { get; set; }

        // ===== 标定参数 =====
        public double Fx { get; set; }
        public double Fy { get; set; }
        public double Fz { get; set; }

        // ===== 配方路径 =====
        public string RecipePath { get; set; } = "";

        //==== 模板匹配参数 =====
        public double AngleStart { get; set; } = 0;
        public double AngleExtent { get; set; } = 360;
        public double MinScale { get; set; } = 0.9;
        public double MaxScale { get; set; } = 1.1;
        public double MinScore { get; set; } = 0.5;
        public int MaxMatchNum { get; set; } = 1;
        public double MaxOverlap { get; set; } = 0.5;
        public int NumLevel { get; set; }
        public double Greediness { get; set; } = 0.9;
        public string SubPixel { get; set; } = "least_squares";

        // ===== AI 模型路径 =====
        public string DetModelPath { get; set; } = "";
        public string SegModelPaths { get; set; } = "";
    }


    public struct InspectOrder
    {
        public int Row;//排数
        public int Col;//列数
        public int[] Start;
        public int[] End;
    }
}

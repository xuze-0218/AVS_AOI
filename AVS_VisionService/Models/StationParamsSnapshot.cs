namespace AVS_Service.Models
{
    public class StationParamsSnapshot
    {
        // ===== 检测方法 =====
        public bool IsNormalCheck { get; init; }
        public bool IsAiCheck { get; init; }
        public bool IsRotated { get; init; }

        // ===== 产品类型 =====
        public bool IsSquareBarWeldMark { get; init; }
        public bool IsCirWeldMark { get; init; }

        // ===== AI 阈值 =====
        public double ScoreValue { get; init; } = 0.8;

        // ===== 3D 模式 =====
        public bool IsPlaneCheck { get; init; }

        // ===== 标定参数 =====
        public double Fx { get; init; }
        public double Fy { get; init; }
        public double Fz { get; init; }

        // ===== 配方路径 =====
        public string RecipePath { get; init; } = "";

        //==== 模板匹配参数 =====
        public double AngleStart;   //起始角度__deg
        public double AngleExtent;  //角度范围__deg
        public double MinScale;     //最小缩放
        public double MaxScale;     //最大缩放
        public double MinScore;     //最小分数
        public int MaxMatchNum;     //最大匹配数目
        public double MaxOverlap;   //最大重叠
        public int NumLevel;        //最大金子塔层级
        public double Greediness;   //贪婪度
        public string SubPixel;     //亚像素

        // ===== AI 模型路径 =====
        public string DetModelPath { get; init; } = "";
        public string SegModelPaths { get; init; } = "";
    }


    public struct InspectOrder
    {
        public int Row;//排数
        public int Col;//列数
        public int[] Start;
        public int[] End;
    }
}

namespace AVS_Core.Models
{
    public class InspectionParams
    {
        public double Score { get; set; }
        public bool IsAiCheck { get; set; }

        /// <summary>
        /// 模组名称（用于综合检测CSV的模组码列）
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// 工作模式（检测/标定/点检）
        /// </summary>
        public string WorkType { get; set; }
    }

    public class CalibrationParams
    {
        public string ParamDir { get; set; }
        public double ResoX { get; set; }
        public double ResoY { get; set; }
    }

    public class InspectionInitParams
    {
        /// <summary>
        /// 模组名称（用于图像保存）
        /// </summary>
        public string ImageName { get; set; }
        /// <summary>
        /// 存储拍照顺序对应的物理极柱号 例如：1-52
        /// </summary>
        public int[] PoleOrder { get; set; }
        /// <summary>
        /// 单次报文容量（10 或 25）
        /// </summary>
        public int MsgPoleCapacity { get; set; }
    }
}

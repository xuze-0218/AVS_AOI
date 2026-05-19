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
        /// 拍照顺序（物理编号数组）
        /// </summary>
        public int[] PoleOrder { get; set; }
        /// <summary>
        /// 单次报文容量（10 或 25）
        /// </summary>
        public int MsgPoleCapacity { get; set; }
    }
}

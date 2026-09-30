using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Modules_Settings.Models
{
    /// <summary>卡尺几何拟合结果项：每个卡尺对象一行</summary>
    public class GeometryResultItem
    {
        public int Index { get; set; }
        /// <summary>circle / ellipse / rectangle2 / line</summary>
        public string ObjectType { get; set; }
        public double CenterRow { get; set; }
        public double CenterCol { get; set; }
        /// <summary>圆：半径；椭圆：主轴；矩形：长；直线：长度</summary>
        public double Size1 { get; set; }
        /// <summary>椭圆：次轴；矩形：宽</summary>
        public double Size2 { get; set; }
        public double AngleDeg { get; set; }
        /// <summary>拟合残差 RMS（越小越贴合，用于判断边缘质量）</summary>
        public double RMS { get; set; }

        public string TypeLabel
        {
            get
            {
                if (ObjectType == null)
                    return ObjectType;

                var typeLower = ObjectType.ToLower();
                switch (typeLower)
                {
                    case "circle":
                        return "圆";
                    case "ellipse":
                        return "椭圆";
                    case "rectangle2":
                    case "rectangle":
                        return "矩形";
                    case "line":
                        return "直线";
                    default:
                        return ObjectType;
                }
            }
        }

    }
}

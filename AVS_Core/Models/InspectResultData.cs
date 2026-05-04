using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Models
{
    public enum Result
    {
        [Description("正常")]
        OK,
        [Description("不正常")]
        NG,
        [Description("无结果")]
        None
    }

    public class InspectResult3DData
    {


        //工作模式
        public string WorkType { get; set; }

        //模组码
        public string ModuleName { get; set; }

        //极柱号
        public int PoleNum { get; set; }

        //检测时间
        public DateTime DateTime { get; set; }

        //总结果
        public Result ResultSummary { get; set; }

        //3D结果
        public Result Result3D { get; set; }

        //余高结果
        public Result ResultBeadHump { get; set; }

        //下榻结果
        public Result ResultBeadSag { get; set; }

        //余高值
        public double BeadHump { get; set; }

        //下榻值
        public double BeadSag { get; set; }

        //3D原图
        public string Orn3DPath { get; set; }

        //3D结果图
        public string Dump3DPath { get; set; }


    }

    public class InspectResult2DData
    {

        //工作模式
        public string WorkType { get; set; }

        //模组码
        public string ModuleName { get; set; }

        //极柱号
        public int PoleNum { get; set; }

        //检测时间
        public DateTime DateTime { get; set; }

        //总结果
        public Result ResultSummary { get; set; }

        //2D结果
        public Result Result2D { get; set; }

        //长度结果
        public Result ResultLength { get; set; }

        //宽度结果
        public Result ResultWidth { get; set; }

        //偏移结果
        public Result ResultOffset { get; set; }

        //爆孔结果
        public Result ResultPoreBreak { get; set; }

        //外径结果
        public Result ResultBeadDiameter { get; set; }

        //虚焊结果
        public Result ResultfaultySol { get; set; }

        //长度值
        public double Length { get; set; }

        //宽度值
        public double Width { get; set; }

        //偏移值
        public double Offset { get; set; }

        //爆孔面积
        public double PoreBreakArea { get; set; }

        //外径值
        public double BeadDiameter { get; set; }

        //虚焊值
        public double faultySol { get; set; }


        //2D原图
        public string Orn2DPath { get; set; }

        //2D结果图
        public string Dump2DPath { get; set; }




    }
}

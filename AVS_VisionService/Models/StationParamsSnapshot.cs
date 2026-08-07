using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Models
{
    public class StationParamsSnapshot
    {
        public bool IsNormalCheck { get; init; }
        public bool IsAiCheck { get; init; }
        public bool IsRotated { get; init; }
        public bool IsSquareBarWeldMark { get; init; }
        public bool IsCirWeldMark { get; init; }
        public double ScoreValue { get; init; } = 0.8;
        public bool IsPlaneCheck { get; init; }
        public double Fx { get; init; }
        public double Fy { get; init; }
        public double Fz { get; init; }
        public string RecipePath { get; init; } = "";
        public string ImageSaveDir { get; init; } = "";
        public bool IsSaveOrnImg { get; init; }
        public bool IsSaveOkRenImg { get; init; }
        public bool IsSaveNgRenImg { get; init; }
        public int SaveOrnImgDays { get; init; } = 30;
        public int SaveRenImgDays { get; init; } = 30;
        public string[] DetModelPaths { get; init; } = Array.Empty<string>();
        public string[] SegModelPaths { get; init; } = Array.Empty<string>();
        public InspectOrder[] InspectOrders { get; init; } = Array.Empty<InspectOrder>();
    }


    public struct ModelSearch_Param
    {
        public double angleStart;   //起始角度__deg
        public double angleExtent;  //角度范围__deg
        public double minScale;     //最小缩放
        public double maxScale;     //最大缩放
        public double minScore;     //最小分数
        public int maxMatchNum;     //最大匹配数目
        public double maxOverlap;   //最大重叠
        public int numLevel;        //最大金子塔层级
        public double greediness;   //贪婪度
        public string subPixel;     //亚像素
    }

    public struct InspectOrder
    {
        public int row;//排数
        public int col;//列数
        [JsonIgnore]
        public int[] start;
        [JsonIgnore]
        public int[] end;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Models
{
    public static class InspectionCsvHeaders
    {
        // ---------- 2D 明细表头 ----------
        public static string[] Get2DDetailHeader(bool isSquareBar)
        {
            return isSquareBar ? SquareBar2DDetail : Circle2DDetail;
        }

        private static readonly string[] Circle2DDetail = new[]
        {
            "模组码值","极柱序号","检测结果","长度结果","焊缝长度",
            "宽度结果","焊缝宽度","偏移结果","焊缝偏移","爆孔结果",
            "焊缝爆孔","外径结果","焊缝外径","虚焊结果","焊缝虚焊"
        };

        private static readonly string[] SquareBar2DDetail = new[]
        {
            "模组码值","极柱序号","检测结果","长度结果","焊缝长度",
            "方形宽度结果","方形焊缝宽度","条形宽度结果","条形焊缝宽度",
            "间距结果","焊缝间距","爆孔结果","爆孔直径","虚焊结果","虚焊面积"
        };

        // ---------- 3D 明细表头 ----------
        public static string[] Get3DDetailHeader(bool isSquareBar)
        {
            return isSquareBar ? SquareBar3DDetail : Circle3DDetail;
        }

        private static readonly string[] Circle3DDetail = new[]
        {
            "工作模式","模组码","极柱号","检测时间","3D结果",
            "余高结果","下塌结果","余高值","下塌值",
            "3D原图","3D结果图"
        };

        private static readonly string[] SquareBar3DDetail = new[]
        {
            "工作模式","模组码","极柱号","检测时间","3D结果",
            "方形余高结果","方形下塌结果","方形余高值","方形下塌值",
            "条形余高结果","条形下塌结果","条形余高值","条形下塌值",
            "3D原图","3D结果图"
        };

        // ---------- 综合检测表头 ----------
        public static string[] GetCombinedHeader(bool isSquareBar)
        {
            return isSquareBar ? SquareBarCombined : CircleCombined;
        }

        private static readonly string[] CircleCombined = new[]
        {
            "工作模式","模组码","极柱号","检测时间","总结果",
            "2D结果","长度结果","宽度结果","偏移结果","爆孔结果",
            "外径结果","长度值","宽度值","偏移值","爆孔直径",
            "外径值","虚焊结果","虚焊值","3D结果","余高结果",
            "下榻结果","余高值","下榻值","2D原图","2D结果图",
            "3D原图","3D结果图"
        };

        private static readonly string[] SquareBarCombined = new[]
        {
            "工作模式","模组码","极柱号","检测时间","总结果",
            "2D结果","长度结果","方形宽度结果","条形宽度结果","间距结果",
            "爆孔结果","长度值","方形焊缝宽度","条形焊缝宽度","焊缝间距",
            "爆孔直径","虚焊结果","虚焊面积","3D结果","方形余高结果",
            "方形下塌结果","方形余高值","方形下塌值","条形余高结果","条形下塌结果",
            "条形余高值","条形下塌值","2D原图","2D结果图","3D原图","3D结果图"
        };
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_VisionService.Model
{
    public class CameraSettingModel
    {
        [Description("相机类型/品牌")]
        public int CameraType { get; set; }

        [Description("相机在列表中的索引")]
        public int CamSelectIndex { get; set; } = 0;

        [Description("曝光时间")]
        public short ExposureTime { get; set; } = 1000;

        [Description("增益")]
        public short Gain { get; set; } = 0;

        [Description("拍照后图片保存路径")]
        public string imgpath { get; set; } = "C:\\Images";

        [Description("图片保存类型(0:BMP, 1:JPG, 2:PNG)")]
        public int imgType { get; set; }

        [Description("相机序列号")]
        public string? SerilalNum { get; set; }

        [Description("IP")]
        public string? IP { get; set; }

        [Description("Port")]
        public int Port { get; set; }
    }
}

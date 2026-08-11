using HalconDotNet;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Common.Events
{
    public class CameraImagePayload
    {
        public string CameraSN { get; set; }
        public HObject Image { get; set; }
        /// <summary>
        ///图像是否来自调试界面，如果不是，则调试界面不显示该图像，避免调试界面后台占用过多资源
        /// </summary>
        //public bool IsFromDebug { get; set; }
    }

    public class HImageDisplayEvent : PubSubEvent<CameraImagePayload>
    {

    }
}

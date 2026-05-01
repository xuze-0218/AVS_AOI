using HalconDotNet;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Common
{
    public class CameraImagePayload
    {
        public string CameraSN { get; set; }
        public HObject Image { get; set; }
    }

    public class HImageDisplayEvent : PubSubEvent<CameraImagePayload>
    {

    }
}

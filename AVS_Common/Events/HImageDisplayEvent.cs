using HalconDotNet;
using Prism.Events;

namespace AVS_Common.Events
{
    public enum CameraImageType { Raw, Processed, CameraImageType }
    public class CameraImagePayload
    {
        public string CameraSN { get; set; }
        public HObject Image { get; set; }
        public CameraImageType ImageType { get; set; } = CameraImageType.Raw;
        /// <summary>
        ///图像是否来自调试界面，如果不是，则调试界面不显示该图像，避免调试界面后台占用过多资源
        /// </summary>
        //public bool IsFromDebug { get; set; }
    }

    public class HImageDisplayEvent : PubSubEvent<CameraImagePayload>
    {

    } 
    public class HIntensityImageDisplayEvent : PubSubEvent<CameraImagePayload>
    {

    }


}

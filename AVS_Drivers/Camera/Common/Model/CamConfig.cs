using AVS_Drivers.Camera.Common.Enum;
namespace AVS_Drivers.Camera.Common.Model
{
    public class CamConfig
    {
        public TriggerMode triggerMode { get; set; }

        public TriggerSource triggeSource { get; set; }

        public TriggerPolarity triggerPolarity { get; set; }

        public ushort ExpouseTime { get; set; }

        public ushort TriggerFilter { get; set; }

        public ushort TriggerDelay { get; set; }

        public short Gain { get; set; }

        public string SerilalNum { get;set; }
    }

    public class CameraInfoModel
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride { get; set; }
        public CamPixelFormat PixelFormat { get; set; }
    }
}

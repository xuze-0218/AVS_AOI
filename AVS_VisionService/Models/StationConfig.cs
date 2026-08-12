using Prism.Mvvm;
namespace AVS_Service.Models
{
    /// <summary>
    /// 相机与PLC映射
    /// </summary>
    public class StationConfig : BindableBase
    {
        /// <summary>
        /// 工位唯一标识，例如 "Left2D"、"Right3D"，用于会话管理、图像入队、PLC通信路由
        /// </summary>
        private string _stationId;
        public string StationId
        {
            get => _stationId;
            set => SetProperty(ref _stationId, value);
        }

        private string _cameraRole;
        public string CameraRole
        {
            get => _cameraRole;
            set => SetProperty(ref _cameraRole, value);
        }

        private string _ip = "127.0.0.1";
        public string IP
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        private int _port = 5000;
        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        private CommProtocol _protocol = CommProtocol.TCP;
        public CommProtocol Protocol
        {
            get => _protocol;
            set => SetProperty(ref _protocol, value);
        }

        private CommRole _role = CommRole.Server;
        public CommRole Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        /// <summary>
        /// 相机类型
        /// </summary>
        private VisionDimension _dimension = VisionDimension.TwoD;
        public VisionDimension Dimension
        {
            get => _dimension;
            set => SetProperty(ref _dimension, value);
        }

      

        /// <summary>
        ///共享模型Key，多个工位可共用同一套AI模型。为空时默认使用 StationId</summary>
        /// </summary>
        public string AiModelStationId { get; set; }
    }

    public enum CommProtocol { TCP, UDP }
    public enum CommRole { Server, Client }
    public enum VisionDimension { TwoD, ThreeD }
}

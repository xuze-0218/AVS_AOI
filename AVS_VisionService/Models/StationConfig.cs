using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Models
{
    /// <summary>
    /// 相机与PLC映射
    /// </summary>
    public class StationConfig : BindableBase
    {
        /// <summary>
        /// 工位ID，唯一标识一个工位，例如 "Station1", "Station2"，用于与 PLC&相机 映射
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
        /// 参数模块名（用于IParametersConfigService 读取对应模块的参数），例如 "SideA", "SideB"
        /// </summary>
        private string _productConfigSection = "SideA_2D";
        public string ProductConfigSection
        {
            get => _productConfigSection;
            set => SetProperty(ref _productConfigSection, value);
        }

        public string AiModelStationId { get; set; }
    }

    public enum CommProtocol { TCP, UDP }
    public enum CommRole { Server, Client }
    public enum VisionDimension { TwoD, ThreeD }
}

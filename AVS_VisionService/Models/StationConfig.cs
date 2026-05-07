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
    }

    public enum CommProtocol { TCP, UDP }
    public enum CommRole { Server, Client }
}

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AVS_Modules_Settings.Models
{
    public class CameraListItem : BindableBase
    {
        /// <summary>
        /// 相机列表项，用于ListBox展示
        /// </summary>
        public string SerialNumber { get; set; }
        public bool IsConfigured { get; set; }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                SetProperty(ref _isConnected, value);
                RaisePropertyChanged(nameof(StatusText));
                RaisePropertyChanged(nameof(StatusColor));
            }
        }

        public string StatusText => IsConnected ? "● 已连接" : "○ 离线";
        public string ConfigText => IsConfigured ? "已配置" : "未保存";
        public Brush StatusColor => IsConnected
            ? new SolidColorBrush(Color.FromRgb(76, 175, 80))
            : new SolidColorBrush(Color.FromRgb(158, 158, 158));
    }
}

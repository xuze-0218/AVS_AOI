using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Modules_Settings.Models
{
    // 单个极柱圆的数据
    public class PoleCircleItem : BindableBase
    {
        public int Row { get; set; }
        public int Col { get; set; }

        private int? _poleNumber;
        public int? PoleNumber
        {
            get => _poleNumber;
            set => SetProperty(ref _poleNumber, value);
        }

        private bool _isStartPoint;
        public bool IsStartPoint
        {
            get => _isStartPoint;
            set => SetProperty(ref _isStartPoint, value);
        }

        private bool _isEndPoint;
        public bool IsEndPoint
        {
            get => _isEndPoint;
            set => SetProperty(ref _isEndPoint, value);
        }

        private PoleResultStatus _status = PoleResultStatus.Unchecked;
        public PoleResultStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        // 颜色绑定
        public Brush CircleColor
        {
            get
            {
                if (IsStartPoint) return Brushes.Orange;
                if (IsEndPoint) return Brushes.Orchid;
                return Status switch
                {
                    PoleResultStatus.OK => Brushes.LimeGreen,
                    PoleResultStatus.NG => Brushes.Red,
                    _ => Brushes.LightGray
                };
            }
        }

        // PoleCircleItem 中
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                    RaisePropertyChanged(nameof(CircleColor));  // 选中高亮
            }
        }
    }

    public enum PoleResultStatus { Unchecked, OK, NG }
}

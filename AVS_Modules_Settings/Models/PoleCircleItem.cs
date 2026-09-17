using Prism.Mvvm;
using System.Windows.Media;

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
            set
            {
                if (SetProperty(ref _isStartPoint, value))
                    RaisePropertyChanged(nameof(StrokeColor));
            }
        }

        private bool _isEndPoint;
        public bool IsEndPoint
        {
            get => _isEndPoint;
            set
            {
                if (SetProperty(ref _isEndPoint, value))
                    RaisePropertyChanged(nameof(StrokeColor));
            }
        }

        private PoleResultStatus _status = PoleResultStatus.Unchecked;
        public PoleResultStatus Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                    RaisePropertyChanged(nameof(FillColor));
            }
        }

        // ===== 填充色：表达检测结果 =====
        public Brush FillColor
        {
            get
            {
                switch (Status)
                {
                    case PoleResultStatus.OK: return Brushes.LimeGreen;
                    case PoleResultStatus.NG: return Brushes.Red;
                    default: return Brushes.LightGray;
                }
            }
        }

        // ===== 边框色：表达拓扑角色 =====
        public Brush StrokeColor
        {
            get
            {
                if (IsStartPoint) return Brushes.Orange;
                if (IsEndPoint) return Brushes.Orchid;
                return Brushes.Gray;   // 普通极柱也给个灰边框，方便选中加粗时看得见
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                    RaisePropertyChanged(nameof(StrokeThickness));
            }
        }

        // ===== 边框粗细：表达选中 =====
        public double StrokeThickness => IsSelected ? 4.0 : 2.0;

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }
    }

    public enum PoleResultStatus { Unchecked, OK, NG }
}
using AVS_Common.Model;
using AVS_Service;
using HalconDotNet;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace AVS_Modules_Settings.ViewModels
{
    /// <summary>卡尺测量结果项</summary>
    public class MetrologyResultItem
    {
        public int Index { get; set; }
        public double Row { get; set; }
        public double Col { get; set; }
        public double Amplitude { get; set; }
        public double Distance { get; set; }
        public double InterDistance { get; set; }
    }

    public class MetrologyViewModel : BindableBase
    {
        private readonly IMetrologyService _metrologyService;

        // ========== 依赖注入属性 ==========
        private HObjectRegion _currentRoi;
        public HObjectRegion CurrentRoi
        {
            get => _currentRoi;
            set
            {
                if (SetProperty(ref _currentRoi, value))
                    RefreshGeomProperties();
            }
        }

        private HObject _currentImage;
        public HObject CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        private HWindow _halconWindow;
        public HWindow HalconWindow
        {
            get => _halconWindow;
            set => _halconWindow = value;
        }

        // ========== 对象类型显示（只读，反映 CurrentRoi.Style） ==========
        public List<string> ObjectTypes { get; } = new List<string> { "Rectangle2", "Circle", "Ellipse", "Line" };

        private string _selectedObjectType = "Rectangle2";
        public string SelectedObjectType
        {
            get => _selectedObjectType;
            set
            {
                // 不允许外部改变，仅由 CurrentRoi 同步
                if (SetProperty(ref _selectedObjectType, value))
                    RaisePropertyChanged(nameof(IsRectangle2Selected));
                // 其他布尔属性同理
            }
        }

        // 根据类型控制面板显示
        public bool IsRectangle2Selected => SelectedObjectType == "Rectangle2";
        public bool IsCircleSelected => SelectedObjectType == "Circle";
        public bool IsEllipseSelected => SelectedObjectType == "Ellipse";
        public bool IsLineSelected => SelectedObjectType == "Line";

        // ========== 几何参数（包装 CurrentRoi 字段） ==========
        public double RectLength1
        {
            get => CurrentRoi?.Length1 ?? 100;
            set { if (CurrentRoi != null) { CurrentRoi.Length1 = value; RaisePropertyChanged(); } }
        }
        public double RectLength2
        {
            get => CurrentRoi?.Length2 ?? 100;
            set { if (CurrentRoi != null) { CurrentRoi.Length2 = value; RaisePropertyChanged(); } }
        }
        public double RectPhi
        {
            get => CurrentRoi?.Angle ?? 0;
            set { if (CurrentRoi != null) { CurrentRoi.Angle = value; RaisePropertyChanged(); } }
        }
        public double CircleRadius
        {
            get => CurrentRoi?.Radius ?? 100;
            set { if (CurrentRoi != null) { CurrentRoi.Radius = value; RaisePropertyChanged(); } }
        }
        public double EllipseRadius1
        {
            get => CurrentRoi?.Length1 ?? 200;
            set { if (CurrentRoi != null) { CurrentRoi.Length1 = value; RaisePropertyChanged(); } }
        }
        public double EllipseRadius2
        {
            get => CurrentRoi?.Length2 ?? 100;
            set { if (CurrentRoi != null) { CurrentRoi.Length2 = value; RaisePropertyChanged(); } }
        }
        public double EllipsePhi
        {
            get => CurrentRoi?.Angle ?? 0;
            set { if (CurrentRoi != null) { CurrentRoi.Angle = value; RaisePropertyChanged(); } }
        }
        public double LineStartRow
        {
            get => CurrentRoi?.LeftY ?? 200;
            set { if (CurrentRoi != null) { CurrentRoi.LeftY = value; RaisePropertyChanged(); } }
        }
        public double LineStartCol
        {
            get => CurrentRoi?.LeftX ?? 200;
            set { if (CurrentRoi != null) { CurrentRoi.LeftX = value; RaisePropertyChanged(); } }
        }
        public double LineEndRow
        {
            get => CurrentRoi?.RightY ?? 400;
            set { if (CurrentRoi != null) { CurrentRoi.RightY = value; RaisePropertyChanged(); } }
        }
        public double LineEndCol
        {
            get => CurrentRoi?.RightX ?? 400;
            set { if (CurrentRoi != null) { CurrentRoi.RightX = value; RaisePropertyChanged(); } }
        }

        // ========== 测量参数 ==========
        private double _measureLength1 = 30;
        public double MeasureLength1 { get => _measureLength1; set => SetProperty(ref _measureLength1, value); }

        private double _measureLength2 = 5;
        public double MeasureLength2 { get => _measureLength2; set => SetProperty(ref _measureLength2, value); }

        private double _sigma = 1.0;
        public double Sigma { get => _sigma; set => SetProperty(ref _sigma, value); }

        private double _threshold = 30;
        public double Threshold { get => _threshold; set => SetProperty(ref _threshold, value); }

        private double _minScore = 0.5;
        public double MinScore { get => _minScore; set => SetProperty(ref _minScore, value); }

        private int _numInstances = 1;
        public int NumInstances { get => _numInstances; set => SetProperty(ref _numInstances, value); }

        private double _measureDistance = 10;
        public double MeasureDistance { get => _measureDistance; set => SetProperty(ref _measureDistance, value); }

        // ========== 边缘过滤参数 ==========
        public List<string> TransitionOptions { get; } = new List<string> { "all", "positive", "negative", "uniform" };
        private string _selectedTransition = "all";
        public string SelectedTransition { get => _selectedTransition; set => SetProperty(ref _selectedTransition, value); }

        public List<string> SelectOptions { get; } = new List<string> { "all", "first", "last" };
        private string _selectedSelect = "all";
        public string SelectedSelect { get => _selectedSelect; set => SetProperty(ref _selectedSelect, value); }

        public List<string> InterpolationOptions { get; } = new List<string> { "nearest_neighbor", "bilinear", "bicubic" };
        private string _selectedInterpolation = "bilinear";
        public string SelectedInterpolation { get => _selectedInterpolation; set => SetProperty(ref _selectedInterpolation, value); }

        // ========== 显示选项 ==========
        private bool _showMeasures = true;
        public bool ShowMeasures { get => _showMeasures; set => SetProperty(ref _showMeasures, value); }

        private bool _showResultContour = true;
        public bool ShowResultContour { get => _showResultContour; set => SetProperty(ref _showResultContour, value); }

        // ========== 结果 ==========
        private ObservableCollection<MetrologyResultItem> _caliperResults = new ObservableCollection<MetrologyResultItem>();
        public ObservableCollection<MetrologyResultItem> CaliperResults
        {
            get => _caliperResults;
            set => SetProperty(ref _caliperResults, value);
        }

        private string _statusMessage;
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        // ========== 命令 ==========
        public DelegateCommand MeasureCommand { get; }

        public MetrologyViewModel(IMetrologyService metrologyService)
        {
            _metrologyService = metrologyService;
            MeasureCommand = new DelegateCommand(OnMeasure);
        }

        // ========== CurrentRoi 改变时刷新类型与几何属性 ==========
        private void RefreshGeomProperties()
        {
            if (CurrentRoi == null) return;
            SelectedObjectType = CurrentRoi.Style switch
            {
                RoiType.RECTANGLE2 => "Rectangle2",
                RoiType.CIRCLE => "Circle",
                RoiType.ELLIPSE => "Ellipse",
                RoiType.LINE => "Line",
                _ => "Rectangle2"
            };
            RaisePropertyChanged(nameof(RectLength1));
            RaisePropertyChanged(nameof(RectLength2));
            RaisePropertyChanged(nameof(RectPhi));
            RaisePropertyChanged(nameof(CircleRadius));
            RaisePropertyChanged(nameof(EllipseRadius1));
            RaisePropertyChanged(nameof(EllipseRadius2));
            RaisePropertyChanged(nameof(EllipsePhi));
            RaisePropertyChanged(nameof(LineStartRow));
            RaisePropertyChanged(nameof(LineStartCol));
            RaisePropertyChanged(nameof(LineEndRow));
            RaisePropertyChanged(nameof(LineEndCol));
        }

        // ========== 测量执行 ==========
        private async void OnMeasure()
        {
            if (CurrentImage == null || !CurrentImage.IsInitialized())
            {
                StatusMessage = "请先加载图像";
                return;
            }
            if (CurrentRoi == null)
            {
                StatusMessage = "请先绘制测量对象";
                return;
            }

            try
            {
                // 1. 同步绘图对象并生成区域
                CurrentRoi.SyncFromDrawingObject();
                CurrentRoi.GenerateRegion();

                // 2. 创建计量模型并添加对象
                _metrologyService.SetImage(CurrentImage);
                HTuple modelHandle = _metrologyService.CreateMetrologyModel();

                switch (CurrentRoi.Style)
                {
                    case RoiType.RECTANGLE2:
                        _metrologyService.AddMetrologyObjectRectangle2(
                            modelHandle, CurrentRoi.Y, CurrentRoi.X, CurrentRoi.Angle,
                            CurrentRoi.Length1, CurrentRoi.Length2,
                            MeasureLength1, MeasureLength2,
                            Sigma, Threshold, MinScore, NumInstances, MeasureDistance,
                            SelectedTransition, SelectedSelect, SelectedInterpolation);
                        break;
                    case RoiType.CIRCLE:
                        _metrologyService.AddMetrologyObjectCircle(
                            modelHandle, CurrentRoi.Y, CurrentRoi.X, CurrentRoi.Radius,
                            MeasureLength1, MeasureLength2,
                            Sigma, Threshold, MinScore, NumInstances, MeasureDistance,
                            SelectedTransition, SelectedSelect, SelectedInterpolation);
                        break;
                    case RoiType.ELLIPSE:
                        _metrologyService.AddMetrologyObjectEllipse(
                            modelHandle, CurrentRoi.Y, CurrentRoi.X, CurrentRoi.Angle,
                            CurrentRoi.Length1, CurrentRoi.Length2,
                            MeasureLength1, MeasureLength2,
                            Sigma, Threshold, MinScore, NumInstances, MeasureDistance,
                            SelectedTransition, SelectedSelect, SelectedInterpolation);
                        break;
                    case RoiType.LINE:
                        _metrologyService.AddMetrologyObjectLine(
                            modelHandle, CurrentRoi.LeftY, CurrentRoi.LeftX,
                            CurrentRoi.RightY, CurrentRoi.RightX,
                            MeasureLength1, MeasureLength2,
                            Sigma, Threshold, MinScore, NumInstances, MeasureDistance,
                            SelectedTransition, SelectedSelect, SelectedInterpolation);
                        break;
                    default:
                        StatusMessage = "当前对象类型不支持测量";
                        return;
                }

                // 3. 执行测量
                _metrologyService.ApplyMetrologyModel(modelHandle, out HObject measures, out HObject resultContours);

                // 4. 显示图形
                DisplayResults(measures, resultContours);

                // 5. 提取边缘点数据
                ExtractEdgePoints(modelHandle, out var rows, out var cols, out var amplitudes);
                FillResults(rows, cols, amplitudes);

                _metrologyService.ClearMetrologyModel(modelHandle);
                StatusMessage = $"测量完成，找到 {CaliperResults.Count} 个边缘点";
            }
            catch (Exception ex)
            {
                StatusMessage = $"测量失败：{ex.Message}";
            }
        }

        private void DisplayResults(HObject measures, HObject resultContours)
        {
            if (HalconWindow == null) return;
            HOperatorSet.SetLineWidth(HalconWindow, 1);
            if (ShowMeasures && measures != null && measures.IsInitialized())
            {
                HOperatorSet.SetColor(HalconWindow, "cyan");
                HOperatorSet.DispObj(measures, HalconWindow);
            }
            if (ShowResultContour && resultContours != null && resultContours.IsInitialized())
            {
                HOperatorSet.SetColor(HalconWindow, "lime green");
                HOperatorSet.SetLineWidth(HalconWindow, 2);
                HOperatorSet.DispObj(resultContours, HalconWindow);
            }
        }

        private void ExtractEdgePoints(HTuple modelHandle, out HTuple rows, out HTuple cols, out HTuple amplitudes)
        {
            rows = new HTuple(); cols = new HTuple(); amplitudes = new HTuple();
            try
            {
                // 获取所有测量对象索引
                HOperatorSet.GetMetrologyObjectIndices(modelHandle, out HTuple indices);
                if (indices.Length == 0) return;

                for (int i = 0; i < indices.Length; i++)
                {
                    HTuple idx = indices[i];
                    // 修正：获取 num_instances 需要传入一个空元组作为 GenParamValue
                    HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "num_instances", new HTuple(), out HTuple numInst);
                    int n = numInst.I;
                    if (n > 0)
                    {
                        HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", "row", out HTuple rowOut);
                        HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", "column", out HTuple colOut);
                        HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", "amplitude", out HTuple ampOut);
                        rows = rows.TupleConcat(rowOut);
                        cols = cols.TupleConcat(colOut);
                        amplitudes = amplitudes.TupleConcat(ampOut);
                    }
                }
            }
            catch (HalconException ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractEdgePoints error: {ex.Message}");
            }
        }

        private void FillResults(HTuple rows, HTuple cols, HTuple amplitudes)
        {
            var list = new ObservableCollection<MetrologyResultItem>();
            if (rows.Length > 0)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    list.Add(new MetrologyResultItem
                    {
                        Index = i + 1,
                        Row = rows[i].D,
                        Col = cols[i].D,
                        Amplitude = amplitudes[i].D,
                        Distance = 0,
                        InterDistance = 0
                    });
                }
            }
            CaliperResults = list;
        }
    }
}
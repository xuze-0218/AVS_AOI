using AVS_Common.Model;
using AVS_Service;
using HalconDotNet;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Win32;

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

        private HObjectRegion _currentRoi;
        public HObjectRegion CurrentRoi
        {
            get => _currentRoi;
            //set
            //{
            //    if (SetProperty(ref _currentRoi, value))
            //        RefreshGeomProperties();
            //}
            set => SetProperty(ref _currentRoi, value);
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

        // 是否启用边缘对测量（仅对矩形2有效）
        private bool _useEdgePairs;
        public bool UseEdgePairs
        {
            get => _useEdgePairs;
            set => SetProperty(ref _useEdgePairs, value);
        }


        private string _currentMetroPath; // 当前加载的计量模型路径
        public bool IsFollowModel { get => _isFollowModel; set => SetProperty(ref _isFollowModel, value); }
        private bool _isFollowModel;

        // 模板参考位姿（制作时由协调器传入）
        public double ModelRefRow { get; set; }
        public double ModelRefCol { get; set; }
        public double ModelRefAngle { get; set; }

        // 运行时匹配结果（由协调器传入）
        public double MatchRow { get; set; }
        public double MatchCol { get; set; }
        public double MatchAngle { get; set; }

        //// ========== 对象类型显示（只读，反映 CurrentRoi.Style） ==========
        //public List<string> ObjectTypes { get; } = new List<string> { "Rectangle2", "Circle", "Ellipse", "Line" };

        //private string _selectedObjectType = "Rectangle2";
        //public string SelectedObjectType
        //{
        //    get => _selectedObjectType;
        //    set
        //    {
        //        // 不允许外部改变，仅由 CurrentRoi 同步
        //        if (SetProperty(ref _selectedObjectType, value))
        //            RaisePropertyChanged(nameof(IsRectangle2Selected));
        //    }
        //}

        //public bool IsRectangle2Selected => SelectedObjectType == "Rectangle2";
        //public bool IsCircleSelected => SelectedObjectType == "Circle";
        //public bool IsEllipseSelected => SelectedObjectType == "Ellipse";
        //public bool IsLineSelected => SelectedObjectType == "Line";

        // ========== 几何参数（包装 CurrentRoi 字段） ==========
        //public double RectLength1
        //{
        //    get => CurrentRoi?.Length1 ?? 100;
        //    set { if (CurrentRoi != null) { CurrentRoi.Length1 = value; RaisePropertyChanged(); } }
        //}
        //public double RectLength2
        //{
        //    get => CurrentRoi?.Length2 ?? 100;
        //    set { if (CurrentRoi != null) { CurrentRoi.Length2 = value; RaisePropertyChanged(); } }
        //}
        //public double RectPhi
        //{
        //    get => CurrentRoi?.Angle ?? 0;
        //    set { if (CurrentRoi != null) { CurrentRoi.Angle = value; RaisePropertyChanged(); } }
        //}
        //public double CircleRadius
        //{
        //    get => CurrentRoi?.Radius ?? 100;
        //    set { if (CurrentRoi != null) { CurrentRoi.Radius = value; RaisePropertyChanged(); } }
        //}
        //public double EllipseRadius1
        //{
        //    get => CurrentRoi?.Length1 ?? 200;
        //    set { if (CurrentRoi != null) { CurrentRoi.Length1 = value; RaisePropertyChanged(); } }
        //}
        //public double EllipseRadius2
        //{
        //    get => CurrentRoi?.Length2 ?? 100;
        //    set { if (CurrentRoi != null) { CurrentRoi.Length2 = value; RaisePropertyChanged(); } }
        //}
        //public double EllipsePhi
        //{
        //    get => CurrentRoi?.Angle ?? 0;
        //    set { if (CurrentRoi != null) { CurrentRoi.Angle = value; RaisePropertyChanged(); } }
        //}
        //public double LineStartRow
        //{
        //    get => CurrentRoi?.LeftY ?? 200;
        //    set { if (CurrentRoi != null) { CurrentRoi.LeftY = value; RaisePropertyChanged(); } }
        //}
        //public double LineStartCol
        //{
        //    get => CurrentRoi?.LeftX ?? 200;
        //    set { if (CurrentRoi != null) { CurrentRoi.LeftX = value; RaisePropertyChanged(); } }
        //}
        //public double LineEndRow
        //{
        //    get => CurrentRoi?.RightY ?? 400;
        //    set { if (CurrentRoi != null) { CurrentRoi.RightY = value; RaisePropertyChanged(); } }
        //}
        //public double LineEndCol
        //{
        //    get => CurrentRoi?.RightX ?? 400;
        //    set { if (CurrentRoi != null) { CurrentRoi.RightX = value; RaisePropertyChanged(); } }
        //}

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
        public DelegateCommand SaveMetroCommand { get; }
        public DelegateCommand LoadMetroCommand { get; }

        public MetrologyViewModel(IMetrologyService metrologyService)
        {
            _metrologyService = metrologyService;
            MeasureCommand = new DelegateCommand(OnMeasure);
            SaveMetroCommand = new DelegateCommand(OnSaveMetro, () => CurrentRoi != null);
            LoadMetroCommand = new DelegateCommand(OnLoadMetro);
        }

        // ========== CurrentRoi 改变时刷新类型与几何属性 ==========
        //private void RefreshGeomProperties()
        //{
        //    if (CurrentRoi == null) return;
        //    SelectedObjectType = CurrentRoi.Style switch
        //    {
        //        RoiType.RECTANGLE2 => "Rectangle2",
        //        RoiType.CIRCLE => "Circle",
        //        RoiType.ELLIPSE => "Ellipse",
        //        RoiType.LINE => "Line",
        //        _ => "Rectangle2"
        //    };
        //    RaisePropertyChanged(nameof(RectLength1));
        //    RaisePropertyChanged(nameof(RectLength2));
        //    RaisePropertyChanged(nameof(RectPhi));
        //    RaisePropertyChanged(nameof(CircleRadius));
        //    RaisePropertyChanged(nameof(EllipseRadius1));
        //    RaisePropertyChanged(nameof(EllipseRadius2));
        //    RaisePropertyChanged(nameof(EllipsePhi));
        //    RaisePropertyChanged(nameof(LineStartRow));
        //    RaisePropertyChanged(nameof(LineStartCol));
        //    RaisePropertyChanged(nameof(LineEndRow));
        //    RaisePropertyChanged(nameof(LineEndCol));
        //}

        // ========== 测量执行 ==========
        private void OnMeasure()
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
                HTuple modelHandle;
                if (IsFollowModel && File.Exists(_currentMetroPath))
                {
                    // 从文件加载已保存的计量模型（包含参考系统）
                    modelHandle = _metrologyService.ReadMetrologyModel(_currentMetroPath);
                    // 对齐到匹配结果
                    _metrologyService.AlignMetrologyModel(modelHandle, MatchRow, MatchCol, MatchAngle);
                }
                else
                {
                    CurrentRoi.SyncFromDrawingObject();
                    CurrentRoi.GenerateRegion();
                    modelHandle = _metrologyService.CreateMetrologyModel();
                    AddCurrentObjectToModel(modelHandle);
                }              
                _metrologyService.SetImage(CurrentImage);                
                _metrologyService.ApplyMetrologyModel(modelHandle, out HObject measures, out HObject resultContours);
                DisplayResults(measures, resultContours);

                if (!IsFollowModel) // 动态模式提取边缘点，跟随模式也可提取
                {
                    ExtractEdgePoints(modelHandle, out var rows, out var cols, out var amplitudes);
                    FillResults(rows, cols, amplitudes);
                }
                else
                {
                    // 从计量模型提取结果
                    ExtractEdgePoints(modelHandle, out var rows, out var cols, out var amplitudes);
                    FillResults(rows, cols, amplitudes);
                }
                _metrologyService.ClearMetrologyModel(modelHandle);
                StatusMessage = $"测量完成，找到 {CaliperResults.Count} 个边缘点";
            }
            catch (Exception ex)
            {
                StatusMessage = $"测量失败：{ex.Message}";
            }
        }

        private void AddCurrentObjectToModel(HTuple modelHandle)
        {
            switch (CurrentRoi.Style)
            {
                case RoiType.RECTANGLE2:
                    if (UseEdgePairs)
                    {
                        MeasureEdgePairs();
                        return;
                    }
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
        }

        private void MeasureEdgePairs()
        {
            try
            {
                CurrentRoi.SyncFromDrawingObject();
                CurrentRoi.GenerateRegion();

                double row = CurrentRoi.Y;
                double col = CurrentRoi.X;
                double phi = CurrentRoi.Angle;
                double length1 = CurrentRoi.Length1;
                double length2 = CurrentRoi.Length2;

                // 生成测量句柄
                HTuple measureHandle;
                HOperatorSet.GetImageSize(CurrentImage, out HTuple width, out HTuple height);
                HOperatorSet.GenMeasureRectangle2(row, col, phi, length1, length2, width, height, SelectedInterpolation,      // 插值方式可改为绑定
                    out measureHandle);

                // 转换极性与选择为 Halcon 字符串
                string transition = SelectedTransition; // "all"/"positive"/"negative"/"uniform"
                string select = SelectedSelect;         // "all"/"first"/"last"

                HOperatorSet.MeasurePairs(CurrentImage, measureHandle,
                    Sigma, Threshold, transition, select,
                    out HTuple rowEdgeFirst, out HTuple colEdgeFirst, out HTuple amplitudeFirst,
                    out HTuple rowEdgeSecond, out HTuple colEdgeSecond, out HTuple amplitudeSecond,
                    out HTuple intraDistance, out HTuple interDistance);

                // 显示结果
                if (HalconWindow != null)
                {
                    HOperatorSet.SetLineWidth(HalconWindow, 1);
                    if (ShowMeasures)
                    {
                        HOperatorSet.SetColor(HalconWindow, "cyan");
                        //HOperatorSet.DispObj(measureHandle, HalconWindow); // 不能直接 DispObj measureHandle，需要取轮廓
                        //                                                   // 正确获取测量轮廓并显示：
                        HObject measureContours;
                        HOperatorSet.GenMeasureRectangle2(row, col, phi, length1, length2,
                            width, height, SelectedInterpolation, out HTuple tmpHandle);
                        // 或者用 get_metrology_object_measures 类似，但这里简单处理：不显示测量线，只显示结果点
                    }
                    if (ShowResultContour && rowEdgeFirst.Length > 0)
                    {
                        HOperatorSet.SetColor(HalconWindow, "lime green");
                        HOperatorSet.SetLineWidth(HalconWindow, 2);
                        for (int i = 0; i < rowEdgeFirst.Length; i++)
                        {
                            HOperatorSet.DispCross(HalconWindow, rowEdgeFirst[i], colEdgeFirst[i], 12.0, 0);
                            HOperatorSet.DispCross(HalconWindow, rowEdgeSecond[i], colEdgeSecond[i], 12.0, 0);
                            HOperatorSet.SetColor(HalconWindow, "yellow");
                            HOperatorSet.DispLine(HalconWindow, rowEdgeFirst[i], colEdgeFirst[i],
                                rowEdgeSecond[i], colEdgeSecond[i]);
                        }
                    }
                }

                // 填充表格
                var results = new ObservableCollection<MetrologyResultItem>();
                for (int i = 0; i < rowEdgeFirst.Length; i++)
                {
                    results.Add(new MetrologyResultItem
                    {
                        Index = i + 1,
                        Row = rowEdgeFirst[i].D,
                        Col = colEdgeFirst[i].D,
                        Amplitude = amplitudeFirst[i].D,
                        Distance = intraDistance[i].D,      // 边缘对内部宽度
                        InterDistance = interDistance.Length > i ? interDistance[i].D : 0
                    });
                }
                CaliperResults = results;
                HOperatorSet.CloseMeasure(measureHandle);
                StatusMessage = $"边缘对测量完成，找到 {results.Count} 对";
            }
            catch (HalconException ex)
            {
                StatusMessage = $"边缘对测量失败：{ex.Message}";
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


        private void OnSaveMetro()
        {
            if (CurrentRoi == null) return;
            try
            {
                CurrentRoi.SyncFromDrawingObject();
                CurrentRoi.GenerateRegion();

                // 保存对话框
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Metrology Model (*.mtr)|*.mtr",
                    Title = "保存计量模型"
                };
                if (sfd.ShowDialog() != true) return;

                HTuple modelHandle = _metrologyService.CreateMetrologyModel();
                AddCurrentObjectToModel(modelHandle);

                // 设置参考系统（如果启用了跟随模式，则使用协调器传入的模板中心位姿）
                if (IsFollowModel)
                {
                    _metrologyService.SetReferenceSystem(modelHandle, ModelRefRow, ModelRefCol, ModelRefAngle);
                }
                _metrologyService.SaveMetrologyModel(modelHandle, sfd.FileName);
                _metrologyService.ClearMetrologyModel(modelHandle);
                _currentMetroPath = sfd.FileName;
                StatusMessage = $"计量模型已保存到 {sfd.FileName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败：{ex.Message}";
            }
        }

        private void OnLoadMetro()
        {
            try
            {
               OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "Metrology Model (*.mtr)|*.mtr",
                    Title = "加载计量模型"
                };
                if (ofd.ShowDialog() != true) return;

                _currentMetroPath = ofd.FileName;
                IsFollowModel = true; // 加载后自动启用跟随模式（可手动切换）
                StatusMessage = $"已加载计量模型 {_currentMetroPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败：{ex.Message}";
            }
        }

        //private void ExtractEdgePoints(HTuple modelHandle, out HTuple rows, out HTuple cols, out HTuple amplitudes)
        //{
        //    rows = new HTuple(); cols = new HTuple(); amplitudes = new HTuple();
        //    try
        //    {
        //        // 获取所有测量对象索引
        //        HOperatorSet.GetMetrologyObjectIndices(modelHandle, out HTuple indices);
        //        if (indices.Length == 0) return;

        //        for (int i = 0; i < indices.Length; i++)
        //        {
        //            HTuple idx = indices[i];
        //            HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "num_instances", new HTuple(), out HTuple numInst);
        //            int n = numInst.I;
        //            if (n > 0)
        //            {
        //                HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", "row", out HTuple rowOut);
        //                HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", "column", out HTuple colOut);
        //                HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", "amplitude", out HTuple ampOut);
        //                rows = rows.TupleConcat(rowOut);
        //                cols = cols.TupleConcat(colOut);
        //                amplitudes = amplitudes.TupleConcat(ampOut);
        //            }
        //        }
        //    }
        //    catch (HalconException ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"ExtractEdgePoints error: {ex.Message}");
        //    }
        //}


        private void ExtractEdgePoints(HTuple modelHandle, out HTuple rows, out HTuple cols, out HTuple amplitudes)
        {
            rows = new HTuple(); cols = new HTuple(); amplitudes = new HTuple();
            try
            {
                HOperatorSet.GetMetrologyObjectIndices(modelHandle, out HTuple indices);
                if (indices == null || indices.Length == 0) return;

                for (int i = 0; i < indices.Length; i++)
                {
                    HTuple idx = indices[i];
                    HTuple rowPart = new HTuple(), colPart = new HTuple(), ampPart = new HTuple();
                    try
                    {
                        HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", "row", out rowPart);
                    }
                    catch (HalconException) { }
                    try
                    {
                        HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", "column", out colPart);
                    }
                    catch (HalconException) { }
                    try
                    {
                        HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", "amplitude", out ampPart);
                    }
                    catch (HalconException) { }

                    if (rowPart.Length > 0)
                    {
                        int count = rowPart.Length;
                        rows = rows.TupleConcat(rowPart);
                        cols = cols.TupleConcat(colPart.Length == count ? colPart : GenConstantTuple(0.0, count));
                        amplitudes = amplitudes.TupleConcat(ampPart.Length == count ? ampPart : GenConstantTuple(0.0, count));
                    }
                }
            }
            catch (HalconException ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractEdgePoints error: {ex.Message}");
            }
        }

        private HTuple GenConstantTuple(double value, int count)
        {
            HTuple t = new HTuple();
            for (int i = 0; i < count; i++) t[i] = value;
            return t;
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
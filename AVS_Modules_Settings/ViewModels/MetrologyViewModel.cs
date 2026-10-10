using AVS_Modules_Settings.Models;
using AVS_Service.Models;
using AVS_Service.Services;
using HalconDotNet;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace AVS_Modules_Settings.ViewModels
{
    /// <summary>
    /// 卡尺测量结果项
    /// </summary>
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
        private readonly IParametersConfigService _paramService;
        public Func<bool> RequestMatch { get; set; }

        private HObjectRegion _currentRoi;
        public HObjectRegion CurrentRoi
        {
            get => _currentRoi;
            set
            {
                if (SetProperty(ref _currentRoi, value))
                    SaveMetroCommand?.RaiseCanExecuteChanged();
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

        // 是否启用边缘对测量（仅对矩形2有效）
        private bool _useEdgePairs;
        public bool UseEdgePairs
        {
            get => _useEdgePairs;
            set => SetProperty(ref _useEdgePairs, value);
        }

        /// <summary>
        /// 当前加载的计量模型路径
        /// </summary>
        private string _currentMetroPath;
        /// <summary>
        /// 上一次测量的中间结果，供"显示选项"切换时重绘
        /// </summary>
        private HObject _cachedMeasures;
        private HObject _cachedResultContours;
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
        public bool ShowMeasures
        {
            get => _showMeasures;
            set
            {
                if (SetProperty(ref _showMeasures, value))
                    RedrawWithCurrentSettings();
            }
        }

        private bool _showResultContour = true;
        public bool ShowResultContour
        {
            get => _showResultContour;
            set
            {
                if (SetProperty(ref _showResultContour, value))
                    RedrawWithCurrentSettings();
            }
        }

        // ========== 结果 ==========
        private ObservableCollection<MetrologyResultItem> _caliperResults = new ObservableCollection<MetrologyResultItem>();
        public ObservableCollection<MetrologyResultItem> CaliperResults
        {
            get => _caliperResults;
            set => SetProperty(ref _caliperResults, value);
        }

        private ObservableCollection<GeometryResultItem> _geometryResults = new ObservableCollection<GeometryResultItem>();
        public ObservableCollection<GeometryResultItem> GeometryResults
        {
            get => _geometryResults;
            set => SetProperty(ref _geometryResults, value);
        }

        private string _statusMessage;
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        // ========== 命令 ==========
        public DelegateCommand MeasureCommand { get; }
        public DelegateCommand SaveMetroCommand { get; }
        public DelegateCommand LoadMetroCommand { get; }
        public DelegateCommand CalcResolutionCommand { get; }

        public MetrologyViewModel(IMetrologyService metrologyService, IParametersConfigService paramService)
        {
            _paramService = paramService;
            _metrologyService = metrologyService;
            MeasureCommand = new DelegateCommand(OnMeasure);
            SaveMetroCommand = new DelegateCommand(OnSaveMetro, () => CurrentRoi != null);
            LoadMetroCommand = new DelegateCommand(OnLoadMetro);
            CalcResolutionCommand = new DelegateCommand(OnCalcResolution);
        }


        // ========== 测量执行 ==========
        private void OnMeasure()
        {
            Debug.WriteLine($"[Measure] Style={CurrentRoi.Style}, UseEdgePairs={UseEdgePairs}, IsFollowModel={IsFollowModel}, mtr存在={File.Exists(_currentMetroPath)}");
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
            //边缘对分流,不进 metrology 流程
            if (UseEdgePairs && CurrentRoi.Style == RoiType.RECTANGLE2)
            {
                MeasureEdgePairs();
                GeometryResults = new ObservableCollection<GeometryResultItem>();  //清掉旧的几何表
                _cachedMeasures?.Dispose();
                _cachedMeasures = null;
                _cachedResultContours?.Dispose();
                _cachedResultContours = null;
                return;
            }
            HTuple modelHandle = null;
            HObject measures = null;
            HObject resultContours = null;
            try
            {
                if (IsFollowModel && File.Exists(_currentMetroPath))
                {
                    if (RequestMatch == null || !RequestMatch.Invoke())
                    {
                        StatusMessage = "模板匹配失败，无法定位卡尺";
                        return;
                    }
                    // 从文件加载已保存的计量模型（包含参考系统）
                    modelHandle = _metrologyService.ReadMetrologyModel(_currentMetroPath);
                    ApplyVmParamsToModel(modelHandle);
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
                if (HalconWindow != null)
                {
                    HalconWindow.ClearWindow();
                    if (CurrentImage != null && CurrentImage.IsInitialized())
                        HalconWindow.DispObj(CurrentImage);

                    if (CurrentRoi?.Region != null
                        && CurrentRoi.Region.IsInitialized()
                        && CurrentRoi.Region.CountObj() > 0)
                    {
                        HalconWindow.SetDraw("margin");
                        HalconWindow.SetColor("green");
                        HalconWindow.SetLineWidth(2);
                        HalconWindow.DispObj(CurrentRoi.Region);
                    }
                }
                _metrologyService.ApplyMetrologyModel(modelHandle, out measures, out resultContours);
                _cachedMeasures?.Dispose();
                _cachedResultContours?.Dispose();
                _cachedMeasures = (measures != null && measures.IsInitialized()) ? measures.Clone() : null;
                _cachedResultContours = (resultContours != null && resultContours.IsInitialized()) ? resultContours.Clone() : null;
                DisplayResults(measures, resultContours);
                //几何拟合结果
                ExtractGeometryResults(modelHandle);
                // 从计量模型提取结果
                ExtractEdgePoints(modelHandle, out var rows, out var cols, out var amplitudes);
                FillResults(rows, cols, amplitudes);
                StatusMessage = $"测量完成，找到 {CaliperResults.Count} 个边缘点";
            }
            catch (Exception ex)
            {
                StatusMessage = $"测量失败：{ex.Message}";
            }
            finally
            {
                measures?.Dispose();
                resultContours?.Dispose();
                if (modelHandle != null && modelHandle.Length > 0)
                {
                    try { _metrologyService.ClearMetrologyModel(modelHandle); } catch { }
                }
            }
        }

        private void AddCurrentObjectToModel(HTuple modelHandle)
        {
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
        }

        /// <summary>
        /// 把 VM 上的 UI 参数应用到已加载的 metrology 模型上，
        /// </summary>
        private void ApplyVmParamsToModel(HTuple modelHandle)
        {
            try
            {
                HOperatorSet.GetMetrologyObjectIndices(modelHandle, out HTuple indices);
                if (indices == null || indices.Length == 0) return;

                for (int i = 0; i < indices.Length; i++)
                {
                    HTuple idx = indices[i];
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "measure_length1", MeasureLength1);
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "measure_length2", MeasureLength2);
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "measure_sigma", Sigma);
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "measure_threshold", Threshold);
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "min_score", MinScore);
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "num_instances", NumInstances);
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "measure_distance", MeasureDistance);
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "measure_transition", SelectedTransition);
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "measure_select", SelectedSelect);
                    HOperatorSet.SetMetrologyObjectParam(modelHandle, idx, "measure_interpolation", SelectedInterpolation);
                }
                Debug.WriteLine($"[ApplyVmParams] 已应用 {indices.Length} 个对象的 UI 参数");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApplyVmParams] {ex.Message}");
            }
        }


        /// <summary>
        /// 从 metrology 对象读参数写到 VM 属性，
        /// 让 View 上的 UI 反映 mtr 文件里的实际配置
        /// </summary>
        private void LoadVmParamsFromModel(HTuple modelHandle, HTuple idx)
        {
            try
            {
                // ---- 数值参数 ----
                double ml1 = GetMetroParam(modelHandle, idx, "measure_length1");
                double ml2 = GetMetroParam(modelHandle, idx, "measure_length2");
                double sg = GetMetroParam(modelHandle, idx, "measure_sigma");
                double th = GetMetroParam(modelHandle, idx, "measure_threshold");
                double ms = GetMetroParam(modelHandle, idx, "min_score");
                double md = GetMetroParam(modelHandle, idx, "measure_distance");
                double ni = GetMetroParam(modelHandle, idx, "num_instances");

                if (ml1 > 0) MeasureLength1 = ml1;
                if (ml2 > 0) MeasureLength2 = ml2;
                if (sg > 0) Sigma = sg;
                if (th > 0) Threshold = th;
                if (ms > 0) MinScore = ms;
                if (md > 0) MeasureDistance = md;
                if (ni > 0) NumInstances = (int)ni;

                // ---- 字符串参数（要判合法性，避免 UI 上选不中）----
                string trans = GetMetroParamString(modelHandle, idx, "measure_transition");
                if (!string.IsNullOrEmpty(trans) && TransitionOptions.Contains(trans))
                    SelectedTransition = trans;

                string sel = GetMetroParamString(modelHandle, idx, "measure_select");
                if (!string.IsNullOrEmpty(sel) && SelectOptions.Contains(sel))
                    SelectedSelect = sel;

                string interp = GetMetroParamString(modelHandle, idx, "measure_interpolation");
                if (!string.IsNullOrEmpty(interp) && InterpolationOptions.Contains(interp))
                    SelectedInterpolation = interp;

                Debug.WriteLine($"[LoadVmParams] ML1={MeasureLength1}, ML2={MeasureLength2}, " +
                                $"Sigma={Sigma}, Thr={Threshold}, MinScore={MinScore}, " +
                                $"Trans={SelectedTransition}, Sel={SelectedSelect}, Interp={SelectedInterpolation}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadVmParams] {ex.Message}");
            }
        }

        /// <summary>
        /// 读字符串类型参数；读不到或类型不对返回 null
        /// </summary>
        private string GetMetroParamString(HTuple modelHandle, HTuple idx, string paramName)
        {
            try
            {
                HOperatorSet.GetMetrologyObjectParam(modelHandle, idx, paramName, out HTuple val);
                return (val != null && val.Length > 0) ? val[0].S : null;
            }
            catch
            {
                return null;
            }
        }
        private void MeasureEdgePairs()
        {
            HTuple measureHandle = null;
            try
            {
                double row, col, phi, length1, length2;

                if (IsFollowModel && File.Exists(_currentMetroPath) && RequestMatch != null)
                {
                    //跟随模板路径 模板匹配，拿到当前位姿
                    if (!RequestMatch.Invoke())
                    {
                        StatusMessage = "模板匹配失败，无法定位卡尺";
                        return;
                    }
                    HOperatorSet.VectorAngleToRigid(ModelRefRow, ModelRefCol, ModelRefAngle, MatchRow, MatchCol, MatchAngle, out HTuple homMat);
                    //把 CurrentRoi 的绝对坐标变换到当前图像坐标
                    HOperatorSet.AffineTransPixel(homMat, CurrentRoi.Y, CurrentRoi.X, out HTuple newR, out HTuple newC);

                    row = newR.D;
                    col = newC.D;
                    phi = CurrentRoi.Angle + (MatchAngle - ModelRefAngle);   // 角度增量
                    length1 = CurrentRoi.Length1;
                    length2 = CurrentRoi.Length2;

                    Debug.WriteLine($"[EdgePairs] 跟随: 旧Ref=({ModelRefRow:F1},{ModelRefCol:F1},{ModelRefAngle:F3}) " +
                                    $"新Match=({MatchRow:F1},{MatchCol:F1},{MatchAngle:F3}) " +
                                    $"变换后=({row:F1},{col:F1},{phi:F3})");
                }
                else
                {
                    //不跟随：直接用ROI当前位置
                    CurrentRoi.SyncFromDrawingObject();
                    CurrentRoi.GenerateRegion();
                    row = CurrentRoi.Y;
                    col = CurrentRoi.X;
                    phi = CurrentRoi.Angle;
                    length1 = CurrentRoi.Length1;
                    length2 = CurrentRoi.Length2;
                }

                // 生成测量句柄
                HOperatorSet.GetImageSize(CurrentImage, out HTuple width, out HTuple height);
                HOperatorSet.GenMeasureRectangle2(row, col, phi, length1, length2, width, height, SelectedInterpolation, out measureHandle);
                if (HalconWindow != null)
                {
                    HalconWindow.ClearWindow();
                    if (CurrentImage != null && CurrentImage.IsInitialized())
                        HalconWindow.DispObj(CurrentImage);

                    if (CurrentRoi?.Region != null
                        && CurrentRoi.Region.IsInitialized()
                        && CurrentRoi.Region.CountObj() > 0)
                    {
                        HalconWindow.SetDraw("margin");
                        HalconWindow.SetColor("green");
                        HalconWindow.SetLineWidth(2);
                        HalconWindow.DispObj(CurrentRoi.Region);
                    }

                    // 画测量矩形（红色）
                    HOperatorSet.GenRectangle2(out HObject rectDbg, row, col, phi, length1, length2);
                    HalconWindow.SetColor("red");
                    HalconWindow.SetDraw("margin");
                    HalconWindow.SetLineWidth(2);
                    HalconWindow.DispObj(rectDbg);
                    rectDbg.Dispose();
                }
                HOperatorSet.MeasurePairs(CurrentImage, measureHandle, Sigma, Threshold, SelectedTransition, SelectedSelect,
                    out HTuple rowEdgeFirst, out HTuple colEdgeFirst, out HTuple amplitudeFirst,
                    out HTuple rowEdgeSecond, out HTuple colEdgeSecond, out HTuple amplitudeSecond,
                    out HTuple intraDistance, out HTuple interDistance);

                // 显示结果
                if (HalconWindow != null && ShowResultContour && rowEdgeFirst.Length > 0)
                {
                    HOperatorSet.SetLineWidth(HalconWindow, 1);
                    HOperatorSet.SetColor(HalconWindow, "lime green");
                    for (int i = 0; i < rowEdgeFirst.Length; i++)
                    {
                        HOperatorSet.DispCross(HalconWindow, rowEdgeFirst[i], colEdgeFirst[i], 12.0, 0);
                        HOperatorSet.DispCross(HalconWindow, rowEdgeSecond[i], colEdgeSecond[i], 12.0, 0);
                        HOperatorSet.SetColor(HalconWindow, "yellow");
                        HOperatorSet.DispLine(HalconWindow, rowEdgeFirst[i], colEdgeFirst[i], rowEdgeSecond[i], colEdgeSecond[i]);
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
                StatusMessage = $"边缘对测量完成，找到 {results.Count} 对";
            }
            catch (HalconException ex)
            {
                StatusMessage = $"边缘对测量失败：{ex.Message}";
            }
            finally
            {
                if (measureHandle != null && measureHandle.Length > 0)
                {
                    try { HOperatorSet.CloseMeasure(measureHandle); } catch { }
                }
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

        /// <summary>
        /// 按当前显示开关重绘图像 ROI 测量结果
        /// </summary>
        private void RedrawWithCurrentSettings()
        {
            if (HalconWindow == null || CurrentImage == null || !CurrentImage.IsInitialized())
                return;

            try
            {
                HalconWindow.ClearWindow();
                HalconWindow.DispObj(CurrentImage);

                // ROI（和协调器显示保持一致）
                if (CurrentRoi?.Region != null
                    && CurrentRoi.Region.IsInitialized()
                    && CurrentRoi.Region.CountObj() > 0)
                {
                    HalconWindow.SetDraw("margin");
                    HalconWindow.SetColor("green");
                    HalconWindow.SetLineWidth(2);
                    HalconWindow.DispObj(CurrentRoi.Region);
                }

                if (ShowMeasures && _cachedMeasures != null && _cachedMeasures.IsInitialized())
                {
                    HalconWindow.SetColor("cyan");
                    HalconWindow.SetLineWidth(1);
                    HalconWindow.DispObj(_cachedMeasures);
                }

                if (ShowResultContour && _cachedResultContours != null && _cachedResultContours.IsInitialized())
                {
                    HalconWindow.SetColor("lime green");
                    HalconWindow.SetLineWidth(2);
                    HalconWindow.DispObj(_cachedResultContours);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RedrawWithCurrentSettings error: {ex.Message}");
            }
        }

        /// <summary>
        /// 清空上一次的测量结果（切换ROI、加载新图时调用
        /// </summary>
        public void ClearResults()
        {
            CaliperResults = new ObservableCollection<MetrologyResultItem>();
            GeometryResults = new ObservableCollection<GeometryResultItem>();

            _cachedMeasures?.Dispose();
            _cachedMeasures = null;
            _cachedResultContours?.Dispose();
            _cachedResultContours = null;
        }

        /// <summary>
        /// 视图卸载时释放缓存
        /// </summary>
        public void Cleanup()
        {
            _cachedMeasures?.Dispose();
            _cachedMeasures = null;
            _cachedResultContours?.Dispose();
            _cachedResultContours = null;
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
                if (IsFollowModel)
                {
                    //保存时做一次匹配，把当前位姿作为参考位姿
                    if (RequestMatch == null || !RequestMatch.Invoke())
                    {
                        StatusMessage = "模板匹配失败，无法保存跟随模型的卡尺";
                        _metrologyService.ClearMetrologyModel(modelHandle);
                        return;
                    }
                    ModelRefRow = MatchRow;
                    ModelRefCol = MatchCol;
                    ModelRefAngle = MatchAngle;

                    // 设参考系统，然后卡尺用相对坐标添加
                    _metrologyService.SetReferenceSystem(modelHandle, ModelRefRow, ModelRefCol, ModelRefAngle);
                    AddCurrentObjectToModelRelative(modelHandle);  //相对坐标
                }
                else
                {
                    AddCurrentObjectToModel(modelHandle);           //绝对坐标
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
                    InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"),
                    Filter = "Metrology Model (*.mtr)|*.mtr",
                    Title = "加载计量模型"
                };
                if (ofd.ShowDialog() != true) return;

                _currentMetroPath = ofd.FileName;
                bool ok = SyncRoiFromMetroFile(_currentMetroPath);
                if (!ok)
                {
                    return;
                }

                IsFollowModel = true;
                StatusMessage = $"已加载计量模型 {_currentMetroPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载失败：{ex.Message}";
            }
        }

        /// <summary>
        /// 从 .mtr 文件读取卡尺几何，恢复到 CurrentRoi（含 Style）
        /// </summary>
        private bool SyncRoiFromMetroFile(string mtrPath)
        {
            HTuple h = null;
            try
            {
                h = _metrologyService.ReadMetrologyModel(mtrPath);
                HOperatorSet.GetMetrologyObjectIndices(h, out HTuple indices);
                if (indices == null || indices.Length == 0)
                {
                    StatusMessage = "卡尺文件为空";
                    return false;
                }
                HOperatorSet.GetMetrologyModelParam(h, "reference_system", out HTuple refSys);
                double refRow = refSys[0].D;
                double refCol = refSys[1].D;
                double refAngle = refSys[2].D;
                ModelRefRow = refRow;
                ModelRefCol = refCol;
                ModelRefAngle = refAngle;
                HOperatorSet.VectorAngleToRigid(0, 0, 0, refRow, refCol, refAngle, out HTuple homMatRef);
                HTuple idx = indices[0];
                HOperatorSet.GetMetrologyObjectParam(h, idx, "object_type", out HTuple objType);

                switch (objType.S)
                {
                    case "rectangle":
                    case "rectangle2":
                        CurrentRoi.Style = RoiType.RECTANGLE2;
                        {
                            double r = GetMetroParam(h, idx, "row");
                            double c = GetMetroParam(h, idx, "column");
                            double p = GetMetroParam(h, idx, "phi");
                            HOperatorSet.AffineTransPixel(homMatRef, r, c, out HTuple ar, out HTuple ac);
                            CurrentRoi.Y = ar.D;
                            CurrentRoi.X = ac.D;
                            CurrentRoi.Angle = p + refAngle;   // 角度叠加
                            CurrentRoi.Length1 = GetMetroParam(h, idx, "length1");
                            CurrentRoi.Length2 = GetMetroParam(h, idx, "length2");
                        }
                        break;

                    case "circle":
                        CurrentRoi.Style = RoiType.ELLIPSE;
                        {
                            double r = GetMetroParam(h, idx, "row");
                            double c = GetMetroParam(h, idx, "column");
                            double p = GetMetroParam(h, idx, "phi");
                            HOperatorSet.AffineTransPixel(homMatRef, r, c, out HTuple ar, out HTuple ac);
                            CurrentRoi.Y = ar.D;
                            CurrentRoi.X = ac.D;
                            CurrentRoi.Angle = p + refAngle;
                            CurrentRoi.Length1 = GetMetroParam(h, idx, "radius1");
                            CurrentRoi.Length2 = GetMetroParam(h, idx, "radius2");
                        }
                        break;

                    case "ellipse":
                        CurrentRoi.Style = RoiType.ELLIPSE;
                        {
                            double r = GetMetroParam(h, idx, "row");
                            double c = GetMetroParam(h, idx, "column");
                            double p = GetMetroParam(h, idx, "phi");
                            HOperatorSet.AffineTransPixel(homMatRef, r, c, out HTuple ar, out HTuple ac);
                            CurrentRoi.Y = ar.D;
                            CurrentRoi.X = ac.D;
                            CurrentRoi.Angle = p + refAngle;
                            CurrentRoi.Length1 = GetMetroParam(h, idx, "radius1");
                            CurrentRoi.Length2 = GetMetroParam(h, idx, "radius2");
                        }
                        break;

                    case "line":
                        CurrentRoi.Style = RoiType.LINE;
                        {
                            double rb = GetMetroParam(h, idx, "row_begin");
                            double cb = GetMetroParam(h, idx, "column_begin");
                            double re = GetMetroParam(h, idx, "row_end");
                            double ce = GetMetroParam(h, idx, "column_end");
                            HOperatorSet.AffineTransPixel(homMatRef, rb, cb, out HTuple arb, out HTuple acb);
                            HOperatorSet.AffineTransPixel(homMatRef, re, ce, out HTuple are, out HTuple ace);
                            CurrentRoi.LeftY = arb.D;
                            CurrentRoi.LeftX = acb.D;
                            CurrentRoi.RightY = are.D;
                            CurrentRoi.RightX = ace.D;
                            CurrentRoi.Y = (arb.D + are.D) * 0.5;
                            CurrentRoi.X = (acb.D + ace.D) * 0.5;
                        }
                        break;

                    default:
                        StatusMessage = $"卡尺类型 {objType.S} 暂不支持恢复";
                        return false;
                }
                LoadVmParamsFromModel(h, idx);
                CurrentRoi.GenerateRegion();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SyncRoiFromMetroFile error: {ex.Message}");
                StatusMessage = $"从卡尺文件恢复 ROI 失败：{ex.Message}";
                return false;
            }
            finally
            {
                if (h != null && h.Length > 0)
                {
                    try { _metrologyService.ClearMetrologyModel(h); } catch { }
                }
            }
        }


        /// <summary>
        /// 用相对参考位姿的坐标添加卡尺（配合 SetReferenceSystem 使用）
        /// </summary>
        private void AddCurrentObjectToModelRelative(HTuple modelHandle)
        {
            //计算从参考位姿到原点的变换
            HOperatorSet.VectorAngleToRigid(ModelRefRow, ModelRefCol, ModelRefAngle, 0, 0, 0, out HTuple homMat);
            //卡尺中心 / 起点终点 转到相对坐标
            HOperatorSet.AffineTransPixel(homMat, CurrentRoi.Y, CurrentRoi.X, out HTuple relRow, out HTuple relCol);
            double relAngle = CurrentRoi.Angle - ModelRefAngle;
            // 用相对坐标添加卡尺
            switch (CurrentRoi.Style)
            {
                case RoiType.RECTANGLE2:
                    _metrologyService.AddMetrologyObjectRectangle2(
                        modelHandle, relRow.D, relCol.D, relAngle,
                        CurrentRoi.Length1, CurrentRoi.Length2,
                        MeasureLength1, MeasureLength2,
                        Sigma, Threshold, MinScore, NumInstances, MeasureDistance,
                        SelectedTransition, SelectedSelect, SelectedInterpolation);
                    break;

                case RoiType.CIRCLE:
                    _metrologyService.AddMetrologyObjectCircle(
                        modelHandle, relRow.D, relCol.D, CurrentRoi.Radius,
                        MeasureLength1, MeasureLength2,
                        Sigma, Threshold, MinScore, NumInstances, MeasureDistance,
                        SelectedTransition, SelectedSelect, SelectedInterpolation);
                    break;

                case RoiType.ELLIPSE:
                    _metrologyService.AddMetrologyObjectEllipse(
                        modelHandle, relRow.D, relCol.D, relAngle,
                        CurrentRoi.Length1, CurrentRoi.Length2,
                        MeasureLength1, MeasureLength2,
                        Sigma, Threshold, MinScore, NumInstances, MeasureDistance,
                        SelectedTransition, SelectedSelect, SelectedInterpolation);
                    break;

                case RoiType.LINE:
                    HOperatorSet.AffineTransPixel(homMat,
                        CurrentRoi.LeftY, CurrentRoi.LeftX,
                        out HTuple relRb, out HTuple relCb);
                    HOperatorSet.AffineTransPixel(homMat,
                        CurrentRoi.RightY, CurrentRoi.RightX,
                        out HTuple relRe, out HTuple relCe);
                    _metrologyService.AddMetrologyObjectLine(
                        modelHandle, relRb.D, relCb.D, relRe.D, relCe.D,
                        MeasureLength1, MeasureLength2,
                        Sigma, Threshold, MinScore, NumInstances, MeasureDistance,
                        SelectedTransition, SelectedSelect, SelectedInterpolation);
                    break;

                default:
                    StatusMessage = "当前对象类型不支持跟随模型";
                    break;
            }
        }

        /// <summary>
        /// 从 metrology 对象读单个几何参数（越界或异常返回 0）
        /// </summary>
        private double GetMetroParam(HTuple modelHandle, HTuple idx, string paramName)
        {
            try
            {
                HOperatorSet.GetMetrologyObjectParam(modelHandle, idx, paramName, out HTuple val);
                return (val != null && val.Length > 0) ? val[0].D : 0;
            }
            catch
            {
                return 0;
            }
        }

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

        /// <summary>
        /// 提取每个卡尺对象的几何拟合结果
        /// </summary>
        /// <param name="modelHandle"></param>
        private void ExtractGeometryResults(HTuple modelHandle)
        {
            var list = new ObservableCollection<GeometryResultItem>();
            try
            {
                HOperatorSet.GetMetrologyObjectIndices(modelHandle, out HTuple indices);
                if (indices == null || indices.Length == 0)
                {
                    GeometryResults = list;
                    return;
                }

                for (int i = 0; i < indices.Length; i++)
                {
                    HTuple idx = indices[i];
                    var item = new GeometryResultItem { Index = i + 1 };

                    HOperatorSet.GetMetrologyObjectParam(modelHandle, idx, "object_type", out HTuple objType);
                    item.ObjectType = objType?.S ?? string.Empty;

                    switch (item.ObjectType.ToLower())
                    {
                        case "circle":
                            item.CenterRow = GetFirstResult(modelHandle, idx, "row");
                            item.CenterCol = GetFirstResult(modelHandle, idx, "column");
                            item.Size1 = GetFirstResult(modelHandle, idx, "radius");
                            break;

                        case "ellipse":
                            item.CenterRow = GetFirstResult(modelHandle, idx, "row");
                            item.CenterCol = GetFirstResult(modelHandle, idx, "column");
                            item.Size1 = GetFirstResult(modelHandle, idx, "radius1");
                            item.Size2 = GetFirstResult(modelHandle, idx, "radius2");
                            item.AngleDeg = RadToDeg(GetFirstResult(modelHandle, idx, "phi"));
                            break;

                        case "rectangle2":
                        case "rectangle":
                            item.CenterRow = GetFirstResult(modelHandle, idx, "row");
                            item.CenterCol = GetFirstResult(modelHandle, idx, "column");
                            item.Size1 = GetFirstResult(modelHandle, idx, "length1");
                            item.Size2 = GetFirstResult(modelHandle, idx, "length2");
                            item.AngleDeg = RadToDeg(GetFirstResult(modelHandle, idx, "phi"));
                            break;

                        case "line":
                            double rb = GetFirstResult(modelHandle, idx, "row_begin");
                            double cb = GetFirstResult(modelHandle, idx, "column_begin");
                            double re = GetFirstResult(modelHandle, idx, "row_end");
                            double ce = GetFirstResult(modelHandle, idx, "column_end");
                            item.CenterRow = (rb + re) * 0.5;
                            item.CenterCol = (cb + ce) * 0.5;
                            double dx = ce - cb;
                            double dy = re - rb;
                            item.Size1 = Math.Sqrt(dx * dx + dy * dy);
                            item.AngleDeg = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                            break;
                    }

                    // RMS 用来判断边缘质量（越小越好）
                    item.RMS = GetFirstResult(modelHandle, idx, "rms");

                    list.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExtractGeometryResults error: {ex.Message}");
            }
            GeometryResults = list;
        }

        private double GetFirstResult(HTuple modelHandle, HTuple idx, string resultName)
        {
            try
            {
                HOperatorSet.GetMetrologyObjectResult(modelHandle, idx, "all", "result_type", resultName, out HTuple result);
                return (result != null && result.Length > 0) ? result[0].D : 0;
            }
            catch (HalconException)
            {
                return 0;
            }
        }

        private double RadToDeg(double rad) => rad * 180.0 / Math.PI;
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

        public void SetMatchResult(double row, double col, double angle)
        {
            MatchRow = row;
            MatchCol = col;
            MatchAngle = angle;
        }

        private void OnCalcResolution()
        {
            if (CaliperResults != null || GeometryResults != null)
            {
                //GeometryResults[0].Size1 * 2
            }
        }
    }
}
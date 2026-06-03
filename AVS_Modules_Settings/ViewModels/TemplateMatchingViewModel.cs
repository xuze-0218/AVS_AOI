using AVS_Service;
using HalconDotNet;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using AVS_Service.Models;

namespace AVS_Modules_Settings.ViewModels
{
    /// <summary>
    /// 模板匹配结果项
    /// </summary>
    public class MatchResultItem
    {
        public int Index { get; set; }
        public double Row { get; set; }
        public double Col { get; set; }
        public double Angle { get; set; }
        public double Score { get; set; }
        public double ScaleR { get; set; }
        public double ScaleC { get; set; }
    }

    /// <summary>
    /// 模板匹配子ViewModel
    /// ROI 绘制已迁入协调器 TempAndCaliDebugViewModel，本类仅保留
    /// 掩膜编辑、模板创建/查找/保存/加载 功能。
    /// </summary>
    public class TemplateMatchingViewModel : BindableBase
    {
        private readonly ITemplateMatchingService _templateMatchingService;
        private HWindow _halconWindow;

        // ===== 掩膜相关 =====
        private bool _isMaskEditing;
        private double _eraserSize = 10;
        private string _eraserType = "rectangle";
        private HObject _accumulatedMaskRegion = new HObject();
        private HObject _accumulatedEraseRegion = new HObject();
        private bool _isMouseDown;
        private bool _isEraseMode;

        public bool IsMaskEditing
        {
            get => _isMaskEditing;
            set
            {
                if (SetProperty(ref _isMaskEditing, value))
                {
                    RaisePropertyChanged(nameof(IsCustomMode));
                    if (value) EnterMaskEdit();
                    else ExitMaskEdit();
                }
            }
        }

        public bool IsEraseMode
        {
            get => _isEraseMode;
            set
            {
                if (SetProperty(ref _isEraseMode, value))
                    StatusMessage = value
                        ? "擦除模式：按住鼠标左键拖动擦除已有掩膜"
                        : "掩膜编辑：按住鼠标左键拖动绘制掩膜";
            }
        }

        public double EraserSize { get => _eraserSize; set => SetProperty(ref _eraserSize, value); }
        public string EraserType { get => _eraserType; set => SetProperty(ref _eraserType, value); }
        public List<string> EraserTypes { get; } = new List<string> { "rectangle", "circle" };

        // ===== 通用属性 =====
        private string _statusMessage = "右键绘制形状，掩膜编辑剔除干扰";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        private HObject _currentImage = new HObject();
        public HObject CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        // ===== ROI 区域（由协调器传入） =====
        private HObject _modelRegion = new HObject();
        /// <summary>带掩膜处理的最终区域，供模板匹配使用</summary>
        public HObject ModelRegion
        {
            get => _modelRegion;
            set
            {
                _modelRegion?.Dispose();
                _modelRegion = value ?? new HObject();
                if (!_modelRegion.IsInitialized())
                    _modelRegion.GenEmptyObj();
                RaisePropertyChanged();
            }
        }

        /// <summary>当前活跃的 ROI 对象引用（由协调器注入），供独立操作时同步 DrawingObject 和绘制 Region</summary>
        public HObjectRegion ActiveRoi { get; set; }

        /// <summary>当前是否处于自定义交互模式（掩膜编辑），用于禁用 HMoveContent</summary>
        public bool IsCustomMode => IsMaskEditing;

        // ===== Halcon 窗口 =====
        public HWindow HalconWindow
        {
            get => _halconWindow;
            set => SetHalconWindow(value);
        }

        // ===== 模板参数 =====
        private string _cmAngleStart = "0";
        public string CmAngleStart { get => _cmAngleStart; set => SetProperty(ref _cmAngleStart, value); }

        private string _cmAngleExtent = "360";
        public string CmAngleExtent { get => _cmAngleExtent; set => SetProperty(ref _cmAngleExtent, value); }

        private string _cmMetric;
        public string CmMetric { get => _cmMetric; set => SetProperty(ref _cmMetric, value); }

        private string _cmContrastLow = "30";
        public string CmContrastLow { get => _cmContrastLow; set => SetProperty(ref _cmContrastLow, value); }

        private string _cmContrastHigh = "200";
        public string CmContrastHigh { get => _cmContrastHigh; set => SetProperty(ref _cmContrastHigh, value); }

        private string _cmMinContrast = "5";
        public string CmMinContrast { get => _cmMinContrast; set => SetProperty(ref _cmMinContrast, value); }

        private string _cmNumLevels = "auto";
        public string CmNumLevels { get => _cmNumLevels; set => SetProperty(ref _cmNumLevels, value); }

        private string _cmOptimization;
        public string CmOptimization { get => _cmOptimization; set => SetProperty(ref _cmOptimization, value); }

        private bool _cmIsNumLevels;
        public bool CmIsNumLevels { get => _cmIsNumLevels; set => SetProperty(ref _cmIsNumLevels, value); }

        private bool _cmIsContrast;
        public bool CmIsContrast { get => _cmIsContrast; set => SetProperty(ref _cmIsContrast, value); }

        private bool _cmIsMinContrast;
        public bool CmIsMinContrast { get => _cmIsMinContrast; set => SetProperty(ref _cmIsMinContrast, value); }

        private string _cmMinBorder = "0";
        public string CmMinBorder { get => _cmMinBorder; set => SetProperty(ref _cmMinBorder, value); }

        private string _fmAngleStart = "0";
        public string FmAngleStart { get => _fmAngleStart; set => SetProperty(ref _fmAngleStart, value); }

        private string _fmAngleExtent = "360";
        public string FmAngleExtent { get => _fmAngleExtent; set => SetProperty(ref _fmAngleExtent, value); }

        private string _fmScaleMin = "0.9";
        public string FmScaleMin { get => _fmScaleMin; set => SetProperty(ref _fmScaleMin, value); }

        private string _fmScaleMax = "1.1";
        public string FmScaleMax { get => _fmScaleMax; set => SetProperty(ref _fmScaleMax, value); }

        private string _fmNumMatches = "10";
        public string FmNumMatches { get => _fmNumMatches; set => SetProperty(ref _fmNumMatches, value); }

        private string _fmMinScore = "0.5";
        public string FmMinScore { get => _fmMinScore; set => SetProperty(ref _fmMinScore, value); }

        private string _fmMaxOverlap = "0.5";
        public string FmMaxOverlap { get => _fmMaxOverlap; set => SetProperty(ref _fmMaxOverlap, value); }

        private string _fmGreediness = "0.75";
        public string FmGreediness { get => _fmGreediness; set => SetProperty(ref _fmGreediness, value); }

        private string _fmSubPixel;
        public string FmSubPixel { get => _fmSubPixel; set => SetProperty(ref _fmSubPixel, value); }

        private string _fmNumLevels = "0";
        public string FmNumLevels { get => _fmNumLevels; set => SetProperty(ref _fmNumLevels, value); }

        private bool _isModeXld;
        public bool IsModeXld { get => _isModeXld; set => SetProperty(ref _isModeXld, value); }

        private bool _isCross;
        public bool IsCross { get => _isCross; set => SetProperty(ref _isCross, value); }

        private bool _isDispRoi;
        public bool IsDispRoi { get => _isDispRoi; set => SetProperty(ref _isDispRoi, value); }

        private bool _isScoreJudge;
        public bool IsScoreJudge { get => _isScoreJudge; set => SetProperty(ref _isScoreJudge, value); }

        private string _lowScore;
        public string LowScore { get => _lowScore; set => SetProperty(ref _lowScore, value); }

        private string _highScore;
        public string HighScore { get => _highScore; set => SetProperty(ref _highScore, value); }

        private bool _isNumsJudge;
        public bool IsNumsJudge { get => _isNumsJudge; set => SetProperty(ref _isNumsJudge, value); }

        private string _lowNums;
        public string LowNums { get => _lowNums; set => SetProperty(ref _lowNums, value); }

        private string _highNums;
        public string HighNums { get => _highNums; set => SetProperty(ref _highNums, value); }

        private string _modelPath;
        public string ModelPath { get => _modelPath; set => SetProperty(ref _modelPath, value); }

        private ObservableCollection<MatchResultItem> _matchResults = new ObservableCollection<MatchResultItem>();
        public ObservableCollection<MatchResultItem> MatchResults
        {
            get => _matchResults;
            set => SetProperty(ref _matchResults, value);
        }
        /// <summary>
        /// 最佳匹配
        /// </summary>
        public MatchResultItem BestMatch => MatchResults != null && MatchResults.Count > 0 ? MatchResults[0] : null;

        public List<string> MetricOptions { get; }
        public List<string> OptimizationOptions { get; }
        public List<string> SubPixelOptions { get; }

        private HTuple _currentModelId = null;

        // ===== 命令 =====
        public DelegateCommand CreateModelCommand { get; }
        public DelegateCommand FindModelCommand { get; }
        public DelegateCommand SaveModelCommand { get; }
        public DelegateCommand LoadModelCommand { get; }
        public DelegateCommand ClearMaskCommand { get; }

        public TemplateMatchingViewModel(ITemplateMatchingService templateMatchingService)
        {
            _templateMatchingService = templateMatchingService;

            _accumulatedMaskRegion.GenEmptyObj();
            _accumulatedEraseRegion.GenEmptyObj();
            CurrentImage.GenEmptyObj();
            ModelRegion.GenEmptyObj();

            MetricOptions = new List<string> { "use_polarity", "ignore_global_polarity", "ignore_local_polarity", "ignore_color_polarity" };
            OptimizationOptions = new List<string> { "none", "point_reduction_low", "point_reduction_medium", "point_reduction_high", "pregeneration", "no_pregeneration" };
            SubPixelOptions = new List<string> { "none", "interpolation", "least_squares", "least_squares_high", "least_squares_very_high" };

            CmMetric = MetricOptions[0];
            CmOptimization = OptimizationOptions[0];
            FmSubPixel = SubPixelOptions[0];

            ClearMaskCommand = new DelegateCommand(ClearMask);
            CreateModelCommand = new DelegateCommand(CreateModel);
            FindModelCommand = new DelegateCommand(FindModel, () => _currentModelId != null);
            SaveModelCommand = new DelegateCommand(SaveModel);
            LoadModelCommand = new DelegateCommand(LoadModel);
        }

        #region Halcon 窗口设置
        public void SetHalconWindow(HWindow window)
        {
            _halconWindow = window;
            _templateMatchingService.SetHalconWindow(window);
        }
        #endregion

        #region 掩膜编辑
        private void EnterMaskEdit()
        {
            if (_halconWindow == null) return;
            ClearErasePreview();
            StatusMessage = IsEraseMode
                ? "擦除模式：按住鼠标左键拖动擦除已有掩膜"
                : "掩膜编辑：按住鼠标左键拖动绘制掩膜";
            RefreshDisplayWithMask();
        }

        private void ExitMaskEdit()
        {
            _isMouseDown = false;
            IsEraseMode = false;
            ClearErasePreview();
            StatusMessage = "掩膜编辑已退出";
            if (_halconWindow != null)
            {
                DisplayImagePreserveZoom();
                RefreshDisplayWithMask();
            }
        }

        public void OnMouseDown(double row, double col)
        {
            if (!IsMaskEditing) return;
            _isMouseDown = true;
            AddEraserAt(row, col);
        }

        public void OnMouseMove(double row, double col)
        {
            if (!IsMaskEditing || !_isMouseDown) return;
            AddEraserAt(row, col);
        }

        public void OnMouseUp()
        {
            if (!IsMaskEditing) return;
            _isMouseDown = false;
            if (IsEraseMode && _accumulatedEraseRegion != null && _accumulatedEraseRegion.IsInitialized() && _accumulatedEraseRegion.CountObj() > 0)
                ApplyErase();
            ClearErasePreview();
            RefreshDisplayWithMask();
        }

        private void ApplyErase()
        {
            if (_accumulatedMaskRegion == null || !_accumulatedMaskRegion.IsInitialized() || _accumulatedMaskRegion.CountObj() == 0)
                return;
            HObject result = new HObject();
            HOperatorSet.Difference(_accumulatedMaskRegion, _accumulatedEraseRegion, out result);
            _accumulatedMaskRegion.Dispose();
            _accumulatedMaskRegion = result;
        }

        private void ClearErasePreview()
        {
            _accumulatedEraseRegion?.Dispose();
            _accumulatedEraseRegion = new HObject();
            _accumulatedEraseRegion.GenEmptyObj();
        }

        private void AddEraserAt(double row, double col)
        {
            HObject eraser;
            if (EraserType == "rectangle")
                HOperatorSet.GenRectangle2(out eraser, row, col, 0, EraserSize, EraserSize);
            else
                HOperatorSet.GenCircle(out eraser, row, col, EraserSize);

            if (IsEraseMode)
            {
                HObject temp = new HObject();
                HOperatorSet.Union2(_accumulatedEraseRegion, eraser, out temp);
                _accumulatedEraseRegion.Dispose();
                _accumulatedEraseRegion = temp;
            }
            else
            {
                HObject temp = new HObject();
                HOperatorSet.Union2(_accumulatedMaskRegion, eraser, out temp);
                _accumulatedMaskRegion.Dispose();
                _accumulatedMaskRegion = temp;
            }
            eraser.Dispose();
            RefreshDisplayWithMask();
        }

        public void ClearMask()
        {
            _accumulatedMaskRegion?.Dispose();
            _accumulatedMaskRegion = new HObject();
            _accumulatedMaskRegion.GenEmptyObj();
            ClearErasePreview();
            if (IsMaskEditing)
                RefreshDisplayWithMask();
            StatusMessage = "掩膜已清除";
        }

        private void RefreshDisplayWithMask()
        {
            if (_halconWindow == null) return;
            DisplayImagePreserveZoom();

            // 绘制当前 ROI 轮廓（绿色边框），让用户看清掩膜与ROI的关系
            if (ActiveRoi?.Region != null && ActiveRoi.Region.IsInitialized() && ActiveRoi.Region.CountObj() > 0)
            {
                _halconWindow.SetDraw("margin");
                _halconWindow.SetColor("green");
                _halconWindow.SetLineWidth(2);
                _halconWindow.DispObj(ActiveRoi.Region);
            }

            // 绘制掩膜半透明红
            if (_accumulatedMaskRegion != null && _accumulatedMaskRegion.IsInitialized() && _accumulatedMaskRegion.CountObj() > 0)
            {
                _halconWindow.SetDraw("fill");
                _halconWindow.SetRgba(255, 0, 0, 150);
                _halconWindow.SetLineWidth(1);
                _halconWindow.DispObj(_accumulatedMaskRegion);
            }

            // 绘制擦除预览（蓝色半透明）
            if (IsEraseMode && _accumulatedEraseRegion != null && _accumulatedEraseRegion.IsInitialized() && _accumulatedEraseRegion.CountObj() > 0)
            {
                _halconWindow.SetDraw("fill");
                _halconWindow.SetRgba(0, 0, 255, 120);
                _halconWindow.SetLineWidth(1);
                _halconWindow.DispObj(_accumulatedEraseRegion);
            }
        }

        private void DisplayImagePreserveZoom()
        {
            if (_halconWindow == null) return;
            _halconWindow.ClearWindow();
            HObject img = _currentImage;
            if (img != null && img.IsInitialized())
                img.DispObj(_halconWindow);
        }
        #endregion

        #region 模板操作
        private void CreateModel()
        {
            // 从 DrawingObject 同步最新 ROI 并生成 Region
            ActiveRoi?.SyncFromDrawingObject();
            ActiveRoi?.GenerateRegion();

            // 优先用 ActiveRoi 的 Region，回退到旧的 _modelRegion
            HObject roiRegion = (ActiveRoi?.Region != null && ActiveRoi.Region.IsInitialized() && ActiveRoi.Region.CountObj() > 0)
                ? ActiveRoi.Region
                : _modelRegion;
            HObject finalRegion = ApplyMaskToRegion(roiRegion);

            if (finalRegion == null || !finalRegion.IsInitialized() || finalRegion.CountObj() == 0)
            {
                StatusMessage = "无有效区域，请先绘制 ROI";
                return;
            }

            try
            {
                HOperatorSet.ReduceDomain(CurrentImage, finalRegion, out HObject templateImage);

                double.TryParse(CmAngleStart, out double angleStart);
                double.TryParse(CmAngleExtent, out double angleExtent);

                string numLevels = CmIsNumLevels ? "auto" : CmNumLevels;
                string contrast = CmIsContrast ? "auto" : $"{CmContrastLow},{CmContrastHigh}";
                string minContrast = CmIsMinContrast ? "auto" : CmMinContrast;

                _currentModelId = _templateMatchingService.CreateShapeModel(
                    templateImage,
                    angleStart, angleExtent,
                    numLevels,
                    contrast,
                    minContrast,
                    CmMetric,
                    CmOptimization);

                StatusMessage = "模板创建成功";
                FindModelCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                StatusMessage = $"创建模板失败：{ex.Message}";
            }
        }

        private HObject ApplyMaskToRegion(HObject roiRegion)
        {
            if (roiRegion == null || !roiRegion.IsInitialized() || roiRegion.CountObj() == 0)
                return new HObject();

            if (_accumulatedMaskRegion == null || !_accumulatedMaskRegion.IsInitialized() || _accumulatedMaskRegion.CountObj() == 0)
                return roiRegion.Clone();

            HObject result;
            HOperatorSet.Difference(roiRegion, _accumulatedMaskRegion, out result);
            return result;
        }

        private void FindModel()
        {
            if (_currentModelId == null) { StatusMessage = "请先创建模板"; return; }
            if (CurrentImage == null || !CurrentImage.IsInitialized()) { StatusMessage = "无图像"; return; }

            try
            {
                double.TryParse(FmAngleStart, out double angleStart);
                double.TryParse(FmAngleExtent, out double angleExtent);
                double.TryParse(FmScaleMin, out double scaleMin);
                double.TryParse(FmScaleMax, out double scaleMax);
                double.TryParse(FmMinScore, out double minScore);
                double.TryParse(FmMaxOverlap, out double maxOverlap);
                double.TryParse(FmGreediness, out double greediness);
                int.TryParse(FmNumLevels, out int numLevels);
                int.TryParse(FmNumMatches, out int numMatches);

                _templateMatchingService.FindShapeModel(
                    _currentModelId,
                    angleStart, angleExtent,
                    scaleMin, scaleMax,
                    minScore, numMatches, maxOverlap,
                    FmSubPixel, numLevels, greediness,
                    out HTuple rows, out HTuple cols, out HTuple angles, out HTuple scores);

                _templateMatchingService.DisplayResult(_currentModelId, rows, cols, angles, scores);

                MatchResults.Clear();
                if (rows != null && rows.Length > 0)
                {
                    for (int i = 0; i < rows.Length; i++)
                    {
                        MatchResults.Add(new MatchResultItem
                        {
                            Index = i + 1,
                            Row = rows[i].D,
                            Col = cols[i].D,
                            Angle = angles[i].D,
                            Score = scores[i].D
                        });
                    }
                }
                StatusMessage = $"找到 {MatchResults.Count} 个匹配结果";
            }
            catch (Exception ex)
            {
                StatusMessage = $"查找失败：{ex.Message}";
            }
        }

        private void SaveModel()
        {
            if (_currentModelId == null) { StatusMessage = "请先创建模板"; return; }
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Shape Model|*.shm|All files|*.*",
                Title = "保存模板"
            };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    _templateMatchingService.SaveShapeModel(_currentModelId, sfd.FileName);
                    ModelPath = sfd.FileName;
                    StatusMessage = "模板保存成功";
                }
                catch (Exception ex) { StatusMessage = $"保存失败：{ex.Message}"; }
            }
        }

        private void LoadModel()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Shape Model|*.shm|All files|*.*",
                Title = "加载模板"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    _currentModelId = null;
                    _currentModelId = _templateMatchingService.LoadShapeModel(ofd.FileName);
                    ModelPath = ofd.FileName;
                    FindModelCommand.RaiseCanExecuteChanged();
                    StatusMessage = "模板加载成功";
                }
                catch (Exception ex) { StatusMessage = $"加载失败：{ex.Message}"; }
            }
        }
        #endregion
    }
}

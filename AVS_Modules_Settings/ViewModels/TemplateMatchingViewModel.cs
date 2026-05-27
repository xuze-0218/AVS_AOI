using AVS_Common.Model;
using AVS_Service;
using HalconDotNet;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

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
    /// 集成 ROI 绘制（Halcon DrawingObject）、掩膜编辑（涂抹/擦除）和模板匹配功能
    /// 参考 PreviousTMViewModel 设计
    /// </summary>
    public class TemplateMatchingViewModel : BindableBase
    {
        private readonly ITemplateMatchingService _templateMatchingService;
        private HWindow _halconWindow;

        // ===== ROI 绘制相关 =====
        private readonly HObjectRegion _currentRoi = new HObjectRegion();
        private List<double> _polygonTempRows = new List<double>();
        private List<double> _polygonTempCols = new List<double>();
        private bool _isDrawingPolygon = false;
        public bool IsDrawingPolygon
        {
            get => _isDrawingPolygon;
            set
            {
                if (SetProperty(ref _isDrawingPolygon, value))
                    RaisePropertyChanged(nameof(IsCustomMode));
            }
        }

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
                {
                    StatusMessage = value ? "擦除模式：按住鼠标左键拖动擦除已有掩膜" : "掩膜编辑：按住鼠标左键拖动绘制掩膜";
                }
            }
        }

        public double EraserSize { get => _eraserSize; set => SetProperty(ref _eraserSize, value); }

        public string EraserType
        {
            get => _eraserType;
            set => SetProperty(ref _eraserType, value);
        }

        public List<string> EraserTypes { get; } = new List<string> { "rectangle", "circle" };

        // ===== 通用属性 =====
        private double _currentMouseRow, _currentMouseCol;
        public double CurrentMouseRow { get => _currentMouseRow; set => SetProperty(ref _currentMouseRow, value); }
        public double CurrentMouseCol { get => _currentMouseCol; set => SetProperty(ref _currentMouseCol, value); }

        private string _statusMessage = "右键绘制形状，掩膜编辑剔除干扰";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        private HObject _currentImage = new HObject();
        public HObject CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        private HObject _currentRegionDisplay = new HObject();
        public HObject CurrentRegionDisplay
        {
            get => _currentRegionDisplay;
            set => SetProperty(ref _currentRegionDisplay, value);
        }

        // ===== 对外暴露的属性（供父级 TempAndCaliDebugViewModel 使用） =====

        /// <summary>Halcon 窗口属性（供父级协调器直接设置）</summary>
        public HWindow HalconWindow
        {
            get => _halconWindow;
            set => SetHalconWindow(value);
        }

        /// <summary>当前 ROI 区域（含掩膜处理后的最终区域），供模板匹配使用</summary>
        public HObject FinalRegion => _currentRegionDisplay;

        /// <summary>模型区域（供父级协调器 OnRun 时设置 ROI）</summary>
        public HObject ModelRegion
        {
            get => _currentRegionDisplay;
            set
            {
                _currentRegionDisplay?.Dispose();
                _currentRegionDisplay = value ?? new HObject();
                if (!_currentRegionDisplay.IsInitialized())
                    _currentRegionDisplay.GenEmptyObj();
                RaisePropertyChanged(nameof(CurrentRegionDisplay));
                RaisePropertyChanged(nameof(FinalRegion));
            }
        }

        /// <summary>当前是否处于自定义交互模式（掩膜编辑或多边形绘制），用于禁用 HMoveContent</summary>
        public bool IsCustomMode => IsMaskEditing || IsDrawingPolygon;

        /// <summary>当前鼠标是否在图像区域内（用于右键菜单坐标定位）</summary>
        public bool IsMouseInImage { get; set; }

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

        public List<string> MetricOptions { get; }
        public List<string> OptimizationOptions { get; }
        public List<string> SubPixelOptions { get; }

        private HTuple _currentModelId = null;

        // ===== 命令 =====
        public DelegateCommand CreateModelCommand { get; }
        public DelegateCommand FindModelCommand { get; }
        public DelegateCommand SaveModelCommand { get; }
        public DelegateCommand LoadModelCommand { get; }
        public DelegateCommand DrawRect1Command { get; }
        public DelegateCommand DrawRect2Command { get; }
        public DelegateCommand DrawCircleCommand { get; }
        public DelegateCommand DrawPolygonCommand { get; }
        public DelegateCommand ClearDrawingCommand { get; }
        public DelegateCommand ClearMaskCommand { get; }

        public TemplateMatchingViewModel(ITemplateMatchingService templateMatchingService)
        {
            _templateMatchingService = templateMatchingService;

            _accumulatedMaskRegion.GenEmptyObj();
            _accumulatedEraseRegion.GenEmptyObj();
            CurrentImage.GenEmptyObj();
            CurrentRegionDisplay.GenEmptyObj();

            MetricOptions = new List<string> { "use_polarity", "ignore_global_polarity", "ignore_local_polarity", "ignore_color_polarity" };
            OptimizationOptions = new List<string> { "none", "point_reduction_low", "point_reduction_medium", "point_reduction_high", "pregeneration", "no_pregeneration" };
            SubPixelOptions = new List<string> { "none", "interpolation", "least_squares", "least_squares_high", "least_squares_very_high" };

            CmMetric = MetricOptions[0];
            CmOptimization = OptimizationOptions[0];
            FmSubPixel = SubPixelOptions[0];

            // 命令绑定
            DrawRect1Command = new DelegateCommand(() => StartDraw(RoiType.RECTANGLE1, "red"));
            DrawRect2Command = new DelegateCommand(() => StartDraw(RoiType.RECTANGLE2, "green"));
            DrawCircleCommand = new DelegateCommand(() => StartDraw(RoiType.CIRCLE, "yellow"));
            DrawPolygonCommand = new DelegateCommand(StartPolygonDraw);
            ClearDrawingCommand = new DelegateCommand(ClearDrawing);
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

        #region 形状绘制（基于 Halcon DrawingObject，参考 PreviousTMViewModel）
        private void StartDraw(RoiType type, string color)
        {
            if (_halconWindow == null || IsMaskEditing) return;

            _currentRoi.DetachDrawingObject();
            _currentRoi.Style = type;
            _currentRoi.Color = color;

            // 以鼠标右键点击位置为中心，设置默认大小
            double size = 100;
            double col = _currentMouseCol;
            double row = _currentMouseRow;

            switch (type)
            {
                case RoiType.RECTANGLE1:
                    _currentRoi.LeftX = col - size / 2;
                    _currentRoi.LeftY = row - size / 2;
                    _currentRoi.RightX = col + size / 2;
                    _currentRoi.RightY = row + size / 2;
                    _currentRoi.X = col;
                    _currentRoi.Y = row;
                    break;
                case RoiType.RECTANGLE2:
                    _currentRoi.X = col;
                    _currentRoi.Y = row;
                    _currentRoi.Length1 = size / 2;
                    _currentRoi.Length2 = size / 2;
                    _currentRoi.Angle = 0;
                    break;
                case RoiType.CIRCLE:
                    _currentRoi.X = col;
                    _currentRoi.Y = row;
                    _currentRoi.Radius = size / 2;
                    break;
            }

            if (!_currentRoi.AttachDrawingObject(_halconWindow))
                StatusMessage = $"无法创建 {type} 绘图对象";
            else
                StatusMessage = $"绘制 {type}：拖动调整大小和位置";
        }

        private void StartPolygonDraw()
        {
            if (_halconWindow == null || IsMaskEditing) return;
            EndPolygonDraw();
            _currentRoi.DetachDrawingObject();
            _currentRoi.Style = RoiType.POLYGON;
            _currentRoi.Color = "cyan";
            _polygonTempRows.Clear();
            _polygonTempCols.Clear();
            _isDrawingPolygon = true;
            StatusMessage = "多边形绘制：左键添加顶点，右键闭合结束";
        }

        public void AddPolygonPoint(double row, double col)
        {
            if (!_isDrawingPolygon) return;
            _polygonTempRows.Add(row);
            _polygonTempCols.Add(col);
            DrawTempPolygon();
        }

        /// <summary>右键调用：完成多边形闭合</summary>
        public void FinishPolygon()
        {
            if (!_isDrawingPolygon || _polygonTempRows.Count < 3)
            {
                StatusMessage = "多边形至少需要3个顶点";
                _isDrawingPolygon = false;
                return;
            }
            _currentRoi.SetPolygonVertices(new HTuple(_polygonTempRows.ToArray()), new HTuple(_polygonTempCols.ToArray()));
            _currentRoi.GenerateRegion();
            _isDrawingPolygon = false;
            UpdateFinalRegionDisplay();

            // 刷新窗口显示闭合后的多边形区域
            DisplayImagePreserveZoom();
            if (_halconWindow != null && CurrentRegionDisplay != null && CurrentRegionDisplay.IsInitialized())
            {
                _halconWindow.SetColor("green");
                _halconWindow.SetDraw("margin");
                _halconWindow.SetLineWidth(2);
                _halconWindow.DispObj(CurrentRegionDisplay);
            }
            StatusMessage = "多边形绘制完成，可创建模板";
        }

        private void EndPolygonDraw()
        {
            if (IsDrawingPolygon)
            {
                IsDrawingPolygon = false;
                if (_halconWindow != null)
                {
                    DisplayImagePreserveZoom();
                }
            }
        }

        private void DrawTempPolygon()
        {
            if (_halconWindow == null || _polygonTempRows.Count < 2) return;
            HOperatorSet.SetSystem("flush_graphic", "false");
            DisplayImagePreserveZoom();
            _halconWindow.SetColor("magenta");
            _halconWindow.SetLineWidth(1);
            double[] rows = _polygonTempRows.ToArray();
            double[] cols = _polygonTempCols.ToArray();
            for (int i = 0; i < rows.Length - 1; i++)
                _halconWindow.DispLine(rows[i], cols[i], rows[i + 1], cols[i + 1]);
            for (int i = 0; i < rows.Length; i++)
                _halconWindow.DispCross(rows[i], cols[i], 6, 0);
            HOperatorSet.SetSystem("flush_graphic", "true");
        }

        private void ClearDrawing()
        {
            _currentRoi.DetachDrawingObject();
            if (_halconWindow != null)
            {
                _halconWindow.ClearWindow();
                DisplayImagePreserveZoom();
            }
            CurrentRegionDisplay?.Dispose();
            CurrentRegionDisplay = new HObject();
            CurrentRegionDisplay.GenEmptyObj();
            RaisePropertyChanged(nameof(CurrentRegionDisplay));
            StatusMessage = "绘图已清除";
        }
        #endregion

        #region 掩膜编辑（参考 PreviousTMViewModel 的涂抹/擦除设计）
        private void EnterMaskEdit()
        {
            if (_halconWindow == null) return;
            // 如果当前有可拖拽的绘图对象，同步参数并生成 Region
            if (_currentRoi.Style != RoiType.POLYGON)
            {
                _currentRoi.SyncFromDrawingObject();
                _currentRoi.GenerateRegion();
            }
            _currentRoi.DetachDrawingObject();
            ClearErasePreview();
            StatusMessage = IsEraseMode ? "擦除模式：按住鼠标左键拖动擦除已有掩膜" : "掩膜编辑：按住鼠标左键拖动绘制掩膜";
            RefreshDisplayWithMask();
        }

        private void ExitMaskEdit()
        {
            _isMouseDown = false;
            IsEraseMode = false;
            ClearErasePreview();
            StatusMessage = "掩膜编辑已退出，可创建模板";
            UpdateFinalRegionDisplay();
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
            {
                ApplyErase();
            }
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
            else
                UpdateFinalRegionDisplay();
            StatusMessage = "掩膜已清除";
        }

        private void RefreshDisplayWithMask()
        {
            if (_halconWindow == null) return;
            DisplayImagePreserveZoom();

            // 绘制基本 ROI 轮廓（如果存在）
            if (_currentRoi.Region != null && _currentRoi.Region.IsInitialized())
            {
                _halconWindow.SetColor("green");
                _halconWindow.SetDraw("margin");
                _halconWindow.SetLineWidth(2);
                _halconWindow.DispObj(_currentRoi.Region);
            }

            // 绘制掩膜半透明红
            if (_accumulatedMaskRegion != null && _accumulatedMaskRegion.IsInitialized())
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

        private void UpdateFinalRegionDisplay()
        {
            if (_currentRoi.Region == null || !_currentRoi.Region.IsInitialized())
                return;

            HObject finalRegion = _currentRoi.Region;
            if (_accumulatedMaskRegion != null && _accumulatedMaskRegion.IsInitialized() &&
                _accumulatedMaskRegion.CountObj() > 0)
            {
                HObject diffRegion = new HObject();
                HOperatorSet.Difference(finalRegion, _accumulatedMaskRegion, out diffRegion);
                finalRegion = diffRegion;
            }

            CurrentRegionDisplay?.Dispose();
            CurrentRegionDisplay = finalRegion;
            RaisePropertyChanged(nameof(CurrentRegionDisplay));
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
            // 同步非多边形参数
            if (_currentRoi.Style != RoiType.POLYGON)
                _currentRoi.SyncFromDrawingObject();
            _currentRoi.GenerateRegion();

            UpdateFinalRegionDisplay();

            if (CurrentRegionDisplay == null || !CurrentRegionDisplay.IsInitialized())
            {
                StatusMessage = "无有效区域";
                return;
            }

            try
            {
                HOperatorSet.ReduceDomain(CurrentImage, CurrentRegionDisplay, out HObject templateImage);

                double.TryParse(CmAngleStart, out double angleStart);
                double.TryParse(CmAngleExtent, out double angleExtent);

                // 构建字符串参数
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

        private void FindModel()
        {
            if (_currentModelId == null)
            {
                StatusMessage = "请先创建模板";
                return;
            }
            if (CurrentImage == null || !CurrentImage.IsInitialized())
            {
                StatusMessage = "无图像";
                return;
            }

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
            if (_currentModelId == null)
            {
                StatusMessage = "请先创建模板";
                return;
            }

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
                catch (Exception ex)
                {
                    StatusMessage = $"保存失败：{ex.Message}";
                }
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
                catch (Exception ex)
                {
                    StatusMessage = $"加载失败：{ex.Message}";
                }
            }
        }

        private void Cleanup()
        {
            _currentModelId = null;
            _currentRoi?.DetachDrawingObject();
            _accumulatedMaskRegion?.Dispose();
            _accumulatedEraseRegion?.Dispose();
            CurrentRegionDisplay?.Dispose();
            CurrentImage?.Dispose();
        }

        ~TemplateMatchingViewModel()
        {
            Cleanup();
        }
        #endregion
    }
}
using AVS_Common.Model;
using AVS_Service;
using HalconDotNet;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AVS_Modules_Settings.ViewModels
{
    public class TempAndCaliDebugViewModel : BindableBase
    {
        private readonly ITemplateMatchingService _matchingService;
        private HWindow _halconWindow;
        private HObjectRegion _currentRoi;

        private List<double> _polygonTempRows = new List<double>();
        private List<double> _polygonTempCols = new List<double>();

        private bool _isDrawingPolygon = false;
        public bool IsDrawingPolygon
        {
            get => _isDrawingPolygon;
            set => SetProperty(ref _isDrawingPolygon, value);
        }

        /// <summary>
        /// 绑定图像
        /// </summary>
        private HObject _currentImage = new HObject();
        public HObject CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }
        /// <summary>
        /// 绑定区域
        /// </summary>
        private HObject _currentRegionDisplay = new HObject();
        public HObject CurrentRegionDisplay
        {
            get => _currentRegionDisplay;
            set => SetProperty(ref _currentRegionDisplay, value);
        }


        // ========== 掩膜相关 ==========
        private bool _isMaskEditing;
        private double _eraserSize = 10;
        private string _eraserType = "rectangle";
        private HObject _accumulatedMaskRegion = new HObject();
        private bool _isMouseDown;

        public bool IsMaskEditing
        {
            get => _isMaskEditing;
            set
            {
                if (SetProperty(ref _isMaskEditing, value))
                {
                    if (value) EnterMaskEdit();
                    else ExitMaskEdit();
                }
            }
        }



        public double EraserSize { get => _eraserSize; set => SetProperty(ref _eraserSize, value); }
        public string EraserType { get => _eraserType; set => SetProperty(ref _eraserType, value); }

        private string _statusMessage = "右键绘制形状，掩膜编辑剔除干扰";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        private HTuple _currentModelId = null;

        public ICommand LoadImageCommand { get; }
        public ICommand DrawRect1Command { get; }
        public ICommand DrawRect2Command { get; }
        public ICommand DrawCircleCommand { get; }
        public ICommand DrawPolygonCommand { get; }
        public ICommand ClearDrawingCommand { get; }
        public ICommand CreateModelCommand { get; }
        public ICommand FindModelCommand { get; }
        public ICommand ClearMaskCommand { get; }

        public TempAndCaliDebugViewModel(ITemplateMatchingService matchingService)
        {
            _matchingService = matchingService;
            _accumulatedMaskRegion.GenEmptyObj();
            CurrentImage.GenEmptyObj();
            CurrentRegionDisplay.GenEmptyObj();

            LoadImageCommand = new DelegateCommand(LoadImage);
            DrawRect1Command = new DelegateCommand(() => StartDraw(RoiType.RECTANGLE1, "red"));
            DrawRect2Command = new DelegateCommand(() => StartDraw(RoiType.RECTANGLE2, "green"));
            DrawCircleCommand = new DelegateCommand(() => StartDraw(RoiType.CIRCLE, "yellow"));
            DrawPolygonCommand = new DelegateCommand(StartPolygonDraw);
            ClearDrawingCommand = new DelegateCommand(ClearDrawing);
            CreateModelCommand = new DelegateCommand(CreateModel);
            FindModelCommand = new DelegateCommand(FindModel, () => _currentModelId != null);
            ClearMaskCommand = new DelegateCommand(ClearMask);
        }

        private void ClearDrawing()
        {
            _currentRoi.DetachDrawingObject();
            _halconWindow?.ClearWindow();
            _matchingService.DisplayImage();
            CurrentRegionDisplay?.Dispose();
            CurrentRegionDisplay = new HObject();
            CurrentRegionDisplay.GenEmptyObj();
            RaisePropertyChanged(nameof(CurrentRegionDisplay));
            StatusMessage = "绘图已清除";
        }

        private void StartPolygonDraw()
        {
            if (_halconWindow == null || IsMaskEditing) return;
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

        //鼠标右键调用
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
            // 更新显示区域
            UpdateFinalRegionDisplay();
            StatusMessage = "多边形绘制完成，可创建模板";
        }

        // 强制结束绘制（如选择其他形状）
        private void EndPolygonDraw()
        {
            if (IsDrawingPolygon)
            {
                IsDrawingPolygon = false;
                _halconWindow?.DispObj(_currentImage); // 简单重绘原图清除临时线，可优化
            }
        }

        // 绘制临时多边形（未闭合时的折线）
        private void DrawTempPolygon()
        {
            if (_halconWindow == null || _polygonTempRows.Count < 2) return;
            _matchingService.DisplayImage(); // 重绘原图
            _halconWindow.SetColor("magenta");
            _halconWindow.SetLineWidth(1);
            double[] rows = _polygonTempRows.ToArray();
            double[] cols = _polygonTempCols.ToArray();
            for (int i = 0; i < rows.Length - 1; i++)
                _halconWindow.DispLine(rows[i], cols[i], rows[i + 1], cols[i + 1]);
            for (int i = 0; i < rows.Length; i++)
                _halconWindow.DispCross(rows[i], cols[i], 6, 0);
        }

        private void LoadImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    _matchingService.LoadImage(ofd.FileName);
                    CurrentImage = _matchingService.GetCurrentImage()?.Clone(); // 克隆一份用于显示
                    ClearMask();
                    StatusMessage = $"已加载：{ofd.FileName}";
                }
                catch (Exception ex) { StatusMessage = $"加载失败：{ex.Message}"; }
            }
        }

        public void SetHalconWindow(HWindow window)
        {
            _halconWindow = window;
            _matchingService.SetHalconWindow(window);
        }

        private void StartDraw(RoiType type, string color)
        {
            if (_halconWindow == null || IsMaskEditing) return;
            _currentRoi.DetachDrawingObject();
            _currentRoi.Style = type;
            _currentRoi.Color = color;
            if (!_currentRoi.AttachDrawingObject(_halconWindow))
                StatusMessage = $"无法创建 {type} 绘图对象";
            else
                StatusMessage = $"绘制 {type}：拖动调整大小和位置";
        }

        private void ExitMaskEdit()
        {
            _isMouseDown = false;
            StatusMessage = "掩膜编辑已退出，可创建模板";
            UpdateFinalRegionDisplay();
        }

        private void EnterMaskEdit()
        {
            if (_halconWindow == null) return;
            _currentRoi.DetachDrawingObject();
            StatusMessage = "掩膜编辑：按住鼠标左键拖动擦除干扰区域";
            RefreshDisplayWithMask();
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
            RefreshDisplayWithMask();
        }


        private void AddEraserAt(double row, double col)
        {
            HObject eraser;
            if (EraserType == "rectangle")
                HOperatorSet.GenRectangle2(out eraser, row, col, 0, EraserSize, EraserSize);
            else
                HOperatorSet.GenCircle(out eraser, row, col, EraserSize);

            HObject temp = new HObject();
            HOperatorSet.Union2(_accumulatedMaskRegion, eraser, out temp);
            _accumulatedMaskRegion.Dispose();
            _accumulatedMaskRegion = temp;
            eraser.Dispose();
            RefreshDisplayWithMask();
        }

        private void RefreshDisplayWithMask()
        {
            if (_halconWindow == null) return;
            _matchingService.DisplayImage();

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
                _halconWindow.SetColor("red");
                _halconWindow.SetDraw("fill");
                _halconWindow.SetLineWidth(1);
                _halconWindow.DispObj(_accumulatedMaskRegion);
            }
        }

        public void ClearMask()
        {
            _accumulatedMaskRegion?.Dispose();
            _accumulatedMaskRegion = new HObject();
            _accumulatedMaskRegion.GenEmptyObj();
            if (IsMaskEditing)
                RefreshDisplayWithMask();
            else
                UpdateFinalRegionDisplay();
            StatusMessage = "掩膜已清除";
        }

        // 更新最终区域显示（ROI - 掩膜）
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

        // ==================== 模板操作 ====================
        private void CreateModel()
        {
            // 同步非多边形参数
            if (_currentRoi.Style != RoiType.POLYGON)
                _currentRoi.SyncFromDrawingObject();
            _currentRoi.GenerateRegion();

            UpdateFinalRegionDisplay(); // 确保 CurrentRegionDisplay 是最新的差集

            if (CurrentRegionDisplay == null || !CurrentRegionDisplay.IsInitialized())
            {
                StatusMessage = "无有效区域";
                return;
            }

            try
            {
                _currentModelId = _matchingService.CreateShapeModel(CurrentRegionDisplay);
                StatusMessage = "模板创建成功";
            }
            catch (Exception ex)
            {
                StatusMessage = $"创建模板失败：{ex.Message}";
            }
        }

        private void FindModel()
        {
            if (_currentModelId == null) return;
            try
            {
                _matchingService.FindShapeModel(_currentModelId, out HTuple row, out HTuple col, out HTuple angle, out HTuple score);
                if (score > 0)
                {
                    _matchingService.DisplayResult(row, col, angle, score);
                    StatusMessage = $"找到模板: Row={row:F2}, Col={col:F2}, Angle={angle:F2}, Score={score:F2}";
                }
                else
                    StatusMessage = "未找到模板";
            }
            catch (Exception ex)
            {
                StatusMessage = $"查找失败：{ex.Message}";
            }
        }

        public void Dispose()
        {
            _currentRoi.Dispose();
            _accumulatedMaskRegion?.Dispose();
            CurrentImage?.Dispose();
            CurrentRegionDisplay?.Dispose();
        }
    }
}

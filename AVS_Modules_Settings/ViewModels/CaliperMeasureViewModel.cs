using AVS_Common.Model;
using AVS_Service;
using HalconDotNet;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AVS_Modules_Settings.ViewModels
{
    /// <summary>
    /// 卡尺测量结果项
    /// </summary>
    public class CaliperResultItem
    {
        public int Index { get; set; }
        public double Row { get; set; }
        public double Col { get; set; }
        public double Amplitude { get; set; }
        public double IntraDistance { get; set; }
        public double InterDistance { get; set; }
    }

    public class CaliperMeasureViewModel : BindableBase
    {
        private readonly ICaliperService _caliperService;
        private HTuple _currentMeasureHandle;
        public DelegateCommand MeasureCommand { get; }
        public CaliperMeasureViewModel(ICaliperService caliperService)
        {
            _caliperService = caliperService;

            // 初始化选项列表
            SelectMeasureOptions = new List<string> { "距离", "宽度", "Row坐标", "Col坐标" };
            InterpolationOptions = new List<string> { "最近邻", "双线性", "双三次" };

            // 默认值
            CaliperSigma = "1";
            CaliperThreshold = "30";
            CaliperScale = "1";
            //CaliperWidth = "10";
            //CaliperHeight = "20";
            CaliperInterpolation = "双线性";
            MeasureCommand = new DelegateCommand(OnMeasure);
        }



        private HObjectRegion _currentRoi;
        public HObjectRegion CurrentRoi
        {
            get => _currentRoi;
            set => _currentRoi = value;
        }

        // ===== 边缘参数 =====
        private bool _caliperIsEdge;
        public bool CaliperIsEdge { get => _caliperIsEdge; set => SetProperty(ref _caliperIsEdge, value); }

        private string _caliperSigma;
        public string CaliperSigma { get => _caliperSigma; set => SetProperty(ref _caliperSigma, value); }

        private string _caliperThreshold;
        public string CaliperThreshold { get => _caliperThreshold; set => SetProperty(ref _caliperThreshold, value); }

        private int _caliperTransition; // 0=白到黑 1=黑到白 2=全部
        public int CaliperTransition { get => _caliperTransition; set => SetProperty(ref _caliperTransition, value); }

        private int _caliperSelect; // 0=第一个 1=最后一个 2=全部
        public int CaliperSelect { get => _caliperSelect; set => SetProperty(ref _caliperSelect, value); }

        //// ===== 卡尺ROI尺寸 =====
        //private string _caliperWidth;
        //public string CaliperWidth { get => _caliperWidth; set => SetProperty(ref _caliperWidth, value); }

        //private string _caliperHeight;
        //public string CaliperHeight { get => _caliperHeight; set => SetProperty(ref _caliperHeight, value); }

        // ===== 测量设置 =====
        private int _caliperSelectMeasure;
        public int CaliperSelectMeasure { get => _caliperSelectMeasure; set => SetProperty(ref _caliperSelectMeasure, value); }

        private string _caliperScale;
        public string CaliperScale { get => _caliperScale; set => SetProperty(ref _caliperScale, value); }

        private string _caliperInterpolation;
        public string CaliperInterpolation { get => _caliperInterpolation; set => SetProperty(ref _caliperInterpolation, value); }

        // ===== 显示选项 =====
        private bool _caliperIsRule;
        public bool CaliperIsRule { get => _caliperIsRule; set => SetProperty(ref _caliperIsRule, value); }

        private bool _caliperIsLine;
        public bool CaliperIsLine { get => _caliperIsLine; set => SetProperty(ref _caliperIsLine, value); }

        private bool _caliperIsCross;
        public bool CaliperIsCross { get => _caliperIsCross; set => SetProperty(ref _caliperIsCross, value); }

        // ===== 判定条件 =====
        private bool _caliperIsMeasure;
        public bool CaliperIsMeasure { get => _caliperIsMeasure; set => SetProperty(ref _caliperIsMeasure, value); }

        private string _caliperLowMeasure;
        public string CaliperLowMeasure { get => _caliperLowMeasure; set => SetProperty(ref _caliperLowMeasure, value); }

        private string _caliperHighMeasure;
        public string CaliperHighMeasure { get => _caliperHighMeasure; set => SetProperty(ref _caliperHighMeasure, value); }

        // ===== FIXTURE =====
        private bool _caliperIsFixture;
        public bool CaliperIsFixture { get => _caliperIsFixture; set => SetProperty(ref _caliperIsFixture, value); }

        private string _caliperFixtureName;
        public string CaliperFixtureName { get => _caliperFixtureName; set => SetProperty(ref _caliperFixtureName, value); }

        // ===== 测量结果 =====
        private ObservableCollection<CaliperResultItem> _caliperResults = new ObservableCollection<CaliperResultItem>();
        public ObservableCollection<CaliperResultItem> CaliperResults
        {
            get => _caliperResults;
            set => SetProperty(ref _caliperResults, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        // ===== 选项列表 =====
        public List<string> SelectMeasureOptions { get; }
        public List<string> InterpolationOptions { get; }

        public HWindow HalconWindow { get; set; }
        public HObject CurrentImage { get; set; }
        public HRegion MeasureRegion { get; set; }

        /// <summary>
        /// 用 Rectangle2 绘制并生成卡尺测量句柄（带 ROI 显示）
        /// 返回生成的轮廓用于显示 ROI
        /// </summary>
        public HObject GenAndGetMeasureRegion(double row, double col, double phi, double length1, double length2)
        {
            //double width = ParseDouble(CaliperWidth, 10);
            //double height = ParseDouble(CaliperHeight, 20);
            //string interp = GetInterpolationValue();

            //// 关闭旧句柄
            //CloseCurrentMeasure();

            //_currentMeasureHandle = _caliperService.GenMeasureRectangle2(
            //    row, col, phi, length1, length2, width, height, interp, out HObject region);

            //return region;
            return null; // 目前改为在 MeasureWithRect2 内直接生成并显示 ROI，GenAndGetMeasureRegion 不再使用
        }

        /// <summary>
        /// 执行当前卡尺测量（使用已生成的 measureHandle）
        /// </summary>
        public void MeasureWithRect2(double row, double col, double phi, double length1, double length2)
        {
            if (CurrentImage == null) return;

            HOperatorSet.GetImageSize(CurrentImage, out HTuple imgWidth, out HTuple imgHeight);
            string interp = GetInterpolationValue();
            _caliperService.SetImage(CurrentImage);
            _caliperService.SetHalconWindow(HalconWindow);
            // 关闭旧句柄
            CloseCurrentMeasure();

            // 生成新的测量句柄
            _currentMeasureHandle = _caliperService.GenMeasureRectangle2(row, col, phi, length1, length2, imgWidth, imgHeight, interp, out HObject _);

            try
            {
                double sigma = ParseDouble(CaliperSigma);
                int threshold = ParseInt(CaliperThreshold);
                double scale = ParseDouble(CaliperScale, 1.0);

                // 修复：CaliperTransition 是 int (0=Positive, 1=Negative, 2=All)
                // 枚举值 Positive=1, Negative=2, All=3
                CaliperTransition trans = GetTransitionEnum();
                CaliperSelect sel = GetSelectEnum();

                var results = new ObservableCollection<CaliperResultItem>();

                if (CaliperIsEdge)
                {
                    _caliperService.MeasureCaliperEdgePairs(
                        _currentMeasureHandle, sigma, threshold, trans, sel,
                        out HTuple rows1, out HTuple cols1, out HTuple amp1,
                        out HTuple rows2, out HTuple cols2, out HTuple amp2,
                        out HTuple interDist, out HTuple intraDist);

                    if (rows1 != null && rows1.Length > 0)
                    {
                        int count = Math.Min(rows1.Length, rows2.Length);
                        for (int i = 0; i < count; i++)
                        {
                            results.Add(new CaliperResultItem
                            {
                                Index = i + 1,
                                Row = rows1[i].D,
                                Col = cols1[i].D,
                                Amplitude = amp1[i].D,
                                IntraDistance = i < intraDist.Length ? intraDist[i].D * scale : 0,
                                InterDistance = i < interDist.Length ? interDist[i].D * scale : 0
                            });
                        }
                    }

                    // 显示边缘对结果
                    _caliperService.DisplayEdgePairResult(
                        rows1, cols1, amp1, rows2, cols2, amp2,
                        CaliperIsRule, CaliperIsLine, CaliperIsCross);
                }
                else
                {
                    _caliperService.MeasureCaliper(
                        _currentMeasureHandle, sigma, threshold, trans, sel,
                        out HTuple rows, out HTuple cols, out HTuple amps, out HTuple dists);

                    if (rows != null && rows.Length > 0)
                    {
                        for (int i = 0; i < rows.Length; i++)
                        {
                            results.Add(new CaliperResultItem
                            {
                                Index = i + 1,
                                Row = rows[i].D,
                                Col = cols[i].D,
                                Amplitude = amps[i].D
                            });
                        }
                    }

                    // 显示单边缘结果
                    _caliperService.DisplaySingleEdgeResult(
                        rows, cols, amps,
                        CaliperIsRule, CaliperIsLine, CaliperIsCross);
                }

                CaliperResults = results;
            }
            catch (HalconException)
            {
                CaliperResults = new ObservableCollection<CaliperResultItem>();
            }
        }

        private void CloseCurrentMeasure()
        {
            if (_currentMeasureHandle != null)
            {
                _caliperService.CloseMeasure(_currentMeasureHandle);
                _currentMeasureHandle = null;
            }
        }

        private CaliperTransition GetTransitionEnum()
        {
            switch (CaliperTransition)
            {
                case 0: return AVS_Service.CaliperTransition.Positive;  // 白到黑
                case 1: return AVS_Service.CaliperTransition.Negative;  // 黑到白
                default: return AVS_Service.CaliperTransition.All;      // 全部
            }
        }

        private CaliperSelect GetSelectEnum()
        {
            switch (CaliperSelect)
            {
                case 0: return AVS_Service.CaliperSelect.First;
                case 1: return AVS_Service.CaliperSelect.Last;
                default: return AVS_Service.CaliperSelect.All;
            }
        }

        private string GetInterpolationValue()
        {
            // "最近邻" -> "nearest_neighbor", "双线性" -> "bilinear", "双三次" -> "bicubic"
            int idx = InterpolationOptions?.IndexOf(CaliperInterpolation) ?? 1;
            return idx switch { 0 => "nearest_neighbor", 2 => "bilinear", _ => "bicubic" };
        }

        private double ParseDouble(string s, double defaultVal = 0)
        {
            if (double.TryParse(s, out double val)) return val;
            return defaultVal;
        }

        private int ParseInt(string s, int defaultVal = 0)
        {
            if (int.TryParse(s, out int val)) return val;
            return defaultVal;
        }

        private void OnMeasure()
        {
            if (CurrentImage == null || !CurrentImage.IsInitialized())
            {
                StatusMessage = "请先加载图像";
                return;
            }
            if (_currentRoi == null || _currentRoi.Style != RoiType.RECTANGLE2)
            {
                StatusMessage = "请先绘制并确认旋转矩形 (Rectangle2)";
                return;
            }

            try
            {
                // 从 DrawingObject 同步最新参数并重新生成 Region
                _currentRoi.SyncFromDrawingObject();
                _currentRoi.GenerateRegion();

                // 调用已有的测量逻辑
                MeasureWithRect2(_currentRoi.Y, _currentRoi.X, _currentRoi.Angle, _currentRoi.Length1, _currentRoi.Length2);

                StatusMessage = "测量完成";
            }
            catch (Exception ex)
            {
                StatusMessage = $"测量失败：{ex.Message}";
            }
        }
    }
}
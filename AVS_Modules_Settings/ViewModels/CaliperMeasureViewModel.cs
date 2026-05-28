using AVS_Service;
using HalconDotNet;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

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

        // ===== 选项列表 =====
        public List<string> SelectMeasureOptions { get; }
        public List<string> InterpolationOptions { get; }

        public HWindow HalconWindow { get; set; }
        public HObject CurrentImage { get; set; }
        public HRegion MeasureRegion { get; set; }

        /// <summary>
        /// 执行卡尺测量
        /// </summary>
        public void Measure(HObject region)
        {
            if (CurrentImage == null || region == null) return;

            _caliperService.SetImage(CurrentImage);
            _caliperService.SetHalconWindow(HalconWindow);

            try
            {
                int sigma = ParseInt(CaliperSigma);
                int threshold = ParseInt(CaliperThreshold);
                double scale = ParseDouble(CaliperScale, 1.0);

                CaliperTransition trans = (CaliperTransition)(CaliperTransition + 1);
                CaliperSelect sel = (CaliperSelect)(CaliperSelect + 1);

                var results = new ObservableCollection<CaliperResultItem>();

                if (CaliperIsEdge)
                {
                    // 边缘对模式
                    _caliperService.MeasureCaliperEdgePairs(
                        region, sigma, threshold, trans, sel,
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
                                IntraDistance = i < intraDist.Length ? intraDist[i].D : 0,
                                InterDistance = i < interDist.Length ? interDist[i].D : 0
                            });
                        }
                    }
                }
                else
                {
                    // 单边缘模式
                    _caliperService.MeasureCaliper(
                        region, sigma, threshold, trans, sel,
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
                }

                CaliperResults = results;
            }
            catch (HalconException)
            {
                CaliperResults = new ObservableCollection<CaliperResultItem>();
            }
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
    }
}
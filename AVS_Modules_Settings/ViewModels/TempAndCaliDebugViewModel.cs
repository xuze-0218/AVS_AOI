using AVS_Common.Model;
using AVS_Service;
using HalconDotNet;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;

namespace AVS_Modules_Settings.ViewModels
{
    /// <summary>
    /// 温度与标定调试 - 协调器ViewModel
    /// 统一管理子ViewModel，模板匹配和卡尺测量逻辑委托给子ViewModel，
    /// 区域绘制和掩膜操作完全由 TemplateMatchingViewModel 负责（参照 PreviousTempAndCaliDebugViewModel 设计）。
    /// </summary>
    public class TempAndCaliDebugViewModel : BindableBase
    {
        private readonly ITemplateMatchingService _matchingService;
        private readonly ICaliperService _caliperService;
        private HWindow _halconWindow;

        #region 子ViewModel
        private TemplateMatchingViewModel _templateMatchingVM;
        public TemplateMatchingViewModel TemplateMatchingVM
        {
            get => _templateMatchingVM;
            set => SetProperty(ref _templateMatchingVM, value);
        }

        private CaliperMeasureViewModel _caliperMeasureVM;
        public CaliperMeasureViewModel CaliperMeasureVM
        {
            get => _caliperMeasureVM;
            set => SetProperty(ref _caliperMeasureVM, value);
        }
        #endregion

        #region 构造函数
        public TempAndCaliDebugViewModel(ITemplateMatchingService matchingService, ICaliperService caliperService)
        {
            _matchingService = matchingService;
            _caliperService = caliperService;

            TemplateMatchingVM = new TemplateMatchingViewModel(matchingService);
            CaliperMeasureVM = new CaliperMeasureViewModel(caliperService);

            RunCommand = new DelegateCommand(OnRun);
            ConfirmCommand = new DelegateCommand(OnConfirm);
            CancelCommand = new DelegateCommand(OnCancel);
            LoadImageCommand = new DelegateCommand(OnLoadImage);
            SaveRoiCommand = new DelegateCommand(OnSaveRoi);
            LoadRoiCommand = new DelegateCommand(OnLoadRoi);
            ClearRoiCommand = new DelegateCommand(OnClearRoi);
        }
        #endregion

        #region 公共属性 - 图像与Halcon窗口
        public HWindow HalconWindow
        {
            get => _halconWindow;
            set
            {
                if (SetProperty(ref _halconWindow, value))
                {
                    if (TemplateMatchingVM != null) TemplateMatchingVM.HalconWindow = value;
                    if (CaliperMeasureVM != null) CaliperMeasureVM.HalconWindow = value;
                    _matchingService.SetHalconWindow(value);
                    _caliperService.SetHalconWindow(value);
                }
            }
        }

        private HObject _currentImage = new HObject();
        public HObject CurrentImage
        {
            get => _currentImage;
            set
            {
                if (SetProperty(ref _currentImage, value))
                {
                    _caliperService.SetImage(value);
                    if (TemplateMatchingVM != null) TemplateMatchingVM.CurrentImage = value;
                    if (CaliperMeasureVM != null) CaliperMeasureVM.CurrentImage = value;
                }
            }
        }

        private HObject _currentRegionDisplay = new HObject();
        public HObject CurrentRegionDisplay
        {
            get => _currentRegionDisplay;
            set => SetProperty(ref _currentRegionDisplay, value);
        }

        private HObjectRegion _currentRoi = new HObjectRegion();
        public HObjectRegion CurrentRoi => _currentRoi;

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        private string _cameraRoleName;

        public string CameraRoleName
        {
            get => _cameraRoleName;
            set => SetProperty(ref _cameraRoleName, value);
        }

        private double _currentMouseRow;
        public double CurrentMouseRow { get => _currentMouseRow; set => SetProperty(ref _currentMouseRow, value); }

        private double _currentMouseCol;
        public double CurrentMouseCol { get => _currentMouseCol; set => SetProperty(ref _currentMouseCol, value); }

        private bool _isSearchRegion;
        public bool IsSearchRegion
        {
            get => _isSearchRegion;
            set => SetProperty(ref _isSearchRegion, value);
        }

        /// <summary>当前是否处于自定义交互模式（委托给 TemplateMatchingVM）</summary>
        public bool IsCustomMode => TemplateMatchingVM?.IsCustomMode ?? false;
        #endregion

        #region 公共属性 - 运行结果
        private string _runTime = "0 ms";
        public string RunTime { get => _runTime; set => SetProperty(ref _runTime, value); }

        private string _runResult = "---";
        public string RunResult { get => _runResult; set => SetProperty(ref _runResult, value); }

        private bool _isPass;
        public bool IsPass { get => _isPass; set => SetProperty(ref _isPass, value); }

        private string _modelPath;
        public string ModelPath
        {
            get => TemplateMatchingVM?.ModelPath;
            set
            {
                if (TemplateMatchingVM != null)
                    TemplateMatchingVM.ModelPath = value;
                RaisePropertyChanged();
            }
        }

        private string _roiPath;
        public string RoiPath { get => _roiPath; set => SetProperty(ref _roiPath, value); }
        #endregion

        #region 命令
        public DelegateCommand RunCommand { get; }
        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand LoadImageCommand { get; }
        public DelegateCommand CreateModelCommand { get; }
        public DelegateCommand FindModelCommand { get; }
        public DelegateCommand SaveModelCommand { get; }
        public DelegateCommand LoadModelCommand { get; }
        public DelegateCommand SaveRoiCommand { get; }
        public DelegateCommand LoadRoiCommand { get; }
        public DelegateCommand ClearRoiCommand { get; }
        #endregion

        #region Halcon窗口设置
        public void SetHalconWindow(HWindow window)
        {
            HalconWindow = window;
        }
        #endregion

        #region 命令实现
        private void OnRun()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (TemplateMatchingVM != null)
                {
                    TemplateMatchingVM.ModelRegion = _currentRoi.Region;
                    TemplateMatchingVM.FindModelCommand?.Execute();
                }
                if (CaliperMeasureVM != null && _currentRoi.Region != null && _currentRoi.Region.IsInitialized())
                {
                    CaliperMeasureVM.Measure(_currentRoi.Region);
                }
                RunResult = "OK";
                IsPass = true;
                RedrawImage();
            }
            catch (Exception ex)
            {
                RunResult = "ERROR";
                IsPass = false;
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            sw.Stop();
            RunTime = $"{sw.ElapsedMilliseconds} ms";
        }

        private void OnConfirm()
        {
            //更新并锁定当前 ROI
            if (TemplateMatchingVM != null)
            {
                // 同步模板匹配VM中的Region到协调器
                var finalRegion = TemplateMatchingVM.FinalRegion;
                if (finalRegion != null && finalRegion.IsInitialized())
                {
                    _currentRoi.Region?.Dispose();
                    _currentRoi.Region = finalRegion.Clone();
                }
            }
            RedrawImage();
        }

        private void OnCancel()
        {
            // 取消操作：重绘图像
            RedrawImage();
        }

        private void OnLoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.png;*.tiff;*.tif|All Files|*.*",
                Title = "选择图像文件"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ImagePath = openFileDialog.FileName;
                    var image = _matchingService.LoadImage(ImagePath);
                    if (image != null)
                        CurrentImage = image.Clone();
                    RedrawImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载图像失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnSaveRoi()
        {
            if (_currentRoi.Region == null || !_currentRoi.Region.IsInitialized())
            {
                MessageBox.Show("没有可保存的ROI区域", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Region Files|*.reg|All Files|*.*",
                Title = "保存ROI文件"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    HOperatorSet.WriteRegion(_currentRoi.Region, saveFileDialog.FileName);
                    RoiPath = saveFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存ROI失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnLoadRoi()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Region Files|*.reg|All Files|*.*",
                Title = "加载ROI文件"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    HOperatorSet.ReadRegion(out HObject region, openFileDialog.FileName);
                    _currentRoi.Region = region;
                    RoiPath = openFileDialog.FileName;
                    RedrawImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载ROI失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnClearRoi()
        {
            if (_currentRoi.Region != null && _currentRoi.Region.IsInitialized())
            {
                _currentRoi.Region.Dispose();
            }
            _currentRoi.Region = new HObject();
            RoiPath = null;
            RedrawImage();
        }
        #endregion

        #region 图像显示辅助
        private void RedrawImage()
        {
            if (_halconWindow == null || CurrentImage == null || !CurrentImage.IsInitialized())
                return;
            try
            {
                _halconWindow.ClearWindow();
                _halconWindow.DispObj(CurrentImage);
                if (_currentRoi.Region != null && _currentRoi.Region.IsInitialized())
                {
                    _halconWindow.SetColor("green");
                    _halconWindow.SetDraw("margin");
                    _halconWindow.SetLineWidth(2);
                    _halconWindow.DispObj(_currentRoi.Region);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RedrawImage error: {ex.Message}");
            }
        }
        #endregion
    }
}
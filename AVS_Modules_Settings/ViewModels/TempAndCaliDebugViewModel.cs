using AVS_Common.Events;
using AVS_Service.Models;
using AVS_Service.Services;
using HalconDotNet;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AVS_Modules_Settings.ViewModels
{
    /// <summary>
    /// 模板与卡尺协调器
    /// 统一管理子ViewModel和ROI绘制，
    /// 掩膜编辑仍由 TemplateMatchingViewModel 负责，
    /// ROI绘制提升至协调器层供模板匹配和卡尺测量共用。
    /// </summary>
    public class TempAndCaliDebugViewModel : BindableBase
    {
        private readonly ITemplateMatchingService _matchingService;
        private HWindow _halconWindow;
        private readonly ICameraConfigService _cameraService;
        private readonly IStationConfigService _stationConfigService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IParametersConfigService _parametersConfigService;

        #region 子ViewModel
        private TemplateMatchingViewModel _templateMatchingVM;
        public TemplateMatchingViewModel TemplateMatchingVM
        {
            get => _templateMatchingVM;
            set => SetProperty(ref _templateMatchingVM, value);
        }

        //private CaliperMeasureViewModel _caliperMeasureVM;

        //public CaliperMeasureViewModel CaliperMeasureVM
        //{
        //    get => _caliperMeasureVM;
        //    set => SetProperty(ref _caliperMeasureVM, value);
        //}
        private MetrologyViewModel _metrologyVM;
        public MetrologyViewModel MetrologyVM
        {
            get => _metrologyVM;
            set => SetProperty(ref _metrologyVM, value);
        }
        #endregion

        #region 构造函数
        public TempAndCaliDebugViewModel(ITemplateMatchingService matchingService, IMetrologyService metrologyService,
            ICameraConfigService cameraService, IStationConfigService stationConfigService, IEventAggregator eventAggregator,
            IParametersConfigService parametersConfigService
            )
        {
            _parametersConfigService = parametersConfigService;
            _matchingService = matchingService;
            _cameraService = cameraService;
            _stationConfigService = stationConfigService;
            _eventAggregator = eventAggregator;

            TemplateMatchingVM = new TemplateMatchingViewModel(matchingService);
            MetrologyVM = new MetrologyViewModel(metrologyService, _parametersConfigService);
            MetrologyVM.RequestMatch = () =>
            {
                TemplateMatchingVM.FindModelCommand?.Execute();
                var best = TemplateMatchingVM.BestMatch;
                if (best != null)
                {
                    MetrologyVM.SetMatchResult(best.Row, best.Col, best.Angle);
                    return true;
                }
                return false;
            };
            // 将当前 ROI 对象引用注入子 ViewModel，使模板匹配/掩膜编辑可独立同步 DrawingObject
            TemplateMatchingVM.ActiveRoi = _currentRoi;
            MetrologyVM.CurrentRoi = _currentRoi;

            // ===== 命令 =====
            RunCommand = new DelegateCommand(OnRun);
            LoadImageCommand = new DelegateCommand(OnLoadImage);
            SaveRoiCommand = new DelegateCommand(OnSaveRoi);
            LoadRoiCommand = new DelegateCommand(OnLoadRoi);
            ClearRoiCommand = new DelegateCommand(OnClearRoi);
            SetMetroRefCommand = new DelegateCommand(OnSetMetroRef);

            // ===== ROI绘制命令（迁入协调器层） =====
            DrawRect1Command = new DelegateCommand(() => StartDraw(RoiType.RECTANGLE1, "red"));
            DrawRect2Command = new DelegateCommand(() => StartDraw(RoiType.RECTANGLE2, "green"));
            DrawCircleCommand = new DelegateCommand(() => StartDraw(RoiType.CIRCLE, "yellow"));
            DrawLineCommand = new DelegateCommand(() => StartDraw(RoiType.LINE, "orange"));
            DrawEllipseCommand = new DelegateCommand(() => StartDraw(RoiType.ELLIPSE, "magenta"));
            DrawPolygonCommand = new DelegateCommand(StartPolygonDraw);
            ClearDrawingCommand = new DelegateCommand(ClearDrawing);
            ConfirmRoiCommand = new DelegateCommand(OnConfirmRoi);
            GrabFromCameraCommand = new DelegateCommand(async () => await OnGrabFromCameraAsync());
            LoadAvailableStations();
            _eventAggregator.GetEvent<HImageDisplayEvent>().Subscribe(OnFrameReceived, ThreadOption.PublisherThread, false);
            // ===== 初始化多边形数据 =====
            _polygonTempRows = new List<double>();
            _polygonTempCols = new List<double>();
        }
        #endregion

        #region 公共属性 - 图像与Halcon窗口

        public ObservableCollection<StationConfig> AvailableStations { get; } = new ObservableCollection<StationConfig>();

        private StationConfig _selectedStation;
        public StationConfig SelectedStation
        {
            get => _selectedStation;
            set => SetProperty(ref _selectedStation, value);
        }

        // 帧缓存 
        private readonly object _frameLock = new object();
        private readonly Dictionary<string, HObject> _latestFrames = new Dictionary<string, HObject>();

        // 当前相机角色名
        private string _cameraRoleName;
        public string CameraRoleName
        {
            get => _cameraRoleName;
            set => SetProperty(ref _cameraRoleName, value);
        }

        // CameraRole  SerialNumber 缓存，避免每次都遍历 AllSettings
        private readonly Dictionary<string, string> _roleToSn = new Dictionary<string, string>();
        public HWindow HalconWindow
        {
            get => _halconWindow;
            set
            {
                if (ReferenceEquals(_halconWindow, value)) return;
                if (SetProperty(ref _halconWindow, value))
                {
                    if (TemplateMatchingVM != null) TemplateMatchingVM.HalconWindow = value;
                    //if (CaliperMeasureVM != null) CaliperMeasureVM.HalconWindow = value;
                    if (MetrologyVM != null) MetrologyVM.HalconWindow = value;
                    _matchingService.SetHalconWindow(value);
                    //_caliperService.SetHalconWindow(value);
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
                    //_caliperService.SetImage(value);
                    if (TemplateMatchingVM != null) TemplateMatchingVM.CurrentImage = value;
                    //if (CaliperMeasureVM != null) CaliperMeasureVM.CurrentImage = value;
                    if (MetrologyVM != null) MetrologyVM.CurrentImage = value;
                }
            }
        }

        private HObject _rawImage = new HObject();

        private bool _enableEnhance;
        public bool EnableEnhance
        {
            get => _enableEnhance;
            set
            {
                if (SetProperty(ref _enableEnhance, value))
                    ApplyEnhancement();
            }
        }

        private double _enhanceMult = 1.5;
        /// <summary>scale_image 的 Mult 系数</summary>
        public double EnhanceMult
        {
            get => _enhanceMult;
            set
            {
                if (SetProperty(ref _enhanceMult, value))
                    ApplyEnhancement();
            }
        }

        private double _enhanceAdd = -50;
        /// <summary>scale_image 的 Add 偏移</summary>
        public double EnhanceAdd
        {
            get => _enhanceAdd;
            set
            {
                if (SetProperty(ref _enhanceAdd, value))
                    ApplyEnhancement();
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

        private double _currentMouseRow;
        public double CurrentMouseRow { get => _currentMouseRow; set => SetProperty(ref _currentMouseRow, value); }

        private double _currentMouseCol;
        public double CurrentMouseCol { get => _currentMouseCol; set => SetProperty(ref _currentMouseCol, value); }

        private bool _isRoiVisible = true;
        /// <summary>控制绿色 ROI 区域在图像上的显示/隐藏</summary>
        public bool IsRoiVisible
        {
            get => _isRoiVisible;
            set
            {
                if (SetProperty(ref _isRoiVisible, value))
                    RedrawImage();
            }
        }

        // ===== ROI 绘制相关 =====
        private List<double> _polygonTempRows;
        private List<double> _polygonTempCols;

        private bool _isDrawingPolygon;
        public bool IsDrawingPolygon
        {
            get => _isDrawingPolygon;
            set
            {
                if (SetProperty(ref _isDrawingPolygon, value))
                    RaisePropertyChanged(nameof(IsCustomMode));
            }
        }

        /// <summary>
        /// 当前是否处于自定义交互模式（多边形绘制），用于禁用 HMoveContent
        /// 掩膜编辑的 IsCustomMode 仍由 TemplateMatchingVM 提供
        /// </summary>
        public bool IsCustomMode => IsDrawingPolygon || (TemplateMatchingVM?.IsCustomMode ?? false);
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

        private string _statusMessage = "右键图像选择ROI形状，拖动调整";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        #endregion

        #region 命令
        public DelegateCommand RunCommand { get; }
        public DelegateCommand LoadImageCommand { get; }
        public DelegateCommand SaveRoiCommand { get; }
        public DelegateCommand LoadRoiCommand { get; }
        public DelegateCommand ClearRoiCommand { get; }
        public DelegateCommand DrawRect1Command { get; }
        public DelegateCommand DrawRect2Command { get; }
        public DelegateCommand DrawCircleCommand { get; }
        public DelegateCommand DrawLineCommand { get; }
        public DelegateCommand DrawEllipseCommand { get; }
        public DelegateCommand DrawPolygonCommand { get; }
        public DelegateCommand ClearDrawingCommand { get; }
        public DelegateCommand ConfirmRoiCommand { get; }
        public DelegateCommand SetMetroRefCommand { get; }
        public DelegateCommand GrabFromCameraCommand { get; }
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
            var sw = Stopwatch.StartNew();
            try
            {
                RedrawImage();
                // 同步 DrawingObject 参数并生成 Region
                if (_currentRoi.Style != RoiType.POLYGON)
                    _currentRoi.SyncFromDrawingObject();
                _currentRoi.GenerateRegion();

                // 更新协调器的 RegionDisplay
                if (_currentRoi.Region != null && _currentRoi.Region.IsInitialized())
                {
                    CurrentRegionDisplay?.Dispose();
                    CurrentRegionDisplay = _currentRoi.Region.Clone();
                }

                // 模板匹配：使用 ROI Region
                TemplateMatchingVM.FindModelCommand?.Execute();
                var bestMatch = TemplateMatchingVM.BestMatch;
                if (bestMatch != null && MetrologyVM.IsFollowModel)
                {
                    MetrologyVM.MatchRow = bestMatch.Row;
                    MetrologyVM.MatchCol = bestMatch.Col;
                    MetrologyVM.MatchAngle = bestMatch.Angle;
                }
                else
                {
                    MetrologyVM.MatchRow = 0; // 无效标志
                }
                MetrologyVM.MeasureCommand.Execute();
                RunResult = "OK";
                IsPass = true;

            }
            catch (Exception ex)
            {
                RunResult = "ERROR";
                IsPass = false;
                Debug.WriteLine(ex.Message);
            }
            sw.Stop();
            RunTime = $"{sw.ElapsedMilliseconds} ms";
        }

        private void OnSetMetroRef()
        {
            // 尝试在当前图像上执行一次模板匹配，获取模板中心
            TemplateMatchingVM.FindModelCommand?.Execute();
            var best = TemplateMatchingVM.BestMatch;
            if (best != null)
            {
                MetrologyVM.ModelRefRow = best.Row;
                MetrologyVM.ModelRefCol = best.Col;
                MetrologyVM.ModelRefAngle = best.Angle; // 注意角度单位一致性
                StatusMessage = $"参考位姿已设为 Row={best.Row:F2}, Col={best.Col:F2}, Angle={best.Angle:F3}";
            }
            else
            {
                // 如果无法匹配，使用 ROI 中心
                MetrologyVM.ModelRefRow = _currentRoi.Y;
                MetrologyVM.ModelRefCol = _currentRoi.X;
                MetrologyVM.ModelRefAngle = _currentRoi.Angle;
                StatusMessage = "模板匹配失败，参考位姿已设为当前ROI中心";
            }
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
                    {
                        _rawImage?.Dispose();
                        _rawImage = image.Clone();
                        MetrologyVM?.ClearResults();
                        ApplyEnhancement();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载图像失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnSaveRoi()
        {
            _currentRoi.SyncFromDrawingObject();
            _currentRoi.GenerateRegion();
            if (_currentRoi.Region == null || !_currentRoi.Region.IsInitialized())
            {
                MessageBox.Show("没有可保存的ROI区域", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Region Files|*.reg;*.hobj|All Files|*.*",
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
                Filter = "Region Files|*.reg;*.hobj|All Files|*.*",
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
            MetrologyVM?.ClearResults();
            _currentRoi.DetachDrawingObject();
            if (_currentRoi.Region != null && _currentRoi.Region.IsInitialized())
            {
                _currentRoi.Region.Dispose();
            }
            _currentRoi.Region?.Dispose();
            _currentRoi.Region = new HObject();
            _currentRoi.Region.GenEmptyObj();
            RoiPath = null;
            if (_halconWindow != null)
            {
                _halconWindow.ClearWindow();
                DisplayImagePreserveZoom();
            }
            StatusMessage = "ROI已清除，右键图像重新绘制";
            TemplateMatchingVM?.ClearModelFeatureDisplay();
        }

        private void LoadAvailableStations()
        {
            AvailableStations.Clear();
            _roleToSn.Clear();

            foreach (var setting in _cameraService.AllSettings)
            {
                if (string.IsNullOrEmpty(setting.SerilalNum)) continue;
                if (!_cameraService.ConnectedCameras.ContainsKey(setting.SerilalNum)) continue;

                var station = _stationConfigService.GetStationByCameraRole(setting.CameraRole);
                if (station == null) continue;

                _roleToSn[setting.CameraRole] = setting.SerilalNum;
                AvailableStations.Add(station);
            }

            SelectedStation = AvailableStations.FirstOrDefault();
            StatusMessage = AvailableStations.Count > 0
                ? $"检测到 {AvailableStations.Count} 个可用工位"
                : "未检测到已连接相机";
        }

        private async Task OnGrabFromCameraAsync()
        {
            var station = SelectedStation;
            if (station == null)
            {
                StatusMessage = "请先在工具栏选择工位";
                return;
            }

            if (!_roleToSn.TryGetValue(station.CameraRole, out string sn))
            {
                StatusMessage = $"工位 {station.StationId} 的相机 {station.CameraRole} 未连接";
                return;
            }

            //缓存里有: 直接取
            HObject frame = TryGetLatestFrame(sn);

            //缓存空:尝试软触发 + 等待
            if (frame == null)
            {
                if (!_cameraService.ExecuteSoftTrigger(sn))
                {
                    StatusMessage = $"相机 {sn} 无缓存帧，且软触发失败（可能为硬触发模式）";
                    return;
                }

                StatusMessage = "已触发，等待图像...";
                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(100);
                    frame = TryGetLatestFrame(sn);
                    if (frame != null) break;
                }
            }

            if (frame == null)
            {
                StatusMessage = "相机取图超时（2 秒内未收到图像）";
                return;
            }

            _rawImage?.Dispose();
            _rawImage = frame;             // frame 已经是新克隆的
            MetrologyVM?.ClearResults();
            ApplyEnhancement();
            ImagePath = $"CAM:{station.StationId}";
            CameraRoleName = station.CameraRole;
            StatusMessage = $"已从相机取图：{station.StationId} ({station.CameraRole})";
            RedrawImage();
        }


        /// <summary>
        /// 订阅 HImageDisplayEvent，缓存每台相机的最新一帧。
        /// 同步 Clone —— ProcessImagePointer 在 Publish 之后会 Dispose 原图。
        /// </summary>
        private void OnFrameReceived(CameraImagePayload payload)
        {
            if (payload?.Image == null || !payload.Image.IsInitialized()) return;
            if (string.IsNullOrEmpty(payload.CameraSN)) return;

            try
            {
                HObject copy = payload.Image.Clone();
                lock (_frameLock)
                {
                    if (_latestFrames.TryGetValue(payload.CameraSN, out var old))
                        old?.Dispose();
                    _latestFrames[payload.CameraSN] = copy;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnFrameReceived 克隆失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 取缓存帧（返回克隆，调用方负责释放
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        private HObject TryGetLatestFrame(string sn)
        {
            if (string.IsNullOrEmpty(sn)) return null;
            lock (_frameLock)
            {
                if (_latestFrames.TryGetValue(sn, out var cached) && cached.IsInitialized())
                    return cached.Clone();
            }
            return null;
        }
        #endregion

        #region ROI 形状绘制（从 TemplateMatchingVM 迁入）

        private void StartDraw(RoiType type, string color)
        {
            if (_halconWindow == null) return;
            if (TemplateMatchingVM != null && TemplateMatchingVM.IsMaskEditing)
            {
                StatusMessage = "请先退出掩膜编辑模式再绘制ROI";
                return;
            }
            //绘制新ROI前，先清掉上一次的测量结果
            //MetrologyVM?.ClearResults();
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
                case RoiType.LINE:
                    _currentRoi.LeftX = col + size / 2;
                    _currentRoi.LeftY = row + size / 2;
                    _currentRoi.RightX = col - size / 2;
                    _currentRoi.RightY = row - size / 2;
                    _currentRoi.X = col;
                    _currentRoi.Y = row;
                    break;
                case RoiType.ELLIPSE:
                    _currentRoi.X = col;
                    _currentRoi.Y = row;
                    _currentRoi.Angle = 0;
                    _currentRoi.Length1 = size;
                    _currentRoi.Length2 = size * 0.6;
                    break;
            }

            if (!_currentRoi.AttachDrawingObject(_halconWindow))
                StatusMessage = $"无法创建 {type} 绘图对象";
            else
                StatusMessage = $"绘制 {type}：拖动调整大小和位置，右键可重新选择形状";
        }

        private void StartPolygonDraw()
        {
            if (_halconWindow == null) return;
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
            RaisePropertyChanged(nameof(IsCustomMode));

            // 刷新窗口显示闭合后的多边形区域
            DisplayImagePreserveZoom();
            if (_halconWindow != null && _currentRoi.Region != null && _currentRoi.Region.IsInitialized())
            {
                _halconWindow.SetColor("green");
                _halconWindow.SetDraw("margin");
                _halconWindow.SetLineWidth(2);
                _halconWindow.DispObj(_currentRoi.Region);
            }
            StatusMessage = "多边形绘制完成，右键可重新选择形状";
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
            MetrologyVM?.ClearResults();
            _currentRoi.DetachDrawingObject();
            if (_halconWindow != null)
            {
                _halconWindow.ClearWindow();
                DisplayImagePreserveZoom();
            }
            _currentRoi.Region?.Dispose();
            _currentRoi.Region = new HObject();
            StatusMessage = "绘图已清除，右键图像重新绘制";
            TemplateMatchingVM?.ClearModelFeatureDisplay();
        }

        private void OnConfirmRoi()
        {
            if (_currentRoi.Style == RoiType.POLYGON)
            {
                StatusMessage = "多边形已确认，无需重复操作";
                return;
            }
            if (_currentRoi.Region == null || !_currentRoi.Region.IsInitialized())
            {
                StatusMessage = "没有可确认的 ROI，请先右键绘制";
                return;
            }

            _currentRoi.SyncFromDrawingObject();
            _currentRoi.GenerateRegion();
            _currentRoi.DetachDrawingObject();
            RedrawImage();
            StatusMessage = "ROI 已确认，绿色区域固定显示";
        }


        /// <summary>
        /// 从 _rawImage 按当前增强参数生成 CurrentImage，转发给所有子 VM
        /// </summary>
        private void ApplyEnhancement()
        {
            if (_rawImage == null || !_rawImage.IsInitialized())
                return;
            try
            {
                HObject processed;
                if (EnableEnhance)
                    HOperatorSet.ScaleImage(_rawImage, out processed, EnhanceMult, EnhanceAdd);
                else
                    processed = _rawImage.Clone();
                CurrentImage = processed;   // setter 会转发给 TemplateMatchingVM / MetrologyVM
                RedrawImage();
            }
            catch (Exception ex)
            {
                StatusMessage = $"图像增强失败: {ex.Message}";
            }
        }
        #endregion

        #region 图像显示辅助
        public void RedrawImage()
        {
            if (_halconWindow == null || CurrentImage == null || !CurrentImage.IsInitialized())
                return;
            try
            {
                _halconWindow.ClearWindow();
                _halconWindow.DispObj(CurrentImage);
                if (IsRoiVisible && _currentRoi.Region != null && _currentRoi.Region.IsInitialized())
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

        private void DisplayImagePreserveZoom()
        {
            if (_halconWindow == null) return;
            _halconWindow.ClearWindow();
            HObject img = _currentImage;
            if (img != null && img.IsInitialized())
                img.DispObj(_halconWindow);
        }
        #endregion

        //更新计量模块的参考位姿
        public void UpdateMetrologyRef(double refRow, double refCol, double refAngle)
        {
            if (MetrologyVM != null)
            {
                MetrologyVM.ModelRefRow = refRow;
                MetrologyVM.ModelRefCol = refCol;
                MetrologyVM.ModelRefAngle = refAngle;
            }
        }
    }
}
using AVS_Common.Events;
using AVS_Modules_Settings.Models;
using AVS_Service;
using AVS_Service.Models;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AVS_Modules_Settings.ViewModels
{
    public class ParameterConfigViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IParametersConfigService _configService;
        private readonly ILogger _logger;
        private bool _isInitialized = false;
        private string _lastModuleName = "未分类模块";

        // ===== 构造函数 =====
        public ParameterConfigViewModel(IParametersConfigService configService, IEventAggregator eventAggregator, ILogger logger)
        {
            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;

            // ---------- 基础参数命令 ----------
            AddCommand = new DelegateCommand(() =>
            {
                var newParam = new ParametersConfig
                {
                    Name = "New_Param",
                    ModuleName = SelectedSection ?? _lastModuleName
                };
                newParam.PropertyChanged += OnParameterPropertyChanged;
                Parameters.Add(newParam);
                RefreshSectionList();
                _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
            });

            DeleteCommand = new DelegateCommand<ParametersConfig>(p =>
            {
                Parameters.Remove(p);
                RefreshSectionList();
                _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
            });

            SaveCommand = new DelegateCommand(() =>
            {
                _configService.SaveConfig();
                if (SectionList.Count == 0)
                {
                    SelectedSection = null;
                }
                else if (!SectionList.Contains(SelectedSection))
                {
                    SelectedSection = SectionList[0];
                }
                IsBaseParamModify = false;
                _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
                _logger.Information("参数配置已保存");
            });

            DeleteSectionCommand = new DelegateCommand(() =>
            {
                if (string.IsNullOrEmpty(SelectedSection))
                {
                    MessageBox.Show("请先选择一个 Section。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"确定要删除 Section \"{SelectedSection}\" 及其所有参数吗？",
                    "删除确认",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;

                var paramsToDelete = Parameters.Where(p => p.ModuleName == SelectedSection).ToList();
                foreach (var p in paramsToDelete)
                    Parameters.Remove(p);

                _configService.SaveConfig();
                RefreshSectionList();
                _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
                _logger.Information("Section {Section} 已删除", SelectedSection);
            });

            // ---------- AI模型路径命令 ----------
            BrowseDetModelCommand = new DelegateCommand(() =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "模型文件 (*.onnx;*.mm;*.xml)|*.onnx;*.mm;*.xml|所有文件 (*.*)|*.*",
                    Title = "选择检测模型路径"
                };
                if (dialog.ShowDialog() == true)
                {
                    DetModelPath = dialog.FileName;
                    RaisePropertyChanged(nameof(DetModelPath));
                }
            });

            BrowseSegModelCommand = new DelegateCommand(() =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "模型文件 (*.onnx;*.mm;*.xml)|*.onnx;*.mm;*.xml|所有文件 (*.*)|*.*",
                    Title = "选择分割模型路径",
                    Multiselect = true
                };
                if (dialog.ShowDialog() == true)
                {
                    SegModelPaths = string.Join(";", dialog.FileNames);
                    RaisePropertyChanged(nameof(SegModelPaths));
                }
            });

            // ---------- 图像保存命令 ----------
            BrowseImageSaveDirCommand = new DelegateCommand(() =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName = "选择文件夹",
                    Title = "选择图像保存文件夹"
                };
                if (dialog.ShowDialog() == true)
                {
                    ImageSaveDir = System.IO.Path.GetDirectoryName(dialog.FileName);
                }
            });

            // ---------- 检测顺序命令 ----------
            GenerateGridCommand = new DelegateCommand(() => GenerateEmptyGrid());

            ClearRowCommand = new DelegateCommand(() =>
            {
                if (SelectedPole == null) return;
                foreach (var p in PoleItems.Where(p => p.Row == SelectedPole.Row))
                {
                    p.PoleNumber = null;
                    p.IsStartPoint = false;
                    p.IsEndPoint = false;
                }
            });

            SaveToConfigCommand = new DelegateCommand(() =>
            {
                SaveToConfig();
                _logger.Information("检测顺序已保存到配方 {Index}", RecipeIndex);
            });

            // ---------- 产品参数命令 ----------
            AddProductCommand = new DelegateCommand(() =>
            {
                ProductParameters.Add(new ProductParameter
                {
                    Category = "WeldBeadParam",
                    Name = "NewParam",
                    Description = "",
                    Value = "0"
                });
            });

            DeleteProductCommand = new DelegateCommand<ProductParameter>(p =>
            {
                if (p != null)
                    ProductParameters.Remove(p);
            });

            SaveProductCommand = new DelegateCommand(() => SaveProductParameters());

            // 订阅基础参数集合变化
            Parameters.CollectionChanged += (s, e) =>
            {
                IsBaseParamModify = true;
                if (e.NewItems != null)
                    foreach (ParametersConfig p in e.NewItems)
                        p.PropertyChanged += OnParameterPropertyChanged;
                if (e.OldItems != null)
                    foreach (ParametersConfig p in e.OldItems)
                        p.PropertyChanged -= OnParameterPropertyChanged;
            };

            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            RecipeList = new ObservableCollection<int>(Enumerable.Range(1, 20));
            ParametersView = new ListCollectionView(Parameters);
            ParametersView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ParametersConfig.ModuleName)));
            ParametersView.SortDescriptions.Add(new SortDescription(nameof(ParametersConfig.ModuleName), ListSortDirection.Ascending));
            ParametersView.SortDescriptions.Add(new SortDescription(nameof(ParametersConfig.Name), ListSortDirection.Ascending));

            ProductParameters = new ObservableCollection<ProductParameter>();
            ProductParametersView = new ListCollectionView(ProductParameters);

            RefreshSectionList();
            if (SectionList.Count > 0)
                SelectedSection = SectionList[0];

            LoadPoleGrid();
            LoadProductParameters();
        }

        #region 图像保存
        // ===== 通用 =====
        public string ImageSaveDir
        {
            get => _configService.GetString("Global", "ImageSaveDir");
            set
            {
                _configService.UpdateParam("Global", "ImageSaveDir", value);
                RaisePropertyChanged(nameof(ImageSaveDir));
            }
        }

        public int ImageCompressRatio
        {
            get => _configService.GetInt("Global", "ImageCompressRatio", 100);
            set { _configService.UpdateParam("Global", "ImageCompressRatio", value.ToString()); RaisePropertyChanged(); }
        }

        public int SaveOrnImgDays
        {
            get => _configService.GetInt("Global", "SaveOrnImgDays", 30);
            set { _configService.UpdateParam("Global", "SaveOrnImgDays", value.ToString()); RaisePropertyChanged(); }
        }

        public int SaveRenImgDays
        {
            get => _configService.GetInt("Global", "SaveRenImgDays", 30);
            set { _configService.UpdateParam("Global", "SaveRenImgDays", value.ToString()); RaisePropertyChanged(); }
        }

        public DelegateCommand BrowseImageSaveDirCommand { get; }

        // ===== 2D 相机 =====
        // 保存原始图像（与 IsSave2DNGOriginal 互斥）
        public bool IsSave2DOriginal
        {
            get => _configService.GetBool("Global", "IsSave2DOriginal");
            set
            {
                _configService.UpdateParam("Global", "IsSave2DOriginal", value.ToString(), ParamOutputType.BOOL);
                RaisePropertyChanged();
            }
        }

        public bool IsSave2DNGOnly
        {
            get => _configService.GetBool("Global", "IsSave2DNGOnly");
            set
            {
                _configService.UpdateParam("Global", "IsSave2DNGOnly", value.ToString(), ParamOutputType.BOOL);
                RaisePropertyChanged();
            }
        }

        public string Format2DOriginal
        {
            get => _configService.GetString("Global", "Format2DOriginal", "bmp");
            set { _configService.UpdateParam("Global", "Format2DOriginal", value); RaisePropertyChanged(); }
        }

        public bool IsSave2DResult
        {
            get => _configService.GetBool("Global", "IsSave2DResult");
            set { _configService.UpdateParam("Global", "IsSave2DResult", value.ToString(), ParamOutputType.BOOL); RaisePropertyChanged(); }
        }

        public string Format2DResult
        {
            get => _configService.GetString("Global", "Format2DResult", "bmp");
            set { _configService.UpdateParam("Global", "Format2DResult", value); RaisePropertyChanged(); }
        }

        public bool IsSave2DMask
        {
            get => _configService.GetBool("Global", "IsSave2DMask");
            set { _configService.UpdateParam("Global", "IsSave2DMask", value.ToString(), ParamOutputType.BOOL); RaisePropertyChanged(); }
        }

        // ===== 3D 相机 =====
        // 仅保存NG样本的开关（过滤器）
        public bool IsSave3DNGOnly
        {
            get => _configService.GetBool("Global", "IsSave3DNGOnly");
            set
            {
                _configService.UpdateParam("Global", "IsSave3DNGOnly", value.ToString(), ParamOutputType.BOOL);
                RaisePropertyChanged();
            }
        }

        public bool IsSave3DDepth
        {
            get => _configService.GetBool("Global", "IsSave3DDepth");
            set
            {
                _configService.UpdateParam("Global", "IsSave3DDepth", value.ToString(), ParamOutputType.BOOL); RaisePropertyChanged();
            }
        }

        public string Format3DDepth
        {
            get => _configService.GetString("Global", "Format3DDepth", "tiff");
            set { _configService.UpdateParam("Global", "Format3DDepth", value); RaisePropertyChanged(); }
        }

        public bool IsSave3DIntensity
        {
            get => _configService.GetBool("Global", "IsSave3DIntensity");
            set
            {
                _configService.UpdateParam("Global", "IsSave3DIntensity", value.ToString(), ParamOutputType.BOOL); RaisePropertyChanged();
            }
        }

        public string Format3DIntensity
        {
            get => _configService.GetString("Global", "Format3DIntensity", "bmp");
            set { _configService.UpdateParam("Global", "Format3DIntensity", value); RaisePropertyChanged(); }
        }

        public bool IsSave3DResult
        {
            get => _configService.GetBool("Global", "IsSave3DResult");
            set { _configService.UpdateParam("Global", "IsSave3DResult", value.ToString(), ParamOutputType.BOOL); RaisePropertyChanged(); }
        }

        public string Format3DResult
        {
            get => _configService.GetString("Global", "Format3DResult", "bmp");
            set { _configService.UpdateParam("Global", "Format3DResult", value); RaisePropertyChanged(); }
        }

        public bool IsSave3DMask
        {
            get => _configService.GetBool("Global", "IsSave3DMask");
            set { _configService.UpdateParam("Global", "IsSave3DMask", value.ToString(), ParamOutputType.BOOL); RaisePropertyChanged(); }
        }

        #endregion

        #region 基础参数
        public ObservableCollection<ParametersConfig> Parameters => _configService.ConfigParams;

        private ICollectionView _parametersView;
        public ICollectionView ParametersView
        {
            get => _parametersView;
            set => SetProperty(ref _parametersView, value);
        }

        private ParametersConfig _selectedParameter;
        public ParametersConfig SelectedParameter
        {
            get => _selectedParameter;
            set
            {
                if (SetProperty(ref _selectedParameter, value) && value != null)
                    _lastModuleName = value.ModuleName;
            }
        }

        public IEnumerable<ParamOutputType> DataTypeValues =>
            Enum.GetValues(typeof(ParamOutputType)).Cast<ParamOutputType>();

        public DelegateCommand AddCommand { get; }
        public DelegateCommand<ParametersConfig> DeleteCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand DeleteSectionCommand { get; }

        private ObservableCollection<string> _sectionList;
        public ObservableCollection<string> SectionList
        {
            get => _sectionList;
            set => SetProperty(ref _sectionList, value);
        }

        private string _selectedSection;
        public string SelectedSection
        {
            get => _selectedSection;
            set
            {
                if (SetProperty(ref _selectedSection, value))
                {
                    _lastModuleName = value ?? _lastModuleName;
                    RefreshFilter();

                    RaisePropertyChanged(nameof(DetModelPath));
                    RaisePropertyChanged(nameof(SegModelPaths));

                    LoadProductParameters(); // 工位切换，重新加载产品参数
                }
            }
        }

        private bool _isBaseParamModify;
        public bool IsBaseParamModify
        {
            get => _isBaseParamModify;
            private set => SetProperty(ref _isBaseParamModify, value);
        }

        private void OnParameterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsBaseParamModify = true;
            if (e.PropertyName == nameof(ParametersConfig.ModuleName) && sender is ParametersConfig)
            {
                RefreshSectionList();
                _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
            }
        }

        private void RefreshSectionList()
        {
            var sections = Parameters
                .Select(p => p.ModuleName)
                .Where(s => !string.IsNullOrEmpty(s) && s != "Recipe")
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            SectionList = new ObservableCollection<string>(sections);
            ValidateSelectedSection();
        }

        private void ValidateSelectedSection()
        {
            if (SectionList.Count == 0)
            {
                SelectedSection = null;
            }
            else if (SelectedSection == null || !SectionList.Contains(SelectedSection))
            {
                SelectedSection = SectionList[0];
            }
        }

        private void RefreshFilter()
        {
            if (ParametersView == null) return;
            if (string.IsNullOrEmpty(SelectedSection))
                ParametersView.Filter = null;
            else
                ParametersView.Filter = obj => obj is ParametersConfig p && p.ModuleName == SelectedSection;
            ParametersView.Refresh();
        }
        #endregion

        #region AI检测模型
        public string DetModelPath
        {
            get => _configService.GetString(SelectedSection ?? "Global", "DetModelPath");
            set => _configService.UpdateParam(SelectedSection ?? "Global", "DetModelPath", value);
        }

        public string SegModelPaths
        {
            get => _configService.GetString(SelectedSection ?? "Global", "SegModelPaths");
            set => _configService.UpdateParam(SelectedSection ?? "Global", "SegModelPaths", value);
        }

        public DelegateCommand BrowseDetModelCommand { get; }
        public DelegateCommand BrowseSegModelCommand { get; }
        #endregion

        #region 检测顺序
        private ObservableCollection<PoleCircleItem> _poleItems;
        public ObservableCollection<PoleCircleItem> PoleItems
        {
            get => _poleItems;
            set => SetProperty(ref _poleItems, value);
        }

        private int _recipeIndex;
        public int RecipeIndex
        {
            get => _recipeIndex;
            set
            {
                if (SetProperty(ref _recipeIndex, value))
                    LoadPoleGrid();
            }
        }

        private int _gridRows = 4;
        public int GridRows { get => _gridRows; set => SetProperty(ref _gridRows, value); }

        private int _gridCols = 13;
        public int GridCols { get => _gridCols; set => SetProperty(ref _gridCols, value); }

        private ObservableCollection<int> _recipeList;
        public ObservableCollection<int> RecipeList
        {
            get => _recipeList;
            set => SetProperty(ref _recipeList, value);
        }

        private PoleCircleItem _selectedPole;
        public PoleCircleItem SelectedPole
        {
            get => _selectedPole;
            set
            {
                if (_selectedPole != null && _selectedPole != value)
                    _selectedPole.IsSelected = false;
                SetProperty(ref _selectedPole, value);
                if (_selectedPole != null)
                    _selectedPole.IsSelected = true;
            }
        }

        public DelegateCommand GenerateGridCommand { get; }
        public DelegateCommand ClearRowCommand { get; }
        public DelegateCommand SaveToConfigCommand { get; }
        public DelegateCommand<PoleCircleItem> SelectPoleCommand { get; }

        public void BeginEditPole(PoleCircleItem item)
        {
            if (item == null) return;

            var editingItem = PoleItems?.FirstOrDefault(p => p.IsEditing);
            if (editingItem != null && editingItem != item)
            {
                editingItem.IsEditing = false;
            }

            SelectedPole = item;
            item.IsEditing = true;
        }

        public void CommitEditPole(PoleCircleItem item, string newValue)
        {
            if (item == null) return;

            item.IsEditing = false;

            if (int.TryParse(newValue, out int num))
            {
                item.PoleNumber = num;

                var rowItems = PoleItems.Where(p => p.Row == item.Row)
                                        .OrderBy(p => p.Col)
                                        .ToList();
                if (rowItems.Count > 0)
                {
                    foreach (var p in rowItems)
                    {
                        p.IsStartPoint = false;
                        p.IsEndPoint = false;
                    }

                    var first = rowItems.FirstOrDefault(p => p.PoleNumber.HasValue);
                    var last = rowItems.LastOrDefault(p => p.PoleNumber.HasValue);
                    if (first != null) first.IsStartPoint = true;
                    if (last != null && last != first) last.IsEndPoint = true;
                }
                TryAutoFill(item.Row);
            }
            else
            {
                MessageBox.Show("请输入有效的整数序号。", "输入无效", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CancelEditPole(PoleCircleItem item)
        {
            if (item == null) return;
            item.IsEditing = false;
        }

        private void GenerateEmptyGrid()
        {
            var items = new ObservableCollection<PoleCircleItem>();
            for (int r = 0; r < GridRows; r++)
                for (int c = 0; c < GridCols; c++)
                    items.Add(new PoleCircleItem { Row = r, Col = c });
            PoleItems = items;
        }

        private void LoadPoleGrid()
        {
            if (GridRows <= 0 || GridCols <= 0) return;

            var orders = LoadInspectOrdersFromConfig();
            if (orders == null || RecipeIndex >= orders.Length) return;

            var order = orders[RecipeIndex];
            if (order.Row <= 0 || order.Col <= 0) return;
            if (order.Start == null || order.End == null) return;
            if (order.Start.Length < order.Row || order.End.Length < order.Row) return;

            GridRows = order.Row;
            GridCols = order.Col;

            GenerateEmptyGrid();
            for (int r = 0; r < order.Row; r++)
            {
                int totalSteps = order.Col - 1;
                if (totalSteps <= 0) continue;

                int mdiff = Math.Abs(order.End[r] - order.Start[r]) / totalSteps;
                if (order.End[r] - order.Start[r] < 0) mdiff = -mdiff;

                for (int c = 0; c < order.Col; c++)
                {
                    int index = r * order.Col + c;
                    if (index >= PoleItems.Count) break;

                    var item = PoleItems[index];
                    item.PoleNumber = order.Start[r] + mdiff * c;
                    if (c == 0) item.IsStartPoint = true;
                    if (c == order.Col - 1) item.IsEndPoint = true;
                }
            }
        }

        private void TryAutoFill(int row)
        {
            if (PoleItems == null) return;

            var rowItems = PoleItems.Where(p => p.Row == row).OrderBy(p => p.Col).ToList();
            if (rowItems.Count < 2) return;

            var knownItems = rowItems.Where(p => p.PoleNumber.HasValue).ToList();
            if (knownItems.Count < 2) return;

            var first = knownItems[0];
            var second = knownItems[1];
            if (first.Col == second.Col) return;

            double step = (double)(second.PoleNumber.Value - first.PoleNumber.Value) / (second.Col - first.Col);
            if (step != Math.Round(step))
            {
                MessageBox.Show(
                    $"当前两个极柱无法形成整数编号规律。\n\n" +
                    $"位置 {first.Col + 1}：{first.PoleNumber}\n" +
                    $"位置 {second.Col + 1}：{second.PoleNumber}\n\n" +
                    $"计算得到步长：{step:F3}",
                    "无法自动填充",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int intStep = (int)step;
            foreach (var item in knownItems)
            {
                int offset = item.Col - first.Col;
                int expectedNumber = first.PoleNumber.Value + intStep * offset;
                if (item.PoleNumber.Value != expectedNumber)
                {
                    MessageBox.Show(
                        $"当前行已有编号不符合计算规律。\n\n" +
                        $"第 {item.Col + 1} 个位置\n" +
                        $"当前编号：{item.PoleNumber.Value}\n" +
                        $"计算应为：{expectedNumber}\n\n" +
                        $"请检查已经输入的极柱号。",
                        "无法自动填充",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            foreach (var item in rowItems)
            {
                int offset = item.Col - first.Col;
                item.PoleNumber = first.PoleNumber.Value + intStep * offset;
            }
        }

        private void SaveToConfig()
        {
            if (PoleItems == null || PoleItems.Count == 0) return;
            if (GridRows <= 0 || GridCols <= 0) return;

            InspectOrder[] orders = LoadInspectOrdersFromConfig();
            if (orders == null) orders = new InspectOrder[Math.Max(RecipeIndex + 1, 20)];
            if (RecipeIndex >= orders.Length)
            {
                var newOrders = new InspectOrder[RecipeIndex + 1];
                Array.Copy(orders, newOrders, orders.Length);
                orders = newOrders;
            }

            var order = new InspectOrder
            {
                Row = GridRows,
                Col = GridCols,
                Start = new int[GridRows],
                End = new int[GridRows]
            };

            for (int r = 0; r < GridRows; r++)
            {
                var rowItems = PoleItems.Where(p => p.Row == r)
                                        .OrderBy(p => p.Col)
                                        .Where(p => p.PoleNumber.HasValue)
                                        .ToList();
                if (rowItems.Count > 0)
                {
                    order.Start[r] = rowItems.First().PoleNumber.Value;
                    order.End[r] = rowItems.Last().PoleNumber.Value;
                }
                else
                {
                    order.Start[r] = 0;
                    order.End[r] = 0;
                }
            }

            orders[RecipeIndex] = order;

            string json = JsonConvert.SerializeObject(orders, Formatting.Indented);
            _configService.UpdateParam("Recipe", "InspectOrders", json);
            _configService.SaveConfig();

            _logger.Information("配方 {Index} 的检测顺序已保存到 'Recipe' Section", RecipeIndex);
        }

        private InspectOrder[] LoadInspectOrdersFromConfig()
        {
            string json = _configService.GetString("Recipe", "InspectOrders", "");
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonConvert.DeserializeObject<InspectOrder[]>(json);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "反序列化 InspectOrders 失败");
                return null;
            }
        }
        #endregion

        #region 产品参数
        public ObservableCollection<ProductParameter> ProductParameters { get; private set; }
        public ICollectionView ProductParametersView { get; private set; }

        private ProductParameter _selectedProductParameter;
        public ProductParameter SelectedProductParameter
        {
            get => _selectedProductParameter;
            set => SetProperty(ref _selectedProductParameter, value);
        }

        public DelegateCommand AddProductCommand { get; private set; }
        public DelegateCommand<ProductParameter> DeleteProductCommand { get; private set; }
        public DelegateCommand SaveProductCommand { get; private set; }

        private string _productParamFilePath;

        private void SaveProductParameters()
        {
            try
            {
                string json = JsonConvert.SerializeObject(ProductParameters, Formatting.Indented);
                File.WriteAllText(_productParamFilePath, json);
                _logger.Information("产品参数已保存到 {Path}", _productParamFilePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "保存产品参数失败");
                MessageBox.Show("保存产品参数失败：" + ex.Message);
            }
        }

        private void LoadProductParameters()
        {
            ProductParameters.Clear();
            string section = SelectedSection;
            if (string.IsNullOrEmpty(section))
                return;

            bool isSquareBar = _configService.GetBool(section, "IsSquareBarWeldMark", false);
            string productType = isSquareBar ? "SB" : "Circ";
            string fileName = $"{productType}ProductParam{section}.json";
            _productParamFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", fileName);

            if (!File.Exists(_productParamFilePath))
            {
                _logger.Warning("产品参数文件不存在: {Path}", _productParamFilePath);
                return;
            }

            try
            {
                string json = File.ReadAllText(_productParamFilePath);
                var list = JsonConvert.DeserializeObject<ObservableCollection<ProductParameter>>(json);
                if (list != null)
                    foreach (var item in list)
                        ProductParameters.Add(item);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "加载产品参数失败");
                MessageBox.Show("产品参数加载失败：" + ex.Message);
            }
        }

        public void OnProductParamTabActivated()
        {
            if (IsBaseParamModify)
            {
                var result = MessageBox.Show(
                    "基础参数有未保存的修改，是否保存并刷新产品参数？",
                    "确认",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _configService.SaveConfig();
                    IsBaseParamModify = false;
                    LoadProductParameters();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    // 可选：取消切换事件
                }
            }
            else
            {
                LoadProductParameters();
            }
        }
        #endregion
    }
}
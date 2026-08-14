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

        // ===== 数据源 =====
        public ObservableCollection<ParametersConfig> Parameters => _configService.ConfigParams;

        // ===== 基础参数 DataGrid =====
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

        // ===== 命令 =====
        public DelegateCommand AddCommand { get; }
        public DelegateCommand<ParametersConfig> DeleteCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand DeleteSectionCommand { get; }
        public DelegateCommand BrowseDetModelCommand { get; }
        public DelegateCommand BrowseSegModelCommand { get; }
        public DelegateCommand BrowseImageSaveDirCommand { get; }
        // ===== Section 切换 =====
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
                }
            }
        }

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

        // ===== 公共参数：图像保存（Global） =====
        public bool IsSaveOrnImg
        {
            get => _configService.GetBool("Global", "IsSaveOrnImg");
            set => _configService.UpdateParam("Global", "IsSaveOrnImg", value.ToString());
        }

        public bool IsSaveOkRenImg
        {
            get => _configService.GetBool("Global", "IsSaveOkRenImg");
            set => _configService.UpdateParam("Global", "IsSaveOkRenImg", value.ToString());
        }

        public bool IsSaveNgRenImg
        {
            get => _configService.GetBool("Global", "IsSaveNgRenImg");
            set => _configService.UpdateParam("Global", "IsSaveNgRenImg", value.ToString());
        }

        public int SaveOrnImgDays
        {
            get => _configService.GetInt("Global", "SaveOrnImgDays", 30);
            set => _configService.UpdateParam("Global", "SaveOrnImgDays", value.ToString());
        }

        public int SaveRenImgDays
        {
            get => _configService.GetInt("Global", "SaveRenImgDays", 30);
            set => _configService.UpdateParam("Global", "SaveRenImgDays", value.ToString());
        }

        public string ImageSaveDir
        {
            get => _configService.GetString("Global", "ImageSaveDir");
            set
            {
                _configService.UpdateParam("Global", "ImageSaveDir", value);
                RaisePropertyChanged(nameof(ImageSaveDir));
            }
        }

        public string ImageFormat
        {
            get => _configService.GetString("Global", "ImageFormat", "bmp");
            set => _configService.UpdateParam("Global", "ImageFormat", value);
        }

        public int ImageCompressRatio
        {
            get => _configService.GetInt("Global", "ImageCompressRatio", 100);
            set => _configService.UpdateParam("Global", "ImageCompressRatio", value.ToString());
        }

        // ===== 构造函数 =====
        public ParameterConfigViewModel(IParametersConfigService configService, IEventAggregator eventAggregator, ILogger logger)
        {

            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _eventAggregator.GetEvent<SectionsChangedEvent>().Subscribe(() =>
            {
                RefreshSectionList();
            });
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
                // 如果删空了当前 Section，重置选中项
                if (SectionList.Count == 0)
                {
                    SelectedSection = null;
                }
                else if (!SectionList.Contains(SelectedSection))
                {
                    SelectedSection = SectionList[0];
                }
                _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
                _logger.Information("参数配置已保存");
            });
            DeleteSectionCommand = new DelegateCommand(() =>
            {
                if (string.IsNullOrEmpty(SelectedSection))
                {
                    MessageBox.Show("请先选择一个 Section。", "提示",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 确认删除
                var confirm = MessageBox.Show(
                    $"确定要删除 Section \"{SelectedSection}\" 及其所有参数吗？",
                    "删除确认",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;

                // 删除该 ModuleName 下的所有参数
                var paramsToDelete = Parameters
                    .Where(p => p.ModuleName == SelectedSection)
                    .ToList();

                foreach (var p in paramsToDelete)
                {
                    Parameters.Remove(p);
                }

                // 持久化
                _configService.SaveConfig();

                // 刷新 Section 列表（内部会调用 ValidateSelectedSection，自动选择新的 Section）
                RefreshSectionList();

                // 通知其他模块（如 StationConfigViewModel）刷新
                _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();

                _logger.Information("Section {Section} 已删除", SelectedSection);
            });
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
            // 生成空网格
            GenerateGridCommand = new DelegateCommand(() =>
            {
                GenerateEmptyGrid();
            });
            // 清除当前行
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
            // 保存到 InspectOrder
            SaveToConfigCommand = new DelegateCommand(() =>
            {
                SaveToConfig();
                _logger.Information("检测顺序已保存到配方 {Index}", RecipeIndex);
            });
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            RecipeList = new ObservableCollection<int>(Enumerable.Range(1, 20));
            ParametersView = new ListCollectionView(Parameters);
            ParametersView.GroupDescriptions.Add(
                new PropertyGroupDescription(nameof(ParametersConfig.ModuleName)));
            ParametersView.SortDescriptions.Add(
                new SortDescription(nameof(ParametersConfig.ModuleName), ListSortDirection.Ascending));
            ParametersView.SortDescriptions.Add(
                new SortDescription(nameof(ParametersConfig.Name), ListSortDirection.Ascending));
            RefreshSectionList();
            if (SectionList.Count > 0)
                SelectedSection = SectionList[0];
            LoadPoleGrid();
        }

        private void OnParameterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ParametersConfig.ModuleName) && sender is ParametersConfig)
            {
                RefreshSectionList();
                _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
            }
        }

        // ===== 辅助方法 =====

        private void RefreshSectionList()
        {
            var sections = Parameters
                .Select(p => p.ModuleName)
                .Where(s => !string.IsNullOrEmpty(s) && s != "Recipe" /*&& s != "Global"*/ )
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
            {
                ParametersView.Filter = null;
            }
            else
            {
                ParametersView.Filter = obj =>
                {
                    if (obj is ParametersConfig p)
                        return p.ModuleName == SelectedSection;
                    return false;
                };
            }
            ParametersView.Refresh();
        }


        // ===== 检测顺序 Tab 相关属性 =====
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

        public void BeginEditPole(PoleCircleItem item)
        {
            if (item == null) return;

            // 关闭之前正在编辑的项
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

                // 自动标记该行的起点和终点
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
                // 如果该行已有至少两个有效极柱号，自动填充
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

        // 命令
        public DelegateCommand GenerateGridCommand { get; }
        public DelegateCommand ClearRowCommand { get; }
        public DelegateCommand SaveToConfigCommand { get; }
        public DelegateCommand<PoleCircleItem> SelectPoleCommand { get; }

        // 生成空网格
        private void GenerateEmptyGrid()
        {
            var items = new ObservableCollection<PoleCircleItem>();
            for (int r = 0; r < GridRows; r++)
                for (int c = 0; c < GridCols; c++)
                    items.Add(new PoleCircleItem { Row = r, Col = c });
            PoleItems = items;
        }

        // 从配置加载已有数据
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
            if (PoleItems == null)
                return;

            var rowItems = PoleItems.Where(p => p.Row == row).OrderBy(p => p.Col).ToList();
            if (rowItems.Count < 2)
                return;
            // 找出这一行已经填写极柱号的位置
            var knownItems = rowItems.Where(p => p.PoleNumber.HasValue).ToList();
            // 至少需要两个已知点
            if (knownItems.Count < 2) return;
            var first = knownItems[0];
            var second = knownItems[1];
            if (first.Col == second.Col) return;
            double step = (double)(second.PoleNumber.Value - first.PoleNumber.Value) / (second.Col - first.Col);
            // 极柱编号必须是整数，因此步长也必须是整数
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

            // 从配置读取现有的 InspectOrders 数组（如果有）
            InspectOrder[] orders = LoadInspectOrdersFromConfig();

            // 确保数组足够大
            if (orders == null) orders = new InspectOrder[Math.Max(RecipeIndex + 1, 20)];
            if (RecipeIndex >= orders.Length)
            {
                var newOrders = new InspectOrder[RecipeIndex + 1];
                Array.Copy(orders, newOrders, orders.Length);
                orders = newOrders;
            }

            // 构建当前配方的 InspectOrder
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

            // 序列化并保存到 ConfigParams 中
            string json = JsonConvert.SerializeObject(orders, Formatting.Indented);
            _configService.UpdateParam("Recipe", "InspectOrders", json);
            _configService.SaveConfig();

            _logger.Information("配方 {Index} 的检测顺序已保存到 'Recipe' Section", RecipeIndex);
        }

        // 辅助方法：从配置加载 InspectOrders 数组
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
    }
}
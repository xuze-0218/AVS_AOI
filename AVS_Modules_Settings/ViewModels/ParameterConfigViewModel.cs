using AVS_Common.Events;
using AVS_Modules_Settings.Models;
using AVS_Service;
using AVS_Service.Models;
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
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

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


        // ===== 公共参数：AI 模型路径（Global） =====
        public string DetModelPath
        {
            get => _configService.GetString("Global", "DetModelPath");
            set => _configService.UpdateParam("Global", "DetModelPath", value);
        }

        public string SegModelPaths
        {
            get => _configService.GetString("Global", "SegModelPaths");
            set => _configService.UpdateParam("Global", "SegModelPaths", value);
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


            BrowseDetModelCommand = new DelegateCommand(() =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "模型文件 (*.onnx;*.xml)|*.onnx;*.xml|所有文件 (*.*)|*.*",
                    Title = "选择检测模型路径"
                };
                if (dialog.ShowDialog() == true)
                {
                    DetModelPath = dialog.FileName;
                }
            });

            BrowseSegModelCommand = new DelegateCommand(() =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "模型文件 (*.onnx;*.xml)|*.onnx;*.xml|所有文件 (*.*)|*.*",
                    Title = "选择分割模型路径",
                    Multiselect = true
                };
                if (dialog.ShowDialog() == true)
                {
                    SegModelPaths = string.Join(";", dialog.FileNames);
                }
            });
            BrowseImageSaveDirCommand  = new DelegateCommand(() =>
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

            // 从配置加载已有数据（保留作为"加载配方"按钮）
            // 如果需要切换配方，可以用 LoadPoleGrid 替代

            // 标记起点
            MarkAsStartCommand = new DelegateCommand(() =>
            {
                if (SelectedPole == null) return;
                foreach (var p in PoleItems.Where(p => p.Row == SelectedPole.Row))
                    p.IsStartPoint = false;
                SelectedPole.IsStartPoint = true;
                TryAutoFill(SelectedPole.Row);
            });

            // 标记终点
            MarkAsEndCommand = new DelegateCommand(() =>
            {
                if (SelectedPole == null) return;
                foreach (var p in PoleItems.Where(p => p.Row == SelectedPole.Row))
                    p.IsEndPoint = false;
                SelectedPole.IsEndPoint = true;
                TryAutoFill(SelectedPole.Row);
            });

            // 自动填充当前行
            AutoFillRowCommand = new DelegateCommand(() =>
            {
                if (SelectedPole != null)
                    TryAutoFill(SelectedPole.Row);
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
                .Where(s => s != "Global" && !string.IsNullOrEmpty(s))
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
        public int RecipeIndex { get => _recipeIndex; set { SetProperty(ref _recipeIndex, value); LoadPoleGrid(); } }

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
        public PoleCircleItem SelectedPole { get => _selectedPole; set => SetProperty(ref _selectedPole, value); }

        // 命令
        public DelegateCommand GenerateGridCommand { get; }
        public DelegateCommand MarkAsStartCommand { get; }
        public DelegateCommand MarkAsEndCommand { get; }
        public DelegateCommand AutoFillRowCommand { get; }
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
            GenerateEmptyGrid();
            var p = _configService.GetStationParams(SelectedSection ?? "SideA");
            var orders = p.InspectOrders;
            if (orders == null || RecipeIndex >= orders.Length) return;
            var order = orders[RecipeIndex];
            GridRows = order.Row;
            GridCols = order.Col;

            // 根据 start/end 计算每行的极柱号并填充
            for (int r = 0; r < order.Row; r++)
            {
                int mdiff = Math.Abs(order.End[r] - order.Start[r]) / (order.Col - 1);
                if (order.End[r] - order.Start[r] < 0) mdiff = -mdiff;
                for (int c = 0; c < order.Col; c++)
                {
                    var item = PoleItems[r * order.Col + c];
                    item.PoleNumber = order.Start[r] + mdiff * c;
                    if (c == 0) item.IsStartPoint = true;
                    if (c == order.Col - 1) item.IsEndPoint = true;
                }
            }
        }


        private void TryAutoFill(int row)
        {
            if (PoleItems == null) return;

            // 获取该行所有项，按列排序
            var rowItems = PoleItems.Where(p => p.Row == row).OrderBy(p => p.Col).ToList();
            if (rowItems.Count == 0) return;

            // 找到标记为起点和终点的项
            var startItem = rowItems.FirstOrDefault(p => p.IsStartPoint);
            var endItem = rowItems.FirstOrDefault(p => p.IsEndPoint);

            // 两者都必须有有效的极柱号
            if (startItem == null || endItem == null ||
                !startItem.PoleNumber.HasValue || !endItem.PoleNumber.HasValue)
                return;

            int startNum = startItem.PoleNumber.Value;
            int endNum = endItem.PoleNumber.Value;
            int count = rowItems.Count;

            // 计算均匀步长（与 HandleInspectInit 完全一致）
            int mdiff = Math.Abs(endNum - startNum) / (count - 1);
            if (endNum < startNum) mdiff = -mdiff;

            for (int i = 0; i < count; i++)
            {
                rowItems[i].PoleNumber = startNum + mdiff * i;
                // 可选：清除其他标记，只保留起点/终点的特殊颜色
            }
        }


        private void SaveToConfig()
        {
            if (PoleItems == null || PoleItems.Count == 0) return;
            if (string.IsNullOrEmpty(SelectedSection))
            {
                _logger.Warning("未选择 Section，无法保存检测顺序");
                return;
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
                // 获取该行所有有效极柱号（按列排序）
                var rowItems = PoleItems.Where(p => p.Row == r)
                                        .OrderBy(p => p.Col)
                                        .Where(p => p.PoleNumber.HasValue)
                                        .ToList();

                if (rowItems.Count > 0)
                {
                    // 起点 = 该行第一个极柱号，终点 = 该行最后一个极柱号
                    order.Start[r] = rowItems.First().PoleNumber.Value;
                    order.End[r] = rowItems.Last().PoleNumber.Value;
                }
                else
                {
                    // 如果该行完全没填，保留旧值（或设为0）
                    order.Start[r] = 0;
                    order.End[r] = 0;
                }
            }

            // 保存到当前 Section 的 InspectOrders 中
            var p = _configService.GetStationParams(SelectedSection);
            if (p.InspectOrders != null && RecipeIndex < p.InspectOrders.Length)
            {
                p.InspectOrders[RecipeIndex] = order;
            }
            // 如果 InspectOrders 数组长度不够，可以扩展（但旧版固定20个，通常够用）

            _configService.SaveConfig();
            _logger.Information("配方 {Index} 的检测顺序已保存到 Section {Section}", RecipeIndex, SelectedSection);
        }

    }
}
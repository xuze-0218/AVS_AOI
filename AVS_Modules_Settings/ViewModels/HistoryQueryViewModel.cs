using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Modules_Settings.ViewModels
{
    using AVS_Service.Models;
    using AVS_Service.Services;
    using Prism.Commands;
    using Prism.Mvvm;
    using Prism.Services.Dialogs;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    public class HistoryQueryViewModel : BindableBase
    {
        private readonly IHistoryDataService _dataService;
        private readonly IDialogService _dialogService;

        public HistoryQueryViewModel(IHistoryDataService dataService, IDialogService dialogService)
        {
            _dataService = dataService;
            _dialogService = dialogService;

            AnalyzeCommand = new DelegateCommand(async () => await AnalyzeAsync(), CanAnalyze).ObservesProperty(() => IsLoading);
            BrowseFolderCommand = new DelegateCommand(BrowseFolder);
        }

        // ============ 命令 ============
        public DelegateCommand AnalyzeCommand { get; }
        public DelegateCommand BrowseFolderCommand { get; }

        // ============ 基本属性 ============
        private DateTime _startDate = DateTime.Today.AddDays(-6);
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private string _rootPath;
        public string RootPath
        {
            get => _rootPath;
            set => SetProperty(ref _rootPath, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        // ============ 三级联动属性 ============
        private VisionDimension? _selectedDataType = VisionDimension.TwoD;
        public VisionDimension? SelectedDataType
        {
            get => _selectedDataType;
            set
            {
                if (SetProperty(ref _selectedDataType, value))
                {
                    RebuildShapes();
                    RebuildFactors();
                }
            }
        }

        private BarShape? _selectedShape = BarShape.Circle;
        public BarShape? SelectedShape
        {
            get => _selectedShape;
            set
            {
                if (SetProperty(ref _selectedShape, value))
                    RebuildFactors();
            }
        }

        private FactorOption _selectedFactor;
        public FactorOption SelectedFactor
        {
            get => _selectedFactor;
            set => SetProperty(ref _selectedFactor, value);
        }

        // ============ 集合 ============
        public ObservableCollection<RateStatistics> Rates { get; } = new ObservableCollection<RateStatistics>();
        public ObservableCollection<LoadedCsvInfo> LoadedFiles { get; } = new ObservableCollection<LoadedCsvInfo>();
        public ObservableCollection<CsvFormatError> Errors { get; } = new ObservableCollection<CsvFormatError>();

        public ObservableCollection<VisionDimension> DataTypes { get; } = new ObservableCollection<VisionDimension>();
        public ObservableCollection<BarShape> Shapes { get; } = new ObservableCollection<BarShape>();
        public ObservableCollection<FactorOption> FactorOptions { get; } = new ObservableCollection<FactorOption>();

        // 是否显示多选项 —— 用独立属性，集合变化时手动通知
        private bool _hasMultipleShapes;
        public bool HasMultipleShapes
        {
            get => _hasMultipleShapes;
            private set => SetProperty(ref _hasMultipleShapes, value);
        }

        private bool _hasMultipleDataTypes;
        public bool HasMultipleDataTypes
        {
            get => _hasMultipleDataTypes;
            private set => SetProperty(ref _hasMultipleDataTypes, value);
        }

        private IReadOnlyList<InspectResult2DData> _rows2D = Array.Empty<InspectResult2DData>();
        private IReadOnlyList<InspectResult3DData> _rows3D = Array.Empty<InspectResult3DData>();

        // ============ 命令实现 ============
        private bool CanAnalyze() => !IsLoading;

        private async Task AnalyzeAsync()
        {
            if (StartDate.Date > EndDate.Date)
            { _dialogService.ShowDialog("开始时间不能晚于结束时间"); return; }
            if (string.IsNullOrWhiteSpace(RootPath) || !Directory.Exists(RootPath))
            { _dialogService.ShowDialog("数据目录不存在"); return; }

            IsLoading = true;
            try
            {
                var result = await _dataService.LoadAsync(StartDate, EndDate, RootPath, CancellationToken.None);
                _rows2D = result.Rows2D;
                _rows3D = result.Rows3D;

                RebuildRates();
                RebuildSelectors();
                RebuildLoadedFiles(result.Files);
                RebuildErrors(result.Errors);

                StatusText = $"2D {_rows2D.Count} 行 / 3D {_rows3D.Count} 行 / " +
                             $"文件 {result.Files.Count} / 错误 {result.Errors.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询失败：" + ex.Message);
            }
            finally { IsLoading = false; }
        }

        private void BrowseFolder()
        {
            var path = PickFolder(RootPath);
            if (!string.IsNullOrEmpty(path)) RootPath = path;
        }

        public string PickFolder(string initialPath)
        {
            var dlg = new OpenFileDialog
            {
                Title = "选择生产数据目录",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "选定此文件夹",
                Filter = "文件夹|*.folder",
                InitialDirectory = Directory.Exists(initialPath) ? initialPath : ""
            };
            return dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK ? Path.GetDirectoryName(dlg.FileName) : null;
        }

        // ============ 统计 ============
        private void RebuildRates()
        {
            Rates.Clear();
            Rates.Add(StatisticsCalculator.Compute("2D总结果", _rows2D, r => r.Result2D, true));
            Rates.Add(StatisticsCalculator.Compute("2D-长度", _rows2D, r => r.ResultLength, false));
            Rates.Add(StatisticsCalculator.Compute("2D-宽度", _rows2D, r => r.ResultWidth, false));
            Rates.Add(StatisticsCalculator.Compute("2D-偏移", _rows2D, r => r.ResultOffset, false));
            Rates.Add(StatisticsCalculator.Compute("2D-爆孔", _rows2D, r => r.ResultPoreBreak, false));
            Rates.Add(StatisticsCalculator.Compute("2D-外径", _rows2D, r => r.ResultBeadDiameter, false));
            Rates.Add(StatisticsCalculator.Compute("2D-虚焊", _rows2D, r => r.ResultfaultySol, false));

            Rates.Add(StatisticsCalculator.Compute("3D总结果", _rows3D, r => r.Result3D, true));
            Rates.Add(StatisticsCalculator.Compute("3D-下塌", _rows3D, r => r.ResultBeadSag, false));
            Rates.Add(StatisticsCalculator.Compute("3D-余高", _rows3D, r => r.ResultBeadHump, false));
        }

        // ============ 三级联动 ============
        private void RebuildSelectors()
        {
            DataTypes.Clear();
            if (_rows2D.Count > 0) DataTypes.Add(VisionDimension.TwoD);
            if (_rows3D.Count > 0) DataTypes.Add(VisionDimension.ThreeD);
            HasMultipleDataTypes = DataTypes.Count > 1;

            if (DataTypes.Count == 0)
            {
                Shapes.Clear(); FactorOptions.Clear();
                HasMultipleShapes = false;
                SelectedDataType = null;
                SelectedShape = null;
                SelectedFactor = null;
                return;
            }
            if (SelectedDataType == null || !DataTypes.Contains(SelectedDataType.Value))
                SelectedDataType = DataTypes[0];
            RebuildShapes();
            RebuildFactors();
        }



        private void RebuildShapes()
        {
            Shapes.Clear();

            if (SelectedDataType == null)
            {
                HasMultipleShapes = false;
                SelectedShape = null;
                return;
            }

            var source = SelectedDataType == VisionDimension.TwoD
                ? _rows2D.Select(r => r.Shape)
                : _rows3D.Select(r => r.Shape);

            foreach (var s in source.Distinct().OrderBy(s => s))
                Shapes.Add(s);

            HasMultipleShapes = Shapes.Count > 1;

            if (Shapes.Count == 0)
            {
                SelectedShape = null;
                return;
            }
            if (SelectedShape == null || !Shapes.Contains(SelectedShape.Value))
                SelectedShape = Shapes[0];
        }

        private void RebuildFactors()
        {
            FactorOptions.Clear();
            if (SelectedDataType == null) { SelectedFactor = null; return; }

            var schema = CsvSchemaRegistry.Get(SelectedDataType.Value, SelectedShape ?? BarShape.Circle);
            if (schema == null) { SelectedFactor = null; return; }

            foreach (var f in schema.Factors) FactorOptions.Add(f);
            if (!FactorOptions.Contains(SelectedFactor))
                SelectedFactor = FactorOptions.FirstOrDefault();
        }

        // ============ 列表 ============
        private void RebuildLoadedFiles(IReadOnlyList<LoadedCsvInfo> files)
        {
            LoadedFiles.Clear();
            foreach (var f in files.OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase))
                LoadedFiles.Add(f);
        }

        private void RebuildErrors(IReadOnlyList<CsvFormatError> errors)
        {
            Errors.Clear();
            foreach (var e in errors) Errors.Add(e);
            if (errors.Count > 0)
                _dialogService.Show($"有 {errors.Count} 处 CSV 格式问题，详情见错误列表。");
        }
    }
}

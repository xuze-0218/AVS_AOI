using AVS_Common.Events;
using AVS_Service;
using AVS_Service.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AVS_Modules_Settings.ViewModels
{
    internal class StationConfigViewModel : BindableBase
    {
        private readonly IStationConfigService _stationConfigService;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly IParametersConfigService _paramService;
        private readonly IEventAggregator _eventAggregator;

        public ObservableCollection<StationConfig> Stations => _stationConfigService.Stations;
        public ObservableCollection<string> AvailableCameraRoles { get; private set; } = new ObservableCollection<string>();
        public ObservableCollection<VisionDimension> AvailableDimensions { get; } = new ObservableCollection<VisionDimension>
        {
            VisionDimension.TwoD,
            VisionDimension.ThreeD
        };
 
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }

        private StationConfig _selectedStation;
        public StationConfig SelectedStation
        {
            get => _selectedStation;
            set => SetProperty(ref _selectedStation, value);
        }

        public StationConfigViewModel(
            IStationConfigService stationConfigService, 
            ICameraConfigService cameraConfigService, 
            IParametersConfigService paramService,
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _stationConfigService = stationConfigService;
            _cameraConfigService = cameraConfigService;
            _paramService = paramService;

            _eventAggregator.GetEvent<SectionsChangedEvent>().Subscribe(() =>
            {
                RefreshProductSections();
            });
            MoveUpCommand = new DelegateCommand(OnMoveUp, () => SelectedStation != null && Stations.IndexOf(SelectedStation) > 0)
                .ObservesProperty(() => SelectedStation);
            MoveDownCommand = new DelegateCommand(OnMoveDown, () => SelectedStation != null && Stations.IndexOf(SelectedStation) < Stations.Count - 1)
                .ObservesProperty(() => SelectedStation);
            DeleteCommand = new DelegateCommand<StationConfig>(OnDelete);
            AddCommand = new DelegateCommand(OnAdd);
            SaveCommand = new DelegateCommand(() => _stationConfigService.Save());
            LoadCommand = new DelegateCommand(SyncWithCameras);
            RefreshCameraRoles();
            RefreshProductSections();
        }

        private void RefreshCameraRoles()
        {
            var roles = _cameraConfigService.AllSettings
              ?.Select(c => c.CameraRole)
              .Where(r => !string.IsNullOrEmpty(r))
              .Distinct()
              .ToList() ?? new List<string>();

            AvailableCameraRoles.Clear();
            foreach (var role in roles)
            {
                AvailableCameraRoles.Add(role);
            }
        }

        private void RefreshProductSections()
        {
            var modules = _paramService.ConfigParams
                ?.Select(p => p.ModuleName)
                 .Where(m => !string.IsNullOrEmpty(m) && m != "Global")  //排除 Global
                .Distinct()
                .OrderBy(m => m)
                .ToList() ?? new List<string>();          
        }

        /// <summary>
        /// 新增section时候自动补全参数
        /// </summary>
        /// <param name="stationId"></param>
        private void EnsureDefaultParamsForStation(string stationId)
        {
            if (_paramService.ConfigParams.Any(p => p.ModuleName == stationId))
                return;

            var defaultSnapshot = new StationParamsSnapshot(); // 每次创建新实例
            var properties = typeof(StationParamsSnapshot).GetProperties();

            foreach (var prop in properties)
            {
                if (!prop.CanRead) continue;

                var value = prop.GetValue(defaultSnapshot);
                string expression = value switch
                {
                    bool b => b ? "true" : "false",
                    double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    int i => i.ToString(),
                    string s => s ?? "",
                    _ => value?.ToString() ?? ""
                };

                var type = value switch
                {
                    bool => ParamOutputType.BOOL,
                    double or float => ParamOutputType.FLOAT,
                    int or long or short => ParamOutputType.INT,
                    _ => ParamOutputType.STRING
                };

                _paramService.ConfigParams.Add(new ParametersConfig
                {
                    ModuleName = stationId,
                    Name = prop.Name,
                    Expression = expression,
                    InitValue = expression,
                    Note = $"自动生成的默认参数（{stationId}）",
                    OutputType = type
                });
            }
            _eventAggregator.GetEvent<SectionsChangedEvent>().Publish();
        }

        private void OnAdd()
        {
            //刷新参数模块列表以确保最新
            RefreshProductSections();
            var newStation = new StationConfig
            {
                StationId = $"Station{Stations.Count + 1}",
                CameraRole = "SelectRole",
            };
            Stations.Add(newStation);
            EnsureDefaultParamsForStation(newStation.StationId);
        }

        private void OnDelete(StationConfig station)
        {
            if (station != null)
                Stations.Remove(station);
        }

        private void OnMoveUp()
        {
            int index = Stations.IndexOf(SelectedStation);
            if (index > 0)
                Stations.Move(index, index - 1);
        }

        private void OnMoveDown()
        {
            int index = Stations.IndexOf(SelectedStation);
            if (index < Stations.Count - 1)
                Stations.Move(index, index + 1);
        }

        /// <summary>
        /// 用当前相机列表同步工位配置，确保每个 CameraRole 有一条记录
        /// </summary>
        public void SyncWithCameras()
        {
            RefreshCameraRoles();
            RefreshProductSections();
            var cameraRoles = AvailableCameraRoles.ToList();
            // 移除不再存在的相机角色配置
            var toRemove = Stations.Where(s => !cameraRoles.Contains(s.CameraRole)).ToList();
            foreach (var item in toRemove)
                Stations.Remove(item);

            // 添加缺失的角色
            foreach (var role in cameraRoles)
            {
                if (!Stations.Any(s => s.CameraRole == role))
                {
                    var newStation = new StationConfig
                    {
                        StationId = role,
                        CameraRole = role,
                    };
                    Stations.Add(newStation);
                    EnsureDefaultParamsForStation(newStation.StationId); 
                }
            }
        }
    }
}

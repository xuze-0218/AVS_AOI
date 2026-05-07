using AVS_Service;
using AVS_Service.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AVS_Modules_Settings.ViewModels
{
    internal class StationConfigViewModel : BindableBase
    {
        private readonly IStationConfigService _stationConfigService;
        private readonly ICameraConfigService _cameraConfigService;

        public ObservableCollection<StationConfig> Stations => _stationConfigService.Stations;
        public ObservableCollection<string> AvailableCameraRoles { get; private set; } = new ObservableCollection<string>();
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

        public StationConfigViewModel(IStationConfigService stationConfigService, ICameraConfigService cameraConfigService)
        {
            _stationConfigService = stationConfigService;
            _cameraConfigService = cameraConfigService;

            MoveUpCommand = new DelegateCommand(OnMoveUp, () => SelectedStation != null && Stations.IndexOf(SelectedStation) > 0)
                .ObservesProperty(() => SelectedStation);
            MoveDownCommand = new DelegateCommand(OnMoveDown, () => SelectedStation != null && Stations.IndexOf(SelectedStation) < Stations.Count - 1)
                .ObservesProperty(() => SelectedStation);
            DeleteCommand = new DelegateCommand<StationConfig>(OnDelete);
            AddCommand = new DelegateCommand(OnAdd);
            SaveCommand = new DelegateCommand(() => _stationConfigService.Save());
            LoadCommand = new DelegateCommand(SyncWithCameras);
            RefreshCameraRoles();
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

        private void OnAdd()
        {
            var newStation = new StationConfig
            {
                StationId = $"Station{Stations.Count + 1}",
                CameraRole = "SelectRole"
            };
            Stations.Add(newStation);
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
                    Stations.Add(new StationConfig
                    {
                        StationId = "DefaultStation",  // 默认用角色名作为工位ID
                        CameraRole = role
                    });
                }
            }
        }
    }
}

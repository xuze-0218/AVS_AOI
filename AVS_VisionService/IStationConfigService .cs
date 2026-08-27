using AVS_Service.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AVS_Service
{
    /// <summary>
    /// 相机与PLC的映射关系
    /// </summary>
    public interface IStationConfigService
    {
        ObservableCollection<StationConfig> Stations { get; }
        void Load();
        void Save();
        StationConfig GetStation(string stationId);
        StationConfig GetStationByCameraRole(string cameraRole);
    }

    public class StationConfigService : IStationConfigService
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "StationConfigs.json");
        private readonly ILogger _logger;

        public ObservableCollection<StationConfig> Stations { get; private set; } = new ObservableCollection<StationConfig>();

        public StationConfigService(ILogger logger)
        {
            _logger = logger;
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var list = JsonConvert.DeserializeObject<List<StationConfig>>(json);
                    Stations = new ObservableCollection<StationConfig>(list ?? new List<StationConfig>());
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                    Save();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "加载工位配置失败");
                Stations = new ObservableCollection<StationConfig>();
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Stations.ToList(), Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "保存工位配置失败");
            }
        }

        public StationConfig GetStation(string stationId) => Stations.FirstOrDefault(s => s.StationId == stationId);

        public StationConfig GetStationByCameraRole(string cameraRole) => Stations.FirstOrDefault(s => s.CameraRole == cameraRole);
    }
}

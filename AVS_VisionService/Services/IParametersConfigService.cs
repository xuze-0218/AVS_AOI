using AVS_Service.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AVS_Service.Services
{
    public interface IParametersConfigService
    {
        /// <summary>
        /// 获取当前配方的名称或路径（供UI显示）
        /// </summary>
        string CurrentRecipePath { get; }
        /// <summary>
        /// 加载指定配方文件夹内的ConfigParas.json
        /// </summary>
        bool LoadRecipe(string recipePath);
        ObservableCollection<ParametersConfig> ConfigParams { get; }
        void LoadConfig();
        bool SaveConfig();
        //bool SaveConfig(IEnumerable<ParametersConfig> configs);
        void UpdateParam(string moduleName, string paramName, string value, ParamOutputType type = ParamOutputType.STRING);
        int GetInt(string moduleName, string paramName, int defaultValue = 0);
        double GetDouble(string moduleName, string paramName, double defaultValue = 0.0);
        string GetString(string moduleName, string paramName, string defaultValue = "");
        bool GetBool(string moduleName, string paramName, bool defaultValue = false);
        StationParamsSnapshot GetStationParams(string stationId);
    }

    public class ParametersConfigService : IParametersConfigService
    {
        private string _configPath;
        public string CurrentRecipePath { get; private set; }
        public ObservableCollection<ParametersConfig> ConfigParams { get; private set; } = new ObservableCollection<ParametersConfig>();


        public ParametersConfigService()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ConfigParas.json");
            CurrentRecipePath = _configPath;
            //ConfigParams = new ObservableCollection<ParametersConfig>();
            LoadConfig();
        }

        public bool LoadRecipe(string recipePath)
        {
            if (!Directory.Exists(recipePath))
                return false;
            var file = Path.Combine(recipePath, "ConfigParas.json");
            if (!File.Exists(file))
                return false;
            _configPath = file;
            CurrentRecipePath = recipePath;
            LoadConfig();
            return true;
        }

        public int GetInt(string moduleName, string paramName, int defaultValue = 0)
        {
            var p = FindParam(moduleName, paramName);
            if (p != null && int.TryParse(p.Expression, out int result)) return result;
            return defaultValue;
        }

        public double GetDouble(string moduleName, string paramName, double defaultValue = 0.0)
        {
            var p = FindParam(moduleName, paramName);
            if (p != null && double.TryParse(p.Expression, out double result)) return result;
            return defaultValue;
        }

        public string GetString(string moduleName, string paramName, string defaultValue = "")
        {
            var p = FindParam(moduleName, paramName);
            return p != null ? p.Expression : defaultValue;
        }

        public bool GetBool(string moduleName, string paramName, bool defaultValue = false)
        {
            var p = FindParam(moduleName, paramName);
            if (p != null && bool.TryParse(p.Expression, out bool result)) return result;
            if (p != null && p.Expression == "1") return true;
            if (p != null && p.Expression == "0") return false;
            return defaultValue;
        }

        public void UpdateParam(string moduleName, string paramName, string value, ParamOutputType type = ParamOutputType.STRING)
        {
            var p = FindParam(moduleName, paramName);
            if (p != null)
            {
                p.Expression = value;
                p.OutputType = type;
            }
            else
            {
                ConfigParams.Add(new ParametersConfig
                {
                    ModuleName = moduleName,
                    Name = paramName,
                    Expression = value,
                    OutputType = type,
                });
            }
        }

        public void LoadConfig()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    var json = File.ReadAllText(_configPath);
                    var list = JsonConvert.DeserializeObject<ObservableCollection<ParametersConfig>>(json);
                    ConfigParams.Clear();
                    if (list != null)
                    {
                        foreach (var p in list) ConfigParams.Add(p);
                        Log.Information("参数配置加载成功，共 {Count} 项", ConfigParams.Count);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "加载配置文件 ConfigParas.json 失败");
                }
            }
            else
            {
                Log.Warning("配置文件不存在: {Path}", _configPath);
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
                Log.Warning("创建配置文件: {Path}", _configPath);

            }
        }

        public bool SaveConfig()
        {
            try
            {
                var json = JsonConvert.SerializeObject(ConfigParams, Formatting.Indented);
                File.WriteAllText(_configPath, json);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "配置保存失败");
                return false;
            }
        }

        private ParametersConfig FindParam(string moduleName, string paramName)
        {
            // 如果 moduleName 为空，则在全局搜索 paramName；否则按模块匹配
            return ConfigParams.FirstOrDefault(p =>
                (string.IsNullOrEmpty(moduleName) || p.ModuleName == moduleName) &&
                p.Name == paramName);
        }

        public StationParamsSnapshot GetStationParams(string stationId)
        {
            return new StationParamsSnapshot
            {
                IsNormalCheck = GetBool(stationId, "IsNormalCheck"),
                IsAiCheck = GetBool(stationId, "IsAiCheck"),
                IsRotated = GetBool(stationId, "IsRotated"),
                IsSquareBarWeldMark = GetBool(stationId, "IsSquareBarWeldMark"),
                IsCirWeldMark = GetBool(stationId, "IsCirWeldMark"),
                ScoreValue = GetDouble(stationId, "ScoreValue", 0.8),
                IsPlaneCheck = GetBool(stationId, "IsPlaneCheck"),
                Fx = GetDouble(stationId, "Fx", 0.014),
                Fy = GetDouble(stationId, "Fy", 0.014),
                Fz = GetDouble(stationId, "Fz", 0.005),
                RecipePath = GetString(stationId, "RecipePath"),
                AngleStart = GetDouble(stationId, "AngleStart", 0),
                AngleExtent = GetDouble(stationId, "AngleExtent", 360),
                MinScale = GetDouble(stationId, "MinScale", 0.9),
                MaxScale = GetDouble(stationId, "MaxScale", 1.1),
                MinScore = GetDouble(stationId, "MinScore", 0.5),
                MaxMatchNum = GetInt(stationId, "MaxMatchNum", 1),
                MaxOverlap = GetDouble(stationId, "MaxOverlap", 0.5),
                NumLevel = GetInt(stationId, "NumLevel", 0),
                Greediness = GetDouble(stationId, "Greediness", 0.9),
                SubPixel = GetString(stationId, "SubPixel", "least_squares"),
                DetModelPath = GetString(stationId, "DetModelPath", ""),
                SegModelPaths = GetString(stationId, "SegModelPaths", ""),
            };
        }
    }
}
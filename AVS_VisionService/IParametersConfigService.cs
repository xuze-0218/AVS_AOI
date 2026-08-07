using AVS_Service.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service
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
        StationParamsSnapshot GetStationParams(string section);
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
                Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
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

        public StationParamsSnapshot GetStationParams(string section)
        {
            return new StationParamsSnapshot
            {
                IsNormalCheck = GetBool(section, "IsNormalCheck"),
                IsAiCheck = GetBool(section, "IsAiCheck"),
                IsRotated = GetBool(section, "IsRotated"),
                IsSquareBarWeldMark = GetBool(section, "IsSquareBarWeldMark"),
                IsCirWeldMark = GetBool(section, "IsCirWeldMark"),
                ScoreValue = GetDouble(section, "ScoreValue", 0.8),
                IsPlaneCheck = GetBool(section, "IsPlaneCheck"),
                Fx = GetDouble(section, "Fx", 0.014),
                Fy = GetDouble(section, "Fy", 0.014),
                Fz = GetDouble(section, "Fz", 0.005),
                RecipePath = GetString(section, "RecipePath"),
                ImageSaveDir = GetString(section, "ImageSaveDir"),
                IsSaveOrnImg = GetBool(section, "IsSaveOrnImg"),
                IsSaveOkRenImg = GetBool(section, "IsSaveOkRenImg"),
                IsSaveNgRenImg = GetBool(section, "IsSaveNgRenImg"),
                SaveOrnImgDays = GetInt(section, "SaveOrnImgDays", 30),
                SaveRenImgDays = GetInt(section, "SaveRenImgDays", 30),
            };
        }
    }
}
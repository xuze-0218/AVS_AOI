using AVS_Common.Model;
using AVS_Service.Models;
using Newtonsoft.Json;
using Serilog;
using System.IO;


namespace AVS_Service
{

    public interface IProtocolConfigRepository
    {
        /// <summary>
        /// 获取指定工位的公共头解析配置（用于提取funcCode）
        /// </summary>
        List<ProtocolField> GetCommonHeaderConfig(string stationId);

        /// <summary>
        /// 根据工位和功能码获取具体的报文配置
        /// </summary>
        SessionConfig GetSpecificConfig(string stationId, string funcCode);


        void ReloadConfig();
    }

    public class ProtocolConfigRepository : IProtocolConfigRepository
    {
        private readonly ILogger _logger;
        private readonly string _configPath;
        private Dictionary<string, StationProtocolConfig> _stationConfigs; // 内存缓存

        public ProtocolConfigRepository(ILogger logger)
        {
            _logger = logger;
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ProtocolConfig.json");
            LoadConfig();
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    // 将JSON数组转换为以 StationId 为 Key 的字典，方便查询
                    var list = JsonConvert.DeserializeObject<List<StationProtocolConfig>>(json);
                    _stationConfigs = list.ToDictionary(s => s.StationId);
                }
                else
                {
                    _stationConfigs = new Dictionary<string, StationProtocolConfig>();
                    _logger.Error("报文配置文件不存在");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("报文配置转换失败:");
            }
        }

        public void ReloadConfig()
        {
            _stationConfigs = new Dictionary<string, StationProtocolConfig>(); // 清空现有缓存
            LoadConfig();
        }

        public List<ProtocolField> GetCommonHeaderConfig(string stationId)
        {
            if (_stationConfigs.TryGetValue(stationId, out var config))
                return config.CommonHeaderFields;
            return new List<ProtocolField>();
        }

        public SessionConfig GetSpecificConfig(string stationId, string funcCode)
        {
            if (_stationConfigs.TryGetValue(stationId, out var config))
            {
                return config.Messages.FirstOrDefault(m => m.FuncCode == funcCode);
            }
            return null;
        }


    }
}

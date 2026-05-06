using AVS_Core.Models;
using AVS_Service;
using AVS_Service.Models;
using Serilog;


namespace AVS_Core.Services
{
    public interface IPlcMessageRouter
    {
        /// <summary>
        /// 处理来自PLC的原始报文
        /// </summary>
        Task HandleMessageAsync(string rawMessage);
    }


    public class PlcMessageRouter : IPlcMessageRouter
    {
        private readonly IParametersConfigService _paramService;
        private readonly IProtocolEngineService _protocolEngine;
        private readonly IProtocolConfigRepository _configRepo;
        private readonly ICommunicationService _commService;
        private readonly IStationSessionService _sessionService;
        private readonly ILogger _logger;

        public PlcMessageRouter(
            IParametersConfigService paramService,
            IProtocolEngineService protocolEngine,
            IProtocolConfigRepository configRepo,
            ICommunicationService commService,
            IStationSessionService sessionService,
            ILogger logger)
        {
            _protocolEngine = protocolEngine;
            _paramService = paramService;
            _configRepo = configRepo;
            _commService = commService;
            _sessionService = sessionService;
            _logger = logger;
        }

        public async Task HandleMessageAsync(string rawMessage)
        {
            try
            {
                _protocolEngine.ClearVariables();

                string stationId = _paramService.GetString("Global", "CurrentStationID", "Station01");
                // 解析公共头，提取功能码
                var headerConfig = _configRepo.GetCommonHeaderConfig(stationId);
                _protocolEngine.ParseInput(rawMessage, headerConfig);
                string funcCode = _protocolEngine.GetVariable("funcCode");
                if (string.IsNullOrEmpty(funcCode))
                {
                    _logger.Error("Failed to parse funcCode from message");
                    return;
                }

                // 加载具体会话配置
                var sessionConfig = _configRepo.GetSpecificConfig(stationId, funcCode);
                if (sessionConfig == null)
                {
                    _logger.Error("No config for funcCode {FuncCode}", funcCode);
                    return;
                }

                // 完整解析输入字段
                _protocolEngine.ParseInput(rawMessage, sessionConfig.InputFields);

                // 根据功能码和 backup01 分发业务。
                // 0001：检测开始，相机准备开始     0002：检测结束，获取检测结果
                string step = _protocolEngine.GetVariable("backup1") ?? "0001";

                string response = string.Empty;

                if (funcCode == "2003") // 检测
                {
                    response = step switch
                    {
                        "0001" => HandleInspectInit(stationId, sessionConfig),
                        "0002" => HandleInspectResult(stationId, sessionConfig),
                        _ => CreateErrorResponse(sessionConfig, "Unknown step")
                    };
                }
                else if (funcCode == "2001") // 标定\点检
                {
                    //硬编码，04是点检，03是标定
                    bool isverify = _protocolEngine.GetVariable("calibType") == "04";
                    response = step switch
                    {
                        "0001" => HandleCalibInit(stationId, sessionConfig, isverify),
                        "0002" => HandleCalibResult(stationId, sessionConfig),
                        _ => CreateErrorResponse(sessionConfig, "Unknown step")
                    };
                }
                else
                {
                    _logger.Warning("Unhandled funcCode {FuncCode}", funcCode);
                    return;
                }

                if (!string.IsNullOrEmpty(response))
                {
                    await _commService.SendAsync(response);
                    _logger.Information("Response sent to PLC on {StationId}", stationId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling PLC message");
            }
        }

        private string HandleInspectInit(string stationId, SessionConfig config)
        {
            //// 从变量池提取初始化数据
            //var initData = new InspectionInitData
            //{
            //    ModuleName = _protocolEngine.GetVariable("nameText") ?? "Unknown",
            //    StartPole = int.Parse(_protocolEngine.GetVariable("backup02_start") ?? "1"),
            //    EndPole = int.Parse(_protocolEngine.GetVariable("backup02_end") ?? "1"),
            //    // 此处应从产品参数服务获取 inspectOrder
            //    InspectOrder = GenerateInspectOrder(/* 根据 project 获取 */)
            //};

            _sessionService.InitializeSession(stationId, SessionWorkType.Inspect);

            // 构建初始化成功报文（包含占位结果）
            _protocolEngine.SetVariable("Result", "01");
            string resultData = string.Concat(Enumerable.Repeat("01" + new string('0', 48), 25));
            _protocolEngine.SetVariable("ResultData", resultData);
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private string HandleInspectResult(string stationId, SessionConfig config)
        {
            int start = int.Parse(_protocolEngine.GetVariable("backup02_start"));
            int end = int.Parse(_protocolEngine.GetVariable("backup02_end"));
            string resultData = _sessionService.GetResultData(stationId, start, end);
            _protocolEngine.SetVariable("Result", "01");
            _protocolEngine.SetVariable("ResultData", resultData);
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private string HandleCalibInit(string stationId, SessionConfig config, bool isVerify)
        {
            _sessionService.InitializeSession(stationId,
                isVerify ? SessionWorkType.Verify : SessionWorkType.Calibrate);

            // 标定初始化成功返回固定格式
            _protocolEngine.SetVariable("Result", "01");
            _protocolEngine.SetVariable("ResultData", "+0000000+0000000");
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private string HandleCalibResult(string stationId, SessionConfig config)
        {
            string calibResult = _sessionService.GetResultData(stationId);
            // 假设 calibResult 格式为 "01+0000000+0000000" 共18字符，按协议拆分
            _protocolEngine.SetVariable("Result", calibResult.Substring(0, 2));
            _protocolEngine.SetVariable("ResultData", calibResult.Substring(2));
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private string CreateErrorResponse(SessionConfig config, string errorMsg)
        {
            _protocolEngine.SetVariable("Result", "02");
            _protocolEngine.SetVariable("ResultData", new string('0', 1250)); // 全零结果
            _logger.Warning("Error response: {Msg}", errorMsg);
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private int[] GenerateInspectOrder(/* 参数 */)
        {
            // 临时返回连续顺序，实际应从产品参数读取
            return Enumerable.Range(1, 90).ToArray();
        }
    }
}

using AVS_Core.Models;
using AVS_Service;
using AVS_Service.Models;
using Serilog;


namespace AVS_Core.Services
{
    public interface IPlcMessageRouter
    {
        /// <summary>
        /// 处理来自PLC的原始报文，进行任务分发
        /// </summary>
        /// <param name="connectionPlcId">根据传入的plc名映射指定相机名</param>
        /// <param name="rawMessage">Plc报文</param>
        /// <returns></returns>
        Task HandleMessageAsync(string connectionPlcId, string rawMessage);
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


        public async Task HandleMessageAsync(string connectionPlcId, string rawMessage)
        {
            try
            {
                _protocolEngine.ClearVariables();

                ///此处stationId为工位名，如焊后检测
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

                // 根据功能码和 backup01 划分。
                // 0001：检测开始，相机准备开始     0002：检测结束，获取检测结果
                string step = _protocolEngine.GetVariable("backup1") ?? "0001";

                string response = string.Empty;

                if (funcCode == "2003") // 检测
                {
                    response = step switch
                    {
                        "0001" => HandleInspectInit(connectionPlcId, sessionConfig),
                        "0002" => HandleInspectResult(connectionPlcId, sessionConfig),
                        _ => CreateErrorResponse(sessionConfig, "Unknown step")
                    };
                }
                else if (funcCode == "2001") // 标定\点检
                {
                    //硬编码，04是点检，03是标定
                    bool isverify = _protocolEngine.GetVariable("calibType") == "04";
                    response = step switch
                    {
                        "0001" => HandleCalibInit(connectionPlcId, sessionConfig, isverify),
                        "0002" => HandleCalibResult(connectionPlcId, sessionConfig),
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
                    await _commService.SendAsync(connectionPlcId, response);
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
            _sessionService.InitializeSession(stationId, SessionWorkType.Inspect);
            string InspectResult = _sessionService.GetResultData(stationId);
            // 构建初始化成功报文（包含占位结果）
            _protocolEngine.SetVariable("Result", InspectResult);
            _protocolEngine.SetVariable("backup3", "0000");
            _protocolEngine.SetVariable("backup4", "0000");
            _protocolEngine.SetVariable("backup5", "0000");
            _protocolEngine.SetVariable("backup6", "0000");
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

            string calibResult = _sessionService.GetResultData(stationId);
            //格式为 "01+0000000+0000000" 共18字符，按协议拆分
            // 标定初始化成功返回固定格式   这里是硬编码测试
            _protocolEngine.SetVariable("Result", calibResult.Substring(0, 2));
            //backup1不需要，这里放了一个和Result一样的值，后面改
            _protocolEngine.SetVariable("backup2", calibResult.Substring(0, 2));
            _protocolEngine.SetVariable("backup3", calibResult.Substring(2, 4));
            _protocolEngine.SetVariable("backup4", calibResult.Substring(6, 4));
            _protocolEngine.SetVariable("backup5", calibResult.Substring(10, 4));
            _protocolEngine.SetVariable("backup6", calibResult.Substring(14, 4));
            _logger.Information("Calibration session initialized for {StationId}, isVerify: {IsVerify}", stationId, isVerify);
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private string HandleCalibResult(string stationId, SessionConfig config)
        {
            string calibResult = _sessionService.GetResultData(stationId);
            _protocolEngine.SetVariable("Result", calibResult.Substring(0, 2));
            _protocolEngine.SetVariable("backup2", calibResult.Substring(0, 2));
            _protocolEngine.SetVariable("backup3", calibResult.Substring(2, 4));
            _protocolEngine.SetVariable("backup4", calibResult.Substring(6, 4));
            _protocolEngine.SetVariable("backup5", calibResult.Substring(10, 4));
            _protocolEngine.SetVariable("backup6", calibResult.Substring(14, 4));
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private string CreateErrorResponse(SessionConfig config, string errorMsg)
        {
            _protocolEngine.SetVariable("Result", "02");
            _protocolEngine.SetVariable("ResultData", new string('0', 1250)); // 全零结果
            _logger.Warning("Error response: {Msg}", errorMsg);
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

    }
}

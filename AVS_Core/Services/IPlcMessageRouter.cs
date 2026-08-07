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
                        "0001" => await HandleInspectInit(connectionPlcId, sessionConfig),
                        "0002" => await HandleInspectResult(connectionPlcId, sessionConfig),
                        _ => CreateErrorResponse(sessionConfig, "Unknown step")
                    };
                }
                else if (funcCode == "2001") // 标定\点检
                {
                    //硬编码，04是点检，03是标定
                    bool isverify = _protocolEngine.GetVariable("calibType") == "04";
                    response = step switch
                    {
                        "0001" => await HandleCalibInit(connectionPlcId, sessionConfig, isverify),
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

        /// <summary>
        /// 第一次初始化检测会话里面包含了引擎加载及参数配置
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private async Task<string> HandleInspectInit(string stationId, SessionConfig config)
        {
            int inspectStNum = Convert.ToInt32(_protocolEngine.GetVariable("backup2").Substring(0, 2)); //获取检测极柱开始序号
            int inspectEdNum = Convert.ToInt32(_protocolEngine.GetVariable("backup2").Substring(2, 2)); //获取检测极柱结束序号
            int numForInspect = inspectEdNum - inspectStNum + 1;                                        //获取需要检测的极柱总个数
            int[] inspectOrder = new int[numForInspect];

            int msgPoleCapacity = Convert.ToInt32(_protocolEngine.GetVariable("version")) == 1 ? 10 : 25;//版本号为1：10；为2：25
            string imageName = _protocolEngine.GetVariable("imgName");
            int orderIndex = int.Parse(_protocolEngine.GetVariable("inspectType"));            
            var p = _paramService.GetStationParams(stationId);

            if (p.InspectOrders == null || orderIndex < 1 || orderIndex > p.InspectOrders.Length)
            {
                _logger.Error("无效的检测类型索引: {Index}, 工位: {StationId}", orderIndex, stationId);
                return CreateErrorResponse(config, "Invalid inspect order index");
            }
            InspectOrder order = p.InspectOrders[orderIndex - 1];
            for (int j = 0; j < order.row; j++)
            {
                int mdiff = (int)(Math.Abs(order.end[j] - order.start[j])) / (order.col - 1);
                if (order.end[j] - order.start[j] < 0)
                    mdiff = -mdiff;

                for (int i = 0; i < order.col; i++)
                {
                    inspectOrder[i + j * order.col] = (int)order.start[j] + mdiff * i;
                }
            }
            var initParams = new InspectionInitParams
            {
                ImageName = imageName,
                MsgPoleCapacity = msgPoleCapacity,
                PoleOrder = inspectOrder
            };

            await _sessionService.InitializeSession(stationId, SessionWorkType.Inspect, initParams);
            string initResultData = string.Concat(Enumerable.Repeat("01" + new string('0', 48), msgPoleCapacity));
            // 构建初始化成功报文（包含占位结果）
            _protocolEngine.SetVariable("Result", initResultData);
            _protocolEngine.SetVariable("backup3", "0000");
            _protocolEngine.SetVariable("backup4", "0000");
            _protocolEngine.SetVariable("backup5", "0000");
            _protocolEngine.SetVariable("backup6", "0000");
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private async Task<string> HandleInspectResult(string stationId, SessionConfig config)
        {
            string result = "01";
            string resultData = "";
            int startPole = int.Parse(_protocolEngine.GetVariable("backup2").Substring(0, 2));
            int endPole = int.Parse(_protocolEngine.GetVariable("backup2").Substring(2, 2));
            int msgPoleCapacity = Convert.ToInt32(_protocolEngine.GetVariable("version")) == 1 ? 10 : 25;//版本号为1：10；为2：25
            try
            {
                resultData = await _sessionService.GetResultDataAsync(stationId, startPole, endPole, msgPoleCapacity);
            }
            catch (TimeoutException)
            {
                _logger.Warning("工位 {StationId} 获取极柱 {Start}-{End} 结果超时", stationId, startPole, endPole);
                resultData = string.Concat(Enumerable.Repeat("00" + new string('0', 48), msgPoleCapacity));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "工位 {StationId} 获取结果失败", stationId);
                resultData = string.Concat(Enumerable.Repeat("02" + new string('0', 48), msgPoleCapacity));
            }
            _protocolEngine.SetVariable("Result", "01");
            _protocolEngine.SetVariable("ResultData", resultData);
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private async Task<string> HandleCalibInit(string stationId, SessionConfig config, bool isVerify)
        {
            await _sessionService.InitializeSession(stationId, isVerify ? SessionWorkType.Verify : SessionWorkType.Calibrate);

            // 标定初始化成功返回固定格式
            _protocolEngine.SetVariable("Result", "01");          // 初始化成功
            _protocolEngine.SetVariable("backup2", "0000");       
            _protocolEngine.SetVariable("backup3", "0000");
            _protocolEngine.SetVariable("backup4", "0000");
            _protocolEngine.SetVariable("backup5", "0000");
            _protocolEngine.SetVariable("backup6", "0000");
            _logger.Information("Calibration session initialized for {StationId}, isVerify: {IsVerify}", stationId, isVerify);
            return _protocolEngine.BuildOutput(config.OutputFields);
        }

        private string HandleCalibResult(string stationId, SessionConfig config)
        {
            string calibResult = _sessionService.GetResultData(stationId);
            if (string.IsNullOrEmpty(calibResult) || calibResult.Length < 16)
            {
                _logger.Error("标定结果数据无效: {Data}", calibResult ?? "null");
                return CreateErrorResponse(config, "Invalid calibration result");
            }
            _protocolEngine.SetVariable("Result", calibResult.Substring(0, 2));
            _protocolEngine.SetVariable("backup2", calibResult.Substring(2, 4));
            _protocolEngine.SetVariable("backup3", calibResult.Substring(6, 4));
            _protocolEngine.SetVariable("backup4", calibResult.Substring(10, 4));
            _protocolEngine.SetVariable("backup5", calibResult.Substring(14, 4));
            _protocolEngine.SetVariable("backup6", "0000");
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

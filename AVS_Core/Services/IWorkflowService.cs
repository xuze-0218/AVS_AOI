using AVS_Service;
using AVS_Service.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Services
{
    public enum WorkflowMode
    {
        Idle,
        Running
    }
    /// <summary>
    /// 业务处理
    /// </summary>
    public interface IWorkflowService
    {
        Task ProcessPlcTriggerAsync(string rawPlcData);
        void StopWorkflow();
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IAlgorithmService _visionService;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly ICommunicationService _communicationService;
        private readonly IProtocolEngineService _protocolEngine;
        private readonly IParametersConfigService _paramService;
        private readonly ILogger _logger;

        private readonly SemaphoreSlim _runLock = new SemaphoreSlim(1, 1); //防重触发

        public WorkflowService(
            IAlgorithmService visionService,
            ICameraConfigService cameraConfigService,
            ICommunicationService communicationService,
            IProtocolEngineService protocolEngine,
            IParametersConfigService paramConfig,
            ILogger logger)
        {
            _visionService = visionService;
            _cameraConfigService = cameraConfigService;
            _communicationService = communicationService;
            _protocolEngine = protocolEngine;
            _paramService = paramConfig;
            _logger = logger;
        }

        public async Task ProcessPlcTriggerAsync(string rawPlcData)
        {

            if (!await _runLock.WaitAsync(0))
            {
                _logger.Warning("系统正忙，丢弃本次触发");
                await _communicationService.SendAsync("BUSY_RESPONSE");
                return;
            }
            try
            {
                //读取当前工位号(对应界面中的Global-CurrentStationID)
                string currentStationId = _paramService.GetString("Global", "CurrentStationID", "Station01");
                _protocolEngine.ClearVariables();
              
                //更新plcData
                _paramService.UpdateParam("Global", "plcData", rawPlcData, ParamOutputType.STRING);
                //解析报文
                _protocolEngine.ParseInput(rawPlcData, _protocolEngine.GetInputFields());
             
                //相机拍照，这里先写死相机ID，后续根据输入报文或配置调整
                _cameraConfigService.ExecuteSoftTrigger("cam1");
                //调用视觉算法，这里先占位，后续根据算法输入输出调整
                await _visionService.Execute(); //这里有问题，相机拍照后立即执行算法可能拿不到图片，实际需要等相机拍照完成的事件通知后再执行算法。而且这里只是硬编码一个相机

                _protocolEngine.SetVariable("result", "98");              

                //创建返回报文
                string plcResponse = _protocolEngine.BuildOutput(_protocolEngine.GetOutputFields());
                //发送报文
                await _communicationService.SendAsync(plcResponse);

                _runLock.Release();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "处理 PLC 触发异常");
                try
                {
                    _protocolEngine.SetVariable("Result", "99");
                    //string errorResponse = _protocolEngine.BuildOutput(specificConfig.OutputFields);
                    await _communicationService.SendAsync("Error");
                }
                catch { }
            }
        }

        public void StopWorkflow()
        {

        }
    }
}

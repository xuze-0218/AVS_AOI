using AVS_Common;
using AVS_Service;
using AVS_Service.Models;
using HalconDotNet;
using Prism.Events;
using Serilog;
using System;
using System.Collections.Concurrent;
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
    //public interface IWorkflowService
    //{
    //    /// <summary>
    //    /// 处理PLC触发的业务流程，包含解析PLC数据、调用相机拍照、执行视觉算法、构建并发送PLC响应等核心逻辑
    //    /// </summary>
    //    /// <param name="rawPlcData"></param>
    //    /// <returns></returns>
    //    Task ProcessPlcTriggerAsync(string rawPlcData);
    //    void StopWorkflow();
    //}

    //public class WorkflowService : IWorkflowService
    //{
    //    private readonly ICameraConfigService _cameraConfigService;
    //    private readonly ICommunicationService _communicationService;
    //    private readonly IProtocolConfigRepository _configRepository;
    //    private readonly IProtocolEngineService _protocolEngine;
    //    private readonly IParametersConfigService _paramService;
    //    private readonly IEventAggregator _eventAggregator;
    //    private readonly ILogger _logger;

    //    private readonly SemaphoreSlim _runLock = new SemaphoreSlim(1, 1); //防重触发
    //    /// <summary>
    //    /// 用于保存等待图像的任务源，Key为相机SN或Identifier
    //    /// </summary>
    //    private readonly ConcurrentDictionary<string, TaskCompletionSource<HObject>> _pendingImageTasks
    //        = new ConcurrentDictionary<string, TaskCompletionSource<HObject>>();

    //    public WorkflowService(
    //        ICameraConfigService cameraConfigService,
    //        ICommunicationService communicationService,
    //        IProtocolEngineService protocolEngine,
    //        IParametersConfigService paramConfig,
    //        IProtocolConfigRepository configRepository,
    //        IEventAggregator eventAggregator,
    //        ILogger logger)
    //    {
    //        _configRepository = configRepository;
    //        _communicationService = communicationService;
    //        _cameraConfigService = cameraConfigService;
    //        _eventAggregator = eventAggregator;
    //        _protocolEngine = protocolEngine;
    //        _paramService = paramConfig;
    //        _logger = logger;
    //        _eventAggregator.GetEvent<HImageDisplayEvent>().Subscribe(OnImageCaptured);
    //    }

    //    private void OnImageCaptured(CameraImagePayload payload)
    //    {
    //        // 如果字典里有正在等待这个相机出图的 Task，就把图片塞给它并放行
    //        if (_pendingImageTasks.TryRemove(payload.CameraSN, out var tcs))
    //        {
    //            HOperatorSet.CopyImage(payload.Image, out HObject clonedImage);
    //            tcs.TrySetResult(clonedImage);
    //        }
    //    }

    //    public async Task ProcessPlcTriggerAsync(string rawPlcData)
    //    {

    //        if (!await _runLock.WaitAsync(0))
    //        {
    //            _logger.Warning("系统正忙，丢弃本次触发");
    //            await _communicationService.SendAsync("BUSY_RESPONSE");
    //            return;
    //        }
    //        try
    //        {
    //            ////读取当前工位号(对应界面中的Global-CurrentStationID)
    //            string currentStationId = _paramService.GetString("Global", "CurrentStationID", "Station01");
    //            _protocolEngine.ClearVariables();
    //            //更新plcData
    //            _paramService.UpdateParam("Global", "plcData", rawPlcData, ParamOutputType.STRING);
    //            //解析报文


    //            var headerConfig = _configRepository.GetCommonHeaderConfig(currentStationId);
    //            _protocolEngine.ParseInput(rawPlcData, headerConfig);

    //            string funcCode = _protocolEngine.GetVariable("funcCode");
    //            if (string.IsNullOrEmpty(funcCode))
    //            {
    //                _logger.Error("解析公共头失败，未获取到 funcCode. 报文: {Data}", rawPlcData);
    //                return;
    //            }

    //            //加载具体解析
    //            SessionConfig specificConfig = _configRepository.GetSpecificConfig(currentStationId, funcCode);
    //            if (specificConfig == null)
    //            {
    //                _logger.Error("未找到工位 {StationId} 功能码 {FuncCode} 的配置", currentStationId, funcCode);
    //                return;
    //            }
    //            _protocolEngine.ParseInput(rawPlcData, specificConfig.InputFields);
    //            string camSn = GetTargetCameraByStep(funcCode); // 根据业务获取对应的相机
    //            if (string.IsNullOrEmpty(camSn))
    //            {
    //                throw new Exception($"未找到步骤 {funcCode} 对应的相机配置");
    //            }
    //            //准备等待图像的任务
    //            var tcs = new TaskCompletionSource<HObject>();
    //            _pendingImageTasks[camSn] = tcs;
    //            //发送软触发信号,这里也使用的是软触发，不是硬触发模式，实际可能还需要修改逻辑
    //            _logger.Information($"下发软触发至相机: {camSn}");
    //            if (!_cameraConfigService.ExecuteSoftTrigger(camSn))
    //            {
    //                _pendingImageTasks.TryRemove(camSn, out _);
    //                throw new Exception("相机触发失败");
    //            }

    //            //等待图像返回（设置3秒超时防死锁）
    //            HObject grabImage = null;
    //            try
    //            {
    //                // 这里的 await 会挂起，直到OnImageCaptured 把图片塞进来，所以如果是硬触发，逻辑要重写
    //                grabImage = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30));
    //                _logger.Information($"成功获取到相机 {camSn} 的图像");
    //            }
    //            catch (TimeoutException)
    //            {
    //                _pendingImageTasks.TryRemove(camSn, out _);
    //                throw new Exception($"等待相机 {camSn} 出图超时");
    //            }

    //            string resultStr = "99";
    //            if (grabImage != null && grabImage.IsInitialized())
    //            {
    //                try
    //                {

    //                    // resultStr = await _visionService.ExecuteAsync(grabImage, stepCode);
    //                    // 模拟处理耗时
    //                    await Task.Delay(100);
    //                    resultStr = "22"; // 模拟OK
    //                }
    //                finally
    //                {
    //                    grabImage.Dispose(); //释放图像内存
    //                }
    //            }
    //            _protocolEngine.SetVariable("Result", resultStr);
    //            //创建返回报文
    //            string plcResponse = _protocolEngine.BuildOutput(specificConfig.OutputFields);
    //            //发送报文
    //            await _communicationService.SendAsync(plcResponse);

    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.Error(ex, "处理PLC报文异常");
    //            _protocolEngine.SetVariable("Result", "99");
    //            try
    //            {
    //                string errorResponse = _protocolEngine.BuildOutput(_protocolEngine.GetOutputFields());
    //                await _communicationService.SendAsync(errorResponse);
    //            }
    //            catch { }
    //        }
    //        finally { _runLock.Release(); }
    //    }

    //    public void StopWorkflow()
    //    {
    //        // 清理逻辑
    //        _eventAggregator.GetEvent<HImageDisplayEvent>().Unsubscribe(OnImageCaptured);
    //        foreach (var tcs in _pendingImageTasks.Values)
    //        {
    //            tcs.TrySetCanceled();
    //        }
    //        _pendingImageTasks.Clear();
    //    }

    //    /// <summary>
    //    /// 这里暂时硬编码，没有处理多个相机的情况。且目前默认为标定为2d相机。
    //    /// </summary>
    //    /// <param name="funcCode"></param>
    //    /// <returns></returns>
    //    private string GetTargetCameraByStep(string funcCode)
    //    {
    //        return funcCode switch
    //        {
    //            "2001" => _cameraConfigService.AllSettings.FirstOrDefault(c => c.CameraRole.Equals("cam2d", StringComparison.CurrentCultureIgnoreCase))?.SerilalNum,
    //            "2003" => _cameraConfigService.AllSettings.FirstOrDefault(c => c.CameraRole.Equals("cam3d", StringComparison.CurrentCultureIgnoreCase))?.SerilalNum,
    //        };
    //    }
    //}
}

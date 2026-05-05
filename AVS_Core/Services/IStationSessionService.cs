using AVS_Core.Models;
using HalconDotNet;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Services
{
    public interface IStationSessionService
    {
        /// <summary>
        /// 初始化一个会话（检测、标定或点检）
        /// </summary>
        void InitializeSession(string stationId, SessionWorkType workType, object initData);

        /// <summary>
        /// 相机采集到图像后，将其放入对应工位的处理队列
        /// </summary>
        void EnqueueImage(string stationId, HObject image);

        /// <summary>
        /// 获取检测或标定的结果数据（用于PLC回复）
        /// 检测模式需要提供极柱开始和结束序号
        /// </summary>
        string GetResultData(string stationId, int? startIndex = null, int? endIndex = null);

        /// <summary>
        /// 重置指定工位的会话
        /// </summary>
        void Reset(string stationId);
    }

    public class StationSessionService : IStationSessionService
    {
        private readonly IVisionService _visionService;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SessionState> _sessions = new();

        public StationSessionService(IVisionService visionService, ILogger logger)
        {
            _visionService = visionService;
            _logger = logger;
        }

        public void InitializeSession(string stationId, SessionWorkType workType, object initData)
        {
            if (_sessions.TryGetValue(stationId, out var existing) && existing.IsActive)
            {
                _logger.Warning("Station {StationId} is busy, resetting...", stationId);
                existing.CancelAndDispose();
                _sessions.TryRemove(stationId, out _);
            }

            var state = new SessionState
            {
                WorkType = workType,
                Cts = new CancellationTokenSource(),
                ImageQueue = new BlockingCollection<HObject>()
            };

            // 根据类型初始化内部数据容器
            switch (workType)
            {
                case SessionWorkType.Inspect:
                    var inspectInit = (InspectionInitData)initData;
                    state.InspectData = new InspectionDataContainer(inspectInit);
                    break;
                case SessionWorkType.Calibrate:
                case SessionWorkType.Verify:
                    var calibInit = (CalibrationInitData)initData;
                    state.CalibData = new CalibrationDataContainer(calibInit);
                    break;
            }

            _sessions[stationId] = state;

            // 启动后台处理任务
            state.ProcessTask = Task.Run(() => ProcessLoop(stationId, state, state.Cts.Token));
            _logger.Information("Session started on {StationId}, type={WorkType}", stationId, workType);
        }

        public void EnqueueImage(string stationId, HObject image)
        {
            if (_sessions.TryGetValue(stationId, out var state) && state.IsActive)
            {
                state.ImageQueue.Add(image.Clone());
            }
            else
            {
                _logger.Warning("No active session for {StationId}, discarding image", stationId);
                image?.Dispose();
            }
        }

        public string GetResultData(string stationId, int? startIndex = null, int? endIndex = null)
        {
            if (!_sessions.TryGetValue(stationId, out var state) || !state.IsActive)
            {
                _logger.Warning("No active session for {StationId} when querying results", stationId);
                return GenerateErrorResult(state?.WorkType ?? SessionWorkType.Inspect);
            }

            return state.WorkType switch
            {
                SessionWorkType.Inspect => state.InspectData.BuildResultString(startIndex.Value, endIndex.Value),
                SessionWorkType.Calibrate => state.CalibData.BuildOutput(),
                SessionWorkType.Verify => state.CalibData.BuildOutput(),
                _ => string.Empty
            };
        }

        public void Reset(string stationId)
        {
            if (_sessions.TryRemove(stationId, out var state))
            {
                state.CancelAndDispose();
                _logger.Information("Session on {StationId} reset", stationId);
            }
        }

        private async Task ProcessLoop(string stationId, SessionState state, CancellationToken token)
        {
            try
            {
                foreach (var img in state.ImageQueue.GetConsumingEnumerable(token))
                {
                    try
                    {
                        switch (state.WorkType)
                        {
                            case SessionWorkType.Inspect:
                                await ProcessInspectImage(stationId, state, img);
                                break;
                            case SessionWorkType.Calibrate:
                                await ProcessCalibrationImage(state, img, false);
                                state.ImageQueue.CompleteAdding(); // 标定只需一张图
                                break;
                            case SessionWorkType.Verify:
                                await ProcessCalibrationImage(state, img, true);
                                state.ImageQueue.CompleteAdding();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error processing image on {StationId}", stationId);
                    }
                    finally
                    {
                        img.Dispose();
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _logger.Information("Processing loop ended for {StationId}", stationId);
            }
        }

        private async Task ProcessInspectImage(string stationId, SessionState state, HObject image)
        {
            int poleNum = state.InspectData.GetNextPoleNumber();
            _logger.Debug("Processing inspection pole {PoleNum} on {StationId}", poleNum, stationId);

            HTuple result;
            if (stationId == "A") // 2D
                result = await _visionService.Execute2DInspectAsync(image, poleNum, new InspectionParams());
            else
                result = await _visionService.Execute3DInspectAsync(image, poleNum, new InspectionParams());

            state.InspectData.StoreResult(poleNum, result);
        }

        private async Task ProcessCalibrationImage(SessionState state, HObject image, bool isVerification)
        {
            HTuple result;
            if (isVerification)
                result = await _visionService.ExecuteVerificationAsync(image, new CalibrationParams());
            else
                result = await _visionService.ExecuteCalibrationAsync(image, new CalibrationParams());

            state.CalibData.SetResult(result);
        }

        private string GenerateErrorResult(SessionWorkType type)
        {
            if (type == SessionWorkType.Inspect)
                return string.Join("", Enumerable.Repeat("02" + new string('0', 48), 25)); // 默认容量
            else
                return "02+0000000+0000000";
        }
    }

    // 内部状态类
    internal class SessionState
    {
        public SessionWorkType WorkType { get; set; }
        public CancellationTokenSource Cts { get; set; }
        public Task ProcessTask { get; set; }
        public BlockingCollection<HObject> ImageQueue { get; set; }
        public InspectionDataContainer InspectData { get; set; }
        public CalibrationDataContainer CalibData { get; set; }
        public bool IsActive => Cts != null && !Cts.IsCancellationRequested;

        public void CancelAndDispose()
        {
            Cts?.Cancel();
            ImageQueue?.CompleteAdding();
            ProcessTask?.Wait(TimeSpan.FromSeconds(3));
            Cts?.Dispose();
            ImageQueue?.Dispose();
        }
    }

    // 检测数据容器（模拟旧 inspectResult 数组逻辑）
    internal class InspectionDataContainer
    {
        private readonly int[] _inspectOrder;
        private readonly string[] _results;
        private int _processedCount = 0;
        private readonly object _lock = new();

        public InspectionDataContainer(InspectionInitData initData)
        {
            _inspectOrder = initData.InspectOrder;
            int totalPoles = initData.EndPole; // 假设极柱号最大为 EndPole
            _results = new string[totalPoles + 1]; // 索引即极柱号
            for (int i = 0; i < _results.Length; i++)
                _results[i] = new string('0', 50); // 未检测状态
        }

        public int GetNextPoleNumber()
        {
            lock (_lock)
            {
                if (_processedCount >= _inspectOrder.Length)
                    throw new InvalidOperationException("All poles processed");
                return _inspectOrder[_processedCount++];
            }
        }

        public void StoreResult(int poleNum, HTuple resultArray)
        {
            // 转换为旧协议格式：result(01/02) + 6个8字节测量值
            string result = resultArray[0].D == 0 ? "01" : "02";
            string data = result +
                          DoubleToString(resultArray[2].D, 8) +
                          DoubleToString(resultArray[4].D, 8) +
                          DoubleToString(resultArray[6].D, 8) +
                          DoubleToString(resultArray[8].D, 8) +
                          DoubleToString(resultArray[10].D, 8) +
                          DoubleToString(resultArray[12].D, 8);
            _results[poleNum] = data;
        }

        public string BuildResultString(int startIdx, int endIdx)
        {
            // 等待指定极柱结果就绪，可增加超时逻辑（简化示例直接拼接）
            var sb = new System.Text.StringBuilder();
            for (int i = startIdx; i <= endIdx; i++)
            {
                // 如果未检测完成，这里会返回初始化的全0字符串
                sb.Append(_results[i]);
            }
            return sb.ToString();
        }

        private static string DoubleToString(double value, int len)
        {
            // 与旧代码一致
            int valueInt = (int)Math.Abs(value * 1000);
            string s = valueInt.ToString().PadLeft(len - 1, '0');
            s = (value >= 0 ? "+" : "-") + s;
            return s;
        }
    }

    // 标定数据容器
    internal class CalibrationDataContainer
    {
        public string Result { get; private set; } = "00";
        public string Data { get; private set; } = "+0000000+0000000";

        public CalibrationDataContainer(CalibrationInitData initData) { }

        public void SetResult(HTuple resultArray)
        {
            Result = resultArray[0].D == 1 ? "01" : "02";
            string x = DoubleTo(resultArray[1].D);
            string y = DoubleTo(resultArray[2].D);
            Data = x + y;
        }

        public string BuildOutput() => Result + Data;

        private string DoubleTo(double v)
        {
            int val = (int)Math.Abs(v * 1000);
            return (v >= 0 ? "+" : "-") + val.ToString().PadLeft(7, '0');
        }
    }
}

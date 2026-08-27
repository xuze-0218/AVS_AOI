using AVS_Common.Events;
using AVS_Drivers.Camera;
using AVS_Drivers.Camera.Common.Enum;
using AVS_Service.Models;
using HalconDotNet;
using Newtonsoft.Json;
using Prism.Events;
using Serilog;
using System.Collections.Concurrent;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Formatting = Newtonsoft.Json.Formatting;
using System.Threading;
using System.Linq;
using AVS_Drivers.Camera.Mode;

namespace AVS_Service
{
    public interface ICameraConfigService
    {
        IReadOnlyDictionary<string, ICamera> ConnectedCameras { get; }
        List<CameraSettingModel> AllSettings { get; }
        event Action<string, bool> CameraStatusChanged;
        bool IsCameraGrabbing(string sn);
        event Action<string, bool> CameraGrabbingStatusChanged;
        //event Action<string, HObject> OnImageCaptured;
        void SaveSettings();
        void LoadSettings();
        bool ConnectCamera(string sn, int cameraType, bool applySetting = true);
        void DisconnectCamera(string sn);
        Task InitializeAllCameras();
        ICamera GetCameraInstance(string sn);
        CameraSettingModel GetCameraSetting(string sn);
        CameraSettingModel GetCameraSettingBySnOrIndex(string sn, int index);
        bool ExecuteSoftTrigger(string identifier);
        void SetCameraAcquisitionMode(string sn, AcquisitionMode mode);
        void ApplySettingToDevice(string sn);
        /// <summary>
        /// 更新并保存相机设置，应用到设备
        /// </summary>
        /// <param name="setting"></param>
        void UpdateCameraSetting(CameraSettingModel setting);

        /// <summary>
        /// 启动指定相机的采集（幂等，若已启动则忽略）
        /// </summary>
        void StartCameraGrabbing(string sn);
        void StartCameraGrabbing(string sn, AcquisitionMode? mode);

        /// <summary>
        /// 停止指定相机的采集（保持连接，幂等）
        /// </summary>
        void StopCameraGrabbing(string sn);
        /// <summary>
        /// 启动所有已连接相机的采集（应用配置、设置触发模式并启动）
        /// </summary>
        void StartAllCameras();
        /// <summary>
        /// 停止所有已连接相机的采集（保持连接）
        /// </summary>
        void StopAllCameras();
    }
    public class CameraConfigService : ICameraConfigService
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        public event Action<string, bool> CameraStatusChanged;
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "CameraSettings.json");
        private Dictionary<string, ICamera> _connectedCameras = new Dictionary<string, ICamera>();
        private List<CameraSettingModel> _settingsCache = new List<CameraSettingModel>();
        private Dictionary<string, CameraGrabContext> _grabContexts = new Dictionary<string, CameraGrabContext>();
        private Dictionary<string, Action<IntPtr>> _intensityHandlers = new Dictionary<string, Action<IntPtr>>();
        public IReadOnlyDictionary<string, ICamera> ConnectedCameras => _connectedCameras;
        public List<CameraSettingModel> AllSettings => _settingsCache;

        private HashSet<string> _grabbingCameras = new HashSet<string>();
        public bool IsCameraGrabbing(string sn) => _grabbingCameras.Contains(sn);
        public event Action<string, bool> CameraGrabbingStatusChanged;
        public CameraConfigService(ILogger logger, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            LoadSettings();
        }
        public bool ConnectCamera(string sn, int cameraType, bool applySetting = true)
        {
            if (_connectedCameras.ContainsKey(sn)) return true;
            try
            {
                ICamera camera = CamFactory.CreatCamera((CameraBrand)cameraType);
                if (camera != null && camera.InitDevice(sn))
                {
                    _connectedCameras.Add(sn, camera);
                    var setting = GetCameraSettingBySnOrIndex(sn, -1);
                    setting.CameraType = cameraType;
                    if (applySetting)
                        ApplySettingToDevice(sn);
                    CameraStatusChanged?.Invoke(setting.SerilalNum, true);
                    _logger.Information("相机 {SN} 连接成功（未启动采集）", sn);
                    return true;
                }
            }
            catch (Exception ex) { _logger.Error(ex, "相机 {SN} 连接失败", sn); }
            return false;
        }
        public void DisconnectCamera(string sn)
        {
            _logger.Information("[DisconnectCamera] 开始断开相机 {SN}", sn);
            StopCameraGrabbing(sn); // 先停止采集
            if (_connectedCameras.TryGetValue(sn, out var camera))
            {
                try
                {
                    camera.CloseDevice();
                    _logger.Debug("[DisconnectCamera] 相机设备已关闭 {SN}", sn);
                }
                catch (Exception ex) { _logger.Error(ex, "[DisconnectCamera] 关闭相机设备异常 {SN}", sn); }
                _connectedCameras.Remove(sn);
                CameraStatusChanged?.Invoke(sn, false);
                _logger.Information("[DisconnectCamera] 相机 {SN} 已从连接字典移除", sn);
            }
        }
        public void StartAllCameras()
        {
            foreach (var sn in _connectedCameras.Keys.ToList())
            {
                try
                {
                    ApplySettingToDevice(sn);
                    StartCameraGrabbing(sn);
                    _logger.Information("相机 {SN} 已启动采集", sn);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "启动相机 {SN} 采集失败", sn);
                }
            }
        }
        public void StopAllCameras()
        {
            foreach (var sn in _connectedCameras.Keys.ToList())
            {
                try
                {
                    StopCameraGrabbing(sn);
                    _logger.Information("相机 {SN} 已停止采集", sn);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "停止相机 {SN} 采集失败", sn);
                }
            }
        }
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_configPath))
                    _settingsCache = JsonConvert.DeserializeObject<List<CameraSettingModel>>(File.ReadAllText(_configPath)) ?? new List<CameraSettingModel>();
            }
            catch (Exception ex) { _logger.Error(ex, "加载相机JSON配置失败"); }
        }
        public void SaveSettings()
        {
            try
            {
                string dir = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(_configPath, JsonConvert.SerializeObject(_settingsCache, Formatting.Indented));
            }
            catch (Exception ex) { _logger.Error(ex, "保存相机JSON配置失败"); }
        }
        public async Task InitializeAllCameras()
        {
            await Task.Run(async () =>
            {
                foreach (var setting in _settingsCache)
                {
                    if (string.IsNullOrEmpty(setting.SerilalNum)) continue;

                    try
                    {
                        // 先确保没连上
                        if (_connectedCameras.TryGetValue(setting.SerilalNum, out var existing))
                        {
                            existing.CloseDevice();
                            _connectedCameras.Remove(setting.SerilalNum);
                        }

                        bool initSuccess = false;
                        ICamera camera = null;
                        for (int retry = 0; retry < 3; retry++)
                        {
                            camera = CamFactory.CreatCamera((CameraBrand)setting.CameraType); // 每次都新建
                            if (camera != null && camera.InitDevice(setting.SerilalNum))
                            {
                                _connectedCameras.Add(setting.SerilalNum, camera);
                                CameraStatusChanged?.Invoke(setting.SerilalNum, true);
                                ApplySettingToDevice(setting.SerilalNum);
                                initSuccess = true;
                                break;
                            }
                            _logger.Warning("相机 {SN} 初始化失败，第 {Retry} 次重试", setting.SerilalNum, retry + 1);
                            camera.CloseDevice();
                            await Task.Delay(500);
                        }

                        if (!initSuccess)
                        {
                            _logger.Error("相机 {SN} 初始化失败，已重试3次", setting.SerilalNum);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "相机 {SN} 初始化异常", setting.SerilalNum);
                    }
                }
            });

        }
        /// <summary>
        /// 相机启动，等待信号取图，取图后放入队列
        /// </summary>
        /// <param name="sn"></param>
        public void StartCameraGrabbing(string sn)
        {
            StartCameraGrabbing(sn, null);
        }
        public void StartCameraGrabbing(string sn, AcquisitionMode? mode)
        {
            if (!_connectedCameras.TryGetValue(sn, out var camera)) return;
            if (_grabContexts.ContainsKey(sn)) return; // 已经在运行

            var setting = GetCameraSetting(sn);
            if (setting == null)
            {
                _logger.Warning("相机 {SN} 无配置信息，无法启动采集", sn);
                return;
            }

            //统一设置触发模式（只设置一次）
            bool triggerSetOk = false;
            if (mode.HasValue)
            {
                if (mode.Value == AcquisitionMode.Continuous)
                {
                    // 连续模式：关闭触发
                    triggerSetOk = camera.SetTriggerMode(TriggerMode.Off, TriggerSource.Software);
                }
                else if (mode.Value == AcquisitionMode.SoftTrigger)
                {
                    // 软触发模式：开启触发，触发源为软件
                    triggerSetOk = camera.SetTriggerMode(TriggerMode.On, TriggerSource.Software);
                }
            }
            else
            {
                // 使用本地配置的触发源
                triggerSetOk = camera.SetTriggerMode(TriggerMode.On, setting.TriggerSource);
                if (!triggerSetOk)
                {
                    _logger.Warning("相机 {SN} 设置触发模式({Source})失败，回退为软触发", sn, setting.TriggerSource);
                    triggerSetOk = camera.SetTriggerMode(TriggerMode.On, TriggerSource.Software);
                    if (triggerSetOk)
                        setting.TriggerSource = TriggerSource.Software;
                }
            }
            if (!triggerSetOk)
            {
                _logger.Error("相机 {SN} 触发模式设置失败，放弃启动采集", sn);
                return;
            }
            // 创建抓取上下文
            var ctx = new CameraGrabContext { Cts = new CancellationTokenSource() };
            ctx.GrabCallback = ptr =>
            {
                if (ptr != IntPtr.Zero && !ctx.PtrQueue.IsAddingCompleted)
                {
                    if (ctx.PtrQueue.Count >= 5) ctx.PtrQueue.TryTake(out _);
                    ctx.PtrQueue.Add(ptr);
                }
            };

            _grabContexts[sn] = ctx;
            if (_grabbingCameras.Add(sn))
            {
                CameraGrabbingStatusChanged?.Invoke(sn, true);
            }

            // 处理特殊相机的额外回调（如 Hik3DCamera 的亮度图）
            if (camera is Hik3DCamera hikCamera)
            {
                Action<IntPtr> handler = ptr => { /* 处理亮度图，可暂时为空 */ };
                _intensityHandlers[sn] = handler;
                hikCamera.IntensityImageReceived += handler;
            }

            // 启动后台处理任务
            ctx.ProcessingTask = Task.Run(() =>
            {
                try
                {
                    foreach (var ptr in ctx.PtrQueue.GetConsumingEnumerable(ctx.Cts.Token))
                    {
                        ProcessImagePointer(camera, ptr);
                    }
                }
                catch (OperationCanceledException) { }
            }, ctx.Cts.Token);

            bool startOk = camera.StartGrabbing(ctx.GrabCallback);
            if (!startOk)
            {
                _logger.Error("相机 {SN} 启动采集失败", sn);
                return;
            }

            _logger.Information("相机 {SN} 采集启动成功", sn);
        }
        public void StopCameraGrabbing(string sn)
        {
            _logger.Information("[StopCameraGrabbing] 停止相机 {SN} 采集", sn);
            if (_grabContexts.TryGetValue(sn, out var ctx))
            {
                try
                {
                    ctx.Cts?.Cancel();
                    ctx.PtrQueue?.CompleteAdding();
                    if (ctx.ProcessingTask != null && !ctx.ProcessingTask.IsCompleted)
                    {
                        bool finished = ctx.ProcessingTask.Wait(TimeSpan.FromSeconds(3));
                        if (!finished)
                            _logger.Warning("[StopCameraGrabbing] 处理任务未在3秒内结束 {SN}", sn);
                    }
                    if (_connectedCameras.TryGetValue(sn, out var camera))
                    {
                        // 调用新的 StopGrabbing 方法，它会移除回调并调用核心停止逻辑
                        camera.StopGrabbing(ctx.GrabCallback);
                    }
                    _grabContexts.Remove(sn);
                    if (_grabbingCameras.Remove(sn))
                    {
                        CameraGrabbingStatusChanged?.Invoke(sn, false);
                    }
                }
                catch (Exception ex) { _logger.Error(ex, "[StopCameraGrabbing] 停止抓取上下文异常 {SN}", sn); }
            }

            // 若相机支持移除回调，这里可以移除（比如 Hik3DCamera 的 IntensityImageReceived）
            if (_connectedCameras.TryGetValue(sn, out var camera2))
            {
                if (camera2 is Hik3DCamera hikCamera)
                {
                    if (_intensityHandlers.TryGetValue(sn, out var handler))
                    {
                        hikCamera.IntensityImageReceived -= handler;
                        _intensityHandlers.Remove(sn);
                    }
                }
                //不调用 CloseDevice，保持连接
            }
        }
        /// <summary>
        /// 处理图像指针，转为HObject格式
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="ptr"></param>
        private void ProcessImagePointer(ICamera camera, IntPtr ptr)
        {
            var info = camera.ImageInfo;
            HObject img = null;
            try
            {
                if (info.PixelFormat == CamPixelFormat.Mono8)
                    img = ConvertToImage8(ptr, info.Width, info.Height);
                else if (info.PixelFormat == CamPixelFormat.Rgb8)
                    img = ConvertToImage24(ptr, info.Width, info.Height);
                else if (info.PixelFormat == CamPixelFormat.Depth)
                    img = ConvertToImageDepth(ptr, info.Width, info.Height);
                if (img != null && img.IsInitialized())
                {
                    _logger.Information("相机 {SN} 回调产生图像，准备发布", camera.SN);
                    _eventAggregator.GetEvent<HImageDisplayEvent>().Publish(new CameraImagePayload()
                    {
                        CameraSN = camera.SN,
                        Image = img
                    });
                }
            }
            catch (Exception ex) { _logger.Error(ex, "图像解析失败"); img?.Dispose(); }
            finally
            {
                img.Dispose();
                img = null;
            }
        }
        private HObject ConvertToImageDepth(IntPtr pImageBuf, int nWidth, int nHeight)
        {
            HOperatorSet.GenImage1(out HObject image, "int2", nWidth, nHeight, pImageBuf);
            return image;
        }
        public void SetCameraAcquisitionMode(string sn, AcquisitionMode mode)
        {
            if (!_connectedCameras.TryGetValue(sn, out var camera)) return;

            try
            {
                if (mode == AcquisitionMode.Continuous)
                {
                    camera.SetTriggerMode(TriggerMode.Off, TriggerSource.Software);
                }
                else if (mode == AcquisitionMode.SoftTrigger)
                {
                    camera.SetTriggerMode(TriggerMode.On, TriggerSource.Software);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "切换相机 {SN} 采集模式失败", sn);
            }
        }
        public ICamera GetCameraInstance(string sn) => _connectedCameras.TryGetValue(sn, out var cam) ? cam : null;
        public CameraSettingModel GetCameraSetting(string sn) => _settingsCache.FirstOrDefault(x => x.SerilalNum == sn);
        public bool ExecuteSoftTrigger(string identifier)
        {

            var camera = GetCameraInstance(identifier);
            if (camera == null)
            {
                var setting = _settingsCache.FirstOrDefault(x => x.CamSelectIndex.ToString() == identifier);
                if (setting != null) camera = GetCameraInstance(setting.SerilalNum);
            }

            if (camera != null)
            {
                return camera.SoftTrigger();
            }


            _logger.Warning("触发失败：未找到相机实例 {ID}", identifier);
            return false;
        }
        public void ApplySettingToDevice(string sn)
        {
            var setting = GetCameraSetting(sn);
            var camera = GetCameraInstance(sn);
            if (setting != null && camera != null)
            {
                camera.SetExpouseTime((ushort)setting.ExposureTime);
                camera.SetGain(setting.Gain);
            }
        }
        public void UpdateCameraSetting(CameraSettingModel setting)
        {
            var existing = _settingsCache.FirstOrDefault(x =>
                (!string.IsNullOrEmpty(setting.SerilalNum) && x.SerilalNum == setting.SerilalNum) ||
                (x.CamSelectIndex == setting.CamSelectIndex && string.IsNullOrEmpty(x.SerilalNum)));

            if (existing != null && existing != setting)
            {
                existing.CameraType = setting.CameraType;
                existing.ExposureTime = setting.ExposureTime;
                existing.Gain = setting.Gain;
                existing.SerilalNum = setting.SerilalNum;
                existing.CameraRole = setting.CameraRole;
                existing.TriggerMode = setting.TriggerMode;
                existing.TriggerSource = setting.TriggerSource;
            }
            SaveSettings();

            if (!string.IsNullOrEmpty(setting.SerilalNum) && _connectedCameras.TryGetValue(setting.SerilalNum, out var cam))
            {
                cam.SetExpouseTime((ushort)setting.ExposureTime);
                cam.SetGain(setting.Gain);
            }
        }
        public CameraSettingModel GetCameraSettingBySnOrIndex(string sn, int index)
        {
            CameraSettingModel setting = null;
            if (!string.IsNullOrEmpty(sn))
            {
                setting = _settingsCache.FirstOrDefault(x => x.SerilalNum == sn);
            }
            if (setting == null && index >= 0)
            {
                setting = _settingsCache.FirstOrDefault(x => x.CamSelectIndex == index);
            }

            // 如果还是没有，新建一个
            if (setting == null)
            {
                setting = new CameraSettingModel
                {
                    SerilalNum = sn,
                    // 如果 index 无效，自动分配一个最大的 index
                    CamSelectIndex = index >= 0 ? index : (_settingsCache.Count > 0 ? _settingsCache.Max(x => x.CamSelectIndex) + 1 : 0)
                };
                _settingsCache.Add(setting);
            }
            else
            {
                if (string.IsNullOrEmpty(setting.SerilalNum) && !string.IsNullOrEmpty(sn))
                {
                    setting.SerilalNum = sn;
                }
            }

            return setting;
        }
        private HObject ConvertToImage8(IntPtr pImageBuf, int nWidth, int nHeight)
        {
            HOperatorSet.GenImage1(out HObject image, "byte", nWidth, nHeight, pImageBuf);
            return image;
        }
        private HObject ConvertToImage24(IntPtr pImageBuf, int nWidth, int nHeight)
        {

            HObject colorImage;
            //HOperatorSet.GenImageInterleaved(out colorImage, pImageBuf, "bgr", nWidth, nHeight, 0, "byte", 0, 0, 0, 0, -1, 0);
            //HOperatorSet.GenImageInterleaved(out colorImage, pImageBuf, "rgb", nWidth, nHeight, 0, "byte", 0, 0, 0, 0, -1, 0);
            HOperatorSet.GenImageInterleaved(out colorImage, pImageBuf, "rgb", nWidth, nHeight, -1, "byte", 0, 0, 0, 0, -1, 0);
            return colorImage;
        }
    }

    //管理每台相机的取图队列和任务
    public class CameraGrabContext
    {
        public BlockingCollection<IntPtr> PtrQueue = new BlockingCollection<IntPtr>(5);
        public CancellationTokenSource Cts;
        public Task ProcessingTask;
        public Action<IntPtr> GrabCallback;
    }
}

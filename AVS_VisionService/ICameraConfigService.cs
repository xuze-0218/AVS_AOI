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
                // 初始化失败：销毁相机实例，避免其残留在 CamFactory 静态 CameraList 中
                CamFactory.DestroyCamera(camera);
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
                    CamFactory.DestroyCamera(camera); // 从静态 CameraList 移除并关闭设备
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
                            CamFactory.DestroyCamera(existing);
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
                            CamFactory.DestroyCamera(camera); // 失败实例从静态列表移除，避免重试堆叠泄漏
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
                triggerSetOk = camera.SetTriggerMode(setting.TriggerMode, setting.TriggerSource);
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
                if (ptr == IntPtr.Zero || ctx.PtrQueue.IsAddingCompleted)
                    return;

                // P0-1：在 SDK 回调线程内同步拷贝，此时 ptr 仍有效（pin 未释放），
                // 拷贝完成后即与 SDK 缓冲区解耦，避免指针悬垂/释放后使用。
                var info = camera.ImageInfo; // 快照宽高与像素格式
                HObject tempImage = null;
                HObject imageCopy = null;
                try
                {
                    if (info.PixelFormat == CamPixelFormat.Mono8)
                    {
                        HOperatorSet.GenImage1(out tempImage, "byte", info.Width, info.Height, ptr);
                    }
                    else if (info.PixelFormat == CamPixelFormat.Rgb8)
                    {
                        HOperatorSet.GenImageInterleaved(out tempImage, ptr, "rgb", info.Width, info.Height, -1, "byte", 0, 0, 0, 0, -1, 0);
                    }
                    else if (info.PixelFormat == CamPixelFormat.Depth)
                    {
                        HOperatorSet.GenImage1(out tempImage, "int2", info.Width, info.Height, ptr);
                    }

                    if (tempImage != null && tempImage.IsInitialized())
                    {
                        HOperatorSet.CopyImage(tempImage, out imageCopy); // 独立副本

                        // 队列满时丢弃最旧帧，防止队列阻塞/内存无界增长
                        if (ctx.PtrQueue.Count >= 5 && ctx.PtrQueue.TryTake(out var dropped))
                            dropped?.Dispose();
                        if (!ctx.PtrQueue.TryAdd(imageCopy))
                        {
                            imageCopy?.Dispose(); // 极端竞争下仍满，丢弃新帧
                        }
                        imageCopy = null; // 所有权已移交队列，避免 finally 二次释放
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "相机 {SN} 回调图像拷贝失败", sn);
                }
                finally
                {
                    tempImage?.Dispose();
                    imageCopy?.Dispose();
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
                    foreach (var image in ctx.PtrQueue.GetConsumingEnumerable(ctx.Cts.Token))
                    {
                        ProcessImagePointer(camera, image);
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
                    // P0-1：清理队列中残留的 HObject，避免停止时非托管图像资源泄漏
                    if (ctx.PtrQueue != null)
                    {
                        while (ctx.PtrQueue.TryTake(out var leftover))
                            leftover?.Dispose();
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
        /// 处理图像（HObject 副本），发布事件后释放。
        /// 注意：发布后即 Dispose，订阅者必须在 PublisherThread 上同步 Clone 后再使用。
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="img"></param>
        private void ProcessImagePointer(ICamera camera, HObject img)
        {
            if (img == null || !img.IsInitialized())
            {
                img?.Dispose();
                return;
            }

            try
            {
                _logger.Information("相机 {SN} 回调产生图像，准备发布", camera.SN);
                _eventAggregator.GetEvent<HImageDisplayEvent>().Publish(new CameraImagePayload()
                {
                    CameraSN = camera.SN,
                    Image = img
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "图像发布失败");
            }
            finally
            {
                img?.Dispose();
                img = null;
            }
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
    }

    //管理每台相机的取图队列和任务
    public class CameraGrabContext
    {
        public BlockingCollection<HObject> PtrQueue = new BlockingCollection<HObject>(5);
        public CancellationTokenSource Cts;
        public Task ProcessingTask;
        public Action<IntPtr> GrabCallback;
    }
}

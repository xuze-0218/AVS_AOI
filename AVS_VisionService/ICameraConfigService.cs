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
using Formatting = Newtonsoft.Json.Formatting;

namespace AVS_Service
{
    public interface ICameraConfigService
    {
        IReadOnlyDictionary<string, ICamera> ConnectedCameras { get; }
        List<CameraSettingModel> AllSettings { get; }
        event Action<string, bool> CameraStatusChanged;
        //event Action<string, HObject> OnImageCaptured;

        void SaveSettings();
        void LoadSettings();
        bool ConnectAndStartCamera(string sn, int cameraType);
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

        public IReadOnlyDictionary<string, ICamera> ConnectedCameras => _connectedCameras;
        public List<CameraSettingModel> AllSettings => _settingsCache;
        public CameraConfigService(ILogger logger, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            LoadSettings();
        }

        public bool ConnectAndStartCamera(string sn, int cameraType)
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

                    ApplySettingToDevice(sn);
                    StartCameraGrabbing(sn);
                    CameraStatusChanged?.Invoke(setting.SerilalNum, true);
                    _logger.Information("调试界面接入新相机 {SN}，初始化并启动取图成功", sn);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "调试界面接入新相机 {SN} 失败", sn);
            }
            return false;
        }

        public void DisconnectCamera(string sn)
        {
            _logger.Information("[DisconnectCamera] 开始断开相机 {SN}", sn);
            if (_grabContexts.TryGetValue(sn, out var ctx))
            {
                _logger.Debug("[DisconnectCamera] 相机 {SN} 存在抓取上下文，准备停止", sn);
                try
                {
                    _logger.Debug("[DisconnectCamera] 取消令牌 {SN}", sn);
                    ctx.Cts?.Cancel();
                    _logger.Debug("[DisconnectCamera] 关闭队列 {SN}", sn);
                    ctx.PtrQueue?.CompleteAdding();

                    if (ctx.ProcessingTask != null && !ctx.ProcessingTask.IsCompleted)
                    {
                        _logger.Debug("[DisconnectCamera] 等待处理任务结束 {SN}（最多3秒）", sn);
                        bool finished = ctx.ProcessingTask.Wait(TimeSpan.FromSeconds(3));
                        if (!finished)
                            _logger.Warning("[DisconnectCamera] 处理任务未在3秒内结束，继续关闭 {SN}", sn);
                        else
                            _logger.Debug("[DisconnectCamera] 处理任务已结束 {SN}", sn);
                    }

                    _grabContexts.Remove(sn);
                    _logger.Debug("[DisconnectCamera] 抓取上下文已移除 {SN}", sn);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "[DisconnectCamera] 停止抓取上下文异常 {SN}", sn);
                }
            }
            else
                _logger.Debug("[DisconnectCamera] 相机 {SN} 没有抓取上下文", sn);
            if (_connectedCameras.TryGetValue(sn, out var camera))
            {
                _logger.Debug("[DisconnectCamera] 关闭相机设备 {SN} ...", sn);
                try
                {
                    camera.CloseDevice();
                    _logger.Debug("[DisconnectCamera] 相机设备已关闭 {SN}", sn);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "[DisconnectCamera] 关闭相机设备异常 {SN}", sn);
                }
                _connectedCameras.Remove(sn);
                CameraStatusChanged?.Invoke(sn, false);
                _logger.Information("[DisconnectCamera] 相机 {SN} 已从连接字典移除", sn);
            }
            else
                _logger.Warning("[DisconnectCamera] 相机 {SN} 不在连接字典中", sn);
            _logger.Information("[DisconnectCamera] 相机 {SN} 断开流程结束", sn);
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
                             _connectedCameras.Remove(setting.SerilalNum, out _);
                         }

                         bool initSuccess = false;
                         ICamera camera = null;
                         for (int retry = 0; retry < 3; retry++)
                         {
                             camera = CamFactory.CreatCamera((CameraBrand)setting.CameraType); // 每次都新建
                             if (camera != null && camera.InitDevice(setting.SerilalNum))
                             {
                                 _connectedCameras.TryAdd(setting.SerilalNum, camera);
                                 CameraStatusChanged?.Invoke(setting.SerilalNum, true);
                                 ApplySettingToDevice(setting.SerilalNum);
                                 StartCameraGrabbing(setting.SerilalNum);
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

                         //_connectedCameras.TryAdd(setting.SerilalNum, camera);
                         //CameraStatusChanged?.Invoke(setting.SerilalNum, true);
                         //ApplySettingToDevice(setting.SerilalNum);
                         //StartCameraGrabbing(setting.SerilalNum);
                         //_logger.Information("相机 {SN} 初始化成功", setting.SerilalNum);
                     }
                     catch (Exception ex)
                     {
                         _logger.Error(ex, "相机 {SN} 初始化异常", setting.SerilalNum);
                     }

                     await Task.Delay(200); //相机之间的间隔
                 }
             });

        }

        /// <summary>
        /// 相机启动，等待信号取图，取图后放入队列
        /// </summary>
        /// <param name="sn"></param>
        private void StartCameraGrabbing(string sn)
        {
            if (!_connectedCameras.TryGetValue(sn, out var camera)) return;

            if (_grabContexts.ContainsKey(sn)) return; // 已经在运行
            var setting = GetCameraSetting(sn);

            bool triggerSetOk = camera.SetTriggerMode(TriggerMode.On, setting.TriggerSource);
            if (!triggerSetOk)
            {
                _logger.Warning("相机 {SN} 设置触发模式({Source})失败，回退为软触发", sn, setting.TriggerSource);
                bool fallbackOk = camera.SetTriggerMode(TriggerMode.On, TriggerSource.Software);
                if (!fallbackOk)
                {
                    _logger.Error("相机 {SN} 回退软触发也失败，放弃启动采集", sn);
                    return;
                }
                // 回退成功，更新内存中的触发源，保证后续逻辑一致
                setting.TriggerSource = TriggerSource.Software;
            }

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

            if (setting.TriggerSource == TriggerSource.Software)
            {
                camera.StartWith_SoftTriggerModel_SetCallback(ctx.GrabCallback);
                _logger.Information("相机 {SN} 软触发模式启动", sn);
            }
            else
            {
                camera.StartWith_HardTriggerModel_SetCallback(setting.TriggerSource, ctx.GrabCallback);
                _logger.Information("相机 {SN} 硬触发模式启动，触发源: {Source}", sn, setting.TriggerSource);
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
                if (img != null && img.IsInitialized())
                {
                    _eventAggregator.GetEvent<HImageDisplayEvent>().Publish(new CameraImagePayload()
                    {
                        CameraSN = camera.SN,
                        Image = img.Clone()//发布副本，避免Halcon对象被Dispose后引用失效
                    });
                }
            }
            catch (Exception ex) { _logger.Error(ex, "图像解析失败"); }
            finally { img?.Dispose(); }
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
                existing.imgpath = setting.imgpath;
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
            // 如果提供了有效的 SN，优先按 SN 查找
            if (!string.IsNullOrEmpty(sn))
            {
                setting = _settingsCache.FirstOrDefault(x => x.SerilalNum == sn);
            }
            // 如果没找到，且传入了有效的 Index (>= 0)，尝试按 Index 查找
            // （这通常发生在新连上一个相机，已知下拉框索引，但配置文件里还没记录它 SN 的时候）
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
                // 如果刚才通过 Index 找到了对象，但是该对象里还没有记录 SN，
                // 且当前方法传入了真实的 SN，就顺手把 SN 更新上去，完成“占位符”到“实体”的绑定。
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

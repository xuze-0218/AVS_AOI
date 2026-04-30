using AVS_Drivers.Camera;
using AVS_Drivers.Camera.Common.Enum;
using AVS_VisionService.Model;
using HalconDotNet;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace AVS_VisionService
{
    public interface ICameraConfigService
    {
        IReadOnlyDictionary<string, ICamera> ConnectedCameras { get; }
        List<CameraSettingModel> AllSettings { get; }
        event Action<string, HObject> OnImageCaptured;

        void SaveSettings();
        void LoadSettings();
        void InitializeAllCameras();

        ICamera GetCameraInstance(string sn);
        CameraSettingModel GetCameraSetting(string sn);
        CameraSettingModel GetCameraSettingBySnOrIndex(string sn, int index);

        bool ExecuteSoftTrigger(string identifier);
        void SetCameraAcquisitionMode(string sn, AcquisitionMode mode);

        void ApplySettingToDevice(string sn);
        void UpdateCameraSetting(CameraSettingModel setting);
        void RaiseImageCaptured(string cameraKey, HObject image);

    }


    public class CameraConfigService : ICameraConfigService
    {
        private readonly ILogger _logger;
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "CameraSettings.json");
        private Dictionary<string, ICamera> _connectedCameras = new Dictionary<string, ICamera>();
        private List<CameraSettingModel> _settingsCache = new List<CameraSettingModel>();



        private Dictionary<string, CameraGrabContext> _grabContexts = new Dictionary<string, CameraGrabContext>();
        public IReadOnlyDictionary<string, ICamera> ConnectedCameras => _connectedCameras;
        public List<CameraSettingModel> AllSettings => _settingsCache;

        public event Action<string, HObject> OnImageCaptured;

        public CameraConfigService(ILogger logger)
        {
            _logger = logger;
            LoadSettings();
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


        public void InitializeAllCameras()
        {
            foreach (var setting in _settingsCache)
            {
                if (string.IsNullOrEmpty(setting.SerilalNum)) continue;

                try
                {
                    ICamera camera = CamFactory.CreatCamera((CameraBrand)setting.CameraType);

                    if (camera != null && camera.InitDevice(setting.SerilalNum))
                    {
                        if (!_connectedCameras.ContainsKey(setting.SerilalNum))
                        {
                            _connectedCameras.Add(setting.SerilalNum, camera);
                            ApplySettingToDevice(setting.SerilalNum);
                            StartCameraGrabbing(setting.SerilalNum);

                            _logger.Information("相机 {SN} (索引:{Index}) 初始化并启动取图成功", setting.SerilalNum, setting.CamSelectIndex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "相机 {SN} 初始化失败", setting.SerilalNum);
                }
            }
        }



        private void StartCameraGrabbing(string sn)
        {
            if (!_connectedCameras.TryGetValue(sn, out var camera)) return;

            if (_grabContexts.ContainsKey(sn)) return; // 已经在运行

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

            camera.StartWith_SoftTriggerModel_SetCallback(ctx.GrabCallback);

        }

        /// <summary>
        /// 处理图像指针，转为ICogImage格式
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="ptr"></param>
        private void ProcessImagePointer(ICamera camera, IntPtr ptr)
        {
            var info = camera.ImageInfo;
            HObject img = new HObject();
            try
            {
                if (info.PixelFormat == CamPixelFormat.Mono8)
                    img = ConvertToImage8(ptr, info.Width, info.Height);
                else if (info.PixelFormat == CamPixelFormat.Rgb8)
                    img = ConvertToImage24(ptr, info.Width, info.Height);
                if (img != null)
                    RaiseImageCaptured(camera.SN, img);
            }
            catch (Exception ex) { _logger.Error(ex, "图像解析失败"); }
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

        public void RaiseImageCaptured(string cameraKey, HObject image)
        {
            OnImageCaptured?.Invoke(cameraKey, image);
        }

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
            var setting = _settingsCache.FirstOrDefault(x => (!string.IsNullOrEmpty(sn) && x.SerilalNum == sn));
            if (setting == null)
                setting = _settingsCache.FirstOrDefault(x => x.CamSelectIndex == index && string.IsNullOrEmpty(x.SerilalNum));

            if (setting == null)
            {
                setting = new CameraSettingModel { SerilalNum = sn, CamSelectIndex = index };
                _settingsCache.Add(setting);
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
            HOperatorSet.GenImageInterleaved(out colorImage, pImageBuf, "bgr", nWidth, nHeight, 0, "byte", 0, 0, 0, 0, -1, 0);
            //HOperatorSet.GenImageInterleaved(out colorImage, pImageBuf, "rgb", nWidth, nHeight, 0, "byte", 0, 0, 0, 0, -1, 0);
            //HOperatorSet.GenImageInterleaved(out colorImage, pImageBuf, "rgb", nWidth, nHeight, -1, "byte", 0, 0, 0, 0, -1, 0);
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

using HalconDotNet;
using MMDeploy;
using OpenCvSharp;
using Serilog;
using System.Runtime.InteropServices;

namespace AVS_Core.Services
{
    /// <summary>
    /// AI推理服务接口
    /// </summary>
    public interface IAiDriveService : IDisposable
    {
        /// <summary>
        /// 加载分割模型
        /// </summary>
        bool LoadSegModel(string modelId, string[] modelPaths);

        /// <summary>
        /// 加载检测模型
        /// </summary>
        bool LoadDetModel(string modelId, string[] modelPaths);

        /// <summary>
        /// 图像分割推理
        /// </summary>
        void Predict(string modelId, int modelIndex, HObject imgGray, out HObject imgMask);

        /// <summary>
        /// 图像目标检测（单目标）
        /// </summary>
        void Detect(string modelId, int modelIndex, HObject imgGray, double scoreThreshold, out int targetLabel, out HTuple targetRect);

        /// <summary>
        /// 图像目标检测（多目标，最多2个）
        /// </summary>
        void DetectMulti(string modelId, int modelIndex, HObject imgGray, double scoreThreshold, out int[] targetLabels, out HTuple targetRect);

        /// <summary>
        /// 释放指定工位的所有模型
        /// </summary>
        void UnloadStation(string modelId);

        bool IsModelLoaded(string modelId);
    }

    /// <summary>
    /// AI推理驱动服务，封装MMDeploy推理引擎的加载与调用。
    /// 支持多工位、多模型管理，通过依赖注入使用。
    /// </summary>
    public class AiDriveService : IAiDriveService
    {
        private readonly string _deviceName;
        private readonly int _deviceId;
        private readonly ILogger _logger;

        private readonly Dictionary<string, List<Segmentor>> _segHandles = new();
        private readonly Dictionary<string, List<Detector>> _detHandles = new();
        private readonly object _lock = new();

        private bool _disposed;

        public AiDriveService(string deviceName ="cpu" /*"cuda"*/, int deviceId = 0, ILogger logger = null)
        {
            _deviceName = deviceName;
            _deviceId = deviceId;
            _logger = logger;
        }

        // ========== 模型加载 ==========

        public bool LoadSegModel(string modelId, string[] modelPaths)
        {
            if (string.IsNullOrEmpty(modelId) || modelPaths == null || modelPaths.Length == 0)
            {
                _logger?.Warning("[AiDrive] LoadSegModel: stationId 或 modelPaths 无效");
                return false;
            }         
            try
            {
                lock (_lock)
                {
                  
                    if (_segHandles.ContainsKey(modelId))
                    {
                        _logger?.Information("[AiDrive] 工位 {StationId} 分割模型已加载，跳过", modelId);
                        return true;
                    }
                    // 释放旧模型
                    DisposeHandles(_segHandles, modelId);
                    var handles = new List<Segmentor>();
                    foreach (var path in modelPaths)
                    {
                        handles.Add(new Segmentor(path, _deviceName, _deviceId));
                    }
                    _segHandles[modelId] = handles;
                }
                _logger?.Information("[AiDrive] 工位 {StationId} 加载 {Count} 个分割模型完成", modelId, modelPaths.Length);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "[AiDrive] 工位 {StationId} 加载分割模型失败", modelId);
                return false;
            }
        }

        public bool LoadDetModel(string modelId, string[] modelPaths)
        {
            if (string.IsNullOrEmpty(modelId) || modelPaths == null || modelPaths.Length == 0)
            {
                _logger?.Warning("[AiDrive] LoadDetModel: stationId 或 modelPaths 无效");
                return false;
            }

            try
            {
                lock (_lock)
                {
                    if (_detHandles.ContainsKey(modelId))
                    {
                        _logger?.Information("[AiDrive] 工位 {StationId} 检测模型已加载，跳过", modelId);
                        return true;
                    }
                    DisposeHandles(_detHandles, modelId);
                    var handles = new List<Detector>();
                    foreach (var path in modelPaths)
                    {
                        handles.Add(new Detector(path, _deviceName, _deviceId));
                    }
                    _detHandles[modelId] = handles;
                }
                _logger?.Information("[AiDrive] 工位 {StationId} 加载 {Count} 个检测模型完成", modelId, modelPaths.Length);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "[AiDrive] 工位 {StationId} 加载检测模型失败", modelId);
                return false;
            }
        }

        // ========== 推理接口 ==========

        public void Predict(string modelId, int modelIndex, HObject imgGray, out HObject imgMask)
        {
            HOperatorSet.GenEmptyObj(out imgMask);

            if (!TryGetHandle(_segHandles, modelId, modelIndex, out var segmentor))
                return;

            try
            {
                Halcon2MmMat(imgGray, out var mats);
                var output = segmentor.Apply(mats);
                ResultToColorMask(output[0], out OpenCvSharp.Mat colorMask);
                Mat2HalconRgb(colorMask, out imgMask);
                colorMask.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "[AiDrive] 工位 {StationId} 分割推理失败 (modelIndex={Index})", modelId, modelIndex);
                HOperatorSet.GenEmptyObj(out imgMask);
            }
        }

        public void Detect(string modelId, int modelIndex, HObject imgGray, double scoreThreshold, out int targetLabel, out HTuple targetRect)
        {
            DetectInternal(modelId, modelIndex, imgGray, scoreThreshold, multiTarget: false, out var labels, out targetRect);
            targetLabel = labels.Length > 0 ? labels[0] : -1;
        }

        public void DetectMulti(string modelId, int modelIndex, HObject imgGray, double scoreThreshold, out int[] targetLabels, out HTuple targetRect)
        {
            DetectInternal(modelId, modelIndex, imgGray, scoreThreshold, multiTarget: true, out targetLabels, out targetRect);
        }

        private void DetectInternal(string modelId, int modelIndex, HObject imgGray, double scoreThreshold, bool multiTarget,
            out int[] targetLabels, out HTuple targetRect)
        {
            targetRect = new HTuple();
            targetLabels = multiTarget ? new int[2] { -1, -1 } : new int[1] { -1 };

            if (!TryGetHandle(_detHandles, modelId, modelIndex, out var detector))
                return;

            try
            {
                Halcon2MmMat(imgGray, out var mats);
                var output = detector.Apply(mats);

                if (output == null || output.Count == 0 || output[0].Results == null)
                {
                    _logger?.Debug("[AiDrive] 工位 {StationId} 检测输出为空", modelId);
                    return;
                }

                int idx = 0;
                int maxCount = targetLabels.Length;
                foreach (var obj in output[0].Results)
                {
                    if (obj.Score < scoreThreshold)
                        continue;

                    targetLabels[idx] = obj.LabelId;

                    float x1 = Math.Max((float)Math.Floor(obj.BBox.Top) - 1, 0f);
                    float y1 = Math.Max((float)Math.Floor(obj.BBox.Left) - 1, 0f);
                    float x2 = Math.Max((float)Math.Floor(obj.BBox.Bottom) - 1, 0f);
                    float y2 = Math.Max((float)Math.Floor(obj.BBox.Right) - 1, 0f);

                    HOperatorSet.TupleConcat(targetRect, x1, out targetRect);
                    HOperatorSet.TupleConcat(targetRect, y1, out targetRect);
                    HOperatorSet.TupleConcat(targetRect, x2, out targetRect);
                    HOperatorSet.TupleConcat(targetRect, y2, out targetRect);

                    idx++;
                    if (idx >= maxCount)
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "[AiDrive] 工位 {StationId} 检测推理失败 (modelIndex={Index})", modelId, modelIndex);
                targetLabels = multiTarget ? new int[2] { -1, -1 } : new int[1] { -1 };
                targetRect = new HTuple();
            }
        }


        public bool IsModelLoaded(string modelId)
        {
            lock (_lock)
            {
                return _detHandles.ContainsKey(modelId) || _segHandles.ContainsKey(modelId);
            }
        }

        // ========== 资源管理 ==========

        public void UnloadStation(string modelId)
        {
            lock (_lock)
            {
                DisposeHandles(_segHandles, modelId);
                DisposeHandles(_detHandles, modelId);
            }
            _logger?.Information("[AiDrive] 工位 {StationId} 模型已卸载", modelId);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            lock (_lock)
            {
                foreach (var kv in _segHandles)
                    DisposeHandles(_segHandles, kv.Key);
                foreach (var kv in _detHandles)
                    DisposeHandles(_detHandles, kv.Key);
                _segHandles.Clear();
                _detHandles.Clear();
            }
            _logger?.Information("[AiDrive] AiDriveService 已释放所有资源");
        }

        // ========== 内部辅助方法 ==========

        private static void DisposeHandles<T>(Dictionary<string, List<T>> storage, string stationId) where T : IDisposable
        {
            if (storage.TryGetValue(stationId, out var handles))
            {
                foreach (var h in handles)
                    h.Dispose();
                storage.Remove(stationId);
            }
        }

        private bool TryGetHandle<T>(Dictionary<string, List<T>> storage, string stationId, int index, out T handle) where T : class
        {
            handle = null;
            if (!storage.TryGetValue(stationId, out var handles))
            {
                _logger?.Warning("[AiDrive] 工位 {StationId} {Type} 模型未加载", stationId, typeof(T).Name);
                return false;
            }
            if (index < 0 || index >= handles.Count)
            {
                _logger?.Error("[AiDrive] 工位 {StationId} {Type} 模型索引 {Index} 越界 (总数={Count})",
                    stationId, typeof(T).Name, index, handles.Count);
                return false;
            }
            handle = handles[index];
            return true;
        }

        // ========== 图像格式转换 ==========

        private static void Halcon2MmMat(HObject imgGray, out MMDeploy.Mat[] mats)
        {
            HOperatorSet.CountChannels(imgGray, out HTuple chs);
            if ((int)chs.D == 1)
            {
                mats = new MMDeploy.Mat[1];
                unsafe
                {
                    HOperatorSet.GetImagePointer1(imgGray, out HTuple ptrGray, out HTuple type, out HTuple width, out HTuple height);
                    IntPtr ptr2 = ptrGray;
                    int bytes = width * height;
                    byte[] rgbvalues = new byte[bytes];
                    Marshal.Copy(ptr2, rgbvalues, 0, bytes);

                    OpenCvSharp.Mat mat = new OpenCvSharp.Mat();
                    mat.Create(height, width, MatType.CV_8UC1);
                    Marshal.Copy(rgbvalues, 0, mat.Data, rgbvalues.Length);

                    mats[0].Data = mat.DataPointer;
                    mats[0].Height = mat.Height;
                    mats[0].Width = mat.Width;
                    mats[0].Channel = mat.Dims;
                    mats[0].Format = PixelFormat.Grayscale;
                    mats[0].Type = DataType.Int8;
                    mats[0].Device = null;
                }
            }
            else if ((int)chs.D == 3)
            {
                HOperatorSet.GetImagePointer3(imgGray, out HTuple ptrRed, out HTuple ptrGreen, out HTuple ptrBlue,
                    out HTuple type, out HTuple width, out HTuple height);
                int bytes = width * height * 3;
                byte[] rgbvalues = new byte[bytes];

                unsafe
                {
                    byte* r = (byte*)(IntPtr)ptrRed;
                    byte* g = (byte*)(IntPtr)ptrGreen;
                    byte* b = (byte*)(IntPtr)ptrBlue;
                    int length = width * height;
                    for (int i = 0; i < length; i++)
                    {
                        rgbvalues[i * 3 + 0] = b[i];
                        rgbvalues[i * 3 + 1] = g[i];
                        rgbvalues[i * 3 + 2] = r[i];
                    }
                }

                OpenCvSharp.Mat mat = new OpenCvSharp.Mat();
                mat.Create(height, width, MatType.CV_8UC3);
                Marshal.Copy(rgbvalues, 0, mat.Data, rgbvalues.Length);

                mats = new MMDeploy.Mat[1];
                unsafe
                {
                    mats[0].Data = mat.DataPointer;
                    mats[0].Height = mat.Height;
                    mats[0].Width = mat.Width;
                    mats[0].Channel = mat.Dims;
                    mats[0].Format = PixelFormat.BGR;
                    mats[0].Type = DataType.Int8;
                    mats[0].Device = null;
                }
            }
            else
            {
                mats = new MMDeploy.Mat[1];
            }
        }

        private static void ResultToColorMask(SegmentorOutput output, out OpenCvSharp.Mat colorMask)
        {
            colorMask = new OpenCvSharp.Mat(output.Height, output.Width, MatType.CV_8UC3, new Scalar());
            Vec3b[] palette = GenPalette(output.Classes);
            unsafe
            {
                byte* data = colorMask.DataPointer;
                if (output.Mask.Length > 0)
                {
                    fixed (int* _label = output.Mask)
                    {
                        int* label = _label;
                        for (int i = 0; i < output.Height; i++)
                        {
                            for (int j = 0; j < output.Width; j++)
                            {
                                data[0] = palette[*label][0];
                                data[1] = palette[*label][1];
                                data[2] = palette[*label][2];
                                data += 3;
                                label++;
                            }
                        }
                    }
                }
                else
                {
                    fixed (float* _score = output.Score)
                    {
                        float* score = _score;
                        int total = output.Height * output.Width;
                        for (int i = 0; i < output.Height; i++)
                        {
                            for (int j = 0; j < output.Width; j++)
                            {
                                var scores = new List<Tuple<float, int>>();
                                for (int k = 0; k < output.Classes; k++)
                                {
                                    scores.Add(new Tuple<float, int>(score[k * total + i * output.Width + j], k));
                                }
                                scores.Sort();
                                var lastScore = scores.Last();
                                data[0] = palette[lastScore.Item2][0];
                                data[1] = palette[lastScore.Item2][1];
                                data[2] = palette[lastScore.Item2][2];
                                data += 3;
                            }
                        }
                    }
                }
            }
        }

        private static Vec3b[] GenPalette(int classes)
        {
            Random rnd = new Random(0);
            Vec3b[] palette = new Vec3b[classes];
            for (int i = 0; i < classes; i++)
            {
                palette[i] = new Vec3b((byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255), (byte)rnd.Next(0, 255));
            }
            return palette;
        }

        private static void Mat2HalconRgb(OpenCvSharp.Mat mat, out HObject image)
        {
            int ImageWidth = mat.Width;
            int ImageHeight = mat.Height;
            int channel = mat.Channels();
            long size = ImageWidth * ImageHeight * channel;
            int col_byte_num = ImageWidth * channel;

            byte[] rgbValues = new byte[size];
            unsafe
            {
                for (int i = 0; i < mat.Height; i++)
                {
                    IntPtr c = mat.Ptr(i);
                    Marshal.Copy(c, rgbValues, i * col_byte_num, col_byte_num);
                }

                fixed (byte* pc = rgbValues)
                {
                    IntPtr ptr = new IntPtr(pc);
                    HOperatorSet.GenImageInterleaved(out image, ptr, "bgr", ImageWidth, ImageHeight, 0, "byte", 0, 0, 0, 0, -1, 0);
                }
            }
        }
    }
}
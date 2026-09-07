using HalconDotNet;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using MMDeploy;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System.Diagnostics;

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


        void Predict3DImage(string modelSide, int modelNum, HObject imgGray, out HObject imgMask, out HObject mask01, out HObject mask02, out HObject mask03);

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

        private readonly Dictionary<string, List<Segmentor>> _segHandles = new Dictionary<string, List<Segmentor>>();
        private readonly Dictionary<string, List<Detector>> _detHandles = new Dictionary<string, List<Detector>>();
        private readonly object _lock = new object();

        private bool _disposed;

        public AiDriveService(string deviceName = "cuda", int deviceId = 0, ILogger logger = null)
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
                _logger?.Warning("LoadSegModel: stationId 或 modelPaths 无效");
                return false;
            }
            try
            {
                lock (_lock)
                {

                    if (_segHandles.ContainsKey(modelId))
                    {
                        _logger?.Information("工位 {StationId} 分割模型已加载，跳过", modelId);
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
                _logger?.Information("工位 {StationId} 加载 {Count} 个分割模型完成", modelId, modelPaths.Length);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "工位 {StationId} 加载分割模型失败", modelId);
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
                using (var mmInput = Halcon2MmMat(imgGray))
                {
                    var output = segmentor.Apply(mmInput.Mats);
                    if (output == null || output.Count == 0)
                    {
                        _logger?.Warning("分割输出为空");
                        return;
                    }
                    ResultToColorMask(output[0], out var colorMask);
                    try
                    {
                        Mat2HalconRgb(colorMask, out imgMask);
                    }
                    finally
                    {
                        colorMask.Dispose();
                    }
                }
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
                var swConvert = Stopwatch.StartNew();
                var mmInput = Halcon2MmMat(imgGray);
                swConvert.Stop();
                var swInfer = Stopwatch.StartNew();
                var output = detector.Apply(mmInput.Mats);
                swInfer.Stop();
                _logger?.Information("[AI检测] 模型ID={ModelId} 图像转换耗时: {ConvertMs} ms, 推理耗时: {InferMs} ms",
    modelId, swConvert.ElapsedMilliseconds, swInfer.ElapsedMilliseconds);
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

        private static unsafe MmMatInput Halcon2MmMat(HObject imgGray)
        {
            if (imgGray == null)
                throw new ArgumentNullException(nameof(imgGray));

            HOperatorSet.CountChannels(imgGray, out HTuple chsTuple);
            int chs = chsTuple.I;

            // 灰度图像
            if (chs == 1)
            {
                HOperatorSet.GetImagePointer1(imgGray, out HTuple ptrGray, out HTuple type, out HTuple width, out HTuple height);
                int w = width.I;
                int h = height.I;
                int bytes = w * h;
                byte[] buffer = new byte[bytes];
                Marshal.Copy((IntPtr)ptrGray, buffer, 0, bytes);

                var mats = new MMDeploy.Mat[1];
                var input = new MmMatInput(mats, buffer); // 固定 buffer
                mats[0].Height = h;
                mats[0].Width = w;
                mats[0].Channel = 1; // 修正通道数
                mats[0].Format = PixelFormat.Grayscale;
                mats[0].Type = DataType.Int8;
                mats[0].Device = null;

                return input;
            }
            // 彩色图像
            else if (chs == 3)
            {
                HOperatorSet.GetImagePointer3(imgGray, out HTuple ptrRed, out HTuple ptrGreen, out HTuple ptrBlue,
                    out HTuple type, out HTuple width, out HTuple height);
                int w = width.I;
                int h = height.I;
                int pixels = w * h;
                int bytes = pixels * 3;
                byte[] buffer = new byte[bytes];

                unsafe
                {
                    byte* r = (byte*)(IntPtr)ptrRed;
                    byte* g = (byte*)(IntPtr)ptrGreen;
                    byte* b = (byte*)(IntPtr)ptrBlue;
                    for (int i = 0; i < pixels; i++)
                    {
                        buffer[i * 3 + 0] = b[i];
                        buffer[i * 3 + 1] = g[i];
                        buffer[i * 3 + 2] = r[i];
                    }
                }

                var mats = new MMDeploy.Mat[1];
                var input = new MmMatInput(mats, buffer); // 固定 buffer
                mats[0].Height = h;
                mats[0].Width = w;
                mats[0].Channel = 3; // 修正通道数
                mats[0].Format = PixelFormat.BGR;
                mats[0].Type = DataType.Int8;
                mats[0].Device = null;

                return input;
            }
            else
            {
                throw new NotSupportedException($"不支持的图像通道数: {chs}");
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

        private static unsafe void Mat2HalconRgb(OpenCvSharp.Mat mat, out HObject image)
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

        public void Predict3DImage(string modelId, int modelIndex, HObject imgGray, out HObject imgMask, out HObject mask01, out HObject mask02, out HObject mask03)
        {
            // 初始化输出为空对象
            HOperatorSet.GenEmptyObj(out imgMask);
            HOperatorSet.GenEmptyObj(out mask01);
            HOperatorSet.GenEmptyObj(out mask02);
            HOperatorSet.GenEmptyObj(out mask03);

            if (!TryGetHandle(_segHandles, modelId, modelIndex, out var segmentor))
                return;

            try
            {
                // 转换图像并固定内存
                using (var mmInput = Halcon2MmMat(imgGray))
                {
                    var output = segmentor.Apply(mmInput.Mats);
                    if (output == null || output.Count == 0 || output[0].Mask == null || output[0].Mask.Length == 0)
                    {
                        _logger?.Warning("[AiDrive] 3D分割输出无效");
                        return;
                    }

                    var segOut = output[0];
                    int width = segOut.Width;
                    int height = segOut.Height;
                    int[] maskData = segOut.Mask;

                    // 检查 mask 数据长度
                    if (maskData.Length < width * height)
                    {
                        _logger?.Error("[AiDrive] 3D分割Mask数据长度不足");
                        return;
                    }

                    // 转换为字节数组（类别ID）
                    byte[] byteMask = new byte[width * height];
                    for (int i = 0; i < byteMask.Length; i++)
                    {
                        byteMask[i] = (byte)maskData[i];
                    }

                    // 创建临时 class map 图像
                    HObject classMap = null;
                    GCHandle handle = default;
                    try
                    {
                        handle = GCHandle.Alloc(byteMask, GCHandleType.Pinned);
                        IntPtr ptr = handle.AddrOfPinnedObject();
                        HOperatorSet.GenImage1(out classMap, "byte", width, height, ptr);

                        // 阈值分割：类别1->mask01, 2->mask02, 3->mask03
                        HOperatorSet.Threshold(classMap, out mask01, 1, 1);
                        HOperatorSet.Threshold(classMap, out mask02, 2, 2);
                        HOperatorSet.Threshold(classMap, out mask03, 3, 3);
                    }
                    finally
                    {
                        if (handle.IsAllocated) handle.Free();
                        classMap?.Dispose();
                    }

                    // 生成彩色掩码图（用于保存/调试）
                    ResultToColorMask(segOut, out OpenCvSharp.Mat colorMask);
                    try
                    {
                        Mat2HalconRgb(colorMask, out imgMask);
                    }
                    finally
                    {
                        colorMask.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "[AiDrive] 3D分割推理失败");
                HOperatorSet.GenEmptyObj(out imgMask);
                HOperatorSet.GenEmptyObj(out mask01);
                HOperatorSet.GenEmptyObj(out mask02);
                HOperatorSet.GenEmptyObj(out mask03);
            }
        }

        private sealed class MmMatInput : IDisposable
        {
            private readonly GCHandle _handle;
            public MMDeploy.Mat[] Mats { get; }

            public MmMatInput(MMDeploy.Mat[] mats, byte[] buffer)
            {
                Mats = mats;
                _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                unsafe
                {
                    mats[0].Data = (byte*)_handle.AddrOfPinnedObject();
                }
            }

            public void Dispose()
            {
                if (_handle.IsAllocated)
                    _handle.Free();
            }
        }
    }
}
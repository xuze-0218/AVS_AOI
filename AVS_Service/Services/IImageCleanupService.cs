using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using System.IO;
using System.Threading;

namespace AVS_Service.Services
{
    /// <summary>
    /// 定时图像清理服务：按保留天数删除过期的原图/结果图。
    /// 首次启动延迟一段时间后执行一次，之后每 24 小时执行一次。
    /// </summary>
    public interface IImageCleanupService
    {
        /// <summary>启动后台清理循环</summary>
        void Start();

        /// <summary>停止后台清理循环并等待当前清理完成</summary>
        Task StopAsync();

        /// <summary>手动触发一次清理（调试/管理用）</summary>
        Task RunOnceAsync();
    }

    public class ImageCleanupService : IImageCleanupService
    {
        private readonly IParametersConfigService _paramService;
        private readonly ILogger _logger;

        private CancellationTokenSource _cts;
        private Task _loopTask;

        // 每24小时跑一次
        private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
        // 首次启动延迟 30 秒
        private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(30);

        public ImageCleanupService(IParametersConfigService paramService, ILogger logger)
        {
            _paramService = paramService;
            _logger = logger;
        }

        public void Start()
        {
            if (_loopTask != null && !_loopTask.IsCompleted)
            {
                _logger.Debug("图像清理服务已在运行，跳过重复启动");
                return;
            }

            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => CleanupLoopAsync(_cts.Token));
            _logger.Information("图像清理服务已启动（间隔 {Hours} 小时）", Interval.TotalHours);
        }

        public async Task StopAsync()
        {
            if (_cts == null) return;
            try
            {
                _cts.Cancel();
                if (_loopTask != null)
                {
                    var timeout = Task.Delay(TimeSpan.FromSeconds(5));
                    var completed = await Task.WhenAny(_loopTask, timeout).ConfigureAwait(false);
                    if (completed == timeout)
                        _logger.Warning("图像清理服务停止超时，强制继续关闭流程");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "停止图像清理服务失败");
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _loopTask = null;
            }
        }

        public Task RunOnceAsync()
        {
            return Task.Run(() => CleanupOnce());
        }

        private async Task CleanupLoopAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(StartupDelay, token).ConfigureAwait(false);

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        CleanupOnce();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "图像清理执行异常，将在下一周期重试");
                    }

                    await Task.Delay(Interval, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // 正常退出
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "图像清理循环异常退出");
            }
        }

        /// <summary>
        /// 执行一次清理：按保留天数删除过期原图/结果图。
        /// </summary>
        private void CleanupOnce()
        {
            string rootDir = _paramService.GetString("Global", "ImageSaveDir", "");
            if (string.IsNullOrWhiteSpace(rootDir) || !Directory.Exists(rootDir))
            {
                _logger.Warning("图像保存目录未配置或不存在，跳过清理: {Dir}", rootDir);
                return;
            }

            // 读取保留天数（2D/3D 分开，3D 缺省沿用 2D）
            int ornDays2D = _paramService.GetInt("Global", "SaveOrnImgDays", 30);
            int renDays2D = _paramService.GetInt("Global", "SaveRenImgDays", 30);
            int ornDays3D = _paramService.GetInt("Global", "SaveOrn3DImgDays", ornDays2D);
            int renDays3D = _paramService.GetInt("Global", "SaveRen3DImgDays", renDays2D);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            int totalDeleted = 0;
            long totalBytes = 0;

            // 结构：{ImageSaveDir}/{yyyy_MM_dd}/{OK|NG}/{module}/{2D|3D}/{Originallmage|ResultImage/...}
            // 按日期目录遍历，根据日期判断是否过期
            foreach (var dateDir in Directory.EnumerateDirectories(rootDir))
            {
                string dateName = Path.GetFileName(dateDir);
                if (!DateTime.TryParseExact(dateName, "yyyy_MM_dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var dirDate))
                {
                    continue; // 非日期目录（如"标定图片"）跳过
                }

                int ageDays = (DateTime.Today - dirDate.Date).Days;

                CleanupDimensionDir(dateDir, "2D", "Originallmage", ornDays2D, ageDays, ref totalDeleted, ref totalBytes);
                CleanupDimensionDir(dateDir, "2D", "ResultImage", renDays2D, ageDays, ref totalDeleted, ref totalBytes);
                CleanupDimensionDir(dateDir, "3D", "Originallmage", ornDays3D, ageDays, ref totalDeleted, ref totalBytes);
                CleanupDimensionDir(dateDir, "3D", "OriginallmageGray", ornDays3D, ageDays, ref totalDeleted, ref totalBytes);
                CleanupDimensionDir(dateDir, "3D", "ResultImage", renDays3D, ageDays, ref totalDeleted, ref totalBytes);

                TryDeleteEmptyDir(dateDir);
            }

            sw.Stop();
            _logger.Information("图像清理完成：删除 {Count} 个文件，释放 {MB:F1} MB，耗时 {Ms} ms",
                totalDeleted, totalBytes / 1024.0 / 1024.0, sw.ElapsedMilliseconds);
        }

        private void CleanupDimensionDir(string dateDir, string dimension, string subDir,
            int keepDays, int ageDays, ref int deletedCount, ref long deletedBytes)
        {
            if (ageDays < keepDays) return;   // 未过期

            // 结构：{dateDir}/{OK|NG}/{module}/{dimension}/{subDir}
            foreach (var typeDir in Directory.EnumerateDirectories(dateDir))
            {
                string typeName = Path.GetFileName(typeDir);
                if (typeName != "OK" && typeName != "NG") continue;

                foreach (var moduleDir in Directory.EnumerateDirectories(typeDir))
                {
                    string targetDir = Path.Combine(moduleDir, dimension, subDir);
                    if (!Directory.Exists(targetDir)) continue;

                    DeleteExpiredFiles(targetDir, keepDays, ref deletedCount, ref deletedBytes);
                    TryDeleteEmptyDir(targetDir);
                }
            }
        }

        private void DeleteExpiredFiles(string dir, int keepDays, ref int deletedCount, ref long deletedBytes)
        {
            DateTime cutoff = DateTime.Today.AddDays(-keepDays);
            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "枚举目录失败: {Dir}", dir);
                return;
            }

            foreach (var file in files)
            {
                try
                {
                    var fi = new FileInfo(file);
                    if (!fi.Exists) continue;
                    if (fi.LastWriteTime >= cutoff) continue;   // 未过期

                    long size = fi.Length;
                    using (new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        // 能打开说明没被占用
                    }
                    fi.Delete();
                    deletedCount++;
                    deletedBytes += size;
                }
                catch (IOException)
                {
                    // 文件被占用，跳过，下次再删
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.Warning("无权限删除文件: {File}", file);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "删除文件失败: {File}", file);
                }
            }
        }

        private void TryDeleteEmptyDir(string dir)
        {
            try
            {
                if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                    Directory.Delete(dir);
            }
            catch { }
        }
    }
}

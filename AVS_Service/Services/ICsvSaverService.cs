using AVS_Common;
using AVS_Service.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AVS_Service.Services
{
    public interface ICsvSaverService
    {
        void Report2D(InspectResult2DData data);
        void Report3D(InspectResult3DData data);
        void Clear();
    }


    public class CsvSaverService : ICsvSaverService
    {
        private const string DefaultCsvSaveDir = "DataRecord";
        private readonly ICsvFileWriter _csvWriter;
        private readonly IParametersConfigService _paramService;
        private readonly ILogger _logger;
        private readonly object _lock = new object();
        private readonly Dictionary<int, SaveData> _pending = new Dictionary<int, SaveData>();

        // 表头检查缓存
        private readonly HashSet<string> _checked2DDetailCsv = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _checked3DDetailCsv = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _checkedCombinedCsv = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public CsvSaverService(ICsvFileWriter csvWriter, IParametersConfigService paramService, ILogger logger)
        {
            _csvWriter = csvWriter;
            _paramService = paramService;
            _logger = logger;
        }

        public void Report2D(InspectResult2DData data)
        {
            if (data == null || data.PoleNum <= 0) return;

            lock (_lock)
            {
                //2D明细CSV
                Write2DDetailCsv(data);

                //缓存用于综合CSV聚合
                var sd = GetOrCreate(data.PoleNum);
                sd.Inspect2DData = data;
                sd.IsDetect2D = true;
                TryWriteCombinedCsv(sd);
            }
        }

        public void Report3D(InspectResult3DData data)
        {
            if (data == null || data.PoleNum <= 0) return;

            lock (_lock)
            {
                //3D明细CSV
                Write3DDetailCsv(data);

                //缓存用于综合CSV聚合
                var sd = GetOrCreate(data.PoleNum);
                sd.Inspect3DData = data;
                sd.IsDetect3D = true;
                TryWriteCombinedCsv(sd);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pending.Clear();
            }
        }

        // ========== 2D明细CSV ==========
        private void Write2DDetailCsv(InspectResult2DData data)
        {
            BarShape shape = data.IsSquareBar ? BarShape.SquareBar : BarShape.Circle;
            string csvPath = BuildCsvPath("2D检测", DateTime.Now.ToString("yyyy_MM_dd"), shape);
            var header = InspectionCsvHeaders.Get2DDetailHeader(data.IsSquareBar);
            EnsureHeader(csvPath, header, _checked2DDetailCsv);

            var row = new string[]
            {
                data.ModuleName ?? string.Empty,
                data.PoleNum.ToString().PadLeft(2, '0'),
                data.Result2D == Result.OK ? "OK" : "NG",
                data.ResultLength == Result.OK ? "OK" : "NG",
                formatStr(data.Length),
                data.ResultWidth == Result.OK ? "OK" : "NG",
                formatStr(data.Width),
                data.ResultOffset == Result.OK ? "OK" : "NG",
                formatStr(data.Offset),
                data.ResultPoreBreak == Result.OK ? "OK" : "NG",
                formatStr(data.PoreBreakArea),
                data.ResultBeadDiameter == Result.OK ? "OK" : "NG",
                formatStr(data.BeadDiameter),
                data.ResultfaultySol == Result.OK ? "OK" : "NG",
                formatStr(data.faultySol)
            };

            _csvWriter.WriteCsv(csvPath, new List<string[]> { row }, true);
        }

        // ========== 3D明细CSV ==========
        private void Write3DDetailCsv(InspectResult3DData data)
        {
            BarShape shape = data.IsSquareBar ? BarShape.SquareBar : BarShape.Circle;
            string csvPath = BuildCsvPath("3D检测", DateTime.Now.ToString("yyyy_MM_dd"), shape);
            var header = InspectionCsvHeaders.Get3DDetailHeader(data.IsSquareBar);
            EnsureHeader(csvPath, header, _checked3DDetailCsv);

            var row = new List<string>
            {
                data.WorkType ?? string.Empty,
                data.ModuleName ?? string.Empty,
                data.PoleNum.ToString(),
                data.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                data.Result3D.ToString(),
                data.ResultBeadHump.ToString(),
                data.ResultBeadSag.ToString(),
                formatStr(data.BeadHump),
                formatStr(data.BeadSag)
            };

            if (data.IsSquareBar)
            {
                row.Add(data.ResultBarBeadHump.ToString());
                row.Add(data.ResultBarBeadSag.ToString());
                row.Add(formatStr(data.BarBeadHump));
                row.Add(formatStr(data.BarBeadSag));
            }

            row.Add(string.IsNullOrEmpty(data.Orn3DPath) ? "定位失败" : data.Orn3DPath);
            row.Add(string.IsNullOrEmpty(data.Dump3DPath) ? "定位失败" : data.Dump3DPath);

            _csvWriter.WriteCsv(csvPath, new List<string[]> { row.ToArray() }, true);
        }

        // ========== 综合CSV ==========
        private void TryWriteCombinedCsv(SaveData sd)
        {
            if (!sd.IsDetect2D || !sd.IsDetect3D) return;
            if (sd.Inspect2DData != null && sd.Inspect3DData != null && sd.Inspect2DData.IsSquareBar != sd.Inspect3DData.IsSquareBar)
            {
                _logger.Warning($"极柱 {sd.Inspect2DData.PoleNum} 的 2D/3D 形状不一致 " +
                             $"(2D:{(sd.Inspect2DData.IsSquareBar ? "方" : "圆")}, " +
                             $"3D:{(sd.Inspect3DData.IsSquareBar ? "方" : "圆")})，跳过综合写入");
                // 清掉缓存，避免后续重复触发
                _pending.Remove(sd.Inspect2DData.PoleNum);
                return;
            }
            WriteCombinedCsv(sd);
            if (sd.Inspect2DData != null)
                _pending.Remove(sd.Inspect2DData.PoleNum);
        }

        private void WriteCombinedCsv(SaveData sd)
        {
            var d2 = sd.Inspect2DData;
            var d3 = sd.Inspect3DData;
            bool isSquareBar = d2.IsSquareBar;
            BarShape shape = d2.IsSquareBar ? BarShape.SquareBar : BarShape.Circle;
            string csvPath = BuildCsvPath("综合检测", DateTime.Now.ToString("yyyy_MM_dd"), shape);
            var header = InspectionCsvHeaders.GetCombinedHeader(isSquareBar);
            EnsureHeader(csvPath, header, _checkedCombinedCsv);

            var row = new List<string>
            {
                d2.WorkType,
                d2.ModuleName,
                d2.PoleNum.ToString(),
                d2.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                ((int)d2.Result2D + (int)d3.Result3D) == 0 ? "OK" : "NG",
                d2.Result2D.ToString(),
                d2.ResultLength.ToString(),
                d2.ResultWidth.ToString(),
                d2.ResultOffset.ToString(),
                d2.ResultPoreBreak.ToString(),
                d2.ResultBeadDiameter.ToString(),
                formatStr(d2.Length),
                formatStr(d2.Width),
                formatStr(d2.Offset),
                formatStr(d2.PoreBreakArea),
                formatStr(d2.BeadDiameter),
                d2.ResultfaultySol.ToString(),
                formatStr(d2.faultySol),
                d3.Result3D.ToString(),
                d3.ResultBeadHump.ToString(),
                d3.ResultBeadSag.ToString(),
                formatStr(d3.BeadHump),
                formatStr(d3.BeadSag)
            };

            if (isSquareBar)
            {
                row.Add(d3.ResultBarBeadHump.ToString());
                row.Add(d3.ResultBarBeadSag.ToString());
                row.Add(formatStr(d3.BarBeadHump));
                row.Add(formatStr(d3.BarBeadSag));
            }

            row.Add(string.IsNullOrEmpty(d2.Orn2DPath) ? "定位失败" : d2.Orn2DPath);
            row.Add(string.IsNullOrEmpty(d2.Dump2DPath) ? "定位失败" : d2.Dump2DPath);
            row.Add(string.IsNullOrEmpty(d3.Orn3DPath) ? "定位失败" : d3.Orn3DPath);
            row.Add(string.IsNullOrEmpty(d3.Dump3DPath) ? "定位失败" : d3.Dump3DPath);

            _csvWriter.WriteCsv(csvPath, new List<string[]> { row.ToArray() }, true);
        }

        // ========== 工具方法 ==========
        private SaveData GetOrCreate(int poleNum)
        {
            if (!_pending.TryGetValue(poleNum, out var sd))
            {
                sd = new SaveData();
                _pending[poleNum] = sd;
            }
            return sd;
        }

        private string BuildCsvPath(string subDir, string dateStr, BarShape shape)
        {
            string baseDir = _paramService.GetString("Global", "CsvSaveDir", "");
            if (string.IsNullOrWhiteSpace(baseDir))
                baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultCsvSaveDir);

            string saveDir = Path.Combine(baseDir, subDir, dateStr);
            Directory.CreateDirectory(saveDir);
            return Path.Combine(saveDir, $"_DataRecord_{shape.ToFileSuffix()}.csv");
        }

        private void EnsureHeader(string csvPath, string[] expectedHeader, HashSet<string> checkedSet)
        {
            lock (_lock)
            {
                if (checkedSet.Contains(csvPath)) return;
                if (File.Exists(csvPath) && new FileInfo(csvPath).Length > 0)
                {
                    if (!HasExpectedHeader(csvPath, expectedHeader))
                    {
                        HandleCorruptFile(csvPath);
                        _csvWriter.WriteCsv(csvPath, new List<string[]> { expectedHeader }, true);
                    }
                }
                else
                {
                    _csvWriter.WriteCsv(csvPath, new List<string[]> { expectedHeader }, true);
                }
                checkedSet.Add(csvPath);
            }
        }

        private void HandleCorruptFile(string csvPath)
        {
            string backup = $"{csvPath}.corrupt-{DateTime.Now:yyyyMMddHHmmss}";
            try
            {
                File.Move(csvPath, backup);
                _logger.Warning($"CSV 表头异常，已隔离：{csvPath} → {backup}");
            }
            catch (Exception ex)
            {
                _logger.Error($"隔离异常 CSV 失败：{csvPath}", ex);
            }
        }

        private bool HasExpectedHeader(string csvPath, string[] expectedHeader)
        {
            var rows = _csvWriter.ReadCsv(csvPath);
            if (rows.Count == 0) return false;
            return rows[0].SequenceEqual(expectedHeader, StringComparer.OrdinalIgnoreCase);
        }

        private static string formatStr(double value)
        {
            return string.Format("{0:000.00}", value);
        }
    }
}

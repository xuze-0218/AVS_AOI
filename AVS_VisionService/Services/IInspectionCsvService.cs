using AVS_Common;
using AVS_Service.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Services
{
    public interface IInspectionCsvService
    {
        void Report2D(InspectResult2DData data);
        void Report3D(InspectResult3DData data);
        void Clear();
    }


    public class InspectionCsvService : IInspectionCsvService
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

        public InspectionCsvService(ICsvFileWriter csvWriter, IParametersConfigService paramService, ILogger logger)
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
            string csvPath = BuildCsvPath("2D检测", DateTime.Now.ToString("yyyy_MM_dd"), "_DataRecord.csv");
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
            string csvPath = BuildCsvPath("3D检测", DateTime.Now.ToString("yyyy_MM_dd"), "_DataRecord.csv");
            bool isSquareBar = data.ResultBarBeadHump != Result.None || data.ResultBarBeadSag != Result.None; // 简化判断，也可通过配置
            var header = InspectionCsvHeaders.Get3DDetailHeader(isSquareBar);
            EnsureHeader(csvPath, header, _checked3DDetailCsv);

            var row = new List<string>
            {
                data.WorkType ?? string.Empty,
                data.ModuleName ?? string.Empty,
                data.PoleNum.ToString(),
                data.DateTime.ToString(),
                data.Result3D.ToString(),
                data.ResultBeadHump.ToString(),
                data.ResultBeadSag.ToString(),
                formatStr(data.BeadHump),
                formatStr(data.BeadSag)
            };

            if (isSquareBar)
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

            WriteCombinedCsv(sd);
            if (sd.Inspect2DData != null)
                _pending.Remove(sd.Inspect2DData.PoleNum);
        }

        private void WriteCombinedCsv(SaveData sd)
        {
            var d2 = sd.Inspect2DData;
            var d3 = sd.Inspect3DData;
            bool isSquareBar = d2.IsSquareBar;

            string csvPath = BuildCsvPath("综合检测", DateTime.Now.ToString("yyyy_MM_dd"), "_DataRecord.csv");
            var header = InspectionCsvHeaders.GetCombinedHeader(isSquareBar);
            EnsureHeader(csvPath, header, _checkedCombinedCsv);

            var row = new List<string>
            {
                d2.WorkType,
                d2.ModuleName,
                d2.PoleNum.ToString(),
                d2.DateTime.ToString(),
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

        private string BuildCsvPath(string subDir, string dateStr, string fileName)
        {
            string baseDir = _paramService.GetString("", "CsvSaveDir", "");
            if (string.IsNullOrWhiteSpace(baseDir))
                baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultCsvSaveDir);

            string saveDir = Path.Combine(baseDir, subDir, dateStr);
            Directory.CreateDirectory(saveDir);
            return Path.Combine(saveDir, fileName);
        }

        private void EnsureHeader(string csvPath, string[] expectedHeader, HashSet<string> checkedSet)
        {
            lock (_lock)
            {
                if (checkedSet.Contains(csvPath))
                    return;

                if (File.Exists(csvPath) && new FileInfo(csvPath).Length > 0)
                {
                    // 检查表头是否一致，不一致则迁移
                    if (!HasExpectedHeader(csvPath, expectedHeader))
                    {
                        MigrateHeader(csvPath, expectedHeader);
                    }
                }
                else
                {
                    _csvWriter.WriteCsv(csvPath, new List<string[]> { expectedHeader }, true);
                }
                checkedSet.Add(csvPath);
            }
        }

        private bool HasExpectedHeader(string csvPath, string[] expectedHeader)
        {
            var rows = _csvWriter.ReadCsv(csvPath);
            if (rows.Count == 0) return false;
            return rows[0].SequenceEqual(expectedHeader, StringComparer.OrdinalIgnoreCase);
        }

        private void MigrateHeader(string csvPath, string[] expectedHeader)
        {
            var rows = _csvWriter.ReadCsv(csvPath);
            if (rows.Count == 0)
            {
                rows.Add(expectedHeader);
            }
            else
            {
                rows[0] = expectedHeader;
            }
            _csvWriter.WriteCsv(csvPath, rows, false); // 覆盖写回
        }

        private static string formatStr(double value)
        {
            return string.Format("{0:000.00}", value);
        }
    }
}

using AVS_Common;
using AVS_Service.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AVS_Service.Services
{
    public interface IHistoryDataService
    {
        Task<HistoryLoadResult> LoadAsync(DateTime start, DateTime end, string root, CancellationToken ct);
    }

    public class HistoryLoadResult
    {
        public IReadOnlyList<InspectResult2DData> Rows2D { get; set; } = Array.Empty<InspectResult2DData>();
        public IReadOnlyList<InspectResult3DData> Rows3D { get; set; } = Array.Empty<InspectResult3DData>();
        public IReadOnlyList<LoadedCsvInfo> Files { get; set; } = Array.Empty<LoadedCsvInfo>();
        public IReadOnlyList<CsvFormatError> Errors { get; set; } = Array.Empty<CsvFormatError>();
    }

    public class LoadedCsvInfo
    {
        public string Path { get; set; }
        public string SchemaName { get; set; }
        public int DataRowCount { get; set; }
    }

    public class HistoryDataService : IHistoryDataService
    {
        private readonly ICsvFileWriter _csvWriter;
        private readonly ILogger _logger;

        public HistoryDataService(ICsvFileWriter csvWriter, ILogger logger)
        {
            _csvWriter = csvWriter;
            _logger = logger;
        }

        public Task<HistoryLoadResult> LoadAsync(DateTime start, DateTime end, string root, CancellationToken ct)
            => Task.Run(() => Load(start, end, root, ct), ct);

        private HistoryLoadResult Load(DateTime start, DateTime end, string root, CancellationToken ct)
        {
            var rows2D = new List<InspectResult2DData>();
            var rows3D = new List<InspectResult3DData>();
            var files = new List<LoadedCsvInfo>();
            var errors = new List<CsvFormatError>();

            foreach (var (type, subDir) in new[]
            {
            (VisionDimension.TwoD,   "2D检测"),
            (VisionDimension.ThreeD, "3D检测")
        })
            {
                for (DateTime d = start.Date; d <= end.Date; d = d.AddDays(1))
                {
                    ct.ThrowIfCancellationRequested();
                    string dir = Path.Combine(root, subDir, d.ToString("yyyy_MM_dd"));
                    if (!Directory.Exists(dir)) continue;

                    foreach (var shape in new[] { BarShape.Circle, BarShape.SquareBar })
                    {
                        string path = Path.Combine(dir, $"_DataRecord_{shape.ToFileSuffix()}.csv");
                        if (!File.Exists(path)) continue;

                        List<string[]> csv;
                        try { csv = _csvWriter.ReadCsv(path); }
                        catch (Exception ex)
                        {
                            _logger.Error($"读取失败：{path}", ex);
                            errors.Add(new CsvFormatError { FilePath = path, LineNumber = 0, Reason = ex.Message });
                            continue;
                        }
                        if (csv.Count == 0) continue;

                        var schema = CsvSchemaRegistry.MatchHeader(csv[0]);
                        if (schema == null || schema.DataType != type || schema.Shape != shape)
                        {
                            errors.Add(new CsvFormatError
                            {
                                FilePath = path,
                                LineNumber = 1,
                                Reason = "表头与文件名不匹配"
                            });
                            continue;
                        }

                        int count = 0;
                        for (int i = 1; i < csv.Count; i++)
                        {
                            if (IsEmpty(csv[i])) continue;
                            var row = CsvRowParser.Parse(csv[i], schema, path, i + 1, d, errors);
                            if (row == null) continue;
                            if (row is InspectResult2DData r2) { rows2D.Add(r2); count++; }
                            else if (row is InspectResult3DData r3) { rows3D.Add(r3); count++; }
                        }
                        files.Add(new LoadedCsvInfo
                        {
                            Path = path,
                            SchemaName = schema.Name,
                            DataRowCount = count
                        });
                    }
                }
            }

            return new HistoryLoadResult
            {
                Rows2D = rows2D,
                Rows3D = rows3D,
                Files = files,
                Errors = errors
            };
        }

        private static bool IsEmpty(string[] row)
            => row == null || row.Length == 0 || row.All(string.IsNullOrWhiteSpace);
    }
}

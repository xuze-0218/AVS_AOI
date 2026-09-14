using AVS_Service.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Services
{
    internal static class CsvRowParser
    {
        public static object Parse(string[] row, CsvSchema schema, string path, int line,
            DateTime folderDate, List<CsvFormatError> errors)
        {
            if (row.Length < schema.Headers.Length)
            {
                errors.Add(new CsvFormatError
                {
                    FilePath = path,
                    LineNumber = line,
                    Reason = $"列数不足（需要{schema.Headers.Length}，实际{row.Length}）"
                });
                return null;
            }

            if (schema.DataType == VisionDimension.TwoD)
            {
                return Parse2D(row, schema, path, line, folderDate, errors);
            }
            else
            {
                return Parse3D(row, schema, path, line, errors);
            }

        }

        private static InspectResult2DData Parse2D(string[] row, CsvSchema schema, string path, int line,
            DateTime folderDate, List<CsvFormatError> errors)
        {
            int[] statusIdx = { 2, 3, 5, 7, 9, 11, 13 };
            int[] valueIdx = { 4, 6, 8, 10, 12, 14 };

            foreach (int i in statusIdx)
                if (!IsOkNg(row[i])) { errors.Add(Err(path, line, i, schema.Headers[i], row[i])); return null; }

            var v = new double[6];
            for (int k = 0; k < valueIdx.Length; k++)
                if (!TryNum(row[valueIdx[k]], out v[k]))
                { errors.Add(Err(path, line, valueIdx[k], schema.Headers[valueIdx[k]], row[valueIdx[k]])); return null; }

            return new InspectResult2DData
            {
                ModuleName = row[0],
                PoleNum = int.TryParse(row[1], out var p) ? p : 0,
                DateTime = folderDate,                 // 2D 明细无时间列，用文件夹日期
                IsSquareBar = schema.Shape == BarShape.SquareBar,
                ResultSummary = Result.None,           // 明细无总结果，显式置空防误用
                Result2D = ToResult(row[2]),
                ResultLength = ToResult(row[3]),
                Length = v[0],
                ResultWidth = ToResult(row[5]),
                Width = v[1],
                ResultOffset = ToResult(row[7]),
                Offset = v[2],
                ResultPoreBreak = ToResult(row[9]),
                PoreBreakArea = v[3],
                ResultBeadDiameter = ToResult(row[11]),
                BeadDiameter = v[4],
                ResultfaultySol = ToResult(row[13]),
                faultySol = v[5]
            };
        }

        private static InspectResult3DData Parse3D(string[] row, CsvSchema schema, string path, int line,
            List<CsvFormatError> errors)
        {
            bool square = schema.Shape == BarShape.SquareBar;
            int[] statusIdx = square ? new[] { 4, 5, 6, 9, 10 } : new[] { 4, 5, 6 };
            int[] valueIdx = square ? new[] { 7, 8, 11, 12 } : new[] { 7, 8 };

            foreach (int i in statusIdx)
                if (!IsOkNg(row[i])) { errors.Add(Err(path, line, i, schema.Headers[i], row[i])); return null; }

            var v = new double[4];
            for (int k = 0; k < valueIdx.Length; k++)
                if (!TryNum(row[valueIdx[k]], out v[k]))
                { errors.Add(Err(path, line, valueIdx[k], schema.Headers[valueIdx[k]], row[valueIdx[k]])); return null; }

            DateTime.TryParse(row[3], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt);

            return new InspectResult3DData
            {
                WorkType = row[0],
                ModuleName = row[1],
                PoleNum = int.TryParse(row[2], out var p) ? p : 0,
                DateTime = dt,
                IsSquareBar = square,
                ResultSummary = Result.None,
                Result3D = ToResult(row[4]),
                ResultBeadHump = ToResult(row[5]),
                BeadHump = v[0],
                ResultBeadSag = ToResult(row[6]),
                BeadSag = v[1],
                ResultBarBeadHump = square ? ToResult(row[9]) : Result.None,
                ResultBarBeadSag = square ? ToResult(row[10]) : Result.None,
                BarBeadHump = square ? v[2] : 0,
                BarBeadSag = square ? v[3] : 0
            };
        }

        private static bool IsOkNg(string s)
            => string.Equals(s?.Trim(), "OK", StringComparison.OrdinalIgnoreCase)
            || string.Equals(s?.Trim(), "NG", StringComparison.OrdinalIgnoreCase);

        private static Result ToResult(string s)
            => string.Equals(s?.Trim(), "OK", StringComparison.OrdinalIgnoreCase) ? Result.OK
             : string.Equals(s?.Trim(), "NG", StringComparison.OrdinalIgnoreCase) ? Result.NG
             : Result.None;

        private static bool TryNum(string s, out double v)
        {
            v = 0;
            string c = (s ?? "").Trim().Trim('"');
            return double.TryParse(c, NumberStyles.Float, CultureInfo.CurrentCulture, out v)
                || double.TryParse(c, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
        }

        private static CsvFormatError Err(string path, int line, int col, string header, string actual)
            => new CsvFormatError
            {
                FilePath = path,
                LineNumber = line,
                Reason = $"第{col + 1}列“{header}”非法，实际“{actual}”"
            };
    }

    public sealed class CsvFormatError
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string Reason { get; set; }
        public override string ToString()
            => $"{Path.GetFileName(FilePath)} 第{LineNumber}行：{Reason}";
    }
}

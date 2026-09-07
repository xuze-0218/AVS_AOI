using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace AVS_Common
{
    public interface ICsvFileWriter
    {
        void WriteCsv(string filePathName, List<string[]> rows, bool append);
        List<string[]> ReadCsv(string filePathName);
        List<string[]> ReadCsvShared(string filePathName);
    }

    public class CsvRw : ICsvFileWriter
    {
        private static readonly char QuoteChar = ',';
        private static readonly object WriteLock = new object();

        public void WriteCsv(string filePathName, List<string[]> rows, bool append)
        {
            lock (WriteLock)
            {
                using (var streamWriter = new StreamWriter(filePathName, append, Encoding.Default))
                {
                    foreach (string[] row in rows)
                    {
                        var stringBuilder = new StringBuilder();
                        for (int i = 0; i < row.Length; i++)
                        {
                            string text = (row[i] ?? string.Empty).Replace("\"", "").Trim();
                            if (text.IndexOf(",") > -1)
                            {
                                text = "\"" + text + "\"";
                            }
                            stringBuilder.Append(text);
                            if (i != row.Length - 1)
                            {
                                stringBuilder.Append(QuoteChar);
                            }
                        }
                        streamWriter.WriteLine(stringBuilder.ToString());
                    }
                    streamWriter.Flush();
                }
            }
        }

        public List<string[]> ReadCsv(string filePathName)
        {
            lock (WriteLock)
            {
                return ReadCsvCore(filePathName);
            }
        }

        /// <summary>
        /// 统计/历史界面使用：允许生产线程同时向当天 CSV 追加数据。
        /// 先读取文件快照；若末行尚未写完整，则忽略该末行。
        /// </summary>
        public List<string[]> ReadCsvShared(string filePathName)
        {
            if (!File.Exists(filePathName))
                return new List<string[]>();

            byte[] snapshot;
            int bytesRead = 0;
            using (var fileStream = new FileStream(filePathName, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete))
            {
                if (fileStream.Length > int.MaxValue)
                {
                    throw new IOException("CSV文件过大，无法进行统计：" + filePathName);
                }

                snapshot = new byte[(int)fileStream.Length];
                while (bytesRead < snapshot.Length)
                {
                    int count = fileStream.Read(snapshot, bytesRead, snapshot.Length - bytesRead);
                    if (count == 0)
                    {
                        break;
                    }
                    bytesRead += count;
                }
            }

            string content = Encoding.Default.GetString(snapshot, 0, bytesRead);
            if (content.Length > 0 && !content.EndsWith("\n", StringComparison.Ordinal))
            {
                int lastNewLine = content.LastIndexOf('\n');
                content = lastNewLine < 0 ? string.Empty : content.Substring(0, lastNewLine + 1);
            }

            var list = new List<string[]>();
            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(GetStrCellVal(line).ToArray());
                }
            }
            return list;
        }

        private List<string[]> ReadCsvCore(string filePathName)
        {
            var list = new List<string[]>();
            if (!File.Exists(filePathName))
                return list;

            using (var streamReader = new StreamReader(filePathName, Encoding.Default))
            {
                string text;
                while ((text = streamReader.ReadLine()) != null)
                {
                    list.Add(GetStrCellVal(text).ToArray());
                }
            }
            return list;
        }

        private static List<string> GetStrCellVal(string rowStr)
        {
            var list = new List<string>();
            while (rowStr != null && rowStr.Length > 0)
            {
                string text = "";
                if (rowStr.StartsWith("\""))
                {
                    rowStr = rowStr.Substring(1);
                    int num = rowStr.IndexOf("\",");
                    int num2 = rowStr.IndexOf("\" ,");
                    int num3 = rowStr.IndexOf("\"");
                    if (num < 0)
                    {
                        num = num2;
                    }
                    if (num < 0)
                    {
                        num = num3;
                    }
                    if (num > -1)
                    {
                        text = rowStr.Substring(0, num);
                        rowStr = ((num + 2 >= rowStr.Length) ? "" : rowStr.Substring(num + 2).Trim());
                    }
                    else
                    {
                        text = rowStr;
                        rowStr = "";
                    }
                }
                else
                {
                    int num = rowStr.IndexOf(",");
                    if (num > -1)
                    {
                        text = rowStr.Substring(0, num);
                        rowStr = ((num + 1 >= rowStr.Length) ? "" : rowStr.Substring(num + 1).Trim());
                    }
                    else
                    {
                        text = rowStr;
                        rowStr = "";
                    }
                }
                if (text == "")
                {
                    text = " ";
                }
                list.Add(text);
            }
            return list;
        }
    }
}

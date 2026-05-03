using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS
{
    class CSV_RW
    {
        //设置分隔符
        private static char quotechar = ',';

        public static void WriteCSV(string filePathName, List<string[]> rows, bool append)
        {
            StreamWriter streamWriter = new StreamWriter(filePathName, append, Encoding.Default);
            foreach (string[] row in rows)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < row.Length; i++)
                {
                    string text = row[i].Replace("\"", "").Trim();
                    if (text == null)
                    {
                        text = "";
                    }
                    if (text.IndexOf(",") > -1)
                    {
                        text = "\"" + text + "\"";
                    }
                    stringBuilder.Append(text);
                    if (i != row.Length - 1)
                    {
                        stringBuilder.Append(quotechar);
                    }
                }
                streamWriter.WriteLine(stringBuilder.ToString());
            }
            streamWriter.Flush();
            streamWriter.Close();
        }

        public static List<string[]> ReadCSV(string filePathName)
        {
            StreamReader streamReader = new StreamReader(filePathName, Encoding.Default);
            string text = streamReader.ReadLine();
            List<string[]> list = new List<string[]>();
            while (text != null)
            {
                List<string> strCellVal = getStrCellVal(text);
                string[] array = new string[strCellVal.Count];
                for (int i = 0; i < strCellVal.Count; i++)
                {
                    array[i] = strCellVal[i];
                }
                list.Add(array);
                text = streamReader.ReadLine();
            }
            streamReader.Close();
            return list;
        }

        private static List<string> getStrCellVal(string rowStr)
        {
            List<string> list = new List<string>();
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

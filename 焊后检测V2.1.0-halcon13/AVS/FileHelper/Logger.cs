using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AVS
{
    public class Logger
    {
        private static readonly object _fileLocker = new object();
        private static readonly object _viewLocker = new object();
        private static readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();

        public static string BasePath = Application.StartupPath + "\\Log";
        public static Action<string> ViewLogHandler { get; set; }
        public static bool DebugMode { get; set; }

        private static bool isRunning;
        public static bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                if (isRunning)
                {
                    StartLog();
                }
            }
        }

        public static void Log(string msg)
        {
            //logQueue.Enqueue(msg);
            Task.Run(() =>
            {
                FileLog(msg);
                ViewLog(msg);
            });
        }

        public static void ProcessLog()
        {
            while (IsRunning)
            {
                Thread.Sleep(10);
                if (logQueue.TryDequeue(out string msg))
                {
                    FileLog(msg);
                    ViewLog(msg);
                }
            }
            while (logQueue.Count > 0)
            {
                if (logQueue.TryDequeue(out string msg))
                {
                    FileLog(msg);
                    ViewLog(msg);
                }
            }
        }

        public static void StartLog()
        {
            var thread = new Thread(() => { IsRunning = true; ProcessLog(); });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void Info(string msg)
        {
            Log("[消息] " + msg);
        }

        public static void Error(string msg)
        {
            Log("[异常] " + msg);
        }

        public static void Warning(string msg)
        {
            Log("[警告] " + msg);
        }

        public static void Debug(string msg)
        {
            if (DebugMode)
            {
                Log("[调试] " + msg);
            }
        }
        public static void ViewLog(string msg)
        {
            try
            {
                ViewLogHandler?.Invoke($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} {msg}" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        public static void FileLog(string msg)
        {
            string fileName = $"MES_{DateTime.Now:yyyy-MM-dd}.log";
            string fullPath = Path.Combine(BasePath, fileName);
            try
            {
                if (!Directory.Exists(BasePath))
                {
                    Directory.CreateDirectory(BasePath);
                }
                var info = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} {msg}{Environment.NewLine}";
                lock (_fileLocker)
                {
                    File.AppendAllText(fullPath, info, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        public static void ShowInfo(string text, bool logFlag = false)
        {
            MessageBox.Show(text, "消息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (logFlag) Info(text);
        }

        public static void ShowError(string text, bool logFlag = false)
        {
            MessageBox.Show(text, "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (logFlag) Error(text);
        }

        public static bool ShowWarning(string text, bool logFlag = false)
        {
            var dialogResult = MessageBox.Show(text, "警告提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (logFlag) Warning($"{text},选择项：{dialogResult}");
            return dialogResult == DialogResult.OK;
        }

        public static bool ShowQuestion(string text, bool logFlag = false)
        {
            var dialogResult = MessageBox.Show(text, "确认提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (logFlag) Warning($"{text},选择项：{dialogResult}");
            return dialogResult == DialogResult.Yes;
        }

        public static DialogResult ShowChoice(string text, bool logFlag = false)
        {
            var dialogResult = MessageBox.Show(text, "选择提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (logFlag) Warning($"{text},选择项：{dialogResult}");
            return dialogResult;
        }
    }
}

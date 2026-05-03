using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AVS
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region 异常处理
            //设置应用程序处理异常的模式
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //注册UI线程异常处理函数
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            //注册非UI线程异常处理函数
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandleExpection);
            #endregion

            #region 防止重复启动
            bool createNew = false;
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, Application.ProductName, out createNew))
            {
                if (createNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    MessageBox.Show("应用程序已启动...");
                    System.Threading.Thread.Sleep(1000);
                    System.Environment.Exit(1);
                }

                //Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);
                //Application.Run(new MainForm());
            }
            #endregion
        }

        /// <summary>
        /// 处理UI线程未经处理的异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            string error = DateTime.Now + $",出现应用程序未经处理的异常{e}\r\n";
            if (ex != null)
            {
                error += $"错误源:{ex.Source},异常类型:{ex.GetType().Name},异常消息:{ex.Message}\r\n异常函数:{ex.TargetSite},堆栈信息:{ex.StackTrace}";
            }
            SaveLog(Global.LogPath, error);
        }

        /// <summary>
        /// 处理非UI线程未经处理的异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandleExpection(object sender, System.UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            string error = DateTime.Now + $",出现应用程序未经处理的异常{e}\r\n";

            if (ex != null)
            {
                error += $"异常消息:{ex.Message},堆栈信息:{ex.StackTrace}";
                SaveLog(Global.LogPath, error);
            }
        }

        public static void SaveLog(string logPath, string logText)
        {
            DateTime t = DateTime.Now;

            try
            {
                // 如果记录文件目录不存在，先创建目录
                if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);

                // 一天一个记录文件。新信息加入到文件末尾
                using (StreamWriter sw = new StreamWriter(
                    new FileStream(logPath + t.ToString("yyyy-MM-dd") + ".log", FileMode.Append), Encoding.Default))
                {
                    sw.Write(t.ToString("yyyy-MM-dd HH:mm:ss") + " - " + logText + "\r\n");
                    sw.Flush();
                }
            }
            catch
            {
            }
        }
    }
}

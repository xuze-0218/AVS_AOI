using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using System.IO;
using HalconDotNet;

using VisionUserControls.PrimaryClass;
using VisionUserControls.SocketCommManage;

namespace AVS
{
    internal partial class Global
    {
        //For AboutForm
        public static string appName = "VisionSystem";
        public static string appVersion = "180814.A1";
        public static string fullAppName_CN = "视觉系统";
        public static string fullAppName_EN = "Vision System for PACK";
        public static string copyRightString = "Copyright(c) 2018-19, Wuxi Autowell Technology Co., Ltd.";

        public delegate void LogEventHandler(string msg);
        public static LogEventHandler AddLog;       
    }

    internal partial class Global
    {
        public static ClientManageData cMD = new ClientManageData();    //客户端管理
    }
    internal partial class Global
    {
        
    }
    internal partial class Global
    {
        //程序所需的各个参数和模型均保存在配方参数文件夹内，
        //程序启动时，先读取设置获取配方参数文件夹路径，然后根据路径加载参数
        // 从本地文件读取配方参数
        public static bool ReadGlobalParams()
        {
            return ParamReadWrite.ReadGlobalParams();
        }

        // 将配方参数写入本地文件
        public static bool WriteGlobalParams()
        {
            return ParamReadWrite.WriteGlobalParams();
        }

        // 获取配方参数文件路径
        public static string GetParamDir()
        {
            return ParamReadWrite.GetParamDir();
        }

        // 保存配方参数文件路径
        public static bool SaveParamDir(String paramName)
        {
            return ParamReadWrite.SaveParamDir(paramName);
        }
        //************************************************************************************************************************
    }
    internal partial class Global
    {
        //实例化全局参数
        public static ParamsGlobal myParams = new ParamsGlobal();
        //当前执行文件路径
        private static string _appPath;   
        public static string AppPath
        {
            get { return _appPath; }
            set { _appPath = value;}
        }
        //系统配置路径
        private static string _configFileName; 
        public static string ConfigFileName
        {
            get { return _configFileName; }
            set { _configFileName = value; }
        }
        //配方文件路径
        private static string _recipePath;
        public static string RecipePath
        {
            get { return _recipePath; }
            set { _recipePath = value; }
        }
        //系统日志文件存储路径
        private static string _logPath;
        public static string LogPath
        {
            get { return _logPath; }
            set { _logPath = value; }
        }

        public static void SystemInit()
        {
            AppPath = Application.StartupPath;
            if (!AppPath.EndsWith("\\"))
            {
                AppPath += "\\";
            }
            ConfigFileName = AppPath + "01config\\myConfig.json";   //系统配置路径
            RecipePath = AppPath + "02recipe\\";                    //配方文件路径
            LogPath = AppPath + "03log\\";                          //系统日志文件存储路径
            //获取配方保存路径
            RecipePath = GetParamDir();
            //读取配方参数
            if (!ReadGlobalParams())
            {
                MessageBox.Show("参数加载失败，请检查后重新加载！");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;

namespace AVS
{
    public class ParamReadWrite
    {
        //Global.configFileName内保存的是配方参数文件夹路径，根据该文件保存获取配方参数文件夹
        //系统启动时先读取该文件从而获取配方参数文件夹路径，然后根据配方参数路径加载配方参数
        // 参数读入
        public static bool ReadGlobalParams()
        {
            try
            {
                if (!File.Exists(Global.ConfigFileName))
                {
                    //File.Create(configFileName).Close();
                    MessageBox.Show("参数路径设置文件不存在！");
                    return false;
                }
                string paramFileName =  Global.RecipePath + "GlobalParam.json";
                if (!File.Exists(paramFileName))
                {
                    MessageBox.Show("参数文件不存在！");
                    return false;
                }
                Global.myParams = JsonConvert.DeserializeObject<ParamsGlobal>(ReadJson(paramFileName));//反序列化
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数读取出错！\r\n" + ex.Message);
                return false;
            }
        }

        // 参数写入
        public static bool WriteGlobalParams()
        {
            try
            {
                if (!File.Exists(Global.ConfigFileName))
                {
                    MessageBox.Show("参数路径设置文件不存在！");
                    return false;
                }
                //string paramFolder = JsonConvert.DeserializeObject<String>(ReadJson(Global.ConfigFileName));//反序列化
                string paramFileName = Global.RecipePath + "GlobalParam.json";
                if (!paramFileName.EndsWith(".json"))
                {
                    paramFileName += ".json";
                }
                if (WriteJson(paramFileName, Global.myParams))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数存储出错！\r\n" + ex.Message);
                return false;
            }
        }

        // 获取参数文件路径名称
        public static string GetParamDir()
        {
            try
            {
                if (!File.Exists(Global.ConfigFileName))
                {
                    MessageBox.Show("参数路径设置文件不存在！");
                    return null;
                }
                string paramFolder = JsonConvert.DeserializeObject<String>(ReadJson(Global.ConfigFileName));//反序列化
                string paramFileName = Global.AppPath + paramFolder + "\\";
                return paramFileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数读取出错！\r\n" + ex.Message);
                return null;
            }
        }

        // 保存参数文件名称
        public static bool SaveParamDir(String paramName)
        {
            try
            {
                WriteJson(Global.ConfigFileName, paramName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数读取出错！\r\n" + ex.Message);
                return false;
            }
        }

        //************************************************************************************************************************
        /// <summary>
        /// 将对象序列化后保存至输入路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool WriteJson(string path, Object obj)
        {
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        string str = JsonConvert.SerializeObject(obj, Formatting.Indented);
                        streamWriter.WriteLine(str);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 读取json文件，返回字符串
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static String ReadJson(string path)
        {
            string objStr = "";
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        objStr = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                objStr = "";
            }
            finally
            {

            }
            return objStr;
        }
    }
}

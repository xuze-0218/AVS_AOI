using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisionUserControls.PrimaryClass;

namespace AVS
{
    public class ParmManage
    {
        public static string configFileName = "";

        //导出检测参数，写入配置文件
        public static int ParmDataExport(string fileName)
        {
            string ret = "";
            string data = "";
            string delimiter = ",";
            /*
            //A相机 --- 定位模型匹配参数
            data = Utils.ObjectToString(typeof(ImgLocation.ModelSearch_PARM), (object)Global.imgSearchParmA, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "相机A定位匹配参数", data, out ret) < 0)
            {
                MessageBox.Show("相机A定位模板参数保存失败！");
                return -1;
            }
            //A相机 --- 定位拍照点位参数
            data = Utils.ObjectToString(typeof(ImgLocation.ImgCamCapPsn_PARM), (object)Global.camCapPsnParmA, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "相机A拍照点位参数", data, out ret) < 0)
            {
                MessageBox.Show("相机A拍照点位参数保存失败！");
                return -1;
            }
            //A相机 --- 定位防呆检测参数
            data = Utils.ObjectToString(typeof(ImgLocation.CircleMarkParam), (object)Global.mistakeProofA, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "相机A防呆小圆参数", data, out ret) < 0)
            {
                MessageBox.Show("相机A防呆小圆检测参数保存失败！");
                return -1;
            }
            //A相机 --- 定位大圆检测参数
            data = Utils.ObjectToString(typeof(ImgLocation.CircleMarkParam), (object)Global.markCircleA, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "相机A定位大圆参数", data, out ret) < 0)
            {
                MessageBox.Show("相机A定位大圆检测参数保存失败！");
                return -1;
            }
            //A相机 --- 图像保存参数写入
            data = Utils.ObjectToString(typeof(ImgLocation.ImageSave_PARM), (object)Global.imgSaveParmA, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "A_图像保存参数", data, out ret) < 0)
            {
                MessageBox.Show("A_图像保存参数写入失败。");
                return -1;
            }
            //**********************************************************************************************************
            //B相机 --- 定位模型匹配参数
            data = Utils.ObjectToString(typeof(ImgLocation.ModelSearch_PARM), (object)Global.imgSearchParmB, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "相机B定位匹配参数", data, out ret) < 0)
            {
                MessageBox.Show("相机B定位模板参数保存失败！");
                return -1;
            }
            //B相机 --- 定位拍照点位参数
            data = Utils.ObjectToString(typeof(ImgLocation.ImgCamCapPsn_PARM), (object)Global.camCapPsnParmB, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "相机B拍照点位参数", data, out ret) < 0)
            {
                MessageBox.Show("相机B拍照点位参数保存失败！");
                return -1;
            }
            //B相机 --- 定位防呆检测参数
            data = Utils.ObjectToString(typeof(ImgLocation.CircleMarkParam), (object)Global.mistakeProofB, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "相机B防呆小圆参数", data, out ret) < 0)
            {
                MessageBox.Show("相机B防呆小圆参数保存失败！");
                return -1;
            }
            //B相机 --- 定位大圆检测参数
            data = Utils.ObjectToString(typeof(ImgLocation.CircleMarkParam), (object)Global.markCircleB, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "相机B定位大圆参数", data, out ret) < 0)
            {
                MessageBox.Show("相机B定位大圆参数保存失败！");
                return -1;
            }
            //B相机 --- 图像保存参数写入
            data = Utils.ObjectToString(typeof(ImgLocation.ImageSave_PARM), (object)Global.imgSaveParmB, delimiter);
            if (Utils.IniWrite(fileName, "Parms", "B_图像保存参数", data, out ret) < 0)
            {
                MessageBox.Show("B_图像保存参数写入失败。");
                return -1;
            }
            */
            return 1;
        }
        //导入检测参数,从配置文件中读取
        public static int ParmDataImport(string fileName)
        {
            //若配置文件不存在
            if (!File.Exists(fileName))
            {
                MessageBox.Show("检测参数导入失败，配置文件不存在！");
                return -1;
            }
            string retString = "";
            object obj = null;
            string data = "";
            try
            {
                /*
                //A 相机 --- 定位模型匹配参数
                data = Utils.IniRead(fileName, "Parms", "相机A定位匹配参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.ModelSearch_PARM), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("相机A定位匹配参数导入失败。");
                    return -1;
                }
                Global.imgSearchParmA = (ImgLocation.ModelSearch_PARM)obj;
                //A 相机 --- 定位拍照点位参数
                data = Utils.IniRead(fileName, "Parms", "相机A拍照点位参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.ImgCamCapPsn_PARM), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("相机A拍照点位参数导入失败。");
                    return -1;
                }
                Global.camCapPsnParmA = (ImgLocation.ImgCamCapPsn_PARM)obj;

                //A 相机 --- 定位防呆小圆参数
                data = Utils.IniRead(fileName, "Parms", "相机A防呆小圆参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.CircleMarkParam), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("相机A防呆小圆数导入失败。");
                    return -1;
                }
                Global.mistakeProofA = (ImgLocation.CircleMarkParam)obj;

                //A 相机 --- 定位大圆检测参数
                data = Utils.IniRead(fileName, "Parms", "相机A定位大圆参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.CircleMarkParam), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("相机A定位大圆数导入失败。");
                    return -1;
                }
                Global.markCircleA = (ImgLocation.CircleMarkParam)obj;

                //A 相机 --- 图像保存参数
                data = Utils.IniRead(fileName, "Parms", "A_图像保存参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.ImageSave_PARM), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("A_图像保存参数导入失败。");
                    return -1;
                }
                Global.imgSaveParmA = (ImgLocation.ImageSave_PARM)obj;
                //***************************************************
                //B 相机 ---定位模型匹配参数
                data = Utils.IniRead(fileName, "Parms", "相机B定位匹配参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.ModelSearch_PARM), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("参数导入失败：定位B");
                    return -1;
                }
                Global.imgSearchParmB = (ImgLocation.ModelSearch_PARM)obj;
                //B 相机 --- 定位拍照点位参数
                data = Utils.IniRead(fileName, "Parms", "相机B拍照点位参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.ImgCamCapPsn_PARM), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("参数导入失败：定位B1");
                    return -1;
                }
                Global.camCapPsnParmB = (ImgLocation.ImgCamCapPsn_PARM)obj;
                //B 相机 --- 定位防呆小圆参数
                data = Utils.IniRead(fileName, "Parms", "相机B防呆小圆参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.CircleMarkParam), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("相机B防呆小圆数导入失败。");
                    return -1;
                }
                Global.mistakeProofB = (ImgLocation.CircleMarkParam)obj;

                //A 相机 --- 定位大圆检测参数
                data = Utils.IniRead(fileName, "Parms", "相机B定位大圆参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.CircleMarkParam), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("相机B定位大圆数导入失败。");
                    return -1;
                }
                Global.markCircleB = (ImgLocation.CircleMarkParam)obj;

                //B 相机 --- 模型匹配搜索参数
                data = Utils.IniRead(fileName, "Parms", "B_图像保存参数", "", out retString);
                if (Utils.StringToObject(typeof(ImgLocation.ImageSave_PARM), data, out obj) < 0 || data == "")
                {
                    MessageBox.Show("B_图像保存参数导入失败。");
                    return -1;
                }
                Global.imgSaveParmB = (ImgLocation.ImageSave_PARM)obj;
                */
            }
            catch
            {
                return -1;
            }

            return 1;
        }
  
    }
}

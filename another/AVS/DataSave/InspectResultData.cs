using DevComponents.DotNetBar;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS
{
    public class InspectDataHeader
    {
        public static string[] Header =
        {
            "工作模式","模组码","极柱号","检测时间","总结果",
            "2D结果","长度结果","宽度结果","偏移结果","爆孔结果",
            "外径结果","长度值","宽度值","偏移值","爆孔ra",
            "外径值","3D结果","余高结果","下榻结果","余高值",
            "下榻值","2D原图","2D结果图","3D原图","3D结果图",
        };
    }


    public enum Result
    {
        [Description("正常")]
        OK,
        [Description("不正常")]
        NG,
        [Description("无结果")]
        None
    }

    public class InspectResult3DData
    {


        //工作模式
        public string WorkType { get; set; }

        //模组码
        public string ModuleName { get; set; }

        //极柱号
        public int PoleNum { get; set; }

        //检测时间
        public DateTime DateTime { get; set; }

        //总结果
        public Result ResultSummary { get; set; }

        //3D结果
        public Result Result3D { get; set; }

        //余高结果
        public Result ResultBeadHump { get; set; }

        //下榻结果
        public Result ResultBeadSag { get; set; }

        //余高值
        public double BeadHump { get; set; }

        //下榻值
        public double BeadSag { get; set; }

        //3D原图
        public string Orn3DPath { get; set; }

        //3D结果图
        public string Dump3DPath { get; set; }

      
    }

    public class InspectResult2DData
    {

        //工作模式
        public string WorkType { get; set; }

        //模组码
        public string ModuleName { get; set; }

        //极柱号
        public int PoleNum { get; set; }

        //检测时间
        public DateTime DateTime { get; set; }

        //总结果
        public Result ResultSummary { get; set; }

        //2D结果
        public Result Result2D { get; set; }

        //长度结果
        public Result ResultLength { get; set; }

        //宽度结果
        public Result ResultWidth { get; set; }

        //偏移结果
        public Result ResultOffset { get; set; }

        //爆孔结果
        public Result ResultPoreBreak { get; set; }

        //外径结果
        public Result ResultBeadDiameter { get; set; }

        //虚焊结果
        public Result ResultfaultySol { get; set; }

        //长度值
        public double Length { get; set; }

        //宽度值
        public double Width { get; set; }

        //偏移值
        public double Offset { get; set; }

        //爆孔面积
        public double PoreBreakArea { get; set; }

        //外径值
        public double BeadDiameter { get; set; }

        //虚焊值
        public double faultySol { get; set; }


        //2D原图
        public string Orn2DPath { get; set; }

        //2D结果图
        public string Dump2DPath { get; set; }

     


    }

    public class SaveData
    {   
        public InspectResult2DData Inspect2DData { get; set; }

        public InspectResult3DData Inspect3DData { get; set; }

        private bool isDetect2D;

        public bool IsDetect2D
        {
            get { return isDetect2D; }
            set { isDetect2D = value;
                if(IsDetect2D && IsDetect3D)
                {
                    SaveCsvData("ALL");
                }
                else if(!(Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck)&&isDetect2D)
                {
                    SaveCsvData("2D");
                }

            }
        }

        private bool isDetect3D;

        public bool IsDetect3D
        {
            get { return isDetect3D; }
            set { isDetect3D = value;
                if (IsDetect2D && IsDetect3D)
                {
                    SaveCsvData("ALL");
                }
            }
        }

        //-保存图像检测数据
        private  void SaveCsvData(string type)
        {
            try
            {
                //测量数据保存**********************************************************************************************
                string dateStr = System.DateTime.Now.ToString("yyyy_MM_dd");
                string timeStr = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                string saveDir = "E:\\Data";
              
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                //测量数据保存****************************************************************************************
                //****************************************************************************************************
                //string data01 = DoubleToString(resultArray[1].D, 8);//焊缝长度
                //string data02 = DoubleToString(resultArray[2].D, 8);//焊缝宽度
                //string data03 = DoubleToString(resultArray[3].D, 8);//焊缝偏移
                //string data04 = DoubleToString(resultArray[4].D, 8);//爆孔尺寸
                //string data05 = DoubleToString(resultArray[5].D, 8);//
                //string data06 = DoubleToString(resultArray[6].D, 8);//焊缝外径
                string csvPath = saveDir + "\\" + dateStr + "_DataRecord.csv";
                csvPath = csvPath.Replace("\\", "/");
                //当文件不存在时添加标题栏
                if (!File.Exists(csvPath))
                {                
                    //创建一个List 将所有的内容都装入其中
                    List<string[]> dataHead = new List<string[]>();
                    //添加每一行的内容
                    dataHead.Add(InspectDataHeader.Header);
                    //保存到CSV当中
                    CSV_RW.WriteCSV(csvPath, dataHead, true);
                }
                string[] rowContent = new string[InspectDataHeader.Header.Length];

                int i = 0;
                rowContent[i++] = Inspect2DData.WorkType;   //工作模式
                rowContent[i++] = Inspect2DData.ModuleName; //模组码
                rowContent[i++] = Inspect2DData.PoleNum.ToString();
                rowContent[i++] = Inspect2DData.DateTime.ToString();

                if(type == "ALL")
                    rowContent[i++] = ((int)Inspect2DData.Result2D + (int)Inspect3DData.Result3D) == 0 ? "OK" : "NG";
                else if (type == "2D")
                    rowContent[i++] = Inspect2DData.Result2D.ToString();
                rowContent[i++] = Inspect2DData.Result2D.ToString();
                rowContent[i++] = Inspect2DData.ResultLength.ToString();
                rowContent[i++] = Inspect2DData.ResultWidth.ToString();
                rowContent[i++] = Inspect2DData.ResultOffset.ToString();
                rowContent[i++] = Inspect2DData.ResultPoreBreak.ToString();
                rowContent[i++] = Inspect2DData.ResultBeadDiameter.ToString();

                rowContent[i++] = string.Format("{0:000.00}", Inspect2DData.Length);
                rowContent[i++] = string.Format("{0:000.00}", Inspect2DData.Width);
                rowContent[i++] = string.Format("{0:000.00}", Inspect2DData.Offset);
                rowContent[i++] = string.Format("{0:000.00}", Inspect2DData.PoreBreakArea);
                rowContent[i++] = string.Format("{0:000.00}", Inspect2DData.BeadDiameter);

                if (type == "ALL")
                {
                    rowContent[i++] = Inspect3DData.Result3D.ToString();
                    rowContent[i++] = Inspect3DData.ResultBeadHump.ToString();
                    rowContent[i++] = Inspect3DData.ResultBeadSag.ToString();

                    rowContent[i++] = string.Format("{0:000.00}", Inspect3DData.BeadHump);
                    rowContent[i++] = string.Format("{0:000.00}", Inspect3DData.BeadSag);
                }
                else if(type == "2D")
                {
                    rowContent[i++] = "无结果";
                    rowContent[i++] = "无结果";
                    rowContent[i++] = "无结果";

                    rowContent[i++] = "无结果";
                    rowContent[i++] = "无结果";
                }

                
                rowContent[i++] = Inspect2DData.Orn2DPath == null ? "定位失败" : Inspect2DData.Orn2DPath;
                rowContent[i++] = Inspect2DData.Dump2DPath == null ? "定位失败" : Inspect2DData.Dump2DPath;
                if (type == "ALL")
                {
                    rowContent[i++] = Inspect3DData.Orn3DPath == null ? "定位失败" : Inspect3DData.Orn3DPath;
                    rowContent[i++] = Inspect3DData.Dump3DPath == null ? "定位失败" : Inspect3DData.Dump3DPath;
                }
                else if(type == "2D")
                {
                    rowContent[i++] = "无结果";
                    rowContent[i++] = "无结果";
                }
                    
                //创建一个List 将所有的内容都装入其中
                List<string[]> dataRows = new List<string[]>();
                //添加每一行的内容
                dataRows.Add(rowContent);
                //保存到CSV当中
                CSV_RW.WriteCSV(csvPath, dataRows, true);
            }
            catch (Exception ex)
            {
                string exMsg = "测量数据保存出错：\r\n" + ex.Message.ToString();
                Global.AddLog(exMsg);
            }
        }

    }

}

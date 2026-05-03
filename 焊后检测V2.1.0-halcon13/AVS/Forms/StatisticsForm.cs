using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AVS.Forms
{
    public partial class StatisticsForm : Form
    {
        List<string> listArry = new List<string>();
        List<string> allNG = new List<string>();
        List<string> NG2D = new List<string>();
        List<string> 长度NG = new List<string>();
        List<string> 宽度NG = new List<string>();
        List<string> 偏移NG = new List<string>();
        List<string> 爆孔NG = new List<string>();
        List<string> 外径NG = new List<string>();
        List<string> 虚焊NG = new List<string>();
        List<string> NG3D = new List<string>();
        List<string> 下塌NG = new List<string>();
        List<string> 余高NG = new List<string>();
        public StatisticsForm()
        {
            InitializeComponent();
        }
        public void statistics()
        {
            try
            {
                bool ok3d = false;
                bool ok2d = false;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = @"E:/Image/"; // 设置初始目录
                openFileDialog.Title = "选择文件";
                openFileDialog.Multiselect = true; // 允许选择多个文件
                List<string> list = new List<string>();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        string[] namestr = file.Split('\\');
                        string switch_on = namestr[2];
                        string[] lines = File.ReadAllLines(file, Encoding.Default);
                        list = lines.ToList(); // 将数组转换为List
                                               // 移除第一个元素
                        list.RemoveAt(0);
                        listArry.AddRange(list);
                        switch (switch_on)
                        {
                            case "2D检测":
                                ok2d = true;
                                for (int i = 0; i < list.Count; i++)
                                {
                                    string[] result = list[i].Split(',');
                                    if (result[2] == "NG")
                                    {
                                        NG2D.Add(result[2]);
                                    }
                                    if (result[3] == "NG")
                                    {
                                        长度NG.Add(result[3]);
                                    }
                                    if (result[5] == "NG")
                                    {
                                        宽度NG.Add(result[5]);
                                    }
                                    if (result[7] == "NG")
                                    {
                                        偏移NG.Add(result[7]);
                                    }
                                    if (result[9] == "NG")
                                    {
                                        爆孔NG.Add(result[9]);
                                    }
                                    if (result[11] == "NG")
                                    {
                                        外径NG.Add(result[11]);
                                    }
                                    if (result[13] == "NG")
                                    {
                                        虚焊NG.Add(result[13]);
                                    }
                                }
                                break;
                            case "3D检测":
                                ok3d = true;
                                for (int i = 0; i < list.Count; i++)
                                {
                                    string[] result = list[i].Split(',');
                                    if (result[2] == "NG")
                                    {
                                        NG3D.Add(result[2]);
                                    }
                                    if (result[3] == "NG")
                                    {
                                        下塌NG.Add(result[3]);
                                    }
                                    if (result[5] == "NG")
                                    {
                                        余高NG.Add(result[5]);
                                    }
                                    //else if (result[7] == "NG")
                                    //{
                                    //    偏移NG.Add(result[7]);
                                    //}
                                    //else if (result[9] == "NG")
                                    //{
                                    //    爆孔NG.Add(result[9]);
                                    //}
                                    //else if (result[11] == "NG")
                                    //{
                                    //    外径NG.Add(result[11]);
                                    //}
                                    //else if (result[13] == "NG")
                                    //{
                                    //    虚焊NG.Add(result[13]);
                                    //}
                                }
                                break;
                        }
                    }
                }
                if (ok2d)
                {
                    listView1.Items.Clear();
                    listView1.Items.Add("NG率_2D:" + (((double)NG2D.Count) / listArry.Count * 100).ToString("F4") + "％");
                    listView1.Items.Add("长度NG率:" + (((double)长度NG.Count) / listArry.Count * 100).ToString("F4") + "％");
                    listView1.Items.Add("宽度NG率:" + (((double)宽度NG.Count) / listArry.Count * 100).ToString("F4") + "％");
                    listView1.Items.Add("偏移NG率:" + (((double)偏移NG.Count) / listArry.Count * 100).ToString("F4") + "％");
                    listView1.Items.Add("爆孔NG率:" + (((double)爆孔NG.Count) / listArry.Count * 100).ToString("F4") + "％");
                    listView1.Items.Add("外径NG率:" + (((double)外径NG.Count) / listArry.Count * 100).ToString("F4") + "％");
                    listView1.Items.Add("虚焊NG率:" + (((double)虚焊NG.Count) / listArry.Count * 100).ToString("F4") + "％");
                }
                if (ok3d)
                {
                    listView1.Items.Clear();
                    listView1.Items.Add("NG率_3D:" + (((double)NG3D.Count) / listArry.Count * 100).ToString("F4") + "％");
                    listView1.Items.Add("下塌NG率:" + (((double)下塌NG.Count) / listArry.Count * 100).ToString("F4") + "％");
                    listView1.Items.Add("余高NG率:" + (((double)余高NG.Count) / listArry.Count * 100).ToString("F4") + "％");
                }
            }
            catch
            {
                MessageBox.Show("文件格式不对");
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            statistics();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            statistics();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

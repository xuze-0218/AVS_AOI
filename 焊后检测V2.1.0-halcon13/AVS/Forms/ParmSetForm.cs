using AVS.DataSave;
using AVS.Forms;
using DevComponents.DotNetBar;
using HalconDotNet;
using Lmi3d.GoSdk;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using VisionUserControls.PrimaryClass;

namespace AVS
{

    public partial class ParmSetForm : Form
    {
        //统计NG率
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
        StatisticsForm staform;
        //ParamProduct 即客户的生产控制参数，是开放给客户来调整进行产品的生产控制
        //ParamImage   即图像处理需要用到的参数
        private int CurrentRepice;
        //参数文件名
        string paramProductFileA = Global.RecipePath + "SBProductParamA.json";
        string paramProductFileB = Global.RecipePath + "SBProductParamB.json";

        string paramImageFileA = Global.RecipePath + "ImageParamA.json";
        string paramImageFileB = Global.RecipePath + "ImageParamB.json";

        bool isDgvValueChanged = false;
        private static AutoSizeFormClass asf = new AutoSizeFormClass();

        public ParmSetForm()
        {
            InitializeComponent();
            //参数设置表格字体设置
            dgvParam.RowsDefaultCellStyle.Font = new Font("黑体", 8, FontStyle.Regular);
            dgvParam.ColumnHeadersDefaultCellStyle.Font = new Font("黑体", 8, FontStyle.Regular);
            dgvParam.Columns[0].ReadOnly = true;
            dgvParam.Columns[1].ReadOnly = true;
            dgvParam.Columns[2].ReadOnly = true;
            dgvParam.Columns[3].ReadOnly = true;
            asf.controllInitializeSize(this);
            CurrentRepice = 1;
        }
        private void ParmSetForm_Load(object sender, EventArgs e)
        {
            ControlsInitinal();
            dgvParam.CellEndEdit += dgvParam_CellEndEdit;
            tbxModelPathA01.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathA02.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathA03.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathA04.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathA05.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathA06.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathB01.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathB02.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathB03.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathB04.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathB05.DoubleClick += tbxModelPathXXX_DoubleClick;
            tbxModelPathB06.DoubleClick += tbxModelPathXXX_DoubleClick;
        }


        //初始化基础参数页面控件显示
        public void ControlsInitinal()
        {
            try
            {
                //***************************************************************************************************
                if (Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark==true)
                {
                    paramProductFileA = Global.RecipePath + "SBProductParamA.json";
                    paramProductFileB = Global.RecipePath + "SBProductParamB.json";
                }
                else
                {
                    paramProductFileA = Global.RecipePath + "CircProductParamA.json";
                    paramProductFileB = Global.RecipePath + "CircProductParamB.json";
                }
                //paramProductFileA = Global.RecipePath + "ProductParamA.json";
                //paramProductFileB = Global.RecipePath + "ProductParamB.json";

                paramImageFileA = Global.RecipePath + "ImageParamA.json";
                paramImageFileB = Global.RecipePath + "ImageParamB.json";

                rbn01.Checked = true;//将生产参数A显示在表格内   

                //***************************************************************************************************

                recipeParmPath.Text = Global.GetParamDir();// 将参数保存路径名称显示在地址框
                TbxImgSaveFoloder.Text = Global.myParams.ImageSaveDir;
                // A - 图像保存参数
                cbxSaveOrnImg_A.Checked = Global.myParams.sideParamA.imgSaveParam.isSaveOrnImg;
                cbxSaveOkRenImg_A.Checked = Global.myParams.sideParamA.imgSaveParam.isSaveOkRenImg;
                cbxSaveNgRenImg_A.Checked = Global.myParams.sideParamA.imgSaveParam.isSaveNgRenImg;
                nbxSaveOrnImgDays_A.Value = Global.myParams.sideParamA.imgSaveParam.saveOrnImgDays;
                nbxSaveRenImgDays_A.Value = Global.myParams.sideParamA.imgSaveParam.saveRenImgDays;

                cmbImgFormat_A.SelectedIndex = (Global.myParams.sideParamA.imgSaveParam.format == "jpeg") ? 0 : 1;
                nbx_imgRadio_A.Value = Global.myParams.sideParamA.imgSaveParam.radio;
                Squradio.Checked = Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark;
                Cirradio.Checked = Global.myParams.sideParamA.imgSaveParam.isCirWeldMark;
                // B - 图像保存参数
                cbxSaveOrnImg_B.Checked = Global.myParams.sideParamB.imgSaveParam.isSaveOrnImg;
                cbxSaveOkRenImg_B.Checked = Global.myParams.sideParamB.imgSaveParam.isSaveOkRenImg;
                cbxSaveNgRenImg_B.Checked = Global.myParams.sideParamB.imgSaveParam.isSaveNgRenImg;
                nbxSaveOrnImgDays_B.Value = Global.myParams.sideParamB.imgSaveParam.saveOrnImgDays;
                nbxSaveRenImgDays_B.Value = Global.myParams.sideParamB.imgSaveParam.saveRenImgDays;
                PlaneFit3D.Checked = Global.myParams.sideParamB.imgSaveParam.isPlanecheck;

                nbxRowNum.Value = Global.myParams.sideParamA.inspectOrders[(int)(nbxRepiceNum.Value - 1)].row;
                nbxColNum.Value = Global.myParams.sideParamA.inspectOrders[(int)(nbxRepiceNum.Value - 1)].col;

                //A - 检测方法选择
                CbxNormalCheck2d.Checked = Global.myParams.sideParamA.isNormalCheck;
                CbxAiCheck2d.Checked = Global.myParams.sideParamA.isAiCheck;
                //B - 检测方法选择
                CbxNormalCheck3d.Checked = Global.myParams.sideParamB.isNormalCheck;
                CbxAiCheck3d.Checked = Global.myParams.sideParamB.isAiCheck;

                //深度学习模型路径
                tbxModelPathA01.Text = Global.myParams.sideParamA.detModelPath[0];
                tbxModelPathA02.Text = Global.myParams.sideParamA.detModelPath[1];
                tbxModelPathA03.Text = Global.myParams.sideParamA.detModelPath[2];
                tbxModelPathA04.Text = Global.myParams.sideParamA.segModelPath[0];
                tbxModelPathA05.Text = Global.myParams.sideParamA.segModelPath[1];
                tbxModelPathA06.Text = Global.myParams.sideParamA.segModelPath[2];

                tbxModelPathB01.Text = Global.myParams.sideParamB.detModelPath[0];
                tbxModelPathB02.Text = Global.myParams.sideParamB.detModelPath[1];
                tbxModelPathB03.Text = Global.myParams.sideParamB.detModelPath[2];
                tbxModelPathB04.Text = Global.myParams.sideParamB.segModelPath[0];
                tbxModelPathB05.Text = Global.myParams.sideParamB.segModelPath[1];
                tbxModelPathB06.Text = Global.myParams.sideParamB.segModelPath[2];
                nbxRepiceNum.Value = CurrentRepice;
                //pbxDiagram.Image = Image.FromFile(@"./BatteryMoudleDiagram.jpg");
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数读取出错！\r\n" + ex.Message);
            }
            finally
            { }
        }

        //保存参数配方
        private void BtnRecipeSave_Click(object sender, EventArgs e)
        {
            try
            {
                //保存 DataGridView 参数
                if (isDgvValueChanged && rbn01.Checked == true)//
                {
                    SaveCsvData(paramProductFileA, dgvParam);
                }
                else if (isDgvValueChanged && rbn02.Checked == true)
                {
                    SaveCsvData(paramProductFileB, dgvParam);
                }
                else if (isDgvValueChanged && rbn03.Checked == true)
                {
                    SaveCsvData(paramImageFileA, dgvParam);
                }
                else if (isDgvValueChanged && rbn04.Checked == true)
                {
                    SaveCsvData(paramImageFileB, dgvParam);
                }

                //********************************************************************************************************
                Global.myParams.ImageSaveDir = TbxImgSaveFoloder.Text;
                // A - 图像保存参数
                Global.myParams.sideParamA.imgSaveParam.isSaveOrnImg = cbxSaveOrnImg_A.Checked;
                Global.myParams.sideParamA.imgSaveParam.isSaveOkRenImg = cbxSaveOkRenImg_A.Checked;
                Global.myParams.sideParamA.imgSaveParam.isSaveNgRenImg = cbxSaveNgRenImg_A.Checked;
                Global.myParams.sideParamA.imgSaveParam.saveOrnImgDays = (int)nbxSaveOrnImgDays_A.Value;
                Global.myParams.sideParamA.imgSaveParam.saveRenImgDays = (int)nbxSaveRenImgDays_A.Value;

                Global.myParams.sideParamA.imgSaveParam.format = cmbImgFormat_A.SelectedItem.ToString();
                Global.myParams.sideParamA.imgSaveParam.radio = (int)nbx_imgRadio_A.Value;
                Global.myParams.sideParamA.imgSaveParam.isSquareBarWeldMark = Squradio.Checked;
                Global.myParams.sideParamA.imgSaveParam.isCirWeldMark = Cirradio.Checked;
                // B - 图像保存参数
                Global.myParams.sideParamB.imgSaveParam.isSaveOrnImg = cbxSaveOrnImg_B.Checked;
                Global.myParams.sideParamB.imgSaveParam.isSaveOkRenImg = cbxSaveOkRenImg_B.Checked;
                Global.myParams.sideParamB.imgSaveParam.isSaveNgRenImg = cbxSaveNgRenImg_B.Checked;
                Global.myParams.sideParamB.imgSaveParam.saveOrnImgDays = (int)nbxSaveOrnImgDays_B.Value;
                Global.myParams.sideParamB.imgSaveParam.saveRenImgDays = (int)nbxSaveRenImgDays_B.Value;
                Global.myParams.sideParamB.imgSaveParam.isPlanecheck = PlaneFit3D.Checked;

                //序号保存
                Global.myParams.sideParamA.inspectOrders[(int)(nbxRepiceNum.Value - 1)].row = (int)nbxRowNum.Value;
                Global.myParams.sideParamA.inspectOrders[(int)(nbxRepiceNum.Value - 1)].col = (int)nbxColNum.Value;

                //A - 检测方法选择
                Global.myParams.sideParamA.isNormalCheck = CbxNormalCheck2d.Checked;
                Global.myParams.sideParamA.isAiCheck = CbxAiCheck2d.Checked;
                //B - 极柱检测顺序
                Global.myParams.sideParamB.isNormalCheck = CbxNormalCheck3d.Checked;
                Global.myParams.sideParamB.isAiCheck = CbxAiCheck3d.Checked;
                //深度学习模型路径
                //深度学习模型路径
                Global.myParams.sideParamA.detModelPath[0] = tbxModelPathA01.Text;
                Global.myParams.sideParamA.detModelPath[1] = tbxModelPathA02.Text;
                Global.myParams.sideParamA.detModelPath[2] = tbxModelPathA03.Text;
                Global.myParams.sideParamA.segModelPath[0] = tbxModelPathA04.Text;
                Global.myParams.sideParamA.segModelPath[1] = tbxModelPathA05.Text;
                Global.myParams.sideParamA.segModelPath[2] = tbxModelPathA06.Text;
                Global.myParams.sideParamB.detModelPath[0] = tbxModelPathB01.Text;
                Global.myParams.sideParamB.detModelPath[1] = tbxModelPathB02.Text;
                Global.myParams.sideParamB.detModelPath[2] = tbxModelPathB03.Text;
                Global.myParams.sideParamB.segModelPath[0] = tbxModelPathB04.Text;
                Global.myParams.sideParamB.segModelPath[1] = tbxModelPathB05.Text;
                Global.myParams.sideParamB.segModelPath[2] = tbxModelPathB06.Text;
                Draw();
                if (Global.WriteGlobalParams())
                {
                    Global.AddLog("参数保存成功！");
                    Thread.Sleep(100);
                    Global.ReadGlobalParams();
                    MessageBox.Show("参数保存成功！");
                }
                else
                {
                    Global.AddLog("参数保存失败！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数保存出错！\r\n" + ex.Message);
            }
        }

        //切换配方按钮
        private void BtnRecipeLoad_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog sfd = new FolderBrowserDialog();
                sfd.Description = "请选择配方参数保存文件夹，选择后请点击保存参数！";
                sfd.SelectedPath = Global.AppPath;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string fileDir = (sfd.SelectedPath).Substring(sfd.SelectedPath.LastIndexOf("\\") + 1);
                    Global.SaveParamDir(fileDir);//将新的参数路径名称更新保存至本地
                    Thread.Sleep(200);
                    Global.ReadGlobalParams();//读取本地参数                
                    recipeParmPath.Text = Global.GetParamDir();//将选择的路径显示在地址框
                    ControlsInitinal();//参数控件更新
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("配方参数更改出错！\r\n" + ex.Message.ToString());
            }
        }

        //区域设置窗体
        private void ShowRegionForm(string savePath)
        {
            SetRegion sr = new SetRegion(savePath);
            sr.ShowDialog();
        }
        //
        private void ShowModelForm(string side, string number, string regionPath, string modelPath, string metroPath)
        {
            SetModel sm = new SetModel(side, number, regionPath, modelPath, metroPath);
            sm.ShowDialog();
        }

        //卡尺设置窗体
        private void ShowMetroForm(string side, string savePath)
        {
            SetMetro sm = new SetMetro(side, savePath);
            sm.ShowDialog();
        }

        // 搜索匹配模型设置
        private void btnModelA_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbxModelA.SelectedIndex < 0)
                {
                    MessageBox.Show("请先选择要设置的匹配模板！");
                    return;
                }
                //设置兴趣区域、匹配模型和测量模型的保存路径
                string sideStr = "A";
                string indexNum = (cbxModelA.SelectedIndex + 1).ToString();
                //获取配方参数的保存路径，单个配方的各个参数共同保存在一个文件夹内
                string recipeSaveDir = Global.RecipePath;
                if (!Directory.Exists(recipeSaveDir))
                {
                    Directory.CreateDirectory(recipeSaveDir);
                }
                //分别设置兴趣区域、匹配模型、测量模型的保存名称
                string regionSavePath = recipeSaveDir + "Region" + sideStr + "_" + indexNum + ".hobj";
                string modelSavePath = recipeSaveDir + "Match" + sideStr + "_" + indexNum + ".shm";
                string metroSavePath = recipeSaveDir + "Metro" + sideStr + "_" + indexNum + ".mtr";

                ShowModelForm(sideStr, indexNum, regionSavePath, modelSavePath, metroSavePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("匹配模型设置出错！\r\n" + ex.Message.ToString());
            }
        }

        private void btnModelB_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbxModelB.SelectedIndex < 0)
                {
                    MessageBox.Show("请先选择要设置的匹配模板！");
                    return;
                }
                string sideStr = "B";
                string indexNum = (cbxModelB.SelectedIndex + 1).ToString();
                //获取配方参数的保存路径，单个配方的各个参数共同保存在一个文件夹内
                string recipeSaveDir = Global.RecipePath;
                if (!Directory.Exists(recipeSaveDir))
                {
                    Directory.CreateDirectory(recipeSaveDir);
                }
                //分别设置兴趣区域、匹配模型、测量模型的保存名称
                string regionSavePath = recipeSaveDir + "Region" + sideStr + "_" + indexNum + ".hobj";
                string modelSavePath = recipeSaveDir + "Match" + sideStr + "_" + indexNum + ".shm";
                string metroSavePath = recipeSaveDir + "Metro" + sideStr + "_" + indexNum + ".mtr";

                ShowModelForm(sideStr, indexNum, regionSavePath, modelSavePath, metroSavePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("匹配模型设置出错！\r\n" + ex.Message.ToString());
            }
        }

        private void BtnSelectImgSaveFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog sfd = new FolderBrowserDialog();
            sfd.Description = "修改图像保存文件夹，请选择并确定后保存参数！";
            sfd.SelectedPath = "E:\\Image\\";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                TbxImgSaveFoloder.Text = sfd.SelectedPath;
            }
        }

        ///*******************************************************************************************************************
        ///
        private void LoadCsvData(string csvPath, DataGridView dgv)
        {
            List<string[]> row_List = CSV_RW.ReadCSV(csvPath);

            while (dgv.Rows.Count > 0)
            {
                dgv.Rows.RemoveAt(0);
            }

            for (int i = 1; i < row_List.Count; i++)
            {
                int row = dgv.Rows.Add();
                for (int j = 0; j < row_List[i].Length; j++)
                {
                    dgv.Rows[row].Cells[j].Value = row_List[i][j];
                }
            }
        }
        private void LoadPoleOrder(int[] poleOrder, DataGridView dgv)
        {
            while (dgv.Rows.Count > 0)
            {
                dgv.Rows.RemoveAt(0);
            }

            for (int i = 0; i < poleOrder.Count(); i++)
            {
                int row = dgv.Rows.Add();

                dgv.Rows[row].Cells[0].Value = "PoleOrderSet";
                dgv.Rows[row].Cells[1].Value = "PoleNum" + (i + 1).ToString();
                dgv.Rows[row].Cells[2].Value = "极柱拍照序号对应的极柱号";
                dgv.Rows[row].Cells[3].Value = poleOrder[i];
            }
        }
        private void SaveCsvData(string csvPath, DataGridView dgv)
        {
            //当文件不存在时先创建文件并将每栏标题写入
            if (!File.Exists(csvPath))
            {
                try
                {
                    StreamWriter streamWriter = new StreamWriter(csvPath, false, Encoding.UTF8);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                catch
                {
                    MessageBox.Show("文件错误，请查验！");
                    return;
                }

                string[] head = new string[4];
                for (int i = 0; i < 4; i++)
                {
                    //将每栏标题先写入
                    head[i] = dgv.Columns[i].HeaderText.ToString();
                }
                //创建一个List 将所有的内容都装入其中
                List<string[]> dataRows = new List<string[]>();
                //添加每一行的内容
                dataRows.Add(head);
                //保存到CSV当中
                CSV_RW.WriteCSV(csvPath, dataRows, false);
            }

            int rowNum = dgv.RowCount;
            //创建一个List 将所有的内容都装入其中
            List<string[]> rows = new List<string[]>();
            //将标题加入
            string[] colHead = new string[4];
            for (int n = 0; n < 4; n++)
            {
                colHead[n] = dgv.Columns[n].HeaderText.ToString();
            }
            //添加每一行的内容
            rows.Add(colHead);


            for (int m = 0; m < rowNum; m++)
            {
                //将第一行的内容追加进入文件
                string[] row = new string[4];
                for (int n = 0; n < 4; n++)
                {
                    row[n] = dgv.Rows[m].Cells[n].Value.ToString();
                }
                //添加每一行的内容
                rows.Add(row);
            }

            //保存到CSV当中，追加模式
            CSV_RW.WriteCSV(csvPath, rows, false);
        }

        private void SavePoleOrder(int[] poleOrder, DataGridView dgv)
        {
            int rowNum = dgv.RowCount;

            for (int m = 0; m < rowNum; m++)
            {
                poleOrder[m] = int.Parse(dgv.Rows[m].Cells[3].Value.ToString());
            }
        }

        private void dgvParam_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBoxButtons msgButton = MessageBoxButtons.OKCancel;
            //DialogResult dr = MessageBox.Show("确定要退出吗?", "退出系统", msgButton);
            //if (dr == DialogResult.OK)
            //{
            //    MessageBox.Show("xxxx");
            //}
        }

        private void rbnXXX_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rbn = sender as RadioButton;

            if (rbn.Checked == false)
            {
                if (isDgvValueChanged == true)
                {
                    isDgvValueChanged = false;

                    MessageBoxButtons msgButton = MessageBoxButtons.OKCancel;
                    DialogResult dr = MessageBox.Show("表格参数已修改，离开前请确认是否保存?", "参数保存确认", msgButton);
                    if (dr == DialogResult.OK)
                    {
                        if (rbn.Name == "rbn01")//
                        {
                            SaveCsvData(paramProductFileA, dgvParam);
                            MessageBox.Show("参数保存成功！");
                        }
                        else if (rbn.Name == "rbn02")
                        {
                            SaveCsvData(paramProductFileB, dgvParam);
                            MessageBox.Show("参数保存成功！");
                        }
                        else if (rbn.Name == "rbn03")
                        {
                            SaveCsvData(paramImageFileA, dgvParam);
                            MessageBox.Show("参数保存成功！");
                        }
                        else if (rbn.Name == "rbn04")
                        {
                            SaveCsvData(paramImageFileB, dgvParam);
                            MessageBox.Show("参数保存成功！");
                        }

                    }
                }
                return;
            }

            dgvParam.CellValueChanged -= dgvParam_CellValueChanged;

            if (rbn.Name == "rbn01")//
            {
                LoadCsvData(paramProductFileA, dgvParam);
            }
            else if (rbn.Name == "rbn02")
            {
                LoadCsvData(paramProductFileB, dgvParam);
            }
            else if (rbn.Name == "rbn03")
            {
                LoadCsvData(paramImageFileA, dgvParam);
            }
            else if (rbn.Name == "rbn04")
            {
                LoadCsvData(paramImageFileB, dgvParam);
            }


            dgvParam.CellValueChanged += dgvParam_CellValueChanged;
        }

        private void dgvParam_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            isDgvValueChanged = true;
        }

        private void tabParamSet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabParamSet.SelectedTab != tabParam)
            {
                if (isDgvValueChanged == true)
                {
                    isDgvValueChanged = false;

                    MessageBoxButtons msgButton = MessageBoxButtons.OKCancel;
                    DialogResult dr = MessageBox.Show("表格参数已修改，离开前请确认是否保存?", "参数保存确认", msgButton);
                    if (dr == DialogResult.OK)
                    {
                        BtnRecipeSave_Click(null, null);
                    }
                }
            }
            if (tabParamSet.SelectedTab == tabDiagram)
            {
                nbxRepiceNum_ValueChanged(nbxRepiceNum, null);


            }
        }


        private void BtnSetModelPath_Click(object sender, EventArgs e)
        {
            try
            {
                string modelPath = "";

                FolderBrowserDialog sfd = new FolderBrowserDialog();
                sfd.Description = "选择AI模型文件夹，请选择后保存参数！";
                sfd.SelectedPath = "D:\\";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    modelPath = sfd.SelectedPath;
                }

                Button btn = (Button)sender;

                if (btn.Name.Contains("01"))
                {
                    tbxModelPathA01.Text = modelPath;
                }
                else if (btn.Name.Contains("02"))
                {
                    tbxModelPathA02.Text = modelPath;
                }
                else if (btn.Name.Contains("03"))
                {
                    tbxModelPathA03.Text = modelPath;
                }
                else if (btn.Name.Contains("04"))
                {
                    tbxModelPathB01.Text = modelPath;
                }
                else if (btn.Name.Contains("05"))
                {
                    tbxModelPathB02.Text = modelPath;
                }
                else if (btn.Name.Contains("06"))
                {
                    tbxModelPathB03.Text = modelPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("AI模型文件选择出错！\r\n" + ex.Message.ToString());
            }
        }

        private void tbxModelPathXXX_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                //FolderBrowserDialog sfd = new FolderBrowserDialog();
                //sfd.Description = "选择AI模型文件夹，请选择后保存参数！";
                //sfd.SelectedPath = "D:\\";
                //if (sfd.ShowDialog() == DialogResult.OK)
                //{
                //    TextBox tbx = (TextBox)sender;
                //    tbx.Text = sfd.SelectedPath;
                //}

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "";
                ofd.Filter = "";
                ofd.InitialDirectory = "";
                ofd.RestoreDirectory = true;
                ofd.Multiselect = false;
                ofd.Filter = @"All Model Files(*.mm)|*.mm|
                               All files(*.*) | *.* ";

                if (DialogResult.OK == ofd.ShowDialog(this))
                {
                    TextBox tbx = (TextBox)sender;
                    tbx.Text = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("AI模型文件选择出错！\r\n" + ex.Message.ToString());
            }
        }

        private void btnSetModelPath01_Resize(object sender, EventArgs e)
        {

        }

        private void ParmSetForm_Resize(object sender, EventArgs e)
        {
            if (this.Size == this.MinimumSize)
                return;
            asf.controlAutoSize(this);
        }




        private Bitmap canvasBitmap; // 内存中的画布

        private void Draw()
        {

            float width = pbxDiagram.Width;
            float height = pbxDiagram.Height;
            canvasBitmap = new Bitmap((int)width, (int)height);

            InspectOrder orders = Global.myParams.sideParamA.inspectOrders[(int)(nbxRepiceNum.Value - 1)];
            int row = orders.row;
            int col = orders.col;
            List<Point2f> points = new List<Point2f>();

            float xstep = (width - 100) / col;
            float ystep = (height - 100) / row;
            if (row - col == 0) return;

            List<int> order = new List<int>();

            float m = 0;
            for (int j = 0; j < row; j++)
            {
                int mdiff = (int)(Math.Abs(orders.end[j] - orders.start[j])) / (col - 1);
                if (orders.end[j] - orders.start[j] < 0)
                    mdiff = -mdiff;


                for (int i = 0; i < col; i++)
                {
                    if (j % 2 == 0)
                    {
                        m = xstep * i + 50;
                        points.Add(new Point2f(m, ystep * j + 50));
                        order.Add((int)orders.start[j] + mdiff * i);
                    }
                    else
                    {
                        points.Add(new Point2f(m - (xstep * i), ystep * j + 50));
                        order.Add((int)orders.start[j] + mdiff * i);
                    }

                }


            }
            using (Graphics g = Graphics.FromImage(canvasBitmap))
            {
                g.Clear(Color.SkyBlue);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // 抗锯齿

                int n = 1;
                int radius = (int)(xstep / 1.5);
                foreach (var item in points)
                {
                    Brush brush = Brushes.Blue; // 圆的填充颜色
                    Pen pen = new Pen(Color.Black, 1); // 圆的边框颜色和宽度
                    Rectangle rect = new Rectangle((int)item.X, (int)item.Y, radius, radius);

                    g.DrawEllipse(pen, rect);
                    g.DrawString("拍照位" + (n), new Font("Arial", (int)radius / 7, FontStyle.Bold), Brushes.Red, item.X + radius / 8, item.Y + radius / 4);

                    g.DrawString("极柱" + order[n - 1], new Font("Arial", (int)radius / 7, FontStyle.Bold), Brushes.Blue, item.X + radius / 4, item.Y + radius / 2);

                    if (n < row * col)
                    {
                        DrawArrowLine(g, new System.Drawing.Point((int)item.X, (int)item.Y), new System.Drawing.Point((int)points[n].X, (int)points[n].Y), radius / 2);
                    }


                    n++;
                }
            }
            pbxDiagram.Image = canvasBitmap; // 显示画布

        }

        private void DrawArrowLine(Graphics g, System.Drawing.Point startPoint, System.Drawing.Point endPoint, float radius)
        {
            using (Pen pen = new Pen(Color.Black, 2))
            {
                // 绘制直线部分

                // 计算箭头的位置和大小
                float length = 10; // 箭头长度
                System.Drawing.Point arrowTip = endPoint;
                System.Drawing.Point arrow1 = new System.Drawing.Point(arrowTip.X - (int)(length / 2), arrowTip.Y + (int)(length / Math.Sqrt(3)));
                System.Drawing.Point arrow2 = new System.Drawing.Point(arrowTip.X + (int)(length / 2), arrowTip.Y + (int)(length / Math.Sqrt(3)));

                if (endPoint.X > startPoint.X)
                {
                    startPoint = new System.Drawing.Point((int)(startPoint.X + radius * 2), (int)(startPoint.Y + radius));
                    endPoint = new System.Drawing.Point((int)(endPoint.X), (int)(endPoint.Y + radius));
                    arrowTip = endPoint;
                    arrow1 = new System.Drawing.Point(arrowTip.X - (int)(length / Math.Sqrt(3)), arrowTip.Y - (int)(length / 2));
                    arrow2 = new System.Drawing.Point(arrowTip.X - (int)(length / Math.Sqrt(3)), arrowTip.Y + (int)(length / 2));

                }
                else if (endPoint.X == startPoint.X)
                {
                    startPoint = new System.Drawing.Point((int)(startPoint.X + radius), (int)(startPoint.Y + 2 * radius));
                    endPoint = new System.Drawing.Point((int)(endPoint.X + radius), (int)(endPoint.Y));
                    arrowTip = endPoint;
                    arrow1 = new System.Drawing.Point(arrowTip.X - (int)(length / 2), arrowTip.Y - (int)(length / Math.Sqrt(3)));
                    arrow2 = new System.Drawing.Point(arrowTip.X + (int)(length / 2), arrowTip.Y - (int)(length / Math.Sqrt(3)));
                }
                else if (endPoint.X < startPoint.X)
                {
                    startPoint = new System.Drawing.Point((int)(startPoint.X), (int)(startPoint.Y + radius));
                    endPoint = new System.Drawing.Point((int)(endPoint.X + 2 * radius), (int)(endPoint.Y + radius));
                    arrowTip = endPoint;
                    arrow1 = new System.Drawing.Point(arrowTip.X + (int)(length / Math.Sqrt(3)), arrowTip.Y - (int)(length / 2));
                    arrow2 = new System.Drawing.Point(arrowTip.X + (int)(length / Math.Sqrt(3)), arrowTip.Y + (int)(length / 2));
                }

                g.DrawLine(pen, startPoint, endPoint);

                // 绘制箭头头
                using (Brush brush = new SolidBrush(pen.Color))
                {
                    g.FillPolygon(brush, new[] { endPoint, arrow1, arrow2 });
                }
            }
        }

        private void nbxRepiceNum_ValueChanged(object sender, EventArgs e)
        {
            CurrentRepice = (int)nbxRepiceNum.Value;
            nbxColNum.Value = Global.myParams.sideParamA.inspectOrders[CurrentRepice - 1].col;
            nbxRowNum.Value = Global.myParams.sideParamA.inspectOrders[CurrentRepice - 1].row;

            nbxColSelect.Value = 1;
            nbxStartNum.Value = Global.myParams.sideParamA.inspectOrders[CurrentRepice - 1].start[0];
            nbxEndNum.Value = Global.myParams.sideParamA.inspectOrders[CurrentRepice - 1].end[0];

            Draw();
        }

        private void nbxColSelect_ValueChanged(object sender, EventArgs e)
        {
            CurrentRepice = (int)nbxRepiceNum.Value;
            nbxEndNum.Value = Global.myParams.sideParamA.inspectOrders[CurrentRepice - 1].end[(int)(nbxColSelect.Value - 1)];
            nbxStartNum.Value = Global.myParams.sideParamA.inspectOrders[CurrentRepice - 1].start[(int)(nbxColSelect.Value - 1)];
        }

        private void nbxStartNum_ValueChanged(object sender, EventArgs e)
        {
            CurrentRepice = (int)nbxRepiceNum.Value;

            Global.myParams.sideParamA.inspectOrders[CurrentRepice - 1].start[(int)(nbxColSelect.Value - 1)] = (int)nbxStartNum.Value;
        }

        private void nbxEndNum_ValueChanged(object sender, EventArgs e)
        {
            CurrentRepice = (int)nbxRepiceNum.Value;
            Global.myParams.sideParamA.inspectOrders[CurrentRepice - 1].end[(int)(nbxColSelect.Value - 1)] = (int)nbxEndNum.Value;
        }

        private void button7_Click(object sender, EventArgs e)
        {

            staform = new StatisticsForm();
            staform.Show();
        }
        //public void DisplyFrom()
        //{
        //    staform = new StatisticsForm();
        //    staform.Show();
        //}
    }
}

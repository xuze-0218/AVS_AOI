using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisionUserControls.PrimaryControls;
using System.Diagnostics;
using System.Threading;
using System.IO;
using VisionUserControls.SocketCommManage;
using VisionUserControls.AuthorityManage;

using MMDeploy;
using OpenCvSharp;
using TextBox = System.Windows.Forms.TextBox;
using System.Runtime.InteropServices;
using DevComponents.DotNetBar;
using System.Reflection;

namespace AVS
{
    public partial class MainForm : Form
    {
        private ButtonNavigate btnNagCurrentActived;//当前活动的导航按钮
        WatchForm watchFrm;
        WaitingForm waitingfrm;
        SplashScreenManager waiting;
        private static string currentDate;
        private static string currentHour;
        public static HDevEngine hEngine;           //定义调用hdev程序的引擎，可以设置和传入参数的全局变量
        private static AutoSizeFormClass asf = new AutoSizeFormClass();
        [DllImport("user32.dll")] // 调用 Windows API，用于实现窗口拖拽功能 user32.dll 是 Windows 的用户界面库
        private static extern int ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern bool SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private bool isDragging = false;
        private System.Drawing.Point startPoint;
        public MainForm()
        {
            //避免软件重启
            if (Process.GetProcessesByName("AVS").ToList().Count > 1)
            {
                MessageBox.Show("视觉程序已启动,请勿重新启动！");
                System.Environment.Exit(0);
            }
            InitializeComponent();
            // 获取程序集版本并设置窗口标题
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            // 设置程序名称标签，自动获取并显示版本号
            this.lblProgramName.Text = $"焊后检测 V{version.Major}.{version.Minor}.{version.Build}";
            this.WindowState = FormWindowState.Maximized;
            asf.controllInitializeSize(this);
            // 注册鼠标事件
            this.MouseDown += MainForm_MouseDown;
            this.MouseMove += MainForm_MouseMove;
            this.MouseUp += MainForm_MouseUp;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                //系统初始化
                Global.SystemInit();
                Global.AddLog = logText.AddLog;
                //控件初始化
                timeDispSys.SysTimer.Start();       //时间显示控件
                logText.LogFilePath = Global.LogPath;
                logText.Active = true;              //日志显示、保存控件
                authorityManage.Enabled = true;
                authorityManage.loginEventHandler = new AuthorityManage.LoginEventHandler(refreshControlEnable);

                //组件初始化
                formsManage.MainCtrl = this;        //form管理组件
                formsManage.Active = true;

                //窗口绑定按钮
                btnNagWatchForm.FormConnect = typeof(WatchForm);
                btnNagDeviceManage.FormConnect = typeof(DevicesForm);
                btnNagParmSet.FormConnect = typeof(ParmSetForm);

                //初始化界面
                btnNavigateForm_Click(btnNagWatchForm, null);
                watchFrm = (WatchForm)formsManage.GetFrm(btnNagWatchForm.FormConnect);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.ToString());
            }
            try
            {
                //设备初始化
                Tcp.TcpInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.ToString());
            }
            try
            {     
                CameraManage.CameraManageInit();                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.ToString());
            }
            try
            {
                //线程初始化
                HWindowControl[] windowName = { watchFrm.GetHWindow("A"), watchFrm.GetHWindow("B"), watchFrm.GetHWindow("C"), watchFrm.GetHWindow("D") };
                ThreadProcess.Init(windowName);

                //HalconEngine 初始化
                hEngine = new HDevEngine();
                hEngine.SetProcedurePath(Global.RecipePath);
                hEngine.StartDebugServer(); //开启调试服务器

                //启动程序
                OnNavigateEventClick(btnNagStartStop, null);
                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.ToString());
            }
            try
            {
                if (Global.myParams.sideParamA.isAiCheck == true)
                {
                    //-A,2d检测的模型加载
                    string sideStr = "A";
                    string[] detModelPath = new string[]
                    {
                        Global.myParams.sideParamA.detModelPath[0],
                    };
                    bool isDetModelLoad = AiDrive.LoadAiDetModel(sideStr, detModelPath);
                    Global.AddLog(sideStr + "_Ai DetModel Loaded: " + isDetModelLoad.ToString());        //日志记录

                    string[] segModelPath = new string[]
                    {
                        Global.myParams.sideParamA.segModelPath[0],
                        Global.myParams.sideParamA.segModelPath[1],
                        Global.myParams.sideParamA.segModelPath[2],
                    };

                    bool isSegModelLoad = AiDrive.LoadAiSegModel(sideStr, segModelPath);
                    Global.AddLog(sideStr + "_Ai SegModel Loaded: " + isSegModelLoad.ToString());        //日志记录
                }
                if (Global.myParams.sideParamB.isAiCheck == true)
                {
                    //-A,3d检测的模型加载
                    string sideStr = "B";
                    string[] detModelPath = new string[]
                    {
                        Global.myParams.sideParamB.detModelPath[0],
                    };
                    bool isDetModelLoad = AiDrive.LoadAiDetModel(sideStr, detModelPath);
                    Global.AddLog(sideStr + "_Ai DetModel Loaded: " + isDetModelLoad.ToString());        //日志记录

                    string[] segModelPath = new string[]
                    {
                        Global.myParams.sideParamB.segModelPath[0],
                        Global.myParams.sideParamB.segModelPath[1],
                        Global.myParams.sideParamB.segModelPath[2],
                    };

                    bool isSegModelLoad = AiDrive.LoadAiSegModel(sideStr, segModelPath);
                    Global.AddLog(sideStr + "_Ai SegModel Loaded: " + isSegModelLoad.ToString());        //日志记录
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.ToString());
            }
        }

        //窗口切换按钮事件
        private void btnNavigateForm_Click(object sender, EventArgs e)
        {
            try
            {
                btnNagCurrentActived = sender as ButtonNavigate;

                //执行按钮对应的事件
                OnNavigateFormClick(sender, e);

                //更改按钮颜色
                if (btnNagCurrentActived.BackColor != btnNagCurrentActived.SelectedBackColor)
                {
                    btnNagCurrentActived.BackColor = btnNagCurrentActived.SelectedBackColor;
                    btnNagCurrentActived.ForeColor = btnNagCurrentActived.SelectedForeColor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.ToString());
            }
        }
        //窗口切换事件
        public void OnNavigateFormClick(object sender, EventArgs e)
        {
            //关闭历史画面,若为WatchForm则隐藏，若为其他Form，为了释放资源，直接关闭
            pnlMiddle.Controls.Remove(formsManage.FrmCurrentActived);
            if (formsManage.FrmCurrentActived != null && formsManage.FrmCurrentActived.GetType() == typeof(WatchForm))
                formsManage.FrmCurrentActived.Visible = false;
            else if (formsManage.FrmCurrentActived != null)
                formsManage.FrmCurrentActived.Close();

            //显示当前选择的画面
            Type frmtype = (sender as ButtonNavigate).FormConnect;
            formsManage.FrmCurrentActived = formsManage.GetFrm(frmtype) as Form;
            if (formsManage.FrmCurrentActived == null)
                return;
            if (frmtype != typeof(WatchForm))
            {
                //btnNagWatchForm.Visible = true;
                btnNagWatchForm.Enabled = true;
                formsManage.FrmCurrentActived = Activator.CreateInstance(frmtype) as Form;//因每次被close,所以需要实例
            }
            else
                //btnNagWatchForm.Visible = false;
                btnNagWatchForm.Enabled = false;
            pnlMiddle.Controls.Remove(formsManage.FrmCurrentActived);
           
            formsManage.FrmCurrentActived.TopLevel = false;
            formsManage.FrmCurrentActived.Size = new System.Drawing.Size(pnlMiddle.Width, pnlMiddle.Height);
            pnlMiddle.Controls.Add(formsManage.FrmCurrentActived);

            formsManage.FrmCurrentActived.Show();
            
            //刷新各控件使能
            refreshControlEnable();
        }

        //启动、停止、关闭等按钮事件
        private void btnNavigateEvent_Click(object sender, EventArgs e)
        {
            
            try
            {
                btnNagCurrentActived = sender as ButtonNavigate;
                OnNavigateEventClick(btnNagCurrentActived, e);
                //更改按钮颜色
                if (btnNagCurrentActived.BackColor != btnNagCurrentActived.SelectedBackColor)
                {
                    btnNagCurrentActived.BackColor = btnNagCurrentActived.SelectedBackColor;
                    btnNagCurrentActived.ForeColor = btnNagCurrentActived.SelectedForeColor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        public void OnNavigateEventClick(ButtonNavigate sender, EventArgs e)
        {
            string msg = sender.Text.Replace(" ", "");
            //waitingfrm = new WaitingForm(this, "程序" + msg + "中,请稍后...");
            //waiting = new SplashScreenManager(waitingfrm);
            //waiting.ShowWaiting();
            //根据按键名称选择事件
            switch (sender.Text)
            {
                case "启   动":
                    OnNavigateFormClick(btnNagWatchForm, null);//切回观察界面
                    StartProgram("A");                 
                    StartProgram("C");
                    if (Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck)
                    {
                        StartProgram("B"); 
                        StartProgram("D");
                    }
                        
                    tmrUpdateUI.Start();    // 显示界面开始刷新  
                    sender.Text = "停   止";
                    //this.btnNagStartStop.Enabled = false;
                    //sender.Image = Properties.Resources.Stop;
                    break;
                case "停   止":
                    
                    StopProgram("A");
                    StopProgram("C");
                    if (Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck)
                    {
                        StopProgram("B");
                        StopProgram("D");
                    }
                                       
                    sender.Text = "启   动";
                    //this.btnNagStartStop.Enabled = true;
                    //sender.Image = Properties.Resources.Start;
                    hEngine.UnloadAllProcedures();
                    break;
                case "×":
                    StopProgram("A");
                    StopProgram("C");
                    ExitProgram("A");
                    ExitProgram("C");
                    if (Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck)
                    {
                        StopProgram("B");
                        StopProgram("D");
                        ExitProgram("B");
                        ExitProgram("D");
                    }                                 
                    this.Close();
                    break;
                //最小化窗口
                case "—":
                    this.WindowState = FormWindowState.Minimized;
                    break;
                //最大化窗口
                case "口":
                    if (this.WindowState == FormWindowState.Maximized)
                        this.WindowState = FormWindowState.Normal;
                    else if (this.WindowState == FormWindowState.Normal)                   
                        this.WindowState = FormWindowState.Maximized;
                    break;
                //显示帮助窗口
                case "":
                    AboutForm frm = new AboutForm();
                    frm.ShowDialog();
                    break;
                default:
                    break;
            }
            //刷新各控件使能
            refreshControlEnable();
            //waiting.CloseWaitForm();
        }

        //启动程序
        public void StartProgram(string id)
        {
            if (id == "A")
            {
                CameraManage.ConnectCameras(id);            //连接相机
                Tcp.ConnectToPLC(id);                       //连接PLC
                ThreadProcess.StartProcess(id);             //开启检测线程
            }
            if (id == "B")
            {
                CameraManage.ConnectCameras(id);          //连接相机
                Tcp.ConnectToPLC(id);                     //连接PLC
                ThreadProcess.StartProcess(id);             //开启检测线程
            }
        }

        //停止程序
        public void StopProgram(string id)
        {
            //A,B线程连接相机和TCP，C、D线程用于处理图像
            if (id == "A")
            {
                CameraManage.DisConnectCameras(id);         //断开相机
                Tcp.DisConnectToPLC(id);                    //断开PLC
                ThreadProcess.StopProcess(id);              //停止检测线程
            }
            if (id == "B")
            {
                CameraManage.DisConnectCameras(id);       //断开相机
                Tcp.DisConnectToPLC(id);                  //断开PLC
                ThreadProcess.StopProcess(id);              //停止检测线程
            }
        }
        //退出程序
        public void ExitProgram(string id)
        {
            //A,B线程连接相机和TCP，C、D线程用于处理图像
            if (id == "A")
            {
                ThreadProcess.ExitProcess(id);              //释放线程
            }
            if (id == "B")
            {
                ThreadProcess.ExitProcess(id);              //释放线程
            }
        }
        //根据权限刷新控件使能
        private void refreshControlEnable()
        {
            if (formsManage.FrmCurrentActived.GetType() == typeof(DevicesForm))
            {
                SetControlEnable(formsManage.FrmCurrentActived.Controls);
            }
            if (formsManage.FrmCurrentActived.GetType() == typeof(ParmSetForm))
            {
                SetControlEnable(formsManage.FrmCurrentActived.Controls);
                SetDataGridViewEnable(formsManage.FrmCurrentActived.Controls);
            }

            if (formsManage.FrmCurrentActived.GetType() == typeof(WatchForm))
            {
                SetWatchFormEnable(formsManage.FrmCurrentActived.Controls);
            }
        }
        private void SetControlEnable(Control.ControlCollection ctl)
        {
            foreach (Control c in ctl)
            {
                if (c.HasChildren)
                    SetControlEnable(c.Controls);
                else if (c is Button || c is TextBox || c is ComboBox)
                {
                    if ((btnNagStartStop.Text == "启   动" && authorityManage.authority.CurrentLevel > Authority.AuthorityLevel.lvlOperator)||c.Text== "数据统计")
                        c.Enabled = true;
                    else
                        c.Enabled = false;
                }
            }
        }

        private void SetWatchFormEnable(Control.ControlCollection ctl)
        {
            foreach (Control c in ctl)
            {
                if (c.HasChildren)
                    SetWatchFormEnable(c.Controls);
                else if (c is Button )
                {
                    if (btnNagStartStop.Text == "启   动"/* && authorityManage.authority.CurrentLevel > Authority.AuthorityLevel.lvlOperator*/)
                        c.Enabled = true;
                    else if (c.Name == "btnOKErrorProof2d" || c.Name == "btnOKErrorProof3d")
                        c.Enabled = true;
                    else
                        c.Enabled = false;
                }
            }
        }

        private void SetDataGridViewEnable(Control.ControlCollection ctl)
        {
            foreach (Control c in ctl)
            {
                if (c is DataGridView)
                {
                    if (btnNagStartStop.Text == "启   动" && authorityManage.authority.CurrentLevel > Authority.AuthorityLevel.lvlOperator)
                    {
                        DataGridView dgv = c as DataGridView;
                        dgv.Columns[3].ReadOnly = false;
                        dgv.Columns[3].DefaultCellStyle.BackColor = false ? Color.White : Color.LimeGreen;
                    }
                    else
                    {
                        DataGridView dgv = c as DataGridView;
                        dgv.Columns[3].ReadOnly = true;
                        dgv.Columns[3].DefaultCellStyle.BackColor = true ? Color.White : Color.LimeGreen;
                    }
                }
                if (c.HasChildren)
                    SetDataGridViewEnable(c.Controls);
            }
        }
        //更新界面显示，检测结果，设备连接状态
        private void tmrUpdateUI_Tick(object sender, EventArgs e)
        {
            #region 设备状态更新
            #region A 窗口状态
            //PLC状态
            if (Tcp.clientSocketA == null || !Tcp.clientSocketA.Connected)
            {
                watchFrm.txtPLCAState.Text = "×";
                watchFrm.txtPLCAState.BackColor = Color.Red;
            }
            else
            {
                watchFrm.txtPLCAState.Text = "√";
                watchFrm.txtPLCAState.BackColor = Color.LimeGreen;
            }
            //相机状态
            if (CameraManage.camera != null && CameraManage.camera.IsConnect)
            {
                watchFrm.txtCamAState.Text = "√";
                watchFrm.txtCamAState.BackColor = Color.LimeGreen;
            }
            else
            {
                watchFrm.txtCamAState.Text = "×";
                watchFrm.txtCamAState.BackColor = Color.Red;
            }
            #endregion

            #region B 窗口状态            
            //PLC状态
            if (Tcp.clientSocketB == null || !Tcp.clientSocketB.Connected)
            {
                watchFrm.txtPLCBState.Text = "×";
                watchFrm.txtPLCBState.BackColor = Color.Red;
            }
            else
            {
                watchFrm.txtPLCBState.Text = "√";
                watchFrm.txtPLCBState.BackColor = Color.LimeGreen;
            }
            //相机状态
            if (CameraManage.isLMICamConnect)
            {
                watchFrm.txtCamBState.Text = "√";
                watchFrm.txtCamBState.BackColor = Color.LimeGreen;
            }
            else
            {
                watchFrm.txtCamBState.Text = "×";
                watchFrm.txtCamBState.BackColor = Color.Red;
            }
            #endregion            
            #endregion  
            SysTimer_Tick(null, null);
        }

        private static void SysTimer_Tick(object sender, EventArgs e)
        {
            //每天检查是否有过期图像文件，如果有则删除
            if (currentDate != DateTime.Now.Date.ToString())
            {
                //Task.Run(() => { DeleteImgFile(); });
                Thread thDeleteF = new Thread(DeleteImgFile);
                thDeleteF.IsBackground = true;
                thDeleteF.Priority = ThreadPriority.Lowest;
                thDeleteF.Start();
            }
            currentDate = DateTime.Now.Date.ToString();
            //每小时检查程序内存占用
            if (currentHour != DateTime.Now.Hour.ToString())
            {
                //Task.Run(() => { ClearMemory(); });
                Thread thDeleteF = new Thread(ClearMemory);
                thDeleteF.IsBackground = true;
                thDeleteF.Priority = ThreadPriority.Lowest;
                thDeleteF.Start();
            }
            currentHour = DateTime.Now.Hour.ToString();
        }
        private static void DeleteImgFile()
        {
            DeleteFile myDeleteFile = new DeleteFile();

            //删除过期的图片
            try
            {
                string imgSaveFolder = Global.myParams.ImageSaveDir;
                int saveDaysAO = Global.myParams.sideParamA.imgSaveParam.saveOrnImgDays;
                int saveDaysBO = Global.myParams.sideParamB.imgSaveParam.saveOrnImgDays;
                int saveDaysAW = Global.myParams.sideParamA.imgSaveParam.saveRenImgDays;
                int saveDaysBW = Global.myParams.sideParamB.imgSaveParam.saveRenImgDays;

                myDeleteFile.DeleteOverTimeFiles(imgSaveFolder , saveDaysAO, "Originallmage", "2D");
                myDeleteFile.DeleteOverTimeFiles(imgSaveFolder , saveDaysAW, "ResultImage", "2D");
                if (Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck)
                {
                    myDeleteFile.DeleteOverTimeFiles(imgSaveFolder, saveDaysBO, "Originallmage", "3D");
                    myDeleteFile.DeleteOverTimeFiles(imgSaveFolder, saveDaysBW, "ResultImage", "3D");
                }

            }
            catch (Exception ex)
            {
                Global.AddLog("过期文件删除错误：" + ex.Message.ToString());
            }
            finally
            {
                myDeleteFile = null;
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        public static void ClearMemory()
        {
            //获得当前工作进程
            Process proc = Process.GetCurrentProcess();
            long usedMemory = proc.PrivateMemorySize64;
            if (usedMemory > 1024 * 1024 * 100)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                //if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                //{
                //    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                //}
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.Size == this.MinimumSize)
                return;

            if (this.Size.Width < 480) return;
            asf.controlAutoSize(this);
            if (formsManage.FrmCurrentActived != null)
                formsManage.FrmCurrentActived.Size = new System.Drawing.Size(pnlMiddle.Width, pnlMiddle.Height);
        }

        

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                System.Drawing.Point currentScreenPos = this.PointToScreen(e.Location);
                int newX = currentScreenPos.X - startPoint.X;
                int newY = currentScreenPos.Y - startPoint.Y;

                // 限制窗体位置
                if (newX < 0) newX = 0;
                if (newY < 0) newY = 0;
                if (newX > Screen.PrimaryScreen.Bounds.Width - this.Width)
                    newX = Screen.PrimaryScreen.Bounds.Width - this.Width;
                if (newY > Screen.PrimaryScreen.Bounds.Height - this.Height)
                    newY = Screen.PrimaryScreen.Bounds.Height - this.Height;
                this.Location = new System.Drawing.Point(newX, newY);
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }        
    }
}

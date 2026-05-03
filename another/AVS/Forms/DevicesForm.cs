using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisionDevelopLibrary.CameraClass;
using VisionDevelopLibrary.PrimaryClass;


namespace AVS
{
    public partial class DevicesForm : Form
    {
        string camSelected = "A";
        private HObject img = null;

        bool isContinueGrab = false;
        bool isImgSourceLocal = false;
        private static AutoSizeFormClass asf = new AutoSizeFormClass();

        public DevicesForm()
        {            
            InitializeComponent();
            asf.controllInitializeSize(this);
        }


        private void DevicesForm_Load(object sender, EventArgs e)
        {
            // A相机参数加载至界面
            if (Global.myParams.sideParamA.camParam.isImgSourceLocal == true)
            {
                RbnImgSource_Local.Checked = true;                
            }
            else
            {
                RbnImgSource_Cam.Checked = true;
            }
             
            if(Global.myParams.sideParamA.camParam.addressImg != null)
            {
                TbxImgPath.Text = Global.myParams.sideParamA.camParam.addressImg;
            }
            
            TbxCamIpAddress.Text = Global.myParams.sideParamA.camParam.addressIp;
            //NbxCamExposure.Value = (decimal)Global.myParams.sideParamA.camParam.exporsure;
            //NbxCamGain.Value = (decimal)Global.myParams.sideParamA.camParam.gain;

            cmbCameraSelected.SelectedIndex = Global.myParams.sideParamA.camParam.CameraKind;

            //TCP IP
            clientManage.clientMD = Global.cMD;
            clientManage.ClientManageInit();
            clientManage.ClientUpdate = Tcp.TcpInit;
            //CAM     
        }

        #region Camera
        private void BtnGrabContinue_Click(object sender, EventArgs e)
        {
            try
            {
                if(CameraManage.ConnectCameras(camSelected))//CameraManage.ConnectCameras(camSelected))
                {
                    isContinueGrab = true;
                    Task.Run(() => ContinueGrabImgAndShow(camSelected));
                    btnContinueGrabImg.Enabled = false;
                    btnGrabOneImg.Enabled = false;
                    btnStopGrapImg.Enabled = true; 
                    rbnCamSelectA.Enabled = false;
                    rbnCamSelectB.Enabled = false;
                    rbnCamSelectC.Enabled = false;
                    rbmCamSelectD.Enabled = false;
                }
                else
                {
                    isContinueGrab = false;
                    MessageBox.Show(camSelected +  "_相机连接失败!");
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(camSelected + "_相机采图出错：\r\n" + ex.Message.ToString());
            }            
        }

        private void BtnGrabStop_Click(object sender, EventArgs e)
        {
            try
            {
                btnContinueGrabImg.Enabled = true;
                btnGrabOneImg.Enabled = true;
                rbnCamSelectA.Enabled = true;
                rbnCamSelectB.Enabled = true;
                rbnCamSelectC.Enabled = true;
                rbmCamSelectD.Enabled = true;
                isContinueGrab = false;               
            }
            catch(Exception ex)
            {
                MessageBox.Show(camSelected + "_相机断开出错：\r\n" + ex.Message.ToString());
            }
        }
        #endregion

        // A - 相机单张取图
        private void BtnGrabOne_Click(object sender, EventArgs e)
        {
            try
            {
                HOperatorSet.GenEmptyObj(out img);
                if(CameraManage.ConnectCameras(camSelected))
                {
                    img = CameraManage.GrabImage(camSelected);
                    //img = Basler.GrabImage(camSelected);
                    if (!IsObjectEmpty(img))
                    {
                        img = img.Clone();
                        ImageShow(img, deviceHWindowA);
                    }
                    else
                    {
                        MessageBox.Show("采集图像为空！");
                    }
                    CameraManage.DisConnectCameras(camSelected);
                    //Basler.Close(camSelected);  
                }
                else
                {
                    MessageBox.Show(camSelected + "_相机连接失败！");
                }                                  
            }
            catch
            { }
        }
        
        private void BtnSaveImg_Click(object sender, EventArgs e)
        {
            if (img == null || IsObjectEmpty(img))
            {
                MessageBox.Show("图像为空，请先抓取一张图像再进行保存！");
                return;
            }
            else
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Image Files (*.bmp)|*.bmp";
                sfd.Title = "请选择图像保存路径！";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string fileName = sfd.FileName;
                    if (!fileName.Equals(null))
                    {
                        HOperatorSet.WriteImage(img, "bmp", 0, fileName);
                    }
                }
            }
        }
        private void BtnSaveCamSet_Click(object sender, EventArgs e)
        {
           try
            {
                CamParam camSet = new CamParam();
                camSet.isImgSourceLocal = isImgSourceLocal;
                camSet.addressImg = TbxImgPath.Text;
                camSet.addressIp = TbxCamIpAddress.Text;
                //camSet.exporsure = (int)NbxCamExposure.Value;
                //camSet.gain = (int)NbxCamGain.Value;
                camSet.CameraKind = cmbCameraSelected.SelectedIndex;
                switch (camSelected)
                {
                    case "A":
                        Global.myParams.sideParamA.camParam = camSet; 
                        break;
                    case "B":
                        Global.myParams.sideParamB.camParam = camSet;
                        break;
                    case "C":
                        //Global.myParams.sideParamC.camParam = camSet;
                        break;
                    case "D":
                        //Global.myParams.sideParamD.camParam = camSet;
                        break;
                }
                if (Global.WriteGlobalParams())
                {
                    Global.AddLog("参数保存成功！");
                    Thread.Sleep(300);
                    Global.ReadGlobalParams();
                }
                else
                {
                    Global.AddLog("参数保存失败！");
                }
                MessageBox.Show(camSelected + "_相机参数保存成功！");
            }
            catch(Exception ex)
            {
                MessageBox.Show("相机参数保存出错：\r\n" + ex.Message);    
            }
        }

        private void BtnSelectImg_Click(object sender, EventArgs e)
        {
            try
            {
                /*
               OpenFileDialog ofd = new OpenFileDialog();
               ofd.Multiselect = false;
               ofd.Filter = @"All Image Files|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.png;*.tif;*.tiff|
                              Windows Bitmap(*.bmp)|*.bmp|
                              Windows Icon(*.ico)|*.ico|
                              Graphics Interchange Format (*.gif)|(*.gif)|
                              JPEG File Interchange Format (*.jpg)|*.jpg;*.jpeg|
                              Portable Network Graphics (*.png)|*.png|
                              Tag Image File Format (*.tif)|*.tif;*.tiff";
               if (ofd.ShowDialog() == DialogResult.OK)
               {
                   HOperatorSet.ReadImage(out img, ofd.FileName);
                   ImageShow(img, deviceHWindowA);
                   TbxImgPath.Text = ofd.FileName;
               }
               */
                FolderBrowserDialog sfd = new FolderBrowserDialog();
                sfd.Description = "选择图像文件夹，请选择后保存参数！";
                sfd.SelectedPath = "E:\\Image\\";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    TbxImgPath.Text = sfd.SelectedPath;

                    HTuple imgFiles = null;
                    HOperatorSet.ListFiles(sfd.SelectedPath, (new HTuple("files")), out imgFiles);
                    HOperatorSet.TupleRegexpSelect(imgFiles, (new HTuple("\\.(tif|tiff|gif|bmp|jpg|jpeg|jp2|png|pcx|pgm|ppm|pbm|xwd|ima|hobj)$")).TupleConcat("ignore_case"), out imgFiles);

                    HOperatorSet.GenEmptyObj(out img);
                    if ((int)(new HTuple(imgFiles.TupleLength())) > 1)
                    {
                        HOperatorSet.GenEmptyObj(out img);
                        HOperatorSet.ReadImage(out img, imgFiles.TupleSelect(0));
                        ImageShow(img, deviceHWindowA);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("选择图像时出错：\r\n" + ex.Message.ToString());
            }           
        }

        //刷新按钮状态
        public void setControlEnable(object sender)
        {
            /*
            if (sender == null)
            {
                btnContinueGrabImg.Enabled = true;
                btnStopGrapImg.Enabled = false;
                foreach (Control c in clientManage.Controls)
                    if (c.HasChildren)
                        foreach (Control cChildren in c.Controls)
                            cChildren.Enabled = true;
                return;
            }
            */
        }

        public static void ImageShow(HObject img, HWindowControl hWindow)
        {
            if (IsObjectEmpty(img))
            { return; }

            HTuple row01 = new HTuple(), col01 = new HTuple(), row02 = new HTuple(), col02 = new HTuple();
            HTuple imgW = new HTuple(), imgH = new HTuple();
            HOperatorSet.GetImageSize(img, out imgW, out imgH);

            HTuple winW = hWindow.Width;
            HTuple winH = hWindow.Height;

            HTuple ScaleW = imgW / (winW * 1.0);
            HTuple ScaleH = imgH / (winH * 1.0);

            if (ScaleW >= ScaleH)
            {
                row01 = -(1.0) * ((winH * ScaleW) - imgH) / 2;
                col01 = 0;
                row02 = row01 + winH * ScaleW;
                col02 = col01 + winW * ScaleW;
            }
            else
            {
                row01 = 0;
                col01 = -(1.0) * ((winW * ScaleH) - imgW) / 2;
                row02 = row01 + winH * ScaleH;
                col02 = col01 + winW * ScaleH;
            }

            HOperatorSet.SetPart(hWindow.HalconWindow, row01, col01, row02, col02);
            HOperatorSet.ClearWindow(hWindow.HalconWindow);
            HOperatorSet.DispObj(img, hWindow.HalconWindow);
        }

        private static void LoadImage(out HObject img)
        {
            img = new HObject();
            bool loadImg;
            DialogResult dr = MessageBox.Show("是否手动选择一张图像?", "相机采图失败，请检查相关问题!", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.OK)
            {
                loadImg = true;
            }
            else
            {
                loadImg = false;
                img = null;
            }

            if (loadImg)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;
                //ofd.Filter = "(*.jpg,*.png,*.jpeg,*.bmp,*.gif)|*.jgp;*.png;*.jpeg;*.bmp;*.gif|All files(*.*)|*.*";
                ofd.Filter = @"All Image Files|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.png;*.tif;*.tiff|
                               Windows Bitmap(*.bmp)|*.bmp|
                               Windows Icon(*.ico)|*.ico|
                               Graphics Interchange Format (*.gif)|(*.gif)|
                               JPEG File Interchange Format (*.jpg)|*.jpg;*.jpeg|
                               Portable Network Graphics (*.png)|*.png|
                               Tag Image File Format (*.tif)|*.tif;*.tiff";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    HOperatorSet.ReadImage(out img, ofd.FileName);
                }
            }
        }

        public static bool IsObjectEmpty(HObject image)
        {
            //判断 图像是否为空, 为空时返回---true
            if (image == null)
                return true;

            try
            {
                HObject emptyImg = new HObject();
                HTuple isEqual = new HTuple();
                HOperatorSet.GenEmptyObj(out emptyImg);
                HOperatorSet.TestEqualObj(image, emptyImg, out isEqual);
                return (bool)isEqual;
            }
            catch
            {
                return true;
            }
        }

        private void ContinueGrabImgAndShow(string camNum)
        {
            while (isContinueGrab)
            {
                HOperatorSet.GenEmptyObj(out img);
                //img = CameraManage.GrabImage(camSelected);
                img = CameraManage.GrabImage(camSelected);
                if (!IsObjectEmpty(img))
                {
                    ImageShow(img, deviceHWindowA);
                }
                Thread.Sleep(200);  
            }
            if (CameraManage.DisConnectCameras(camSelected))//CameraManage.DisConnectCameras(camSelected))
            {
                MessageBox.Show(camSelected + "_相机关闭成功!");
            }
            else
            {
                MessageBox.Show(camSelected + "_相机关闭失败!");
            }
        }

        private void rbnCamSelect_CheckedChanged(object sender, EventArgs e)
        {
            BtnGrabStop_Click(null, null);//先停止连续取图
            CamParam camSet = new CamParam();    
            RadioButton rbn = (RadioButton)sender;  
            if(rbn.Checked == true)
            {
                string camChecked = rbn.Text;
                switch (camChecked)
                {
                    case "A":
                        camSelected = "A";
                        camSet = Global.myParams.sideParamA.camParam;                        
                        break;
                    case "B":
                        camSelected = "B";
                        camSet = Global.myParams.sideParamB.camParam;
                        break;
                    case "C":
                        camSelected = "C";
                        camSet = Global.myParams.sideParamC.camParam;
                        break;
                    case "D":
                        camSelected = "D";
                        camSet = Global.myParams.sideParamD.camParam;
                        break;
                    default:
                        break;
                }
                if (camSet.isImgSourceLocal == true)
                {
                    RbnImgSource_Local.Checked = true;
                }
                else
                {
                    RbnImgSource_Cam.Checked = true;
                }

                if (camSet.addressImg != null)
                {
                    TbxImgPath.Text = camSet.addressImg;
                }
                TbxCamIpAddress.Text = camSet.addressIp;
                //NbxCamExposure.Value = (decimal)camSet.exporsure;
                //NbxCamGain.Value = (decimal)camSet.gain;
            }
        }

        private void RbnImgSource_Cam_CheckedChanged(object sender, EventArgs e)
        {
            isImgSourceLocal = false;    
        }

        private void RbnImgSource_Local_CheckedChanged(object sender, EventArgs e)
        {
            isImgSourceLocal = true;
        }
        private void DevicesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if(isContinueGrab == true)
                {
                    BtnGrabStop_Click(null, null);  
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("相机关闭出错" + ex.Message.ToString());
            }
        }

        private void DevicesForm_Resize(object sender, EventArgs e)
        {
            if (this.Size == this.MinimumSize)
                return;
            asf.controlAutoSize(this);
        }
    }
}

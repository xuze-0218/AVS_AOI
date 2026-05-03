using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using AVS.Forms;
using DevComponents.DotNetBar;
using static OpenCvSharp.ML.LogisticRegression;

namespace AVS
{

    public partial class WatchForm : Form
    {
        private HWindowControl hWindow = null;
        private static AutoSizeFormClass asf = new AutoSizeFormClass();
        textForm textFrm;
        bool isWindow = true;
        public WatchForm()
        {
            InitializeComponent();
            asf.controllInitializeSize(this);
        }


        public HWindowControl GetHWindow(string windowID)
        {
            switch (windowID)
            {
                case "A":
                    hWindow = hWindowA;
                    break;
                case "B":
                    hWindow = hWindowB;
                    break;
                case "C":
                    hWindow = hWindowC;
                    break;
                case "D":
                    hWindow = hWindowD;
                    break;
                default:
                    break;
            }
            return hWindow;
        }

        private void WatchForm_Load(object sender, EventArgs e)
        {
            bool isCheckA = Global.myParams.sideParamA.isNormalCheck || Global.myParams.sideParamA.isAiCheck;
            bool isCheckB = Global.myParams.sideParamB.isNormalCheck || Global.myParams. sideParamB.isAiCheck;

            if (isCheckA && !isCheckB)
            {

                panel1.Size = new Size(this.Width, 43 * this.Height / 560);
                hWindowA.Size = new Size(this.Width / 2 - 2, this.Height);
                hWindowC.Size = new Size(this.Width / 2 - 2, this.Height);
                hWindowA.Location = new Point(1, 45 * this.Height / 560);
                hWindowC.Location = new Point(this.Width / 2, 45 * this.Height / 560);

                hWindowB.Location = new Point(1, 2000);
                hWindowD.Location = new Point(this.Width / 2, 2000);
            }
        }

        private void WatchForm_Resize(object sender, EventArgs e)
        {
            if (this.Size == this.MinimumSize)
                return;
            asf.controlAutoSize(this);

            bool isCheckA = Global.myParams.sideParamA.isNormalCheck || Global.myParams.sideParamA.isAiCheck;
            bool isCheckB = Global.myParams.sideParamB.isNormalCheck || Global.myParams.sideParamB.isAiCheck;
            if (isCheckA && !isCheckB)
            {

                panel1.Size = new Size(this.Width, 43 * this.Height / 560);
                hWindowA.Size = new Size(this.Width / 2 - 2, this.Height);
                hWindowC.Size = new Size(this.Width / 2 - 2, this.Height);
                hWindowA.Location = new Point(1, 45 * this.Height / 560);
                hWindowC.Location = new Point(this.Width / 2, 45 * this.Height / 560);
                hWindowB.Location = new Point(1, 2000);
                hWindowD.Location = new Point(this.Width / 2, 2000);
            }
        }

        private async void btnErrorProof_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Name == "btnOKErrorProof2d" && isWindow)
            {
                OfflineTest("A");
                isWindow = false;
            }
            else if (btn.Name == "btnOKErrorProof3d" && isWindow)
            {
                OfflineTest("B");
                isWindow = false;
            }
            await Task.Run(() =>
            {
                IsWindow();
                //ImageSaveProcess.initData();
                //Proof_Thread("D:\\post_weld_inspection\\图片\\2D\\阿特斯\\图片\\pack\\81001713;100824;20250723;01072;1_2\\焊后C0312\\OK\\2D检测\\2025_09_18\\Originallmage", "A", "OK");
            });
            //string sideStr = "";string retKind = "";
            //Button btn = sender as Button;
            //ImageSaveProcess.initData();
            //string filePath = null;
            //if (btn.Name == "btnOKErrorProof2d" || btn.Name == "btnNGErrorProof2d")
            //{
            //    sideStr = "A";
            //    btnNGErrorProof2d.Enabled = false;
            //    btnOKErrorProof2d.Enabled = false;
            //    retKind = (btn.Name == "btnOKErrorProof2d") ? "OK" : "NG";
            //    filePath = (btn.Name == "btnOKErrorProof2d") ? Global.myParams.sideParamA.imgSaveParam.okProofPath : Global.myParams.sideParamA.imgSaveParam.ngProofPath;
            //}
            //else if (btn.Name == "btnOKErrorProof3d" || btn.Name == "btnNGErrorProof3d")
            //{
            //    sideStr = "B";
            //    btnNGErrorProof3d.Enabled = false;
            //    btnOKErrorProof3d.Enabled = false;
            //    retKind = (btn.Name == "btnOKErrorProof3d") ? "OK" : "NG";
            //    filePath = (btn.Name == "btnOKErrorProof3d") ? Global.myParams.sideParamB.imgSaveParam.okProofPath : Global.myParams.sideParamB.imgSaveParam.ngProofPath;
            //}
            //await Task.Run(() => {
            //    //for (int i = 0; i < 100; i++)
            //    //{
            //        Proof_Thread(filePath, sideStr, retKind);
            //    //}

            //});
            //if (btn.Name == "btnOKErrorProof2d" || btn.Name == "btnNGErrorProof2d")
            //{
            //    btnNGErrorProof2d.Enabled = true;
            //    btnOKErrorProof2d.Enabled = true;
            //}
            //else if (btn.Name == "btnOKErrorProof3d" || btn.Name == "btnNGErrorProof3d")
            //{
            //    btnNGErrorProof3d.Enabled = true;
            //    btnOKErrorProof3d.Enabled = true;
            //}
        }
        private void IsWindow()
        {
            while (true)
            {
                if (textFrm.IsDisposed)
                {
                    isWindow = true;
                    break;
                }
            }
        }


        private void Proof_Thread(string imgFolder, string sideStr, string retKind)
        {
            HObject img = null;
            HTuple imgFiles = null, imgIndex = null;
            HOperatorSet.ListFiles(imgFolder, (new HTuple("files")).TupleConcat("follow_links"), out imgFiles);
            HOperatorSet.TupleRegexpSelect(imgFiles, (new HTuple("\\.(tif|tiff|gif|bmp|jpg|jpeg|jp2|png|pcx|pgm|ppm|pbm|xwd|ima|hobj)$")).TupleConcat("ignore_case"), out imgFiles);

            if (imgFiles.TupleLength() == 0)
            {
                Global.AddLog("防错图片数量为空，请检测路径是否正确，或者文件夹是否有图片");
            }

            if (sideStr == "A" && retKind == "OK")
                ImgInspect2D.InspecteInitial("2D_OK防错", imgFiles.TupleLength());
            else if (sideStr == "A" && retKind == "NG")
                ImgInspect2D.InspecteInitial("2D_NG防错", imgFiles.TupleLength());
            else if (sideStr == "B" && retKind == "OK")
                ImgInspect3D.InspecteInitial("3D_OK防错", imgFiles.TupleLength());
            else if (sideStr == "B" && retKind == "NG")
                ImgInspect3D.InspecteInitial("3D_NG防错", imgFiles.TupleLength());



            HOperatorSet.GenEmptyObj(out img);

            for (imgIndex = 0; (int)imgIndex <= (int)((new HTuple(imgFiles.TupleLength())) - 1); imgIndex = (int)imgIndex + 1)
            {
                img.Dispose();
                HOperatorSet.ReadImage(out img, imgFiles.TupleSelect(imgIndex));
                //Image Acquisition 01: Do something
                Thread.Sleep(1000);
                if (sideStr == "A")
                {
                    ImgInspect2D.ImageReceive(img);

                    ImgInspect2D.ProcessImgInQueue();
                    ThreadProcess.thread2d.WaitOne();
                }
                else if (sideStr == "B")
                {
                    ImgInspect3D.ImageReceive(img);
                    ImgInspect3D.ProcessImgInQueue();
                    ThreadProcess.thread3d.WaitOne();
                }

                //Thread.Sleep(900);
            }

        }
        private void OfflineTest(string sideStr)
        {
            textFrm = new textForm(sideStr);
            textFrm.Show();
        }
    }
}

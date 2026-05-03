using HalconDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AVS.Forms
{
    public partial class textForm : Form
    {
        HTuple imgFiles = null;
        HTuple imgIndex = 0;
        string sidestr;

        public textForm(string sideStr)
        {
            InitializeComponent();
            this.sidestr = sideStr;
        }
        private void Close_bt_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ImageFile_Bt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "图片文件|*.tif;*.tiff;*.gif;*.bmp;*.jpg;*.jpeg;*.jp2;*.png;*.pcx;*.pgm;*.ppm;*.pbm;*.xwd;*.ima;*.hobj|所有文件|*.*";
                ofd.Title = "选择图片文件（可在地址栏输入路径）";
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;

                if (DialogResult.OK == ofd.ShowDialog(this))
                {
                    imgFiles = new HTuple(ofd.FileName);
                }
            }
            imgIndex = 0;
        }


        private void SelectFolder_Bt_Click(object sender, EventArgs e)
        {
            string selectedFolderPath = null;

            Form inputForm = new Form()
            {
                Text = "选择文件夹（可直接粘贴路径）",
                Size = new Size(500, 160),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label lbl = new Label() { Text = "文件夹路径：", Location = new Point(12, 15), AutoSize = true };
            TextBox txtPath = new TextBox() { Location = new Point(90, 12), Size = new Size(380, 25) };
            if (Clipboard.ContainsText())
            {
                string clipText = Clipboard.GetText().Trim();
                if (System.IO.Directory.Exists(clipText))
                    txtPath.Text = clipText;
            }
            txtPath.SelectAll();

            Button btnBrowse = new Button() { Text = "浏览...", Location = new Point(90, 45), Size = new Size(80, 28) };
            Button btnOK = new Button() { Text = "确定", Location = new Point(310, 85), Size = new Size(80, 28), DialogResult = DialogResult.OK };
            Button btnCancel = new Button() { Text = "取消", Location = new Point(400, 85), Size = new Size(80, 28), DialogResult = DialogResult.Cancel };

            btnBrowse.Click += (s, args) =>
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    if (!string.IsNullOrEmpty(txtPath.Text) && System.IO.Directory.Exists(txtPath.Text))
                        fbd.SelectedPath = txtPath.Text;
                    if (fbd.ShowDialog(inputForm) == DialogResult.OK)
                        txtPath.Text = fbd.SelectedPath;
                }
            };

            inputForm.Controls.AddRange(new Control[] { lbl, txtPath, btnBrowse, btnOK, btnCancel });
            inputForm.AcceptButton = btnOK;
            inputForm.CancelButton = btnCancel;

            if (inputForm.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(txtPath.Text))
            {
                selectedFolderPath = txtPath.Text.Trim();
            }

            if (!string.IsNullOrEmpty(selectedFolderPath) && System.IO.Directory.Exists(selectedFolderPath))
            {
                HOperatorSet.ListFiles(selectedFolderPath, (new HTuple("files")).TupleConcat("follow_links"), out imgFiles);
                HOperatorSet.TupleRegexpSelect(imgFiles, (new HTuple("\\.(tif|tiff|gif|bmp|jpg|jpeg|jp2|png|pcx|pgm|ppm|pbm|xwd|ima|hobj)$")).TupleConcat("ignore_case"), out imgFiles);
            }
            imgIndex = 0;
        }

        //private void NextImage_bt_Click(object sender, EventArgs e)
        //{
        //    if (imgFiles == null || imgFiles.Length == 0)
        //    {
        //        MessageBox.Show("请先选择图片文件夹！");
        //        return;
        //    }

        //    HObject img = null;
        //    HOperatorSet.ReadImage(out img, imgFiles.TupleSelect(imgIndex));

        //    if (sidestr == "A")
        //    {
        //        ImgInspect2D.ImageReceive(img);
        //    }
        //    else
        //    {
        //        ImgInspect3D.ImageReceive(img);
        //    }

        //    imgIndex++;
        //    if (imgIndex == imgFiles.Length)
        //    {
        //        imgIndex = 0;
        //    }
        //}


        private void NextImage_bt_Click(object sender, EventArgs e)
        {
            HObject img = null;
            HOperatorSet.ReadImage(out img, imgFiles.TupleSelect(imgIndex));
            if (sidestr == "A")
            {
                ImgInspect2D.InspectImg(img, tWindowA);
            }
            else
            {
                ImgInspect3D.InspectImg(img, tWindowA);
            }

            imgIndex++;
            if (imgIndex == imgFiles.Length)
            {
                imgIndex = 0;
            }
        }
        //private async void NextImage_bt_Click(object sender, EventArgs e)
        //{
        //    HObject img = null;
        //    await Task.Run(() =>
        //    {
        //        //while (true)
        //        //{
        //            if (ImgInspect2D.imgQueue.Count == 0)
        //            {
        //                for (int i = 0; i < imgFiles.Length; i++)
        //                {
        //                    HOperatorSet.ReadImage(out img, imgFiles.TupleSelect(i));
        //                    ImgInspect2D.ImageReceive(img);
        //                }
        //            }

        //        //}
        //        //// Global.AddLog("exMsg图片压缩完成");
        //        //ImageSaveProcess.initData();
        //        //Proof_Thread("D:\\post_weld_inspection\\图片\\2D\\阿特斯\\图片\\pack\\81001713;100824;20250723;01072;1_2\\焊后C0312\\OK\\2D检测\\2025_09_18\\Originallmage", "A", "OK");
        //    });
        //}

        private void LoadToQueue_bt_Click(object sender, EventArgs e)
        {
            if (imgFiles == null || imgFiles.Length == 0)
            {
                MessageBox.Show("请先选择图片文件夹！");
                return;
            }

            int imgCount = imgFiles.Length;

            if (sidestr == "A")
            {
                for (int i = 0; i < imgCount; i++)
                {
                    HObject img = null;
                    HOperatorSet.ReadImage(out img, imgFiles.TupleSelect(i));
                    ImgInspect2D.ImageReceive(img);
                }
                MessageBox.Show($"已将 {imgCount} 张图片压入2D图像队列！\n请确保已发送初始化电文。");
            }
            else
            {
                for (int i = 0; i < imgCount; i++)
                {
                    HObject img = null;
                    HOperatorSet.ReadImage(out img, imgFiles.TupleSelect(i));
                    ImgInspect3D.ImageReceive(img);
                }
                MessageBox.Show($"已将 {imgCount} 张图片压入3D图像队列！\n请确保已发送初始化电文。");
            }
        }

    }
}

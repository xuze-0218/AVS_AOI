using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HalconDotNet;

namespace AVS
{
    public partial class ManualLine : Form
    {
        private HTuple current_beginRow, current_beginCol, current_endRow, current_endCol;
        private HTuple zoomWndFactor = 1;
        private HTuple imgW, imgH;
        private HObject image;

        public HTuple row1;
        public HTuple row2;
        public HTuple col1;
        public HTuple col2;

        public ManualLine()
        {
            InitializeComponent();
        }
        public ManualLine(HObject img)
        {
            InitializeComponent();
            HOperatorSet.GenEmptyObj(out image);
            image = img;
        }

        private void ManualLine_Load(object sender, EventArgs e)
        {
            this.TopMost = false;
            this.BringToFront();
            this.TopMost = true;

            //HOperatorSet.DispObj(img, hWindowManual.HalconWindow);
            LoadImageAsAspectRatio(hWindowManual, image);
        }
        private void BtnManualDrawing_Click(object sender, EventArgs e)
        {
            hWindowManual.HMouseWheel -= HoWindow_HMouseWheel;
            row1 = null;
            col1 = null;
            row2 = null;
            col2 = null;
            string remind = "左键点击画面画线，右键确认！";
            //Utils.HDispMessage(hWindowManual.HalconWindow, remind, 20, 20, "blue", "Arial", "-12", false);
            MessageBox.Show(remind);
            hWindowManual.Focus();
            HOperatorSet.SetColor(hWindowManual.HalconWindow, "red");
            HOperatorSet.DrawLine(hWindowManual.HalconWindow, out row1, out col1, out row2, out col2);
            if (row1 != null)
            {
                HOperatorSet.DispLine(hWindowManual.HalconWindow, row1, col1, row2, col2);
            }
            hWindowManual.HMouseWheel += HoWindow_HMouseWheel;
        }

        private void BtnCancelExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnSaveExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void HoWindow_HMouseWheel(object sender, HMouseEventArgs e)
        {
            try
            {
                int Button;
                double HRow = 0, HCol = 0, mode = 1;

                hWindowManual.HalconWindow.GetMpositionSubPix(out HRow, out HCol, out Button);
                if (e.Delta > 0)
                {
                    mode = 1;
                }
                else
                {
                    mode = -1;
                }

                HOperatorSet.ClearWindow(hWindowManual.HalconWindow);
                DispImageZoom(sender, image, null, mode, HRow, HCol);
                if (row1 != null)
                {
                    HOperatorSet.DispLine(hWindowManual.HalconWindow, row1, col1, row2, col2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "提示：");
            }
        }

        //按缩放比例显示图像
        private void DispImageZoom(object sender, HObject image, HObject obj, double mode, double Mouse_row, double Mouse_col)
        {
            HWindowControl showWindow = sender as HWindowControl;
            HTuple imgW, imgH, zoom_beginRow, zoom_beginCol, zoom_endRow, zoom_endCol;
            try
            {
                HOperatorSet.GetImageSize(image, out imgW, out imgH);
                showWindow.HalconWindow.GetPart(out current_beginRow, out current_beginCol, out current_endRow, out current_endCol);

                if (mode > 0)   // 放大图像
                {
                    zoom_beginRow = (int)(current_beginRow.D + (Mouse_row - current_beginRow.D) * 0.300d);
                    zoom_beginCol = (int)(current_beginCol.D + (Mouse_col - current_beginCol.D) * 0.300d);
                    zoom_endRow = (int)(current_endRow.D - (current_endRow.D - Mouse_row) * 0.300d);
                    zoom_endCol = (int)(current_endCol.D - (current_endCol.D - Mouse_col) * 0.300d);
                }
                else            // 缩小图像
                {
                    zoom_beginRow = (int)(Mouse_row - (Mouse_row - current_beginRow.D) / 0.700d);
                    zoom_beginCol = (int)(Mouse_col - (Mouse_col - current_beginCol.D) / 0.700d);
                    zoom_endRow = (int)(Mouse_row + (current_endRow.D - Mouse_row) / 0.700d);
                    zoom_endCol = (int)(Mouse_col + (current_endCol.D - Mouse_col) / 0.700d);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return;
            }

            try
            {
                int hw_width, hw_height;
                hw_width = showWindow.WindowSize.Width;
                hw_height = showWindow.WindowSize.Height;

                bool _isOutOfArea = true;
                bool _isOutOfSize = true;
                bool _isOutOfPixel = true;  //避免像素过大

                _isOutOfArea = zoom_beginRow >= imgH || zoom_endRow <= 0 || zoom_beginCol >= imgW || zoom_endCol < 0;
                _isOutOfSize = (zoom_endRow - zoom_beginRow) > imgH * 20 || (zoom_endCol - zoom_beginCol) > imgW * 20;
                _isOutOfPixel = hw_height / (zoom_endRow - zoom_beginRow) > 500 || hw_width / (zoom_endCol - zoom_beginCol) > 500;

                if (_isOutOfArea || _isOutOfSize || _isOutOfPixel)
                {
                    return;
                }

                showWindow.HalconWindow.SetPaint(new HTuple("default"));
                //保持图像显示比例
                showWindow.HalconWindow.SetPart(zoom_beginRow, zoom_beginCol, zoom_endRow, zoom_beginCol + (zoom_endRow - zoom_beginRow) * hw_width / hw_height);

                int w01 = (zoom_endRow - zoom_beginRow) * hw_width / hw_height;
                int w02 = current_endCol - current_beginCol;
                double scale = (double)w01 / w02;
                zoomWndFactor *= scale;

                showWindow.HalconWindow.ClearWindow();
                //HOperatorSet.DispImage(image, showWindow.HalconWindow);
                HOperatorSet.DispObj(image, showWindow.HalconWindow);
                //HOperatorSet.DispObj(obj, showWindow.HalconWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return;
            }
        }

        //按图像宽高比例显示图像
        private void LoadImageAsAspectRatio(object sender, HObject image)
        {
            try
            {
                zoomWndFactor = 1;

                HWindowControl showWindow = sender as HWindowControl;
                HTuple row01, col01, row02, col02;
                HOperatorSet.GetImageSize(image, out imgW, out imgH);

                HTuple winW = showWindow.Width;
                HTuple winH = showWindow.Height;

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

                HOperatorSet.SetPart(showWindow.HalconWindow, row01, col01, row02, col02);
                HOperatorSet.ClearWindow(showWindow.HalconWindow);
                //HOperatorSet.DispImage(wImage, showWindow.HalconWindow);
                HOperatorSet.DispObj(image, showWindow.HalconWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}

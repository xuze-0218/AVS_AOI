namespace AVS
{
    partial class ManualLine
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BtnManualDrawing = new System.Windows.Forms.Button();
            this.hWindowManual = new HalconDotNet.HWindowControl();
            this.BtnSaveExit = new System.Windows.Forms.Button();
            this.BtnCancelExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnManualDrawing
            // 
            this.BtnManualDrawing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnManualDrawing.Location = new System.Drawing.Point(867, 9);
            this.BtnManualDrawing.Name = "BtnManualDrawing";
            this.BtnManualDrawing.Size = new System.Drawing.Size(125, 43);
            this.BtnManualDrawing.TabIndex = 0;
            this.BtnManualDrawing.Text = "手动画线";
            this.BtnManualDrawing.UseVisualStyleBackColor = true;
            this.BtnManualDrawing.Click += new System.EventHandler(this.BtnManualDrawing_Click);
            // 
            // hWindowManual
            // 
            this.hWindowManual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hWindowManual.BackColor = System.Drawing.Color.Black;
            this.hWindowManual.BorderColor = System.Drawing.Color.Black;
            this.hWindowManual.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowManual.Location = new System.Drawing.Point(9, 9);
            this.hWindowManual.Margin = new System.Windows.Forms.Padding(0);
            this.hWindowManual.Name = "hWindowManual";
            this.hWindowManual.Size = new System.Drawing.Size(855, 504);
            this.hWindowManual.TabIndex = 18;
            this.hWindowManual.WindowSize = new System.Drawing.Size(855, 504);
            this.hWindowManual.HMouseWheel += new HalconDotNet.HMouseEventHandler(this.HoWindow_HMouseWheel);
            // 
            // BtnSaveExit
            // 
            this.BtnSaveExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSaveExit.Location = new System.Drawing.Point(867, 58);
            this.BtnSaveExit.Name = "BtnSaveExit";
            this.BtnSaveExit.Size = new System.Drawing.Size(125, 43);
            this.BtnSaveExit.TabIndex = 0;
            this.BtnSaveExit.Text = "确认退出";
            this.BtnSaveExit.UseVisualStyleBackColor = true;
            this.BtnSaveExit.Click += new System.EventHandler(this.BtnSaveExit_Click);
            // 
            // BtnCancelExit
            // 
            this.BtnCancelExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnCancelExit.Location = new System.Drawing.Point(867, 107);
            this.BtnCancelExit.Name = "BtnCancelExit";
            this.BtnCancelExit.Size = new System.Drawing.Size(125, 43);
            this.BtnCancelExit.TabIndex = 0;
            this.BtnCancelExit.Text = "取消退出";
            this.BtnCancelExit.UseVisualStyleBackColor = true;
            this.BtnCancelExit.Click += new System.EventHandler(this.BtnCancelExit_Click);
            // 
            // ManualLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1004, 522);
            this.Controls.Add(this.hWindowManual);
            this.Controls.Add(this.BtnCancelExit);
            this.Controls.Add(this.BtnSaveExit);
            this.Controls.Add(this.BtnManualDrawing);
            this.Name = "ManualLine";
            this.Text = "ManualLine";
            this.Load += new System.EventHandler(this.ManualLine_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnManualDrawing;
        private HalconDotNet.HWindowControl hWindowManual;
        private System.Windows.Forms.Button BtnSaveExit;
        private System.Windows.Forms.Button BtnCancelExit;
    }
}
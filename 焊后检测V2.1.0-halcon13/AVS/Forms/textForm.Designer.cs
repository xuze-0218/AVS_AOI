namespace AVS.Forms
{
    partial class textForm
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
            this.tWindowA = new HalconDotNet.HWindowControl();
            this.ImageFile_Bt = new System.Windows.Forms.Button();
            this.SelectFolder_Bt = new System.Windows.Forms.Button();
            this.NextImage_bt = new System.Windows.Forms.Button();
            this.Close_bt = new System.Windows.Forms.Button();
            this.LoadToQueue_bt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tWindowA
            // 
            this.tWindowA.BackColor = System.Drawing.Color.Black;
            this.tWindowA.BorderColor = System.Drawing.Color.Black;
            this.tWindowA.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.tWindowA.Location = new System.Drawing.Point(1, 1);
            this.tWindowA.Margin = new System.Windows.Forms.Padding(0);
            this.tWindowA.Name = "tWindowA";
            this.tWindowA.Size = new System.Drawing.Size(624, 423);
            this.tWindowA.TabIndex = 19;
            this.tWindowA.UseWaitCursor = true;
            this.tWindowA.WindowSize = new System.Drawing.Size(624, 423);
            // 
            // ImageFile_Bt
            // 
            this.ImageFile_Bt.Location = new System.Drawing.Point(12, 457);
            this.ImageFile_Bt.Name = "ImageFile_Bt";
            this.ImageFile_Bt.Size = new System.Drawing.Size(87, 46);
            this.ImageFile_Bt.TabIndex = 22;
            this.ImageFile_Bt.Text = "单张图片";
            this.ImageFile_Bt.UseVisualStyleBackColor = true;
            this.ImageFile_Bt.Click += new System.EventHandler(this.ImageFile_Bt_Click);
            // 
            // SelectFolder_Bt
            // 
            this.SelectFolder_Bt.Location = new System.Drawing.Point(105, 457);
            this.SelectFolder_Bt.Name = "SelectFolder_Bt";
            this.SelectFolder_Bt.Size = new System.Drawing.Size(87, 46);
            this.SelectFolder_Bt.TabIndex = 26;
            this.SelectFolder_Bt.Text = "文件夹";
            this.SelectFolder_Bt.UseVisualStyleBackColor = true;
            this.SelectFolder_Bt.Click += new System.EventHandler(this.SelectFolder_Bt_Click);
            // 
            // NextImage_bt
            // 
            this.NextImage_bt.Location = new System.Drawing.Point(198, 457);
            this.NextImage_bt.Name = "NextImage_bt";
            this.NextImage_bt.Size = new System.Drawing.Size(87, 46);
            this.NextImage_bt.TabIndex = 23;
            this.NextImage_bt.Text = "下一张";
            this.NextImage_bt.UseVisualStyleBackColor = true;
            this.NextImage_bt.Click += new System.EventHandler(this.NextImage_bt_Click);
            // 
            // Close_bt
            // 
            this.Close_bt.Location = new System.Drawing.Point(480, 457);
            this.Close_bt.Name = "Close_bt";
            this.Close_bt.Size = new System.Drawing.Size(134, 46);
            this.Close_bt.TabIndex = 24;
            this.Close_bt.Text = "关闭";
            this.Close_bt.UseVisualStyleBackColor = true;
            this.Close_bt.Click += new System.EventHandler(this.Close_bt_Click);
            // 
            // LoadToQueue_bt
            // 
            this.LoadToQueue_bt.Location = new System.Drawing.Point(291, 457);
            this.LoadToQueue_bt.Name = "LoadToQueue_bt";
            this.LoadToQueue_bt.Size = new System.Drawing.Size(120, 46);
            this.LoadToQueue_bt.TabIndex = 25;
            this.LoadToQueue_bt.Text = "压入队列测试";
            this.LoadToQueue_bt.UseVisualStyleBackColor = true;
            this.LoadToQueue_bt.Click += new System.EventHandler(this.LoadToQueue_bt_Click);
            // 
            // textForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 512);
            this.Controls.Add(this.LoadToQueue_bt);
            this.Controls.Add(this.Close_bt);
            this.Controls.Add(this.NextImage_bt);
            this.Controls.Add(this.SelectFolder_Bt);
            this.Controls.Add(this.ImageFile_Bt);
            this.Controls.Add(this.tWindowA);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "textForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "textForm";
            this.ResumeLayout(false);

        }

        #endregion
        public HalconDotNet.HWindowControl tWindowA;
        private System.Windows.Forms.Button ImageFile_Bt;
        private System.Windows.Forms.Button SelectFolder_Bt;
        private System.Windows.Forms.Button NextImage_bt;
        private System.Windows.Forms.Button Close_bt;
        private System.Windows.Forms.Button LoadToQueue_bt;
    }
}
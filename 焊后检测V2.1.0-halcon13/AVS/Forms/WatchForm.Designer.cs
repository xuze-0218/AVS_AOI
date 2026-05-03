namespace AVS
{
    partial class WatchForm
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnOKErrorProof3d = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCamBState = new System.Windows.Forms.TextBox();
            this.txtPLCBState = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.hWindowB = new HalconDotNet.HWindowControl();
            this.hWindowA = new HalconDotNet.HWindowControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOKErrorProof2d = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCamAState = new System.Windows.Forms.TextBox();
            this.txtPLCAState = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.hWindowC = new HalconDotNet.HWindowControl();
            this.hWindowD = new HalconDotNet.HWindowControl();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightGray;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.btnOKErrorProof3d);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.txtCamBState);
            this.panel2.Controls.Add(this.txtPLCBState);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.label11);
            this.panel2.ForeColor = System.Drawing.Color.Red;
            this.panel2.Location = new System.Drawing.Point(512, 1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(510, 43);
            this.panel2.TabIndex = 14;
            // 
            // btnOKErrorProof3d
            // 
            this.btnOKErrorProof3d.Location = new System.Drawing.Point(366, 9);
            this.btnOKErrorProof3d.Name = "btnOKErrorProof3d";
            this.btnOKErrorProof3d.Size = new System.Drawing.Size(67, 25);
            this.btnOKErrorProof3d.TabIndex = 19;
            this.btnOKErrorProof3d.Text = "3D测试";
            this.btnOKErrorProof3d.UseVisualStyleBackColor = true;
            this.btnOKErrorProof3d.Click += new System.EventHandler(this.btnErrorProof_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(2, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 31);
            this.label2.TabIndex = 16;
            this.label2.Text = "B-3D相机检测";
            // 
            // txtCamBState
            // 
            this.txtCamBState.BackColor = System.Drawing.Color.Red;
            this.txtCamBState.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCamBState.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCamBState.ForeColor = System.Drawing.SystemColors.Window;
            this.txtCamBState.Location = new System.Drawing.Point(332, 9);
            this.txtCamBState.Multiline = true;
            this.txtCamBState.Name = "txtCamBState";
            this.txtCamBState.Size = new System.Drawing.Size(28, 26);
            this.txtCamBState.TabIndex = 15;
            this.txtCamBState.Text = "×";
            this.txtCamBState.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtPLCBState
            // 
            this.txtPLCBState.BackColor = System.Drawing.Color.Red;
            this.txtPLCBState.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPLCBState.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold);
            this.txtPLCBState.ForeColor = System.Drawing.SystemColors.Window;
            this.txtPLCBState.Location = new System.Drawing.Point(212, 9);
            this.txtPLCBState.Multiline = true;
            this.txtPLCBState.Name = "txtPLCBState";
            this.txtPLCBState.Size = new System.Drawing.Size(28, 26);
            this.txtPLCBState.TabIndex = 14;
            this.txtPLCBState.Text = "×";
            this.txtPLCBState.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.ForeColor = System.Drawing.Color.Black;
            this.label12.Location = new System.Drawing.Point(259, 10);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(71, 22);
            this.label12.TabIndex = 8;
            this.label12.Text = "Camera";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.ForeColor = System.Drawing.Color.Black;
            this.label11.Location = new System.Drawing.Point(169, 11);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(41, 22);
            this.label11.TabIndex = 5;
            this.label11.Text = "PLC";
            // 
            // hWindowB
            // 
            this.hWindowB.BackColor = System.Drawing.Color.Black;
            this.hWindowB.BorderColor = System.Drawing.Color.Black;
            this.hWindowB.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowB.Location = new System.Drawing.Point(512, 45);
            this.hWindowB.Margin = new System.Windows.Forms.Padding(0);
            this.hWindowB.Name = "hWindowB";
            this.hWindowB.Size = new System.Drawing.Size(510, 245);
            this.hWindowB.TabIndex = 17;
            this.hWindowB.WindowSize = new System.Drawing.Size(510, 245);
            // 
            // hWindowA
            // 
            this.hWindowA.BackColor = System.Drawing.Color.Black;
            this.hWindowA.BorderColor = System.Drawing.Color.Black;
            this.hWindowA.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowA.Location = new System.Drawing.Point(1, 45);
            this.hWindowA.Margin = new System.Windows.Forms.Padding(0);
            this.hWindowA.Name = "hWindowA";
            this.hWindowA.Size = new System.Drawing.Size(510, 245);
            this.hWindowA.TabIndex = 17;
            this.hWindowA.WindowSize = new System.Drawing.Size(510, 245);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightGray;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnOKErrorProof2d);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.txtCamAState);
            this.panel1.Controls.Add(this.txtPLCAState);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label4);
            this.panel1.ForeColor = System.Drawing.Color.Red;
            this.panel1.Location = new System.Drawing.Point(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(510, 43);
            this.panel1.TabIndex = 14;
            // 
            // btnOKErrorProof2d
            // 
            this.btnOKErrorProof2d.Location = new System.Drawing.Point(365, 9);
            this.btnOKErrorProof2d.Name = "btnOKErrorProof2d";
            this.btnOKErrorProof2d.Size = new System.Drawing.Size(67, 25);
            this.btnOKErrorProof2d.TabIndex = 19;
            this.btnOKErrorProof2d.Text = "2D测试";
            this.btnOKErrorProof2d.UseVisualStyleBackColor = true;
            this.btnOKErrorProof2d.Click += new System.EventHandler(this.btnErrorProof_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(2, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 31);
            this.label1.TabIndex = 16;
            this.label1.Text = "A-2D相机检测";
            // 
            // txtCamAState
            // 
            this.txtCamAState.BackColor = System.Drawing.Color.Red;
            this.txtCamAState.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCamAState.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCamAState.ForeColor = System.Drawing.SystemColors.Window;
            this.txtCamAState.Location = new System.Drawing.Point(321, 9);
            this.txtCamAState.Multiline = true;
            this.txtCamAState.Name = "txtCamAState";
            this.txtCamAState.Size = new System.Drawing.Size(28, 26);
            this.txtCamAState.TabIndex = 15;
            this.txtCamAState.Text = "×";
            this.txtCamAState.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtPLCAState
            // 
            this.txtPLCAState.BackColor = System.Drawing.Color.Red;
            this.txtPLCAState.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPLCAState.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold);
            this.txtPLCAState.ForeColor = System.Drawing.SystemColors.Window;
            this.txtPLCAState.Location = new System.Drawing.Point(210, 9);
            this.txtPLCAState.Multiline = true;
            this.txtPLCAState.Name = "txtPLCAState";
            this.txtPLCAState.Size = new System.Drawing.Size(28, 26);
            this.txtPLCAState.TabIndex = 14;
            this.txtPLCAState.Text = "×";
            this.txtPLCAState.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(244, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 22);
            this.label3.TabIndex = 8;
            this.label3.Text = "Camera";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(173, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 22);
            this.label4.TabIndex = 5;
            this.label4.Text = "PLC";
            // 
            // hWindowC
            // 
            this.hWindowC.BackColor = System.Drawing.Color.Black;
            this.hWindowC.BorderColor = System.Drawing.Color.Black;
            this.hWindowC.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowC.Location = new System.Drawing.Point(1, 291);
            this.hWindowC.Margin = new System.Windows.Forms.Padding(0);
            this.hWindowC.Name = "hWindowC";
            this.hWindowC.Size = new System.Drawing.Size(510, 272);
            this.hWindowC.TabIndex = 17;
            this.hWindowC.WindowSize = new System.Drawing.Size(510, 272);
            // 
            // hWindowD
            // 
            this.hWindowD.BackColor = System.Drawing.Color.Black;
            this.hWindowD.BorderColor = System.Drawing.Color.Black;
            this.hWindowD.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowD.Location = new System.Drawing.Point(512, 291);
            this.hWindowD.Margin = new System.Windows.Forms.Padding(0);
            this.hWindowD.Name = "hWindowD";
            this.hWindowD.Size = new System.Drawing.Size(510, 272);
            this.hWindowD.TabIndex = 17;
            this.hWindowD.WindowSize = new System.Drawing.Size(510, 272);
            // 
            // WatchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 560);
            this.Controls.Add(this.hWindowC);
            this.Controls.Add(this.hWindowA);
            this.Controls.Add(this.hWindowD);
            this.Controls.Add(this.hWindowB);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "WatchForm";
            this.Text = "WatchForm";
            this.Load += new System.EventHandler(this.WatchForm_Load);
            this.Resize += new System.EventHandler(this.WatchForm_Resize);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        public HalconDotNet.HWindowControl hWindowB;
        public System.Windows.Forms.TextBox txtCamBState;
        public System.Windows.Forms.TextBox txtPLCBState;
        private System.Windows.Forms.Label label2;
        public HalconDotNet.HWindowControl hWindowA;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox txtCamAState;
        public System.Windows.Forms.TextBox txtPLCAState;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        public HalconDotNet.HWindowControl hWindowC;
        public HalconDotNet.HWindowControl hWindowD;
        private System.Windows.Forms.Button btnOKErrorProof3d;
        private System.Windows.Forms.Button btnOKErrorProof2d;
    }
}
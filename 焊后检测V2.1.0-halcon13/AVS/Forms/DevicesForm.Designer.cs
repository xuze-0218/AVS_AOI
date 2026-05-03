namespace AVS
{
    partial class DevicesForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabDevices = new System.Windows.Forms.TabControl();
            this.tabCam = new System.Windows.Forms.TabPage();
            this.PnlCtrArea = new System.Windows.Forms.Panel();
            this.gbxCamSelect = new System.Windows.Forms.GroupBox();
            this.rbmCamSelectD = new System.Windows.Forms.RadioButton();
            this.rbnCamSelectC = new System.Windows.Forms.RadioButton();
            this.rbnCamSelectB = new System.Windows.Forms.RadioButton();
            this.rbnCamSelectA = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cmbCameraSelected = new System.Windows.Forms.ComboBox();
            this.BtnSaveCamSet = new System.Windows.Forms.Button();
            this.BtnSelectImg = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.TbxImgPath = new System.Windows.Forms.TextBox();
            this.TbxCamIpAddress = new System.Windows.Forms.TextBox();
            this.RbnImgSource_Local = new System.Windows.Forms.RadioButton();
            this.RbnImgSource_Cam = new System.Windows.Forms.RadioButton();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.colParm = new DevComponents.DotNetBar.Controls.DataGridViewLabelXColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSaveImg = new System.Windows.Forms.Button();
            this.btnStopGrapImg = new System.Windows.Forms.Button();
            this.btnGrabOneImg = new System.Windows.Forms.Button();
            this.btnContinueGrabImg = new System.Windows.Forms.Button();
            this.PnlCamView = new System.Windows.Forms.Panel();
            this.deviceHWindowA = new HalconDotNet.HWindowControl();
            this.tabComm = new System.Windows.Forms.TabPage();
            this.clientManage = new VisionUserControls.SocketCommManage.ClientManage();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.groupBox1.SuspendLayout();
            this.tabDevices.SuspendLayout();
            this.tabCam.SuspendLayout();
            this.PnlCtrArea.SuspendLayout();
            this.gbxCamSelect.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.PnlCamView.SuspendLayout();
            this.tabComm.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.groupBox1.Controls.Add(this.tabDevices);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Font = new System.Drawing.Font("宋体", 9F);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1024, 615);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            // 
            // tabDevices
            // 
            this.tabDevices.Controls.Add(this.tabCam);
            this.tabDevices.Controls.Add(this.tabComm);
            this.tabDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabDevices.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabDevices.Location = new System.Drawing.Point(3, 17);
            this.tabDevices.Multiline = true;
            this.tabDevices.Name = "tabDevices";
            this.tabDevices.SelectedIndex = 0;
            this.tabDevices.Size = new System.Drawing.Size(1018, 595);
            this.tabDevices.TabIndex = 0;
            // 
            // tabCam
            // 
            this.tabCam.Controls.Add(this.PnlCtrArea);
            this.tabCam.Controls.Add(this.PnlCamView);
            this.tabCam.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.tabCam.Location = new System.Drawing.Point(4, 31);
            this.tabCam.Name = "tabCam";
            this.tabCam.Padding = new System.Windows.Forms.Padding(3);
            this.tabCam.Size = new System.Drawing.Size(1010, 560);
            this.tabCam.TabIndex = 0;
            this.tabCam.Text = "相机设置";
            this.tabCam.UseVisualStyleBackColor = true;
            // 
            // PnlCtrArea
            // 
            this.PnlCtrArea.Controls.Add(this.gbxCamSelect);
            this.PnlCtrArea.Controls.Add(this.groupBox3);
            this.PnlCtrArea.Controls.Add(this.dataGridView1);
            this.PnlCtrArea.Controls.Add(this.btnSaveImg);
            this.PnlCtrArea.Controls.Add(this.btnStopGrapImg);
            this.PnlCtrArea.Controls.Add(this.btnGrabOneImg);
            this.PnlCtrArea.Controls.Add(this.btnContinueGrabImg);
            this.PnlCtrArea.Dock = System.Windows.Forms.DockStyle.Right;
            this.PnlCtrArea.Location = new System.Drawing.Point(669, 3);
            this.PnlCtrArea.Name = "PnlCtrArea";
            this.PnlCtrArea.Size = new System.Drawing.Size(338, 554);
            this.PnlCtrArea.TabIndex = 20;
            // 
            // gbxCamSelect
            // 
            this.gbxCamSelect.Controls.Add(this.rbmCamSelectD);
            this.gbxCamSelect.Controls.Add(this.rbnCamSelectC);
            this.gbxCamSelect.Controls.Add(this.rbnCamSelectB);
            this.gbxCamSelect.Controls.Add(this.rbnCamSelectA);
            this.gbxCamSelect.Location = new System.Drawing.Point(3, 2);
            this.gbxCamSelect.Name = "gbxCamSelect";
            this.gbxCamSelect.Size = new System.Drawing.Size(280, 59);
            this.gbxCamSelect.TabIndex = 9;
            this.gbxCamSelect.TabStop = false;
            this.gbxCamSelect.Text = "相机选择";
            // 
            // rbmCamSelectD
            // 
            this.rbmCamSelectD.AutoSize = true;
            this.rbmCamSelectD.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbmCamSelectD.Location = new System.Drawing.Point(241, 33);
            this.rbmCamSelectD.Name = "rbmCamSelectD";
            this.rbmCamSelectD.Size = new System.Drawing.Size(33, 20);
            this.rbmCamSelectD.TabIndex = 4;
            this.rbmCamSelectD.Text = "D";
            this.rbmCamSelectD.UseVisualStyleBackColor = true;
            this.rbmCamSelectD.CheckedChanged += new System.EventHandler(this.rbnCamSelect_CheckedChanged);
            // 
            // rbnCamSelectC
            // 
            this.rbnCamSelectC.AutoSize = true;
            this.rbnCamSelectC.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbnCamSelectC.Location = new System.Drawing.Point(160, 33);
            this.rbnCamSelectC.Name = "rbnCamSelectC";
            this.rbnCamSelectC.Size = new System.Drawing.Size(33, 20);
            this.rbnCamSelectC.TabIndex = 3;
            this.rbnCamSelectC.Text = "C";
            this.rbnCamSelectC.UseVisualStyleBackColor = true;
            this.rbnCamSelectC.CheckedChanged += new System.EventHandler(this.rbnCamSelect_CheckedChanged);
            // 
            // rbnCamSelectB
            // 
            this.rbnCamSelectB.AutoSize = true;
            this.rbnCamSelectB.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbnCamSelectB.Location = new System.Drawing.Point(87, 33);
            this.rbnCamSelectB.Name = "rbnCamSelectB";
            this.rbnCamSelectB.Size = new System.Drawing.Size(33, 20);
            this.rbnCamSelectB.TabIndex = 2;
            this.rbnCamSelectB.Text = "B";
            this.rbnCamSelectB.UseVisualStyleBackColor = true;
            this.rbnCamSelectB.CheckedChanged += new System.EventHandler(this.rbnCamSelect_CheckedChanged);
            // 
            // rbnCamSelectA
            // 
            this.rbnCamSelectA.AutoSize = true;
            this.rbnCamSelectA.Checked = true;
            this.rbnCamSelectA.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbnCamSelectA.Location = new System.Drawing.Point(6, 33);
            this.rbnCamSelectA.Name = "rbnCamSelectA";
            this.rbnCamSelectA.Size = new System.Drawing.Size(33, 20);
            this.rbnCamSelectA.TabIndex = 1;
            this.rbnCamSelectA.TabStop = true;
            this.rbnCamSelectA.Text = "A";
            this.rbnCamSelectA.UseVisualStyleBackColor = true;
            this.rbnCamSelectA.CheckedChanged += new System.EventHandler(this.rbnCamSelect_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cmbCameraSelected);
            this.groupBox3.Controls.Add(this.BtnSaveCamSet);
            this.groupBox3.Controls.Add(this.BtnSelectImg);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.TbxImgPath);
            this.groupBox3.Controls.Add(this.TbxCamIpAddress);
            this.groupBox3.Controls.Add(this.RbnImgSource_Local);
            this.groupBox3.Controls.Add(this.RbnImgSource_Cam);
            this.groupBox3.Location = new System.Drawing.Point(3, 141);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(280, 260);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "相机设置";
            // 
            // cmbCameraSelected
            // 
            this.cmbCameraSelected.FormattingEnabled = true;
            this.cmbCameraSelected.Items.AddRange(new object[] {
            "海康",
            "basler"});
            this.cmbCameraSelected.Location = new System.Drawing.Point(147, 23);
            this.cmbCameraSelected.Name = "cmbCameraSelected";
            this.cmbCameraSelected.Size = new System.Drawing.Size(113, 30);
            this.cmbCameraSelected.TabIndex = 11;
            // 
            // BtnSaveCamSet
            // 
            this.BtnSaveCamSet.Location = new System.Drawing.Point(80, 216);
            this.BtnSaveCamSet.Name = "BtnSaveCamSet";
            this.BtnSaveCamSet.Size = new System.Drawing.Size(194, 31);
            this.BtnSaveCamSet.TabIndex = 4;
            this.BtnSaveCamSet.Text = "保存设置";
            this.BtnSaveCamSet.UseVisualStyleBackColor = true;
            this.BtnSaveCamSet.Click += new System.EventHandler(this.BtnSaveCamSet_Click);
            // 
            // BtnSelectImg
            // 
            this.BtnSelectImg.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnSelectImg.Location = new System.Drawing.Point(5, 79);
            this.BtnSelectImg.Name = "BtnSelectImg";
            this.BtnSelectImg.Size = new System.Drawing.Size(74, 27);
            this.BtnSelectImg.TabIndex = 4;
            this.BtnSelectImg.Text = "选择图像";
            this.BtnSelectImg.UseVisualStyleBackColor = true;
            this.BtnSelectImg.Click += new System.EventHandler(this.BtnSelectImg_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(7, 116);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 16);
            this.label1.TabIndex = 9;
            this.label1.Text = "相机地址";
            // 
            // TbxImgPath
            // 
            this.TbxImgPath.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TbxImgPath.Location = new System.Drawing.Point(80, 79);
            this.TbxImgPath.Name = "TbxImgPath";
            this.TbxImgPath.Size = new System.Drawing.Size(194, 26);
            this.TbxImgPath.TabIndex = 8;
            // 
            // TbxCamIpAddress
            // 
            this.TbxCamIpAddress.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TbxCamIpAddress.Location = new System.Drawing.Point(80, 111);
            this.TbxCamIpAddress.Name = "TbxCamIpAddress";
            this.TbxCamIpAddress.Size = new System.Drawing.Size(194, 26);
            this.TbxCamIpAddress.TabIndex = 8;
            // 
            // RbnImgSource_Local
            // 
            this.RbnImgSource_Local.AutoSize = true;
            this.RbnImgSource_Local.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnImgSource_Local.Location = new System.Drawing.Point(20, 53);
            this.RbnImgSource_Local.Name = "RbnImgSource_Local";
            this.RbnImgSource_Local.Size = new System.Drawing.Size(89, 20);
            this.RbnImgSource_Local.TabIndex = 0;
            this.RbnImgSource_Local.Text = "本地图像";
            this.RbnImgSource_Local.UseVisualStyleBackColor = true;
            this.RbnImgSource_Local.CheckedChanged += new System.EventHandler(this.RbnImgSource_Local_CheckedChanged);
            // 
            // RbnImgSource_Cam
            // 
            this.RbnImgSource_Cam.AutoSize = true;
            this.RbnImgSource_Cam.Checked = true;
            this.RbnImgSource_Cam.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnImgSource_Cam.Location = new System.Drawing.Point(20, 27);
            this.RbnImgSource_Cam.Name = "RbnImgSource_Cam";
            this.RbnImgSource_Cam.Size = new System.Drawing.Size(89, 20);
            this.RbnImgSource_Cam.TabIndex = 0;
            this.RbnImgSource_Cam.TabStop = true;
            this.RbnImgSource_Cam.Text = "相机图像";
            this.RbnImgSource_Cam.UseVisualStyleBackColor = true;
            this.RbnImgSource_Cam.CheckedChanged += new System.EventHandler(this.RbnImgSource_Cam_CheckedChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colParm,
            this.colValue});
            this.dataGridView1.Location = new System.Drawing.Point(3, 460);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(280, 91);
            this.dataGridView1.TabIndex = 0;
            // 
            // colParm
            // 
            this.colParm.HeaderText = "参数名称";
            this.colParm.Name = "colParm";
            this.colParm.ReadOnly = true;
            this.colParm.TextAlignment = System.Drawing.StringAlignment.Center;
            this.colParm.Width = 155;
            // 
            // colValue
            // 
            this.colValue.HeaderText = "值";
            this.colValue.Name = "colValue";
            this.colValue.Width = 157;
            // 
            // btnSaveImg
            // 
            this.btnSaveImg.Location = new System.Drawing.Point(163, 104);
            this.btnSaveImg.Name = "btnSaveImg";
            this.btnSaveImg.Size = new System.Drawing.Size(120, 31);
            this.btnSaveImg.TabIndex = 4;
            this.btnSaveImg.Text = "保存图像";
            this.btnSaveImg.UseVisualStyleBackColor = true;
            this.btnSaveImg.Click += new System.EventHandler(this.BtnSaveImg_Click);
            // 
            // btnStopGrapImg
            // 
            this.btnStopGrapImg.Location = new System.Drawing.Point(163, 67);
            this.btnStopGrapImg.Name = "btnStopGrapImg";
            this.btnStopGrapImg.Size = new System.Drawing.Size(120, 31);
            this.btnStopGrapImg.TabIndex = 4;
            this.btnStopGrapImg.Text = "停止采集";
            this.btnStopGrapImg.UseVisualStyleBackColor = true;
            this.btnStopGrapImg.Click += new System.EventHandler(this.BtnGrabStop_Click);
            // 
            // btnGrabOneImg
            // 
            this.btnGrabOneImg.Location = new System.Drawing.Point(3, 104);
            this.btnGrabOneImg.Name = "btnGrabOneImg";
            this.btnGrabOneImg.Size = new System.Drawing.Size(120, 31);
            this.btnGrabOneImg.TabIndex = 3;
            this.btnGrabOneImg.Text = "单张采集";
            this.btnGrabOneImg.UseVisualStyleBackColor = true;
            this.btnGrabOneImg.Click += new System.EventHandler(this.BtnGrabOne_Click);
            // 
            // btnContinueGrabImg
            // 
            this.btnContinueGrabImg.Location = new System.Drawing.Point(3, 67);
            this.btnContinueGrabImg.Name = "btnContinueGrabImg";
            this.btnContinueGrabImg.Size = new System.Drawing.Size(120, 31);
            this.btnContinueGrabImg.TabIndex = 3;
            this.btnContinueGrabImg.Text = "连续采集";
            this.btnContinueGrabImg.UseVisualStyleBackColor = true;
            this.btnContinueGrabImg.Click += new System.EventHandler(this.BtnGrabContinue_Click);
            // 
            // PnlCamView
            // 
            this.PnlCamView.BackColor = System.Drawing.Color.Transparent;
            this.PnlCamView.Controls.Add(this.deviceHWindowA);
            this.PnlCamView.Dock = System.Windows.Forms.DockStyle.Left;
            this.PnlCamView.Location = new System.Drawing.Point(3, 3);
            this.PnlCamView.Name = "PnlCamView";
            this.PnlCamView.Size = new System.Drawing.Size(660, 554);
            this.PnlCamView.TabIndex = 19;
            // 
            // deviceHWindowA
            // 
            this.deviceHWindowA.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceHWindowA.BackColor = System.Drawing.Color.Black;
            this.deviceHWindowA.BorderColor = System.Drawing.Color.Black;
            this.deviceHWindowA.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.deviceHWindowA.Location = new System.Drawing.Point(0, 3);
            this.deviceHWindowA.Name = "deviceHWindowA";
            this.deviceHWindowA.Size = new System.Drawing.Size(660, 503);
            this.deviceHWindowA.TabIndex = 18;
            this.deviceHWindowA.WindowSize = new System.Drawing.Size(660, 503);
            // 
            // tabComm
            // 
            this.tabComm.Controls.Add(this.clientManage);
            this.tabComm.Location = new System.Drawing.Point(4, 31);
            this.tabComm.Name = "tabComm";
            this.tabComm.Padding = new System.Windows.Forms.Padding(3);
            this.tabComm.Size = new System.Drawing.Size(1010, 560);
            this.tabComm.TabIndex = 1;
            this.tabComm.Text = "通信设置";
            this.tabComm.UseVisualStyleBackColor = true;
            // 
            // clientManage
            // 
            this.clientManage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clientManage.Location = new System.Drawing.Point(3, 3);
            this.clientManage.Name = "clientManage";
            this.clientManage.Size = new System.Drawing.Size(1004, 554);
            this.clientManage.TabIndex = 0;
            // 
            // DevicesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1024, 615);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DevicesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DevicesForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DevicesForm_FormClosing);
            this.Load += new System.EventHandler(this.DevicesForm_Load);
            this.Resize += new System.EventHandler(this.DevicesForm_Resize);
            this.groupBox1.ResumeLayout(false);
            this.tabDevices.ResumeLayout(false);
            this.tabCam.ResumeLayout(false);
            this.PnlCtrArea.ResumeLayout(false);
            this.gbxCamSelect.ResumeLayout(false);
            this.gbxCamSelect.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.PnlCamView.ResumeLayout(false);
            this.tabComm.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabControl tabDevices;
        private System.Windows.Forms.TabPage tabCam;
        private System.Windows.Forms.DataGridView dataGridView1;
        private DevComponents.DotNetBar.Controls.DataGridViewLabelXColumn colParm;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.Button btnStopGrapImg;
        private System.Windows.Forms.Button btnContinueGrabImg;
        private System.Windows.Forms.TabPage tabComm;
        private VisionUserControls.SocketCommManage.ClientManage clientManage;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private HalconDotNet.HWindowControl deviceHWindowA;
        private System.Windows.Forms.Panel PnlCamView;
        private System.Windows.Forms.Panel PnlCtrArea;
        private System.Windows.Forms.Button btnSaveImg;
        private System.Windows.Forms.Button btnGrabOneImg;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton RbnImgSource_Local;
        private System.Windows.Forms.RadioButton RbnImgSource_Cam;
        private System.Windows.Forms.TextBox TbxImgPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TbxCamIpAddress;
        private System.Windows.Forms.Button BtnSaveCamSet;
        private System.Windows.Forms.Button BtnSelectImg;
        private System.Windows.Forms.GroupBox gbxCamSelect;
        private System.Windows.Forms.RadioButton rbmCamSelectD;
        private System.Windows.Forms.RadioButton rbnCamSelectC;
        private System.Windows.Forms.RadioButton rbnCamSelectB;
        private System.Windows.Forms.RadioButton rbnCamSelectA;
        private System.Windows.Forms.ComboBox cmbCameraSelected;
    }
}
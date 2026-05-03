namespace AVS
{
    partial class MainForm
    {
        /// <summary>
        /// ����������������
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// ������������ʹ�õ���Դ��
        /// </summary>
        /// <param name="disposing">���Ӧ�ͷ��й���Դ��Ϊ true������Ϊ false��</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows ������������ɵĴ���

        /// <summary>
        /// �����֧������ķ��� - ��Ҫ�޸�
        /// ʹ�ô���༭���޸Ĵ˷��������ݡ�
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnNagWatchForm = new VisionUserControls.PrimaryControls.ButtonNavigate(this.components);
            this.authorityManage = new VisionUserControls.AuthorityManage.AuthorityManage();
            this.timeDispSys = new VisionUserControls.PrimaryControls.TimeDisplay();
            this.logText = new VisionUserControls.PrimaryControls.Log();
            this.btnNagDeviceManage = new VisionUserControls.PrimaryControls.ButtonNavigate(this.components);
            this.btnNagStartStop = new VisionUserControls.PrimaryControls.ButtonNavigate(this.components);
            this.btnNagParmSet = new VisionUserControls.PrimaryControls.ButtonNavigate(this.components);
            this.btnNagCloseForm = new VisionUserControls.PrimaryControls.ButtonNavigate(this.components);
            this.pnlMiddle = new System.Windows.Forms.Panel();
            this.btnNagMinizeWindow = new VisionUserControls.PrimaryControls.ButtonNavigate(this.components);
            this.formsManage = new VisionUserControls.PrimaryControls.FormsManage(this.components);
            this.tmrUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.lblProgramName = new System.Windows.Forms.Label();
            this.btnNagMaxizeWindow = new VisionUserControls.PrimaryControls.ButtonNavigate(this.components);
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlBottom
            // 
            this.pnlBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBottom.Controls.Add(this.btnNagWatchForm);
            this.pnlBottom.Controls.Add(this.authorityManage);
            this.pnlBottom.Controls.Add(this.timeDispSys);
            this.pnlBottom.Controls.Add(this.logText);
            this.pnlBottom.Controls.Add(this.btnNagDeviceManage);
            this.pnlBottom.Controls.Add(this.btnNagStartStop);
            this.pnlBottom.Controls.Add(this.btnNagParmSet);
            this.pnlBottom.Location = new System.Drawing.Point(3, 596);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1018, 194);
            this.pnlBottom.TabIndex = 3;
            // 
            // btnNagWatchForm
            // 
            this.btnNagWatchForm.BackColor = System.Drawing.Color.LightGray;
            this.btnNagWatchForm.FlatAppearance.BorderSize = 0;
            this.btnNagWatchForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNagWatchForm.Font = new System.Drawing.Font("Microsoft Sans Serif", 26F);
            this.btnNagWatchForm.ForeColor = System.Drawing.Color.Black;
            this.btnNagWatchForm.FormConnect = null;
            this.btnNagWatchForm.Location = new System.Drawing.Point(167, 4);
            this.btnNagWatchForm.Name = "btnNagWatchForm";
            this.btnNagWatchForm.SelectedBackColor = System.Drawing.Color.LightSkyBlue;
            this.btnNagWatchForm.SelectedForeColor = System.Drawing.Color.Empty;
            this.btnNagWatchForm.Size = new System.Drawing.Size(173, 50);
            this.btnNagWatchForm.TabIndex = 8;
            this.btnNagWatchForm.Text = "主界面";
            this.btnNagWatchForm.UnselectedBackColor = System.Drawing.Color.LightGray;
            this.btnNagWatchForm.UnselectedForeColor = System.Drawing.Color.Empty;
            this.btnNagWatchForm.UseVisualStyleBackColor = false;
            this.btnNagWatchForm.Click += new System.EventHandler(this.btnNavigateForm_Click);
            // 
            // authorityManage
            // 
            this.authorityManage.BackColor = System.Drawing.Color.LightGray;
            this.authorityManage.DialogFormParent = null;
            this.authorityManage.Location = new System.Drawing.Point(3, 3);
            this.authorityManage.Name = "authorityManage";
            this.authorityManage.Size = new System.Drawing.Size(130, 50);
            this.authorityManage.TabIndex = 7;
            // 
            // timeDispSys
            // 
            this.timeDispSys.BackColor = System.Drawing.Color.LightGray;
            this.timeDispSys.Location = new System.Drawing.Point(883, 3);
            this.timeDispSys.Name = "timeDispSys";
            this.timeDispSys.Size = new System.Drawing.Size(130, 50);
            this.timeDispSys.TabIndex = 5;
            // 
            // logText
            // 
            this.logText.Active = false;
            this.logText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.logText.Location = new System.Drawing.Point(3, 59);
            this.logText.LogFilePath = null;
            this.logText.Name = "logText";
            this.logText.Size = new System.Drawing.Size(1010, 119);
            this.logText.TabIndex = 0;
            // 
            // btnNagDeviceManage
            // 
            this.btnNagDeviceManage.BackColor = System.Drawing.Color.LightGray;
            this.btnNagDeviceManage.FlatAppearance.BorderSize = 0;
            this.btnNagDeviceManage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNagDeviceManage.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F);
            this.btnNagDeviceManage.ForeColor = System.Drawing.Color.Black;
            this.btnNagDeviceManage.FormConnect = null;
            this.btnNagDeviceManage.Location = new System.Drawing.Point(346, 4);
            this.btnNagDeviceManage.Name = "btnNagDeviceManage";
            this.btnNagDeviceManage.SelectedBackColor = System.Drawing.Color.LightSkyBlue;
            this.btnNagDeviceManage.SelectedForeColor = System.Drawing.Color.Empty;
            this.btnNagDeviceManage.Size = new System.Drawing.Size(173, 50);
            this.btnNagDeviceManage.TabIndex = 1;
            this.btnNagDeviceManage.Text = "设备管理";
            this.btnNagDeviceManage.UnselectedBackColor = System.Drawing.Color.LightGray;
            this.btnNagDeviceManage.UnselectedForeColor = System.Drawing.Color.Empty;
            this.btnNagDeviceManage.UseVisualStyleBackColor = false;
            this.btnNagDeviceManage.Click += new System.EventHandler(this.btnNavigateForm_Click);
            // 
            // btnNagStartStop
            // 
            this.btnNagStartStop.BackColor = System.Drawing.Color.LightGray;
            this.btnNagStartStop.FlatAppearance.BorderSize = 0;
            this.btnNagStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNagStartStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 26F);
            this.btnNagStartStop.ForeColor = System.Drawing.Color.Black;
            this.btnNagStartStop.FormConnect = null;
            this.btnNagStartStop.Location = new System.Drawing.Point(704, 3);
            this.btnNagStartStop.Name = "btnNagStartStop";
            this.btnNagStartStop.SelectedBackColor = System.Drawing.Color.LightSkyBlue;
            this.btnNagStartStop.SelectedForeColor = System.Drawing.Color.Empty;
            this.btnNagStartStop.Size = new System.Drawing.Size(173, 50);
            this.btnNagStartStop.TabIndex = 0;
            this.btnNagStartStop.Text = "启   动";
            this.btnNagStartStop.UnselectedBackColor = System.Drawing.Color.LightGray;
            this.btnNagStartStop.UnselectedForeColor = System.Drawing.Color.Empty;
            this.btnNagStartStop.UseVisualStyleBackColor = false;
            this.btnNagStartStop.Click += new System.EventHandler(this.btnNavigateEvent_Click);
            // 
            // btnNagParmSet
            // 
            this.btnNagParmSet.BackColor = System.Drawing.Color.LightGray;
            this.btnNagParmSet.FlatAppearance.BorderSize = 0;
            this.btnNagParmSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNagParmSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 25F);
            this.btnNagParmSet.ForeColor = System.Drawing.Color.Black;
            this.btnNagParmSet.FormConnect = null;
            this.btnNagParmSet.Location = new System.Drawing.Point(525, 4);
            this.btnNagParmSet.Name = "btnNagParmSet";
            this.btnNagParmSet.SelectedBackColor = System.Drawing.Color.LightSkyBlue;
            this.btnNagParmSet.SelectedForeColor = System.Drawing.Color.Empty;
            this.btnNagParmSet.Size = new System.Drawing.Size(173, 50);
            this.btnNagParmSet.TabIndex = 4;
            this.btnNagParmSet.Text = "参数设置";
            this.btnNagParmSet.UnselectedBackColor = System.Drawing.Color.LightGray;
            this.btnNagParmSet.UnselectedForeColor = System.Drawing.Color.Empty;
            this.btnNagParmSet.UseVisualStyleBackColor = false;
            this.btnNagParmSet.Click += new System.EventHandler(this.btnNavigateForm_Click);
            // 
            // btnNagCloseForm
            // 
            this.btnNagCloseForm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(112)))), ((int)(((byte)(193)))));
            this.btnNagCloseForm.FlatAppearance.BorderSize = 0;
            this.btnNagCloseForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNagCloseForm.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNagCloseForm.ForeColor = System.Drawing.Color.Red;
            this.btnNagCloseForm.FormConnect = null;
            this.btnNagCloseForm.Location = new System.Drawing.Point(994, 0);
            this.btnNagCloseForm.Name = "btnNagCloseForm";
            this.btnNagCloseForm.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(112)))), ((int)(((byte)(193)))));
            this.btnNagCloseForm.SelectedForeColor = System.Drawing.Color.Red;
            this.btnNagCloseForm.Size = new System.Drawing.Size(30, 30);
            this.btnNagCloseForm.TabIndex = 5;
            this.btnNagCloseForm.Text = "×";
            this.btnNagCloseForm.UnselectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(112)))), ((int)(((byte)(193)))));
            this.btnNagCloseForm.UnselectedForeColor = System.Drawing.Color.Red;
            this.btnNagCloseForm.UseVisualStyleBackColor = false;
            this.btnNagCloseForm.Click += new System.EventHandler(this.btnNavigateEvent_Click);
            // 
            // pnlMiddle
            // 
            this.pnlMiddle.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pnlMiddle.Location = new System.Drawing.Point(3, 30);
            this.pnlMiddle.Name = "pnlMiddle";
            this.pnlMiddle.Size = new System.Drawing.Size(1018, 560);
            this.pnlMiddle.TabIndex = 19;
            // 
            // btnNagMinizeWindow
            // 
            this.btnNagMinizeWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(112)))), ((int)(((byte)(193)))));
            this.btnNagMinizeWindow.FlatAppearance.BorderSize = 0;
            this.btnNagMinizeWindow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNagMinizeWindow.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnNagMinizeWindow.FormConnect = null;
            this.btnNagMinizeWindow.Location = new System.Drawing.Point(934, 0);
            this.btnNagMinizeWindow.Name = "btnNagMinizeWindow";
            this.btnNagMinizeWindow.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(112)))), ((int)(((byte)(193)))));
            this.btnNagMinizeWindow.SelectedForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnNagMinizeWindow.Size = new System.Drawing.Size(30, 30);
            this.btnNagMinizeWindow.TabIndex = 4;
            this.btnNagMinizeWindow.Text = "—";
            this.btnNagMinizeWindow.UnselectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(112)))), ((int)(((byte)(193)))));
            this.btnNagMinizeWindow.UnselectedForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnNagMinizeWindow.UseVisualStyleBackColor = false;
            this.btnNagMinizeWindow.Click += new System.EventHandler(this.btnNavigateEvent_Click);
            // 
            // formsManage
            // 
            this.formsManage.Active = false;
            this.formsManage.FrmCurrentActived = null;
            this.formsManage.MainCtrl = null;
            // 
            // tmrUpdateUI
            // 
            this.tmrUpdateUI.Interval = 500;
            this.tmrUpdateUI.Tick += new System.EventHandler(this.tmrUpdateUI_Tick);
            // 
            // lblProgramName
            // 
            this.lblProgramName.BackColor = System.Drawing.SystemColors.Control;
            this.lblProgramName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblProgramName.ForeColor = System.Drawing.SystemColors.ButtonShadow;
            this.lblProgramName.Location = new System.Drawing.Point(3, 0);
            this.lblProgramName.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.lblProgramName.Name = "lblProgramName";
            this.lblProgramName.Size = new System.Drawing.Size(116, 30);
            this.lblProgramName.TabIndex = 13;
            this.lblProgramName.Text = "焊后检测     ";
            this.lblProgramName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnNagMaxizeWindow
            // 
            this.btnNagMaxizeWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(112)))), ((int)(((byte)(193)))));
            this.btnNagMaxizeWindow.FlatAppearance.BorderSize = 0;
            this.btnNagMaxizeWindow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNagMaxizeWindow.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnNagMaxizeWindow.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnNagMaxizeWindow.FormConnect = null;
            this.btnNagMaxizeWindow.Location = new System.Drawing.Point(964, 0);
            this.btnNagMaxizeWindow.Name = "btnNagMaxizeWindow";
            this.btnNagMaxizeWindow.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(112)))), ((int)(((byte)(193)))));
            this.btnNagMaxizeWindow.SelectedForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnNagMaxizeWindow.Size = new System.Drawing.Size(30, 30);
            this.btnNagMaxizeWindow.TabIndex = 20;
            this.btnNagMaxizeWindow.Text = "口";
            this.btnNagMaxizeWindow.UnselectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(112)))), ((int)(((byte)(193)))));
            this.btnNagMaxizeWindow.UnselectedForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.btnNagMaxizeWindow.UseVisualStyleBackColor = false;
            this.btnNagMaxizeWindow.Click += new System.EventHandler(this.btnNavigateEvent_Click);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.btnNagMaxizeWindow);
            this.Controls.Add(this.lblProgramName);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.btnNagCloseForm);
            this.Controls.Add(this.btnNagMinizeWindow);
            this.Controls.Add(this.pnlMiddle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseUp);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private VisionUserControls.PrimaryControls.ButtonNavigate btnNagDeviceManage;
        private VisionUserControls.PrimaryControls.FormsManage formsManage;
        public System.Windows.Forms.Panel pnlBottom;
        private VisionUserControls.PrimaryControls.ButtonNavigate btnNagParmSet;
        private VisionUserControls.PrimaryControls.Log logText;
        private System.Windows.Forms.Panel pnlMiddle;
        private VisionUserControls.PrimaryControls.TimeDisplay timeDispSys;
        private VisionUserControls.PrimaryControls.ButtonNavigate btnNagStartStop;
        private VisionUserControls.PrimaryControls.ButtonNavigate btnNagCloseForm;
        private VisionUserControls.PrimaryControls.ButtonNavigate btnNagMinizeWindow;
        private VisionUserControls.AuthorityManage.AuthorityManage authorityManage;
        private System.Windows.Forms.Timer tmrUpdateUI;
        public System.Windows.Forms.Label lblProgramName;
        private VisionUserControls.PrimaryControls.ButtonNavigate btnNagMaxizeWindow;
        private VisionUserControls.PrimaryControls.ButtonNavigate btnNagWatchForm;
    }
}


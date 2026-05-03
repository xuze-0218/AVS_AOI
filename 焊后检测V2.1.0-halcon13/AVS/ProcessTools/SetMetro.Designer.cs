
namespace AVS
{
    partial class SetMetro
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetMetro));
            this.HoWindow = new HalconDotNet.HWindowControl();
            this.pnlShowArea = new System.Windows.Forms.Panel();
            this.pnlCtrArea = new System.Windows.Forms.Panel();
            this.tabOpnArea = new System.Windows.Forms.TabControl();
            this.tabSetMetro = new System.Windows.Forms.TabPage();
            this.MetroRefPoint = new System.Windows.Forms.GroupBox();
            this.CbxManualSetMetroRef = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.NbxRefR = new System.Windows.Forms.NumericUpDown();
            this.NbxRefY = new System.Windows.Forms.NumericUpDown();
            this.NbxRefX = new System.Windows.Forms.NumericUpDown();
            this.BtnSearchMatchShape = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.BtnLoadCamImg = new System.Windows.Forms.Button();
            this.BtnLoadLocalImg = new System.Windows.Forms.Button();
            this.LbxMetroObjParam = new System.Windows.Forms.ListBox();
            this.LbxMetroObj = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.RbnObjCircle = new System.Windows.Forms.RadioButton();
            this.RbnObjEllipse = new System.Windows.Forms.RadioButton();
            this.RbnObjLine = new System.Windows.Forms.RadioButton();
            this.RbnObjRect = new System.Windows.Forms.RadioButton();
            this.BtnSaveMetro = new System.Windows.Forms.Button();
            this.BtnTestMetro = new System.Windows.Forms.Button();
            this.BtnRemoveMetroObj = new System.Windows.Forms.Button();
            this.BtnAddMetroObj = new System.Windows.Forms.Button();
            this.BtnReadOtherMetro = new System.Windows.Forms.Button();
            this.BtnReadLocalMetro = new System.Windows.Forms.Button();
            this.tabTestModel = new System.Windows.Forms.TabPage();
            this.LbxTestImgList = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.BtnTestAlignMeasure = new System.Windows.Forms.Button();
            this.BtnRemoveTestImg = new System.Windows.Forms.Button();
            this.BtnAddTestImg = new System.Windows.Forms.Button();
            this.pnlShowArea.SuspendLayout();
            this.pnlCtrArea.SuspendLayout();
            this.tabOpnArea.SuspendLayout();
            this.tabSetMetro.SuspendLayout();
            this.MetroRefPoint.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NbxRefR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbxRefY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbxRefX)).BeginInit();
            this.panel1.SuspendLayout();
            this.tabTestModel.SuspendLayout();
            this.SuspendLayout();
            // 
            // HoWindow
            // 
            this.HoWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HoWindow.BackColor = System.Drawing.Color.Maroon;
            this.HoWindow.BorderColor = System.Drawing.Color.Maroon;
            this.HoWindow.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.HoWindow.Location = new System.Drawing.Point(2, 2);
            this.HoWindow.Margin = new System.Windows.Forms.Padding(2);
            this.HoWindow.Name = "HoWindow";
            this.HoWindow.Size = new System.Drawing.Size(735, 534);
            this.HoWindow.TabIndex = 0;
            this.HoWindow.WindowSize = new System.Drawing.Size(735, 534);
            // 
            // pnlShowArea
            // 
            this.pnlShowArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlShowArea.Controls.Add(this.HoWindow);
            this.pnlShowArea.Location = new System.Drawing.Point(0, 0);
            this.pnlShowArea.Margin = new System.Windows.Forms.Padding(2);
            this.pnlShowArea.Name = "pnlShowArea";
            this.pnlShowArea.Size = new System.Drawing.Size(740, 539);
            this.pnlShowArea.TabIndex = 3;
            // 
            // pnlCtrArea
            // 
            this.pnlCtrArea.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pnlCtrArea.Controls.Add(this.tabOpnArea);
            this.pnlCtrArea.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlCtrArea.Location = new System.Drawing.Point(736, 0);
            this.pnlCtrArea.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCtrArea.Name = "pnlCtrArea";
            this.pnlCtrArea.Size = new System.Drawing.Size(292, 538);
            this.pnlCtrArea.TabIndex = 3;
            // 
            // tabOpnArea
            // 
            this.tabOpnArea.Controls.Add(this.tabSetMetro);
            this.tabOpnArea.Controls.Add(this.tabTestModel);
            this.tabOpnArea.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabOpnArea.Location = new System.Drawing.Point(0, 0);
            this.tabOpnArea.Margin = new System.Windows.Forms.Padding(2);
            this.tabOpnArea.Name = "tabOpnArea";
            this.tabOpnArea.SelectedIndex = 0;
            this.tabOpnArea.Size = new System.Drawing.Size(292, 538);
            this.tabOpnArea.TabIndex = 3;
            // 
            // tabSetMetro
            // 
            this.tabSetMetro.Controls.Add(this.MetroRefPoint);
            this.tabSetMetro.Controls.Add(this.textBox1);
            this.tabSetMetro.Controls.Add(this.BtnLoadCamImg);
            this.tabSetMetro.Controls.Add(this.BtnLoadLocalImg);
            this.tabSetMetro.Controls.Add(this.LbxMetroObjParam);
            this.tabSetMetro.Controls.Add(this.LbxMetroObj);
            this.tabSetMetro.Controls.Add(this.panel1);
            this.tabSetMetro.Controls.Add(this.BtnSaveMetro);
            this.tabSetMetro.Controls.Add(this.BtnTestMetro);
            this.tabSetMetro.Controls.Add(this.BtnRemoveMetroObj);
            this.tabSetMetro.Controls.Add(this.BtnAddMetroObj);
            this.tabSetMetro.Controls.Add(this.BtnReadOtherMetro);
            this.tabSetMetro.Controls.Add(this.BtnReadLocalMetro);
            this.tabSetMetro.Location = new System.Drawing.Point(4, 22);
            this.tabSetMetro.Name = "tabSetMetro";
            this.tabSetMetro.Size = new System.Drawing.Size(284, 512);
            this.tabSetMetro.TabIndex = 2;
            this.tabSetMetro.Text = "卡尺设置";
            this.tabSetMetro.UseVisualStyleBackColor = true;
            // 
            // MetroRefPoint
            // 
            this.MetroRefPoint.Controls.Add(this.CbxManualSetMetroRef);
            this.MetroRefPoint.Controls.Add(this.label3);
            this.MetroRefPoint.Controls.Add(this.label2);
            this.MetroRefPoint.Controls.Add(this.label1);
            this.MetroRefPoint.Controls.Add(this.NbxRefR);
            this.MetroRefPoint.Controls.Add(this.NbxRefY);
            this.MetroRefPoint.Controls.Add(this.NbxRefX);
            this.MetroRefPoint.Controls.Add(this.BtnSearchMatchShape);
            this.MetroRefPoint.Location = new System.Drawing.Point(5, 395);
            this.MetroRefPoint.Name = "MetroRefPoint";
            this.MetroRefPoint.Size = new System.Drawing.Size(271, 74);
            this.MetroRefPoint.TabIndex = 44;
            this.MetroRefPoint.TabStop = false;
            // 
            // CbxManualSetMetroRef
            // 
            this.CbxManualSetMetroRef.AutoSize = true;
            this.CbxManualSetMetroRef.Location = new System.Drawing.Point(29, 21);
            this.CbxManualSetMetroRef.Name = "CbxManualSetMetroRef";
            this.CbxManualSetMetroRef.Size = new System.Drawing.Size(108, 16);
            this.CbxManualSetMetroRef.TabIndex = 2;
            this.CbxManualSetMetroRef.Text = "修改模型参考点";
            this.CbxManualSetMetroRef.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(196, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(11, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "R";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(108, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(11, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Y";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(11, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "X";
            // 
            // NbxRefR
            // 
            this.NbxRefR.DecimalPlaces = 1;
            this.NbxRefR.Location = new System.Drawing.Point(209, 50);
            this.NbxRefR.Maximum = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.NbxRefR.Minimum = new decimal(new int[] {
            720,
            0,
            0,
            -2147483648});
            this.NbxRefR.Name = "NbxRefR";
            this.NbxRefR.Size = new System.Drawing.Size(60, 21);
            this.NbxRefR.TabIndex = 0;
            // 
            // NbxRefY
            // 
            this.NbxRefY.DecimalPlaces = 1;
            this.NbxRefY.Location = new System.Drawing.Point(121, 50);
            this.NbxRefY.Maximum = new decimal(new int[] {
            90000,
            0,
            0,
            0});
            this.NbxRefY.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.NbxRefY.Name = "NbxRefY";
            this.NbxRefY.Size = new System.Drawing.Size(60, 21);
            this.NbxRefY.TabIndex = 0;
            // 
            // NbxRefX
            // 
            this.NbxRefX.DecimalPlaces = 1;
            this.NbxRefX.Location = new System.Drawing.Point(29, 50);
            this.NbxRefX.Maximum = new decimal(new int[] {
            90000,
            0,
            0,
            0});
            this.NbxRefX.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.NbxRefX.Name = "NbxRefX";
            this.NbxRefX.Size = new System.Drawing.Size(60, 21);
            this.NbxRefX.TabIndex = 0;
            // 
            // BtnSearchMatchShape
            // 
            this.BtnSearchMatchShape.Font = new System.Drawing.Font("宋体", 11.25F);
            this.BtnSearchMatchShape.Location = new System.Drawing.Point(161, 15);
            this.BtnSearchMatchShape.Name = "BtnSearchMatchShape";
            this.BtnSearchMatchShape.Size = new System.Drawing.Size(108, 29);
            this.BtnSearchMatchShape.TabIndex = 38;
            this.BtnSearchMatchShape.Text = "搜索匹配点";
            this.BtnSearchMatchShape.UseVisualStyleBackColor = true;
            this.BtnSearchMatchShape.Click += new System.EventHandler(this.BtnSearchMatchModel_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(5, 9);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(272, 21);
            this.textBox1.TabIndex = 43;
            // 
            // BtnLoadCamImg
            // 
            this.BtnLoadCamImg.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnLoadCamImg.Location = new System.Drawing.Point(169, 34);
            this.BtnLoadCamImg.Margin = new System.Windows.Forms.Padding(2);
            this.BtnLoadCamImg.Name = "BtnLoadCamImg";
            this.BtnLoadCamImg.Size = new System.Drawing.Size(110, 30);
            this.BtnLoadCamImg.TabIndex = 42;
            this.BtnLoadCamImg.Text = "加载相机图像";
            this.BtnLoadCamImg.UseVisualStyleBackColor = true;
            this.BtnLoadCamImg.Click += new System.EventHandler(this.BtnLoadCamImg_Click);
            // 
            // BtnLoadLocalImg
            // 
            this.BtnLoadLocalImg.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnLoadLocalImg.Location = new System.Drawing.Point(5, 34);
            this.BtnLoadLocalImg.Margin = new System.Windows.Forms.Padding(2);
            this.BtnLoadLocalImg.Name = "BtnLoadLocalImg";
            this.BtnLoadLocalImg.Size = new System.Drawing.Size(110, 30);
            this.BtnLoadLocalImg.TabIndex = 42;
            this.BtnLoadLocalImg.Text = "加载本地图像";
            this.BtnLoadLocalImg.UseVisualStyleBackColor = true;
            this.BtnLoadLocalImg.Click += new System.EventHandler(this.BtnLoadLocalImg_Click);
            // 
            // LbxMetroObjParam
            // 
            this.LbxMetroObjParam.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LbxMetroObjParam.FormattingEnabled = true;
            this.LbxMetroObjParam.ItemHeight = 14;
            this.LbxMetroObjParam.Location = new System.Drawing.Point(5, 274);
            this.LbxMetroObjParam.Margin = new System.Windows.Forms.Padding(2);
            this.LbxMetroObjParam.Name = "LbxMetroObjParam";
            this.LbxMetroObjParam.ScrollAlwaysVisible = true;
            this.LbxMetroObjParam.Size = new System.Drawing.Size(272, 116);
            this.LbxMetroObjParam.TabIndex = 41;
            // 
            // LbxMetroObj
            // 
            this.LbxMetroObj.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LbxMetroObj.FormattingEnabled = true;
            this.LbxMetroObj.ItemHeight = 14;
            this.LbxMetroObj.Location = new System.Drawing.Point(5, 182);
            this.LbxMetroObj.Margin = new System.Windows.Forms.Padding(2);
            this.LbxMetroObj.Name = "LbxMetroObj";
            this.LbxMetroObj.ScrollAlwaysVisible = true;
            this.LbxMetroObj.Size = new System.Drawing.Size(272, 88);
            this.LbxMetroObj.TabIndex = 40;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.RbnObjCircle);
            this.panel1.Controls.Add(this.RbnObjEllipse);
            this.panel1.Controls.Add(this.RbnObjLine);
            this.panel1.Controls.Add(this.RbnObjRect);
            this.panel1.Location = new System.Drawing.Point(5, 103);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(272, 39);
            this.panel1.TabIndex = 39;
            // 
            // RbnObjCircle
            // 
            this.RbnObjCircle.AutoSize = true;
            this.RbnObjCircle.Checked = true;
            this.RbnObjCircle.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnObjCircle.ForeColor = System.Drawing.Color.Blue;
            this.RbnObjCircle.Location = new System.Drawing.Point(2, 10);
            this.RbnObjCircle.Margin = new System.Windows.Forms.Padding(2);
            this.RbnObjCircle.Name = "RbnObjCircle";
            this.RbnObjCircle.Size = new System.Drawing.Size(53, 18);
            this.RbnObjCircle.TabIndex = 19;
            this.RbnObjCircle.TabStop = true;
            this.RbnObjCircle.Text = "圆形";
            this.RbnObjCircle.UseVisualStyleBackColor = true;
            // 
            // RbnObjEllipse
            // 
            this.RbnObjEllipse.AutoSize = true;
            this.RbnObjEllipse.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnObjEllipse.ForeColor = System.Drawing.Color.Blue;
            this.RbnObjEllipse.Location = new System.Drawing.Point(73, 10);
            this.RbnObjEllipse.Margin = new System.Windows.Forms.Padding(2);
            this.RbnObjEllipse.Name = "RbnObjEllipse";
            this.RbnObjEllipse.Size = new System.Drawing.Size(53, 18);
            this.RbnObjEllipse.TabIndex = 18;
            this.RbnObjEllipse.Text = "椭圆";
            this.RbnObjEllipse.UseVisualStyleBackColor = true;
            // 
            // RbnObjLine
            // 
            this.RbnObjLine.AutoSize = true;
            this.RbnObjLine.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnObjLine.ForeColor = System.Drawing.Color.Blue;
            this.RbnObjLine.Location = new System.Drawing.Point(215, 10);
            this.RbnObjLine.Margin = new System.Windows.Forms.Padding(2);
            this.RbnObjLine.Name = "RbnObjLine";
            this.RbnObjLine.Size = new System.Drawing.Size(53, 18);
            this.RbnObjLine.TabIndex = 21;
            this.RbnObjLine.Text = "直线";
            this.RbnObjLine.UseVisualStyleBackColor = true;
            // 
            // RbnObjRect
            // 
            this.RbnObjRect.AutoSize = true;
            this.RbnObjRect.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnObjRect.ForeColor = System.Drawing.Color.Blue;
            this.RbnObjRect.Location = new System.Drawing.Point(144, 10);
            this.RbnObjRect.Margin = new System.Windows.Forms.Padding(2);
            this.RbnObjRect.Name = "RbnObjRect";
            this.RbnObjRect.Size = new System.Drawing.Size(53, 18);
            this.RbnObjRect.TabIndex = 20;
            this.RbnObjRect.Text = "矩形";
            this.RbnObjRect.UseVisualStyleBackColor = true;
            // 
            // BtnSaveMetro
            // 
            this.BtnSaveMetro.Font = new System.Drawing.Font("宋体", 11.25F);
            this.BtnSaveMetro.Location = new System.Drawing.Point(166, 475);
            this.BtnSaveMetro.Name = "BtnSaveMetro";
            this.BtnSaveMetro.Size = new System.Drawing.Size(108, 29);
            this.BtnSaveMetro.TabIndex = 38;
            this.BtnSaveMetro.Text = "保存测量模型";
            this.BtnSaveMetro.UseVisualStyleBackColor = true;
            this.BtnSaveMetro.Click += new System.EventHandler(this.BtnSaveOneMetro_Click);
            // 
            // BtnTestMetro
            // 
            this.BtnTestMetro.Font = new System.Drawing.Font("宋体", 11.25F);
            this.BtnTestMetro.Location = new System.Drawing.Point(5, 475);
            this.BtnTestMetro.Name = "BtnTestMetro";
            this.BtnTestMetro.Size = new System.Drawing.Size(108, 29);
            this.BtnTestMetro.TabIndex = 38;
            this.BtnTestMetro.Text = "测试测量模型";
            this.BtnTestMetro.UseVisualStyleBackColor = true;
            // 
            // BtnRemoveMetroObj
            // 
            this.BtnRemoveMetroObj.Font = new System.Drawing.Font("宋体", 11.25F);
            this.BtnRemoveMetroObj.Location = new System.Drawing.Point(169, 147);
            this.BtnRemoveMetroObj.Name = "BtnRemoveMetroObj";
            this.BtnRemoveMetroObj.Size = new System.Drawing.Size(108, 29);
            this.BtnRemoveMetroObj.TabIndex = 38;
            this.BtnRemoveMetroObj.Text = "移除测量对象";
            this.BtnRemoveMetroObj.UseVisualStyleBackColor = true;
            // 
            // BtnAddMetroObj
            // 
            this.BtnAddMetroObj.Font = new System.Drawing.Font("宋体", 11.25F);
            this.BtnAddMetroObj.Location = new System.Drawing.Point(5, 147);
            this.BtnAddMetroObj.Name = "BtnAddMetroObj";
            this.BtnAddMetroObj.Size = new System.Drawing.Size(108, 29);
            this.BtnAddMetroObj.TabIndex = 38;
            this.BtnAddMetroObj.Text = "添加测量对象";
            this.BtnAddMetroObj.UseVisualStyleBackColor = true;
            // 
            // BtnReadOtherMetro
            // 
            this.BtnReadOtherMetro.Font = new System.Drawing.Font("宋体", 11.25F);
            this.BtnReadOtherMetro.Location = new System.Drawing.Point(169, 69);
            this.BtnReadOtherMetro.Name = "BtnReadOtherMetro";
            this.BtnReadOtherMetro.Size = new System.Drawing.Size(110, 29);
            this.BtnReadOtherMetro.TabIndex = 38;
            this.BtnReadOtherMetro.Text = "读取其他卡尺";
            this.BtnReadOtherMetro.UseVisualStyleBackColor = true;
            this.BtnReadOtherMetro.Click += new System.EventHandler(this.BtnReadOtherMetro_Click);
            // 
            // BtnReadLocalMetro
            // 
            this.BtnReadLocalMetro.Font = new System.Drawing.Font("宋体", 11.25F);
            this.BtnReadLocalMetro.Location = new System.Drawing.Point(5, 69);
            this.BtnReadLocalMetro.Name = "BtnReadLocalMetro";
            this.BtnReadLocalMetro.Size = new System.Drawing.Size(110, 29);
            this.BtnReadLocalMetro.TabIndex = 38;
            this.BtnReadLocalMetro.Text = "读取本地卡尺";
            this.BtnReadLocalMetro.UseVisualStyleBackColor = true;
            // 
            // tabTestModel
            // 
            this.tabTestModel.Controls.Add(this.LbxTestImgList);
            this.tabTestModel.Controls.Add(this.button1);
            this.tabTestModel.Controls.Add(this.BtnTestAlignMeasure);
            this.tabTestModel.Controls.Add(this.BtnRemoveTestImg);
            this.tabTestModel.Controls.Add(this.BtnAddTestImg);
            this.tabTestModel.Location = new System.Drawing.Point(4, 22);
            this.tabTestModel.Name = "tabTestModel";
            this.tabTestModel.Size = new System.Drawing.Size(284, 512);
            this.tabTestModel.TabIndex = 3;
            this.tabTestModel.Text = "模型测试";
            this.tabTestModel.UseVisualStyleBackColor = true;
            // 
            // LbxTestImgList
            // 
            this.LbxTestImgList.FormattingEnabled = true;
            this.LbxTestImgList.ItemHeight = 12;
            this.LbxTestImgList.Location = new System.Drawing.Point(5, 9);
            this.LbxTestImgList.Name = "LbxTestImgList";
            this.LbxTestImgList.Size = new System.Drawing.Size(272, 172);
            this.LbxTestImgList.TabIndex = 8;
            this.LbxTestImgList.SelectedIndexChanged += new System.EventHandler(this.LbxTestImgList_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(168, 224);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(109, 30);
            this.button1.TabIndex = 6;
            this.button1.Text = "全部测试";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // BtnTestAlignMeasure
            // 
            this.BtnTestAlignMeasure.Location = new System.Drawing.Point(5, 224);
            this.BtnTestAlignMeasure.Margin = new System.Windows.Forms.Padding(2);
            this.BtnTestAlignMeasure.Name = "BtnTestAlignMeasure";
            this.BtnTestAlignMeasure.Size = new System.Drawing.Size(109, 30);
            this.BtnTestAlignMeasure.TabIndex = 6;
            this.BtnTestAlignMeasure.Text = "单张测试";
            this.BtnTestAlignMeasure.UseVisualStyleBackColor = true;
            this.BtnTestAlignMeasure.Click += new System.EventHandler(this.BtnTestAlignMeasure_Click);
            // 
            // BtnRemoveTestImg
            // 
            this.BtnRemoveTestImg.Location = new System.Drawing.Point(169, 190);
            this.BtnRemoveTestImg.Margin = new System.Windows.Forms.Padding(2);
            this.BtnRemoveTestImg.Name = "BtnRemoveTestImg";
            this.BtnRemoveTestImg.Size = new System.Drawing.Size(108, 30);
            this.BtnRemoveTestImg.TabIndex = 7;
            this.BtnRemoveTestImg.Text = "删除图像";
            this.BtnRemoveTestImg.UseVisualStyleBackColor = true;
            // 
            // BtnAddTestImg
            // 
            this.BtnAddTestImg.Location = new System.Drawing.Point(5, 190);
            this.BtnAddTestImg.Margin = new System.Windows.Forms.Padding(2);
            this.BtnAddTestImg.Name = "BtnAddTestImg";
            this.BtnAddTestImg.Size = new System.Drawing.Size(108, 30);
            this.BtnAddTestImg.TabIndex = 7;
            this.BtnAddTestImg.Text = "添加图像";
            this.BtnAddTestImg.UseVisualStyleBackColor = true;
            this.BtnAddTestImg.Click += new System.EventHandler(this.BtnAddTestImg_Click);
            // 
            // SetMetro
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 538);
            this.Controls.Add(this.pnlCtrArea);
            this.Controls.Add(this.pnlShowArea);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SetMetro";
            this.Text = "测量模型设置";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlShowArea.ResumeLayout(false);
            this.pnlCtrArea.ResumeLayout(false);
            this.tabOpnArea.ResumeLayout(false);
            this.tabSetMetro.ResumeLayout(false);
            this.tabSetMetro.PerformLayout();
            this.MetroRefPoint.ResumeLayout(false);
            this.MetroRefPoint.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NbxRefR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbxRefY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NbxRefX)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabTestModel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HalconDotNet.HWindowControl HoWindow;
        private System.Windows.Forms.Panel pnlShowArea;
        private System.Windows.Forms.Panel pnlCtrArea;
        private System.Windows.Forms.TabControl tabOpnArea;
        private System.Windows.Forms.TabPage tabSetMetro;
        private System.Windows.Forms.TabPage tabTestModel;
        private System.Windows.Forms.Button BtnTestAlignMeasure;
        private System.Windows.Forms.Button BtnAddTestImg;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton RbnObjCircle;
        private System.Windows.Forms.RadioButton RbnObjEllipse;
        private System.Windows.Forms.RadioButton RbnObjLine;
        private System.Windows.Forms.RadioButton RbnObjRect;
        private System.Windows.Forms.Button BtnAddMetroObj;
        private System.Windows.Forms.Button BtnReadOtherMetro;
        private System.Windows.Forms.Button BtnReadLocalMetro;
        private System.Windows.Forms.ListBox LbxMetroObjParam;
        private System.Windows.Forms.ListBox LbxMetroObj;
        private System.Windows.Forms.Button BtnRemoveMetroObj;
        private System.Windows.Forms.Button BtnTestMetro;
        private System.Windows.Forms.Button BtnLoadLocalImg;
        private System.Windows.Forms.Button BtnSaveMetro;
        private System.Windows.Forms.GroupBox MetroRefPoint;
        private System.Windows.Forms.NumericUpDown NbxRefR;
        private System.Windows.Forms.NumericUpDown NbxRefY;
        private System.Windows.Forms.NumericUpDown NbxRefX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox CbxManualSetMetroRef;
        private System.Windows.Forms.Button BtnLoadCamImg;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button BtnSearchMatchShape;
        private System.Windows.Forms.ListBox LbxTestImgList;
        private System.Windows.Forms.Button BtnRemoveTestImg;
        private System.Windows.Forms.Button button1;
    }
}


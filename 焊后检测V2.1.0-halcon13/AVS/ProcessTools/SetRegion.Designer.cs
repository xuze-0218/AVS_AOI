
namespace AVS
{
    partial class SetRegion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetRegion));
            this.HoWindow = new HalconDotNet.HWindowControl();
            this.pnlShowArea = new System.Windows.Forms.Panel();
            this.pnlRegionCtr = new System.Windows.Forms.Panel();
            this.BtnLoadImage = new System.Windows.Forms.Button();
            this.BtnSaveRegion = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.BtnCreateRegion = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.RbnUnion2 = new System.Windows.Forms.RadioButton();
            this.RbnIntersection = new System.Windows.Forms.RadioButton();
            this.RbnSymmDiff = new System.Windows.Forms.RadioButton();
            this.RbnDifference = new System.Windows.Forms.RadioButton();
            this.LbxShapeKey = new System.Windows.Forms.ListBox();
            this.LbxShapes = new System.Windows.Forms.ListBox();
            this.BtnSaveTheArea = new System.Windows.Forms.Button();
            this.BtnRemovePoint = new System.Windows.Forms.Button();
            this.BtnInsertPoint = new System.Windows.Forms.Button();
            this.BtnRemoveShape = new System.Windows.Forms.Button();
            this.BtnAddShape = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.RbnCircle = new System.Windows.Forms.RadioButton();
            this.RbnEllipse = new System.Windows.Forms.RadioButton();
            this.RbnPolygon = new System.Windows.Forms.RadioButton();
            this.RbnRectA = new System.Windows.Forms.RadioButton();
            this.RbnRectN = new System.Windows.Forms.RadioButton();
            this.pnlCtrArea = new System.Windows.Forms.Panel();
            this.pnlShowArea.SuspendLayout();
            this.pnlRegionCtr.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnlCtrArea.SuspendLayout();
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
            // pnlRegionCtr
            // 
            this.pnlRegionCtr.BackColor = System.Drawing.Color.PaleGreen;
            this.pnlRegionCtr.Controls.Add(this.BtnLoadImage);
            this.pnlRegionCtr.Controls.Add(this.BtnSaveRegion);
            this.pnlRegionCtr.Controls.Add(this.textBox1);
            this.pnlRegionCtr.Controls.Add(this.BtnCreateRegion);
            this.pnlRegionCtr.Controls.Add(this.panel4);
            this.pnlRegionCtr.Controls.Add(this.LbxShapeKey);
            this.pnlRegionCtr.Controls.Add(this.LbxShapes);
            this.pnlRegionCtr.Controls.Add(this.BtnSaveTheArea);
            this.pnlRegionCtr.Controls.Add(this.BtnRemovePoint);
            this.pnlRegionCtr.Controls.Add(this.BtnInsertPoint);
            this.pnlRegionCtr.Controls.Add(this.BtnRemoveShape);
            this.pnlRegionCtr.Controls.Add(this.BtnAddShape);
            this.pnlRegionCtr.Controls.Add(this.panel2);
            this.pnlRegionCtr.Location = new System.Drawing.Point(5, 11);
            this.pnlRegionCtr.Margin = new System.Windows.Forms.Padding(2);
            this.pnlRegionCtr.Name = "pnlRegionCtr";
            this.pnlRegionCtr.Size = new System.Drawing.Size(285, 525);
            this.pnlRegionCtr.TabIndex = 24;
            // 
            // BtnLoadImage
            // 
            this.BtnLoadImage.Location = new System.Drawing.Point(10, 32);
            this.BtnLoadImage.Margin = new System.Windows.Forms.Padding(2);
            this.BtnLoadImage.Name = "BtnLoadImage";
            this.BtnLoadImage.Size = new System.Drawing.Size(260, 30);
            this.BtnLoadImage.TabIndex = 26;
            this.BtnLoadImage.Text = "加载窗口图像";
            this.BtnLoadImage.UseVisualStyleBackColor = true;
            // 
            // BtnSaveRegion
            // 
            this.BtnSaveRegion.Location = new System.Drawing.Point(144, 480);
            this.BtnSaveRegion.Margin = new System.Windows.Forms.Padding(2);
            this.BtnSaveRegion.Name = "BtnSaveRegion";
            this.BtnSaveRegion.Size = new System.Drawing.Size(126, 39);
            this.BtnSaveRegion.TabIndex = 1;
            this.BtnSaveRegion.Text = "保存区域";
            this.BtnSaveRegion.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(10, 7);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(260, 21);
            this.textBox1.TabIndex = 2;
            // 
            // BtnCreateRegion
            // 
            this.BtnCreateRegion.Location = new System.Drawing.Point(10, 480);
            this.BtnCreateRegion.Margin = new System.Windows.Forms.Padding(2);
            this.BtnCreateRegion.Name = "BtnCreateRegion";
            this.BtnCreateRegion.Size = new System.Drawing.Size(126, 39);
            this.BtnCreateRegion.TabIndex = 1;
            this.BtnCreateRegion.Text = "生成区域";
            this.BtnCreateRegion.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.RbnUnion2);
            this.panel4.Controls.Add(this.RbnIntersection);
            this.panel4.Controls.Add(this.RbnSymmDiff);
            this.panel4.Controls.Add(this.RbnDifference);
            this.panel4.Location = new System.Drawing.Point(10, 113);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(260, 39);
            this.panel4.TabIndex = 23;
            // 
            // RbnUnion2
            // 
            this.RbnUnion2.AutoSize = true;
            this.RbnUnion2.Checked = true;
            this.RbnUnion2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnUnion2.ForeColor = System.Drawing.Color.Black;
            this.RbnUnion2.Location = new System.Drawing.Point(4, 10);
            this.RbnUnion2.Margin = new System.Windows.Forms.Padding(2);
            this.RbnUnion2.Name = "RbnUnion2";
            this.RbnUnion2.Size = new System.Drawing.Size(47, 16);
            this.RbnUnion2.TabIndex = 19;
            this.RbnUnion2.TabStop = true;
            this.RbnUnion2.Text = "并集";
            this.RbnUnion2.UseVisualStyleBackColor = true;
            // 
            // RbnIntersection
            // 
            this.RbnIntersection.AutoSize = true;
            this.RbnIntersection.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnIntersection.ForeColor = System.Drawing.Color.Black;
            this.RbnIntersection.Location = new System.Drawing.Point(55, 10);
            this.RbnIntersection.Margin = new System.Windows.Forms.Padding(2);
            this.RbnIntersection.Name = "RbnIntersection";
            this.RbnIntersection.Size = new System.Drawing.Size(47, 16);
            this.RbnIntersection.TabIndex = 18;
            this.RbnIntersection.Text = "交集";
            this.RbnIntersection.UseVisualStyleBackColor = true;
            // 
            // RbnSymmDiff
            // 
            this.RbnSymmDiff.AutoSize = true;
            this.RbnSymmDiff.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnSymmDiff.ForeColor = System.Drawing.Color.Black;
            this.RbnSymmDiff.Location = new System.Drawing.Point(160, 10);
            this.RbnSymmDiff.Margin = new System.Windows.Forms.Padding(2);
            this.RbnSymmDiff.Name = "RbnSymmDiff";
            this.RbnSymmDiff.Size = new System.Drawing.Size(59, 16);
            this.RbnSymmDiff.TabIndex = 21;
            this.RbnSymmDiff.Text = "对称差";
            this.RbnSymmDiff.UseVisualStyleBackColor = true;
            // 
            // RbnDifference
            // 
            this.RbnDifference.AutoSize = true;
            this.RbnDifference.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnDifference.ForeColor = System.Drawing.Color.Black;
            this.RbnDifference.Location = new System.Drawing.Point(108, 9);
            this.RbnDifference.Margin = new System.Windows.Forms.Padding(2);
            this.RbnDifference.Name = "RbnDifference";
            this.RbnDifference.Size = new System.Drawing.Size(47, 16);
            this.RbnDifference.TabIndex = 20;
            this.RbnDifference.Text = "差集";
            this.RbnDifference.UseVisualStyleBackColor = true;
            // 
            // LbxShapeKey
            // 
            this.LbxShapeKey.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LbxShapeKey.FormattingEnabled = true;
            this.LbxShapeKey.ItemHeight = 14;
            this.LbxShapeKey.Location = new System.Drawing.Point(10, 321);
            this.LbxShapeKey.Margin = new System.Windows.Forms.Padding(2);
            this.LbxShapeKey.Name = "LbxShapeKey";
            this.LbxShapeKey.ScrollAlwaysVisible = true;
            this.LbxShapeKey.Size = new System.Drawing.Size(260, 144);
            this.LbxShapeKey.TabIndex = 4;
            this.LbxShapeKey.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LbxShapeKey_MouseDoubleClick);
            // 
            // LbxShapes
            // 
            this.LbxShapes.FormattingEnabled = true;
            this.LbxShapes.ItemHeight = 12;
            this.LbxShapes.Location = new System.Drawing.Point(10, 213);
            this.LbxShapes.Margin = new System.Windows.Forms.Padding(2);
            this.LbxShapes.Name = "LbxShapes";
            this.LbxShapes.ScrollAlwaysVisible = true;
            this.LbxShapes.Size = new System.Drawing.Size(260, 100);
            this.LbxShapes.TabIndex = 4;
            // 
            // BtnSaveTheArea
            // 
            this.BtnSaveTheArea.Location = new System.Drawing.Point(10, 545);
            this.BtnSaveTheArea.Margin = new System.Windows.Forms.Padding(2);
            this.BtnSaveTheArea.Name = "BtnSaveTheArea";
            this.BtnSaveTheArea.Size = new System.Drawing.Size(128, 29);
            this.BtnSaveTheArea.TabIndex = 1;
            this.BtnSaveTheArea.Text = "Save The Area";
            this.BtnSaveTheArea.UseVisualStyleBackColor = true;
            // 
            // BtnRemovePoint
            // 
            this.BtnRemovePoint.Enabled = false;
            this.BtnRemovePoint.Font = new System.Drawing.Font("楷体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnRemovePoint.Location = new System.Drawing.Point(210, 158);
            this.BtnRemovePoint.Margin = new System.Windows.Forms.Padding(2);
            this.BtnRemovePoint.Name = "BtnRemovePoint";
            this.BtnRemovePoint.Size = new System.Drawing.Size(60, 48);
            this.BtnRemovePoint.TabIndex = 1;
            this.BtnRemovePoint.Text = "删除\r\n顶点";
            this.BtnRemovePoint.UseVisualStyleBackColor = true;
            // 
            // BtnInsertPoint
            // 
            this.BtnInsertPoint.Enabled = false;
            this.BtnInsertPoint.Font = new System.Drawing.Font("楷体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnInsertPoint.Location = new System.Drawing.Point(144, 158);
            this.BtnInsertPoint.Margin = new System.Windows.Forms.Padding(2);
            this.BtnInsertPoint.Name = "BtnInsertPoint";
            this.BtnInsertPoint.Size = new System.Drawing.Size(60, 48);
            this.BtnInsertPoint.TabIndex = 1;
            this.BtnInsertPoint.Text = "添加\r\n顶点";
            this.BtnInsertPoint.UseVisualStyleBackColor = true;
            // 
            // BtnRemoveShape
            // 
            this.BtnRemoveShape.Font = new System.Drawing.Font("楷体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnRemoveShape.Location = new System.Drawing.Point(76, 158);
            this.BtnRemoveShape.Margin = new System.Windows.Forms.Padding(2);
            this.BtnRemoveShape.Name = "BtnRemoveShape";
            this.BtnRemoveShape.Size = new System.Drawing.Size(60, 48);
            this.BtnRemoveShape.TabIndex = 1;
            this.BtnRemoveShape.Text = "删除\r\n图形";
            this.BtnRemoveShape.UseVisualStyleBackColor = true;
            // 
            // BtnAddShape
            // 
            this.BtnAddShape.Font = new System.Drawing.Font("楷体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnAddShape.Location = new System.Drawing.Point(10, 158);
            this.BtnAddShape.Margin = new System.Windows.Forms.Padding(2);
            this.BtnAddShape.Name = "BtnAddShape";
            this.BtnAddShape.Size = new System.Drawing.Size(60, 48);
            this.BtnAddShape.TabIndex = 1;
            this.BtnAddShape.Text = "添加\r\n图形";
            this.BtnAddShape.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.RbnCircle);
            this.panel2.Controls.Add(this.RbnEllipse);
            this.panel2.Controls.Add(this.RbnPolygon);
            this.panel2.Controls.Add(this.RbnRectA);
            this.panel2.Controls.Add(this.RbnRectN);
            this.panel2.Location = new System.Drawing.Point(10, 66);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(260, 39);
            this.panel2.TabIndex = 22;
            // 
            // RbnCircle
            // 
            this.RbnCircle.AutoSize = true;
            this.RbnCircle.Checked = true;
            this.RbnCircle.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnCircle.ForeColor = System.Drawing.Color.Blue;
            this.RbnCircle.Location = new System.Drawing.Point(4, 12);
            this.RbnCircle.Margin = new System.Windows.Forms.Padding(2);
            this.RbnCircle.Name = "RbnCircle";
            this.RbnCircle.Size = new System.Drawing.Size(47, 16);
            this.RbnCircle.TabIndex = 19;
            this.RbnCircle.TabStop = true;
            this.RbnCircle.Text = "正圆";
            this.RbnCircle.UseVisualStyleBackColor = true;
            // 
            // RbnEllipse
            // 
            this.RbnEllipse.AutoSize = true;
            this.RbnEllipse.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnEllipse.ForeColor = System.Drawing.Color.Blue;
            this.RbnEllipse.Location = new System.Drawing.Point(55, 12);
            this.RbnEllipse.Margin = new System.Windows.Forms.Padding(2);
            this.RbnEllipse.Name = "RbnEllipse";
            this.RbnEllipse.Size = new System.Drawing.Size(47, 16);
            this.RbnEllipse.TabIndex = 18;
            this.RbnEllipse.Text = "椭圆";
            this.RbnEllipse.UseVisualStyleBackColor = true;
            // 
            // RbnPolygon
            // 
            this.RbnPolygon.AutoSize = true;
            this.RbnPolygon.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnPolygon.ForeColor = System.Drawing.Color.Blue;
            this.RbnPolygon.Location = new System.Drawing.Point(212, 12);
            this.RbnPolygon.Margin = new System.Windows.Forms.Padding(2);
            this.RbnPolygon.Name = "RbnPolygon";
            this.RbnPolygon.Size = new System.Drawing.Size(47, 16);
            this.RbnPolygon.TabIndex = 21;
            this.RbnPolygon.Text = "多点";
            this.RbnPolygon.UseVisualStyleBackColor = true;
            // 
            // RbnRectA
            // 
            this.RbnRectA.AutoSize = true;
            this.RbnRectA.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnRectA.ForeColor = System.Drawing.Color.Blue;
            this.RbnRectA.Location = new System.Drawing.Point(160, 12);
            this.RbnRectA.Margin = new System.Windows.Forms.Padding(2);
            this.RbnRectA.Name = "RbnRectA";
            this.RbnRectA.Size = new System.Drawing.Size(47, 16);
            this.RbnRectA.TabIndex = 21;
            this.RbnRectA.Text = "斜矩";
            this.RbnRectA.UseVisualStyleBackColor = true;
            // 
            // RbnRectN
            // 
            this.RbnRectN.AutoSize = true;
            this.RbnRectN.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RbnRectN.ForeColor = System.Drawing.Color.Blue;
            this.RbnRectN.Location = new System.Drawing.Point(108, 12);
            this.RbnRectN.Margin = new System.Windows.Forms.Padding(2);
            this.RbnRectN.Name = "RbnRectN";
            this.RbnRectN.Size = new System.Drawing.Size(47, 16);
            this.RbnRectN.TabIndex = 20;
            this.RbnRectN.Text = "正矩";
            this.RbnRectN.UseVisualStyleBackColor = true;
            // 
            // pnlCtrArea
            // 
            this.pnlCtrArea.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pnlCtrArea.Controls.Add(this.pnlRegionCtr);
            this.pnlCtrArea.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlCtrArea.Location = new System.Drawing.Point(736, 0);
            this.pnlCtrArea.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCtrArea.Name = "pnlCtrArea";
            this.pnlCtrArea.Size = new System.Drawing.Size(292, 538);
            this.pnlCtrArea.TabIndex = 3;
            // 
            // SetRegion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 538);
            this.Controls.Add(this.pnlCtrArea);
            this.Controls.Add(this.pnlShowArea);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SetRegion";
            this.Text = "匹配模板设置";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlShowArea.ResumeLayout(false);
            this.pnlRegionCtr.ResumeLayout(false);
            this.pnlRegionCtr.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.pnlCtrArea.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HalconDotNet.HWindowControl HoWindow;
        private System.Windows.Forms.Panel pnlShowArea;
        private System.Windows.Forms.Panel pnlCtrArea;
        private System.Windows.Forms.Panel pnlRegionCtr;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.RadioButton RbnUnion2;
        private System.Windows.Forms.RadioButton RbnIntersection;
        private System.Windows.Forms.RadioButton RbnSymmDiff;
        private System.Windows.Forms.RadioButton RbnDifference;
        private System.Windows.Forms.ListBox LbxShapes;
        private System.Windows.Forms.Button BtnCreateRegion;
        private System.Windows.Forms.Button BtnSaveTheArea;
        private System.Windows.Forms.Button BtnRemovePoint;
        private System.Windows.Forms.Button BtnInsertPoint;
        private System.Windows.Forms.Button BtnRemoveShape;
        private System.Windows.Forms.Button BtnAddShape;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton RbnCircle;
        private System.Windows.Forms.RadioButton RbnEllipse;
        private System.Windows.Forms.RadioButton RbnPolygon;
        private System.Windows.Forms.RadioButton RbnRectA;
        private System.Windows.Forms.RadioButton RbnRectN;
        private System.Windows.Forms.ListBox LbxShapeKey;
        private System.Windows.Forms.Button BtnSaveRegion;
        private System.Windows.Forms.Button BtnLoadImage;
        private System.Windows.Forms.TextBox textBox1;
    }
}


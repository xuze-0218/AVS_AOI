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
    public partial class InputBoxRegion : Form
    {
        enum EnumType
        {
            中心点_Y = 0,
            中心点_X = 0,
            半径长_R = 0,
            主轴半长 = 0,
            顶点     = 0,

            纵坐标_Y = 0,
            起始点_Y = 0,
            横坐标_X = 0,
            起始点_X = 0,

            终止点_Y = 0,
            终止点_X = 0,
            半径长度 = 0,
            次轴半长 = 0,

            起始角度 = 1,
            终止角度 = 1,
            主轴角度 = 1,
            主轴方向 = 1,

            测框长_1 = 2,//1.0 ≤measure_length，10.0, 20.0, 30.0
            测框长_2 = 2,//

            平滑系数 = 3,//0.4 ≤ Sigma ≤ 100, 推荐值0.4 - 10，递增量0.1,
            幅度阈值 = 4,//1.0 ≤ Threshold ≤ 255, 推荐值5.0 - 110.0，递增量2.0
            最小分数 = 5,//0.1 ≤ MinScore ≤ 1.0，0.5, 0.7, 0.9
            实例个数 = 6,//1.0 ≤ Instances ≤ 200，1, 2, 3, 4
            测量间距 = 7,//5.0, 15.0, 20.0, 30.0
            测量极性 = 8,// 'all', 'negative', 'positive', 'uniform'

        }


        public HTuple valueForRenew { get; set; }

        public object obj;

        public InputBoxRegion()
        {
            InitializeComponent();
        }

        public InputBoxRegion(string itemName, HTuple itemOne)
        {
            InitializeComponent();

            obj = CheckForParamLimit(itemName, itemOne);
  
            this.Text = itemName;
        }
        private void InputBox_Load(object sender, EventArgs e)
        {
            this.AcceptButton = this.BtnOK;
            this.CancelButton = this.BtnCancel;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            BtnOK.Click += BtnOk_Click;
            BtnCancel.Click += BtnCancel_Click;
        }

        //确定
        private void BtnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;

            if(obj is NumericUpDown)
            {
                NumericUpDown objNbx = (NumericUpDown)obj;
                valueForRenew = (double)(objNbx.Value);
            }
            else if(obj is ComboBox)
            {
                ComboBox objLbx = (ComboBox)obj;
                valueForRenew = (string)(objLbx.SelectedItem);
            }
            this.Close();
        }
        //取消
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private object CheckForParamLimit(string itemName, HTuple itemOne)
        {
            if(itemName.Contains("顶点"))
            {
                itemName = "顶点";
            }
            if(itemName.Contains(":"))
            {
                itemName = itemName.Replace(":","") ;
            }

            var itemType = Enum.Parse(typeof(EnumType), itemName);
            if((int)itemType > -1 && (int)itemType < 8)
            {
                NumericUpDown nbx = new NumericUpDown();
                nbx.Parent = this;
                nbx.Location = new System.Drawing.Point(30, 30);
                nbx.Size = new System.Drawing.Size(200, 30); ;
                nbx.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));

                if ((int)itemType == 0)
                {
                    nbx.DecimalPlaces = 1;
                    nbx.Increment = 1;
                    nbx.Minimum = -20000;
                    nbx.Maximum = 20000;
                }
                else if ((int)itemType == 1)//角度设置
                {
                    nbx.DecimalPlaces = 1;
                    nbx.Increment = 1;
                    nbx.Minimum = 0;
                    nbx.Maximum = 360;
                }
                else if((int)itemType == 2)//测量框长
                {
                    nbx.DecimalPlaces = 1;
                    nbx.Increment = 1;
                    nbx.Minimum = 1;
                    nbx.Maximum = 200;
                }
                else if ((int)itemType == 3)//平滑系数
                {
                    nbx.DecimalPlaces = 1;
                    nbx.Increment = (decimal)0.1;
                    nbx.Minimum = (decimal)0.4;
                    nbx.Maximum = 10;
                }
                else if ((int)itemType == 4)//幅度值
                {
                    nbx.DecimalPlaces = 1;
                    nbx.Increment = 2;
                    nbx.Minimum = 1;
                    nbx.Maximum = 255;
                }
                else if ((int)itemType == 5)//最小得分
                {
                    nbx.DecimalPlaces = 2;
                    nbx.Increment = (decimal)0.1;
                    nbx.Minimum = (decimal)0.1;
                    nbx.Maximum = (decimal)0.99;
                }
                else if ((int)itemType == 6)//实例个数
                {
                    nbx.DecimalPlaces = 0;
                    nbx.Increment = 1;
                    nbx.Minimum = 1;
                    nbx.Maximum = 100;
                }
                else if ((int)itemType == 7)//测量间距
                {
                    nbx.DecimalPlaces = 1;
                    nbx.Increment = 1;
                    nbx.Minimum = 1;
                    nbx.Maximum = 200;
                }

                nbx.Value = Convert.ToDecimal(itemOne.D);
                nbx.Show();
                return nbx;
            }
            else
            {
                ComboBox Cbx = new ComboBox();
                Cbx.Parent = this;
                Cbx.Items.Add("all");
                Cbx.Items.Add("negative");
                Cbx.Items.Add("positive");
                Cbx.Items.Add("uniform");
                Cbx.SelectedIndex = Cbx.Items.IndexOf((string)(itemOne.S));

                Cbx.SelectedItem = itemOne;
                Cbx.Location = new System.Drawing.Point(30, 30);    
                Cbx.Show(); 
                return Cbx;
            }
        }

    }
}

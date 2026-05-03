using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;

namespace AVS
{
    public partial class SetMetro : Form
    {
        enum EnumPsn
        {
            纵坐标_Y = 0,
            起始点_Y = 0,
            横坐标_X = 1,
            起始点_X = 1,

            主轴方向 = 2,
            终止点_Y = 2,
            终止点_X = 3,
            半径长度 = 4,
            主轴半长 = 4,
            次轴半长 = 5,
            起始角度 = 6,
            终止角度 = 7,

            测框长_1 = 10,
            测框长_2 = 11,
            最小分数 = 12,
            平滑系数 = 13,
            幅度阈值 = 14,
            实例个数 = 15,
            测量间距 = 16,
            测量极性 = 17,
            测量选择 = 18,
            插值方法 = 19,
        }
        #region 图形参数
        //定义形状结构体
        public struct MetroModelShape
        {
            public HTuple refRow;
            public HTuple refCol;
            public HTuple refAngle;
            public List<ShapeMetro> shapeMetros;
        }

        public struct ShapeMetro
        {
            public HTuple[] metroParams;
            public Shape MyShape;
        }

        public struct ShapeRegion
        {
            public string OpnMode { get; set; }

            public Shape MyShape;
        }
        public struct Shape
        {
            //public string OpnMode { get; set; }
            public string ShapeType { get; set; }

            public List<HTuple[]> KeyPoints;// 创建一个顶点列表
            public List<HTuple[]> CtrPoints;
        }

        // 添加圆形
        private void CreateCircle(Shape shape)
        {
            HTuple row1 = new HTuple(), col1 = new HTuple();
            HTuple row2 = new HTuple(), col2 = new HTuple();
            HTuple min = new HTuple();
            hWindow.HalconWindow.GetPart(out row1, out col1, out row2, out col2);
            HTuple width = (col2 - col1);
            HTuple height = (row2 - row1);
            HOperatorSet.TupleMin2(width, height, out min);

            shape.KeyPoints.Clear();
            shape.KeyPoints.Add(new HTuple[] { (row1 + row2) * 0.5, (col1 + col2) * 0.5 });//中心点X,Y
            shape.KeyPoints.Add(new HTuple[] { 0, 0 });//水平角度
            shape.KeyPoints.Add(new HTuple[] { min * 0.25, min * 0.25 });//半径
            shape.KeyPoints.Add(new HTuple[] { 0, 360 });//起始角
        }

        // 添加椭圆形
        private void CreateEllipse(Shape shape)
        {
            HTuple row1 = new HTuple(), col1 = new HTuple();
            HTuple row2 = new HTuple(), col2 = new HTuple();
            HTuple min = new HTuple();
            hWindow.HalconWindow.GetPart(out row1, out col1, out row2, out col2);
            HTuple width = (col2 - col1);
            HTuple height = (row2 - row1);
            HOperatorSet.TupleMin2(width, height, out min);

            shape.KeyPoints.Clear();
            shape.KeyPoints.Add(new HTuple[] { (row1 + row2) * 0.5, (col1 + col2) * 0.5 });//中心X,Y
            shape.KeyPoints.Add(new HTuple[] { 000, 000 });//水平角度
            shape.KeyPoints.Add(new HTuple[] { min * 0.5, min * 0.25 });//长轴，短轴
            shape.KeyPoints.Add(new HTuple[] { 0, 360 });//起止角度
        }

        // 添加矩形
        private void CreateRectangle(Shape shape)
        {
            HTuple row1 = new HTuple(), col1 = new HTuple();
            HTuple row2 = new HTuple(), col2 = new HTuple();
            HTuple min = new HTuple();
            hWindow.HalconWindow.GetPart(out row1, out col1, out row2, out col2);
            HTuple width = (col2 - col1);
            HTuple height = (row2 - row1);
            HOperatorSet.TupleMin2(width, height, out min);

            shape.KeyPoints.Clear();
            shape.KeyPoints.Add(new HTuple[] { row1 * 0.75 + row2 * 0.25, col1 * 0.75 + col2 * 0.25 });//左上点
            shape.KeyPoints.Add(new HTuple[] { row1 * 0.25 + row2 * 0.75, col1 * 0.25 + col2 * 0.75 });//右下点
        }

        // 添加旋转矩形
        private void CreateRectangle2(Shape shape)
        {
            HTuple row1 = new HTuple(), col1 = new HTuple();
            HTuple row2 = new HTuple(), col2 = new HTuple();
            HTuple min = new HTuple();
            hWindow.HalconWindow.GetPart(out row1, out col1, out row2, out col2);
            HTuple width = (col2 - col1);
            HTuple height = (row2 - row1);
            HOperatorSet.TupleMin2(width, height, out min);

            shape.KeyPoints.Clear();
            shape.KeyPoints.Add(new HTuple[] { (row1 + row2) * 0.5, (col1 + col2) * 0.5 });//中心X,Y
            shape.KeyPoints.Add(new HTuple[] { 000, 000 });//水平角度
            shape.KeyPoints.Add(new HTuple[] { min * 0.5, min * 0.25 });//长轴，短轴
            //shape.KeyPoints.Add(new HTuple[] { 0, 6.28318 });//起止角度
        }

        // 添加 Polygon
        private void CreatePolygon(Shape shape)
        {
            HTuple row1 = new HTuple(), col1 = new HTuple();
            HTuple row2 = new HTuple(), col2 = new HTuple();
            HTuple min = new HTuple();
            hWindow.HalconWindow.GetPart(out row1, out col1, out row2, out col2);
            HTuple width = (col2 - col1);
            HTuple height = (row2 - row1);
            HOperatorSet.TupleMin2(width, height, out min);

            shape.KeyPoints.Clear();
            shape.KeyPoints.Add(new HTuple[] { row1 * 0.75 + row2 * 0.25, col1 * 0.75 + col2 * 0.25 });
            shape.KeyPoints.Add(new HTuple[] { row1 * 0.25 + row2 * 0.75, col1 * 0.75 + col2 * 0.25 });
            shape.KeyPoints.Add(new HTuple[] { (row1 + row2) * 0.5, (col1 + col2) * 0.5 });
        }

        // 图形绘制，并根据关键点计算出拖拽控制点
        public void DrawCircle(HTuple windowHandle, Shape myShape, string color)
        {
            HTuple row = myShape.KeyPoints[0][0];
            HTuple col = myShape.KeyPoints[0][1];
            HTuple radius = myShape.KeyPoints[2][0];
            HTuple startPhi = myShape.KeyPoints[3][0].TupleRad();
            HTuple endPhi = myShape.KeyPoints[3][1].TupleRad();

            myShape.CtrPoints.Clear();
            myShape.CtrPoints.Add(new HTuple[] { row, col });
            myShape.CtrPoints.Add(new HTuple[] { row - radius, col });
            myShape.CtrPoints.Add(new HTuple[] { row + radius, col });
            myShape.CtrPoints.Add(new HTuple[] { row, col - radius });
            myShape.CtrPoints.Add(new HTuple[] { row, col + radius });

            HTuple rowCtr = new HTuple();
            rowCtr[0] = row;
            rowCtr[1] = row - radius;
            rowCtr[2] = row + radius;
            rowCtr[3] = row;
            rowCtr[4] = row;

            HTuple colCtr = new HTuple();
            colCtr[0] = col;
            colCtr[1] = col;
            colCtr[2] = col;
            colCtr[3] = col - radius;
            colCtr[4] = col + radius;

            HTuple rectPhi = new HTuple();
            rectPhi[0] = 0;
            rectPhi[1] = 0;
            rectPhi[2] = 0;
            rectPhi[3] = 0;
            rectPhi[4] = 0;

            HTuple rectL01 = new HTuple();
            rectL01[0] = 20 * zoomWndFactor;
            rectL01[1] = 20 * zoomWndFactor;
            rectL01[2] = 20 * zoomWndFactor;
            rectL01[3] = 20 * zoomWndFactor;
            rectL01[4] = 20 * zoomWndFactor;

            HTuple rectL02 = new HTuple();
            rectL02[0] = 20 * zoomWndFactor;
            rectL02[1] = 20 * zoomWndFactor;
            rectL02[2] = 20 * zoomWndFactor;
            rectL02[3] = 20 * zoomWndFactor;
            rectL02[4] = 20 * zoomWndFactor;
            HObject myShapeXLD = new HObject(), ctrXLD = new HObject();
            HOperatorSet.GenCircleContourXld(out myShapeXLD, row, col, radius, startPhi, endPhi, "positive", 1);
            HOperatorSet.GenRectangle2ContourXld(out ctrXLD, rowCtr, colCtr, rectPhi, rectL01, rectL02);
            HOperatorSet.SetColor(windowHandle, color);
            HOperatorSet.DispObj(myShapeXLD, windowHandle);
            HOperatorSet.DispObj(ctrXLD, windowHandle);
        }
        public void DrawEllipse(HTuple windowHandle, Shape myShape, string color)
        {
            HTuple row = myShape.KeyPoints[0][0];
            HTuple col = myShape.KeyPoints[0][1];
            HTuple phi = myShape.KeyPoints[1][0].TupleRad();
            HTuple radius1 = myShape.KeyPoints[2][0];
            HTuple radius2 = myShape.KeyPoints[2][1];
            HTuple startPhi = myShape.KeyPoints[3][0].TupleRad();
            HTuple endPhi = myShape.KeyPoints[3][1].TupleRad();

            HTuple controlRow1 = row + (((new HTuple((new HTuple(0 + 90)).TupleRad()) + phi).TupleCos()) * radius1);
            HTuple controlCol1 = col + (((new HTuple((new HTuple(0 + 90)).TupleRad()) + phi).TupleSin()) * radius1);

            HTuple controlRow2 = row + (((new HTuple((new HTuple(0 + 180)).TupleRad()) + phi).TupleCos()) * radius2);
            HTuple controlCol2 = col + (((new HTuple((new HTuple(0 + 180)).TupleRad()) + phi).TupleSin()) * radius2);

            HTuple controlRow3 = row + (((new HTuple((new HTuple(0 + 270)).TupleRad()) + phi).TupleCos()) * radius1);
            HTuple controlCol3 = col + (((new HTuple((new HTuple(0 + 270)).TupleRad()) + phi).TupleSin()) * radius1);

            HTuple controlRow4 = row + (((new HTuple((new HTuple(0 + 000)).TupleRad()) + phi).TupleCos()) * radius2);
            HTuple controlCol4 = col + (((new HTuple((new HTuple(0 + 000)).TupleRad()) + phi).TupleSin()) * radius2);

            HTuple controlRow5 = row + (((new HTuple((new HTuple(0 + 90)).TupleRad()) + phi).TupleCos()) * (radius1 + 36));
            HTuple controlCol5 = col + (((new HTuple((new HTuple(0 + 90)).TupleRad()) + phi).TupleSin()) * (radius1 + 36));

            myShape.CtrPoints.Clear();
            myShape.CtrPoints.Add(new HTuple[] { row, col });
            myShape.CtrPoints.Add(new HTuple[] { controlRow1, controlCol1 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow2, controlCol2 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow3, controlCol3 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow4, controlCol4 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow5, controlCol5 });

            HTuple rowCtr = new HTuple();
            rowCtr[0] = row;
            rowCtr[1] = controlRow1;
            rowCtr[2] = controlRow2;
            rowCtr[3] = controlRow3;
            rowCtr[4] = controlRow4;
            rowCtr[5] = controlRow5;

            HTuple colCtr = new HTuple();
            colCtr[0] = col;
            colCtr[1] = controlCol1;
            colCtr[2] = controlCol2;
            colCtr[3] = controlCol3;
            colCtr[4] = controlCol4;
            colCtr[5] = controlCol5;

            HTuple rectPhi = new HTuple();
            rectPhi[0] = phi;
            rectPhi[1] = phi;
            rectPhi[2] = phi;
            rectPhi[3] = phi;
            rectPhi[4] = phi;
            rectPhi[5] = phi;

            HTuple rectL01 = new HTuple();
            rectL01[0] = 20 * zoomWndFactor;
            rectL01[1] = 20 * zoomWndFactor;
            rectL01[2] = 20 * zoomWndFactor;
            rectL01[3] = 20 * zoomWndFactor;
            rectL01[4] = 20 * zoomWndFactor;
            rectL01[5] = 20 * zoomWndFactor;

            HTuple rectL02 = new HTuple();
            rectL02[0] = 20 * zoomWndFactor;
            rectL02[1] = 20 * zoomWndFactor;
            rectL02[2] = 20 * zoomWndFactor;
            rectL02[3] = 20 * zoomWndFactor;
            rectL02[4] = 20 * zoomWndFactor;
            rectL02[5] = 20 * zoomWndFactor;
            HObject myShapeXLD = new HObject(), ctrXLD = new HObject();
            HOperatorSet.GenEllipseContourXld(out myShapeXLD, row, col, phi, radius1, radius2, startPhi, endPhi, "positive", 1.5);
            HOperatorSet.GenRectangle2ContourXld(out ctrXLD, rowCtr, colCtr, rectPhi, rectL01, rectL02);
            HOperatorSet.SetColor(windowHandle, color);
            HOperatorSet.DispObj(myShapeXLD, windowHandle);
            HOperatorSet.DispObj(ctrXLD, windowHandle);
            HOperatorSet.DispArrow(windowHandle, controlRow1, controlCol1, controlRow5, controlCol5, 2);
        }
        public void DrawRectangle(HTuple windowHandle, Shape myShape, string color)
        {
            HTuple row1 = myShape.KeyPoints[0][0];
            HTuple col1 = myShape.KeyPoints[0][1];
            HTuple row2 = myShape.KeyPoints[1][0];
            HTuple col2 = myShape.KeyPoints[1][1];

            HTuple controlRow0 = (row1 + row2) * 0.5;
            HTuple controlCol0 = (col1 + col2) * 0.5;

            HTuple controlRow1 = (row1 + row2) * 0.5;
            HTuple controlCol1 = col2;

            HTuple controlRow2 = row1;
            HTuple controlCol2 = (col1 + col2) * 0.5;

            HTuple controlRow3 = (row1 + row2) * 0.5;
            HTuple controlCol3 = col1;

            HTuple controlRow4 = row2;
            HTuple controlCol4 = (col1 + col2) * 0.5;

            HObject myShapeXLD;
            HOperatorSet.GenRectangle2ContourXld(out myShapeXLD, controlRow0, controlCol0, 0, (col2 - col1) * 0.5, (row2 - row1) * 0.5);

            myShape.CtrPoints.Clear();
            myShape.CtrPoints.Add(new HTuple[] { controlRow0, controlCol0 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow1, controlCol1 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow2, controlCol2 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow3, controlCol3 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow4, controlCol4 });

            HTuple rowCtr = new HTuple();
            rowCtr[0] = controlRow0;
            rowCtr[1] = controlRow1;
            rowCtr[2] = controlRow2;
            rowCtr[3] = controlRow3;
            rowCtr[4] = controlRow4;

            HTuple colCtr = new HTuple();
            colCtr[0] = controlCol0;
            colCtr[1] = controlCol1;
            colCtr[2] = controlCol2;
            colCtr[3] = controlCol3;
            colCtr[4] = controlCol4;

            HTuple rectPhi = new HTuple();
            rectPhi[0] = 0;
            rectPhi[1] = 0;
            rectPhi[2] = 0;
            rectPhi[3] = 0;
            rectPhi[4] = 0;

            HTuple rectL01 = new HTuple();
            rectL01[0] = 20 * zoomWndFactor;
            rectL01[1] = 20 * zoomWndFactor;
            rectL01[2] = 20 * zoomWndFactor;
            rectL01[3] = 20 * zoomWndFactor;
            rectL01[4] = 20 * zoomWndFactor;

            HTuple rectL02 = new HTuple();
            rectL02[0] = 20 * zoomWndFactor;
            rectL02[1] = 20 * zoomWndFactor;
            rectL02[2] = 20 * zoomWndFactor;
            rectL02[3] = 20 * zoomWndFactor;
            rectL02[4] = 20 * zoomWndFactor;

            HObject ctrXLD = new HObject();
            HOperatorSet.GenRectangle2ContourXld(out ctrXLD, rowCtr, colCtr, rectPhi, rectL01, rectL02);
            HOperatorSet.SetColor(windowHandle, color);
            HOperatorSet.DispObj(myShapeXLD, windowHandle);
            HOperatorSet.DispObj(ctrXLD, windowHandle);
        }
        public void DrawRectangle2(HTuple windowHandle, Shape myShape, string color)
        {
            HTuple row = myShape.KeyPoints[0][0];
            HTuple col = myShape.KeyPoints[0][1];
            HTuple phi = myShape.KeyPoints[1][0].TupleRad();
            HTuple length1 = myShape.KeyPoints[2][0];
            HTuple length2 = myShape.KeyPoints[2][1];

            HTuple controlRow1 = row + (((new HTuple((new HTuple(0 + 90)).TupleRad()) + phi).TupleCos()) * length1);
            HTuple controlCol1 = col + (((new HTuple((new HTuple(0 + 90)).TupleRad()) + phi).TupleSin()) * length1);

            HTuple controlRow2 = row + (((new HTuple((new HTuple(0 + 180)).TupleRad()) + phi).TupleCos()) * length2);
            HTuple controlCol2 = col + (((new HTuple((new HTuple(0 + 180)).TupleRad()) + phi).TupleSin()) * length2);

            HTuple controlRow3 = row + (((new HTuple((new HTuple(0 + 270)).TupleRad()) + phi).TupleCos()) * length1);
            HTuple controlCol3 = col + (((new HTuple((new HTuple(0 + 270)).TupleRad()) + phi).TupleSin()) * length1);

            HTuple controlRow4 = row + (((new HTuple((new HTuple(0 + 000)).TupleRad()) + phi).TupleCos()) * length2);
            HTuple controlCol4 = col + (((new HTuple((new HTuple(0 + 000)).TupleRad()) + phi).TupleSin()) * length2);

            HTuple controlRow5 = row + (((new HTuple((new HTuple(0 + 90)).TupleRad()) + phi).TupleCos()) * (length1 + 36));
            HTuple controlCol5 = col + (((new HTuple((new HTuple(0 + 90)).TupleRad()) + phi).TupleSin()) * (length1 + 36));

            myShape.CtrPoints.Clear();
            myShape.CtrPoints.Add(new HTuple[] { row, col });
            myShape.CtrPoints.Add(new HTuple[] { controlRow1, controlCol1 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow2, controlCol2 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow3, controlCol3 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow4, controlCol4 });
            myShape.CtrPoints.Add(new HTuple[] { controlRow5, controlCol5 });

            HTuple rowCtr = new HTuple();
            rowCtr[0] = row;
            rowCtr[1] = controlRow1;
            rowCtr[2] = controlRow2;
            rowCtr[3] = controlRow3;
            rowCtr[4] = controlRow4;
            rowCtr[5] = controlRow5;

            HTuple colCtr = new HTuple();
            colCtr[0] = col;
            colCtr[1] = controlCol1;
            colCtr[2] = controlCol2;
            colCtr[3] = controlCol3;
            colCtr[4] = controlCol4;
            colCtr[5] = controlCol5;

            HTuple rectPhi = new HTuple();
            rectPhi[0] = phi;
            rectPhi[1] = phi;
            rectPhi[2] = phi;
            rectPhi[3] = phi;
            rectPhi[4] = phi;
            rectPhi[5] = phi;

            HTuple rectL01 = new HTuple();
            rectL01[0] = 20 * zoomWndFactor;
            rectL01[1] = 20 * zoomWndFactor;
            rectL01[2] = 20 * zoomWndFactor;
            rectL01[3] = 20 * zoomWndFactor;
            rectL01[4] = 20 * zoomWndFactor;
            rectL01[5] = 20 * zoomWndFactor;

            HTuple rectL02 = new HTuple();
            rectL02[0] = 20 * zoomWndFactor;
            rectL02[1] = 20 * zoomWndFactor;
            rectL02[2] = 20 * zoomWndFactor;
            rectL02[3] = 20 * zoomWndFactor;
            rectL02[4] = 20 * zoomWndFactor;
            rectL02[5] = 20 * zoomWndFactor;
            HObject myShapeXLD = new HObject(), ctrXLD = new HObject();
            HOperatorSet.GenRectangle2ContourXld(out myShapeXLD, row, col, phi, length1, length2);
            HOperatorSet.GenRectangle2ContourXld(out ctrXLD, rowCtr, colCtr, rectPhi, rectL01, rectL02);
            HOperatorSet.SetColor(windowHandle, color);
            HOperatorSet.DispObj(myShapeXLD, windowHandle);
            HOperatorSet.DispObj(ctrXLD, windowHandle);
            HOperatorSet.DispArrow(windowHandle, controlRow1, controlCol1, controlRow5, controlCol5, 2);
        }
        public void DrawPolygon(HTuple windowHandle, Shape myShape, string color)
        {
            // 设置图形线宽
            HOperatorSet.SetLineWidth(windowHandle, 1);
            HOperatorSet.SetColor(windowHandle, color);
            int index = myShape.KeyPoints.Count;
            //string pointN = "";
            for (int i = 0; i < index; i++)
            {
                //pointN = pointN + i.ToString() + "." + "[" + ((int)myShape.KeyPoints[i][0].D).ToString() + "," + ((int)myShape.KeyPoints[i][1].D).ToString() + "]\r\n";

                if (i < index - 1)
                {
                    HTuple x1 = myShape.KeyPoints[i][0];
                    HTuple y1 = myShape.KeyPoints[i][1];

                    HTuple x2 = myShape.KeyPoints[i + 1][0];
                    HTuple y2 = myShape.KeyPoints[i + 1][1];

                    HOperatorSet.DispLine(windowHandle, x1, y1, x2, y2);
                    HObject rect = new HObject();
                    HOperatorSet.GenRectangle2ContourXld(out rect, x1, y1, 0, 18 * zoomWndFactor, 18 * zoomWndFactor);
                    HOperatorSet.DispObj(rect, windowHandle);
                    HOperatorSet.SetTposition(windowHandle, x1, y1);
                    HOperatorSet.WriteString(windowHandle, "#" + i.ToString());
                }
                else if (i == index - 1)
                {
                    HTuple x1 = myShape.KeyPoints[i][0];
                    HTuple y1 = myShape.KeyPoints[i][1];

                    HTuple x2 = myShape.KeyPoints[0][0];
                    HTuple y2 = myShape.KeyPoints[0][1];

                    HOperatorSet.DispLine(windowHandle, x1, y1, x2, y2);
                    HObject rect = new HObject();
                    HOperatorSet.GenRectangle2ContourXld(out rect, x1, y1, 0, 18 * zoomWndFactor, 20 * zoomWndFactor);
                    HOperatorSet.DispObj(rect, windowHandle);
                    HOperatorSet.SetTposition(windowHandle, x1, y1);
                    HOperatorSet.WriteString(windowHandle, "#" + i.ToString());
                }
            }
            if (iChange >= myShape.KeyPoints.Count())
            {
                iChange = myShape.KeyPoints.Count() - 1;
            }
            HOperatorSet.DispRectangle2(windowHandle, myShape.KeyPoints[iChange][0], myShape.KeyPoints[iChange][1], 0, 18 * zoomWndFactor, 18 * zoomWndFactor);
            //HOperatorSet.DispCircle(windowHandle, myShape.KeyPoints[iChange][0], myShape.KeyPoints[iChange][1], 3);
        }
        public List<HTuple[]> DrawShapes(HTuple windowHandle, List<ShapeRegion> shapesList, int selectIndex)
        {
            List<HTuple[]> shapeKey = new List<HTuple[]> { };
            int count = shapesList.Count();
            if (count > 0)
            {
                for (int shapeIndex = 0; shapeIndex < count; shapeIndex++)
                {
                    string shapeColor = "green";
                    if (shapeIndex == selectIndex)
                    {
                        shapeColor = "blue";
                    }
                    else
                    {
                        shapeColor = "green";
                    }

                    if (shapesList[shapeIndex].MyShape.ShapeType == "Circle")
                    {
                        DrawCircle(windowHandle, shapesList[shapeIndex].MyShape, shapeColor);
                        if (shapeIndex == selectIndex)
                        {
                            shapeKey.Clear();
                            HTuple row = shapesList[shapeIndex].MyShape.KeyPoints[0][0];
                            HTuple col = shapesList[shapeIndex].MyShape.KeyPoints[0][1];
                            HTuple rad = shapesList[shapeIndex].MyShape.KeyPoints[2][0];
                            HTuple phB = shapesList[shapeIndex].MyShape.KeyPoints[3][0];
                            HTuple phE = shapesList[shapeIndex].MyShape.KeyPoints[3][1];

                            shapeKey.Add(new HTuple[] { "中心点_X:", col, "0", "1" });
                            shapeKey.Add(new HTuple[] { "中心点_Y:", row, "0", "0" });
                            shapeKey.Add(new HTuple[] { "半径长_R:", rad, "2", "0" });
                            shapeKey.Add(new HTuple[] { "起始角度:", phB, "3", "0" });
                            shapeKey.Add(new HTuple[] { "终止角度:", phE, "3", "1" });
                        }
                    }
                    else if (shapesList[shapeIndex].MyShape.ShapeType == "Ellipse")
                    {
                        DrawEllipse(windowHandle, shapesList[shapeIndex].MyShape, shapeColor);
                        if (shapeIndex == selectIndex)
                        {
                            shapeKey.Clear();
                            HTuple row = shapesList[shapeIndex].MyShape.KeyPoints[0][0];
                            HTuple col = shapesList[shapeIndex].MyShape.KeyPoints[0][1];
                            HTuple phi = shapesList[shapeIndex].MyShape.KeyPoints[1][0];
                            HTuple l01 = shapesList[shapeIndex].MyShape.KeyPoints[2][0];
                            HTuple l02 = shapesList[shapeIndex].MyShape.KeyPoints[2][1];
                            HTuple phB = shapesList[shapeIndex].MyShape.KeyPoints[3][0];
                            HTuple phE = shapesList[shapeIndex].MyShape.KeyPoints[3][1];

                            shapeKey.Add(new HTuple[] { "中心点_X:", col, "0", "1" });
                            shapeKey.Add(new HTuple[] { "中心点_Y:", row, "0", "0" });
                            shapeKey.Add(new HTuple[] { "主轴角度:", phi, "1", "0" });
                            shapeKey.Add(new HTuple[] { "主轴半长:", l01, "2", "0" });
                            shapeKey.Add(new HTuple[] { "次轴半长:", l02, "2", "1" });
                            shapeKey.Add(new HTuple[] { "起始角度:", phB, "3", "0" });
                            shapeKey.Add(new HTuple[] { "终止角度:", phE, "3", "1" });
                        }
                    }
                    else if (shapesList[shapeIndex].MyShape.ShapeType == "Rectangle")
                    {
                        DrawRectangle(windowHandle, shapesList[shapeIndex].MyShape, shapeColor);
                        if (shapeIndex == selectIndex)
                        {
                            shapeKey.Clear();
                            HTuple row1 = shapesList[shapeIndex].MyShape.KeyPoints[0][0];
                            HTuple col1 = shapesList[shapeIndex].MyShape.KeyPoints[0][1];
                            HTuple row2 = shapesList[shapeIndex].MyShape.KeyPoints[1][0];
                            HTuple col2 = shapesList[shapeIndex].MyShape.KeyPoints[1][1];

                            shapeKey.Add(new HTuple[] { "起始点_X:", col1, "0", "1" });
                            shapeKey.Add(new HTuple[] { "起始点_Y:", row1, "0", "0" });
                            shapeKey.Add(new HTuple[] { "终止点_X:", col2, "1", "1" });
                            shapeKey.Add(new HTuple[] { "终止点_Y:", row2, "1", "0" });
                        }
                    }
                    else if (shapesList[shapeIndex].MyShape.ShapeType == "Rectangle2")
                    {
                        DrawRectangle2(windowHandle, shapesList[shapeIndex].MyShape, shapeColor);
                        if (shapeIndex == selectIndex)
                        {
                            shapeKey.Clear();
                            HTuple row = shapesList[shapeIndex].MyShape.KeyPoints[0][0];
                            HTuple col = shapesList[shapeIndex].MyShape.KeyPoints[0][1];
                            HTuple phi = shapesList[shapeIndex].MyShape.KeyPoints[1][0];
                            HTuple l01 = shapesList[shapeIndex].MyShape.KeyPoints[2][0];
                            HTuple l02 = shapesList[shapeIndex].MyShape.KeyPoints[2][1];

                            shapeKey.Add(new HTuple[] { "中心点_X:", col, "0", "1" });
                            shapeKey.Add(new HTuple[] { "中心点_Y:", row, "0", "0" });
                            shapeKey.Add(new HTuple[] { "主轴角度:", phi, "1", "0" });
                            shapeKey.Add(new HTuple[] { "主轴半长:", l01, "2", "0" });
                            shapeKey.Add(new HTuple[] { "次轴半长:", l02, "2", "1" });
                        }
                    }
                    else if (shapesList[shapeIndex].MyShape.ShapeType == "Polygon")
                    {
                        DrawPolygon(windowHandle, shapesList[shapeIndex].MyShape, shapeColor);

                        if (shapeIndex == selectIndex)
                        {
                            int num = shapesList[shapeIndex].MyShape.KeyPoints.Count();
                            shapeKey.Clear();
                            for (int i = 0; i < num; i++)
                            {
                                HTuple row = shapesList[shapeIndex].MyShape.KeyPoints[i][0];
                                HTuple col = shapesList[shapeIndex].MyShape.KeyPoints[i][1];
                                shapeKey.Add(new HTuple[] { "顶点" + i.ToString() + "_X:", col, i.ToString(), "1" });
                                shapeKey.Add(new HTuple[] { "顶点" + i.ToString() + "_Y:", row, i.ToString(), "0" });
                            }
                        }
                    }
                }
            }
            return (shapeKey);
        }
        //区域设置操作方法
        public void AddShape(List<ShapeRegion> shapesRegion, string opnMode, string shapeType, int insertAt)
        {
            ShapeRegion shapeRegion = new ShapeRegion();

            shapeRegion.OpnMode = opnMode;
            shapeRegion.MyShape.ShapeType = shapeType;

            shapeRegion.MyShape.KeyPoints = new List<HTuple[]>();
            shapeRegion.MyShape.CtrPoints = new List<HTuple[]>();

            if (shapeType == "Circle")
            {
                CreateCircle(shapeRegion.MyShape);
            }
            else if (shapeType == "Ellipse")
            {
                CreateEllipse(shapeRegion.MyShape);
            }
            else if (shapeType == "Rectangle")
            {
                CreateRectangle(shapeRegion.MyShape);
            }
            else if (shapeType == "Rectangle2")
            {
                CreateRectangle2(shapeRegion.MyShape);
            }
            else if (shapeType == "Polygon")
            {
                CreatePolygon(shapeRegion.MyShape);
            }

            // 确定添加的位置
            if (shapesRegion.Count < 0)
            {
                shapesRegion.Add(shapeRegion);
            }
            else
            {
                shapesRegion.Insert(insertAt, shapeRegion);
            }
        }
        public void RemoveShape(List<ShapeRegion> shapesRegion, int insertAt)
        {
            shapesRegion.RemoveAt(insertAt);
            DrawShapes(hWindow.HalconWindow, shapesRegion, insertAt);
        }
        // Polygon 中插入顶点
        public void InsertPoint(List<ShapeRegion> shapesRegion, int shapeIndex)
        {
            HTuple[] pointN = new HTuple[2];

            if (shapesRegion[shapeIndex].MyShape.ShapeType == "Polygon")
            {
                int Num = shapesRegion[shapeIndex].MyShape.KeyPoints.Count();
                if (iChange == Num - 1)
                {
                    HTuple xm = (shapesRegion[shapeIndex].MyShape.KeyPoints[0][0] + shapesRegion[shapeIndex].MyShape.KeyPoints[Num - 1][0]) / 2;
                    HTuple ym = (shapesRegion[shapeIndex].MyShape.KeyPoints[0][1] + shapesRegion[shapeIndex].MyShape.KeyPoints[Num - 1][1]) / 2;

                    pointN = new HTuple[] { xm, ym };
                    // Points.Add(pointN);
                    shapesRegion[shapeIndex].MyShape.KeyPoints.Insert(iChange + 1, pointN);

                    // DrawPolygon();
                }
                else if (iChange < Num - 1)
                {
                    HTuple xm = (shapesRegion[shapeIndex].MyShape.KeyPoints[iChange][0] + shapesRegion[shapeIndex].MyShape.KeyPoints[iChange + 1][0]) / 2;
                    HTuple ym = (shapesRegion[shapeIndex].MyShape.KeyPoints[iChange][1] + shapesRegion[shapeIndex].MyShape.KeyPoints[iChange + 1][1]) / 2;

                    pointN = new HTuple[] { xm, ym };
                    //points.Add(pointN);
                    shapesRegion[shapeIndex].MyShape.KeyPoints.Insert(iChange + 1, pointN);
                    //DrawPolygon();
                }
            }
            // 重绘图形
            //DrawShapes(shapeSelected);
        }

        // Polygon 中移除顶点
        public void RemovePoint(List<ShapeRegion> shapesRegion, int shapeIndex)
        {
            if (shapesRegion[shapeIndex].MyShape.ShapeType == "Polygon")
            {
                int Num = shapesRegion[shapeIndex].MyShape.KeyPoints.Count();
                if (Num > 2 && iChange < Num)
                {
                    shapesRegion[shapeIndex].MyShape.KeyPoints.RemoveAt(iChange);
                    if (iChange > 0)
                    {
                        iChange = iChange - 1;
                    }
                }
            }
            // 重绘图形
            //ShowImage(ho_Image);
            //DrawShapes(shapeSelected);
        }

        //设置区域操作所需变量
        private List<ShapeRegion> myShapesRegion = new List<ShapeRegion>();
        private HObject[] regions = new HObject[2];//01为ModelRegion，02为SearchRegion
        //private HTuple regionIndex;
        private int shapeSelected = 0;
        #endregion

        //定义图像窗口相关变量
        private HWindowControl hWindow;
        private HObject wImage;
        private HTuple imgW, imgH;
        private int iChange = 0;
        private HTuple X_B4Move = 0, Y_B4Move = 0, X_AftMove = 0, Y_AftMove = 0;
        private HTuple current_beginRow, current_beginCol, current_endRow, current_endCol;
        private HTuple zoomWndFactor = 1;
        //定义匹配模型变量
        HTuple shapeModelID;
        ModelSearch_Param shapeSearchParam;
        HObject shapeSearchRegion;
        //测量卡尺所需变量
        MetroModelShape metroModelShape;
        HTuple metroObjSelect = 0;
        //卡尺保存路径
        string metroSavePath = "";
        string paramSide = "";
        //控件初始化
        public SetMetro()
        {
            InitializeComponent();
        }
        public SetMetro(string sideStr, string metroSavePath)
        {
            InitializeComponent();
            this.paramSide = sideStr;
            this.metroSavePath = metroSavePath;
        }
        public SetMetro(string sideStr, HObject img, HTuple shapeModelId, HObject roi, ModelSearch_Param searchParam, string metroSaveFileName)
        {
            InitializeComponent();
            this.paramSide = sideStr;
            this.wImage = img;
            this.shapeModelID = shapeModelId;
            this.shapeSearchRegion = roi;
            this.shapeSearchParam = searchParam;
            this.metroSavePath = metroSaveFileName;
        }
        //加载窗体
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                HOperatorSet.SetWindowAttr("background_color", "black"); //先设置一下background_color属性，再OpenWindow一下
                HoWindow.HalconWindow.OpenWindow(0, 0, HoWindow.Width, HoWindow.Height, HoWindow.Handle, "visible", "");

                hWindow = HoWindow as HWindowControl;
                this.HoWindow.HMouseDown += HoWindow_HMouseDown;
                this.HoWindow.HMouseMove += HoWindow_HMouseMove;

                this.HoWindow.HMouseWheel += HoWindow_HMouseWheel;

                this.BtnLoadLocalImg.Click += BtnLoadLocalImg_Click;


                //卡尺模型相关
                this.BtnReadLocalMetro.Click += BtnReadLocalMetro_Click;
                this.BtnAddMetroObj.Click += BtnAddMetroObj_Click;
                this.LbxMetroObj.SelectedIndexChanged += LbxMetroObj_SelectedIndexChanged;

                this.HoWindow.HMouseUp += HoWindowDrawMetro_HMouseUp;
                this.BtnTestMetro.Click += BtnTestMetro_Click;
                this.BtnRemoveMetroObj.Click += BtnRmvMetroObj_Click;
                this.LbxMetroObjParam.MouseDoubleClick += LbxMetroObjParam_MouseDoubleClick;


                if (!IsObjectEmpty(wImage) && shapeModelID != null && metroSavePath != null)
                {
                    LoadImageAsAspectRatio(HoWindow, wImage);
                    BtnReadLocalMetro_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        #region 图像缩放以及图形修改
        //鼠标按下(单击---， 滚轮双击---图像宽高比例显示)
        private void HoWindow_HMouseDown(object sender, HMouseEventArgs e)
        {
            try
            {
                HWindowControl showWindow = sender as HWindowControl;
                HTuple DownRow = new HTuple(), DownCol = new HTuple(), Button = new HTuple();
                HOperatorSet.GetMposition(showWindow.HalconWindow, out DownRow, out DownCol, out Button);
                if (e.Clicks == 2 && Button == 2)//滚轮双击按图像宽高比显示
                {
                    LoadImageAsAspectRatio(sender, wImage);
                    if (myShapesRegion != null && myShapesRegion.Count > 0)
                    {
                        DrawShapes(hWindow.HalconWindow, myShapesRegion, shapeSelected);
                    }
                    if (metroModelShape.shapeMetros != null && metroModelShape.shapeMetros.Count > 0)
                    {
                        HTuple metroModelHandle = new HTuple();
                        ShapeToMetro(metroModelShape.shapeMetros[metroObjSelect], out metroModelHandle);
                        DrawMetroObj(hWindow.HalconWindow, metroModelHandle);
                    }
                }
                else if (e.Clicks == 1 && Button == 1)//鼠标左键单击
                {
                    X_B4Move = e.Y;
                    Y_B4Move = e.X;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("鼠标按下出现错误：\r\n\r\n" + ex.ToString());
                return;
            }
        }

        //鼠标移动
        private void HoWindow_HMouseMove(object sender, HMouseEventArgs e)
        {
            try
            {
                HWindowControl showWindow = sender as HWindowControl;
                HTuple PointGray;
                HTuple Row = new HTuple(), Col = new HTuple(), Button = new HTuple();
                HOperatorSet.GetMposition(showWindow.HalconWindow, out Row, out Col, out Button);
                if (wImage != null && (Row > 0 && Row < imgH) && (Col > 0 && Col < imgW))//设置3个条件项，防止程序崩溃。
                {
                    HOperatorSet.GetGrayval(wImage, Row, Col, out PointGray);                 //获取当前点的灰度值
                }
                else
                {
                    PointGray = "_";
                }

                textBox1.Text = String.Format("X_{0}, Y_{1}, Gray_{2}", Col, Row, PointGray); //格式化字符串
            }
            catch (Exception ex)
            {
                //MessageBox.Show("鼠标移动出现错误：\r\n\r\n" + ex.ToString());
                return;
            }
        }

        //鼠标抬起
        private void HoWindowDrawMetro_HMouseUp(object sender, HMouseEventArgs e)
        {
            try
            {
                HWindowControl showWindow = sender as HWindowControl;
                if (e.Clicks == 1 && e.Button.ToString() == "Left")//鼠标左键单击
                {
                    try
                    {
                        X_AftMove = e.Y;
                        Y_AftMove = e.X;
                        if (metroModelShape.shapeMetros != null)
                        {
                            if (metroModelShape.shapeMetros.Count > metroObjSelect && metroModelShape.shapeMetros[metroObjSelect].MyShape.KeyPoints.Count > 0)
                            {
                                MoveControlPoint(metroModelShape.shapeMetros[metroObjSelect].MyShape);
                                HTuple mm = new HTuple();
                                ShapeToMetro(metroModelShape.shapeMetros[metroObjSelect], out mm);
                                DrawMetroObj(hWindow.HalconWindow, mm);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show("鼠标抬起出现错误：\r\n\r\n" + ex.ToString());
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());
                return;
            }
        }

        //滚动鼠标滚轮进行窗口图片放缩显示
        private void HoWindow_HMouseWheel(object sender, HMouseEventArgs e)
        {
            try
            {
                int Button;
                double HRow = 0, HCol = 0, mode = 1;

                HoWindow.HalconWindow.GetMpositionSubPix(out HRow, out HCol, out Button);
                if (e.Delta > 0)
                {
                    mode = 1;
                }
                else
                {
                    mode = -1;
                }

                HOperatorSet.ClearWindow(hWindow.HalconWindow);
                DispImageZoom(sender, wImage, null, mode, HRow, HCol);

                if (tabOpnArea.SelectedTab == tabSetMetro)
                {
                    if (myShapesRegion != null && myShapesRegion.Count > 0)
                    {
                        DrawShapes(hWindow.HalconWindow, myShapesRegion, shapeSelected);
                    }
                    if (metroModelShape.shapeMetros != null && metroModelShape.shapeMetros.Count > 0)
                    {
                        HTuple metroModelHandle = new HTuple();
                        ShapeToMetro(metroModelShape.shapeMetros[metroObjSelect], out metroModelHandle);
                        DrawMetroObj(hWindow.HalconWindow, metroModelHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("鼠标滚动出现错误：\r\n\r\n" + ex.ToString());
                return;
            }
        }
        #endregion

        //加载窗口图像
        private void BtnLoadLocalImg_Click(object sender, EventArgs e)
        {
            try
            {
                LoadLocalImg();
            }
            catch (Exception ex)
            { MessageBox.Show("图像加载过程出现错误，请核验！\r\n\r\n" + ex.Message.ToString()); }
        }

        private void BtnLoadCamImg_Click(object sender, EventArgs e)
        {
            try
            {
                //获取图像
                HOperatorSet.GenEmptyObj(out wImage);
                try
                {
                    switch (paramSide)
                    {
                        case "A":
                            if (Global.myParams.sideParamA.camParam.isImgSourceLocal)
                            {
                                HOperatorSet.ReadImage(out wImage, Global.myParams.sideParamA.camParam.addressImg);
                            }
                            else
                            {
                                if (CameraManage.ConnectCameras(paramSide))//)CameraManage.ConnectCameras(paramSide))
                                {
                                    //wImage = CameraManage.GrabImage(paramSide);
                                    //CameraManage.DisConnectCameras(paramSide);
                                    wImage = CameraManage.GrabImage(paramSide);
                                    wImage = wImage.Clone();
                                    CameraManage.DisConnectCameras(paramSide);
                                }
                                else
                                {
                                    MessageBox.Show(paramSide + "_相机连接失败！");
                                }
                            }
                            break;
                        case "B":
                            if (Global.myParams.sideParamB.camParam.isImgSourceLocal)
                            {
                                HOperatorSet.ReadImage(out wImage, Global.myParams.sideParamB.camParam.addressImg);
                            }
                            else
                            {
                                if (CameraManage.ConnectCameras(paramSide))//)CameraManage.ConnectCameras(paramSide))
                                {
                                    //wImage = CameraManage.GrabImage(paramSide);
                                    //CameraManage.DisConnectCameras(paramSide);
                                    wImage = CameraManage.GrabImage(paramSide);
                                    wImage = wImage.Clone();
                                    CameraManage.DisConnectCameras(paramSide);
                                }
                                else
                                {
                                    MessageBox.Show(paramSide + "_相机连接失败！");
                                }
                            }
                            break;
                        case "C":
                            if (Global.myParams.sideParamC.camParam.isImgSourceLocal)
                            {
                                HOperatorSet.ReadImage(out wImage, Global.myParams.sideParamC.camParam.addressImg);
                            }
                            else
                            {
                                if (CameraManage.ConnectCameras(paramSide))//)CameraManage.ConnectCameras(paramSide))
                                {
                                    //wImage = CameraManage.GrabImage(paramSide);
                                    //CameraManage.DisConnectCameras(paramSide);
                                    wImage = CameraManage.GrabImage(paramSide);
                                    wImage = wImage.Clone();
                                    CameraManage.DisConnectCameras(paramSide);
                                }
                                else
                                {
                                    MessageBox.Show(paramSide + "_相机连接失败！");
                                }
                            }
                            break;
                        case "D":
                            if (Global.myParams.sideParamD.camParam.isImgSourceLocal)
                            {
                                HOperatorSet.ReadImage(out wImage, Global.myParams.sideParamD.camParam.addressImg);
                            }
                            else
                            {
                                if (CameraManage.ConnectCameras(paramSide))//)CameraManage.ConnectCameras(paramSide))
                                {
                                    //wImage = CameraManage.GrabImage(paramSide);
                                    //CameraManage.DisConnectCameras(paramSide);
                                    wImage = CameraManage.GrabImage(paramSide);
                                    wImage = wImage.Clone();
                                    CameraManage.DisConnectCameras(paramSide);
                                }
                                else
                                {
                                    MessageBox.Show(paramSide + "_相机连接失败！");
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    if (!IsObjectEmpty(wImage))
                    {
                        LoadImageAsAspectRatio(HoWindow, wImage);
                    }
                }
                catch (Exception err)
                {
                    Global.AddLog(paramSide + "_图像加载出错:\r\n" + err.Message.ToString());
                }
            }
            catch (Exception ex)
            { MessageBox.Show("窗口图像加载出错：\r\n" + ex.Message.ToString()); }
        }
        //添加卡尺对象
        private void BtnAddMetroObj_Click(object sender, EventArgs e)
        {
            try
            {
                if (wImage == null || IsObjectEmpty(wImage))
                {
                    MessageBox.Show("请先加载窗口图像！");
                    return;
                }
                if (metroModelShape.shapeMetros == null)
                {
                    MessageBox.Show("请先创建测量模型！");
                    return;
                }
                //**************************************************************
                HTuple metroModel = new HTuple();
                HOperatorSet.CreateMetrologyModel(out metroModel);
                HOperatorSet.SetMetrologyModelImageSize(metroModel, imgW, imgH);
                if (RbnObjCircle.Checked == true)
                {
                    HTuple circleParam = new HTuple();
                    circleParam[0] = Convert.ToInt32(imgH.D * 0.5);//row
                    circleParam[1] = Convert.ToInt32(imgW.D * 0.5);//col
                    HTuple imgMin = new HTuple();
                    HOperatorSet.TupleMin2(imgH, imgW, out imgMin);//
                    circleParam[2] = Convert.ToInt32(imgMin.D * 0.25);//rad

                    HTuple metroType = "circle";
                    HTuple lenth01 = 30;
                    HTuple lenth02 = 5;
                    HTuple sigma = 1.5;
                    HTuple threshold = 30;

                    HTuple paramName = new HTuple();
                    HTuple paramValue = new HTuple();
                    HTuple metroObjIndex = new HTuple();
                    HOperatorSet.AddMetrologyObjectGeneric(metroModel, metroType, circleParam, lenth01, lenth02, sigma, threshold, paramName, paramValue, out metroObjIndex);
                }
                else if (RbnObjEllipse.Checked == true)
                {
                    HTuple circleParam = new HTuple();
                    circleParam[0] = Convert.ToInt32(imgH.D * 0.5);//row
                    circleParam[1] = Convert.ToInt32(imgW.D * 0.5);//col
                    circleParam[2] = 0;
                    HTuple imgMin = new HTuple();
                    HOperatorSet.TupleMin2(imgH, imgW, out imgMin);
                    circleParam[3] = Convert.ToInt32(imgMin.D * 0.50);//r01
                    circleParam[4] = Convert.ToInt32(imgMin.D * 0.25);//r02

                    HTuple metroType = "ellipse";
                    HTuple lenth01 = 30;
                    HTuple lenth02 = 5;
                    HTuple sigma = 1.5;
                    HTuple threshold = 30;
                    HTuple paramName = new HTuple();
                    HTuple paramValue = new HTuple();
                    HTuple metroObjIndex = new HTuple();
                    HOperatorSet.AddMetrologyObjectGeneric(metroModel, metroType, circleParam, lenth01, lenth02, sigma, threshold, paramName, paramValue, out metroObjIndex);
                }
                else if (RbnObjRect.Checked == true)
                {
                    HTuple circleParam = new HTuple();
                    circleParam[0] = Convert.ToInt32(imgH.D * 0.5);//row
                    circleParam[1] = Convert.ToInt32(imgW.D * 0.5);//col
                    circleParam[2] = 000;//phi
                    HTuple imgMin = new HTuple();
                    HOperatorSet.TupleMin2(imgH, imgW, out imgMin);
                    circleParam[3] = Convert.ToInt32(imgMin.D * 0.50);//L01
                    circleParam[4] = Convert.ToInt32(imgMin.D * 0.25);//L02

                    HTuple metroType = "rectangle2";
                    HTuple lenth01 = 30;
                    HTuple lenth02 = 5;
                    HTuple sigma = 1.5;
                    HTuple threshold = 30;
                    HTuple paramName = new HTuple();
                    HTuple paramValue = new HTuple();
                    HTuple metroObjIndex = new HTuple();
                    HOperatorSet.AddMetrologyObjectGeneric(metroModel, metroType, circleParam, lenth01, lenth02, sigma, threshold, paramName, paramValue, out metroObjIndex);
                }
                else if (RbnObjLine.Checked)
                {
                    HTuple circleParam = new HTuple();
                    circleParam[0] = Convert.ToInt32(imgH.D * 0.5);//row
                    circleParam[1] = Convert.ToInt32(imgW.D * 0.25);//col
                    circleParam[2] = Convert.ToInt32(imgH.D * 0.5);//row
                    circleParam[3] = Convert.ToInt32(imgW.D * 0.75);//col

                    HTuple metroType = "line";
                    HTuple lenth01 = 30;
                    HTuple lenth02 = 5;
                    HTuple sigma = 1.5;
                    HTuple threshold = 30;
                    HTuple paramName = new HTuple();
                    HTuple paramValue = new HTuple();
                    HTuple metroObjIndex = new HTuple();
                    HOperatorSet.AddMetrologyObjectGeneric(metroModel, metroType, circleParam, lenth01, lenth02, sigma, threshold, paramName, paramValue, out metroObjIndex);
                }
                else
                {
                    return;
                }

                metroObjSelect = metroModelShape.shapeMetros.Count();
                ShapeMetro sm = new ShapeMetro();
                MetroToShape(metroModel, out sm);
                metroModelShape.shapeMetros.Add(sm);
                HTuple objType = new HTuple();
                HOperatorSet.GetMetrologyObjectParam(metroModel, 0, "object_type", out objType);
                LbxMetroObj.Items.Add(objType.S);
                LbxMetroObj.SelectedIndex = metroObjSelect;

                HOperatorSet.ClearMetrologyModel(metroModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加卡尺出现错误：\r\n\r\n" + ex.ToString());
            }
        }

        //移除卡尺对象
        private void BtnRmvMetroObj_Click(object sender, EventArgs e)
        {
            try
            {
                if (metroModelShape.shapeMetros.Count() > 0 && LbxMetroObj.Items.Count > 0)
                {
                    LbxMetroObj.Items.RemoveAt(metroObjSelect);
                    metroModelShape.shapeMetros.RemoveAt(metroObjSelect);
                    if (LbxMetroObj.Items.Count > 0)
                    {
                        if (metroModelShape.shapeMetros.Count() == metroObjSelect && LbxMetroObj.Items.Count == metroObjSelect)
                        {
                            metroObjSelect--;
                            LbxMetroObj.SelectedIndex = metroObjSelect;
                        }
                        else
                        {
                            LbxMetroObj.SelectedIndex = metroObjSelect;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("移除卡尺出现错误：\r\n\r\n" + ex.ToString());
            }
        }

        private void LbxMetroObj_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (LbxMetroObj.SelectedIndex > -1)
                {
                    metroObjSelect = LbxMetroObj.SelectedIndex;
                    HTuple metroHandle = new HTuple();
                    ShapeToMetro(metroModelShape.shapeMetros[metroObjSelect], out metroHandle);
                    DrawMetroObj(hWindow.HalconWindow, metroHandle);
                }
                else
                {
                    HOperatorSet.ClearWindow(hWindow.HalconWindow);
                    HOperatorSet.DispObj(wImage, hWindow.HalconWindow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("选择卡尺出现错误：\r\n\r\n" + ex.ToString());
            }
        }

        private void LbxMetroObjParam_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                int index = this.LbxMetroObjParam.IndexFromPoint(e.Location);
                if (index != System.Windows.Forms.ListBox.NoMatches)
                {
                    HOperatorSet.ClearWindow(hWindow.HalconWindow);
                    HOperatorSet.DispObj(wImage, hWindow.HalconWindow);

                    int p = 0, m = 0, n = 0;
                    string itemName = LbxMetroObjParam.SelectedItem.ToString().Substring(0, 5);
                    if (itemName.Contains(":"))
                    {
                        itemName = itemName.Substring(0, 4);
                    }

                    var num = Enum.Parse(typeof(EnumPsn), itemName);
                    int psn = (int)num;

                    if (psn > 9)
                    {
                        p = 1;
                        m = psn - 10;
                    }
                    else
                    {
                        p = 0;
                        m = (int)(psn * 0.5);
                        n = (int)((psn * 0.5 - m) * 2);
                    }

                    HTuple itemValue = null;
                    if (p == 0)
                    {
                        itemValue = metroModelShape.shapeMetros[metroObjSelect].MyShape.KeyPoints[m][n];
                    }
                    else if (p == 1)
                    {
                        itemValue = metroModelShape.shapeMetros[metroObjSelect].metroParams[m];
                    }
                    InputBoxMetro inbox = new InputBoxMetro(itemName, itemValue);

                    DialogResult dr = inbox.ShowDialog();
                    if (dr == DialogResult.OK && inbox.valueForRenew.Length > 0)
                    {
                        if (p == 0)
                        {
                            metroModelShape.shapeMetros[metroObjSelect].MyShape.KeyPoints[m][n] = inbox.valueForRenew;
                        }
                        else if (p == 1)
                        {
                            metroModelShape.shapeMetros[metroObjSelect].metroParams[m] = inbox.valueForRenew;
                        }

                        LbxMetroObj_SelectedIndexChanged(null, null);
                    }
                    inbox.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("双击参数出现错误：\r\n\r\n" + ex.ToString());
            }
        }

        private void BtnTestMetro_Click(object sender, EventArgs e)
        {
            try
            {
                int num = metroModelShape.shapeMetros.Count();
                if (num < 1)
                {
                    MessageBox.Show("测量模型为空，请先创建测量模型并设置！");
                    return;
                }
                HTuple metroHandle = new HTuple();
                ShapeToSingleMetro(metroModelShape.shapeMetros, out metroHandle);
                //HOperatorSet.AlignMetrologyModel(metroHandle, metroModelShape.refRow, metroModelShape.refCol, metroModelShape.refAngle);
                HOperatorSet.ApplyMetrologyModel(wImage, metroHandle);
                HObject mContour = new HObject();
                HTuple rows = new HTuple(), cols = new HTuple();
                HOperatorSet.GetMetrologyObjectMeasures(out mContour, metroHandle, "all", "all", out rows, out cols);
                HOperatorSet.SetColor(hWindow.HalconWindow, "white");
                HOperatorSet.DispObj(mContour, hWindow.HalconWindow);
                mContour.Dispose();
                HObject contour = new HObject();
                HOperatorSet.GetMetrologyObjectResultContour(out contour, metroHandle, "all", "all", 1.5);
                HOperatorSet.SetColor(hWindow.HalconWindow, "green");
                HOperatorSet.DispObj(contour, hWindow.HalconWindow);
                contour.Dispose();
                HOperatorSet.ClearMetrologyModel(metroHandle);
            }
            catch (Exception ex)
            {
                MessageBox.Show("卡尺测试出现错误：\r\n\r\n" + ex.ToString());
            }
        }

        private void BtnSaveOneMetro_Click(object sender, EventArgs e)
        {
            try
            {
                HTuple metroHandle = new HTuple();
                HTuple param = new HTuple(), refParam = new HTuple();
                if (CbxManualSetMetroRef.Checked == true)
                {
                    metroModelShape.refCol = (HTuple)NbxRefX.Value;
                    metroModelShape.refRow = (HTuple)NbxRefY.Value;
                    HOperatorSet.TupleRad((HTuple)NbxRefR.Value, out metroModelShape.refAngle);
                    CbxManualSetMetroRef.Checked = false;
                }

                refParam = ((metroModelShape.refRow.TupleConcat(metroModelShape.refCol))).TupleConcat(metroModelShape.refAngle);
                ShapeToSingleMetro(metroModelShape.shapeMetros, out metroHandle);
                HOperatorSet.SetMetrologyModelParam(metroHandle, "reference_system", refParam);
                HOperatorSet.AlignMetrologyModel(metroHandle, metroModelShape.refRow, metroModelShape.refCol, metroModelShape.refAngle);
                HOperatorSet.ApplyMetrologyModel(wImage, metroHandle);
                HOperatorSet.GetMetrologyObjectResult(metroHandle, "all", "all", "result_type", "all_param", out param);
                if (param.Length > 0)
                {
                    HOperatorSet.WriteMetrologyModel(metroHandle, metroSavePath);
                    MessageBox.Show("卡尺模型保存成功");
                }
                else
                {
                    MessageBox.Show("卡尺测量失败，模型未保存。");
                }
                HOperatorSet.ClearMetrologyModel(metroHandle);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存卡尺出现错误：\r\n\r\n" + ex.ToString());
            }
        }

        private void BtnSearchMatchModel_Click(object sender, EventArgs e)
        {
            try
            {
                if (wImage == null)
                {
                    MessageBox.Show("窗口图像为空，请先加载窗口图像！");
                    return;
                }
                if (shapeModelID == null)
                {
                    MessageBox.Show("形状匹配模型为空，请先创建匹配模型！");
                    return;
                }
                Type type = typeof(ModelSearch_Param);
                PropertyInfo[] fields = type.GetProperties();
                foreach (PropertyInfo inf in fields)
                {

                }

                HTuple angleStart = (HTuple)shapeSearchParam.angleStart;
                HTuple angleExtent = (HTuple)shapeSearchParam.angleExtent;
                HTuple scaleMin = (HTuple)shapeSearchParam.minScale;
                HTuple scaleMax = (HTuple)shapeSearchParam.maxScale;
                HTuple minScore = (HTuple)shapeSearchParam.minScore;
                HTuple numMatch = (HTuple)(1);//shapeSearchParam.maxMatchNum;
                HTuple maxOverlap = (HTuple)shapeSearchParam.maxOverlap;
                HTuple subPixel = (HTuple)shapeSearchParam.subPixel;
                HTuple numLevel = (HTuple)shapeSearchParam.numLevel;
                HTuple greediness = (HTuple)shapeSearchParam.greediness;
                // Local iconic variables 
                HObject ho_ImageReduced;
                // Local control variables             
                HTuple hv_HomMat2D = null, hv_Length = null;
                HTuple fRow = null, fCol = null, fAngle = null, fScale = null, fScore = null;
                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_ImageReduced);

                if (!IsObjectEmpty(shapeSearchRegion))
                {
                    HOperatorSet.ReduceDomain(wImage, shapeSearchRegion, out ho_ImageReduced);
                    HOperatorSet.FindScaledShapeModel(ho_ImageReduced, shapeModelID, angleStart, angleExtent, scaleMin, scaleMax,
                              minScore, numMatch, maxOverlap, subPixel, numLevel, greediness,
                              out fRow, out fCol, out fAngle, out fScale, out fScore);
                }
                else
                {
                    HOperatorSet.FindScaledShapeModel(wImage, shapeModelID, angleStart, angleExtent, scaleMin, scaleMax,
                              minScore, numMatch, maxOverlap, subPixel, numLevel, greediness,
                              out fRow, out fCol, out fAngle, out fScale, out fScore);
                }
                HOperatorSet.TupleLength(fScore, out hv_Length);
                if ((int)(new HTuple(hv_Length.TupleEqual(1))) != 0)
                {
                    HObject modelContour, findContour;
                    HOperatorSet.GenEmptyObj(out modelContour);
                    HOperatorSet.GenEmptyObj(out findContour);
                    HOperatorSet.GetShapeModelContours(out modelContour, shapeModelID, 1);
                    HOperatorSet.HomMat2dIdentity(out hv_HomMat2D);
                    HOperatorSet.VectorAngleToRigid(0, 0, 0, fRow, fCol, fAngle, out hv_HomMat2D);
                    HOperatorSet.AffineTransContourXld(modelContour, out findContour, hv_HomMat2D);
                    HOperatorSet.SetColor(hWindow.HalconWindow, "green");
                    HOperatorSet.DispObj(findContour, hWindow.HalconWindow);
                    HOperatorSet.DispCross(hWindow.HalconWindow, fRow, fCol, 66, fAngle);
                    modelContour.Dispose();
                    findContour.Dispose();

                    NbxRefX.Value = (Decimal)(fCol.D);
                    NbxRefY.Value = (Decimal)(fRow.D);
                    HOperatorSet.TupleDeg(fAngle, out fAngle);
                    NbxRefR.Value = (Decimal)(fAngle.D);
                }
                ho_ImageReduced.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("搜索模型匹配时出错:\r\n" + ex.Message.ToString());
            }
        }

        //读取本地卡尺模型
        private void BtnReadLocalMetro_Click(object sender, EventArgs e)
        {
            try
            {
                ReadMetroModel(metroSavePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建卡尺模型出现错误：\r\n\r\n" + ex.ToString());
            }
        }
        private void BtnReadOtherMetro_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;
                ofd.Filter = "(*.mtr)|*.mtr|All files(*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ReadMetroModel(ofd.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex.Message.ToString());
            }
        }
        private void BtnAddTestImg_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;
                ofd.Filter = @"All Image Files|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.png;*.tif;*.tiff|
                               Windows Bitmap(*.bmp)|*.bmp|
                               Windows Icon(*.ico)|*.ico|
                               Graphics Interchange Format (*.gif)|(*.gif)|
                               JPEG File Interchange Format (*.jpg)|*.jpg;*.jpeg|
                               Portable Network Graphics (*.png)|*.png|
                               Tag Image File Format (*.tif)|*.tif;*.tiff";
                if (DialogResult.OK == ofd.ShowDialog(this))
                {
                    HOperatorSet.ReadImage(out wImage, ofd.FileName);
                    LoadImageAsAspectRatio(HoWindow, wImage);
                    LbxTestImgList.Items.Add(ofd.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载图像出粗：\r\n" + ex.Message.ToString());
            }
        }
        private void BtnRemoveTestImg_Click(object sender, EventArgs e)
        {
            try
            {
                if (LbxTestImgList.SelectedIndex < 0)
                {
                    MessageBox.Show("未选中图像，请先选中图像后进行删除！");
                    return;
                }
                LbxTestImgList.Items.RemoveAt(LbxTestImgList.SelectedIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除图像出错：\r\n" + ex.Message.ToString());
            }
        }
        private void BtnTestAlignMeasure_Click(object sender, EventArgs e)
        {
            try
            {
                if (wImage == null)
                {
                    MessageBox.Show("窗口图像为空，请先加载窗口图像！");
                    return;
                }
                if (shapeModelID == null)
                {
                    MessageBox.Show("形状匹配模型为空，请先创建匹配模型！");
                    return;
                }
                Type type = typeof(ModelSearch_Param);
                PropertyInfo[] fields = type.GetProperties();
                foreach (PropertyInfo inf in fields)
                {

                }

                HTuple angleStart = (HTuple)shapeSearchParam.angleStart;
                HTuple angleExtent = (HTuple)shapeSearchParam.angleExtent;
                HTuple scaleMin = (HTuple)shapeSearchParam.minScale;
                HTuple scaleMax = (HTuple)shapeSearchParam.maxScale;
                HTuple minScore = (HTuple)shapeSearchParam.minScore;
                HTuple numMatch = (HTuple)(1);//shapeSearchParam.maxMatchNum;
                HTuple maxOverlap = (HTuple)shapeSearchParam.maxOverlap;
                HTuple subPixel = (HTuple)shapeSearchParam.subPixel;
                HTuple numLevel = (HTuple)shapeSearchParam.numLevel;
                HTuple greediness = (HTuple)shapeSearchParam.greediness;
                // Local iconic variables 
                HObject ho_ImageReduced;
                // Local control variables             
                HTuple hv_HomMat2D = null, hv_Length = null;
                HTuple fRow = null, fCol = null, fAngle = null, fScale = null, fScore = null;
                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out ho_ImageReduced);

                if (!IsObjectEmpty(shapeSearchRegion))
                {
                    HOperatorSet.ReduceDomain(wImage, shapeSearchRegion, out ho_ImageReduced);
                    HOperatorSet.FindScaledShapeModel(ho_ImageReduced, shapeModelID, angleStart, angleExtent, scaleMin, scaleMax,
                              minScore, numMatch, maxOverlap, subPixel, numLevel, greediness,
                              out fRow, out fCol, out fAngle, out fScale, out fScore);
                    ho_ImageReduced.Dispose();
                }
                else
                {
                    HOperatorSet.FindScaledShapeModel(wImage, shapeModelID, angleStart, angleExtent, scaleMin, scaleMax,
                              minScore, numMatch, maxOverlap, subPixel, numLevel, greediness,
                              out fRow, out fCol, out fAngle, out fScale, out fScore);
                }
                HOperatorSet.TupleLength(fScore, out hv_Length);
                if ((int)(new HTuple(hv_Length.TupleEqual(1))) != 0)
                {
                    HObject modelContour, findContour;
                    HOperatorSet.GenEmptyObj(out modelContour);
                    HOperatorSet.GenEmptyObj(out findContour);
                    HOperatorSet.GetShapeModelContours(out modelContour, shapeModelID, 1);
                    HOperatorSet.HomMat2dIdentity(out hv_HomMat2D);
                    HOperatorSet.VectorAngleToRigid(0, 0, 0, fRow, fCol, fAngle, out hv_HomMat2D);
                    HOperatorSet.AffineTransContourXld(modelContour, out findContour, hv_HomMat2D);
                    HOperatorSet.SetColor(hWindow.HalconWindow, "green");
                    HOperatorSet.DispObj(findContour, hWindow.HalconWindow);
                    HOperatorSet.DispCross(hWindow.HalconWindow, fRow, fCol, 66, fAngle);
                    modelContour.Dispose();
                    findContour.Dispose();
                    //
                    HObject measureCtr, measureResultCtr;
                    HOperatorSet.GenEmptyObj(out measureCtr);
                    HOperatorSet.GenEmptyObj(out measureResultCtr);
                    HTuple metroHandle = new HTuple();
                    HTuple param = new HTuple(), refParam = new HTuple();
                    refParam = ((metroModelShape.refRow.TupleConcat(metroModelShape.refCol))).TupleConcat(metroModelShape.refAngle);
                    ShapeToSingleMetro(metroModelShape.shapeMetros, out metroHandle);
                    HOperatorSet.SetMetrologyModelParam(metroHandle, "reference_system", refParam);
                    HOperatorSet.AlignMetrologyModel(metroHandle, fRow, fCol, fAngle);
                    HOperatorSet.ApplyMetrologyModel(wImage, metroHandle);
                    HOperatorSet.GetMetrologyObjectResult(metroHandle, "all", "all", "result_type", "all_param", out param);
                    if (param.Length > 0)
                    {
                        HTuple rowM = null, colM = null;
                        HOperatorSet.GetMetrologyObjectMeasures(out measureCtr, metroHandle, "all", "all", out rowM, out colM);
                        HOperatorSet.GetMetrologyObjectResultContour(out measureResultCtr, metroHandle, "all", "all", 1.5);
                        HOperatorSet.SetColor(hWindow.HalconWindow, "blue");
                        HOperatorSet.DispObj(measureCtr, hWindow.HalconWindow);
                        HOperatorSet.SetColor(hWindow.HalconWindow, "green");
                        HOperatorSet.DispObj(measureResultCtr, hWindow.HalconWindow);
                        measureCtr.Dispose();
                        measureResultCtr.Dispose();
                    }
                    HOperatorSet.ClearMetrologyModel(metroHandle);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void LbxTestImgList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (LbxTestImgList.SelectedIndex < 0)
                {
                    MessageBox.Show("未选中图像！");
                    return;
                }
                HOperatorSet.ReadImage(out wImage, LbxTestImgList.SelectedItem.ToString());
                LoadImageAsAspectRatio(HoWindow, wImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("改选图像出错：\r\n" + ex.Message.ToString());
            }
        }
        private void DrawMetroShape(HTuple window, Shape shape, HTuple color)
        {
            if (shape.ShapeType == "Circle")
            {
                DrawCircle(window, shape, color);
            }
            else if (shape.ShapeType == "Ellipse")
            {
                DrawEllipse(window, shape, color);
            }
            else if (shape.ShapeType == "Rectangle2")
            {
                DrawRectangle2(window, shape, color);
            }
            else if (shape.ShapeType == "Polygon")
            {
                DrawPolygon(window, shape, color);
            }
            else
            { }
        }

        private void ShapeToMetro(ShapeMetro shapeMetro, out HTuple metroHandle)
        {
            HOperatorSet.CreateMetrologyModel(out metroHandle);
            HTuple metroType = new HTuple();
            HTuple param = new HTuple();

            if (shapeMetro.MyShape.ShapeType == "Circle")
            {
                metroType = "circle";
                param[0] = shapeMetro.MyShape.KeyPoints[0][0];//row
                param[1] = shapeMetro.MyShape.KeyPoints[0][1];//col
                param[2] = shapeMetro.MyShape.KeyPoints[2][0];//radius

            }
            else if (shapeMetro.MyShape.ShapeType == "Ellipse")
            {
                metroType = "ellipse";
                param[0] = shapeMetro.MyShape.KeyPoints[0][0];//row
                param[1] = shapeMetro.MyShape.KeyPoints[0][1];//col
                param[2] = shapeMetro.MyShape.KeyPoints[1][0].TupleRad();//phi
                param[3] = shapeMetro.MyShape.KeyPoints[2][0];//radius1
                param[4] = shapeMetro.MyShape.KeyPoints[2][1];//radius2

            }
            else if (shapeMetro.MyShape.ShapeType == "Rectangle2")
            {
                metroType = "rectangle2";
                param[0] = shapeMetro.MyShape.KeyPoints[0][0];//row
                param[1] = shapeMetro.MyShape.KeyPoints[0][1];//col
                param[2] = shapeMetro.MyShape.KeyPoints[1][0].TupleRad();//phi
                param[3] = shapeMetro.MyShape.KeyPoints[2][0];//length1
                param[4] = shapeMetro.MyShape.KeyPoints[2][1];//length2
            }
            else if (shapeMetro.MyShape.ShapeType == "Polygon")
            {
                metroType = "line";
                param[0] = shapeMetro.MyShape.KeyPoints[0][0];//rowBegin
                param[1] = shapeMetro.MyShape.KeyPoints[0][1];//colBegin
                param[2] = shapeMetro.MyShape.KeyPoints[1][0];//rowEnd
                param[3] = shapeMetro.MyShape.KeyPoints[1][1];//colEnd
            }

            HTuple measureLenth01 = shapeMetro.metroParams[0];
            HTuple measureLenth02 = shapeMetro.metroParams[1];
            HTuple sigma = shapeMetro.metroParams[3];
            HTuple threshold = shapeMetro.metroParams[4];

            HTuple paramName = null;
            paramName = new HTuple();
            paramName[0] = "min_score";
            paramName[1] = "num_instances";
            paramName[2] = "measure_distance";
            paramName[3] = "measure_transition";
            paramName[4] = "measure_select";
            paramName[5] = "measure_interpolation";

            HTuple paramValue = new HTuple();
            paramValue[0] = shapeMetro.metroParams[2];//min_score
            paramValue[1] = Convert.ToInt32(shapeMetro.metroParams[5].D);//num_instance
            paramValue[2] = shapeMetro.metroParams[6];//measure_distance
            paramValue[3] = shapeMetro.metroParams[7];//measure_transition
            paramValue[4] = shapeMetro.metroParams[8];//measure_select
            paramValue[5] = shapeMetro.metroParams[9];//measure_interpolation
            //Convert.ToInt32(shapeMetro.metroParams[6].D);
            if (metroType == "circle" || metroType == "ellipse")
            {
                //参数名称
                paramName[6] = "start_phi";
                paramName[7] = "end_phi";
                //参数赋值
                paramValue[6] = shapeMetro.MyShape.KeyPoints[3][0].TupleRad();
                paramValue[7] = shapeMetro.MyShape.KeyPoints[3][1].TupleRad();
            }
            HTuple metroObjIndex = new HTuple();
            HOperatorSet.AddMetrologyObjectGeneric(metroHandle, metroType, param, measureLenth01, measureLenth02, sigma, threshold, paramName, paramValue, out metroObjIndex);
        }

        private void ShapeToSingleMetro(List<ShapeMetro> metroShapes, out HTuple metroHandle)
        {
            try
            {
                int num = metroShapes.Count();
                if (num < 1)
                {
                    MessageBox.Show("测量模型为空，请先创建测量模型并设置！");
                    metroHandle = null;
                    return;
                }
                metroHandle = null;
                HOperatorSet.CreateMetrologyModel(out metroHandle);

                for (int i = 0; i < num; i++)
                {
                    HTuple metroType = new HTuple();
                    HTuple param = new HTuple();
                    if (metroShapes[i].MyShape.ShapeType == "Circle")
                    {
                        metroType = "circle";
                        param[0] = metroShapes[i].MyShape.KeyPoints[0][0];//row
                        param[1] = metroShapes[i].MyShape.KeyPoints[0][1];//col
                        param[2] = metroShapes[i].MyShape.KeyPoints[2][0];//radius

                    }
                    else if (metroShapes[0].MyShape.ShapeType == "Ellipse")
                    {
                        metroType = "ellipse";
                        param[0] = metroShapes[i].MyShape.KeyPoints[0][0];//row
                        param[1] = metroShapes[i].MyShape.KeyPoints[0][1];//col
                        param[2] = metroShapes[i].MyShape.KeyPoints[1][0].TupleRad();//phi
                        param[3] = metroShapes[i].MyShape.KeyPoints[2][0];//radius1
                        param[4] = metroShapes[i].MyShape.KeyPoints[2][1];//radius2

                    }
                    else if (metroShapes[i].MyShape.ShapeType == "Rectangle2")
                    {
                        metroType = "rectangle2";
                        param[0] = metroShapes[i].MyShape.KeyPoints[0][0];//row
                        param[1] = metroShapes[i].MyShape.KeyPoints[0][1];//col
                        param[2] = metroShapes[i].MyShape.KeyPoints[1][0].TupleRad();//phi
                        param[3] = metroShapes[i].MyShape.KeyPoints[2][0];//length1
                        param[4] = metroShapes[i].MyShape.KeyPoints[2][1];//length2
                    }
                    else if (metroShapes[i].MyShape.ShapeType == "Polygon")
                    {
                        metroType = "line";
                        param[0] = metroShapes[i].MyShape.KeyPoints[0][0];//rowBegin
                        param[1] = metroShapes[i].MyShape.KeyPoints[0][1];//colBegin
                        param[2] = metroShapes[i].MyShape.KeyPoints[1][0];//rowEnd
                        param[3] = metroShapes[i].MyShape.KeyPoints[1][1];//colEnd
                    }
                    HTuple measureLenth01 = metroShapes[i].metroParams[0];
                    HTuple measureLenth02 = metroShapes[i].metroParams[1];
                    HTuple sigma = metroShapes[i].metroParams[3];
                    HTuple threshold = metroShapes[i].metroParams[4];

                    HTuple paramName = null;
                    paramName = new HTuple();
                    paramName[0] = "min_score";
                    paramName[1] = "num_instances";
                    paramName[2] = "measure_distance";
                    paramName[3] = "measure_transition";
                    paramName[4] = "measure_select";
                    paramName[5] = "measure_interpolation";

                    HTuple paramValue = new HTuple();
                    paramValue[0] = metroShapes[i].metroParams[2];//min_score
                    paramValue[1] = Convert.ToInt32(metroShapes[i].metroParams[5].D);//num_instance
                    paramValue[2] = metroShapes[i].metroParams[6];//measure_distance
                    paramValue[3] = metroShapes[i].metroParams[7];//measure_transition
                    paramValue[4] = metroShapes[i].metroParams[8];//measure_select
                    paramValue[5] = metroShapes[i].metroParams[9];//measure_interpolation
                                                                  //Convert.ToInt32(shapeMetro.metroParams[6].D);
                    if (metroType == "circle" || metroType == "ellipse")
                    {
                        //参数名称
                        paramName[6] = "start_phi";
                        paramName[7] = "end_phi";
                        //参数赋值
                        paramValue[6] = metroShapes[i].MyShape.KeyPoints[3][0].TupleRad();
                        paramValue[7] = metroShapes[i].MyShape.KeyPoints[3][1].TupleRad();
                    }
                    HTuple metroObjIndex = new HTuple();
                    HOperatorSet.AddMetrologyObjectGeneric(metroHandle, metroType, param, measureLenth01, measureLenth02, sigma, threshold, paramName, paramValue, out metroObjIndex);
                }
            }
            catch (Exception ex)
            {
                metroHandle = null;
                MessageBox.Show("测量模型数据转换为测量模型时出错：\r\n" + ex.Message.ToString());
            }
        }

        private void MetroToShape(HTuple metroHandle, out ShapeMetro shapeMetro)
        {
            try
            {
                shapeMetro = new ShapeMetro();
                shapeMetro.metroParams = new HTuple[20];
                shapeMetro.MyShape.CtrPoints = new List<HTuple[]>();
                shapeMetro.MyShape.KeyPoints = new List<HTuple[]>();
                HTuple objType = null;
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "object_type", out objType);

                if (objType.S == "circle")
                {
                    HTuple objRow = new HTuple(), objCol = new HTuple();
                    HTuple objRad = new HTuple();
                    HTuple objPhiS = new HTuple(), objPhiE = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row", out objRow);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column", out objCol);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "radius", out objRad);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "start_phi", out objPhiS);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "end_phi", out objPhiE);

                    shapeMetro.MyShape.ShapeType = "Circle";
                    shapeMetro.MyShape.KeyPoints.Clear();
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRow, objCol });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { 0, 0 });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRad, objRad });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objPhiS.TupleDeg(), objPhiE.TupleDeg() });
                }
                else if (objType.S == "ellipse")
                {
                    HTuple objRow = new HTuple(), objCol = new HTuple();
                    HTuple objPhi = new HTuple();
                    HTuple objRad1 = new HTuple(), objRad2 = new HTuple();
                    HTuple objPhiS = new HTuple(), objPhiE = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row", out objRow);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column", out objCol);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "phi", out objPhi);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "radius1", out objRad1);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "radius2", out objRad2);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "start_phi", out objPhiS);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "end_phi", out objPhiE);

                    shapeMetro.MyShape.ShapeType = "Ellipse";
                    shapeMetro.MyShape.KeyPoints.Clear();
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRow, objCol });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objPhi.TupleDeg(), objPhi.TupleDeg() });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRad1, objRad2 });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objPhiS.TupleDeg(), objPhiE.TupleDeg() });
                }
                else if (objType.S == "rectangle")
                {
                    HTuple objRow = new HTuple(), objCol = new HTuple();
                    HTuple objPhi = new HTuple();
                    HTuple objLth1 = new HTuple(), objLth2 = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row", out objRow);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column", out objCol);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "phi", out objPhi);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "length1", out objLth1);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "length2", out objLth2);

                    shapeMetro.MyShape.ShapeType = "Rectangle2";
                    shapeMetro.MyShape.KeyPoints.Clear();
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRow, objCol });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objPhi.TupleDeg(), objPhi.TupleDeg() });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objLth1, objLth2 });
                }
                else if (objType.S == "line")
                {
                    HTuple objRowB = new HTuple(), objColB = new HTuple();
                    HTuple objRowE = new HTuple(), objColE = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row_begin", out objRowB);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column_begin", out objColB);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row_end", out objRowE);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column_end", out objColE);

                    shapeMetro.MyShape.ShapeType = "Polygon";

                    shapeMetro.MyShape.KeyPoints.Clear();
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRowB, objColB });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRowE, objColE });
                }
                HTuple objMeasureLength01 = new HTuple(), objMeasureLength02 = new HTuple();
                HTuple objMinScore = new HTuple(), objSigma = new HTuple(), objThreshold = new HTuple();
                HTuple objNumInstance = new HTuple(), objMeasureDistance = new HTuple();
                HTuple objTransition = new HTuple(), objSelect = new HTuple(), objInterpolation = new HTuple();

                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_length1", out objMeasureLength01);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_length2", out objMeasureLength02);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "min_score", out objMinScore);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_sigma", out objSigma);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_threshold", out objThreshold);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "num_instances", out objNumInstance);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_distance_min", out objMeasureDistance);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_transition", out objTransition);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_select", out objSelect);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_interpolation", out objInterpolation);

                shapeMetro.metroParams[0] = objMeasureLength01;
                shapeMetro.metroParams[1] = objMeasureLength02;
                shapeMetro.metroParams[2] = objMinScore;
                shapeMetro.metroParams[3] = objSigma;
                shapeMetro.metroParams[4] = objThreshold;
                shapeMetro.metroParams[5] = objNumInstance;
                shapeMetro.metroParams[6] = objMeasureDistance;
                shapeMetro.metroParams[7] = objTransition;
                shapeMetro.metroParams[8] = objSelect;
                shapeMetro.metroParams[9] = objInterpolation;
            }
            catch (Exception ex)
            {
                shapeMetro = new ShapeMetro();
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void MetroToShapes(HTuple metroHandle, out List<ShapeMetro> shapeMetroes)
        {
            try
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            shapeMetroes = new List<ShapeMetro>();

            HTuple objNum = 0;
            HTuple objList = null;
            HOperatorSet.GetMetrologyObjectIndices(metroHandle, out objList);
            HOperatorSet.TupleLength(objList, out objNum);
            int metroObjNum = Convert.ToInt32(objNum.D);

            if (metroObjNum < 1)
            {
                shapeMetroes.Clear();
                return;
            }

            for (int i = 0; i < metroObjNum; i++)
            {
                ShapeMetro shapeMetro = new ShapeMetro();
                shapeMetro.metroParams = new HTuple[20];
                shapeMetro.MyShape.CtrPoints = new List<HTuple[]>();
                shapeMetro.MyShape.KeyPoints = new List<HTuple[]>();
                HTuple objType = null;
                HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "object_type", out objType);

                if (objType.S == "circle")
                {
                    HTuple objRow = new HTuple(), objCol = new HTuple();
                    HTuple objRad = new HTuple();
                    HTuple objPhiS = new HTuple(), objPhiE = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "row", out objRow);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "column", out objCol);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "radius", out objRad);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "start_phi", out objPhiS);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "end_phi", out objPhiE);

                    shapeMetro.MyShape.ShapeType = "Circle";
                    shapeMetro.MyShape.KeyPoints.Clear();
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRow, objCol });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { 0, 0 });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRad, objRad });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objPhiS.TupleDeg(), objPhiE.TupleDeg() });
                }
                else if (objType.S == "ellipse")
                {
                    HTuple objRow = new HTuple(), objCol = new HTuple();
                    HTuple objPhi = new HTuple();
                    HTuple objRad1 = new HTuple(), objRad2 = new HTuple();
                    HTuple objPhiS = new HTuple(), objPhiE = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "row", out objRow);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "column", out objCol);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "phi", out objPhi);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "radius1", out objRad1);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "radius2", out objRad2);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "start_phi", out objPhiS);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "end_phi", out objPhiE);

                    shapeMetro.MyShape.ShapeType = "Ellipse";
                    shapeMetro.MyShape.KeyPoints.Clear();
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRow, objCol });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objPhi.TupleDeg(), objPhi.TupleDeg() });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRad1, objRad2 });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objPhiS.TupleDeg(), objPhiE.TupleDeg() });
                }
                else if (objType.S == "rectangle")
                {
                    HTuple objRow = new HTuple(), objCol = new HTuple();
                    HTuple objPhi = new HTuple();
                    HTuple objLth1 = new HTuple(), objLth2 = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "row", out objRow);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "column", out objCol);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "phi", out objPhi);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "length1", out objLth1);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "length2", out objLth2);

                    shapeMetro.MyShape.ShapeType = "Rectangle2";
                    shapeMetro.MyShape.KeyPoints.Clear();
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRow, objCol });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objPhi.TupleDeg(), objPhi.TupleDeg() });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objLth1, objLth2 });
                }
                else if (objType.S == "line")
                {
                    HTuple objRowB = new HTuple(), objColB = new HTuple();
                    HTuple objRowE = new HTuple(), objColE = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "row_begin", out objRowB);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "column_begin", out objColB);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "row_end", out objRowE);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, objList[i], "column_end", out objColE);

                    shapeMetro.MyShape.ShapeType = "Polygon";

                    shapeMetro.MyShape.KeyPoints.Clear();
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRowB, objColB });
                    shapeMetro.MyShape.KeyPoints.Add(new HTuple[] { objRowE, objColE });
                }
                HTuple objMeasureLength01 = new HTuple(), objMeasureLength02 = new HTuple();
                HTuple objMinScore = new HTuple(), objSigma = new HTuple(), objThreshold = new HTuple();
                HTuple objNumInstance = new HTuple(), objMeasureDistance = new HTuple();
                HTuple objTransition = new HTuple(), objSelect = new HTuple(), objInterpolation = new HTuple();

                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "measure_length1", out objMeasureLength01);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "measure_length2", out objMeasureLength02);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "min_score", out objMinScore);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "measure_sigma", out objSigma);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "measure_threshold", out objThreshold);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "num_instances", out objNumInstance);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "measure_distance_min", out objMeasureDistance);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "measure_transition", out objTransition);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "measure_select", out objSelect);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, i, "measure_interpolation", out objInterpolation);

                shapeMetro.metroParams[0] = objMeasureLength01;
                shapeMetro.metroParams[1] = objMeasureLength02;
                shapeMetro.metroParams[2] = objMinScore;
                shapeMetro.metroParams[3] = objSigma;
                shapeMetro.metroParams[4] = objThreshold;
                shapeMetro.metroParams[5] = objNumInstance;
                shapeMetro.metroParams[6] = objMeasureDistance;
                shapeMetro.metroParams[7] = objTransition;
                shapeMetro.metroParams[8] = objSelect;
                shapeMetro.metroParams[9] = objInterpolation;
                shapeMetroes.Add(shapeMetro);
            }
        }

        private void DrawMetroObj(HTuple window, HTuple metroHandle)
        {
            try
            {
                LbxMetroObjParam.Items.Clear();
                HTuple objType = new HTuple();
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "object_type", out objType);

                if (objType.S == "circle")
                {
                    HTuple objRow = new HTuple(), objCol = new HTuple();
                    HTuple objRad = new HTuple();
                    HTuple objPhiS = new HTuple(), objPhiE = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row", out objRow);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column", out objCol);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "radius", out objRad);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "start_phi", out objPhiS);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "end_phi", out objPhiE);

                    LbxMetroObjParam.Items.Add("横坐标_X: " + objCol.D.ToString());
                    LbxMetroObjParam.Items.Add("纵坐标_Y: " + objRow.D.ToString());
                    LbxMetroObjParam.Items.Add("半径长度: " + objRad.D.ToString());
                    LbxMetroObjParam.Items.Add("起始角度: " + (objPhiS.TupleDeg()).D.ToString());
                    LbxMetroObjParam.Items.Add("终止角度: " + (objPhiE.TupleDeg()).D.ToString());
                }
                else if (objType.S == "ellipse")
                {
                    HTuple objRow = new HTuple(), objCol = new HTuple();
                    HTuple objPhi = new HTuple();
                    HTuple objRad1 = new HTuple(), objRad2 = new HTuple();
                    HTuple objPhiS = new HTuple(), objPhiE = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row", out objRow);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column", out objCol);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "phi", out objPhi);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "radius1", out objRad1);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "radius2", out objRad2);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "start_phi", out objPhiS);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "end_phi", out objPhiE);

                    LbxMetroObjParam.Items.Add("横坐标_X: " + objCol.D.ToString());
                    LbxMetroObjParam.Items.Add("纵坐标_Y: " + objRow.D.ToString());
                    LbxMetroObjParam.Items.Add("主轴方向: " + (objPhi.TupleDeg()).D.ToString());
                    LbxMetroObjParam.Items.Add("主轴半长: " + objRad1.D.ToString());
                    LbxMetroObjParam.Items.Add("次轴半长: " + objRad2.D.ToString());
                    LbxMetroObjParam.Items.Add("起始角度: " + (objPhiS.TupleDeg()).D.ToString());
                    LbxMetroObjParam.Items.Add("终止角度: " + (objPhiE.TupleDeg()).D.ToString());
                }
                else if (objType.S == "rectangle")
                {
                    HTuple objRow = new HTuple(), objCol = new HTuple();
                    HTuple objPhi = new HTuple();
                    HTuple objLth1 = new HTuple(), objLth2 = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row", out objRow);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column", out objCol);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "phi", out objPhi);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "length1", out objLth1);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "length2", out objLth2);

                    LbxMetroObjParam.Items.Add("横坐标_X: " + objCol.D.ToString());
                    LbxMetroObjParam.Items.Add("纵坐标_Y: " + objRow.D.ToString());
                    LbxMetroObjParam.Items.Add("主轴方向: " + (objPhi.TupleDeg()).D.ToString());
                    LbxMetroObjParam.Items.Add("主轴半长: " + objLth1.D.ToString());
                    LbxMetroObjParam.Items.Add("次轴半长: " + objLth2.D.ToString());
                }
                else if (objType.S == "line")
                {
                    HTuple objRowB = new HTuple(), objColB = new HTuple();
                    HTuple objRowE = new HTuple(), objColE = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row_begin", out objRowB);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column_begin", out objColB);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "row_end", out objRowE);
                    HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "column_end", out objColE);

                    LbxMetroObjParam.Items.Add("起始点_X: " + objColB.D.ToString());
                    LbxMetroObjParam.Items.Add("起始点_Y: " + objRowB.D.ToString());
                    LbxMetroObjParam.Items.Add("终止点_X: " + objColE.D.ToString());
                    LbxMetroObjParam.Items.Add("终止点_Y: " + objRowE.D.ToString());
                }
                HTuple objMeasureLength01 = new HTuple(), objMeasureLength02 = new HTuple();
                HTuple objMinScore = new HTuple(), objSigma = new HTuple(), objThreshold = new HTuple();
                HTuple objInstanceNum = new HTuple(), objMeasureDistance = new HTuple();
                HTuple objTransition = new HTuple(), objSelect = new HTuple(), objInterpolation = new HTuple();
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_length1", out objMeasureLength01);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_length2", out objMeasureLength02);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "min_score", out objMinScore);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_sigma", out objSigma);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_threshold", out objThreshold);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "num_instances", out objInstanceNum);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_distance_min", out objMeasureDistance);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_transition", out objTransition);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_select", out objSelect);
                HOperatorSet.GetMetrologyObjectParam(metroHandle, 0, "measure_interpolation", out objInterpolation);
                //num_instances
                LbxMetroObjParam.Items.Add("测框长_1: " + objMeasureLength01.D.ToString());
                LbxMetroObjParam.Items.Add("测框长_2: " + objMeasureLength02.D.ToString());
                LbxMetroObjParam.Items.Add("最小分数: " + objMinScore.D.ToString());
                LbxMetroObjParam.Items.Add("平滑系数: " + objSigma.D.ToString());
                LbxMetroObjParam.Items.Add("幅度阈值: " + objThreshold.D.ToString());
                LbxMetroObjParam.Items.Add("实例个数: " + objInstanceNum.D.ToString());
                LbxMetroObjParam.Items.Add("测量间距: " + objMeasureDistance.D.ToString());
                LbxMetroObjParam.Items.Add("测量极性: " + objTransition.S.ToString());
                LbxMetroObjParam.Items.Add("测量选择: " + objSelect.S.ToString());
                LbxMetroObjParam.Items.Add("插值方法: " + objInterpolation.S.ToString());

                HObject measureRect = new HObject();
                HTuple row = new HTuple(), col = new HTuple();
                HOperatorSet.GetMetrologyObjectMeasures(out measureRect, metroHandle, 0, "all", out row, out col);
                HOperatorSet.ClearWindow(window);
                HOperatorSet.DispObj(wImage, window);
                HOperatorSet.SetColor(window, "green");
                HOperatorSet.DispObj(measureRect, window);
                DrawMetroShape(window, metroModelShape.shapeMetros[metroObjSelect].MyShape, "blue");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void CreateMetroModel()
        {
            try
            {
                if (wImage == null || IsObjectEmpty(wImage))
                {
                    MessageBox.Show("请先加载窗口图像！");
                    return;
                }

                metroModelShape.shapeMetros = new List<ShapeMetro>();
                metroModelShape.refRow = 0;
                metroModelShape.refCol = 0;
                metroModelShape.refAngle = 0;
                CbxManualSetMetroRef.Checked = false;
                NbxRefX.Value = 0;
                NbxRefY.Value = 0;
                NbxRefR.Value = 0;
                LbxMetroObj.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("创建卡尺模型出现错误：\r\n\r\n" + ex.ToString());
            }
        }

        private void ReadMetroModel(string filePathName)
        {
            try
            {
                CreateMetroModel();//
                HTuple mHandle = new HTuple(), refParam = new HTuple(), angleDeg = new HTuple();
                HOperatorSet.ReadMetrologyModel(filePathName, out mHandle);
                HOperatorSet.GetMetrologyModelParam(mHandle, "reference_system", out refParam);
                metroModelShape.refRow = refParam[0];
                metroModelShape.refCol = refParam[1];
                metroModelShape.refAngle = refParam[2];
                CbxManualSetMetroRef.Checked = false;
                NbxRefX.Value = (Decimal)(refParam[1].D);
                NbxRefY.Value = (Decimal)(refParam[0].D);
                HOperatorSet.TupleDeg(refParam[2], out angleDeg);
                NbxRefR.Value = (Decimal)(angleDeg.D);

                HTuple refAngle = (((HTuple)0).TupleConcat(0).TupleConcat(-metroModelShape.refAngle));
                HOperatorSet.SetMetrologyModelParam(mHandle, "reference_system", refAngle);

                HTuple refMove = (((-metroModelShape.refRow).TupleConcat(-metroModelShape.refCol))).TupleConcat(0);
                HOperatorSet.SetMetrologyModelParam(mHandle, "reference_system", refMove);

                HTuple refOrigin = (((HTuple)0).TupleConcat(0).TupleConcat(0));
                HOperatorSet.SetMetrologyModelParam(mHandle, "reference_system", refOrigin);

                metroModelShape.shapeMetros.Clear();
                MetroToShapes(mHandle, out metroModelShape.shapeMetros);
                HTuple objNum = 0;
                HTuple objList = null;
                HOperatorSet.GetMetrologyObjectIndices(mHandle, out objList);
                HOperatorSet.TupleLength(objList, out objNum);
                int metroObjNum = Convert.ToInt32(objNum.D);
                LbxMetroObj.Items.Clear();
                for (int i = 0; i < metroObjNum; i++)
                {
                    HTuple objType = new HTuple();
                    HOperatorSet.GetMetrologyObjectParam(mHandle, objList[i], "object_type", out objType);
                    LbxMetroObj.Items.Add(objType.S);
                }

                LbxMetroObj.SelectedIndex = metroObjNum - 1;
                HOperatorSet.ClearMetrologyModel(mHandle);

                DrawMetroShape(HoWindow.HalconWindow, metroModelShape.shapeMetros[metroObjNum - 1].MyShape, "green");
            }
            catch (Exception ex)
            {
                MessageBox.Show("测量模型读取失败！\r\n" + ex.Message.ToString());
            }
        }

        private void LoadLocalImg()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = @"All Image Files|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.png;*.tif;*.tiff|
                               Windows Bitmap(*.bmp)|*.bmp|
                               Windows Icon(*.ico)|*.ico|
                               Graphics Interchange Format (*.gif)|(*.gif)|
                               JPEG File Interchange Format (*.jpg)|*.jpg;*.jpeg|
                               Portable Network Graphics (*.png)|*.png|
                               Tag Image File Format (*.tif)|*.tif;*.tiff";
            if (DialogResult.OK == ofd.ShowDialog(this))
            {
                //HTuple row01, col01, row02, col02;
                HOperatorSet.ReadImage(out wImage, ofd.FileName);

                LoadImageAsAspectRatio(HoWindow, wImage);
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
                HOperatorSet.GetImageSize(wImage, out imgW, out imgH);

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
                HOperatorSet.DispObj(wImage, showWindow.HalconWindow);
            }
            catch
            { }
        }

        //按缩放比例显示图像
        private void DispImageZoom(object sender, HObject image, HObject obj, double mode, double Mouse_row, double Mouse_col)
        {
            HWindowControl showWindow = sender as HWindowControl;
            HTuple imgW, imgH, zoom_beginRow, zoom_beginCol, zoom_endRow, zoom_endCol;
            try
            {
                HOperatorSet.GetImageSize(image, out imgW, out imgH);
            }
            catch
            {
                return;
            }
            try
            {
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
            catch
            {
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
            catch    //ex.Message;
            {
                //DispImageFit();
            }
        }

        //窗口信息显示
        private void HDispMessage(HTuple windowHandle, string msg, int row, int col, string color, string font, string fontSize, bool bold)
        {
            HOperatorSet.SetColor(windowHandle, color);
            HTuple font2 = new HTuple();
            HOperatorSet.QueryFont(windowHandle, out font2);
            if (font2[0].S.Length > 10)
            {
                string text = bold ? "1" : "0";
                HOperatorSet.SetFont(windowHandle, "-" + font + fontSize + "-*-0-*-*-" + text + "-");
            }
            else
            {
                string str = bold ? "-Bold" : "";
                HOperatorSet.SetFont(windowHandle, font + str + fontSize);
            }

            HOperatorSet.SetTposition(windowHandle, row, col);
            HOperatorSet.WriteString(windowHandle, msg);
            HOperatorSet.SetColor(windowHandle, "green");
        }
        //通过移动控制点修改图形
        public void MoveControlPoint(Shape myShape)
        {
            // 查找鼠标位置是否有顶点，如果有顶点则给出顶点索引
            if (myShape.ShapeType == "Polygon")
            {
                iChange = myShape.KeyPoints.FindIndex((HTuple[] pf) => (Math.Abs(X_B4Move.D - pf[0].D) < 17 * zoomWndFactor && Math.Abs(Y_B4Move.D - pf[1].D) < 17 * zoomWndFactor));
            }
            else
            {
                iChange = myShape.CtrPoints.FindIndex((HTuple[] pf) => (Math.Abs(X_B4Move.D - pf[0].D) < 17 * zoomWndFactor && Math.Abs(Y_B4Move.D - pf[1].D) < 17 * zoomWndFactor));
                // 列表查找例子，Part part2 = parts.Find((Part p) => p.id == 222);
            }

            if (iChange == 0)//
            {
                if (myShape.ShapeType == "Rectangle")
                {
                    HTuple row0 = myShape.CtrPoints[0][0];
                    HTuple col0 = myShape.CtrPoints[0][1];

                    HTuple rowDiff = X_AftMove - row0;
                    HTuple colDiff = Y_AftMove - col0;

                    myShape.KeyPoints[0][0] = myShape.KeyPoints[0][0] + rowDiff;
                    myShape.KeyPoints[0][1] = myShape.KeyPoints[0][1] + colDiff;
                    myShape.KeyPoints[1][0] = myShape.KeyPoints[1][0] + rowDiff;
                    myShape.KeyPoints[1][1] = myShape.KeyPoints[1][1] + colDiff;
                }
                else
                {
                    myShape.KeyPoints[0] = new HTuple[2] { X_AftMove, Y_AftMove };
                }
            }
            else if (iChange > 0) // 鼠标位置有顶点则改顶点改为鼠标抬起时的位置 
            {
                if (myShape.ShapeType == "Circle")
                {
                    HTuple radius;
                    HTuple row = myShape.KeyPoints[0][0];
                    HTuple col = myShape.KeyPoints[0][1];

                    HOperatorSet.DistancePp(row, col, X_AftMove, Y_AftMove, out radius);

                    myShape.KeyPoints[2] = new HTuple[2] { radius, radius };
                    iChange = 0;
                }
                else if (myShape.ShapeType == "Ellipse")
                {
                    if (iChange == 1 || iChange == 3)
                    {
                        HTuple radius1;
                        HTuple row2 = myShape.CtrPoints[2][0];
                        HTuple col2 = myShape.CtrPoints[2][1];
                        HTuple row4 = myShape.CtrPoints[4][0];
                        HTuple col4 = myShape.CtrPoints[4][1];

                        HOperatorSet.DistancePl(X_AftMove, Y_AftMove, row2, col2, row4, col4, out radius1);
                        //HOperatorSet.DistancePp(row0, col0, X_AftMove, Y_AftMove, out radius1);

                        myShape.KeyPoints[2][0] = radius1;
                    }
                    else if (iChange == 2 || iChange == 4)
                    {
                        HTuple radius2;
                        HTuple row1 = myShape.CtrPoints[1][0];
                        HTuple col1 = myShape.CtrPoints[1][1];
                        HTuple row3 = myShape.CtrPoints[3][0];
                        HTuple col3 = myShape.CtrPoints[3][1];
                        HOperatorSet.DistancePl(X_AftMove, Y_AftMove, row1, col1, row3, col3, out radius2);
                        myShape.KeyPoints[2][1] = radius2;
                    }
                    else if (iChange == 5)
                    {
                        HTuple angleChange = new HTuple();
                        HTuple row0 = myShape.CtrPoints[0][0];
                        HTuple col0 = myShape.CtrPoints[0][1];
                        HTuple row1 = myShape.CtrPoints[1][0];
                        HTuple col1 = myShape.CtrPoints[1][1];
                        HTuple row5 = myShape.CtrPoints[5][0];
                        HTuple col5 = myShape.CtrPoints[5][1];

                        HOperatorSet.AngleLl(row0, col0, row5, col5, row0, col0, X_AftMove, Y_AftMove, out angleChange);
                        myShape.KeyPoints[1][0] = myShape.KeyPoints[1][0] + angleChange;
                    }
                    iChange = 0;
                }
                else if (myShape.ShapeType == "Rectangle")
                {
                    if (iChange == 1)
                    {
                        myShape.KeyPoints[1][1] = Y_AftMove;
                    }
                    else if (iChange == 2)
                    {
                        myShape.KeyPoints[0][0] = X_AftMove;
                    }
                    else if (iChange == 3)
                    {
                        myShape.KeyPoints[0][1] = Y_AftMove;
                    }
                    else if (iChange == 4)
                    {
                        myShape.KeyPoints[1][0] = X_AftMove;
                    }
                    iChange = 0;
                }
                else if (myShape.ShapeType == "Rectangle2")
                {
                    if (iChange == 1 || iChange == 3)
                    {
                        HTuple length1;
                        HTuple row2 = myShape.CtrPoints[2][0];
                        HTuple col2 = myShape.CtrPoints[2][1];
                        HTuple row4 = myShape.CtrPoints[4][0];
                        HTuple col4 = myShape.CtrPoints[4][1];

                        HOperatorSet.DistancePl(X_AftMove, Y_AftMove, row2, col2, row4, col4, out length1);

                        myShape.KeyPoints[2][0] = length1;
                    }
                    else if (iChange == 2 || iChange == 4)
                    {
                        HTuple length2;
                        HTuple row1 = myShape.CtrPoints[1][0];
                        HTuple col1 = myShape.CtrPoints[1][1];
                        HTuple row3 = myShape.CtrPoints[3][0];
                        HTuple col3 = myShape.CtrPoints[3][1];
                        HOperatorSet.DistancePl(X_AftMove, Y_AftMove, row1, col1, row3, col3, out length2);

                        myShape.KeyPoints[2][1] = length2;
                    }
                    else if (iChange == 5)
                    {
                        HTuple angleChange = new HTuple();
                        HTuple row0 = myShape.CtrPoints[0][0];
                        HTuple col0 = myShape.CtrPoints[0][1];
                        HTuple row1 = myShape.CtrPoints[1][0];
                        HTuple col1 = myShape.CtrPoints[1][1];
                        HTuple row5 = myShape.CtrPoints[5][0];
                        HTuple col5 = myShape.CtrPoints[5][1];

                        HOperatorSet.AngleLl(row0, col0, row5, col5, row0, col0, X_AftMove, Y_AftMove, out angleChange);
                        myShape.KeyPoints[1][0] = myShape.KeyPoints[1][0] + angleChange;
                    }
                    iChange = 0;
                }
                else if (myShape.ShapeType == "Polygon")
                {
                    myShape.KeyPoints[iChange] = new HTuple[2] { X_AftMove, Y_AftMove };
                }

            }
            else if (iChange < 0)
            {
                iChange = 0;
            }

            if (iChange >= 0)
            {
                //LbxShapes_SelectedIndexChanged(null, null);
            }

        }

        public static bool IsObjectEmpty(HObject image)
        {
            //判断 图像是否为空, 为空时返回---true
            if (image == null)
                return true;

            try
            {
                HObject emptyImg = new HObject();
                HTuple isEqual = new HTuple();
                HOperatorSet.GenEmptyObj(out emptyImg);
                HOperatorSet.TestEqualObj(image, emptyImg, out isEqual);
                return (bool)isEqual;
            }
            catch
            {
                return true;
            }
        }
    }
}

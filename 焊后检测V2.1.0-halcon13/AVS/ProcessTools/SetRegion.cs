using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;

namespace AVS
{
    public partial class SetRegion : Form
    {
        enum EnumPsn
        {
            //0---length01
            //1---length02
            //2---sigma
            //3---threshold
            //4---minSocre
            //5---transition
            //6---instanceNum
            //7---measureDistance
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
            平滑系数 = 12,
            幅度阈值 = 13,
            最小分数 = 14,
            测量极性 = 15,
            实例个数 = 16,
            测量间距 = 17,
        }
        #region 图形参数
        //定义形状结构体
        public struct ShapeMetro
        {
            public HTuple[] metroParams;
            //0---length01
            //1---length02
            //2---sigma
            //3---threshold
            //4---minSocre
            //5---transition
            //6---instanceNum
            //7---measureDistance
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
            rectL01[0] = 18 * zoomWndFactor;
            rectL01[1] = 18 * zoomWndFactor;
            rectL01[2] = 18 * zoomWndFactor;
            rectL01[3] = 18 * zoomWndFactor;
            rectL01[4] = 18 * zoomWndFactor;

            HTuple rectL02 = new HTuple();
            rectL02[0] = 18 * zoomWndFactor;
            rectL02[1] = 18 * zoomWndFactor;
            rectL02[2] = 18 * zoomWndFactor;
            rectL02[3] = 18 * zoomWndFactor;
            rectL02[4] = 18 * zoomWndFactor;
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
            rectL01[0] = 18 * zoomWndFactor;
            rectL01[1] = 18 * zoomWndFactor;
            rectL01[2] = 18 * zoomWndFactor;
            rectL01[3] = 18 * zoomWndFactor;
            rectL01[4] = 18 * zoomWndFactor;
            rectL01[5] = 18 * zoomWndFactor;

            HTuple rectL02 = new HTuple();
            rectL02[0] = 18 * zoomWndFactor;
            rectL02[1] = 18 * zoomWndFactor;
            rectL02[2] = 18 * zoomWndFactor;
            rectL02[3] = 18 * zoomWndFactor;
            rectL02[4] = 18 * zoomWndFactor;
            rectL02[5] = 18 * zoomWndFactor;
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
            rectL01[0] = 18 * zoomWndFactor;
            rectL01[1] = 18 * zoomWndFactor;
            rectL01[2] = 18 * zoomWndFactor;
            rectL01[3] = 18 * zoomWndFactor;
            rectL01[4] = 18 * zoomWndFactor;

            HTuple rectL02 = new HTuple();
            rectL02[0] = 18 * zoomWndFactor;
            rectL02[1] = 18 * zoomWndFactor;
            rectL02[2] = 18 * zoomWndFactor;
            rectL02[3] = 18 * zoomWndFactor;
            rectL02[4] = 18 * zoomWndFactor;
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
            rectL01[0] = 18 * zoomWndFactor;
            rectL01[1] = 18 * zoomWndFactor;
            rectL01[2] = 18 * zoomWndFactor;
            rectL01[3] = 18 * zoomWndFactor;
            rectL01[4] = 18 * zoomWndFactor;
            rectL01[5] = 18 * zoomWndFactor;

            HTuple rectL02 = new HTuple();
            rectL02[0] = 18 * zoomWndFactor;
            rectL02[1] = 18 * zoomWndFactor;
            rectL02[2] = 18 * zoomWndFactor;
            rectL02[3] = 18 * zoomWndFactor;
            rectL02[4] = 18 * zoomWndFactor;
            rectL02[5] = 18 * zoomWndFactor;
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
                    HOperatorSet.GenRectangle2ContourXld(out rect, x1, y1, 0, 18 * zoomWndFactor, 18 * zoomWndFactor);
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
                                shapeKey.Add(new HTuple[] { "顶点" + i.ToString() + "_X:", col, i.ToString(), "1"});
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

        // Polygon 中插入顶点
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
        private HTuple regionIndex=0;
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

        //定义区域保存路径
        string regionSavePath = "";
        //控件初始化
        
        public SetRegion()
        {
            InitializeComponent();
        }
        public SetRegion(string regionNamePath)
        {
            InitializeComponent();
            regionSavePath = regionNamePath;    
        }
        public SetRegion(string regionPath, string sideStr, string idNum)
        {
            InitializeComponent();
            regionSavePath = regionPath;
            regionSavePath = regionPath + sideStr + "\\" + "inspectRegion_" + idNum + ".hobj";
        }

        //加载窗体
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                HOperatorSet.SetWindowAttr("background_color", "blue"); //先设置一下background_color属性，再OpenWindow一下
                HoWindow.HalconWindow.OpenWindow(0, 0, HoWindow.Width, HoWindow.Height, HoWindow.Handle, "visible", "");

                hWindow = HoWindow as HWindowControl;
                this.HoWindow.HMouseDown += HoWindow_HMouseDown;
                this.HoWindow.HMouseMove += HoWindow_HMouseMove;
                this.HoWindow.HMouseUp += HoWindowDrawRegion_HMouseUp;
                this.HoWindow.HMouseWheel += HoWindow_HMouseWheel;

                this.BtnLoadImage.Click += BtnLoadImage_Click;

                this.BtnAddShape.Click += BtnAddShape_Click;
                this.BtnRemoveShape.Click += BtnRemoveShape_Click;

                BtnInsertPoint.Click += BtnInsertPoint_Click;
                BtnRemovePoint.Click += BtnRemovePoint_Click;

                LbxShapes.SelectedIndexChanged += LbxShapes_SelectedIndexChanged;

                this.BtnCreateRegion.Click += BtnCreateRegion_Click;
                this.BtnSaveRegion.Click += BtnSaveRegion_Click;    
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "提示：");
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
                }
                else if (e.Clicks == 1 && Button == 1)//鼠标左键单击
                {
                    X_B4Move = e.Y;
                    Y_B4Move = e.X;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
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

                textBox1.Text = String.Format("Row_{0}, Col_{1}, Gray_{2}", Row, Col, PointGray); //格式化字符串
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        //鼠标抬起
        private void HoWindowDrawRegion_HMouseUp(object sender, HMouseEventArgs e)
        {          
            try
            {
                HWindowControl showWindow = sender as HWindowControl;
                X_AftMove = e.Y;
                Y_AftMove = e.X;
                if (myShapesRegion.Count > 0)
                {
                    MoveControlPoint(myShapesRegion[shapeSelected].MyShape);                    
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
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

                if (myShapesRegion != null && myShapesRegion.Count > 0)
                {
                    DrawShapes(hWindow.HalconWindow, myShapesRegion, shapeSelected);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "提示：");    
            }
        }

        //通过移动控制点修改图形
        public void MoveControlPoint(Shape myShape)
        {
            //points[iChange][0] = X_AftMove;
            //points[iChange][1] = Y_AftMove;
            // 查找鼠标位置是否有顶点，如果有顶点则给出顶点索引
            if (myShape.ShapeType == "Polygon")
            {
                iChange = myShape.KeyPoints.FindIndex((HTuple[] pf) => (Math.Abs(X_B4Move.D - pf[0].D) < 15 && Math.Abs(Y_B4Move.D - pf[1].D) < 15));
            }
            else
            {
                iChange = myShape.CtrPoints.FindIndex((HTuple[] pf) => (Math.Abs(X_B4Move.D - pf[0].D) < 15 && Math.Abs(Y_B4Move.D - pf[1].D) < 15));
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

            if(iChange>=0)
            {
                LbxShapes_SelectedIndexChanged(null, null);
            }

        }
        #endregion

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            try
            {
                LoadImage();
            }
            catch(Exception ex)
            { 
                MessageBox.Show("出现以下错误，请核验！\r\n\r\n" + ex.Message.ToString());
            }                    
        }
        //创建区域设置按钮命令
        private void BtnCreateRegion_Click(object sender, EventArgs e)
        {
            try
            {
                if (wImage == null || IsObjectEmpty(wImage))
                {
                    MessageBox.Show("图像为空，请加载窗口图像！");
                    return;
                }
                if (myShapesRegion.Count < 1)
                {
                    MessageBox.Show("图形为空，请重新绘制图形！");
                    return;
                }

                int num = myShapesRegion.Count;
                HObject region01 = new HObject();
                if (num > 0)
                {
                    region01 = ReadShapeToRegion(myShapesRegion[0].MyShape);
                }
                if (num > 1)
                {
                    for (int i = 1; i < num; i++)
                    {
                        HObject region02 = ReadShapeToRegion(myShapesRegion[i].MyShape);
                        if (myShapesRegion[i].OpnMode == "∪ ")//并集
                        {
                            HOperatorSet.Union2(region01, region02, out region01);
                        }
                        else if (myShapesRegion[i].OpnMode == "∩ ")//交集
                        {
                            HOperatorSet.Intersection(region01, region02, out region01);
                        }
                        else if (myShapesRegion[i].OpnMode == "－ ")
                        {
                            HOperatorSet.Difference(region01, region02, out region01);
                        }
                        else if (myShapesRegion[i].OpnMode == "∧ ")
                        {
                            HOperatorSet.SymmDifference(region01, region02, out region01);
                        }
                    }
                }
                //***************************************************
                HOperatorSet.CopyObj(region01, out regions[regionIndex], 1, 1);
                HObject regionXLD = new HObject();
                HOperatorSet.GenContourRegionXld(regions[regionIndex], out regionXLD, "border_holes");
                //***************************************************
                HOperatorSet.ClearWindow(hWindow.HalconWindow);
                HWindowControl hW = (HWindowControl)hWindow;
                hWindow.HalconWindow.GetPart(out current_beginRow, out current_beginCol, out current_endRow, out current_endCol);
                HOperatorSet.SetPart(hWindow.HalconWindow, current_beginRow, current_beginCol, current_endRow, current_endCol);
                HOperatorSet.DispObj(wImage, hWindow.HalconWindow);
                HOperatorSet.DispObj(regionXLD, hWindow.HalconWindow);
                region01.Dispose();
                //myShapesRegion.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "提示：");
            }
        }

        //保存区域按钮命令
        private void BtnSaveRegion_Click(object sender, EventArgs e)
        {
            try
            {
                if (wImage == null || IsObjectEmpty(wImage))
                {
                    MessageBox.Show("图像为空，请加载窗口图像！");
                    return;
                }
                if (myShapesRegion.Count < 1)
                {
                    MessageBox.Show("图形为空，请重新绘制图形！");
                    return;
                }

                int num = myShapesRegion.Count;
                HObject region01 = new HObject();
                if (num > 0)
                {
                    region01 = ReadShapeToRegion(myShapesRegion[0].MyShape);
                }
                if (num > 1)
                {
                    for (int i = 1; i < num; i++)
                    {
                        HObject region02 = ReadShapeToRegion(myShapesRegion[i].MyShape);
                        if (myShapesRegion[i].OpnMode == "∪ ")//并集
                        {
                            HOperatorSet.Union2(region01, region02, out region01);
                        }
                        else if (myShapesRegion[i].OpnMode == "∩ ")//交集
                        {
                            HOperatorSet.Intersection(region01, region02, out region01);
                        }
                        else if (myShapesRegion[i].OpnMode == "－ ")
                        {
                            HOperatorSet.Difference(region01, region02, out region01);
                        }
                        else if (myShapesRegion[i].OpnMode == "∧ ")
                        {
                            HOperatorSet.SymmDifference(region01, region02, out region01);
                        }
                    }
                }
                //***************************************************保存
                HOperatorSet.CopyObj(region01, out regions[regionIndex], 1, 1);
                HOperatorSet.WriteRegion(region01,regionSavePath);
                HObject regionXLD = new HObject();
                HOperatorSet.GenContourRegionXld(regions[regionIndex], out regionXLD, "border_holes");
                //***************************************************
                HOperatorSet.ClearWindow(hWindow.HalconWindow);
                HWindowControl hW = (HWindowControl)hWindow;
                hWindow.HalconWindow.GetPart(out current_beginRow, out current_beginCol, out current_endRow, out current_endCol);
                HOperatorSet.SetPart(hWindow.HalconWindow, current_beginRow, current_beginCol, current_endRow, current_endCol);
                HOperatorSet.DispObj(wImage, hWindow.HalconWindow);
                HOperatorSet.DispObj(regionXLD, hWindow.HalconWindow);
                region01.Dispose();
                myShapesRegion.Clear();
                LbxShapes.Items.Clear();
                LbxShapeKey.Items.Clear();
                myShapesRegion.Clear();
                BtnInsertPoint.Enabled = false;
                BtnRemovePoint.Enabled = false;
                MessageBox.Show("区域保存成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "提示：");
            }
        }

        private void LbxShapeKey_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.LbxShapeKey.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                HOperatorSet.ClearWindow(hWindow.HalconWindow);
                HOperatorSet.DispObj(wImage, hWindow.HalconWindow);
                List<HTuple[]> shapeKey = DrawShapes(hWindow.HalconWindow, myShapesRegion, LbxShapes.SelectedIndex);

                string titleStr = shapeKey[index][0].S.ToString();
                HTuple itemValue = shapeKey[index][1];
                int psnM = int.Parse(shapeKey[index][2].S);
                int psnN = int.Parse(shapeKey[index][3].S);

                InputBoxRegion inbox = new InputBoxRegion(titleStr, itemValue);

                //int x = this.Location.X;
                //int y = this.Location.Y;
                //inbox.Location = new Point(x, y);   
                DialogResult dr = inbox.ShowDialog();
                if (dr == DialogResult.OK && inbox.valueForRenew.Length > 0)
                {
                    myShapesRegion[LbxShapes.SelectedIndex].MyShape.KeyPoints[psnM][psnN] = inbox.valueForRenew;
                    LbxShapes_SelectedIndexChanged(null,null);
                }
                inbox.Dispose();
            }
        }

        #region 图形添加修改相关
        //添加图形按钮命令
        private void BtnAddShape_Click(object sender, EventArgs e)
        {
            try
            {
                if(wImage == null || IsObjectEmpty(wImage))
                {
                    MessageBox.Show("窗口图像为空，请先加载窗口图像！");
                    return;
                }
                string shapeType = "";
                string opnMode = "";
                // 确定形状选择
                if (RbnCircle.Checked == true)
                {
                    shapeType = "Circle";
                }
                else if (RbnEllipse.Checked == true)
                {
                    shapeType = "Ellipse";
                }
                else if (RbnRectN.Checked == true)
                {
                    shapeType = "Rectangle";
                }
                else if (RbnRectA.Checked == true)
                {
                    shapeType = "Rectangle2";
                }
                else if (RbnPolygon.Checked == true)
                {
                    shapeType = "Polygon";
                }
                //确定运算选择
                if (RbnUnion2.Checked == true)
                {
                    opnMode = "∪ ";
                }
                else if (RbnIntersection.Checked == true)
                {
                    opnMode = "∩ ";
                }
                else if (RbnDifference.Checked == true)
                {
                    opnMode = "－ ";
                }
                else if (RbnSymmDiff.Checked == true)
                {
                    opnMode = "∧ ";
                }
                if (shapeType != "" && opnMode != "")
                {
                    if (LbxShapes.SelectedIndex < 0)
                    {
                        LbxShapes.Items.Insert(0, opnMode + shapeType);
                        AddShape(myShapesRegion, opnMode, shapeType, 0);
                        //*********
                        //DrawShapes(hWindow.HalconWindow, myShapesRegion, 0);
                        //*********
                        LbxShapes.SelectedIndex = 0;
                    }
                    else
                    {
                        LbxShapes.Items.Insert(LbxShapes.SelectedIndex + 1, opnMode + shapeType);
                        AddShape(myShapesRegion, opnMode, shapeType, LbxShapes.SelectedIndex + 1);
                        //DrawShapes(hWindow.HalconWindow, myShapesRegion, LbxShapes.SelectedIndex + 1);
                        LbxShapes.SelectedIndex++;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "提示：");
            }            
        }//添加图形

        //删除图形按钮命令
        private void BtnRemoveShape_Click(object sender, EventArgs e)
        {
            try
            {
                if (LbxShapes.SelectedIndex >= 0)
                {
                    RemoveShape(myShapesRegion, LbxShapes.SelectedIndex);

                    LbxShapes.Items.RemoveAt(LbxShapes.SelectedIndex);

                    if (LbxShapes.Items.Count > 0)
                    {
                        LbxShapes.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "提示：");
            }
        }//去除图形
        //Polygon插入顶点按钮命令
        private void BtnInsertPoint_Click(object sender, EventArgs e)//Polygon中插入顶点
        {
            try
            {
                InsertPoint(myShapesRegion, LbxShapes.SelectedIndex);
                LbxShapes_SelectedIndexChanged(null, null);
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message.ToString(), "提示：");
            }  
        }
        //Polygon删除顶点按钮命令
        private void BtnRemovePoint_Click(object sender, EventArgs e)
        {
            try
            {
                RemovePoint(myShapesRegion, LbxShapes.SelectedIndex);
                LbxShapes_SelectedIndexChanged(null, null);
            }
            catch( Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "提示：");
            }
        }//Polygon中删除顶点
        //图形选中命令---当图形列表中某个图形被选中时把图形显示出来，被选中的蓝色显示并可以被修改
        private void LbxShapes_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (LbxShapes.SelectedItem != null && LbxShapes.SelectedItem.ToString().Contains("Polygon"))
                {
                    BtnInsertPoint.Enabled = true;
                    BtnRemovePoint.Enabled = true;
                }
                else
                {
                    BtnInsertPoint.Enabled = false;
                    BtnRemovePoint.Enabled = false;
                }
                //ShowImage(ImageOpn.ho_Image);
                shapeSelected = LbxShapes.SelectedIndex;

                HOperatorSet.ClearWindow(hWindow.HalconWindow);
                HOperatorSet.DispObj(wImage, hWindow.HalconWindow);
                //DrawShapes(hWindow.HalconWindow, myShapesRegion, shapeSelected);
                List<HTuple[]> shapeKey = DrawShapes(hWindow.HalconWindow, myShapesRegion, LbxShapes.SelectedIndex);

                LbxShapeKey.Items.Clear();
                int num = shapeKey.Count;
                if (num > 0)
                {
                    for (int i = 0; i < num; i++)
                    {
                        int numitem = shapeKey[i].Length;
                        if (numitem == 4)
                        {
                            string itemStr = "";
                            string aaa = shapeKey[i][0].S.ToString();
                            string bbb = shapeKey[i][1].D.ToString("0.0");
                            string ccc = shapeKey[i][2].S.ToString();
                            string ddd = shapeKey[i][3].S.ToString();
                            itemStr = aaa + " " + bbb;
                            LbxShapeKey.Items.Add(itemStr);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }            
        }
        #endregion
        public HObject ReadShapeToRegion(Shape myShape)
        {
            HObject shapeRegion = new HObject();
            if (myShape.ShapeType == "Circle")
            {
                HTuple row = myShape.KeyPoints[0][0];
                HTuple col = myShape.KeyPoints[0][1];
                HTuple radius = myShape.KeyPoints[2][0];
                HTuple phiB = myShape.KeyPoints[3][0];
                HTuple phiE = myShape.KeyPoints[3][1];
                if ((phiB.D == 0 && phiE.D == 360) || (phiB.D == 360 && phiE.D == 0) || (phiB.D == phiE.D))
                {
                    phiB = phiB.TupleRad();
                    phiE = phiE.TupleRad();
                    HOperatorSet.GenCircle(out shapeRegion, row, col, radius);
                }
                else
                {
                    phiB = phiB.TupleRad();
                    phiE = phiE.TupleRad();
                    HOperatorSet.GenCircleSector(out shapeRegion, row, col, radius, phiB, phiE);
                }
            }
            else if (myShape.ShapeType == "Ellipse")
            {
                HTuple row = myShape.KeyPoints[0][0];
                HTuple col = myShape.KeyPoints[0][1];
                HTuple phi = myShape.KeyPoints[1][0].TupleRad();
                HTuple radius1 = myShape.KeyPoints[2][0];
                HTuple radius2 = myShape.KeyPoints[2][1];
                HTuple phiB = myShape.KeyPoints[3][0];
                HTuple phiE = myShape.KeyPoints[3][1];

                if ((phiB.D == 0 && phiE.D == 360) || (phiB.D == 360 && phiE.D == 0) || (phiB.D == phiE.D))
                {
                    phiB = phiB.TupleRad();
                    phiE = phiE.TupleRad();
                    HOperatorSet.GenEllipse(out shapeRegion, row, col, phi, radius1, radius2);
                }
                else
                {
                    phiB = phiB.TupleRad();
                    phiE = phiE.TupleRad();
                    HOperatorSet.GenEllipseSector(out shapeRegion, row, col, phi, radius1, radius2, phiB, phiE);
                }
            }
            else if (myShape.ShapeType == "Rectangle")
            {
                HTuple row1 = myShape.KeyPoints[0][0];
                HTuple col1 = myShape.KeyPoints[0][1];
                HTuple row2 = myShape.KeyPoints[1][0];
                HTuple col2 = myShape.KeyPoints[1][1];

                //HOperatorSet.GenRectangle2(out shapeRegion, (row1 + row2) * 0.5, (col1 + col2) * 0.5, 0, 100, 50);
                HTuple length1 = new HTuple();
                HTuple length2 = new HTuple();
                HOperatorSet.TupleAbs((col2 - col1) * 0.5, out length1);
                HOperatorSet.TupleAbs((row2 - row1) * 0.5, out length2);

                HOperatorSet.GenRectangle2(out shapeRegion, (row1 + row2) * 0.5, (col1 + col2) * 0.5, 0, length1, length2);
            }
            else if (myShape.ShapeType == "Rectangle2")
            {
                HTuple row = myShape.KeyPoints[0][0];
                HTuple col = myShape.KeyPoints[0][1];
                HTuple phi = myShape.KeyPoints[1][0].TupleRad();
                HTuple length1 = myShape.KeyPoints[2][0];
                HTuple length2 = myShape.KeyPoints[2][1];
                HOperatorSet.GenRectangle2(out shapeRegion, row, col, phi, length1, length2);
            }
            else if (myShape.ShapeType == "Polygon")
            {
                HTuple rows = new HTuple();
                HTuple cols = new HTuple();

                int Num = myShape.KeyPoints.Count();
                for (int i = 0; i <= Num - 1; i++)
                {
                    rows.Append(myShape.KeyPoints[i][0]);
                    cols.Append(myShape.KeyPoints[i][1]);
                }
                rows.Append(myShape.KeyPoints[0][0]);
                cols.Append(myShape.KeyPoints[0][1]);
                HOperatorSet.GenRegionPolygonFilled(out shapeRegion, rows, cols);
            }
            return (shapeRegion);
        }
        private void LoadImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = @"All Image Files|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.png;*.tif;*.tiff|
                               Windows Bitmap(*.bmp)|*.bmp|
                               Windows Icon(*.ico)|*.ico|
                               Graphics Interchange Format (*.gif)|(*.gif)|
                               JPEG File Interchange Format (*.jpg)|*.jpg;*.jpeg|
                               Portable Network Graphics (*.png)|*.png|
                               Tag Image File Format (*.tif)|*.tif;*.tiff|
                               All files(*.*) | *.* ";
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
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message.ToString());
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
            catch(Exception ex)
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
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message.ToString());
                return;
            }
        }

        private void HDispMessage(HTuple windowHandle, string message, int row, int column, string color, string font, string fontSize, bool bold)
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

            HOperatorSet.SetTposition(windowHandle, row, column);
            HOperatorSet.WriteString(windowHandle, message);
            HOperatorSet.SetColor(windowHandle, "green");
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

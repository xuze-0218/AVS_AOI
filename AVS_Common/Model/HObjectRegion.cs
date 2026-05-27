using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System;
namespace AVS_Common.Model
{


    public enum RoiType
    {
        RECTANGLE1,
        RECTANGLE2,
        CIRCLE,
        POLYGON
    }

    public class HObjectRegion : IDisposable
    {
        // ---------- 属性 ----------
        public RoiType Style { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public double LeftX { get; set; }
        public double LeftY { get; set; }
        public double RightX { get; set; }
        public double RightY { get; set; }
        public double Radius { get; set; }
        public double Length1 { get; set; }
        public double Length2 { get; set; }
        public string Color { get; set; }

        // 多边形专用顶点
        public HTuple PolyRows { get; set; }
        public HTuple PolyCols { get; set; }
        //只读，通过 GenerateRegion() 更新
        public HObject Region { get; set; }
        private HTuple _drawingObject = null;

        public HObjectRegion()
        {
            Style = RoiType.RECTANGLE1;
            X = 300; Y = 300;
            LeftX = 100; LeftY = 100;
            RightX = 800; RightY = 800;
            Radius = 200;
            Length1 = 200; Length2 = 200;
            Angle = 0;
            Color = "blue";
            PolyRows = new HTuple();
            PolyCols = new HTuple();
            Region = new HObject();
            Region.GenEmptyObj();
        }


        /// <summary>
        /// 根据当前属性重新生成 Region（静态区域，不涉及交互）
        /// </summary>
        public bool GenerateRegion()
        {
            Region?.Dispose();
            Region = new HObject();

            HObject tempRegion = null;
            try
            {
                switch (Style)
                {
                    case RoiType.RECTANGLE1:
                        HOperatorSet.GenRectangle1(out tempRegion, LeftY, LeftX, RightY, RightX);
                        break;
                    case RoiType.RECTANGLE2:
                        HOperatorSet.GenRectangle2(out tempRegion, Y, X, Angle, Length1, Length2);
                        break;
                    case RoiType.CIRCLE:
                        HOperatorSet.GenCircle(out tempRegion, Y, X, Radius);
                        break;
                    case RoiType.POLYGON:
                        if (PolyRows.Length > 0)
                            HOperatorSet.GenRegionPolygonFilled(out tempRegion, PolyRows, PolyCols);
                        else
                            tempRegion = new HObject();
                        break;
                    default:
                        Region.GenEmptyObj();
                        return false;
                }
                Region = tempRegion ?? new HObject();
                return true;
            }
            catch (Exception)
            {
                // 发生异常时确保 Region 为空对象
                Region?.Dispose();
                Region = new HObject();
                Region.GenEmptyObj();
                return false;
            }
        }

        /// <summary>
        /// 在指定窗口中创建可交互的 DrawingObject，支持拖动调整
        /// 非多边形使用 DrawingObject，多边形返回 false，表示需外部自定义交互
        /// </summary>
        public bool AttachDrawingObject(HWindow window)
        {
            if (window == null) return false;
            if (Style == RoiType.POLYGON) return false;
            DetachDrawingObject(); // 清理旧的

            HOperatorSet.SetSystem("flush_graphic", "false");
            try
            {
                switch (Style)
                {
                    case RoiType.RECTANGLE1:

                        HOperatorSet.CreateDrawingObjectRectangle1(LeftY, LeftX, RightY, RightX, out _drawingObject);
                        break;
                    case RoiType.RECTANGLE2:
                        HOperatorSet.CreateDrawingObjectRectangle2(Y, X, Angle, Length1, Length2, out _drawingObject);
                        break;
                    case RoiType.CIRCLE:
                        HOperatorSet.CreateDrawingObjectCircle(Y, X, Radius, out _drawingObject);
                        break;
                    default:
                        HOperatorSet.SetSystem("flush_graphic", "true");
                        return false;
                }

                HOperatorSet.SetDrawingObjectParams(_drawingObject, "color", Color);
                HOperatorSet.SetDrawingObjectParams(_drawingObject, "line_width", 2);
                HOperatorSet.AttachDrawingObjectToWindow(window, _drawingObject);
            }
            catch
            {
                HOperatorSet.SetSystem("flush_graphic", "true");
                return false;
            }
            HOperatorSet.SetSystem("flush_graphic", "true");
            return true;
        }

        /// <summary>
        /// 从 DrawingObject 同步当前参数值（拖动后调用）
        /// </summary>
        public bool SyncFromDrawingObject()
        {
            if (_drawingObject == null) return false;

            try
            {
                switch (Style)
                {
                    case RoiType.RECTANGLE1:
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "row1", out HTuple r1);
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "column1", out HTuple c1);
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "row2", out HTuple r2);
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "column2", out HTuple c2);
                        LeftX = c1.D; LeftY = r1.D;
                        RightX = c2.D; RightY = r2.D;
                        X = (LeftX + RightX) / 2;
                        Y = (LeftY + RightY) / 2;
                        Length1 = Math.Abs(RightY - LeftY) / 2;
                        Length2 = Math.Abs(RightX - LeftX) / 2;
                        Radius = Math.Min(Length1, Length2);
                        break;
                    case RoiType.RECTANGLE2:
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "row", out HTuple row);
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "column", out HTuple col);
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "length1", out HTuple l1);
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "length2", out HTuple l2);
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "phi", out HTuple phi);
                        Y = row.D; X = col.D;
                        Length1 = l1.D; Length2 = l2.D;
                        Angle = phi.D;
                        Radius = Math.Min(Length1, Length2);
                        LeftX = X - Length2; LeftY = Y - Length1;
                        RightX = X + Length2; RightY = Y + Length1;
                        break;
                    case RoiType.CIRCLE:
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "row", out HTuple cr);
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "column", out HTuple cc);
                        HOperatorSet.GetDrawingObjectParams(_drawingObject, "radius", out HTuple crd);
                        Y = cr.D; X = cc.D; Radius = crd.D;
                        Length1 = Length2 = Radius;
                        LeftX = X - Radius; LeftY = Y - Radius;
                        RightX = X + Radius; RightY = Y + Radius;
                        break;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 手动设置多边形顶点并生成区域（外部完成绘制后调用）
        public void SetPolygonVertices(HTuple rows, HTuple cols)
        {
            PolyRows = rows.Clone();
            PolyCols = cols.Clone();
            // 计算中心点
            if (rows.Length > 0)
            {
                Y = rows.TupleMean().D;
                X = cols.TupleMean().D;
            }
        }

        /// <summary>
        /// 移除 DrawingObject（不销毁对象数据）
        /// </summary>
        public void DetachDrawingObject()
        {
            if (_drawingObject != null)
            {
                HOperatorSet.ClearDrawingObject(_drawingObject);
                _drawingObject = null;
            }
        }

        // ---------- 资源释放 ----------
        public void Dispose()
        {
            DetachDrawingObject();
            Region?.Dispose();
            Region = null;
        }
    }

}

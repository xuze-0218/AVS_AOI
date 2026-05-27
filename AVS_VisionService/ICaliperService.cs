using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service
{
    /// <summary>
    /// 卡尺测量选择模式
    /// </summary>
    public enum CaliperSelectMode
    {
        Distance = 0,  // 距离
        Width = 1,     // 宽度
        Row = 2,       // Row坐标
        Col = 3        // Col坐标
    }

    /// <summary>
    /// 边缘过渡方向
    /// </summary>
    public enum CaliperTransition
    {
        Positive = 1,  // 白到黑
        Negative = 2,  // 黑到白
        All = 3        // 全部
    }

    /// <summary>
    /// 边缘选择
    /// </summary>
    public enum CaliperSelect
    {
        First = 1,  // 第一个
        Last = 2,   // 最后一个
        All = 3     // 全部
    }

    public interface ICaliperService
    {
        void SetHalconWindow(HWindow window);
        void SetImage(HObject image);

        /// <summary>
        /// 执行卡尺测量（单边缘模式）
        /// </summary>
        void MeasureCaliper(
            HObject region,
            int sigma,
            int threshold,
            CaliperTransition transition,
            CaliperSelect select,
            out HTuple rows,
            out HTuple cols,
            out HTuple amplitudes,
            out HTuple distances);

        /// <summary>
        /// 执行卡尺测量（边缘对模式）
        /// </summary>
        void MeasureCaliperEdgePairs(
            HObject region,
            int sigma,
            int threshold,
            CaliperTransition transition,
            CaliperSelect select,
            out HTuple rows1,
            out HTuple cols1,
            out HTuple amplitudes1,
            out HTuple rows2,
            out HTuple cols2,
            out HTuple amplitudes2,
            out HTuple interDistances,
            out HTuple intraDistances);

        /// <summary>
        /// 显示卡尺测量结果（单边缘）
        /// </summary>
        void DisplaySingleEdgeResult(
            HTuple rows,
            HTuple cols,
            HTuple amplitudes,
            bool showRule,
            bool showLine,
            bool showCross);

        /// <summary>
        /// 显示卡尺测量结果（边缘对）
        /// </summary>
        void DisplayEdgePairResult(
            HTuple rows1,
            HTuple cols1,
            HTuple amplitudes1,
            HTuple rows2,
            HTuple cols2,
            HTuple amplitudes2,
            bool showRule,
            bool showLine,
            bool showCross);

        /// <summary>
        /// 获取测量到的距离/宽度等
        /// </summary>
        HTuple GetMeasureResult(
            HTuple rows,
            HTuple cols,
            CaliperSelectMode selectMode,
            double scale);

        /// <summary>
        /// 获取边缘对的距离/宽度结果
        /// </summary>
        void GetEdgePairMeasureResult(
            HTuple rows1,
            HTuple cols1,
            HTuple rows2,
            HTuple cols2,
            CaliperSelectMode selectMode,
            double scale,
            out HTuple interDistances,
            out HTuple intraDistances);
    }

    public class CaliperService : ICaliperService, IDisposable
    {
        private HWindow _halconWindow;
        private HObject _currentImage;
        private HTuple _tempMetrologyHandle = null;

        public void SetHalconWindow(HWindow window) => _halconWindow = window;
        public void SetImage(HObject image) => _currentImage = image;

        public void MeasureCaliper(
            HObject region,
            int sigma,
            int threshold,
            CaliperTransition transition,
            CaliperSelect select,
            out HTuple rows,
            out HTuple cols,
            out HTuple amplitudes,
            out HTuple distances)
        {
            rows = new HTuple(); cols = new HTuple();
            amplitudes = new HTuple(); distances = new HTuple();

            if (_currentImage == null || region == null) return;

            try
            {
                // 从区域获取参数行
                HTuple paramValues = new HTuple();
                HOperatorSet.GetRegionPolygon(region, 1.0, out HTuple regRows, out HTuple regCols);

                if (regRows.Length < 2) return;

                // 使用 measure_pos 算子进行单边缘卡尺测量
                string trans = "all";
                if (transition == CaliperTransition.Positive) trans = "positive";
                else if (transition == CaliperTransition.Negative) trans = "negative";

                string sel = "all";
                if (select == CaliperSelect.First) sel = "first";
                else if (select == CaliperSelect.Last) sel = "last";

                HOperatorSet.MeasurePos(
                    _currentImage,
                    _tempMetrologyHandle,
                    new HTuple(sigma),
                    new HTuple(threshold),
                    trans,
                    sel,

                    out rows,
                    out cols,
                    out amplitudes,
                    out distances);
            }
            catch (HalconException)
            {
                // 测量失败返回空
            }
        }

        public void MeasureCaliperEdgePairs(
            HObject region,
            int sigma,
            int threshold,
            CaliperTransition transition,
            CaliperSelect select,
            out HTuple rows1,
            out HTuple cols1,
            out HTuple amplitudes1,
            out HTuple rows2,
            out HTuple cols2,
            out HTuple amplitudes2,
            out HTuple interDistances,
            out HTuple intraDistances)
        {
            rows1 = new HTuple(); cols1 = new HTuple(); amplitudes1 = new HTuple();
            rows2 = new HTuple(); cols2 = new HTuple(); amplitudes2 = new HTuple();
            interDistances = new HTuple(); intraDistances = new HTuple();

            if (_currentImage == null || region == null) return;

            try
            {
                HOperatorSet.GetRegionPolygon(region, 1.0, out HTuple regRows, out HTuple regCols);//MeasurePairs方法去掉了这个
                if (regRows.Length < 2) return;

                string trans = "all";
                if (transition == CaliperTransition.Positive) trans = "positive";
                else if (transition == CaliperTransition.Negative) trans = "negative";

                string sel = "all";
                if (select == CaliperSelect.First) sel = "first";
                else if (select == CaliperSelect.Last) sel = "last";

                HOperatorSet.MeasurePairs(
                    _currentImage,
                    _tempMetrologyHandle,
                    new HTuple(sigma),
                    new HTuple(threshold),
                    trans,
                    sel,

                    out rows1,
                    out cols1,
                    out amplitudes1,
                    out rows2,
                    out cols2,
                    out amplitudes2,
                    out intraDistances,
                    out interDistances);
            }
            catch (HalconException)
            {
                // 测量失败返回空
            }
        }

        public void DisplaySingleEdgeResult(
            HTuple rows,
            HTuple cols,
            HTuple amplitudes,
            bool showRule,
            bool showLine,
            bool showCross)
        {
            if (_halconWindow == null || rows == null || rows.Length == 0) return;

            _halconWindow.SetLineWidth(2);

            if (showCross)
            {
                _halconWindow.SetColor("lime green");
                for (int i = 0; i < rows.Length; i++)
                    _halconWindow.DispCross((HTuple)rows[i], cols[i], 12, 0);
            }
        }

        public void DisplayEdgePairResult(
            HTuple rows1,
            HTuple cols1,
            HTuple amplitudes1,
            HTuple rows2,
            HTuple cols2,
            HTuple amplitudes2,
            bool showRule,
            bool showLine,
            bool showCross)
        {
            if (_halconWindow == null) return;

            _halconWindow.SetLineWidth(2);

            if (showCross)
            {
                if (rows1 != null && rows1.Length > 0)
                {
                    _halconWindow.SetColor("lime green");
                    for (int i = 0; i < rows1.Length; i++)
                        _halconWindow.DispCross((HTuple)rows1[i], cols1[i], 12, 0);

                    if (rows2 != null && rows2.Length > 0)
                    {
                        _halconWindow.SetColor("red");
                        for (int i = 0; i < rows2.Length; i++)
                            _halconWindow.DispCross((HTuple)rows2[i], cols2[i], 12, 0);

                        // 绘制连接线
                        if (rows1.Length == rows2.Length)
                        {
                            _halconWindow.SetColor("yellow");
                            for (int i = 0; i < rows1.Length; i++)
                            {
                                _halconWindow.DispLine((HTuple)rows1[i], cols1[i], rows2[i], cols2[i]);
                            }
                        }
                    }
                }
            }
        }

        public HTuple GetMeasureResult(
            HTuple rows,
            HTuple cols,
            CaliperSelectMode selectMode,
            double scale)
        {
            if (rows == null || rows.Length == 0) return new HTuple();

            HTuple result = new HTuple();
            switch (selectMode)
            {
                case CaliperSelectMode.Distance:
                    if (rows.Length == 2)
                    {
                        double dist = Math.Sqrt(
                            (rows[0].D - rows[1].D) * (rows[0].D - rows[1].D) +
                            (cols[0].D - cols[1].D) * (cols[0].D - cols[1].D));
                        result = new HTuple(dist * scale);
                    }
                    break;
                case CaliperSelectMode.Width:
                    if (rows.Length == 2)
                    {
                        double width = Math.Abs(cols[0].D - cols[1].D);
                        result = new HTuple(width * scale);
                    }
                    break;
                case CaliperSelectMode.Row:
                    result = rows.TupleMult(scale);
                    break;
                case CaliperSelectMode.Col:
                    result = cols.TupleMult(scale);
                    break;
            }
            return result;
        }

        public void GetEdgePairMeasureResult(
            HTuple rows1,
            HTuple cols1,
            HTuple rows2,
            HTuple cols2,
            CaliperSelectMode selectMode,
            double scale,
            out HTuple interDistances,
            out HTuple intraDistances)
        {
            interDistances = new HTuple();
            intraDistances = new HTuple();

            if (rows1 == null || rows1.Length == 0 || rows2 == null || rows2.Length == 0)
                return;

            int count = Math.Min(rows1.Length, rows2.Length);
            for (int i = 0; i < count; i++)
            {
                // 边缘对内部距离（width）
                double intraDist = Math.Abs(cols1[i].D - cols2[i].D) * scale;
                intraDistances = intraDistances.TupleConcat(new HTuple(intraDist));
            }

            // 相邻边缘对之间的距离
            for (int i = 0; i < count - 1; i++)
            {
                double interDist = Math.Sqrt(
                    (rows1[i].D - rows1[i + 1].D) * (rows1[i].D - rows1[i + 1].D) +
                    (cols1[i].D - cols1[i + 1].D) * (cols1[i].D - cols1[i + 1].D)) * scale;
                interDistances = interDistances.TupleConcat(new HTuple(interDist));
            }
        }

        public void Dispose()
        {
            _currentImage?.Dispose();
            _tempMetrologyHandle = null;
        }
    }
}
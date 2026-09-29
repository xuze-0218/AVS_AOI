using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Services
{
    public interface ITemplateMatchingService
    {
        void SetHalconWindow(HWindow window);
        HObject LoadImage(string filePath);
        HObject GetCurrentImage();

        /// <summary>
        /// 创建形状模板
        /// </summary>
        /// <param name="templateImage">已缩小的模板图像</param>
        /// <param name="angleStart">起始角度(弧度)</param>
        /// <param name="angleExtent">角度范围(弧度)</param>
        /// <param name="numLevels">金字塔层级，"auto" 或数字字符串</param>
        /// <param name="contrast">对比度，"auto" 或 "low,high" 格式</param>
        /// <param name="minContrast">最小对比度，"auto" 或数字字符串</param>
        /// <param name="metric">度量方式</param>
        /// <param name="optimization">优化方式</param>
        /// <returns>模板句柄</returns>
        HTuple CreateShapeModel(HObject templateImage, double angleStart, double angleExtent, string numLevels,
            string contrast, string minContrast, string metric, string optimization);

        /// <summary>
        /// 查找形状模板
        /// </summary>
        /// <param name="modelId">模板句柄</param>
        /// <param name="angleStart">起始角度</param>
        /// <param name="angleExtent">角度范围</param>
        /// <param name="scaleMin">最小缩放</param>
        /// <param name="scaleMax">最大缩放</param>
        /// <param name="minScore">最小分数</param>
        /// <param name="numMatches">最大匹配数</param>
        /// <param name="maxOverlap">最大重叠</param>
        /// <param name="subPixel">亚像素精度</param>
        /// <param name="numLevels">金字塔层级</param>
        /// <param name="greediness">贪婪度</param>
        void FindShapeModel(HTuple modelId, double angleStart, double angleExtent, double scaleMin, double scaleMax, double minScore,
            int numMatches, double maxOverlap, string subPixel, int numLevels, double greediness,
            out HTuple row, out HTuple col, out HTuple angle, out HTuple score);

        /// <summary>
        /// 保存模板到文件
        /// </summary>
        void SaveShapeModel(HTuple modelId, string filePath);

        /// <summary>
        /// 从文件加载模板
        /// </summary>
        HTuple LoadShapeModel(string filePath);

        /// <summary>
        /// 显示模板匹配结果（含掩膜绘制）
        /// </summary>
        void DisplayResult(HTuple modelId, HTuple row, HTuple col, HTuple angle, HTuple score);

        /// <summary>
        /// 显示模板特征轮廓点（创建模板后调用）
        /// </summary>
        /// <param name="modelId">模板句柄</param>
        /// <param name="roiRegion">创建模板时用的有效区域（用于取模板原点）</param>
        /// <param name="featureScale">特征尺度 1~5，1 最密，5 最疏</param>
        void DisplayModelFeaturePoints(HTuple modelId, HObject roiRegion, int featureScale);

        /// <summary>
        /// 取模板轮廓点在图像坐标下的采样点（不绘制）
        /// </summary>
        bool TryGetModelFeaturePoints(HTuple modelId, HObject roiRegion, int featureScale, out double[] rows, out double[] cols);
    }

    public class TemplateMatchingService : ITemplateMatchingService, IDisposable
    {
        private HWindow _halconWindow;
        private HObject _currentImage;
        private HTuple _modelId = null;

        public void SetHalconWindow(HWindow window) => _halconWindow = window;

        public HObject LoadImage(string filePath)
        {
            _currentImage?.Dispose();
            HOperatorSet.ReadImage(out _currentImage, filePath);
            return _currentImage;
        }

        public HObject GetCurrentImage() => _currentImage;

        public HTuple CreateShapeModel(HObject templateImage, double angleStart, double angleExtent, string numLevels,
            string contrast, string minContrast, string metric, string optimization)
        {
            if (_modelId != null && _modelId.Length != 0) HOperatorSet.ClearShapeModel(_modelId);

            // 解析 numLevels
            HTuple hvNumLevels;
            if (numLevels == "auto" || string.IsNullOrEmpty(numLevels))
                hvNumLevels = new HTuple("auto");
            else if (int.TryParse(numLevels, out int nl))
                hvNumLevels = new HTuple(nl);
            else
                hvNumLevels = new HTuple("auto");

            // 解析 contrast
            HTuple hvContrast;
            if (string.IsNullOrWhiteSpace(contrast) || contrast.Trim().ToLower() == "auto")
            {
                hvContrast = new HTuple("auto");
            }
            else if (double.TryParse(contrast.Trim(), out double c) && c > 0)
                hvContrast = new HTuple(c);          //单值
            else
                hvContrast = new HTuple("auto");     // 非法输入回退

            // 解析 minContrast
            HTuple hvMinContrast;
            if (minContrast == "auto" || string.IsNullOrEmpty(minContrast))
                hvMinContrast = new HTuple("auto");
            else if (double.TryParse(minContrast, out double mc))
                hvMinContrast = new HTuple(mc);
            else
                hvMinContrast = new HTuple("auto");

            HOperatorSet.CreateShapeModel(templateImage, hvNumLevels, new HTuple(angleStart).TupleRad(), new HTuple(angleExtent).TupleRad(),
                "auto", optimization, metric, hvContrast, hvMinContrast, out _modelId);
            return _modelId.Clone();
        }

        public void FindShapeModel(HTuple modelId, double angleStart, double angleExtent, double scaleMin,
            double scaleMax, double minScore, int numMatches, double maxOverlap, string subPixel, int numLevels, double greediness,
            out HTuple row, out HTuple col, out HTuple angle, out HTuple score)
        {
            row = col = angle = score = new HTuple();

            if (_currentImage == null) throw new InvalidOperationException("No image loaded.");
            if (modelId == null) throw new InvalidOperationException("No model created.");

            HOperatorSet.FindScaledShapeModel(
                _currentImage,
                modelId,
                new HTuple(angleStart),
                new HTuple(angleExtent),
                new HTuple(scaleMin),
                new HTuple(scaleMax),
                minScore,
                numMatches,
                maxOverlap,
                subPixel,
                new HTuple(numLevels),
                greediness,
                out row, out col, out angle, out HTuple scale, out score);
        }


        public void DisplayModelFeaturePoints(HTuple modelId, HObject roiRegion, int featureScale)
        {
            if (_halconWindow == null) return;
            _halconWindow.ClearWindow();
            if (_currentImage != null && _currentImage.IsInitialized())
                _currentImage.DispObj(_halconWindow);
            // ROI 绿框
            if (roiRegion != null && roiRegion.IsInitialized() && roiRegion.CountObj() > 0)
            {
                _halconWindow.SetDraw("margin");
                _halconWindow.SetColor("green");
                _halconWindow.SetLineWidth(2);
                _halconWindow.DispObj(roiRegion);
            }

            if (!TryGetModelFeaturePoints(modelId, roiRegion, featureScale, out var rows, out var cols))
                return;
            HOperatorSet.GenCrossContourXld(out HObject cross, new HTuple(rows), new HTuple(cols), 4, new HTuple(Math.PI / 4));
            _halconWindow.SetColor("yellow");
            _halconWindow.SetLineWidth(1);
            _halconWindow.DispObj(cross);
            cross.Dispose();
        }

        public void SaveShapeModel(HTuple modelId, string filePath)
        {
            HOperatorSet.WriteShapeModel(modelId, filePath);
        }

        public HTuple LoadShapeModel(string filePath)
        {
            if (_modelId != null)
            {
                if (_modelId.Length != 0)
                {
                    HOperatorSet.ClearShapeModel(_modelId);
                    _modelId = null;
                }
            }
            HOperatorSet.ReadShapeModel(filePath, out _modelId);
            return _modelId;
        }

        public void DisplayResult(HTuple modelId, HTuple row, HTuple col, HTuple angle, HTuple score)
        {
            if (_halconWindow == null) return;
            if (modelId == null || row == null || row.Length == 0) return;

            for (int i = 0; i < row.Length; i++)
            {
                HTuple r = row[i];
                HTuple c = col[i];
                HTuple a = angle[i];
                HTuple s = score[i];

                HOperatorSet.VectorAngleToRigid(0, 0, 0, r, c, a, out HTuple homMat2D);
                HOperatorSet.GetShapeModelContours(out HObject contour, modelId, 1);
                HOperatorSet.AffineTransContourXld(contour, out HObject transContour, homMat2D);

                if (s.D > 0.7)
                    _halconWindow.SetColor("lime green");
                else if (s.D > 0.5)
                    _halconWindow.SetColor("yellow");
                else
                    _halconWindow.SetColor("red");

                _halconWindow.SetLineWidth(2);
                _halconWindow.DispObj(transContour);

                _halconWindow.SetColor("cyan");
                _halconWindow.DispCross(r, c, 20, a.D);

                transContour.Dispose();
                contour.Dispose();
            }
        }


        public bool TryGetModelFeaturePoints(HTuple modelId, HObject roiRegion, int featureScale, out double[] rows, out double[] cols)
        {
            rows = null;
            cols = null;

            if (modelId == null) return false;
            if (roiRegion == null || !roiRegion.IsInitialized() || roiRegion.CountObj() == 0) return false;

            if (featureScale < 1) featureScale = 1;
            if (featureScale > 5) featureScale = 5;

            HOperatorSet.GetShapeModelContours(out HObject modelContours, modelId, 1);
            HOperatorSet.AreaCenter(roiRegion, out _, out HTuple rowRef, out HTuple colRef);
            HOperatorSet.VectorAngleToRigid(0, 0, 0, rowRef, colRef, 0, out HTuple homMat);
            HOperatorSet.AffineTransContourXld(modelContours, out HObject contoursAtRoi, homMat);

            HOperatorSet.CountObj(contoursAtRoi, out HTuple n);
            var allRows = new List<double>();
            var allCols = new List<double>();

            for (int i = 1; i <= n.I; i++)
            {
                HOperatorSet.SelectObj(contoursAtRoi, out HObject one, i);
                HOperatorSet.GetContourXld(one, out HTuple r, out HTuple c);
                allRows.AddRange(r.ToDArr());
                allCols.AddRange(c.ToDArr());
                one.Dispose();
            }

            modelContours.Dispose();
            contoursAtRoi.Dispose();

            if (allRows.Count == 0) return false;

            var sr = new List<double>();
            var sc = new List<double>();
            for (int i = 0; i < allRows.Count; i += featureScale)
            {
                sr.Add(allRows[i]);
                sc.Add(allCols[i]);
            }

            rows = sr.ToArray();
            cols = sc.ToArray();
            return rows.Length > 0;
        }

        public void Dispose()
        {
            _currentImage?.Dispose();
            if (_modelId != null) HOperatorSet.ClearShapeModel(_modelId);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
namespace AVS_Service
{

    /// <summary>
    /// Halcon 2D 计量模型服务接口
    /// 封装 create_metrology_model、add_metrology_object_*、apply_metrology_model 等核心操作
    /// </summary>
    public interface IMetrologyService
    {
        /// <summary>设置当前测量图像</summary>
        void SetImage(HObject image);

        /// <summary>创建空的计量模型句柄</summary>
        HTuple CreateMetrologyModel();

        /// <summary>向模型中添加旋转矩形测量对象</summary>
        /// <param name="modelHandle">计量模型句柄</param>
        /// <param name="row">矩形中心行坐标</param>
        /// <param name="col">矩形中心列坐标</param>
        /// <param name="phi">矩形旋转角度（弧度）</param>
        /// <param name="length1">矩形半长（主轴）</param>
        /// <param name="length2">矩形半宽（次轴）</param>
        /// <param name="measureLength1">测量区域半长（沿边缘方向）</param>
        /// <param name="measureLength2">测量区域半宽（垂直于边缘）</param>
        /// <param name="sigma">平滑系数</param>
        /// <param name="threshold">边缘幅度阈值</param>
        /// <param name="minScore">最小边缘分数</param>
        /// <param name="numInstances">期望找到的实例个数</param>
        /// <param name="measureDistance">测量线最小间距</param>
        /// <param name="transition">过渡极性："all","positive","negative","uniform"</param>
        /// <param name="select">边缘选择："all","first","last"</param>
        /// <param name="interpolation">插值方式："nearest_neighbor","bilinear","bicubic"</param>
        void AddMetrologyObjectRectangle2(
            HTuple modelHandle,
            double row, double col, double phi,
            double length1, double length2,
            double measureLength1, double measureLength2,
            double sigma, double threshold,
            double minScore, int numInstances, double measureDistance,
            string transition, string select, string interpolation);

        // ---------- 圆 ----------
        void AddMetrologyObjectCircle(
            HTuple modelHandle,
            double row, double col, double radius,
            double measureLength1, double measureLength2,
            double sigma, double threshold,
            double minScore, int numInstances, double measureDistance,
            string transition, string select, string interpolation);

        // ---------- 椭圆 ----------
        void AddMetrologyObjectEllipse(
            HTuple modelHandle,
            double row, double col, double phi,
            double radius1, double radius2,
            double measureLength1, double measureLength2,
            double sigma, double threshold,
            double minScore, int numInstances, double measureDistance,
            string transition, string select, string interpolation);

        // ---------- 直线 ----------
        void AddMetrologyObjectLine(
            HTuple modelHandle,
            double rowBegin, double colBegin,
            double rowEnd, double colEnd,
            double measureLength1, double measureLength2,
            double sigma, double threshold,
            double minScore, int numInstances, double measureDistance,
            string transition, string select, string interpolation);

        /// <summary>执行计量测量，返回测量线轮廓和结果边缘轮廓</summary>
        void ApplyMetrologyModel(HTuple modelHandle, out HObject measureContours, out HObject resultContours);

        /// <summary>获取测量对象的结果参数（all_param）</summary>
        void GetMetrologyObjectResult(HTuple modelHandle, out HTuple resultParams);

        /// <summary>保存计量模型到文件（.mtr）</summary>
        void SaveMetrologyModel(HTuple modelHandle, string filePath);

        /// <summary>从文件读取计量模型</summary>
        HTuple ReadMetrologyModel(string filePath);

        /// <summary>销毁计量模型句柄</summary>
        void ClearMetrologyModel(HTuple modelHandle);

        /// <summary>设置计量模型的参考坐标系</summary>
        void SetReferenceSystem(HTuple modelHandle, double row, double col, double angle);

        /// <summary>将计量模型对齐到指定位置和角度</summary>
        void AlignMetrologyModel(HTuple modelHandle, double row, double col, double angle);
    }

    public class MetrologyService : IMetrologyService, IDisposable
    {
        private HObject _currentImage;

        public void SetImage(HObject image)
        {
            _currentImage = image;
        }

        public HTuple CreateMetrologyModel()
        {
            HTuple modelHandle = new HTuple();
            HOperatorSet.CreateMetrologyModel(out modelHandle);
            return modelHandle;
        }

        public void AddMetrologyObjectRectangle2(
            HTuple modelHandle,
            double row, double col, double phi,
            double length1, double length2,
            double measureLength1, double measureLength2,
            double sigma, double threshold,
            double minScore, int numInstances, double measureDistance,
            string transition, string select, string interpolation)
        {
            HTuple param = new HTuple(row, col, phi, length1, length2);
            AddObject(modelHandle, "rectangle2", param, measureLength1, measureLength2, sigma, threshold,
                      minScore, numInstances, measureDistance, transition, select, interpolation);
        }

        // ===== 圆 =====
        public void AddMetrologyObjectCircle(
            HTuple modelHandle,
            double row, double col, double radius,
            double measureLength1, double measureLength2,
            double sigma, double threshold,
            double minScore, int numInstances, double measureDistance,
            string transition, string select, string interpolation)
        {
            HTuple param = new HTuple(row, col, radius);
            AddObject(modelHandle, "circle", param, measureLength1, measureLength2, sigma, threshold,
                      minScore, numInstances, measureDistance, transition, select, interpolation);
        }

        // ===== 椭圆 =====
        public void AddMetrologyObjectEllipse(
            HTuple modelHandle,
            double row, double col, double phi,
            double radius1, double radius2,
            double measureLength1, double measureLength2,
            double sigma, double threshold,
            double minScore, int numInstances, double measureDistance,
            string transition, string select, string interpolation)
        {
            HTuple param = new HTuple(row, col, phi, radius1, radius2);
            AddObject(modelHandle, "ellipse", param, measureLength1, measureLength2, sigma, threshold,
                      minScore, numInstances, measureDistance, transition, select, interpolation);
        }

        // ===== 直线 =====
        public void AddMetrologyObjectLine(
            HTuple modelHandle,
            double rowBegin, double colBegin,
            double rowEnd, double colEnd,
            double measureLength1, double measureLength2,
            double sigma, double threshold,
            double minScore, int numInstances, double measureDistance,
            string transition, string select, string interpolation)
        {
            HTuple param = new HTuple(rowBegin, colBegin, rowEnd, colEnd);
            AddObject(modelHandle, "line", param, measureLength1, measureLength2, sigma, threshold,
                      minScore, numInstances, measureDistance, transition, select, interpolation);
        }

        /// <summary>通用添加测量对象方法</summary>
        private void AddObject(
            HTuple modelHandle,
            string type,
            HTuple geomParam,
            double measureLength1, double measureLength2,
            double sigma, double threshold,
            double minScore, int numInstances, double measureDistance,
            string transition, string select, string interpolation)
        {
            HTuple paramName = new HTuple(
                "min_score", "num_instances", "measure_distance",
                "measure_transition", "measure_select", "measure_interpolation");
            HTuple paramValue = new HTuple(minScore, numInstances, measureDistance,
                                           transition, select, interpolation);

            HOperatorSet.AddMetrologyObjectGeneric(
                modelHandle, type, geomParam,
                measureLength1, measureLength2,
                sigma, threshold,
                paramName, paramValue,
                out HTuple _);
        }

        public void ApplyMetrologyModel(HTuple modelHandle, out HObject measureContours, out HObject resultContours)
        {
            measureContours = new HObject();
            resultContours = new HObject();

            if (_currentImage == null || modelHandle == null)
                return;

            try
            {
                // 应用模型到当前图像
                HOperatorSet.ApplyMetrologyModel(_currentImage, modelHandle);

                // 获取测量线（卡尺矩形）
                HOperatorSet.GetMetrologyObjectMeasures(out measureContours, modelHandle, "all", "all", out HTuple _, out HTuple _);

                // 获取结果边缘轮廓
                HOperatorSet.GetMetrologyObjectResultContour(out resultContours, modelHandle, "all", "all", 1.5);
            }
            catch (HalconException ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyMetrologyModel error: {ex.Message}");
            }
        }

        public void GetMetrologyObjectResult(HTuple modelHandle, out HTuple resultParams)
        {
            HOperatorSet.GetMetrologyObjectResult(
                modelHandle, "all", "all", "result_type", "all_param", out resultParams);
        }

        public void SaveMetrologyModel(HTuple modelHandle, string filePath)
        {
            HOperatorSet.WriteMetrologyModel(modelHandle, filePath);
        }

        public HTuple ReadMetrologyModel(string filePath)
        {
            HTuple modelHandle = new HTuple();
            HOperatorSet.ReadMetrologyModel(filePath, out modelHandle);
            return modelHandle;
        }

        public void ClearMetrologyModel(HTuple modelHandle)
        {
            if (modelHandle != null && modelHandle.Length > 0)
            {
                HOperatorSet.ClearMetrologyModel(modelHandle);
            }
        }

        public void SetReferenceSystem(HTuple modelHandle, double row, double col, double angle)
        {
            HTuple refSystem = new HTuple(new double[] { row, col, angle });
            HOperatorSet.SetMetrologyModelParam(modelHandle, "reference_system", refSystem);
        }

        public void AlignMetrologyModel(HTuple modelHandle, double row, double col, double angle)
        {
            HOperatorSet.AlignMetrologyModel(modelHandle, row, col, angle);
        }

        public void Dispose()
        {
            _currentImage?.Dispose();
        }
    }
}



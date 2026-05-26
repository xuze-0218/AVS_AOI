using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service
{
    public interface ITemplateMatchingService
    {
        void SetHalconWindow(HWindow window);
        void LoadImage(string filePath);
        HObject GetCurrentImage();
        HTuple CreateShapeModel(HObject region, double minScore = 0.7);
        void FindShapeModel(HTuple modelId, out HTuple row, out HTuple col, out HTuple angle, out HTuple score);
        void DisplayResult(HTuple row, HTuple col, HTuple angle, HTuple score);
    }

    public class TemplateMatchingService : ITemplateMatchingService, IDisposable
    {
        private HWindow _halconWindow;
        private HObject _currentImage;
        private HTuple _modelId = null;

        public void SetHalconWindow(HWindow window) => _halconWindow = window;

        public void LoadImage(string filePath)
        {
            _currentImage?.Dispose();
            HOperatorSet.ReadImage(out _currentImage, filePath);
        }

        public HObject GetCurrentImage() => _currentImage;

        public HTuple CreateShapeModel(HObject region, double minScore = 0.7)
        {
            if (_currentImage == null) throw new InvalidOperationException("No image loaded.");
            if (_modelId != null) HOperatorSet.ClearShapeModel(_modelId);
            HOperatorSet.ReduceDomain(_currentImage, region, out HObject imageReduced);
            HOperatorSet.CreateShapeModel(imageReduced, "auto", -0.39, 0.79, "auto", "auto",
                "use_polarity", "auto", "auto", out _modelId);
            imageReduced.Dispose();
            return _modelId;
        }

        public void FindShapeModel(HTuple modelId, out HTuple row, out HTuple col, out HTuple angle, out HTuple score)
        {
            row = col = angle = score = 0;
            HOperatorSet.FindShapeModel(_currentImage, modelId, -0.39, 0.79, 0.5, 1, 0.5,
                "least_squares", 0, 0.9, out row, out col, out angle, out score);
        }

        public void DisplayResult(HTuple row, HTuple col, HTuple angle, HTuple score)
        {
            if (_modelId == null) return;
            HTuple homMat2D;
            HOperatorSet.VectorAngleToRigid(0, 0, 0, row, col, angle, out homMat2D);
            HOperatorSet.GetShapeModelContours(out HObject contour, _modelId, 1);
            HOperatorSet.AffineTransContourXld(contour, out HObject transContour, homMat2D);
            _halconWindow.SetColor("cyan");
            _halconWindow.SetLineWidth(2);
            _halconWindow.DispObj(transContour);
        }

        public void Dispose()
        {
            _currentImage?.Dispose();
            if (_modelId != null) HOperatorSet.ClearShapeModel(_modelId);
        }
    }
}
using AVS_Service.Models;
using HalconDotNet;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Service.Services
{
    /// <summary>
    /// 检测图像保存服务，根据配置自动保存 2D/3D 的原始图、结果图、Mask 图等，
    /// 并将保存路径回填到对应的数据对象中，供 CSV 记录使用。
    /// </summary>
    public interface IImageSaveService
    {
        /// <summary>
        /// 保存2D检测相关图像。
        /// </summary>
        /// <param name="data">2D检测结果数据（包含极柱号、结果、模块名等），保存路径会回填到其 Orn2DPath / Dump2DPath 属性。</param>
        /// <param name="originalImage">2D原始图像（相机采集图）。若为null或未初始化，且配置需要保存原图，则跳过。</param>
        /// <param name="resultImage">2D结果渲染图（Halcon窗口截图）。若为null或未初始化，且配置需要保存结果图，则跳过。</param>
        /// <param name="mask1">分割Mask1（可选）。</param>
        /// <param name="mask2">分割Mask2（可选）。</param>
        /// <param name="mask3">分割Mask3（可选）。</param>
        void Save2DImages(InspectResult2DData data, HObject originalImage, HObject resultImage = null,
            HObject mask1 = null, HObject mask2 = null, HObject mask3 = null);

        /// <summary>
        /// 保存3D检测相关图像。
        /// </summary>
        /// <param name="data">3D检测结果数据（包含极柱号、结果、模块名等），保存路径会回填到其 Orn3DPath / Dump3DPath 属性。</param>
        /// <param name="depthImage">3D深度图（原始图像）。若为null或未初始化，且配置需要保存深度图，则跳过。</param>
        /// <param name="intensityImage">3D亮度图/灰度图（可选）。</param>
        /// <param name="resultImage">3D结果渲染图（Halcon窗口截图）。</param>
        /// <param name="mask1">分割Mask1（可选）。</param>
        /// <param name="mask2">分割Mask2（可选）。</param>
        /// <param name="mask3">分割Mask3（可选）。</param>
        void Save3DImages(InspectResult3DData data, HObject depthImage, HObject intensityImage = null,
                          HObject resultImage = null, HObject mask1 = null,
                          HObject mask2 = null, HObject mask3 = null);

        void Save3DIntensityImage(HObject intensityImage, int poleNum, string moduleName);
    }

    public class ImageSaveService : IImageSaveService
    {
        private readonly IParametersConfigService _paramService;
        private readonly ILogger _logger;

        public ImageSaveService(IParametersConfigService paramService, ILogger logger)
        {
            _paramService = paramService;
            _logger = logger;
        }

        public void Save2DImages(InspectResult2DData data, HObject originalImage, HObject resultImage = null,
                                 HObject mask1 = null, HObject mask2 = null, HObject mask3 = null)

        {
            if (data == null) return;

            bool onlyNG = _paramService.GetBool("Global", "IsSave2DNGOnly", false);
            if (onlyNG && data.Result2D != Result.NG) return;

            // 获取缺陷类型列表，OK 时只有一个空字符串，表示无子目录
            List<string> defectList = GetDefectList(data);
            if (defectList.Count == 0) defectList.Add(""); // OK

            foreach (string defect in defectList)
            {
                // 原始图
                if (_paramService.GetBool("Global", "IsSave2DOriginal", true) && IsValidImage(originalImage))
                {
                    string format = _paramService.GetString("Global", "Format2DOriginal", "bmp");
                    string subDir = "Originallmage";
                    string suffix = "_O";
                    string path = SaveImageAsync(data, originalImage, "2D", subDir, suffix, format, defect);
                    data.Orn2DPath = path;
                }

                // 结果渲染图
                if (_paramService.GetBool("Global", "IsSave2DResult", true) && IsValidImage(resultImage))
                {
                    string format = _paramService.GetString("Global", "Format2DResult", "bmp");
                    string subDir = "ResultImage\\Dumplmage";
                    string suffix = "_R";
                    string path = SaveImageAsync(data, resultImage, "2D", subDir, suffix, format, defect);
                    data.Dump2DPath = path;
                }

                // Mask 图
                if (_paramService.GetBool("Global", "IsSave2DMask", true))
                {
                    SaveMaskImage(data, mask1, "2D", "_Mask01", defect);
                    SaveMaskImage(data, mask2, "2D", "_Mask02", defect);
                    SaveMaskImage(data, mask3, "2D", "_Mask03", defect);
                }
            }
        }

        public void Save3DImages(InspectResult3DData data, HObject depthImage, HObject intensityImage = null,
                                 HObject resultImage = null, HObject mask1 = null, HObject mask2 = null, HObject mask3 = null)
        {
            if (data == null) return;

            bool onlyNG = _paramService.GetBool("Global", "IsSave3DNGOnly", false);
            if (onlyNG && data.Result3D != Result.NG) return;

            List<string> defectList = GetDefectList(data);
            if (defectList.Count == 0) defectList.Add("");

            foreach (string defect in defectList)
            {
                // 深度图
                if (_paramService.GetBool("Global", "IsSave3DDepth", true) && IsValidImage(depthImage))
                {
                    string format = _paramService.GetString("Global", "Format3DDepth", "tiff");
                    string subDir = "Originallmage";
                    string suffix = "_O";
                    string path = SaveImageAsync(data, depthImage, "3D", subDir, suffix, format, defect);
                    data.Orn3DPath = path;
                }

                // 亮度图
                if (_paramService.GetBool("Global", "IsSave3DIntensity", true) && IsValidImage(intensityImage))
                {
                    string format = _paramService.GetString("Global", "Format3DIntensity", "bmp");
                    string subDir = "OriginallmageGray";
                    string suffix = "_O";
                    SaveImageAsync(data, intensityImage, "3D", subDir, suffix, format, defect);
                }

                // 结果图
                if (_paramService.GetBool("Global", "IsSave3DResult", true) && IsValidImage(resultImage))
                {
                    string format = _paramService.GetString("Global", "Format3DResult", "bmp");
                    string subDir = "ResultImage\\Dumplmage";
                    string suffix = "_R";
                    string path = SaveImageAsync(data, resultImage, "3D", subDir, suffix, format, defect);
                    data.Dump3DPath = path;
                }

                // Mask 图
                if (_paramService.GetBool("Global", "IsSave3DMask", false))
                {
                    SaveMaskImage(data, mask1, "3D", "_Mask01", defect);
                    SaveMaskImage(data, mask2, "3D", "_Mask02", defect);
                    SaveMaskImage(data, mask3, "3D", "_Mask03", defect);
                }
            }
        }

        public void Save3DIntensityImage(HObject intensityImage, int poleNum, string moduleName)
        {
            if (!IsValidImage(intensityImage)) return;
            if (!_paramService.GetBool("Global", "IsSave3DIntensity", true)) return;

            string baseDir = GetImageSaveDir();
            string dateStr = DateTime.Now.ToString("yyyy_MM_dd");
            string timeStr = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            string saveDir = Path.Combine(baseDir, dateStr, "OK", moduleName, "3D", "OriginallmageGray");
            Directory.CreateDirectory(saveDir);
            string fileName = $"{timeStr}_{moduleName}_Pole_{poleNum:D2}_O.png";
            string fullPath = Path.Combine(saveDir, fileName);

            HObject clone = intensityImage.Clone();
            Task.Run(() =>
            {
                try { HOperatorSet.WriteImage(clone, "png", 0, fullPath); }
                catch (Exception ex) { _logger.Error(ex, "3D亮度图保存失败"); }
                finally { clone.Dispose(); }
            });
        }
        // ===== 辅助方法 =====

        private bool IsValidImage(HObject image)
        {
            return image != null && image.IsInitialized() && !IsEmpty(image);
        }

        private bool IsEmpty(HObject image)
        {
            HObject emptyObj;
            HOperatorSet.GenEmptyObj(out emptyObj);
            try
            {
                HOperatorSet.TestEqualObj(image, emptyObj, out HTuple isEqual);
                return isEqual.I == 1;
            }
            finally
            {
                emptyObj.Dispose();
            }
        }

        private string SaveImageAsync(InspectResult2DData data, HObject image, string dimension,
                                      string subDir, string suffix, string format, string defectFolder)
        {
            string fullPath = BuildFilePath(data, dimension, subDir, suffix, format, defectFolder);
            HObject clone = image.Clone();
            Task.Run(() =>
            {
                try
                {
                    HOperatorSet.WriteImage(clone, format, 0, fullPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "2D图像保存失败: {Path}", fullPath);
                }
                finally
                {
                    clone.Dispose();
                }
            });
            return fullPath;
        }

        private string SaveImageAsync(InspectResult3DData data, HObject image, string dimension,
                                      string subDir, string suffix, string format, string defectFolder)
        {
            string fullPath = BuildFilePath(data, dimension, subDir, suffix, format, defectFolder);
            HObject clone = image.Clone();
            Task.Run(() =>
            {
                try
                {
                    HOperatorSet.WriteImage(clone, format, 0, fullPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "3D图像保存失败: {Path}", fullPath);
                }
                finally
                {
                    clone.Dispose();
                }
            });
            return fullPath;
        }

        private void SaveMaskImage(InspectResult2DData data, HObject mask, string dimension, string suffix, string defectFolder)
        {
            if (IsValidImage(mask))
            {
                string format = "jpg 100";
                string subDir = "ResultImage\\Masklmage";
                SaveImageAsync(data, mask, dimension, subDir, suffix, format, defectFolder);
            }
        }

        private void SaveMaskImage(InspectResult3DData data, HObject mask, string dimension, string suffix, string defectFolder)
        {
            if (IsValidImage(mask))
            {
                string format = "jpg 100";
                string subDir = "ResultImage\\Masklmage";
                SaveImageAsync(data, mask, dimension, subDir, suffix, format, defectFolder);
            }
        }

        private string BuildFilePath(InspectResult2DData data, string dimension, string subDir,
                                     string suffix, string format, string defectFolder)
        {
            string baseDir = GetImageSaveDir();
            string dateStr = DateTime.Now.ToString("yyyy_MM_dd");
            string timeStr = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            string type = data.Result2D == Result.OK ? "OK" : "NG";
            string module = string.IsNullOrEmpty(data.ModuleName) ? "Unknown" : data.ModuleName;

            string saveRoot = Path.Combine(baseDir, dateStr, type, module, dimension);
            if (!string.IsNullOrEmpty(defectFolder) && type == "NG")
                saveRoot = Path.Combine(saveRoot, defectFolder);

            string fullDir = Path.Combine(saveRoot, subDir);
            Directory.CreateDirectory(fullDir);

            string fileName = $"{timeStr}_{module}_Pole_{data.PoleNum:D2}{suffix}{GetExtension(format)}";
            return Path.Combine(fullDir, fileName);
        }

        private string BuildFilePath(InspectResult3DData data, string dimension, string subDir,
                                     string suffix, string format, string defectFolder)
        {
            string baseDir = GetImageSaveDir();
            string dateStr = DateTime.Now.ToString("yyyy_MM_dd");
            string timeStr = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
            string type = data.Result3D == Result.OK ? "OK" : "NG";
            string module = string.IsNullOrEmpty(data.ModuleName) ? "Unknown" : data.ModuleName;

            string saveRoot = Path.Combine(baseDir, dateStr, type, module, dimension);
            if (!string.IsNullOrEmpty(defectFolder) && type == "NG")
                saveRoot = Path.Combine(saveRoot, defectFolder);

            string fullDir = Path.Combine(saveRoot, subDir);
            Directory.CreateDirectory(fullDir);

            string fileName = $"{timeStr}_{module}_Pole_{data.PoleNum:D2}{suffix}{GetExtension(format)}";
            return Path.Combine(fullDir, fileName);
        }

        private string GetImageSaveDir()
        {
            string dir = _paramService.GetString("Global", "ImageSaveDir", "");
            if (string.IsNullOrWhiteSpace(dir))
                dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            return dir;
        }

        /// <summary>
        /// 获取2D缺陷类型列表（NG时每个缺陷单独一项；OK或None时返回空列表或一项）
        /// </summary>
        private List<string> GetDefectList(InspectResult2DData data)
        {
            var defects = new List<string>();
            if (data.Result2D == Result.OK)
            {
                // OK 不添加缺陷，由调用方添加空字符串
            }
            else if (data.Result2D == Result.None)
            {
                defects.Add("DetectFail");
            }
            else // NG
            {
                if (data.ResultLength == Result.NG) defects.Add("Length");
                if (data.ResultWidth == Result.NG) defects.Add(data.IsSquareBar ? "SquareWidth" : "Width");
                if (data.ResultOffset == Result.NG) defects.Add(data.IsSquareBar ? "BarWidth" : "Offset");
                if (data.ResultPoreBreak == Result.NG) defects.Add(data.IsSquareBar ? "WeldGap" : "PoreBreak");
                if (data.ResultBeadDiameter == Result.NG) defects.Add(data.IsSquareBar ? "PoreBreak" : "BeadDiameter");
                if (data.ResultfaultySol == Result.NG) defects.Add("faultySol");
                if (defects.Count == 0) defects.Add("MeasureFailed");
            }
            return defects;
        }

        private List<string> GetDefectList(InspectResult3DData data)
        {
            var defects = new List<string>();
            if (data.Result3D == Result.OK)
            {
                // OK 不添加
            }
            else if (data.Result3D == Result.None)
            {
                defects.Add("DetectFail");
            }
            else // NG
            {
                bool isSquareBar = IsSquareBarProduct();
                if (data.ResultBeadHump == Result.NG) defects.Add(isSquareBar ? "SquareBeadHump" : "BeadHump");
                if (data.ResultBeadSag == Result.NG) defects.Add(isSquareBar ? "SquareBeadSag" : "BeadSag");
                if (data.ResultBarBeadHump == Result.NG) defects.Add("BarBeadHump");
                if (data.ResultBarBeadSag == Result.NG) defects.Add("BarBeadSag");
                if (defects.Count == 0) defects.Add("MeasureFailed");
            }
            return defects;
        }

        private bool IsSquareBarProduct()
        {
            return _paramService.GetBool("Global", "IsSquareBarWeldMark", false);
        }

        private string GetExtension(string format)
        {
            if (format.Contains("bmp")) return ".bmp";
            if (format.Contains("jpeg") || format.Contains("jpg")) return ".jpg";
            if (format.Contains("tiff") || format.Contains("tif")) return ".tiff";
            if (format.Contains("png")) return ".png";
            return "." + format;
        }
    }
}


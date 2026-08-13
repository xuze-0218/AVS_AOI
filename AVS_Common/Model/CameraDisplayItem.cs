using HalconDotNet;
using Prism.Mvvm;
using System.Windows.Input;

namespace AVS_Common.Model
{
    public class CameraDisplayItem : BindableBase
    {
        public string? CameraRoleName { get; set; }
        public string? PhysicalSN { get; set; }

        public HObject CurrentImage
        {
            get
            {
                if (ShowProcessed && ProcessedImage != null && ProcessedImage.IsInitialized())
                    return ProcessedImage;
                return RawImage;
            }
        }

        private HObject _rawImage;
        /// <summary>
        /// 原始图像
        /// </summary>
        public HObject RawImage
        {
            get => _rawImage;
            set
            {
                // 释放旧图，避免内存泄漏
                if (_rawImage != null && _rawImage.IsInitialized())
                    _rawImage.Dispose();
                if (SetProperty(ref _rawImage, value))
                    RaisePropertyChanged(nameof(CurrentImage));
            }
        }

        private HObject _processedImage;
        /// <summary>
        /// 处理后的图像
        /// </summary>
        public HObject ProcessedImage
        {
            get => _processedImage;
            set
            {
                if (_processedImage != null && _processedImage.IsInitialized())
                    _processedImage.Dispose();
                if (SetProperty(ref _processedImage, value))
                    RaisePropertyChanged(nameof(CurrentImage));
            }
        }


        private bool _showProcessed = true; // 默认显示处理后图像
        public bool ShowProcessed
        {
            get => _showProcessed;
            set
            {
                if (SetProperty(ref _showProcessed, value))
                    RaisePropertyChanged(nameof(CurrentImage));
            }
        }
        /// <summary>
        /// 检测测试命令
        /// </summary>
        public ICommand InspectTestCommand { get; set; }
        /// <summary>
        /// 标定测试命令
        /// </summary>
        public ICommand CalibTestCommand { get; set; }
    }
}

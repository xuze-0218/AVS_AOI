using HalconDotNet;
using Prism.Mvvm;
using System;
using System.Windows.Input;

namespace AVS_Common.Model
{
    public class CameraDisplayItem : BindableBase, IDisposable
    {
        public string CameraRoleName { get; set; }
        public string PhysicalSN { get; set; }

        public HObject CurrentImage
        {
            get
            {
                if (ShowProcessed &&
                    _processedImage != null &&
                    _processedImage.IsInitialized())
                {
                    return _processedImage;
                }

                return _rawImage;
            }
        }

        /// <summary>
        /// 原始图像
        /// </summary>
        private HObject _rawImage;
        public HObject RawImage
        {
            get => _rawImage;
            set
            {
                if (ReferenceEquals(_rawImage, value))
                    return;

                var oldImage = _rawImage;

                if (SetProperty(ref _rawImage, value))
                {
                    oldImage?.Dispose();
                    RaisePropertyChanged(nameof(CurrentImage));
                }
            }
        }

        /// <summary>
        /// 处理后的图像
        /// </summary>
        private HObject _processedImage;       
        public HObject ProcessedImage
        {
            get => _processedImage;
            set
            {
                if (ReferenceEquals(_processedImage, value))
                    return;

                var oldImage = _processedImage;

                if (SetProperty(ref _processedImage, value))
                {
                    oldImage?.Dispose();
                    RaisePropertyChanged(nameof(CurrentImage));
                }
            }
        }

        private bool _showProcessed = true;
        public bool ShowProcessed
        {
            get => _showProcessed;
            set
            {
                if (SetProperty(ref _showProcessed, value))
                {
                    RaisePropertyChanged(nameof(CurrentImage));
                }
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

        public void Dispose()
        {
            RawImage?.Dispose();
            ProcessedImage?.Dispose();
        }
    }
}

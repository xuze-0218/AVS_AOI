using HalconDotNet;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Common.Model
{
    public class CameraDisplayItem : BindableBase
    {
        public string CameraRoleName { get; set; }
        public string PhysicalSN { get; set; }

        private HObject _currentImage;
        public HObject CurrentImage
        {
            get => _currentImage;
            set
            {
                if (_currentImage != null)
                    _currentImage.Dispose();
                HObject newImage = value?.Clone();
                SetProperty(ref _currentImage, newImage);
            }
        }
    }
}

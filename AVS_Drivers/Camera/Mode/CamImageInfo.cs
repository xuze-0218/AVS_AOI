using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Drivers.Camera.Mode
{
    public class CamImageInfo
    {
        public IntPtr ImageData { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride { get; set; }
        public uint PixelFormat { get; set; } 
        public bool IsMono { get; set; }
        public bool IsColor { get; set; }       
      
    }
}

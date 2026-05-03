
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Lmi3d.GoSdk;
using Lmi3d.Zen;
using HalconDotNet;
using Lmi3d.GoSdk.Messages;
using Lmi3d.Zen.Io;


namespace AVS
{
    public class LMIFactory
    {
        public GoSystem system;
        [DllImport(@"GoSdk.dll")]
        public static extern Int32 GoDestroy(IntPtr obj);
        public LMIFactory()
        {
            KApiLib.Construct();
            GoSdkLib.Construct();
            system = new GoSystem();
        }
    }
}

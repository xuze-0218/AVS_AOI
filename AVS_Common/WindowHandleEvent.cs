using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Common
{
    /// <summary>
    /// CameraDisplayUnit无法通过有参构造函数注册服务，因为它是通过XAML实例化的。
    /// 只能通过事件的方式将窗口句柄传递给服务层，WindowHandleEvent提供了一个全局事件机制，
    /// 允许CameraDisplayUnit在加载和卸载时通知服务层注册和注销窗口句柄。
    /// </summary>
    public static class WindowHandleEvent
    {
        /// <summary>
        /// 当控件加载完成，窗口句柄可用时触发
        /// </summary>
        public static event Action<string, HWindow> HandleRegistered;

        /// <summary>
        /// 当控件卸载时触发
        /// </summary>
        public static event Action<string> HandleUnregistered;

        internal static void RaiseHandleRegistered(string cameraSN, HWindow handle)
        {
            HandleRegistered?.Invoke(cameraSN, handle);
        }

        internal static void RaiseHandleUnregistered(string cameraSN)
        {
            HandleUnregistered?.Invoke(cameraSN);
        }
    }
}

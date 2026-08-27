using HalconDotNet;
using System;

namespace AVS_Common.Events
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

        public static void RaiseHandleRegistered(string CameraRoleName, HWindow handle)
        {
            HandleRegistered?.Invoke(CameraRoleName, handle);
        }

        public static void RaiseHandleUnregistered(string CameraRoleName)
        {
            HandleUnregistered?.Invoke(CameraRoleName);
        }
    }
}

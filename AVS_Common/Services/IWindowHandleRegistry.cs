using HalconDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Common.Services
{
    /// <summary>
    /// 历史遗留问题，调用Halcon程序必须传入一个窗口句柄，这样会导致服务层必须依赖于UI组件，违反了MVVM的分层原则。
    /// 这个接口的存在是为了暂时解决这个问题，允许服务层通过接口获取窗口句柄，而不直接引用UI组件。
    /// </summary>
    public interface IWindowHandleRegistry
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraRN">相机角色名RoleName</param>
        /// <param name="windowHandle"></param>
        void Register(string cameraRN, HWindow windowHandle);
        void Unregister(string cameraRN);
        HWindow GetHandle(string cameraRN);
    }

    public class WindowHandleRegistry : IWindowHandleRegistry
    {
        /// <summary>
        /// 存放相机SN与窗口句柄的映射关系，使用ConcurrentDictionary保证线程安全
        /// </summary>
        private readonly ConcurrentDictionary<string, HWindow> _handles = new();
        public HWindow GetHandle(string cameraRN)
        {
            return _handles.TryGetValue(cameraRN, out var handle) ? handle : null;
        }

        public void Register(string cameraRN, HWindow windowHandle)
        {
            _handles[cameraRN] = windowHandle;
        }

        public void Unregister(string cameraRN)
        {
            _handles.TryRemove(cameraRN, out _);
        }
    }
}

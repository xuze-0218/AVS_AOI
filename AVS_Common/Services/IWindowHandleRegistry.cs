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
        void Register(string cameraSN, HWindow windowHandle);
        void Unregister(string cameraSN);
        HWindow GetHandle(string cameraSN);
    }

    public class WindowHandleRegistry : IWindowHandleRegistry
    {
        /// <summary>
        /// 存放相机SN与窗口句柄的映射关系，使用ConcurrentDictionary保证线程安全
        /// </summary>
        private readonly ConcurrentDictionary<string, HWindow> _handles = new();
        public HWindow GetHandle(string cameraSN)
        {
            return _handles.TryGetValue(cameraSN, out var handle) ? handle : null;
        }

        public void Register(string cameraSN, HWindow windowHandle)
        {
            _handles[cameraSN] = windowHandle;
        }

        public void Unregister(string cameraSN)
        {
            _handles.TryRemove(cameraSN, out _);
        }
    }
}

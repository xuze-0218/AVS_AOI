using HalconDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AVS_Common.Services
{
    public interface IWindowHandleManager
    {
        void Register(string roleName, HWindow handle);
        void Unregister(string roleName);
        HWindow GetHandle(string roleName);
        Task<HWindow> WaitForHandleAsync(string roleName, CancellationToken ct = default);
    }
    public class WindowHandleManager : IWindowHandleManager
    {
        private readonly ConcurrentDictionary<string, HWindow> _handles = new ConcurrentDictionary<string, HWindow>();
        private readonly ConcurrentDictionary<string, TaskCompletionSource<HWindow>> _pending = new ConcurrentDictionary<string, TaskCompletionSource<HWindow>> ();

        public void Register(string roleName, HWindow handle)
        {
            if (string.IsNullOrEmpty(roleName) || handle == null) return;

            _handles[roleName] = handle;
            if (_pending.TryRemove(roleName, out var tcs))
            {
                tcs.TrySetResult(handle);
            }
        }

        public void Unregister(string roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return;

            _handles.TryRemove(roleName, out _);
            if (_pending.TryRemove(roleName, out var tcs))
            {
                tcs.TrySetCanceled();
            }
        }

        public HWindow GetHandle(string roleName)
        {
            return _handles.TryGetValue(roleName, out var handle) ? handle : null;
        }

        public async Task<HWindow> WaitForHandleAsync(string roleName, CancellationToken ct = default)
        {
            var existing = GetHandle(roleName);
            if (existing != null) return existing;

            var tcs = new TaskCompletionSource<HWindow>();
            _pending[roleName] = tcs;
            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                return await tcs.Task.ConfigureAwait(false);
            }
        }
    }

}

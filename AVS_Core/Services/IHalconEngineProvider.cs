using HalconDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Services
{
    /// <summary>
    /// 共享Halcon引擎的接口，提供给需要使用Halcon引擎的服务实现类使用
    /// </summary>
    public interface IHalconEngineProvider
    {
        HDevEngine GetEngine();
    }


    public class HalconEngineProvider : IHalconEngineProvider
    {
        private readonly object _lock = new object();
        private HDevEngine _engine;
        private bool _initialized;

        public HDevEngine GetEngine()
        {
            if (_initialized) return _engine;
            lock (_lock)
            {
                if (_initialized) return _engine;
                _engine = new HDevEngine();
                _engine.SetProcedurePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "HalconEngine.hdpl"));
                _engine.StartDebugServer();
                _initialized = true;
            }
            return _engine;
        }
    }
}

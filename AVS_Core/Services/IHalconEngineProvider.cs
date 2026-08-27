using AVS_Service;
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


    public class HalconEngineProvider : IHalconEngineProvider, IDisposable
    {
        //private readonly object _lock = new object();
        //private HDevEngine _engine;
        //private bool _initialized;
        private readonly Lazy<HDevEngine> _engine = new Lazy<HDevEngine>(() =>
        {
            var engine = new HDevEngine();
            string procFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            engine.SetProcedurePath(procFolder);
            //engine.StartDebugServer();//仅debug模式下使用，可以调试halcon程序
            return engine;
        });

        public HDevEngine GetEngine() => _engine.Value;

        public void Dispose()
        {
            if (_engine.IsValueCreated)
            {
                _engine.Value.Dispose();
            }
        }
    }
}

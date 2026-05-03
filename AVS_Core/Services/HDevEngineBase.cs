using AVS_Core.Models;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Services
{
    public abstract class HDevEngineBase : IAlgorithmService
    {
        protected HDevEngine _engine = new HDevEngine();
        protected HDevProcedure _procedure;
        protected HDevProcedureCall _procedureCall;
        protected readonly object _lock = new object();

        public void initialize(string scriptPath, string procedureName)
        {
            try
            {
                _engine.SetProcedurePath(scriptPath);
                _procedure = new HDevProcedure(procedureName);
                _procedureCall = new HDevProcedureCall(_procedure);
            }
            catch (HDevEngineException ex)
            {
                throw new Exception($"加载Halcon失败{scriptPath}, procedure: {procedureName}. Error: {ex.Message}");
            }

        }

        public abstract AlgorithmResult Execute(Dictionary<string, HObject> images, Dictionary<string, HTuple> parameters);
    }
}

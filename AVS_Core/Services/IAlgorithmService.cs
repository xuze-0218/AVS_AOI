using AVS_Core.Models;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Services
{
    public interface IAlgorithmService
    {
        /// <summary>
        /// 初始化引擎，加载算法脚本
        /// </summary>
        /// <param name="scriptPath"></param>
        /// <param name="procedureName"></param>
        void initialize(string scriptPath, string procedureName);

        AlgorithmResult Execute(Dictionary<string,HObject> images,Dictionary<string,HTuple> parameters);
    }
}

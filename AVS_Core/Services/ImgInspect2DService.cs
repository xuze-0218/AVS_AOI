using AVS_Core.Models;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Services
{
    public class ImgInspect2DService : HDevEngineBase
    {
        public override AlgorithmResult Execute(Dictionary<string, HObject> images, Dictionary<string, HTuple> parameters)
        {
            lock (_lock)
            {
                var result = new AlgorithmResult();
                var watch = Stopwatch.StartNew();

                try
                {
                    foreach (var img in images)
                    {
                        _procedureCall.SetInputIconicParamObject(img.Key, img.Value);
                    }
                    foreach (var param in parameters)
                    {
                        _procedureCall.SetInputCtrlParamTuple(param.Key, param.Value);
                    }
                    _procedureCall.Execute();

                    HTuple status = _procedureCall.GetOutputCtrlParamTuple("ResultStatus");
                    result.Status = status.S == "OK" ? InspectStatus.OK : InspectStatus.NG;

                    HObject outRegion = _procedureCall.GetOutputIconicParamObject("ResultRegion");
                    result.HDispRegions["MainRegion"] = outRegion.CopyObj(1, -1);
                }
                catch (Exception ex)
                {
                    result.Status = InspectStatus.Error;
                    result.Message = ex.Message;
                }
                finally
                {
                    watch.Stop();
                    result.ElapsedTime = watch.ElapsedMilliseconds;
                }
                return result;
            }
        }
    }
}

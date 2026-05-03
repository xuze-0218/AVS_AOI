using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS
{
    internal class ParamsGlobal
    {
        //配方参数保存路径
        private string _paramSaveDir = "E:\\param\\";
        public string ParamSaveDir
        {
            get { return _paramSaveDir; }
            set
            {
                _paramSaveDir = value;
            }
        }
        //相关图像保存路径
        private string _imageSaveDir = "E:\\image\\";
        public string ImageSaveDir
        {
            get { return _imageSaveDir; }
            set
            {
                _imageSaveDir = value;
            }
        }
        public ParamsSide sideParamA = new ParamsSide();
        public ParamsSide sideParamB = new ParamsSide();
        public ParamsSide sideParamC = new ParamsSide();
        public ParamsSide sideParamD = new ParamsSide();
    }
}

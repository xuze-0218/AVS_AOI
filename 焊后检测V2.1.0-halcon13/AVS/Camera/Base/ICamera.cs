using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;

namespace AVS
{
    public abstract class ICamera
    {
        #region 属性
        private string ipAddress;

        public string IpAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }
        // 连接状态
        private bool isConnect;
        public bool IsConnect
        {
            get { return isConnect; }
            set { isConnect = value; }
        }
        // 触发源
        private uint triggerSource;
        public uint TriggerSource
        {
            get { return triggerSource; }
            set { triggerSource = value; }
        }
        // 触发模式属性
        private bool isTigger;
        public bool IsTigger
        {
            get { return isTigger; }
            set { isTigger = value; }
        }
        #endregion
        // 委托定义:定义图像处理回调函数委托，用于将采集的图像传递给处理函数
        public delegate void ImgHandleCallBackFunc(HObject image);
        public ImgHandleCallBackFunc callBackFunction;

        public abstract int Connect(); // 抽象方法,子类必须实现具体的相机连接逻辑

        public abstract int DisConnect();

        //public abstract int StartGrab();

        //public abstract int StopGrab();
        public abstract bool SetExposure(long exposureTime);
        public abstract int GrabImage();
        }
    
}

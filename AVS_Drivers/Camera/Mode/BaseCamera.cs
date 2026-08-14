using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.Camera.Common.Model;
using AVS_Drivers.CameraSDKHelper.Common.Enum;
using System.Runtime.InteropServices;


namespace AVS_Drivers.Camera.Mode
{
    internal abstract class BaseCamera : ICamera
    {
        protected BaseCamera()
        {
            ActionGetImage += ResetActionImageSignal;
        }



        #region Parm
        public string SN { get; set; } = string.Empty;

        /// <summary>
        /// 回调委托，获取图像数据，+= 赋值,子类要添加到回调中
        /// </summary>
        protected Action<IntPtr> ActionGetImage { get; set; }

        protected AutoResetEvent ResetGetImageSignal = new AutoResetEvent(false);
        protected IntPtr CallBaclImg { get; set; }

        private readonly CameraInfoModel _imageInfo = new CameraInfoModel();
        public CameraInfoModel ImageInfo => _imageInfo;


        #endregion


        #region  operate

        public abstract void CloseDevice();

        public abstract List<string> GetListEnum();

        public abstract bool InitDevice(string CamSN);

        public bool StartWith_Continue_SetCallback(Action<IntPtr> callbackfunc)
        {
            try
            {
                SetTriggerMode(TriggerMode.Off, TriggerSource.Software);
                if (callbackfunc != null) ActionGetImage += callbackfunc;
                return StartGrabbing();
            }
            catch { return false; }
        }
        public bool StartWith_SoftTriggerModel()
        {
            try
            {
                SetTriggerMode(TriggerMode.On, TriggerSource.Software);
                return StartGrabbing();
            }
            catch { return false; }
        }
        public bool StartWith_HardTriggerModel(TriggerSource hardtriggeritem)
        {
            if (hardtriggeritem == TriggerSource.Software) hardtriggeritem = TriggerSource.Line0;
            SetTriggerMode(TriggerMode.On, hardtriggeritem);
            return StartGrabbing();
        }

        public bool StartWith_HardTriggerModel_SetCallback(TriggerSource hardtriggeritem, Action<IntPtr> callbackfunc)
        {
            if (hardtriggeritem == TriggerSource.Software) hardtriggeritem = TriggerSource.Line0;
            SetTriggerMode(TriggerMode.On, hardtriggeritem);
            if (callbackfunc != null) ActionGetImage += callbackfunc;
            return StartGrabbing();
        }

        public bool StartWith_SoftTriggerModel_SetCallback(Action<IntPtr> callbackfunc)
        {
            try
            {
                //Continue_SoftTrigger();
                SetTriggerMode(TriggerMode.On, TriggerSource.Software);
                if (callbackfunc != null) ActionGetImage += callbackfunc;
                return StartGrabbing();
            }
            catch { return false; }
        }
        public bool StopCallback(Action<IntPtr> callbackfunc)
        {
            if (callbackfunc != null)
                ActionGetImage -= callbackfunc;
            return StopGrabbing();
        }

        /// <summary>
        /// 等待硬触发获取图像
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="outtime"></param>
        /// <returns></returns>
        public bool GetImage(out IntPtr bitmap, int outtime = 3000)
        {
            bitmap = IntPtr.Zero;
            if (ResetGetImageSignal.WaitOne(outtime))
            {
                bitmap = CallBaclImg;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 软触发获取图像
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="outtime"></param>
        /// <returns></returns>
        public bool GetImageWithSoftTrigger(out IntPtr bitmap, int outtime = 3000)
        {
            bitmap = IntPtr.Zero;
            if (!SoftTrigger()) return false;

            if (ResetGetImageSignal.WaitOne(outtime))
            {
                bitmap = CallBaclImg;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 软触发
        /// </summary>
        /// <returns></returns>
        public abstract bool SoftTrigger();
        /// <summary>
        /// 软触发连续采集
        /// </summary>
        /// <returns></returns>
        public abstract bool Continue_SoftTrigger();

        #endregion


        #region SettingConfig
        public void SetCamConfig(CamConfig config)
        {
            if (config == null) return;
            SetExpouseTime(config.ExpouseTime);
            SetTriggerMode(config.triggerMode, config.triggeSource);
            SetTriggerPolarity(config.triggerPolarity);
            SetTriggerFliter(config.TriggerFilter);
            SetGain(config.Gain);
            SetTriggerDelay(config.TriggerDelay);
        }

        public void GetCamConfig(out CamConfig config)
        {
            GetExpouseTime(out ushort expouseTime);
            GetTriggerMode(out TriggerMode triggerMode, out TriggerSource hardwareTriggerModel);
            GetTriggerPolarity(out TriggerPolarity triggerPolarity);
            GetTriggerFliter(out ushort triggerfilter);
            GetGain(out short gain);
            GetTriggerDelay(out ushort triggerdelay);

            config = new CamConfig()
            {
                triggerMode = triggerMode,
                triggeSource = hardwareTriggerModel,
                triggerPolarity = triggerPolarity,
                TriggerFilter = triggerfilter,
                TriggerDelay = triggerdelay,
                ExpouseTime = expouseTime,
                Gain = gain
            };
        }


        /// <summary>
        /// 设置触发模式及触发源
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="triggerEnum"></param>
        /// <returns></returns>
        public abstract bool SetTriggerMode(TriggerMode mode, TriggerSource triggerEnum = TriggerSource.Line0);

        public abstract bool GetTriggerMode(out TriggerMode mode, out TriggerSource hardTriggerModel);



        public abstract bool SetExpouseTime(ushort value);

        public abstract bool GetExpouseTime(out ushort value);



        public abstract bool SetTriggerPolarity(TriggerPolarity polarity);

        public abstract bool GetTriggerPolarity(out TriggerPolarity polarity);



        /// <summary>
        /// 设置触发滤波时间 （us）
        /// </summary>
        /// <param name="flitertime"></param>
        /// <returns></returns>
        public abstract bool SetTriggerFliter(ushort flitertime);

        /// <summary>
        /// 获取触发参数时间 （us）
        /// </summary>
        /// <param name="flitertime"></param>
        /// <returns></returns>
        public abstract bool GetTriggerFliter(out ushort flitertime);


        public abstract bool SetTriggerDelay(ushort delay);

        public abstract bool GetTriggerDelay(out ushort delay);


        public abstract bool SetGain(short gain);

        public abstract bool GetGain(out short gain);

        public abstract bool SetLineMode(IOLines line, LineMode mode);
        public abstract bool SetLineStatus(IOLines line, LineStatus linestatus);
        public abstract bool GetLineStatus(IOLines line, out LineStatus lineStatus);

        public abstract bool AutoBalanceWhite();

        public abstract bool SetALLOutPutValue(int channel);

        #endregion


        #region  protected abstract


        /// <summary>
        /// 开始采图
        /// </summary>
        /// <returns></returns>
        protected abstract bool StartGrabbing();

        /// <summary>
        /// 停止采图
        /// </summary>
        /// <returns></returns>
        protected abstract bool StopGrabbing();

        private void ResetActionImageSignal(IntPtr data)
        {
            CallBaclImg = data;
            ResetGetImageSignal.Set();
        }
        public void Dispose()
        {

            Marshal.FreeHGlobal(CallBaclImg);

        }
        #endregion
    }
}



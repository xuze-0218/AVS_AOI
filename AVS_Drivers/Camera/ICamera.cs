using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.Camera.Common.Model;
using AVS_Drivers.CameraSDKHelper.Common.Enum;
using System;
using System.Collections.Generic;

namespace AVS_Drivers.Camera
{
    public interface ICamera : IDisposable
    {
        event Action<IntPtr> IntensityImageReceived;
        #region  operate
        CameraInfoModel ImageInfo { get; }

        string SN { get; set; }
        /// <summary>
        /// 获取相机SN枚举
        /// </summary>
        /// <returns></returns>
        List<string> GetListEnum();
        /// <summary>
        /// 初始化相机
        /// </summary>
        /// <param name="CamSN"></param>
        /// <returns></returns>
        bool InitDevice(string CamSN);

        /// <summary>
        /// 注销相机
        /// </summary>
        void CloseDevice();
        /// <summary>
        /// 等待硬触发获取图像
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="outtime"></param>
        /// <returns></returns>
        bool GetImage(out IntPtr data, int outtime = 3000);

        /// <summary>
        /// 软触发获取图像
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="outtime"></param>
        /// <returns></returns>
        bool GetImageWithSoftTrigger(out IntPtr data, int outtime = 3000);
        /// <summary>
        /// 软触发
        /// </summary>
        /// <returns></returns>
        bool SoftTrigger();

        /// <summary>
        /// 启动采集并绑定图像回调。触发模式需调用方已通过 SetTriggerMode 设置。
        /// </summary>
        /// <param name="callbackfunc">图像回调，接收图像数据指针</param>
        /// <returns>是否启动成功</returns>
        bool StartGrabbing(Action<IntPtr> callbackfunc);

        /// <summary>
        /// 停止采集。可选移除指定回调。
        /// </summary>
        /// <param name="callbackfunc">要移除的回调，可为 null 表示只停止不移除</param>
        /// <returns>是否停止成功</returns>
        bool StopGrabbing(Action<IntPtr> callbackfunc = null);
        #endregion

        #region SettingConfig
        /// <summary>
        /// 设置相机参数
        /// </summary>
        /// <param name="config"></param>
        void SetCamConfig(CamConfig config);
        /// <summary>
        /// 获取相机参数
        /// </summary>
        /// <param name="config"></param>
        void GetCamConfig(out CamConfig config);

        /// <summary>
        /// 设置触发模式及触发源
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="triggerEnum"></param>
        /// <returns></returns>
        bool SetTriggerMode(TriggerMode mode, TriggerSource triggerEnum = TriggerSource.Line0);

        /// <summary>
        /// 获取触发模式及触发源
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="hardTriggerModel"></param>
        /// <returns></returns>
        bool GetTriggerMode(out TriggerMode mode, out TriggerSource hardTriggerModel);

        /// <summary>
        /// 设置曝光时长
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetExpouseTime(ushort value);

        /// <summary>
        /// 获取曝光时长
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool GetExpouseTime(out ushort value);

        /// <summary>
        /// 设置硬触发极性
        /// </summary>
        /// <param name="polarity"></param>
        /// <returns></returns>
        bool SetTriggerPolarity(TriggerPolarity polarity);

        /// <summary>
        /// 获取硬触发极性
        /// </summary>
        /// <param name="polarity"></param>
        /// <returns></returns>
        bool GetTriggerPolarity(out TriggerPolarity polarity);

        /// <summary>
        /// 设置触发滤波时间 （us）
        /// </summary>
        /// <param name="flitertime"></param>
        /// <returns></returns>
        bool SetTriggerFliter(ushort flitertime);

        /// <summary>
        /// 获取触发滤波时间 （us）
        /// </summary>
        /// <param name="flitertime"></param>
        /// <returns></returns>
        bool GetTriggerFliter(out ushort flitertime);

        /// <summary>
        /// 设置触发延时
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        bool SetTriggerDelay(ushort delay);

        /// <summary>
        /// 获取触发延时
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        bool GetTriggerDelay(out ushort delay);

        /// <summary>
        /// 设置增益
        /// </summary>
        /// <param name="gain"></param>
        /// <returns></returns>
        bool SetGain(short gain);

        /// <summary>
        /// 获取增益值
        /// </summary>
        /// <param name="gain"></param>
        /// <returns></returns>
        bool GetGain(out short gain);

        /// <summary>
        /// 设置信号线模式
        /// </summary>
        /// <param name="line"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        bool SetLineMode(IOLines line, LineMode mode);

        /// <summary>
        /// 设置信号线电平状态
        /// </summary>
        /// <param name="line"></param>
        /// <param name="linestatus"></param>
        /// <returns></returns>
        bool SetLineStatus(IOLines line, LineStatus linestatus);

        /// <summary>
        /// 获取信号线电平状态
        /// </summary>
        /// <param name="line"></param>
        /// <param name="lineStatus"></param>
        /// <returns></returns>
        bool GetLineStatus(IOLines line, out LineStatus lineStatus);

        /// <summary>
        /// 自动白平衡
        /// </summary>
        /// <returns></returns>
        bool AutoBalanceWhite();

        bool SetALLOutPutValue(int channel);

        #endregion

    }
}

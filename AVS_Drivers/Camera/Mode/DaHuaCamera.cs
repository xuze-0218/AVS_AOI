using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.CameraSDKHelper.Common.Enum;
using System.Runtime.InteropServices;
using ThridLibray;

namespace AVS_Drivers.Camera.Mode
{
    internal class DaHuaCamera : BaseCamera
    {
        public DaHuaCamera() : base() { }
        #region param
        string _DeviceName;//设备名称
        List<string> _DeviceList = new List<string>();//设备列表
        IDevice m_dev;//设备对象
        Mutex mutex = new Mutex();
        #endregion


        #region operate
        public override List<string> GetListEnum()
        {
            List<IDeviceInfo> li = Enumerator.EnumerateDevices();
            if (li.Count > 0)
            {
                foreach (var item in li)
                {
                    _DeviceList.Add(item.Key);
                }
            }
            return _DeviceList;
        }
        public override bool InitDevice(string CamSN)
        {
            _DeviceList.Clear();
            _DeviceList = GetListEnum();
            int Index = 0;
            if (_DeviceList.Contains(CamSN))
            {
                Index = _DeviceList.IndexOf(CamSN);
            }
            else
            {
                return false;//集合中没有设备名
            }
            m_dev = Enumerator.GetDeviceByIndex(Index);
            if (!m_dev.Open())
            {
                //@"连接相机失败");
                return false;
            }
            bool res = m_dev.TriggerSet.Open(TriggerSourceEnum.Software);
            m_dev.StreamGrabber.SetBufferCount(8);
            m_dev.StreamGrabber.ImageGrabbed += OnImageGrabbed;//回调函数

           
            return true;
        }
        public override void CloseDevice()
        {
            try
            {
                if (m_dev != null)
                {
                    m_dev?.ShutdownGrab();//取消码流
                    m_dev.StreamGrabber.clearFrameBuffer();
                    m_dev?.Close();
                    //base.Dispose();
                }

            }
            catch (Exception)
            {
                return;
            }
        }
        public override bool SoftTrigger()
        {
            if(null==m_dev)return false;
            bool res = false;
            if (!m_dev.IsGrabbing)
            {
                using (IEnumParameter enumParameter = m_dev.ParameterCollection[new EnumName("TriggerSelector")])
                {
                    res = enumParameter.SetValue("FrameStart");
                }
                using (IEnumParameter enumParameter = m_dev.ParameterCollection[new EnumName("AcquisitionMode")])
                {
                    res = enumParameter.SetValue("Continuous");
                }
                res = SetTriggerMode(TriggerMode.On, TriggerSource.Software);
                res = StartGrabbing();
            }
            res = m_dev.ExecuteSoftwareTrigger();//执行软触发         
            return res;
        }
        public override bool Continue_SoftTrigger()
        {
            bool res = SetTriggerMode(TriggerMode.Off, TriggerSource.Software);
            using (IEnumParameter enumParameter = m_dev.ParameterCollection[new EnumName("AcquisitionMode")])
            {
                res = enumParameter.SetValue("Continuous");
            }
            m_dev.StreamGrabber.clearFrameBuffer();
            return true;
        }
        #endregion


        #region SettingConfig
        public override bool SetTriggerMode(TriggerMode mode, TriggerSource triggerEnum = TriggerSource.Line0)
        {
            bool flag1 = true;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerMode])
            {
                switch (mode)
                {
                    case TriggerMode.Off:
                        flag1 = m_dev.TriggerSet.Close(); ;
                        break;
                    case TriggerMode.On:
                        flag1 = p.SetValue("On");
                        break;
                    default:
                        break;
                }
            }
            bool flag2 = true;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerSource])
            {
                switch (triggerEnum)
                {
                    case TriggerSource.Software:
                        flag2 = p.SetValue("Software");
                        break;
                    case TriggerSource.Line0:
                        flag2 = p.SetValue("Line1");
                        break;
                    case TriggerSource.Line1:
                        flag2 = p.SetValue("Line2");
                        break;
                    case TriggerSource.Line2:
                        flag2 = p.SetValue("Line3");
                        break;
                    case TriggerSource.Line3:
                        flag2 = p.SetValue("Line4");
                        break;
                    case TriggerSource.Line4:
                        flag2 = p.SetValue("Line5");
                        break;
                    case TriggerSource.Line5:
                        flag2 = p.SetValue("Line6");
                        break;
                    default:
                        flag2 = p.SetValue("Software");
                        break;
                }
            }
            return flag1 && flag2;
        }
        public override bool GetTriggerMode(out TriggerMode mode, out TriggerSource hardTriggerModel)
        {

            mode = TriggerMode.On;
            hardTriggerModel = TriggerSource.Line0;
            bool flag1 = true;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerMode])
            {
                string modeStr = p.GetValue();
                switch (modeStr)
                {
                    case "Off":
                        mode = TriggerMode.Off;
                        break;
                    case "On":
                        mode = TriggerMode.On;
                        break;
                    default:
                        break;
                }
            }
            bool flag2 = true;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerSource])
            {
                string SourceStr = p.GetValue();
                switch (SourceStr)
                {
                    case "Software":
                        hardTriggerModel = TriggerSource.Software;
                        break;
                    case "Line1":
                        hardTriggerModel = TriggerSource.Line0;
                        break;
                    case "Line2":
                        hardTriggerModel = TriggerSource.Line1;
                        break;
                    case "Line3":
                        hardTriggerModel = TriggerSource.Line2;
                        break;
                    case "Line4":
                        hardTriggerModel = TriggerSource.Line3;
                        break;
                    case "Line5":
                        hardTriggerModel = TriggerSource.Line4;
                        break;
                    case "Line6":
                        hardTriggerModel = TriggerSource.Line5;
                        break;
                    default:
                        break;
                }
            }

            return flag1 && flag2;
        }
        public override bool SetExpouseTime(ushort value)
        {
            using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ExposureTime])
            {
                return p.SetValue(value);
            }
        }
        public override bool GetExpouseTime(out ushort value)
        {
            using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.ExposureTime])
            {
                value = Convert.ToUInt16(p.GetValue());
                return true;
            }
        }
        public override bool SetTriggerPolarity(TriggerPolarity polarity)
        {
            bool flag = false;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerActivation])
            {
                switch (polarity)
                {
                    case TriggerPolarity.FallingEdge:
                        flag = p.SetValue("Off");
                        break;
                    case TriggerPolarity.RisingEdge:
                        flag = p.SetValue("On");
                        break;
                    default:
                        break;
                }
            }
            return flag;
        }
        public override bool GetTriggerPolarity(out TriggerPolarity polarity)
        {
            polarity = TriggerPolarity.RisingEdge;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerActivation])
            {
                string modeStr = p.GetValue();
                switch (modeStr)
                {
                    case "Off":
                        polarity = TriggerPolarity.FallingEdge;
                        break;
                    case "On":
                        polarity = TriggerPolarity.RisingEdge;
                        break;
                    default:
                        break;
                }
            }
            return true;
        }
        public override bool SetTriggerFliter(ushort flitertime)
        {
            using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.IOLineDebouncerTimeAbs])
            {
                return p.SetValue(flitertime);
            }
        }
        public override bool GetTriggerFliter(out ushort flitertime)
        {
            flitertime = 1000;
            using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.IOLineDebouncerTimeAbs])
            {
                flitertime = Convert.ToUInt16(p.GetValue());
                return true;
            }

        }
        public override bool SetTriggerDelay(ushort delay)
        {
            using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerDelay])
            {
                return p.SetValue(delay);
            }
        }
        public override bool GetTriggerDelay(out ushort delay)
        {
            delay = 1000;
            using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerDelay])
            {
                delay = Convert.ToUInt16(p.GetValue());
                return true;
            }

        }
        public override bool SetGain(short gain)
        {
            using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.GainRaw])
            {
                return p.SetValue(gain);
            }
        }
        public override bool GetGain(out short gain)
        {
            gain = 1000;
            using (IFloatParameter p = m_dev.ParameterCollection[ParametrizeNameSet.GainRaw])
            {
                gain = Convert.ToInt16(p.GetValue());
                return true;
            }
        }
        public override bool SetLineMode(IOLines line, LineMode mode)
        {
            bool flag1 = true;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.IOLineSource])
            {
                switch (line)
                {
                    case IOLines.Line0:
                        flag1 = p.SetValue("Line1");
                        break;
                    case IOLines.Line1:
                        flag1 = p.SetValue("Line2");
                        break;
                    case IOLines.Line2:
                        flag1 = p.SetValue("Line3");
                        break;
                    case IOLines.Line3:
                        flag1 = p.SetValue("Line4");
                        break;
                    case IOLines.Line4:
                        flag1 = p.SetValue("Line5");
                        break;
                    case IOLines.Line5:
                        flag1 = p.SetValue("Line6");
                        break;
                    default:
                        break;
                }
            }
            bool flag2 = true;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.IOLineMode])
            {
                switch (mode)
                {
                    case LineMode.Output:
                        flag2 = p.SetValue("Output");
                        break;
                    case LineMode.Input:
                        flag2 = p.SetValue("Input");
                        break;
                    default:
                        break;
                }
            }
            return flag1 && flag2;

        }
        public override bool SetLineStatus(IOLines line, LineStatus linestatus)
        {
            bool flag1 = true;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.IOLineSource])
            {
                switch (line)
                {
                    case IOLines.Line0:
                        flag1 = p.SetValue("Line1");
                        break;
                    case IOLines.Line1:
                        flag1 = p.SetValue("Line2");
                        break;
                    case IOLines.Line2:
                        flag1 = p.SetValue("Line3");
                        break;
                    case IOLines.Line3:
                        flag1 = p.SetValue("Line4");
                        break;
                    case IOLines.Line4:
                        flag1 = p.SetValue("Line5");
                        break;
                    case IOLines.Line5:
                        flag1 = p.SetValue("Line6");
                        break;
                    default:
                        break;
                }
            }
            bool flag2 = true;
            using (IBooleanParameter p = m_dev.ParameterCollection[ParametrizeNameSet.IOLineStatus])
            {

                switch (linestatus)
                {
                    case LineStatus.Hight:
                        flag2 = p.SetValue(true);
                        break;
                    case LineStatus.Low:
                        flag2 = p.SetValue(true);
                        break;
                    default:
                        break;
                }
            }
            return flag1 && flag2;
        }
        //可能存在问题
        public override bool GetLineStatus(IOLines line, out LineStatus linestatus)
        {
            bool flag1 = true;
            linestatus = LineStatus.Low;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerSource])
            {
                string SourceStr = p.GetValue();
                switch (line)
                {
                    case IOLines.Line0:
                        flag1 = p.SetValue("Line1");
                        break;
                    case IOLines.Line1:
                        flag1 = p.SetValue("Line2");
                        break;
                    case IOLines.Line2:
                        flag1 = p.SetValue("Line3");
                        break;
                    case IOLines.Line3:
                        flag1 = p.SetValue("Line4");
                        break;
                    case IOLines.Line4:
                        flag1 = p.SetValue("Line5");
                        break;
                    case IOLines.Line5:
                        flag1 = p.SetValue("Line6");
                        break;
                    default:
                        break;
                }
            }
            using (IBooleanParameter p = m_dev.ParameterCollection[ParametrizeNameSet.IOLineStatus])
            {
                bool f = p.GetValue();
                switch (f)
                {
                    case true:
                        linestatus = LineStatus.Hight;
                        break;
                    case false:
                        linestatus = LineStatus.Low;
                        break;
                    default:
                        break;
                }
            }
            return true;
        }
        //可能存在问题
        public override bool AutoBalanceWhite()
        {
            bool flag = true;
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.BalanceWhiteAuto])
            {
                flag = p.SetValue("True");
            }
            return flag;
        }

        public override bool SetALLOutPutValue(int channel)
        {
            using (IIntegraParameter p = m_dev.ParameterCollection[new IntegerName("UserOutputValueAll")])
            {
                bool res= p.SetValue(channel);
                return res;
            }
           
        }
        #endregion


        #region helper
        protected override bool StartGrabbing()
        {
            // Set default state after grabbing starts
            // Turn off real-time mode which is default
            // 0: real-time
            // 1: trigger
            m_dev.StreamGrabber.clearFrameBuffer();//清除缓存数据
            if(m_dev.IsGrabbing)return true;
            if (!m_dev.GrabUsingGrabLoopThread())
            {
                //@"开启码流失败";
                return false;
            }
            return true;
        }
        protected override bool StopGrabbing()
        {
            var res = m_dev.ShutdownGrab();
            return res;//取消码流
        }
        IntPtr ArrToPtr(byte[] array)
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
        }
        private void OnImageGrabbed(Object sender, GrabbedEventArgs e)
        {
            // Thread.Sleep(6000);
            mutex.WaitOne();
            //if (frame == null) return;
            ActionGetImage?.Invoke(e.GrabResult.Raw);
            mutex.ReleaseMutex();
            //unsafe
            //{
            //    fixed (byte* p = e.GrabResult.Image)
            //    {

            //        HOperatorSet.GenImage1(out _Image, "byte", e.GrabResult.Width, e.GrabResult.Height, new IntPtr(p));
            //    }
            //}

            //HTuple w, h;
            //HOperatorSet.GetImageSize(halcon_image, out w, out h);
            //// HOperatorSet.WriteImage(halcon_image, "bmp", 0, "d:\\1.bmp");
            //HOperatorSet.SetPart(hv_WindowHandle1, 0, 0, h - 1, w - 1);


            //HDevWindowStack.SetActive(hv_WindowHandle1);

            //if (HDevWindowStack.IsOpen())
            //{
            //    HOperatorSet.DispObj(halcon_image, hv_WindowHandle1);
            //}
            //halcon_image.Dispose();
        }
        #endregion

    }
}

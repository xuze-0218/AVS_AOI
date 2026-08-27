using System;
using System.Collections.Generic;
using System.Diagnostics;
using ThridLibray;

namespace AVS_Drivers.Camera.Mode
{
    public class DHClass
    {

        public static DHClass Instance = new Lazy<DHClass>(() => new DHClass()).Value;

        private bool gColorCameraFlag = false;
        public IntPtr img = IntPtr.Zero;

        private string gCameraSeriNum = "";
        private IDevice m_dev;
        List<string> _DeviceList = new List<string>();//设备列表
        public  bool InitDevice(string CamSN)
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
                //XTrace.WriteLine($"Open Camera Failed");
                throw new Exception("Open Camera Failed");
            }
            bool res = m_dev.TriggerSet.Open(TriggerSourceEnum.Software);
            //XTrace.WriteLine($"软触发打开:{res}");
            if (res)
            {
                //XTrace.WriteLine($"TriggerMode:{res}");
            }
            else
            {
             
                return false;
            }


            // 设置缓存个数为8（默认值为16） 
            // set buffer count to 8 (default 16) 
            res=m_dev.StreamGrabber.SetBufferCount(8);
            if (!res)
            {
                //XTrace.WriteLine($"SetBufferCount:{res}");
            }
            using (IEnumParameter enumParameter = m_dev.ParameterCollection[new EnumName("TriggerSelector")])
            {
                res = enumParameter.SetValue("FrameStart");
                //XTrace.WriteLine($"FrameStart:{res}");
            }
            using (IEnumParameter enumParameter = m_dev.ParameterCollection[new EnumName("AcquisitionMode")])
            {
                res = enumParameter.SetValue("Continuous");
                //XTrace.WriteLine($"Continuous:{res}");
            }
            using (IEnumParameter p = m_dev.ParameterCollection[ParametrizeNameSet.TriggerMode])
            {
                res= p.SetValue("On");
                //XTrace.WriteLine($"TriggerMode:{res}");
            }

            //m_dev.StreamGrabber.ImageGrabbed += OnImageGrabbed;
            // 开启码流 
            // start grabbing 
            if (!m_dev.GrabUsingGrabLoopThread())
            {
                //XTrace.WriteLine("取流开启失败");
                return false;
            }

          

            //if (m_dev.StreamGrabber.Start(GrabStrategyEnum.grabStrartegySequential, GrabLoop.ProvidedByUser) == false)
            //{
            //    XTrace.WriteLine($"StreamGrabber.Start:false");
            //    return false;
            //}

            return true;
        }
        public  List<string> GetListEnum()
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
        //private void OnImageGrabbed(Object sender, GrabbedEventArgs e)
        //{
        //    img = e.GrabResult.Raw;
        //}
        public  bool Trigger()
        {
            return  m_dev.ExecuteSoftwareTrigger();
        }

        public  bool GetFrame(ref IntPtr InData, int InOutTime)
        {
            IGrabbedRawData data = null;
            bool flag = m_dev.WaitForFrameTriggerReady(out data, InOutTime);
            Debug.WriteLine($"拍照是否成功：{flag}");
            //if (flag)
            //{
                
            //    InData = data.Raw;
            //    InSize = new Size(data.Width, data.Height);
            //    XTrace.WriteLine(InData.ToString());
            //    XTrace.WriteLine($"PixelFmt：{data.PixelFmt}");
            //    switch (data.PixelFmt)
            //    {
            //        case GvspPixelFormatType.gvspPixelRGB8:
            //            InImgFomat = ImgFmt.gvspPixelRGB8;
            //            break;
            //        case GvspPixelFormatType.gvspPixelBayGB8:
            //            InImgFomat = ImgFmt.gvspPixelBayGB8;
            //            break;
            //        case GvspPixelFormatType.gvspPixelMono8:
            //            InImgFomat = ImgFmt.gvspPixelMono8;
            //            break;
            //        case GvspPixelFormatType.gvspPixelBayRG8:
            //            InImgFomat = ImgFmt.gvspPixelBayRG8;
            //            break;
            //    }
            //}
            return flag;
        }

        public  void Dispose()
        {
            try
            {
                if (m_dev != null)
                {
                    m_dev.Close();
                }
            }
            catch (Exception ex)
            {
                //XTrace.WriteException(ex);
            }
           
        }

        public enum ImgFmt
        {
            gvspPixelRGB8,
            gvspPixelBayGB8,
            gvspPixelMono8,
            gvspPixelBayRG8,
            Mono8,
            bayer_bg
        }

    }
}

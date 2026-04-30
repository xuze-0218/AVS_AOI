using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DVPCameraType;
using static DVPCameraType.DVPCamera;
using ThridLibray;
using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.CameraSDKHelper.Common.Enum;

namespace AVS_Drivers.Camera.Mode
{
    internal class DSCamera: BaseCamera
    {
        public DSCamera() { }
        public uint m_handle = 0;
        public bool m_bAeOp = false;
        public int m_n_dev_count = 0;
        string m_strFriendlyName = "";

        public static IntPtr m_ptr_wnd = new IntPtr();
        public static IntPtr m_ptr = new IntPtr();
        public static bool m_b_start = false;
        public static int m_CamCount = 0;
        public static dvpCameraInfo[] m_info = new dvpCameraInfo[16];
        private DVPCamera.dvpStreamCallback _proc;
        #region operate

        public override List<string> GetListEnum()
        {
           try
            {
                List<string> deviceList = new List<string>();
                dvpStatus status;
                uint i, n = 0;
                dvpCameraInfo dev_info = new dvpCameraInfo();

                status = DVPCamera.dvpRefresh(ref n);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                m_n_dev_count = (int)n;
                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    m_CamCount = 0;

                    for (i = 0; i < n; i++)
                    {
                        // Acquire each camera's information one by one.
                        status = DVPCamera.dvpEnum(i, ref dev_info);
                        Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        if (status == dvpStatus.DVP_STATUS_OK)
                        {
                            m_info[m_CamCount] = dev_info;
                            deviceList.Add(dev_info.UserID);
                        }
                    }
                }
                return deviceList;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public override bool InitDevice(string CamSN)
        {           
            dvpStatus status = dvpStatus.DVP_STATUS_OK;
            //判断相机句柄是否有效
            if (!IsValidHandle(m_handle))
            {

               // Open the specific device by the selected user define name.
               status = DVPCamera.dvpOpenByUserId(CamSN, dvpOpenMode.OPEN_NORMAL, ref m_handle);
               if (status != dvpStatus.DVP_STATUS_OK)
               {
                   Debug.WriteLine("Open the device failed!");
               }
               else
               {
                  // If it needs to display images ,the user should register a callback function and finish the operation of drawing pictures in the registered callback function.
                  // Note: Drawing pictures in the callback function maybe generate some delays for acquiring image data by the use of "dvpGetFrame".
                  _proc = _dvpStreamCallback;
                  using (Process curProcess = Process.GetCurrentProcess())
                  using (ProcessModule curModule = curProcess.MainModule)
                  {
                     status = DVPCamera.dvpRegisterStreamCallback(m_handle, _proc, dvpStreamEvent.STREAM_EVENT_FRAME_THREAD, m_ptr);
                     Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                  }
               }
            }
            else
            {
                CloseDevice();
            }
            return true;
        }

        public override void CloseDevice()
        {
            try
            {
                dvpStatus status = dvpStatus.DVP_STATUS_OK;
                // check camear
                dvpStreamState StreamState = new dvpStreamState();
                status = DVPCamera.dvpGetStreamState(m_handle, ref StreamState);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                if (StreamState == dvpStreamState.STATE_STARTED)
                {
                    // stop camera
                    status = DVPCamera.dvpStop(m_handle);
                    Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                }

                // cloas camera
                status = DVPCamera.dvpClose(m_handle);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                m_handle = 0;
            }
            catch (Exception e)
            {
                Debug.WriteLine("关闭设备失败"+e.ToString());
            }
        }

        public override bool SoftTrigger()
        {
            try
            {
                dvpStatus status = dvpStatus.DVP_STATUS_OK;
                // check camear
                dvpStreamState StreamState = new dvpStreamState();
                status = DVPCamera.dvpGetStreamState(m_handle, ref StreamState);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                if (StreamState == dvpStreamState.STATE_STARTED)
                {
                    dvpTriggerSource Source = new dvpTriggerSource();
                    DVPCamera.dvpGetTriggerSource(m_handle, ref Source);
                    if (Source != dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE)
                    {
                        DVPCamera.dvpSetTriggerSource(m_handle, dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE);
                    }

                    status = DVPCamera.dvpTriggerFire(m_handle);
                    if (status == dvpStatus.DVP_STATUS_OK)
                    {
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    status = DVPCamera.dvpStart(m_handle);
                    dvpTriggerSource Source = new dvpTriggerSource();
                    DVPCamera.dvpGetTriggerSource(m_handle, ref Source);
                    if (Source != dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE)
                    {
                        DVPCamera.dvpSetTriggerSource(m_handle, dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE);
                    }

                    status = DVPCamera.dvpTriggerFire(m_handle);
                    if (status == dvpStatus.DVP_STATUS_OK)
                    {
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }
            }
          catch (Exception e)
            {
                return false ;
            }
        }

        public override bool Continue_SoftTrigger()
        {
            if (IsValidHandle(m_handle))
            {
                dvpStreamState state = new dvpStreamState();
                dvpStatus status;

                DVPCamera.dvpSetTriggerState(m_handle, false);
                
                //获取当前视频状态
                status = DVPCamera.dvpGetStreamState(m_handle, ref state);                
                if (state == dvpStreamState.STATE_STARTED)
                {
                    status = DVPCamera.dvpStop(m_handle);
                }

                status = DVPCamera.dvpStart(m_handle);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region SettingConfig
        public override bool SetTriggerMode(TriggerMode mode, TriggerSource triggerEnum = TriggerSource.Line0)
        {
            int rec=0;
            dvpStatus status = new dvpStatus();

            //获取视频流状态
            dvpStreamState StreamState = new dvpStreamState();
            switch (mode)
            {
                case TriggerMode.Off:
                    if (IsValidHandle(m_handle))
                    {

                        status = DVPCamera.dvpGetStreamState(m_handle, ref StreamState);
                        Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

                        if (StreamState == dvpStreamState.STATE_STARTED)
                        {
                            //关闭视频流
                            status = DVPCamera.dvpStop(m_handle);
                            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        }
                        //打开/关闭相机触发模式
                        status = DVPCamera.dvpSetTriggerState(m_handle, false);
                        Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        if (status == dvpStatus.DVP_STATUS_OK)
                        {
                            rec = 1;
                        }
                        else
                        {
                            rec = 0;
                        }
                        if (StreamState == dvpStreamState.STATE_STARTED)
                        {
                            //开启视频流
                            status = DVPCamera.dvpStart(m_handle);
                            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        }
                    }
                    break;
                case TriggerMode.On:
                    if (IsValidHandle(m_handle))
                    {
                        status = DVPCamera.dvpGetStreamState(m_handle, ref StreamState);
                        Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

                        if (StreamState == dvpStreamState.STATE_STARTED)
                        {
                            //关闭视频流
                            status = DVPCamera.dvpStop(m_handle);
                            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        }
                        //打开/关闭相机触发模式
                        status = DVPCamera.dvpSetTriggerState(m_handle, true);
                        Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        if(status== dvpStatus.DVP_STATUS_OK)
                        {
                            rec = 1;
                        }
                        else
                        {
                            rec=0;
                        }
                        if (StreamState == dvpStreamState.STATE_STARTED)
                        {
                            //开启视频流
                            status = DVPCamera.dvpStart(m_handle);
                            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        }
                    }
                    break;
                default:
                    if (IsValidHandle(m_handle))
                    {
                        status = DVPCamera.dvpGetStreamState(m_handle, ref StreamState);
                        Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

                        if (StreamState == dvpStreamState.STATE_STARTED)
                        {
                            //关闭视频流
                            status = DVPCamera.dvpStop(m_handle);
                            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        }
                        //打开/关闭相机触发模式
                        status = DVPCamera.dvpSetTriggerState(m_handle, false);
                        Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        if (status == dvpStatus.DVP_STATUS_OK)
                        {
                            rec = 1;
                        }
                        else
                        {
                            rec = 0;
                        }
                        if (StreamState == dvpStreamState.STATE_STARTED)
                        {
                            //开启视频流
                            status = DVPCamera.dvpStart(m_handle);
                            Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        }
                    }
                    else
                    {
                        rec = 0;
                    }
                    break;
            }
            bool flag1 = (1 == rec);
            switch (triggerEnum)
            {
                case TriggerSource.Software:
                    dvpSetTriggerSource(m_handle,dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE);
                    break;
                case TriggerSource.Line0:
                    dvpSetTriggerSource(m_handle, dvpTriggerSource.TRIGGER_SOURCE_LINE1);
                    break;
                case TriggerSource.Line1:
                    dvpSetTriggerSource(m_handle, dvpTriggerSource.TRIGGER_SOURCE_LINE2);
                    break;
                case TriggerSource.Line2:
                    dvpSetTriggerSource(m_handle, dvpTriggerSource.TRIGGER_SOURCE_LINE3);
                    break;
                case TriggerSource.Line3:
                    dvpSetTriggerSource(m_handle, dvpTriggerSource.TRIGGER_SOURCE_LINE4);
                    break;
                default:
                    dvpSetTriggerSource(m_handle, dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE);
                    break;
            }
            bool flag2 = (1 == rec);
            return flag1 && flag2;
        }

        public override bool GetTriggerMode(out TriggerMode mode, out TriggerSource hardTriggerModel)
        {

            //mode = TriggerMode.On;
            //hardTriggerModel = TriggerSource.Line0;
            //MVCameraCtrl.MVCC_ENUMVALUE stParam = new MVCameraCtrl.MVCC_ENUMVALUE();

            //int nRet = _myCamera.MV_CC_GetEnumValue_NET("TriggerMode", ref stParam);
            //MVCameraCtrl.MV_CAM_TRIGGER_MODE Mode = (MVCameraCtrl.MV_CAM_TRIGGER_MODE)stParam.nCurValue;
            //bool flag1 = (MVCameraCtrl.MV_OK == nRet);

            //switch (Mode)
            //{
            //    case MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF:
            //        mode = TriggerMode.Off;
            //        break;
            //    case MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON:
            //        mode = TriggerMode.On;
            //        break;
            //    default:
            //        mode = TriggerMode.On;
            //        break;
            //}

            //nRet = _myCamera.MV_CC_GetEnumValue_NET("TriggerSource", ref stParam);
            //MVCameraCtrl.MV_CAM_TRIGGER_SOURCE Source = (MVCameraCtrl.MV_CAM_TRIGGER_SOURCE)stParam.nCurValue;
            //bool flag2 = (MVCameraCtrl.MV_OK == nRet);
            //switch (Source)
            //{
            //    case MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0:
            //        hardTriggerModel = TriggerSource.Line0;
            //        break;
            //    case MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE1:
            //        hardTriggerModel = TriggerSource.Line1;
            //        break;
            //    case MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE2:
            //        hardTriggerModel = TriggerSource.Line2;
            //        break;
            //    case MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE:
            //        hardTriggerModel = TriggerSource.Software;
            //        break;
            //    default:
            //        hardTriggerModel = TriggerSource.Line0;
            //        break;
            //}

            //return flag1 && flag2;
            mode= TriggerMode.Off;
            hardTriggerModel=TriggerSource.Software;
            return false ;
        }


        public override bool SetExpouseTime(ushort value)
        {

            return false;
        }

        public override bool GetExpouseTime(out ushort value)
        {
            value = 0;
            return false;
        }


        public override bool SetTriggerPolarity(TriggerPolarity polarity)
        {

            return false;
        }

        public override bool GetTriggerPolarity(out TriggerPolarity polarity)
        {
            polarity = TriggerPolarity.RisingEdge;

            return false;
        }


        public override bool SetTriggerFliter(ushort flitertime)
        {
            return false;
        }

        public override bool GetTriggerFliter(out ushort flitertime)
        {
            flitertime = 1000;
            return false;
        }


        public override bool SetTriggerDelay(ushort delay)
        {
            return false;
        }

        public override bool GetTriggerDelay(out ushort delay)
        {
            delay = 0;
            return false;

        }


        public override bool SetGain(short gain)
        {
            return false;
        }

        public override bool GetGain(out short gain)
        {
            gain = 0;
            return false;
        }

        public override bool SetLineMode(IOLines line, LineMode mode)
        {
            return false;

        }
        public override bool SetLineStatus(IOLines line, LineStatus linestatus)
        {
            return false;
        }

        public override bool GetLineStatus(IOLines line, out LineStatus linestatus)
        {
            bool resultsignal = false;
            linestatus=LineStatus.Hight;
            return false;
        }


        public override bool AutoBalanceWhite()
        {
            return false;
        }

        public override bool SetALLOutPutValue(int channel)
        {
            return false ;
        }

        #endregion

        #region helper 

        protected override bool StartGrabbing()
        {
            if (IsValidHandle(m_handle))
            {
                dvpStreamState state = new dvpStreamState();
                dvpStatus status;

                DVPCamera.dvpSetTriggerState(m_handle, false);

                //获取当前视频状态
                status = DVPCamera.dvpGetStreamState(m_handle, ref state);
                if (state == dvpStreamState.STATE_STARTED)
                {
                    status = DVPCamera.dvpStop(m_handle);
                }

                status = DVPCamera.dvpStart(m_handle);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool StopGrabbing()
        {
            try
            {
                
                dvpStatus status = dvpStatus.DVP_STATUS_OK;
                // check camear
                dvpStreamState StreamState = new dvpStreamState();
                status = DVPCamera.dvpGetStreamState(m_handle, ref StreamState);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                if (StreamState == dvpStreamState.STATE_STARTED)
                {
                    // stop camera
                    status = DVPCamera.dvpStop(m_handle);
                    Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("停止取流失败" + e.ToString());
                return false;
            }
        }


        public bool IsValidHandle(uint handle)
        {
            bool bValidHandle = false;
            dvpStatus status = DVPCamera.dvpIsValid(handle, ref bValidHandle);
            if (status == dvpStatus.DVP_STATUS_OK)
            {
                return bValidHandle;
            }
            
            return false;
        }

        public  int  _dvpStreamCallback(uint handle, dvpStreamEvent _event, IntPtr pContext, ref dvpFrame refFrame, IntPtr pBuffer)
        {
           dvpStatus status = DVPCamera.dvpDrawPicture(ref refFrame, pBuffer,m_ptr_wnd, (IntPtr)0, (IntPtr)0);
           Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            if (pBuffer == null) return 0;
            ActionGetImage?.Invoke(pBuffer);
            return 1;
        }
        #endregion
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using HalconDotNet;
using Lmi3d.GoSdk;
using Lmi3d.GoSdk.Messages;
using Lmi3d.Zen;
using Lmi3d.Zen.Io;

namespace AVS
{
    public class LMISingle
    {
        public delegate void LMICallBackFunc(HObject image);

        private int id = -1;

        private bool isconnect = false;

        private bool isUseTrigger = false;

        private bool isgrabimage = false;

        private HObject sensorImage3DUniform;

        private HObject sensorImage3DIntensity;

        private string errorManager = "";

        private GoSensor sensor;

        private GoSetup setup;

        public int pointCloudCalibIndex = 0;

        public string pointCloudSavePath = "E:\\";

        public uint contorlPort = 0u;

        public uint healthPort = 0u;

        public uint dataPort = 0u;

        public LMICallBackFunc CallBackImageDeep;

        public LMICallBackFunc CallBackImageIntensity;

        public int cameraIndex = 0;

        public double sensorRoiOrignX;

        public double sensorRoiOrignZ;

        public double sensorRoiWidth;

        public double sensorRoiHeight;

        public double sensorExposuretime;

        public double scanningFrequency;

        public double xResolution;
        public double yResolution;
        public double zResolution;
        public double xOffset;
        public double yOffset;
        public double zOffset;

        public HTuple isEmptyImageUniform = 0;

        public HTuple isEmptyImageIntensity = 0;

        public HObject emptyImageUniform;

        public HObject emptyImageIntensity;

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public bool IsConnect
        {
            get
            {
                return isconnect;
            }
            set
            {
                isconnect = value;
            }
        }

        public GoSensor Mysensor
        {
            get
            {
                return sensor;
            }
            set
            {
                sensor = value;
            }
        }

        public bool IsUseTrigger
        {
            get
            {
                return isUseTrigger;
            }
            set
            {
                isUseTrigger = value;
            }
        }

        public bool IsGrabImage
        {
            get
            {
                return isgrabimage;
            }
            set
            {
                isgrabimage = value;
            }
        }

        public HObject ImageObjectUniform
        {
            get
            {
                return sensorImage3DUniform;
            }
            set
            {
                sensorImage3DUniform = value;
            }
        }

        public HObject ImageObjectIntensity
        {
            get
            {
                return sensorImage3DIntensity;
            }
            set
            {
                sensorImage3DIntensity = value;
            }
        }

        public string ErrorManager
        {
            get
            {
                return errorManager;
            }
            set
            {
                errorManager = value;
            }
        }

        public LMISingle()
        {
            HOperatorSet.GenEmptyObj(out sensorImage3DUniform);
            HOperatorSet.GenEmptyObj(out sensorImage3DIntensity);
            HOperatorSet.GenEmptyObj(out emptyImageUniform);
            HOperatorSet.GenEmptyObj(out emptyImageIntensity);
        }

        public int Initialization(string ip, LMIFactory lmiFactory, LMISingle LMISideFace)
        {
            try
            {
                KIpAddress address = KIpAddress.Parse(ip);
                LMISideFace.Mysensor = lmiFactory.system.FindSensorByIpAddress(address);
                LMISideFace.pointCloudCalibIndex = 1;
                LMISideFace.cameraIndex = 1;
                LMISideFace.IsUseTrigger = true;
            }
            catch(Exception ex)
            {
                Global.AddLog("3D相机初始化出错：\r\n" + ex.Message.ToString());
                return -1;
            }
            return 1;
        }

        public int Connect(string path, string cameraIndex1)
        {
            sensor.Connect();
            if (!(cameraIndex1 == "FirstLMI"))
            {
                if (cameraIndex1 == "LMI")
                {
                    sensor.UploadFile(path, "LMI.job");
                    sensor.CopyFile("LMI.job", "_live.job");
                }
            }
            else
            {
                sensor.UploadFile(path, "FirstLMI.job");
                sensor.CopyFile("FirstLMI.job", "_live.job");
            }

            sensor.EnableData(enable: true);
            sensor.Start();
            sensor.SetDataHandler(onData);
            setup = sensor.Setup;
            bool intensityEnabled = setup.IntensityEnabled;
            isconnect = true;
            return 1;
        }

        public int GrabImage()
        {
            sensor.SetDataHandler(onData);
            return 0;
        }

        public int DisConnect()
        {
            try
            {
                if (isconnect)
                {
                    sensor.Stop();
                    return 0;
                }

                return -1;
            }
            catch
            {
                isconnect = false;
                return -1;
            }
        }

        public int GetCamParameter()
        {
            try
            {
                sensorExposuretime = setup.GetExposure(0);
                sensorRoiOrignX = setup.GetActiveAreaX(0);
                sensorRoiOrignZ = setup.GetActiveAreaZ(0);
                sensorRoiHeight = setup.GetActiveAreaHeight(0);
                sensorRoiWidth = setup.GetActiveAreaWidth(0);
                scanningFrequency = setup.FrameRate;
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        public int SetCamParameter()
        {
            try
            {
                setup.SetExposure(0, sensorExposuretime);
                setup.SetActiveAreaX(0, sensorRoiOrignX);
                setup.SetActiveAreaZ(0, sensorRoiOrignZ);
                setup.SetActiveAreaHeight(0, sensorRoiHeight);
                setup.SetActiveAreaWidth(0, sensorRoiWidth);
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        public void onData(KObject data)
        {
            try
            {
                GoDataSet goDataSet = (GoDataSet)data;
                for (uint num = 0u; num < goDataSet.Count; num++)
                {
                    GoDataMsg goDataMsg = (GoDataMsg)goDataSet.Get(num);
                    switch ((int)goDataMsg.MessageType)
                    {
                        case 8://深度图
                            {
                                GoUniformSurfaceMsg goSurfaceMsg = (GoUniformSurfaceMsg)goDataMsg;

                                xResolution = goSurfaceMsg.XResolution;
                                yResolution = goSurfaceMsg.YResolution;
                                zResolution = goSurfaceMsg.ZResolution;

                                xOffset = goSurfaceMsg.XOffset;
                                yOffset = goSurfaceMsg.YOffset;
                                zOffset = goSurfaceMsg.ZOffset;

                                int num5 = (int)goSurfaceMsg.Width;
                                int num6 = (int)goSurfaceMsg.Length;
                                long num7 = num5 * num6;
                                IntPtr data3 = goSurfaceMsg.Data;
                                short[] array4 = new short[num7];
                                //HImage hImage2 = new HImage("uint2", num5, num6, data3);                             
                                Marshal.Copy(data3, array4, 0, array4.Length);
                                ushort[] array5 = new ushort[array4.Length];
                                double[] array6 = new double[array4.Length];
                                for (int j = 0; j < array4.Length; j++)
                                {
                                    array5[j] = (ushort)(array4[j] + 32768);
                                }

                                ToHalconImageDeep(array5, num5, num6, out sensorImage3DUniform);
                                CallBackImageDeep(sensorImage3DUniform);
                                break;
                            }
                        case 9://亮度图
                            {
                                GoSurfaceIntensityMsg goSurfaceIntensityMsg = (GoSurfaceIntensityMsg)goDataMsg;
                                //xResolution = goSurfaceIntensityMsg.XResolution;
                                //yResolution = goSurfaceIntensityMsg.YResolution;
                                //xOffset = goSurfaceIntensityMsg.XOffset;
                                //yOffset = goSurfaceIntensityMsg.YOffset;
                                int num2 = (int)goSurfaceIntensityMsg.Width;
                                int num3 = (int)goSurfaceIntensityMsg.Length;
                                long num4 = num2 * num3;
                                IntPtr data2 = goSurfaceIntensityMsg.Data;
                                byte[] array = new byte[goSurfaceIntensityMsg.Width * goSurfaceIntensityMsg.Length];
                                //HImage hImage = new HImage("byte", num2, num3, data2);                     
                                Marshal.Copy(data2, array, 0, array.Length);
                                byte[] array2 = new byte[array.Length];
                                double[] array3 = new double[array.Length];
                                for (int i = 0; i < array.Length; i++)
                                {
                                    array2[i] = array[i];
                                }
                                ToHalconImageIntersity(array2, num2, num3, out sensorImage3DIntensity);
                                CallBackImageIntensity(sensorImage3DIntensity);
                                break;
                            }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void ToHalconImageDeep(ushort[] image, int Width, int Height, out HObject outImage)
        {
            GCHandle gCHandle = GCHandle.Alloc(image, GCHandleType.Pinned);
            outImage = new HObject();
            HOperatorSet.GenEmptyObj(out outImage);
            HOperatorSet.GenImage1(out outImage, "uint2", Width, Height, gCHandle.AddrOfPinnedObject());    
            gCHandle.Free();
        }

        private void ToHalconImageIntersity(byte[] image, int width, int height, out HObject outImage)
        {
            GCHandle gCHandle = GCHandle.Alloc(image, GCHandleType.Pinned);         
            outImage = new HObject();
            HOperatorSet.GenEmptyObj(out outImage);            
            HOperatorSet.GenImage1(out outImage, "byte", width, height, gCHandle.AddrOfPinnedObject());
            gCHandle.Free();
        }
    }
}

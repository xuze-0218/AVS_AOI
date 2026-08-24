using AVS_Drivers.Camera.Common.Enum;
using AVS_Drivers.Camera.Mode;
using System.Reflection;

namespace AVS_Drivers.Camera
{
    public class CamFactory
    {
        public CamFactory() { if (CameraList == null) CameraList = new List<ICamera>(); }

        private static List<ICamera> CameraList { get; set; } = new List<ICamera>() { };

        /// <summary>
        /// 按相机品牌获取相近SN枚举
        /// </summary>
        /// <param name="brand"></param>
        /// <returns></returns>
        public static List<string> GetDeviceEnum(CameraBrand brand)
        {
            ICamera camera = null;
            switch (brand)
            {
                case CameraBrand.DaHeng:
                    camera = new DHCamera();
                    break;
                case CameraBrand.HIK:
                    camera = new HKCamera();
                    break;
                case CameraBrand.DaHua:
                    camera = new DaHuaCamera();
                    break;
                case CameraBrand.Basler:
                    break;
                case CameraBrand.HIK3D:
                    camera = new Hik3DCamera();
                    break;
                default: break;
            }
            return camera?.GetListEnum();
        }
        /// <summary>
        /// 通过反射根据相机类获取相近SN枚举,类需要fullName
        /// </summary>
        /// <param name="CamClass"></param>
        /// <returns></returns>
        public static List<string> GetDevice(string CamClass)
        {
            try
            {
                Assembly ass = Assembly.GetCallingAssembly();
                //获取程序集中的类
                Type t = ass.GetType(CamClass);
                //创建类的实例对象
                ICamera camera = (ICamera)Activator.CreateInstance(t);

                return camera?.GetListEnum();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 按品牌创建相机
        /// </summary>
        /// <param name="brand"></param>
        /// <returns></returns>
        public static ICamera CreatCamera(CameraBrand brand)
        {
            ICamera returncamera = null;
            switch (brand)
            {
                case CameraBrand.DaHeng:
                    returncamera = new DHCamera();
                    break;
                case CameraBrand.HIK:
                    returncamera = new HKCamera();
                    break;
                case CameraBrand.DaHua:
                    returncamera = new DaHuaCamera();
                    break;
                case CameraBrand.Basler:
                    break;
                default:
                    break;
            }
            CameraList.Add(returncamera);
            return returncamera;
        }

        /// <summary>
        /// 通过反射根据相机类创建相机,类需要fullName
        /// </summary>
        /// <param name="brand"></param>
        /// <returns></returns>
        public static ICamera CreatCamera(string CamClass)
        {
            try
            {
                //获取当前程序集
                Assembly ass = Assembly.GetCallingAssembly();
                //获取程序集中的类
                Type t = ass.GetType(CamClass);
                //创建类的实例对象
                ICamera returncamera = (ICamera)Activator.CreateInstance(t);
                CameraList.Add(returncamera);
                return returncamera;
            }
            catch { return null; }

        }
        /// <summary>
        /// 获取对应SN的相机实例
        /// </summary>
        /// <param name="CamSN"></param>
        /// <returns></returns>
        public static ICamera GetItem(string CamSN)
        {
            ICamera cameraStandard = null;
            if (CameraList.Count < 1) return cameraStandard;

            foreach (var item in CameraList)
            {
                if ((item as BaseCamera).SN.Equals(CamSN))
                {
                    cameraStandard = item;
                    break;
                }
            }
            return cameraStandard;
        }

        /// <summary>
        /// 注销相机
        /// </summary>
        /// <param name="decamera"></param>
        public static void DestroyCamera(ICamera decamera)
        {
            CameraList?.Remove(decamera);
            decamera?.CloseDevice();
        }

        /// <summary>
        /// 注销所有相机
        /// </summary>
        public static void DestroyAll()
        {
            if (CameraList.Count < 1) return;
            foreach (var camereaitem in CameraList)
            {
                camereaitem?.CloseDevice();
            }
            CameraList?.Clear();
        }
    }
}

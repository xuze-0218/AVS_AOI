using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AVS
{
    public static class ImageSaveProcess
    {
        private static SaveData[] InspectDataQueue = new SaveData[90];

        public static void initData()
        {
            for (int i = 0; i < InspectDataQueue.Length; i++)
            {
                InspectDataQueue[i] = new SaveData();
                InspectDataQueue[i].IsDetect2D = false;
                InspectDataQueue[i].IsDetect3D = false;
            }
        }

        //回调
        public static void ParamInitial()
        {
            ImgInspect2D.DataSaveCallBack = new DataSave.DataSaveDelegate2D(DataProcess2D);
            ImgInspect3D.DataSaveCallBack = new DataSave.DataSaveDelegate3D(DataProcess3D);
           
        }

        private static void DataProcess2D(InspectResult2DData saveImageData)
        {
            
            InspectDataQueue[saveImageData.PoleNum - 1].Inspect2DData = saveImageData;
            InspectDataQueue[saveImageData.PoleNum - 1].IsDetect2D = true;
        }

        private static void DataProcess3D(InspectResult3DData saveImageData)
        {

            InspectDataQueue[saveImageData.PoleNum - 1].Inspect3DData = saveImageData;
            InspectDataQueue[saveImageData.PoleNum - 1].IsDetect3D = true;

        }
    }



}

using MMDeploy;
using OpenCvSharp;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static AVS.Global;

namespace AVS
{
    public static class AiDrive
    {
        static readonly string deviceName = "cuda";
        //***********************************************************************************************************************
        //-A
        public static string[] segModelPathsA;
        //static List<string> imagePaths;
        public static List<Segmentor> segHandlesA;

        public static string[] detectModelPathsA;
        //public static List<string> detectImagePaths;
        public static List<Detector> detectHandlesA;
        //***********************************************************************************************************************
        //-B
        public static string[] segModelPathsB;
        //static List<string> imagePaths;
        public static List<Segmentor> segHandlesB;

        public static string[] detectModelPathsB;
        //public static List<string> detectImagePaths;
        public static List<Detector> detectHandlesB;
        //***********************************************************************************************************************

        public static bool LoadAiSegModel(string modelSide, string[] modelPaths)
        {
            try
            {
                if (modelSide == "A")
                {
                    segHandlesA = new List<Segmentor>();
                    if (segHandlesA != null)
                    {
                        segHandlesA.ForEach(x => x.Close());
                        segHandlesA.Clear();
                    }
                    int modelNum = modelPaths.Count();
                    for (int i = 0; i < modelNum; i++)
                    {
                        // 创建Segmentor实例：模型路径 + 设备 + 设备ID
                        segHandlesA.Add(new Segmentor(modelPaths[i], deviceName, 0));
                    }
                    //segModelPaths.ForEach(x => segHandles.Add(new Segmentor(x, deviceName, 0)));
                    return true;
                }
                else if(modelSide == "B")
                {
                    segHandlesB = new List<Segmentor>();
                    if (segHandlesB != null)
                    {
                        segHandlesB.ForEach(x => x.Close());
                        segHandlesB.Clear();
                    }
                    int modelNum = modelPaths.Count();
                    for (int i = 0; i < modelNum; i++)
                    {
                        segHandlesB.Add(new Segmentor(modelPaths[i], deviceName, 0));
                    }
                    //segModelPaths.ForEach(x => segHandles.Add(new Segmentor(x, deviceName, 0)));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        public static bool LoadAiDetModel(string modelSide, string[] modelPaths)
        {
            try
            {
                if(modelSide == "A")
                {
                    detectHandlesA = new List<Detector>();
                    if (detectHandlesA != null)
                    {
                        detectHandlesA.ForEach(x => x.Close());
                        detectHandlesA.Clear();
                    }
                    int modelNum = modelPaths.Count();
                    for (int i = 0; i < modelNum; i++)
                    {
                        detectHandlesA.Add(new Detector(modelPaths[i], deviceName, 0));
                    }
                    //detectModelPaths.ForEach(x => detectHandles.Add(new Segmentor(x, deviceName, 0)));
                    return true;
                }
                else if (modelSide == "B")
                {
                    detectHandlesB = new List<Detector>();
                    if (detectHandlesB != null)
                    {
                        detectHandlesB.ForEach(x => x.Close());
                        detectHandlesB.Clear();
                    }
                    int modelNum = modelPaths.Count();
                    for (int i = 0; i < modelNum; i++)
                    {
                        detectHandlesB.Add(new Detector(modelPaths[i], deviceName, 0));
                    }
                    //detectModelPaths.ForEach(x => detectHandles.Add(new Segmentor(x, deviceName, 0)));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 图像推理分割
        /// </summary>
        /// <param name="modelNum"></param>
        /// <param name="originGrayImg"></param>
        /// <param name="imgMask"></param>
        public static void PredictImage(string modelSide, int modelNum, HObject imgGray, out HObject imgMask)
        {
            HOperatorSet.GenEmptyObj(out imgMask); 

            try
            {
                // 步骤1：Halcon图像 → MMDeploy.Mat
                Halcon2MmMat(imgGray, out var mats);
                // 步骤2：执行推理
                List<SegmentorOutput> output = null;
                if(modelSide == "A")
                {
                    output = segHandlesA[modelNum].Apply(mats);
                }
                else if(modelSide == "B")
                {
                    output = segHandlesB[modelNum].Apply(mats);
                }
                else
                {
                    return;
                }
                for (int j = 0; j < 15; j++)
                {
                    //var d1 = DateTime.Now;
                    //output = segHandles[modelNum].Apply(mats);
                    //Console.WriteLine("Predict Time=" + DateTime.Now.Subtract(d1).TotalMilliseconds);
                    //System.Threading.Thread.Sleep(3000);
                }
                // 推理结果 → 彩色Mask
                ResultToColorMask(output[0], out OpenCvSharp.Mat colorMask);
                // 步骤4：Mat → Halcon图像
                Mat2HalconRgb(colorMask, out imgMask);
                colorMask.Dispose();
            }
            catch
            {
                HOperatorSet.GenEmptyObj(out imgMask);
            }
        }

        /// <summary>
        /// 图像目标检测
        /// </summary>
        /// <param name="modelNum"></param>
        /// <param name="imgGray"></param>
        /// <param name="targetRect"></param>
        public static void DetectImages(string modelSide, int modelNum, HObject imgGray, double score, out int[] targetLabel, out HTuple targetRect)
        {

            try
            {
                targetRect = new HTuple();
                // 用于方形模组，检测多个焊缝
                targetLabel = new int[2] { -1, -1 };
                //float score = 0.1f;
                int i = 0;
                Halcon2MmMat(imgGray, out var mats);

                List<DetectorOutput> output = null;
                if (modelSide == "A")
                {
                    output = detectHandlesA[modelNum].Apply(mats);
                }
                else if (modelSide == "B")
                {
                    output = detectHandlesB[modelNum].Apply(mats);
                }
                else
                {
                    return;
                }

                if (output == null || output.Count == 0 || output[0].Results == null)
                {
                    AddLog("AI检测输出为空，未检测到目标！");
                    targetLabel = new int[2] { -1, -1 };
                    targetRect = new HTuple();
                    return;
                }

                foreach (var obj in output[0].Results)
                {
                    //int num = output[0].Results.Count;
                    //Vec3b[] palette = GenPalette(num);

                    if (obj.Score > 0.7)
                    {
                        score = obj.Score;
                        targetLabel[i] = obj.LabelId;

                        float x1 = Math.Max((float)Math.Floor(obj.BBox.Top) - 1, 0f);
                        float y1 = Math.Max((float)Math.Floor(obj.BBox.Left) - 1, 0f);

                        float x2 = Math.Max((float)Math.Floor(obj.BBox.Bottom) - 1, 0f);
                        float y2 = Math.Max((float)Math.Floor(obj.BBox.Right) - 1, 0f);

                        double xC = (x1 + x2) * 0.5;
                        double yC = (y1 + y2) * 0.5;

                        double ra = Math.Abs(x1 - x2) * 0.5;
                        double rb = Math.Abs(y1 - y2) * 0.5;

                        HOperatorSet.TupleConcat(targetRect, x1, out targetRect);
                        HOperatorSet.TupleConcat(targetRect, y1, out targetRect);
                        HOperatorSet.TupleConcat(targetRect, x2, out targetRect);
                        HOperatorSet.TupleConcat(targetRect, y2, out targetRect);
                        i++;
                    }                   
                }
            }
            catch
            {
                targetLabel = new int[2] { -1, -1 };
                targetRect = new HTuple();
            }
        }
        /// <summary>
        /// 图像目标检测
        /// </summary>
        /// <param name="modelNum"></param>
        /// <param name="imgGray"></param>
        /// <param name="targetRect"></param>
        public static void DetectImage(string modelSide,int modelNum, HObject imgGray,double score, out int targetLabel, out HTuple targetRect)
        {

            try
            {
                targetRect = new HTuple();
                targetLabel = -1;
                //float score = 0.1f;

                Halcon2MmMat(imgGray, out var mats);

                List<DetectorOutput> output = null;
                if (modelSide == "A")
                {
                    output = detectHandlesA[modelNum].Apply(mats);
                }
                else if (modelSide == "B")
                {
                    output = detectHandlesB[modelNum].Apply(mats);
                }
                else
                {
                    return;
                }

                if (output == null || output.Count == 0 || output[0].Results == null)
                {
                    AddLog("AI检测输出为空，未检测到目标！");
                    targetLabel = -1;
                    targetRect = new HTuple();
                    return;
                }

                foreach (var obj in output[0].Results)
                {
                    //int num = output[0].Results.Count;
                    //Vec3b[] palette = GenPalette(num);

                    if (obj.Score > 0.7)
                    {
                        score = obj.Score;
                        targetLabel = obj.LabelId;
                        targetRect = new HTuple();

                        float x1 = Math.Max((float)Math.Floor(obj.BBox.Top) - 1, 0f);
                        float y1 = Math.Max((float)Math.Floor(obj.BBox.Left) - 1, 0f);

                        float x2 = Math.Max((float)Math.Floor(obj.BBox.Bottom) - 1, 0f);
                        float y2 = Math.Max((float)Math.Floor(obj.BBox.Right) - 1, 0f);

                        double xC = (x1 + x2) * 0.5;
                        double yC = (y1 + y2) * 0.5;

                        double ra = Math.Abs(x1 - x2) * 0.5;
                        double rb = Math.Abs(y1 - y2) * 0.5;

                        HOperatorSet.TupleConcat(targetRect, x1, out targetRect);
                        HOperatorSet.TupleConcat(targetRect, y1, out targetRect);
                        HOperatorSet.TupleConcat(targetRect, x2, out targetRect);
                        HOperatorSet.TupleConcat(targetRect, y2, out targetRect);
                    }
                }               
            }
            catch
            {
                targetLabel = -1;
                targetRect = new HTuple();
            }
        }


        private static void CvMatToMat(OpenCvSharp.Mat[] cvMats, out MMDeploy.Mat[] mats)
        {
            mats = new MMDeploy.Mat[cvMats.Length];
            unsafe
            {
                for (int i = 0; i < cvMats.Length; i++)
                {
                    mats[i].Data = cvMats[i].DataPointer;
                    mats[i].Height = cvMats[i].Height;
                    mats[i].Width = cvMats[i].Width;
                    mats[i].Channel = cvMats[i].Dims();
                    mats[i].Format = PixelFormat.BGR;
                    mats[i].Type = DataType.Int8;
                    mats[i].Device = null;
                }
            }
        }

        private static void Halcon2MmMat(HObject imgGray, out MMDeploy.Mat[] mats)
        {
            HOperatorSet.CountChannels(imgGray, out HTuple chs);
            if ((int)chs.D == 1)
            {
                mats = new MMDeploy.Mat[1];
                unsafe
                {
                    HTuple ptrGray, type, width, height;

                    HOperatorSet.GetImagePointer1(imgGray, out ptrGray, out type, out width, out height);

                    IntPtr ptr2 = ptrGray;
                    int bytes = width * height;
                    byte[] rgbvalues = new byte[bytes];
                    System.Runtime.InteropServices.Marshal.Copy(ptr2, rgbvalues, 0, bytes);
                    OpenCvSharp.Mat mat = new OpenCvSharp.Mat(height, width, MatType.CV_8UC1, rgbvalues);

                    //OpenCvSharp.Cv2.ImWrite("D:\\h2c.jpg", mat);

                    for (int i = 0; i < 1; i++)
                    {
                        mats[i].Data = mat.DataPointer;
                        mats[i].Height = mat.Height;
                        mats[i].Width = mat.Width;
                        mats[i].Channel = mat.Dims();
                        mats[i].Format = PixelFormat.Grayscale;
                        mats[i].Type = DataType.Int8;
                        mats[i].Device = null;
                    }
                }
            }
            else if ((int)chs.D == 3)
            {
                HTuple ptrRed, ptrGreen, ptrBlue, type, width, height;

                HOperatorSet.GetImagePointer3(imgGray, out ptrRed, out ptrGreen, out ptrBlue, out type, out width, out height);

                int bytes = width * height * 3;
                byte[] rgbvalues = new byte[bytes];

                unsafe
                {
                    IntPtr ptrR = ptrRed;
                    IntPtr ptrG = ptrGreen;
                    IntPtr ptrB = ptrBlue;

                    byte* r = (byte*)(ptrR);
                    byte* g = (byte*)(ptrG);
                    byte* b = (byte*)(ptrB);

                    int lengh = width * height;
                    for (int i = 0; i < lengh; i++)
                    {
                        rgbvalues[i * 3 + 0] = (b)[i];
                        rgbvalues[i * 3 + 1] = (g)[i];
                        rgbvalues[i * 3 + 2] = (r)[i];
                        //bptr[i * 4 + 3] = 255;
                    }
                }
                OpenCvSharp.Mat mat = new OpenCvSharp.Mat(height, width, MatType.CV_8UC3, rgbvalues);

                mats = new MMDeploy.Mat[1];
                unsafe
                {
                    for (int i = 0; i < 1; i++)
                    {
                        mats[i].Data = mat.DataPointer;
                        mats[i].Height = mat.Height;
                        mats[i].Width = mat.Width;
                        mats[i].Channel = mat.Dims();
                        mats[i].Format = PixelFormat.BGR;
                        mats[i].Type = DataType.Int8;
                        mats[i].Device = null;
                    }
                }
            }
            else
            {
                mats = new MMDeploy.Mat[1];
            }
        }

        private static void ResultToColorMask(SegmentorOutput output, out OpenCvSharp.Mat colorMask)
        {
            colorMask = new OpenCvSharp.Mat(output.Height, output.Width, MatType.CV_8UC3, new Scalar());
            Vec3b[] palette = GenPalette(output.Classes);
            unsafe
            {
                byte* data = colorMask.DataPointer;
                if (output.Mask.Length > 0)
                {
                    fixed (int* _label = output.Mask)
                    {
                        int* label = _label;
                        for (int i = 0; i < output.Height; i++)
                        {
                            for (int j = 0; j < output.Width; j++)
                            {
                                data[0] = palette[*label][0];
                                data[1] = palette[*label][1];
                                data[2] = palette[*label][2];
                                data += 3;
                                label++;
                            }
                        }
                    }
                }
                else
                {
                    //int pos = 0;
                    fixed (float* _score = output.Score)
                    {
                        float* score = _score;
                        int total = output.Height * output.Width;
                        for (int i = 0; i < output.Height; i++)
                        {
                            for (int j = 0; j < output.Width; j++)
                            {
                                List<Tuple<float, int>> scores = new List<Tuple<float, int>>();
                                for (int k = 0; k < output.Classes; k++)
                                {
                                    scores.Add(new Tuple<float, int>(score[k * total + i * output.Width + j], k));
                                }
                                scores.Sort();
                                var lastScore = scores.Last();
                                data[0] = palette[lastScore.Item2][0];
                                data[1] = palette[lastScore.Item2][1];
                                data[2] = palette[lastScore.Item2][2];
                                data += 3;
                            }
                        }
                    }
                }
            }
        }

        private static Vec3b[] GenPalette(int classes)
        {
            Random rnd = new Random(0);
            Vec3b[] palette = new Vec3b[classes];
            for (int i = 0; i < classes; i++)
            {
                byte v1 = (byte)rnd.Next(0, 255);
                byte v2 = (byte)rnd.Next(0, 255);
                byte v3 = (byte)rnd.Next(0, 255);
                palette[i] = new Vec3b(v1, v2, v3);
            }
            return palette;
        }

        public static void Mat2HalconRgb(OpenCvSharp.Mat mat, out HObject image)
        {
            int ImageWidth = mat.Width;
            int ImageHeight = mat.Height;
            int channel = mat.Channels();
            long size = ImageWidth * ImageHeight * channel;
            int col_byte_num = ImageWidth * channel;

            byte[] rgbValues = new byte[size];
            //IntPtr imgptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(rgbValues.Length);
            unsafe
            {
                for (int i = 0; i < mat.Height; i++)
                {
                    IntPtr c = mat.Ptr(i);
                    //byte* c1 = (byte*)c;
                    System.Runtime.InteropServices.Marshal.Copy(c, rgbValues, i * col_byte_num, col_byte_num);
                }

                void* p;
                IntPtr ptr;
                fixed (byte* pc = rgbValues)
                {
                    p = (void*)pc;
                    ptr = new IntPtr(p);
                }
                HOperatorSet.GenImageInterleaved(out image, ptr, "bgr", ImageWidth, ImageHeight, 0, "byte", 0, 0, 0, 0, -1, 0);
            }
        }
    }
}

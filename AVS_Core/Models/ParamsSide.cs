using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_Core.Models
{
    public struct Control_Param
    {
        public double a;                //
        public double b;                //
        public double c;                //
    }

    public struct ImgProcess_Param
    {
        public double resolutionX;
        public double resolutionY;
        public double resolutionZ;
        public double pngRoiHih;
        public double pngRoiLow;
        public double imgScaleMul;
        public double hv_scaleAdd;
    }

    public struct Calibrate_Param
    {
        public double rectLengthMin;
        public double rectLengthMax;
        public double angleTolerance;
        public double psnTolerance;
        public double fx;
        public double fy;
        public double fz;
        public double cornerX01;
        public double cornerY01;
        public double cornerX02;
        public double cornerY02;
        public double cornerX03;
        public double cornerY03;
        public double cornerX04;
        public double cornerY04;
    }

    public struct CamParam
    {
        public string addressIp;         //Camera IP 地址
        public bool isImgSourceLocal;    //图像源是否位相机，否的话为本地
        public string addressImg;        //本地图像地址
        public int exporsure;            //Camera 曝光时长
        public int gain;                 //Camera 曝光时长
        public int CameraKind;           //0 海康相机   1 basler相机
    }
    public struct score
    {
        public double scoreValue;         //分数值
    }

    public struct ImgSaveParam
    {
        public bool isSaveOrnImg;        //是否保存原图
        public bool isSaveOkRenImg;        //是否保存OK结果图
        public bool isSaveNgRenImg;        //是否保存NG结果图
        public int saveOrnImgDays;         //保存原图图片天数
        public int saveRenImgDays;         //保存结果图图片天数
        public string imgSaveFolder;    //处理图像保存主路径
        public bool isPlanecheck;       //启用3D拟合平面
        public int radio;               //原图压缩比
        public string format;           //原图存储图像格式
        public bool isSquareBarWeldMark;      //方-条焊印
        public bool isCirWeldMark;      //圆形焊印
    }

    public struct ModelSearch_Param
    {
        public double angleStart;   //起始角度__deg
        public double angleExtent;  //角度范围__deg
        public double minScale;     //最小缩放
        public double maxScale;     //最大缩放
        public double minScore;     //最小分数
        public int maxMatchNum;     //最大匹配数目
        public double maxOverlap;   //最大重叠
        public int numLevel;        //最大金子塔层级
        public double greediness;   //贪心值
        public string subPixel;     //亚像素
    }

    public struct InspectOrder
    {
        public int row;//排数
        public int col;//列数
        [JsonIgnore]
        public int[] start;
        [JsonIgnore]
        public int[] end;
    }


    public class ParamsSide
    {
        public Control_Param ctrParam = new Control_Param();

        public ImgSaveParam imgSaveParam = new ImgSaveParam();

        public CamParam camParam = new CamParam();

        public Calibrate_Param calParam = new Calibrate_Param();

        public score score = new score();

        public ModelSearch_Param[] searchParam = new ModelSearch_Param[9];

        public InspectOrder[] inspectOrders = new InspectOrder[20];

        public bool isNormalCheck;
        public bool isAiCheck;
        public bool isRotated;
        public string[] segModelPath = new string[6] { "xxx", "xxx", "xxx", "xxx", "xxx", "xxx" };
        public string[] detModelPath = new string[6] { "xxx", "xxx", "xxx", "xxx", "xxx", "xxx" };

    }
}

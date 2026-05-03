using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;

namespace AVS
{
    public class HalconExport2D
    {
        HObject Regions = null;
        HTuple ProductParamNamee;
        HTuple ProductParamValue;
        HTuple ParamCata;
        HTuple ParamSpec;
        HTuple ImageParamNamee;
        HTuple ImageParamValue;
        HTuple MatchModels;
        HTuple MetroModels;
        HTuple DispRow01;
        HTuple DispCol01;
        HTuple Interval01;
        HTuple DispRow02;
        HTuple DispCol02;
        HTuple Interval02;
        HTuple searchParams;
        HTuple DispColor;
        HTuple DispFont;
        HTuple DispSize;
        HObject ExpGetGlobalVar_Regions()
        {
            return Regions;
        }
        void ExpSetGlobalVar_Regions(HObject obj)
        {
            if (Regions != null)
                Regions.Dispose();
            Regions = obj;
        }

        HTuple ExpGetGlobalVar_ProductParamNamee()
        {
            return ProductParamNamee;
        }
        void ExpSetGlobalVar_ProductParamNamee(HTuple val)
        {
            ProductParamNamee = val;
        }

        HTuple ExpGetGlobalVar_ProductParamValue()
        {
            return ProductParamValue;
        }
        void ExpSetGlobalVar_ProductParamValue(HTuple val)
        {
            ProductParamValue = val;
        }

        HTuple ExpGetGlobalVar_ParamCata()
        {
            return ParamCata;
        }
        void ExpSetGlobalVar_ParamCata(HTuple val)
        {
            ParamCata = val;
        }

        HTuple ExpGetGlobalVar_ParamSpec()
        {
            return ParamSpec;
        }
        void ExpSetGlobalVar_ParamSpec(HTuple val)
        {
            ParamSpec = val;
        }

        HTuple ExpGetGlobalVar_ImageParamNamee()
        {
            return ImageParamNamee;
        }
        void ExpSetGlobalVar_ImageParamNamee(HTuple val)
        {
            ImageParamNamee = val;
        }

        HTuple ExpGetGlobalVar_ImageParamValue()
        {
            return ImageParamValue;
        }
        void ExpSetGlobalVar_ImageParamValue(HTuple val)
        {
            ImageParamValue = val;
        }

        HTuple ExpGetGlobalVar_MatchModels()
        {
            return MatchModels;
        }
        void ExpSetGlobalVar_MatchModels(HTuple val)
        {
            MatchModels = val;
        }

        HTuple ExpGetGlobalVar_MetroModels()
        {
            return MetroModels;
        }
        void ExpSetGlobalVar_MetroModels(HTuple val)
        {
            MetroModels = val;
        }

        HTuple ExpGetGlobalVar_DispRow01()
        {
            return DispRow01;
        }
        void ExpSetGlobalVar_DispRow01(HTuple val)
        {
            DispRow01 = val;
        }

        HTuple ExpGetGlobalVar_DispCol01()
        {
            return DispCol01;
        }
        void ExpSetGlobalVar_DispCol01(HTuple val)
        {
            DispCol01 = val;
        }

        HTuple ExpGetGlobalVar_Interval01()
        {
            return Interval01;
        }
        void ExpSetGlobalVar_Interval01(HTuple val)
        {
            Interval01 = val;
        }

        HTuple ExpGetGlobalVar_DispRow02()
        {
            return DispRow02;
        }
        void ExpSetGlobalVar_DispRow02(HTuple val)
        {
            DispRow02 = val;
        }

        HTuple ExpGetGlobalVar_DispCol02()
        {
            return DispCol02;
        }
        void ExpSetGlobalVar_DispCol02(HTuple val)
        {
            DispCol02 = val;
        }

        HTuple ExpGetGlobalVar_Interval02()
        {
            return Interval02;
        }
        void ExpSetGlobalVar_Interval02(HTuple val)
        {
            Interval02 = val;
        }

        HTuple ExpGetGlobalVar_searchParams()
        {
            return searchParams;
        }
        void ExpSetGlobalVar_searchParams(HTuple val)
        {
            searchParams = val;
        }

        HTuple ExpGetGlobalVar_DispColor()
        {
            return DispColor;
        }
        void ExpSetGlobalVar_DispColor(HTuple val)
        {
            DispColor = val;
        }

        HTuple ExpGetGlobalVar_DispFont()
        {
            return DispFont;
        }
        void ExpSetGlobalVar_DispFont(HTuple val)
        {
            DispFont = val;
        }

        HTuple ExpGetGlobalVar_DispSize()
        {
            return DispSize;
        }
        void ExpSetGlobalVar_DispSize(HTuple val)
        {
            DispSize = val;
        }

        // Procedures 
        // External procedures 
        // Chapter: Develop
        // Short Description: Open a new graphics window that preserves the aspect ratio of the given image. 
        public void dev_open_window_fit_image(HObject ho_Image, HTuple hv_Row, HTuple hv_Column,
            HTuple hv_WidthLimit, HTuple hv_HeightLimit, out HTuple hv_WindowHandle)
        {




            // Local iconic variables 

            // Local control variables 

            HTuple hv_MinWidth = new HTuple(), hv_MaxWidth = new HTuple();
            HTuple hv_MinHeight = new HTuple(), hv_MaxHeight = new HTuple();
            HTuple hv_ResizeFactor = null, hv_ImageWidth = null, hv_ImageHeight = null;
            HTuple hv_TempWidth = null, hv_TempHeight = null, hv_WindowWidth = null;
            HTuple hv_WindowHeight = null;
            // Initialize local and output iconic variables 
            //This procedure opens a new graphics window and adjusts the size
            //such that it fits into the limits specified by WidthLimit
            //and HeightLimit, but also maintains the correct image aspect ratio.
            //
            //If it is impossible to match the minimum and maximum extent requirements
            //at the same time (f.e. if the image is very long but narrow),
            //the maximum value gets a higher priority,
            //
            //Parse input tuple WidthLimit
            if ((int)((new HTuple((new HTuple(hv_WidthLimit.TupleLength())).TupleEqual(0))).TupleOr(
                new HTuple(hv_WidthLimit.TupleLess(0)))) != 0)
            {
                hv_MinWidth = 500;
                hv_MaxWidth = 800;
            }
            else if ((int)(new HTuple((new HTuple(hv_WidthLimit.TupleLength())).TupleEqual(
                1))) != 0)
            {
                hv_MinWidth = 0;
                hv_MaxWidth = hv_WidthLimit.Clone();
            }
            else
            {
                hv_MinWidth = hv_WidthLimit.TupleSelect(0);
                hv_MaxWidth = hv_WidthLimit.TupleSelect(1);
            }
            //Parse input tuple HeightLimit
            if ((int)((new HTuple((new HTuple(hv_HeightLimit.TupleLength())).TupleEqual(0))).TupleOr(
                new HTuple(hv_HeightLimit.TupleLess(0)))) != 0)
            {
                hv_MinHeight = 400;
                hv_MaxHeight = 600;
            }
            else if ((int)(new HTuple((new HTuple(hv_HeightLimit.TupleLength())).TupleEqual(
                1))) != 0)
            {
                hv_MinHeight = 0;
                hv_MaxHeight = hv_HeightLimit.Clone();
            }
            else
            {
                hv_MinHeight = hv_HeightLimit.TupleSelect(0);
                hv_MaxHeight = hv_HeightLimit.TupleSelect(1);
            }
            //
            //Test, if window size has to be changed.
            hv_ResizeFactor = 1;
            HOperatorSet.GetImageSize(ho_Image, out hv_ImageWidth, out hv_ImageHeight);
            //First, expand window to the minimum extents (if necessary).
            if ((int)((new HTuple(hv_MinWidth.TupleGreater(hv_ImageWidth))).TupleOr(new HTuple(hv_MinHeight.TupleGreater(
                hv_ImageHeight)))) != 0)
            {
                hv_ResizeFactor = (((((hv_MinWidth.TupleReal()) / hv_ImageWidth)).TupleConcat(
                    (hv_MinHeight.TupleReal()) / hv_ImageHeight))).TupleMax();
            }
            hv_TempWidth = hv_ImageWidth * hv_ResizeFactor;
            hv_TempHeight = hv_ImageHeight * hv_ResizeFactor;
            //Then, shrink window to maximum extents (if necessary).
            if ((int)((new HTuple(hv_MaxWidth.TupleLess(hv_TempWidth))).TupleOr(new HTuple(hv_MaxHeight.TupleLess(
                hv_TempHeight)))) != 0)
            {
                hv_ResizeFactor = hv_ResizeFactor * ((((((hv_MaxWidth.TupleReal()) / hv_TempWidth)).TupleConcat(
                    (hv_MaxHeight.TupleReal()) / hv_TempHeight))).TupleMin());
            }
            hv_WindowWidth = hv_ImageWidth * hv_ResizeFactor;
            hv_WindowHeight = hv_ImageHeight * hv_ResizeFactor;
            //Resize window
            HOperatorSet.SetWindowAttr("background_color", "black");
            HOperatorSet.OpenWindow(hv_Row, hv_Column, hv_WindowWidth, hv_WindowHeight, 0, "visible", "", out hv_WindowHandle);
            HDevWindowStack.Push(hv_WindowHandle);
            if (HDevWindowStack.IsOpen())
            {
                HOperatorSet.SetPart(HDevWindowStack.GetActive(), 0, 0, hv_ImageHeight - 1, hv_ImageWidth - 1);
            }

            return;
        }

        // Chapter: Filters / Arithmetic
        // Short Description: Scale the gray values of an image from the interval [Min,Max] to [0,255] 
        public void scale_image_range(HObject ho_Image, out HObject ho_ImageScaled, HTuple hv_Min,
            HTuple hv_Max)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_ImageSelected = null, ho_SelectedChannel = null;
            HObject ho_LowerRegion = null, ho_UpperRegion = null, ho_ImageSelectedScaled = null;

            // Local copy input parameter variables 
            HObject ho_Image_COPY_INP_TMP;
            ho_Image_COPY_INP_TMP = ho_Image.CopyObj(1, -1);



            // Local control variables 

            HTuple hv_LowerLimit = new HTuple(), hv_UpperLimit = new HTuple();
            HTuple hv_Mult = null, hv_Add = null, hv_NumImages = null;
            HTuple hv_ImageIndex = null, hv_Channels = new HTuple();
            HTuple hv_ChannelIndex = new HTuple(), hv_MinGray = new HTuple();
            HTuple hv_MaxGray = new HTuple(), hv_Range = new HTuple();
            HTuple hv_Max_COPY_INP_TMP = hv_Max.Clone();
            HTuple hv_Min_COPY_INP_TMP = hv_Min.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_ImageSelected);
            HOperatorSet.GenEmptyObj(out ho_SelectedChannel);
            HOperatorSet.GenEmptyObj(out ho_LowerRegion);
            HOperatorSet.GenEmptyObj(out ho_UpperRegion);
            HOperatorSet.GenEmptyObj(out ho_ImageSelectedScaled);
            try
            {
                //Convenience procedure to scale the gray values of the
                //input image Image from the interval [Min,Max]
                //to the interval [0,255] (default).
                //Gray values < 0 or > 255 (after scaling) are clipped.
                //
                //If the image shall be scaled to an interval different from [0,255],
                //this can be achieved by passing tuples with 2 values [From, To]
                //as Min and Max.
                //Example:
                //scale_image_range(Image:ImageScaled:[100,50],[200,250])
                //maps the gray values of Image from the interval [100,200] to [50,250].
                //All other gray values will be clipped.
                //
                //input parameters:
                //Image: the input image
                //Min: the minimum gray value which will be mapped to 0
                //     If a tuple with two values is given, the first value will
                //     be mapped to the second value.
                //Max: The maximum gray value which will be mapped to 255
                //     If a tuple with two values is given, the first value will
                //     be mapped to the second value.
                //
                //Output parameter:
                //ImageScale: the resulting scaled image.
                //
                if ((int)(new HTuple((new HTuple(hv_Min_COPY_INP_TMP.TupleLength())).TupleEqual(
                    2))) != 0)
                {
                    hv_LowerLimit = hv_Min_COPY_INP_TMP.TupleSelect(1);
                    hv_Min_COPY_INP_TMP = hv_Min_COPY_INP_TMP.TupleSelect(0);
                }
                else
                {
                    hv_LowerLimit = 0.0;
                }
                if ((int)(new HTuple((new HTuple(hv_Max_COPY_INP_TMP.TupleLength())).TupleEqual(
                    2))) != 0)
                {
                    hv_UpperLimit = hv_Max_COPY_INP_TMP.TupleSelect(1);
                    hv_Max_COPY_INP_TMP = hv_Max_COPY_INP_TMP.TupleSelect(0);
                }
                else
                {
                    hv_UpperLimit = 255.0;
                }
                //
                //Calculate scaling parameters.
                hv_Mult = (((hv_UpperLimit - hv_LowerLimit)).TupleReal()) / (hv_Max_COPY_INP_TMP - hv_Min_COPY_INP_TMP);
                hv_Add = ((-hv_Mult) * hv_Min_COPY_INP_TMP) + hv_LowerLimit;
                //
                //Scale image.
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ScaleImage(ho_Image_COPY_INP_TMP, out ExpTmpOutVar_0, hv_Mult,
                        hv_Add);
                    ho_Image_COPY_INP_TMP.Dispose();
                    ho_Image_COPY_INP_TMP = ExpTmpOutVar_0;
                }
                //
                //Clip gray values if necessary.
                //This must be done for each image and channel separately.
                ho_ImageScaled.Dispose();
                HOperatorSet.GenEmptyObj(out ho_ImageScaled);
                HOperatorSet.CountObj(ho_Image_COPY_INP_TMP, out hv_NumImages);
                HTuple end_val49 = hv_NumImages;
                HTuple step_val49 = 1;
                for (hv_ImageIndex = 1; hv_ImageIndex.Continue(end_val49, step_val49); hv_ImageIndex = hv_ImageIndex.TupleAdd(step_val49))
                {
                    ho_ImageSelected.Dispose();
                    HOperatorSet.SelectObj(ho_Image_COPY_INP_TMP, out ho_ImageSelected, hv_ImageIndex);
                    HOperatorSet.CountChannels(ho_ImageSelected, out hv_Channels);
                    HTuple end_val52 = hv_Channels;
                    HTuple step_val52 = 1;
                    for (hv_ChannelIndex = 1; hv_ChannelIndex.Continue(end_val52, step_val52); hv_ChannelIndex = hv_ChannelIndex.TupleAdd(step_val52))
                    {
                        ho_SelectedChannel.Dispose();
                        HOperatorSet.AccessChannel(ho_ImageSelected, out ho_SelectedChannel, hv_ChannelIndex);
                        HOperatorSet.MinMaxGray(ho_SelectedChannel, ho_SelectedChannel, 0, out hv_MinGray,
                            out hv_MaxGray, out hv_Range);
                        ho_LowerRegion.Dispose();
                        HOperatorSet.Threshold(ho_SelectedChannel, out ho_LowerRegion, ((hv_MinGray.TupleConcat(
                            hv_LowerLimit))).TupleMin(), hv_LowerLimit);
                        ho_UpperRegion.Dispose();
                        HOperatorSet.Threshold(ho_SelectedChannel, out ho_UpperRegion, hv_UpperLimit,
                            ((hv_UpperLimit.TupleConcat(hv_MaxGray))).TupleMax());
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.PaintRegion(ho_LowerRegion, ho_SelectedChannel, out ExpTmpOutVar_0,
                                hv_LowerLimit, "fill");
                            ho_SelectedChannel.Dispose();
                            ho_SelectedChannel = ExpTmpOutVar_0;
                        }
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.PaintRegion(ho_UpperRegion, ho_SelectedChannel, out ExpTmpOutVar_0,
                                hv_UpperLimit, "fill");
                            ho_SelectedChannel.Dispose();
                            ho_SelectedChannel = ExpTmpOutVar_0;
                        }
                        if ((int)(new HTuple(hv_ChannelIndex.TupleEqual(1))) != 0)
                        {
                            ho_ImageSelectedScaled.Dispose();
                            HOperatorSet.CopyObj(ho_SelectedChannel, out ho_ImageSelectedScaled,
                                1, 1);
                        }
                        else
                        {
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.AppendChannel(ho_ImageSelectedScaled, ho_SelectedChannel,
                                    out ExpTmpOutVar_0);
                                ho_ImageSelectedScaled.Dispose();
                                ho_ImageSelectedScaled = ExpTmpOutVar_0;
                            }
                        }
                    }
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_ImageScaled, ho_ImageSelectedScaled, out ExpTmpOutVar_0
                            );
                        ho_ImageScaled.Dispose();
                        ho_ImageScaled = ExpTmpOutVar_0;
                    }
                }
                ho_Image_COPY_INP_TMP.Dispose();
                ho_ImageSelected.Dispose();
                ho_SelectedChannel.Dispose();
                ho_LowerRegion.Dispose();
                ho_UpperRegion.Dispose();
                ho_ImageSelectedScaled.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Image_COPY_INP_TMP.Dispose();
                ho_ImageSelected.Dispose();
                ho_SelectedChannel.Dispose();
                ho_LowerRegion.Dispose();
                ho_UpperRegion.Dispose();
                ho_ImageSelectedScaled.Dispose();

                throw HDevExpDefaultException;
            }
        }

        // Chapter: File / Misc
        // Short Description: Parse a filename into directory, base filename, and extension 
        public void parse_filename(HTuple hv_FileName, out HTuple hv_BaseName, out HTuple hv_Extension,
            out HTuple hv_Directory)
        {



            // Local control variables 

            HTuple hv_DirectoryTmp = null, hv_Substring = null;
            // Initialize local and output iconic variables 
            //This procedure gets a filename (with full path) as input
            //and returns the directory path, the base filename and the extension
            //in three different strings.
            //
            //In the output path the path separators will be replaced
            //by '/' in all cases.
            //
            //The procedure shows the possibilities of regular expressions in HALCON.
            //
            //Input parameters:
            //FileName: The input filename
            //
            //Output parameters:
            //BaseName: The filename without directory description and file extension
            //Extension: The file extension
            //Directory: The directory path
            //
            //Example:
            //basename('C:/images/part_01.png',...) returns
            //BaseName = 'part_01'
            //Extension = 'png'
            //Directory = 'C:\\images\\' (on Windows systems)
            //
            //Explanation of the regular expressions:
            //
            //'([^\\\\/]*?)(?:\\.[^.]*)?$':
            //To start at the end, the '$' matches the end of the string,
            //so it is best to read the expression from right to left.
            //The part in brackets (?:\\.[^.}*) denotes a non-capturing group.
            //That means, that this part is matched, but not captured
            //in contrast to the first bracketed group ([^\\\\/], see below.)
            //\\.[^.]* matches a dot '.' followed by as many non-dots as possible.
            //So (?:\\.[^.]*)? matches the file extension, if any.
            //The '?' at the end assures, that even if no extension exists,
            //a correct match is returned.
            //The first part in brackets ([^\\\\/]*?) is a capture group,
            //which means, that if a match is found, only the part in
            //brackets is returned as a result.
            //Because both HDevelop strings and regular expressions need a '\\'
            //to describe a backslash, inside regular expressions within HDevelop
            //a backslash has to be written as '\\\\'.
            //[^\\\\/] matches any character but a slash or backslash ('\\' in HDevelop)
            //[^\\\\/]*? matches a string od 0..n characters (except '/' or '\\')
            //where the '?' after the '*' switches the greediness off,
            //that means, that the shortest possible match is returned.
            //This option is necessary to cut off the extension
            //but only if (?:\\.[^.]*)? is able to match one.
            //To summarize, the regular expression matches that part of
            //the input string, that follows after the last '/' or '\\' and
            //cuts off the extension (if any) after the last '.'.
            //
            //'\\.([^.]*)$':
            //This matches everything after the last '.' of the input string.
            //Because ([^.]) is a capturing group,
            //only the part after the dot is returned.
            //
            //'.*[\\\\/]':
            //This matches the longest substring with a '/' or a '\\' at the end.
            //
            HOperatorSet.TupleRegexpMatch(hv_FileName, ".*[\\\\/]", out hv_DirectoryTmp);
            HOperatorSet.TupleSubstr(hv_FileName, hv_DirectoryTmp.TupleStrlen(), (hv_FileName.TupleStrlen()
                ) - 1, out hv_Substring);
            HOperatorSet.TupleRegexpMatch(hv_Substring, "([^\\\\/]*?)(?:\\.[^.]*)?$", out hv_BaseName);
            HOperatorSet.TupleRegexpMatch(hv_Substring, "\\.([^.]*)$", out hv_Extension);
            //
            //
            //Finally all found backslashes ('\\') are converted
            //to a slash to get consistent paths
            HOperatorSet.TupleRegexpReplace(hv_DirectoryTmp, (new HTuple("\\\\")).TupleConcat(
                "replace_all"), "/", out hv_Directory);

            return;
        }

        // Chapter: Develop
        // Short Description: Switch dev_update_pc, dev_update_var and dev_update_window to 'off'. 
        public void dev_update_off()
        {

            // Initialize local and output iconic variables 
            //This procedure sets different update settings to 'off'.
            //This is useful to get the best performance and reduce overhead.
            //
            // dev_update_pc(...); only in hdevelop
            // dev_update_var(...); only in hdevelop
            // dev_update_window(...); only in hdevelop

            return;
        }

        // Chapter: File / Misc
        // Short Description: Get all image files under the given path 
        public void list_image_files(HTuple hv_ImageDirectory, HTuple hv_Extensions, HTuple hv_Options,
            out HTuple hv_ImageFiles)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_HalconImages = null, hv_OS = null;
            HTuple hv_Directories = null, hv_Index = null, hv_Length = null;
            HTuple hv_NetworkDrive = null, hv_Substring = new HTuple();
            HTuple hv_FileExists = new HTuple(), hv_AllFiles = new HTuple();
            HTuple hv_i = new HTuple(), hv_Selection = new HTuple();
            HTuple hv_Extensions_COPY_INP_TMP = hv_Extensions.Clone();
            HTuple hv_ImageDirectory_COPY_INP_TMP = hv_ImageDirectory.Clone();

            // Initialize local and output iconic variables 
            //This procedure returns all files in a given directory
            //with one of the suffixes specified in Extensions.
            //
            //Input parameters:
            //ImageDirectory: as the name says
            //   If a tuple of directories is given, only the images in the first
            //   existing directory are returned.
            //   If a local directory is not found, the directory is searched
            //   under %HALCONIMAGES%/ImageDirectory. If %HALCONIMAGES% is not set,
            //   %HALCONROOT%/images is used instead.
            //Extensions: A string tuple containing the extensions to be found
            //   e.g. ['png','tif',jpg'] or others
            //If Extensions is set to 'default' or the empty string '',
            //   all image suffixes supported by HALCON are used.
            //Options: as in the operator list_files, except that the 'files'
            //   option is always used. Note that the 'directories' option
            //   has no effect but increases runtime, because only files are
            //   returned.
            //
            //Output parameter:
            //ImageFiles: A tuple of all found image file names
            //
            if ((int)((new HTuple((new HTuple(hv_Extensions_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Extensions_COPY_INP_TMP.TupleEqual(""))))).TupleOr(new HTuple(hv_Extensions_COPY_INP_TMP.TupleEqual(
                "default")))) != 0)
            {
                hv_Extensions_COPY_INP_TMP = new HTuple();
                hv_Extensions_COPY_INP_TMP[0] = "ima";
                hv_Extensions_COPY_INP_TMP[1] = "tif";
                hv_Extensions_COPY_INP_TMP[2] = "tiff";
                hv_Extensions_COPY_INP_TMP[3] = "gif";
                hv_Extensions_COPY_INP_TMP[4] = "bmp";
                hv_Extensions_COPY_INP_TMP[5] = "jpg";
                hv_Extensions_COPY_INP_TMP[6] = "jpeg";
                hv_Extensions_COPY_INP_TMP[7] = "jp2";
                hv_Extensions_COPY_INP_TMP[8] = "jxr";
                hv_Extensions_COPY_INP_TMP[9] = "png";
                hv_Extensions_COPY_INP_TMP[10] = "pcx";
                hv_Extensions_COPY_INP_TMP[11] = "ras";
                hv_Extensions_COPY_INP_TMP[12] = "xwd";
                hv_Extensions_COPY_INP_TMP[13] = "pbm";
                hv_Extensions_COPY_INP_TMP[14] = "pnm";
                hv_Extensions_COPY_INP_TMP[15] = "pgm";
                hv_Extensions_COPY_INP_TMP[16] = "ppm";
                //
            }
            if ((int)(new HTuple(hv_ImageDirectory_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                hv_ImageDirectory_COPY_INP_TMP = ".";
            }
            HOperatorSet.GetSystem("image_dir", out hv_HalconImages);
            HOperatorSet.GetSystem("operating_system", out hv_OS);
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                hv_HalconImages = hv_HalconImages.TupleSplit(";");
            }
            else
            {
                hv_HalconImages = hv_HalconImages.TupleSplit(":");
            }
            hv_Directories = hv_ImageDirectory_COPY_INP_TMP.Clone();
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_HalconImages.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                hv_Directories = hv_Directories.TupleConcat(((hv_HalconImages.TupleSelect(hv_Index)) + "/") + hv_ImageDirectory_COPY_INP_TMP);
            }
            HOperatorSet.TupleStrlen(hv_Directories, out hv_Length);
            HOperatorSet.TupleGenConst(new HTuple(hv_Length.TupleLength()), 0, out hv_NetworkDrive);
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Length.TupleLength())) - 1); hv_Index = (int)hv_Index + 1)
                {
                    if ((int)(new HTuple(((((hv_Directories.TupleSelect(hv_Index))).TupleStrlen()
                        )).TupleGreater(1))) != 0)
                    {
                        HOperatorSet.TupleStrFirstN(hv_Directories.TupleSelect(hv_Index), 1, out hv_Substring);
                        if ((int)((new HTuple(hv_Substring.TupleEqual("//"))).TupleOr(new HTuple(hv_Substring.TupleEqual(
                            "\\\\")))) != 0)
                        {
                            if (hv_NetworkDrive == null)
                                hv_NetworkDrive = new HTuple();
                            hv_NetworkDrive[hv_Index] = 1;
                        }
                    }
                }
            }
            hv_ImageFiles = new HTuple();
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Directories.TupleLength()
                )) - 1); hv_Index = (int)hv_Index + 1)
            {
                HOperatorSet.FileExists(hv_Directories.TupleSelect(hv_Index), out hv_FileExists);
                if ((int)(hv_FileExists) != 0)
                {
                    HOperatorSet.ListFiles(hv_Directories.TupleSelect(hv_Index), (new HTuple("files")).TupleConcat(
                        hv_Options), out hv_AllFiles);
                    hv_ImageFiles = new HTuple();
                    for (hv_i = 0; (int)hv_i <= (int)((new HTuple(hv_Extensions_COPY_INP_TMP.TupleLength()
                        )) - 1); hv_i = (int)hv_i + 1)
                    {
                        HOperatorSet.TupleRegexpSelect(hv_AllFiles, (((".*" + (hv_Extensions_COPY_INP_TMP.TupleSelect(
                            hv_i))) + "$")).TupleConcat("ignore_case"), out hv_Selection);
                        hv_ImageFiles = hv_ImageFiles.TupleConcat(hv_Selection);
                    }
                    HOperatorSet.TupleRegexpReplace(hv_ImageFiles, (new HTuple("\\\\")).TupleConcat(
                        "replace_all"), "/", out hv_ImageFiles);
                    if ((int)(hv_NetworkDrive.TupleSelect(hv_Index)) != 0)
                    {
                        HOperatorSet.TupleRegexpReplace(hv_ImageFiles, (new HTuple("//")).TupleConcat(
                            "replace_all"), "/", out hv_ImageFiles);
                        hv_ImageFiles = "/" + hv_ImageFiles;
                    }
                    else
                    {
                        HOperatorSet.TupleRegexpReplace(hv_ImageFiles, (new HTuple("//")).TupleConcat(
                            "replace_all"), "/", out hv_ImageFiles);
                    }

                    return;
                }
            }

            return;
        }

        // Local procedures 
        public void FindMatchShape(HObject ho_img, out HObject ho_fContour, HTuple hv_matchHandle,
            HTuple hv_searchParams, out HTuple hv_fRow, out HTuple hv_fCol, out HTuple hv_fAngle,
            out HTuple hv_fScale, out HTuple hv_fScore, out HTuple hv_Length)
        {




            // Local iconic variables 

            HObject ho_modelContour = null;

            // Local control variables 

            HTuple hv_angleStart = null, hv_angleExtent = null;
            HTuple hv_scaleMin = null, hv_scaleMax = null, hv_minScore = null;
            HTuple hv_numMatch = null, hv_maxOverlap = null, hv_subPixel = null;
            HTuple hv_numLevel = null, hv_greed = null, hv_HomMat2D = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_fContour);
            HOperatorSet.GenEmptyObj(out ho_modelContour);
            try
            {
                //
                HOperatorSet.TupleSelect(hv_searchParams, 0, out hv_angleStart);
                HOperatorSet.TupleSelect(hv_searchParams, 1, out hv_angleExtent);
                HOperatorSet.TupleSelect(hv_searchParams, 2, out hv_scaleMin);
                HOperatorSet.TupleSelect(hv_searchParams, 3, out hv_scaleMax);
                HOperatorSet.TupleSelect(hv_searchParams, 4, out hv_minScore);
                //
                HOperatorSet.TupleSelect(hv_searchParams, 5, out hv_numMatch);
                HOperatorSet.TupleSelect(hv_searchParams, 6, out hv_maxOverlap);
                HOperatorSet.TupleSelect(hv_searchParams, 7, out hv_subPixel);
                HOperatorSet.TupleSelect(hv_searchParams, 8, out hv_numLevel);
                HOperatorSet.TupleSelect(hv_searchParams, 9, out hv_greed);
                //
                //多尺度模板匹配
                HOperatorSet.FindScaledShapeModel(ho_img, hv_matchHandle, hv_angleStart, hv_angleExtent,
                    hv_scaleMin, hv_scaleMax, hv_minScore, hv_numMatch, hv_maxOverlap, hv_subPixel,
                    hv_numLevel, hv_greed, out hv_fRow, out hv_fCol, out hv_fAngle, out hv_fScale,
                    out hv_fScore);
                HOperatorSet.TupleLength(hv_fScore, out hv_Length);
                if ((int)(new HTuple(hv_Length.TupleEqual(1))) != 0)
                {
                    //获取模板的轮廓
                    ho_modelContour.Dispose();
                    HOperatorSet.GetShapeModelContours(out ho_modelContour, hv_matchHandle, 1);
                    //创建初始化矩阵
                    HOperatorSet.HomMat2dIdentity(out hv_HomMat2D);
                    //计算刚性仿射变换
                    HOperatorSet.VectorAngleToRigid(0, 0, 0, hv_fRow, hv_fCol, hv_fAngle, out hv_HomMat2D);
                    //对XLD轮廓进行任意仿射2D变换
                    ho_fContour.Dispose();
                    HOperatorSet.AffineTransContourXld(ho_modelContour, out ho_fContour, hv_HomMat2D);
                }
                //
                ho_modelContour.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_modelContour.Dispose();

                throw HDevExpDefaultException;
            }
        }


        public void LoadProductParam(HTuple hv_WindowHandle, HTuple hv_Path, out HTuple hv_Result)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple ExpTmpLocalVar_ProductParamNamee = new HTuple();
            HTuple ExpTmpLocalVar_ProductParamValue = new HTuple();
            HTuple hv_ImageParamNamee = null, hv_ImageParamValue = null;
            HTuple hv_Index = new HTuple(), hv_LineRead = new HTuple();
            HTuple hv_IsEOF = new HTuple(), hv_Substrings = new HTuple();
            HTuple hv_LineItemLength = new HTuple(), hv_Exception = null;
            HTuple hv_DispMessage = new HTuple(), hv_DispColor = new HTuple();
            HTuple hv_DispFont = new HTuple(), hv_DispSize = new HTuple();
            HTuple hv_Path_COPY_INP_TMP = hv_Path.Clone();

            // Initialize local and output iconic variables 
            //**************************************************************************************************
            //**************************************************************************************************
            //**************************************************************************************************
            //加载配方标识（-1加载失败，1加载成功）
            hv_Result = -1;
            //**************************************************************************************************
            //定义全局变量
            //global tuple ProductParamNamee
            //global tuple ProductParamValue
            hv_ImageParamNamee = new HTuple();
            hv_ImageParamValue = new HTuple();
            //加载本地配方文件
            try
            {
                //设置读取编码格式为utf8
                HOperatorSet.SetSystem("filename_encoding", "utf8");

                HOperatorSet.OpenFile(hv_Path_COPY_INP_TMP, "input", out hv_Path_COPY_INP_TMP);
                hv_Index = 0;
                //提取ini配方文件中的所有参数
                do
                {
                    HOperatorSet.FreadLine(hv_Path_COPY_INP_TMP, out hv_LineRead, out hv_IsEOF);
                    HOperatorSet.TupleRegexpReplace(hv_LineRead, "\n", "", out hv_LineRead);
                    HOperatorSet.TupleSplit(hv_LineRead, new HTuple(","), out hv_Substrings);
                    HOperatorSet.TupleLength(hv_Substrings, out hv_LineItemLength);
                    if ((int)(new HTuple(hv_LineItemLength.TupleGreater(2))) != 0)
                    {
                        ExpTmpLocalVar_ProductParamNamee = ExpGetGlobalVar_ProductParamNamee();
                        if (ExpTmpLocalVar_ProductParamNamee == null)
                            ExpTmpLocalVar_ProductParamNamee = new HTuple();
                        ExpTmpLocalVar_ProductParamNamee[hv_Index] = hv_Substrings.TupleSelect(
                            1);
                        ExpSetGlobalVar_ProductParamNamee(ExpTmpLocalVar_ProductParamNamee);
                        ExpTmpLocalVar_ProductParamValue = ExpGetGlobalVar_ProductParamValue();
                        if (ExpTmpLocalVar_ProductParamValue == null)
                            ExpTmpLocalVar_ProductParamValue = new HTuple();
                        ExpTmpLocalVar_ProductParamValue[hv_Index] = hv_Substrings.TupleSelect(
                            3);
                        ExpSetGlobalVar_ProductParamValue(ExpTmpLocalVar_ProductParamValue);
                    }
                    hv_Index = hv_Index + 1;
                }
                while ((int)(new HTuple(hv_IsEOF.TupleEqual(1))) == 0);
                HOperatorSet.CloseFile(hv_Path_COPY_INP_TMP);
            }
            // catch (Exception) 
            catch (HalconException HDevExpDefaultException1)
            {
                HDevExpDefaultException1.ToHTuple(out hv_Exception);
                hv_Result = -1;
                HOperatorSet.CloseFile(hv_Path_COPY_INP_TMP);
                //
                hv_DispMessage = "图像处理匹配模型加载失败，请检查确认！" + hv_Exception;
                hv_DispColor = "red";
                hv_DispFont = "黑体";
                hv_DispSize = -12;
                DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, 0, 0, 200, hv_DispColor,
                    hv_DispFont, hv_DispSize);

                return;
                //
            }
            //**************************************************************************************************
            //**************************************************************************************************
            //所有配方加载完表示成功
            hv_Result = 1;

            return;
        }


        public void SegBreak(HObject ho_Image, HObject ho_CenterArea, HObject ho_InnerXld,
            HObject ho_OuterMarginXld, out HObject ho_BreakRegion, out HObject ho_RegionBreakXld,
            HTuple hv_SegBreakShift, out HTuple hv_BreakRegionNum)
        {




            // Local iconic variables 

            HObject ho_InnerRegion, ho_RegionDilation;
            HObject ho_ImageReduced, ho_ImageRegionDif, ho_DarkRegion;
            HObject ho_DConnectedRegions, ho_DSelectedRegions, ho_DImageReduce;
            HObject ho_ImageTexture, ho_Regions, ho_ConnectedRegions;

            // Local control variables 

            HTuple hv_UsedThreshold = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_BreakRegion);
            HOperatorSet.GenEmptyObj(out ho_RegionBreakXld);
            HOperatorSet.GenEmptyObj(out ho_InnerRegion);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImageRegionDif);
            HOperatorSet.GenEmptyObj(out ho_DarkRegion);
            HOperatorSet.GenEmptyObj(out ho_DConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_DSelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_DImageReduce);
            HOperatorSet.GenEmptyObj(out ho_ImageTexture);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            try
            {
                //扩大焊缝内径
                ho_InnerRegion.Dispose();
                HOperatorSet.GenRegionContourXld(ho_InnerXld, out ho_InnerRegion, "margin");
                ho_RegionDilation.Dispose();
                HOperatorSet.DilationCircle(ho_InnerRegion, out ho_RegionDilation, 10);
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_Image, ho_RegionDilation, out ho_ImageReduced);
                ho_ImageRegionDif.Dispose();
                HOperatorSet.Difference(ho_Image, ho_RegionDilation, out ho_ImageRegionDif);
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_Image, ho_ImageRegionDif, out ho_ImageReduced);
                //
                //筛选出暗区域
                ho_DarkRegion.Dispose();
                HOperatorSet.BinaryThreshold(ho_ImageReduced, out ho_DarkRegion, "max_separability",
                    "dark", out hv_UsedThreshold);
                ho_DConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_DarkRegion, out ho_DConnectedRegions);
                ho_DSelectedRegions.Dispose();
                HOperatorSet.SelectShapeStd(ho_DConnectedRegions, out ho_DSelectedRegions,
                    "max_area", 70);
                ho_DImageReduce.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageReduced, ho_DSelectedRegions, out ho_DImageReduce
                    );
                //
                //纹理检测爆孔
                //爆孔纹理缩放系数 181 SegBreakShift:=4, 178Q SegBreakShift:=3,178N SegBreakShift:=4
                ho_ImageTexture.Dispose();
                HOperatorSet.TextureLaws(ho_DImageReduce, out ho_ImageTexture, "el", hv_SegBreakShift,
                    7);
                ho_Regions.Dispose();
                HOperatorSet.Threshold(ho_ImageTexture, out ho_Regions, 0, 80);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
                ho_BreakRegion.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_BreakRegion, "max_area",
                    70);
                HOperatorSet.CountObj(ho_BreakRegion, out hv_BreakRegionNum);
                //
                ho_InnerRegion.Dispose();
                ho_RegionDilation.Dispose();
                ho_ImageReduced.Dispose();
                ho_ImageRegionDif.Dispose();
                ho_DarkRegion.Dispose();
                ho_DConnectedRegions.Dispose();
                ho_DSelectedRegions.Dispose();
                ho_DImageReduce.Dispose();
                ho_ImageTexture.Dispose();
                ho_Regions.Dispose();
                ho_ConnectedRegions.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_InnerRegion.Dispose();
                ho_RegionDilation.Dispose();
                ho_ImageReduced.Dispose();
                ho_ImageRegionDif.Dispose();
                ho_DarkRegion.Dispose();
                ho_DConnectedRegions.Dispose();
                ho_DSelectedRegions.Dispose();
                ho_DImageReduce.Dispose();
                ho_ImageTexture.Dispose();
                ho_Regions.Dispose();
                ho_ConnectedRegions.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void DrawRegion(HObject ho_Image, HObject ho_Region, out HObject ho_ImageClear)
        {


            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageClear);
            ho_ImageClear.Dispose();
            HOperatorSet.GenImageProto(ho_Image, out ho_ImageClear, 255);
            HOperatorSet.OverpaintRegion(ho_ImageClear, ho_Region, 0, "fill");

            return;
        }

        public void LoadImageParam(HTuple hv_WindowHandle, HTuple hv_ParamDir, HTuple hv_ParamName,
            HTuple hv_RegionPrefix, HTuple hv_MatchModelPrefix, HTuple hv_MetroModelPrefix,
            out HTuple hv_Result)
        {



            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ExpTmpLocalVar_Regions = null, ho_RegionRead = null;

            // Local control variables 

            HTuple ExpTmpLocalVar_ParamCata = null, ExpTmpLocalVar_ParamSpec = null;
            HTuple ExpTmpLocalVar_ImageParamNamee = null, ExpTmpLocalVar_ImageParamValue = null;
            HTuple hv_ParamPath = new HTuple(), hv_ParamFileHandle = new HTuple();
            HTuple hv_Index = new HTuple(), hv_LineRead = new HTuple();
            HTuple hv_IsEOF = new HTuple(), hv_Substrings = new HTuple();
            HTuple hv_LineItemLength = new HTuple(), hv_Exception = null;
            HTuple hv_DispMessage = new HTuple(), hv_DispColor = new HTuple();
            HTuple hv_DispFont = new HTuple(), hv_DispSize = new HTuple();
            HTuple hv_RegionPath = new HTuple(), ExpTmpLocalVar_MatchModels = new HTuple();
            HTuple hv_MatchPath = new HTuple(), hv_MatchRead = new HTuple();
            HTuple ExpTmpLocalVar_MetroModels = new HTuple(), hv_MetroPath = new HTuple();
            HTuple hv_MetroRead = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_RegionRead);
            try
            {
                //**************************************************************************************************
                //**************************************************************************************************
                //**************************************************************************************************
                //加载配方标识（-1加载失败，1加载成功）
                hv_Result = 1;
                //**************************************************************************************************
                //全局变量
                //global tuple ParamCata           //参数类别
                //global tuple ParamSpec           //参数说明
                //global tuple ImageParamNamee     //参数名称
                //global tuple ImageParamValue     //参数数值
                //
                ExpTmpLocalVar_ParamCata = new HTuple();
                ExpSetGlobalVar_ParamCata(ExpTmpLocalVar_ParamCata);
                ExpTmpLocalVar_ParamSpec = new HTuple();
                ExpSetGlobalVar_ParamSpec(ExpTmpLocalVar_ParamSpec);
                ExpTmpLocalVar_ImageParamNamee = new HTuple();
                ExpSetGlobalVar_ImageParamNamee(ExpTmpLocalVar_ImageParamNamee);
                ExpTmpLocalVar_ImageParamValue = new HTuple();
                ExpSetGlobalVar_ImageParamValue(ExpTmpLocalVar_ImageParamValue);
                //
                //加载本地配方文件
                try
                {
                    //
                    //设置读取编码格式为utf8
                    HOperatorSet.SetSystem("filename_encoding", "utf8");

                    hv_ParamPath = hv_ParamDir + hv_ParamName;
                    HOperatorSet.OpenFile(hv_ParamPath, "input", out hv_ParamFileHandle);
                    hv_Index = 0;
                    //提取ini配方文件中的所有参数
                    do
                    {
                        HOperatorSet.FreadLine(hv_ParamFileHandle, out hv_LineRead, out hv_IsEOF);
                        HOperatorSet.TupleRegexpReplace(hv_LineRead, "\n", "", out hv_LineRead);
                        HOperatorSet.TupleSplit(hv_LineRead, new HTuple(","), out hv_Substrings);
                        HOperatorSet.TupleLength(hv_Substrings, out hv_LineItemLength);
                        if ((int)(new HTuple(hv_LineItemLength.TupleGreater(3))) != 0)
                        {
                            ExpTmpLocalVar_ParamCata = ExpGetGlobalVar_ParamCata();
                            if (ExpTmpLocalVar_ParamCata == null)
                                ExpTmpLocalVar_ParamCata = new HTuple();
                            ExpTmpLocalVar_ParamCata[hv_Index] = hv_Substrings.TupleSelect(0);
                            ExpSetGlobalVar_ParamCata(ExpTmpLocalVar_ParamCata);
                            ExpTmpLocalVar_ParamSpec = ExpGetGlobalVar_ParamSpec();
                            if (ExpTmpLocalVar_ParamSpec == null)
                                ExpTmpLocalVar_ParamSpec = new HTuple();
                            ExpTmpLocalVar_ParamSpec[hv_Index] = hv_Substrings.TupleSelect(2);
                            ExpSetGlobalVar_ParamSpec(ExpTmpLocalVar_ParamSpec);
                            ExpTmpLocalVar_ImageParamNamee = ExpGetGlobalVar_ImageParamNamee();
                            if (ExpTmpLocalVar_ImageParamNamee == null)
                                ExpTmpLocalVar_ImageParamNamee = new HTuple();
                            ExpTmpLocalVar_ImageParamNamee[hv_Index] = hv_Substrings.TupleSelect(
                                1);
                            ExpSetGlobalVar_ImageParamNamee(ExpTmpLocalVar_ImageParamNamee);
                            ExpTmpLocalVar_ImageParamValue = ExpGetGlobalVar_ImageParamValue();
                            if (ExpTmpLocalVar_ImageParamValue == null)
                                ExpTmpLocalVar_ImageParamValue = new HTuple();
                            ExpTmpLocalVar_ImageParamValue[hv_Index] = hv_Substrings.TupleSelect(
                                3);
                            ExpSetGlobalVar_ImageParamValue(ExpTmpLocalVar_ImageParamValue);
                        }
                        hv_Index = hv_Index + 1;
                    }
                    while ((int)(new HTuple(hv_IsEOF.TupleEqual(1))) == 0);
                    HOperatorSet.CloseFile(hv_ParamFileHandle);
                    //
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //
                    hv_Result = -1;
                    hv_DispMessage = "图像处理图像参数加载失败，请检查确认！" + hv_Exception;
                    hv_DispColor = "red";
                    hv_DispFont = "黑体";
                    hv_DispSize = -12;
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, 0, 0, 200, hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    ho_RegionRead.Dispose();

                    return;
                    //
                }
                //**************************************************************************************************
                //全局变量
                //global object Regions                 //兴趣区域
                HOperatorSet.SetSystem("clip_region", "false");
                //加载本地配方文件
                try
                {
                    //
                    HOperatorSet.GenEmptyObj(out ExpTmpLocalVar_Regions);
                    ExpSetGlobalVar_Regions(ExpTmpLocalVar_Regions);
                    for (hv_Index = 1; (int)hv_Index <= 1; hv_Index = (int)hv_Index + 1)
                    {
                        hv_RegionPath = (((hv_ParamDir + hv_RegionPrefix) + "_") + (hv_Index.TupleString(
                            "0.0f"))) + ".hobj";
                        ho_RegionRead.Dispose();
                        HOperatorSet.GenEmptyRegion(out ho_RegionRead);
                        ho_RegionRead.Dispose();
                        HOperatorSet.ReadRegion(out ho_RegionRead, hv_RegionPath);
                        HOperatorSet.ConcatObj(ExpGetGlobalVar_Regions(), ho_RegionRead, out ExpTmpLocalVar_Regions
                            );
                        ExpSetGlobalVar_Regions(ExpTmpLocalVar_Regions);
                    }
                    //
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //
                    hv_Result = -1;
                    hv_DispMessage = "图像处理兴趣区域加载失败，请检查确认！" + hv_Exception;
                    hv_DispColor = "red";
                    hv_DispFont = "黑体";
                    hv_DispSize = -12;
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, 0, 0, 200, hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    ho_RegionRead.Dispose();

                    return;
                    //
                }
                //**************************************************************************************************
                //全局变量
                //global tuple MatchModels             //匹配模型
                //加载本地配方文件
                try
                {
                    //
                    ExpTmpLocalVar_MatchModels = new HTuple();
                    ExpSetGlobalVar_MatchModels(ExpTmpLocalVar_MatchModels);
                    for (hv_Index = 1; (int)hv_Index <= 1; hv_Index = (int)hv_Index + 1)
                    {
                        hv_MatchPath = (((hv_ParamDir + hv_MatchModelPrefix) + "_") + (hv_Index.TupleString(
                            "0.0f"))) + ".shm";
                        HOperatorSet.ReadShapeModel(hv_MatchPath, out hv_MatchRead);
                        HOperatorSet.TupleConcat(ExpGetGlobalVar_MatchModels(), hv_MatchRead, out ExpTmpLocalVar_MatchModels);
                        ExpSetGlobalVar_MatchModels(ExpTmpLocalVar_MatchModels);
                    }
                    //
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //
                    hv_Result = -1;
                    hv_DispMessage = "图像处理匹配模型加载失败，请检查确认！" + hv_Exception;
                    hv_DispColor = "red";
                    hv_DispFont = "黑体";
                    hv_DispSize = -12;
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, 0, 0, 200, hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    ho_RegionRead.Dispose();

                    return;
                    //
                }
                //**************************************************************************************************
                //全局变量
                //global tuple MetroModels           //测量模型
                //加载本地配方文件
                try
                {
                    //
                    ExpTmpLocalVar_MetroModels = new HTuple();
                    ExpSetGlobalVar_MetroModels(ExpTmpLocalVar_MetroModels);
                    for (hv_Index = 1; (int)hv_Index <= 2; hv_Index = (int)hv_Index + 1)
                    {
                        hv_MetroPath = (((hv_ParamDir + hv_MetroModelPrefix) + "_") + (hv_Index.TupleString(
                            "0.0f"))) + ".mtr";
                        HOperatorSet.ReadMetrologyModel(hv_MetroPath, out hv_MetroRead);
                        HOperatorSet.TupleConcat(ExpGetGlobalVar_MetroModels(), hv_MetroRead, out ExpTmpLocalVar_MetroModels);
                        ExpSetGlobalVar_MetroModels(ExpTmpLocalVar_MetroModels);
                    }
                    //
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    //
                    hv_Result = -1;
                    hv_DispMessage = "图像处理测量模型加载失败，请检查确认！" + hv_Exception;
                    hv_DispColor = "red";
                    hv_DispFont = "黑体";
                    hv_DispSize = -12;
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, 0, 0, 200, hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    ho_RegionRead.Dispose();

                    return;
                    //
                }
                //**************************************************************************************************
                //所有配方加载完表示成功
                ho_RegionRead.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_RegionRead.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void GetPoleNum(HTuple hv_CheckNum, HTuple hv_WindowHandle, out HTuple hv_PoleNum,
            out HTuple hv_ResultArray)
        {



            // Local iconic variables 

            HObject ho_ImageByte;

            // Local control variables 

            HTuple hv_Indices = new HTuple(), hv_DispRow01 = new HTuple();
            HTuple hv_DispCol01 = new HTuple(), hv_Interval01 = new HTuple();
            HTuple hv_DispRow02 = new HTuple(), hv_DispCol02 = new HTuple();
            HTuple hv_Interval02 = new HTuple(), hv_DispFont = new HTuple();
            HTuple hv_DispSize = new HTuple(), hv_Exception = null;
            HTuple hv_DispMessage = new HTuple(), hv_DispColor = new HTuple();
            HTuple hv_PoleInfor = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_ImageByte);
            hv_PoleNum = new HTuple();
            try
            {
                //
                //**************************************************************************************************
                //1、分割出焊缝区域
                //2、根据焊缝周围平面进行平面拟合
                //3、根据拟合平面对焊缝图像进行调平处理
                //**************************************************************************************************
                //**************************************************************************************************
                //**************************************************************************************************
                //**************************************************************************************************
                //初始化输出参数
                HOperatorSet.TupleGenConst(7, 0, out hv_ResultArray);
                if (hv_ResultArray == null)
                    hv_ResultArray = new HTuple();
                hv_ResultArray[0] = 1;
                ho_ImageByte.Dispose();
                HOperatorSet.GenEmptyObj(out ho_ImageByte);
                //**************************************************************************************************
                //引用全局变量
                //global tuple ImageParamNamee
                //global tuple ImageParamValue
                //global tuple ProductParamNamee
                //global tuple ProductParamValue
                //
                try
                {
                    //结果显示参数
                    HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "StartRow01", out hv_Indices);
                    HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                        out hv_DispRow01);
                    //
                    HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "StartCol01", out hv_Indices);
                    HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                        out hv_DispCol01);
                    //
                    HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "Interval01", out hv_Indices);
                    HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                        out hv_Interval01);
                    //
                    HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "StartRow02", out hv_Indices);
                    HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                        out hv_DispRow02);
                    //
                    HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "StartCol02", out hv_Indices);
                    HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                        out hv_DispCol02);
                    //
                    HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "Interval02", out hv_Indices);
                    HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                        out hv_Interval02);
                    //
                    hv_DispFont = "黑体";
                    hv_DispSize = -12;
                    //
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    hv_DispMessage = "参数读取错误，请检查确认！";
                    hv_DispColor = "red";
                    //
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, hv_DispRow01 + (hv_Interval01 * 0),
                        hv_DispCol01, hv_Interval01, hv_DispColor, hv_DispFont, hv_DispSize);
                    ho_ImageByte.Dispose();

                    return;
                }
                //引用焊缝控制参数
                //****************整数*****************
                //d1 := 123$'6'
                //d2 := 123$'-6'
                //d3 := 123$'.6'
                //d4 := 12345$'10.5'
                //
                try
                {
                    hv_PoleInfor = "CheckNum" + (hv_CheckNum.TupleString("0.2"));
                    HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), hv_PoleInfor,
                        out hv_Indices);
                    if ((int)(new HTuple(hv_Indices.TupleGreater(-1))) != 0)
                    {
                        HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(
                            hv_Indices), out hv_PoleNum);
                    }
                    else
                    {
                        if (hv_ResultArray == null)
                            hv_ResultArray = new HTuple();
                        hv_ResultArray[0] = -1;
                        hv_DispMessage = "未找到检测序号对应的极柱号，请检查确认！";
                        hv_DispColor = "red";
                        DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, hv_DispRow01 + (hv_Interval01 * 0),
                            hv_DispCol01, hv_Interval01, hv_DispColor, hv_DispFont, hv_DispSize);
                        ho_ImageByte.Dispose();

                        return;
                    }
                }
                // catch (Exception) 
                catch (HalconException HDevExpDefaultException1)
                {
                    HDevExpDefaultException1.ToHTuple(out hv_Exception);
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    hv_DispMessage = "参数读取错误，请检查确认！";
                    hv_DispColor = "red";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, hv_DispRow01 + (hv_Interval01 * 0),
                        hv_DispCol01, hv_Interval01, hv_DispColor, hv_DispFont, hv_DispSize);
                    ho_ImageByte.Dispose();

                    return;
                }
                ho_ImageByte.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ImageByte.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void SetParams(out HTuple hv_SearchParams)
        {


            // Local iconic variables 

            // Local control variables 

            HTuple hv_angleStart = null, hv_angleExtent = null;
            HTuple hv_scaleMin = null, hv_scaleMax = null, hv_minScore = null;
            HTuple hv_numMatch = null, hv_maxOverlap = null, hv_subPixel = null;
            HTuple hv_numLevel = null, hv_greed = null;
            // Initialize local and output iconic variables 
            hv_SearchParams = new HTuple();
            hv_angleStart = -0.5;
            hv_angleExtent = 0.5;
            hv_scaleMin = 0.96;
            hv_scaleMax = 1.03;
            hv_minScore = 0.1;
            //最小得分原值：=0.25
            hv_numMatch = 1;
            hv_maxOverlap = 0.35;
            hv_subPixel = "least_squares";
            hv_numLevel = 3;
            hv_greed = 0.9;
            //
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[0] = hv_angleStart;
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[1] = hv_angleExtent;
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[2] = hv_scaleMin;
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[3] = hv_scaleMax;
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[4] = hv_minScore;
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[5] = hv_numMatch;
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[6] = hv_maxOverlap;
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[7] = hv_subPixel;
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[8] = hv_numLevel;
            if (hv_SearchParams == null)
                hv_SearchParams = new HTuple();
            hv_SearchParams[9] = hv_greed;
            //
            //

            return;
        }

        public void FindPoleAndHolesC(HObject ho_ImgRoi, HObject ho_CenterImgSearch, HObject ho_ImgCheck,
            out HObject ho_InnerXld, out HObject ho_PoleContour, HTuple hv_MetroHandle02,
            HTuple hv_WindowHandle, HTuple hv_OuterMarginParamA, HTuple hv_OuterMarginParamB,
            out HTuple hv_CenterFLength, out HTuple hv_CenterFScore, out HTuple hv_ShapeLength,
            out HTuple hv_PoleFScore, out HTuple hv_PoleLength, out HTuple hv_LengthInnerCircle,
            out HTuple hv_InnerMarginRow, out HTuple hv_InnerMarginCol)
        {




            // Local iconic variables 

            HObject ho_ROI_0, ho_ImageClear, ho_InnerCircle;
            HObject ho_InnerImageReduced, ho_ImageScaled, ho_ImageMean;
            HObject ho_InnerRegions, ho_InnerConnectedRegions, ho_SelectedRegions;
            HObject ho_RegionClosing, ho_RegionOpening, ho_RegionDilation;
            HObject ho_RegionTrans, ho_RegionErosion, ho_InnerRegion;
            HObject ho_Rectangle = null, ho_RegionIntersection = null;

            // Local control variables 

            HTuple hv_SearchParams = null, hv_oriceny = null;
            HTuple hv_oricenx = null, hv_Radius = null, hv_PoleModelID = null;
            HTuple hv_Frow = null, hv_FCol = null, hv_Fangle = null;
            HTuple hv_Fscale = null, hv_MatchResult = new HTuple();
            HTuple hv_Row = null, hv_Col = null, hv_Angles = null;
            HTuple hv_Number = null, hv_index = null, hv_IndexNumber = new HTuple();
            HTuple hv_Area = new HTuple(), hv_InnerRowConcat = new HTuple();
            HTuple hv_InnerColConcat = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_InnerXld);
            HOperatorSet.GenEmptyObj(out ho_PoleContour);
            HOperatorSet.GenEmptyObj(out ho_ROI_0);
            HOperatorSet.GenEmptyObj(out ho_ImageClear);
            HOperatorSet.GenEmptyObj(out ho_InnerCircle);
            HOperatorSet.GenEmptyObj(out ho_InnerImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_ImageMean);
            HOperatorSet.GenEmptyObj(out ho_InnerRegions);
            HOperatorSet.GenEmptyObj(out ho_InnerConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_InnerRegion);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_RegionIntersection);
            hv_CenterFLength = new HTuple();
            hv_CenterFScore = new HTuple();
            hv_ShapeLength = new HTuple();
            hv_LengthInnerCircle = new HTuple();
            hv_InnerMarginRow = new HTuple();
            hv_InnerMarginCol = new HTuple();
            try
            {
                //-------------------------------------------------------
                //区域范围：焊缝区域
                //函数功能：通过创建模板匹配极柱位置，再通过阈值分割找到内径
                //可调参数：
                //参数调整建议：
                //-------------------------------------------------------
                //模板参数设定
                SetParams(out hv_SearchParams);
                hv_oriceny = 252.75;
                hv_oricenx = 262.394;
                hv_Radius = 65;
                ho_ROI_0.Dispose();
                HOperatorSet.GenCircle(out ho_ROI_0, hv_oriceny, hv_oricenx, hv_Radius);
                ho_ImageClear.Dispose();
                DrawRegion(ho_ImgRoi, ho_ROI_0, out ho_ImageClear);
                //创建极柱形状模板
                HOperatorSet.CreateShapeModel(ho_ImageClear, "auto", (new HTuple(-90)).TupleRad()
                    , (new HTuple(90)).TupleRad(), "auto", "auto", "use_polarity", "auto",
                    "auto", out hv_PoleModelID);
                //
                //匹配极柱位置
                ho_PoleContour.Dispose();
                FindMatchShape(ho_CenterImgSearch, out ho_PoleContour, hv_PoleModelID, hv_SearchParams,
                    out hv_Frow, out hv_FCol, out hv_Fangle, out hv_Fscale, out hv_PoleFScore,
                    out hv_PoleLength);
                if ((int)(new HTuple(hv_PoleLength.TupleEqual(0))) != 0)
                {
                    hv_MatchResult = 0;
                    ho_ROI_0.Dispose();
                    ho_ImageClear.Dispose();
                    ho_InnerCircle.Dispose();
                    ho_InnerImageReduced.Dispose();
                    ho_ImageScaled.Dispose();
                    ho_ImageMean.Dispose();
                    ho_InnerRegions.Dispose();
                    ho_InnerConnectedRegions.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_RegionClosing.Dispose();
                    ho_RegionOpening.Dispose();
                    ho_RegionDilation.Dispose();
                    ho_RegionTrans.Dispose();
                    ho_RegionErosion.Dispose();
                    ho_InnerRegion.Dispose();
                    ho_Rectangle.Dispose();
                    ho_RegionIntersection.Dispose();

                    return;
                }
                //
                HOperatorSet.ClearShapeModel(hv_PoleModelID);
                //
                //获取计量模型的测量结果(内径)
                ho_InnerCircle.Dispose();
                HOperatorSet.GenCircle(out ho_InnerCircle, hv_OuterMarginParamA, hv_OuterMarginParamB,
                    95);
                ho_InnerImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_ImgCheck, ho_InnerCircle, out ho_InnerImageReduced
                    );
                ho_ImageScaled.Dispose();
                scale_image_range(ho_InnerImageReduced, out ho_ImageScaled, 150, 300);
                ho_ImageMean.Dispose();
                HOperatorSet.MeanImage(ho_ImageScaled, out ho_ImageMean, 5, 5);
                ho_InnerRegions.Dispose();
                HOperatorSet.Threshold(ho_ImageMean, out ho_InnerRegions, 60, 255);
                ho_InnerConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_InnerRegions, out ho_InnerConnectedRegions);
                ho_SelectedRegions.Dispose();
                HOperatorSet.SelectShapeStd(ho_InnerConnectedRegions, out ho_SelectedRegions,
                    "max_area", 70);
                ho_RegionClosing.Dispose();
                HOperatorSet.ClosingCircle(ho_SelectedRegions, out ho_RegionClosing, 7);
                ho_RegionOpening.Dispose();
                HOperatorSet.OpeningCircle(ho_RegionClosing, out ho_RegionOpening, 3);
                ho_RegionDilation.Dispose();
                HOperatorSet.DilationCircle(ho_RegionOpening, out ho_RegionDilation, 5);
                ho_RegionTrans.Dispose();
                HOperatorSet.ShapeTrans(ho_RegionDilation, out ho_RegionTrans, "outer_circle");
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_RegionTrans, out ho_RegionErosion, 2);
                ho_InnerRegion.Dispose();
                HOperatorSet.Difference(ho_RegionTrans, ho_RegionErosion, out ho_InnerRegion
                    );
                //获取内径上可检测的坐标位置
                ho_InnerXld.Dispose();
                HOperatorSet.GenContourRegionXld(ho_InnerRegion, out ho_InnerXld, "border");
                HOperatorSet.GetContourXld(ho_InnerXld, out hv_Row, out hv_Col);
                HOperatorSet.GetContourAngleXld(ho_InnerXld, "abs", "range", 3, out hv_Angles);
                hv_InnerMarginRow = new HTuple();
                hv_InnerMarginCol = new HTuple();
                hv_Number = 19;
                HTuple end_val46 = (new HTuple(hv_Row.TupleLength())) / hv_Number;
                HTuple step_val46 = 1;
                for (hv_index = 0; hv_index.Continue(end_val46, step_val46); hv_index = hv_index.TupleAdd(step_val46))
                {
                    hv_IndexNumber = hv_Number * hv_index;
                    if ((int)(new HTuple(hv_IndexNumber.TupleGreaterEqual(new HTuple(hv_Row.TupleLength()
                        )))) != 0)
                    {
                        break;
                    }
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle2(out ho_Rectangle, hv_Row.TupleSelect(hv_Number * hv_index),
                        hv_Col.TupleSelect(hv_Number * hv_index), (hv_Angles.TupleSelect(hv_Number * hv_index)) + ((new HTuple(90)).TupleRad()
                        ), 15, 1);
                    ho_RegionIntersection.Dispose();
                    HOperatorSet.Intersection(ho_InnerRegion, ho_Rectangle, out ho_RegionIntersection
                        );
                    HOperatorSet.AreaCenter(ho_RegionIntersection, out hv_Area, out hv_InnerRowConcat,
                        out hv_InnerColConcat);
                    HOperatorSet.TupleConcat(hv_InnerMarginRow, hv_InnerRowConcat, out hv_InnerMarginRow);
                    HOperatorSet.TupleConcat(hv_InnerMarginCol, hv_InnerColConcat, out hv_InnerMarginCol);
                }
                //
                hv_CenterFLength = 1;
                hv_CenterFScore = 1;
                hv_ShapeLength = 1;
                hv_PoleFScore = 1;
                hv_PoleLength = 1;
                hv_LengthInnerCircle = 1;
                //
                ho_ROI_0.Dispose();
                ho_ImageClear.Dispose();
                ho_InnerCircle.Dispose();
                ho_InnerImageReduced.Dispose();
                ho_ImageScaled.Dispose();
                ho_ImageMean.Dispose();
                ho_InnerRegions.Dispose();
                ho_InnerConnectedRegions.Dispose();
                ho_SelectedRegions.Dispose();
                ho_RegionClosing.Dispose();
                ho_RegionOpening.Dispose();
                ho_RegionDilation.Dispose();
                ho_RegionTrans.Dispose();
                ho_RegionErosion.Dispose();
                ho_InnerRegion.Dispose();
                ho_Rectangle.Dispose();
                ho_RegionIntersection.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ROI_0.Dispose();
                ho_ImageClear.Dispose();
                ho_InnerCircle.Dispose();
                ho_InnerImageReduced.Dispose();
                ho_ImageScaled.Dispose();
                ho_ImageMean.Dispose();
                ho_InnerRegions.Dispose();
                ho_InnerConnectedRegions.Dispose();
                ho_SelectedRegions.Dispose();
                ho_RegionClosing.Dispose();
                ho_RegionOpening.Dispose();
                ho_RegionDilation.Dispose();
                ho_RegionTrans.Dispose();
                ho_RegionErosion.Dispose();
                ho_InnerRegion.Dispose();
                ho_Rectangle.Dispose();
                ho_RegionIntersection.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void TemplateMatching()
        {


            // Local iconic variables 

            // Local control variables 

            HTuple hv_angleStart = null, hv_angleExtent = null;
            HTuple hv_scaleMin = null, hv_scaleMax = null, hv_minScore = null;
            HTuple hv_numMatch = null, hv_maxOverlap = null, hv_subPixel = null;
            HTuple hv_numLevel = null, hv_greed = null, hv_searchParams = null;
            // Initialize local and output iconic variables 
            //读取焊缝观察孔匹配模型
            //read_shape_model ('./model_1.shm', matchHandle01)
            //读取焊缝外边缘测量模型
            //read_metrology_model ('./metro_1.mtr', metroHandle01)
            //读取焊缝内边缘测量模型
            //read_metrology_model ('./metro_2.mtr', metroHandle02)
            //设置匹配模型匹配参数
            hv_angleStart = (new HTuple(-90)).TupleRad();
            hv_angleExtent = (new HTuple(90)).TupleRad();
            hv_scaleMin = 0.9;
            hv_scaleMax = 1.1;
            hv_minScore = 0.2;
            hv_numMatch = 1;
            hv_maxOverlap = 0.3;
            hv_subPixel = "least_squares";
            hv_numLevel = 5;
            hv_greed = 0.9;
            hv_searchParams = new HTuple();
            HOperatorSet.TupleConcat(hv_searchParams, hv_angleStart, out hv_searchParams);
            HOperatorSet.TupleConcat(hv_searchParams, hv_angleExtent, out hv_searchParams);
            HOperatorSet.TupleConcat(hv_searchParams, hv_scaleMin, out hv_searchParams);
            HOperatorSet.TupleConcat(hv_searchParams, hv_scaleMax, out hv_searchParams);
            HOperatorSet.TupleConcat(hv_searchParams, hv_minScore, out hv_searchParams);
            HOperatorSet.TupleConcat(hv_searchParams, hv_numMatch, out hv_searchParams);
            HOperatorSet.TupleConcat(hv_searchParams, hv_maxOverlap, out hv_searchParams);
            HOperatorSet.TupleConcat(hv_searchParams, hv_subPixel, out hv_searchParams);
            HOperatorSet.TupleConcat(hv_searchParams, hv_numLevel, out hv_searchParams);
            HOperatorSet.TupleConcat(hv_searchParams, hv_greed, out hv_searchParams);

            return;
        }

        public void FindPoleAndHoles(HObject ho_CenterImgSearch, HObject ho_ImgSScaledSearch,
            HObject ho_ImgCheck, out HObject ho_PoleContour, out HObject ho_CenterContour,
            out HObject ho_InnerXld, HTuple hv_MetroHandle02, HTuple hv_OuterMarginParamA,
            HTuple hv_OuterMarginParamB, HTuple hv_SearchParams, HTuple hv_WindowHandle,
            out HTuple hv_ShapeLength, out HTuple hv_CenterFScore, out HTuple hv_CenterFLength,
            out HTuple hv_CenterFrow, out HTuple hv_CenterFCol, out HTuple hv_oriFrow, out HTuple hv_oriFCol,
            out HTuple hv_oriceny, out HTuple hv_oricenx, out HTuple hv_InnerMarginRow,
            out HTuple hv_InnerMarginCol)
        {




            // Local iconic variables 

            HObject ho_Regions, ho_ConnectedRegions, ho_MaxSelectedRegions;
            HObject ho_PoleReduced, ho_CenterHole, ho_ImagePaint, ho_Contours3;
            HObject ho_ImageReduced, ho_CenterImageScaled, ho_CenterROI;
            HObject ho_ImageClear;

            // Local control variables 

            HTuple hv_Area = null, hv_Frow = null, hv_FCol = null;
            HTuple hv_Value = null, hv_CenterMarginRow = null, hv_CenterMarginCol = null;
            HTuple hv_Mean = null, hv_Deviation = null, hv_Int = null;
            HTuple hv_MeanInt = null, hv_Mod = null, hv_rangeMax = null;
            HTuple hv_Radius = null, hv_CenterModelID = null, hv_oriFangle = null;
            HTuple hv_oriFscale = null, hv_orifScore = null, hv_CenterFangle = null;
            HTuple hv_CenterFscale = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_PoleContour);
            HOperatorSet.GenEmptyObj(out ho_CenterContour);
            HOperatorSet.GenEmptyObj(out ho_InnerXld);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_MaxSelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_PoleReduced);
            HOperatorSet.GenEmptyObj(out ho_CenterHole);
            HOperatorSet.GenEmptyObj(out ho_ImagePaint);
            HOperatorSet.GenEmptyObj(out ho_Contours3);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_CenterImageScaled);
            HOperatorSet.GenEmptyObj(out ho_CenterROI);
            HOperatorSet.GenEmptyObj(out ho_ImageClear);
            try
            {
                //-------------------------------------------------------
                //区域范围：焊缝区域
                //函数功能：直接阈值分割出内径，再通过模板匹配找到中心小孔位置
                //可调参数：
                //参数调整建议：
                //-------------------------------------------------------
                //极柱位置
                ho_Regions.Dispose();
                HOperatorSet.Threshold(ho_CenterImgSearch, out ho_Regions, 200, 255);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
                ho_MaxSelectedRegions.Dispose();
                HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_MaxSelectedRegions,
                    "max_area", 70);
                ho_PoleContour.Dispose();
                HOperatorSet.ShapeTrans(ho_MaxSelectedRegions, out ho_PoleContour, "convex");
                //内径位置
                ho_InnerXld.Dispose();
                HOperatorSet.GenContourRegionXld(ho_PoleContour, out ho_InnerXld, "border");
                HOperatorSet.GetContourXld(ho_InnerXld, out hv_InnerMarginRow, out hv_InnerMarginCol);
                HOperatorSet.SetColor(hv_WindowHandle, "green");
                HOperatorSet.DispObj(ho_InnerXld, hv_WindowHandle);
                //
                HOperatorSet.AreaCenter(ho_PoleContour, out hv_Area, out hv_Frow, out hv_FCol);
                HOperatorSet.RegionFeatures(ho_PoleContour, "outer_radius", out hv_Value);
                ho_PoleReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_CenterImgSearch, ho_PoleContour, out ho_PoleReduced
                    );
                ho_CenterHole.Dispose();
                HOperatorSet.GenCircle(out ho_CenterHole, hv_Frow, hv_FCol, 60);
                ho_ImagePaint.Dispose();
                HOperatorSet.PaintRegion(ho_PoleContour, ho_ImgCheck, out ho_ImagePaint, 255,
                    "fill");
                HOperatorSet.AlignMetrologyModel(hv_MetroHandle02, hv_OuterMarginParamA, hv_OuterMarginParamB,
                    0);
                HOperatorSet.SetMetrologyObjectParam(hv_MetroHandle02, "all", (new HTuple("measure_length1")).TupleConcat(
                    "measure_length2"), (new HTuple(90)).TupleConcat(5));
                HOperatorSet.ApplyMetrologyModel(ho_ImagePaint, hv_MetroHandle02);
                ho_Contours3.Dispose();
                HOperatorSet.GetMetrologyObjectMeasures(out ho_Contours3, hv_MetroHandle02,
                    "all", "positive", out hv_CenterMarginRow, out hv_CenterMarginCol);
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_ImgSScaledSearch, ho_CenterHole, out ho_ImageReduced
                    );
                HOperatorSet.Intensity(ho_CenterHole, ho_ImageReduced, out hv_Mean, out hv_Deviation);
                HOperatorSet.TupleInt(hv_Mean / 10, out hv_Int);
                HOperatorSet.TupleInt(hv_Mean, out hv_MeanInt);
                HOperatorSet.TupleMod(hv_MeanInt, 10, out hv_Mod);
                if ((int)(new HTuple(hv_Mod.TupleGreater(5))) != 0)
                {
                    hv_Int = hv_Int + 1;
                }
                hv_rangeMax = (hv_Int - 1) * 10;
                if ((int)(new HTuple(hv_rangeMax.TupleLessEqual(50))) != 0)
                {
                    hv_rangeMax = 51;
                }
                ho_CenterImageScaled.Dispose();
                scale_image_range(ho_ImageReduced, out ho_CenterImageScaled, 30, hv_rangeMax + 10);
                hv_oriceny = 252.75;
                hv_oricenx = 262.394;
                hv_Radius = 18;
                //创建中心小孔形状模板
                ho_CenterROI.Dispose();
                HOperatorSet.GenCircle(out ho_CenterROI, hv_oriceny, hv_oricenx, hv_Radius);
                ho_ImageClear.Dispose();
                DrawRegion(ho_CenterImageScaled, ho_CenterROI, out ho_ImageClear);
                HOperatorSet.CreateShapeModel(ho_ImageClear, "auto", (new HTuple(-90)).TupleRad()
                    , (new HTuple(90)).TupleRad(), "auto", "auto", "use_polarity", "auto",
                    "auto", out hv_CenterModelID);
                ho_CenterContour.Dispose();
                FindMatchShape(ho_ImageClear, out ho_CenterContour, hv_CenterModelID, hv_SearchParams,
                    out hv_oriFrow, out hv_oriFCol, out hv_oriFangle, out hv_oriFscale, out hv_orifScore,
                    out hv_ShapeLength);
                //匹配中心小孔位置
                ho_CenterContour.Dispose();
                FindMatchShape(ho_CenterImageScaled, out ho_CenterContour, hv_CenterModelID,
                    hv_SearchParams, out hv_CenterFrow, out hv_CenterFCol, out hv_CenterFangle,
                    out hv_CenterFscale, out hv_CenterFScore, out hv_CenterFLength);
                HOperatorSet.ClearShapeModel(hv_CenterModelID);
                ho_Regions.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_MaxSelectedRegions.Dispose();
                ho_PoleReduced.Dispose();
                ho_CenterHole.Dispose();
                ho_ImagePaint.Dispose();
                ho_Contours3.Dispose();
                ho_ImageReduced.Dispose();
                ho_CenterImageScaled.Dispose();
                ho_CenterROI.Dispose();
                ho_ImageClear.Dispose();

                return;
                //
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Regions.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_MaxSelectedRegions.Dispose();
                ho_PoleReduced.Dispose();
                ho_CenterHole.Dispose();
                ho_ImagePaint.Dispose();
                ho_Contours3.Dispose();
                ho_ImageReduced.Dispose();
                ho_CenterImageScaled.Dispose();
                ho_CenterROI.Dispose();
                ho_ImageClear.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void FindPoleAndHolesB(HObject ho_ImgRoi, HObject ho_CenterImgSearch, HObject ho_ImgCheck,
            HObject ho_ImgSScaledSearch, HObject ho_OuterMarginXld, out HObject ho_InnerXld,
            out HObject ho_BeadLengthLine, out HObject ho_WidthLineMin, out HObject ho_WidthLineMax,
            out HObject ho_PoleContour, out HObject ho_CenterContour, HTuple hv_MetroHandle02,
            HTuple hv_OuterMarginParamA, HTuple hv_OuterMarginParamB, HTuple hv_WindowHandle,
            HTuple hv_DispFont, HTuple hv_DispSize, out HTuple hv_ShapeLength, out HTuple hv_CenterFScore,
            out HTuple hv_CenterFLength, out HTuple hv_PoleFScore, out HTuple hv_PoleLength,
            out HTuple hv_BeadLengthPix, out HTuple hv_InnerRow, out HTuple hv_InnerCol,
            out HTuple hv_InnerMarginRow, out HTuple hv_InnerMarginCol, out HTuple hv_CenterFrow,
            out HTuple hv_CenterFCol, out HTuple hv_oriFrow, out HTuple hv_oriFCol, out HTuple hv_oriceny,
            out HTuple hv_oricenx, out HTuple hv_LengthInnerCircle, out HTuple hv_MatchResult)
        {




            // Local iconic variables 

            HObject ho_ROI_0, ho_ImageClear, ho_PoleRegion = null;
            HObject ho_CenterHole = null, ho_CenterHoleSearch = null, ho_ImagePaint = null;
            HObject ho_Contours3 = null, ho_ImageReduced = null, ho_CenterImageScaled = null;
            HObject ho_CenterROI = null;

            // Local control variables 

            HTuple hv_SearchParams = null, hv_Radius = null;
            HTuple hv_PoleModelID = null, hv_Frow = null, hv_FCol = null;
            HTuple hv_Fangle = null, hv_Fscale = null, hv_FArea = new HTuple();
            HTuple hv_CenterMarginRow = new HTuple(), hv_CenterMarginCol = new HTuple();
            HTuple hv_Mean = new HTuple(), hv_Deviation = new HTuple();
            HTuple hv_Int = new HTuple(), hv_MeanInt = new HTuple();
            HTuple hv_Mod = new HTuple(), hv_rangeMax = new HTuple();
            HTuple hv_CenterModelID = new HTuple(), hv_oriFangle = new HTuple();
            HTuple hv_oriFscale = new HTuple(), hv_orifScore = new HTuple();
            HTuple hv_CenterFangle = new HTuple(), hv_CenterFscale = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_InnerXld);
            HOperatorSet.GenEmptyObj(out ho_BeadLengthLine);
            HOperatorSet.GenEmptyObj(out ho_WidthLineMin);
            HOperatorSet.GenEmptyObj(out ho_WidthLineMax);
            HOperatorSet.GenEmptyObj(out ho_PoleContour);
            HOperatorSet.GenEmptyObj(out ho_CenterContour);
            HOperatorSet.GenEmptyObj(out ho_ROI_0);
            HOperatorSet.GenEmptyObj(out ho_ImageClear);
            HOperatorSet.GenEmptyObj(out ho_PoleRegion);
            HOperatorSet.GenEmptyObj(out ho_CenterHole);
            HOperatorSet.GenEmptyObj(out ho_CenterHoleSearch);
            HOperatorSet.GenEmptyObj(out ho_ImagePaint);
            HOperatorSet.GenEmptyObj(out ho_Contours3);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_CenterImageScaled);
            HOperatorSet.GenEmptyObj(out ho_CenterROI);
            hv_ShapeLength = new HTuple();
            hv_CenterFScore = new HTuple();
            hv_CenterFLength = new HTuple();
            hv_BeadLengthPix = new HTuple();
            hv_InnerRow = new HTuple();
            hv_InnerCol = new HTuple();
            hv_InnerMarginRow = new HTuple();
            hv_InnerMarginCol = new HTuple();
            hv_CenterFrow = new HTuple();
            hv_CenterFCol = new HTuple();
            hv_oriFrow = new HTuple();
            hv_oriFCol = new HTuple();
            hv_LengthInnerCircle = new HTuple();
            hv_MatchResult = new HTuple();
            try
            {
                //-------------------------------------------------------
                //区域范围：焊缝区域
                //函数功能：通过创建模板匹配极柱位置和中心小孔，再通过计量模型找到内径
                //可调参数：
                //参数调整建议：
                //-------------------------------------------------------
                //模板参数设定
                SetParams(out hv_SearchParams);
                hv_oriceny = 252.75;
                hv_oricenx = 262.394;
                hv_Radius = 80;
                ho_ROI_0.Dispose();
                HOperatorSet.GenCircle(out ho_ROI_0, hv_oriceny, hv_oricenx, hv_Radius);
                ho_ImageClear.Dispose();
                DrawRegion(ho_ImgRoi, ho_ROI_0, out ho_ImageClear);
                //创建极柱形状模板
                HOperatorSet.CreateShapeModel(ho_ImageClear, "auto", (new HTuple(-90)).TupleRad()
                    , (new HTuple(90)).TupleRad(), "auto", "auto", "use_polarity", "auto",
                    "auto", out hv_PoleModelID);
                //匹配极柱位置
                ho_PoleContour.Dispose();
                FindMatchShape(ho_CenterImgSearch, out ho_PoleContour, hv_PoleModelID, hv_SearchParams,
                    out hv_Frow, out hv_FCol, out hv_Fangle, out hv_Fscale, out hv_PoleFScore,
                    out hv_PoleLength);
                if ((int)(new HTuple(hv_PoleFScore.TupleGreater(0))) != 0)
                {
                    //中心小孔匹配范围
                    ho_PoleRegion.Dispose();
                    HOperatorSet.GenRegionContourXld(ho_PoleContour, out ho_PoleRegion, "filled");
                    HOperatorSet.AreaCenter(ho_PoleRegion, out hv_FArea, out hv_Frow, out hv_FCol);
                    ho_CenterHole.Dispose();
                    HOperatorSet.GenCircle(out ho_CenterHole, hv_Frow, hv_FCol, 60);
                    ho_CenterHoleSearch.Dispose();
                    HOperatorSet.GenCircle(out ho_CenterHoleSearch, hv_Frow, hv_FCol, 93);
                    ho_ImagePaint.Dispose();
                    HOperatorSet.PaintRegion(ho_CenterHoleSearch, ho_ImgCheck, out ho_ImagePaint,
                        255, "fill");
                    HOperatorSet.AlignMetrologyModel(hv_MetroHandle02, hv_OuterMarginParamA,
                        hv_OuterMarginParamB, 0);
                    HOperatorSet.SetMetrologyObjectParam(hv_MetroHandle02, "all", (new HTuple("measure_length1")).TupleConcat(
                        "measure_length2"), (new HTuple(90)).TupleConcat(5));
                    HOperatorSet.ApplyMetrologyModel(ho_ImagePaint, hv_MetroHandle02);
                    ho_Contours3.Dispose();
                    HOperatorSet.GetMetrologyObjectMeasures(out ho_Contours3, hv_MetroHandle02,
                        "all", "positive", out hv_CenterMarginRow, out hv_CenterMarginCol);
                    ho_ImageReduced.Dispose();
                    HOperatorSet.ReduceDomain(ho_ImgRoi, ho_CenterHole, out ho_ImageReduced);
                    HOperatorSet.Intensity(ho_CenterHole, ho_ImageReduced, out hv_Mean, out hv_Deviation);
                    HOperatorSet.TupleInt(hv_Mean / 10, out hv_Int);
                    HOperatorSet.TupleInt(hv_Mean, out hv_MeanInt);
                    HOperatorSet.TupleMod(hv_MeanInt, 10, out hv_Mod);
                    if ((int)(new HTuple(hv_Mod.TupleGreater(5))) != 0)
                    {
                        hv_Int = hv_Int + 1;
                    }
                    hv_rangeMax = (hv_Int - 1) * 10;
                    if ((int)(new HTuple(hv_rangeMax.TupleLessEqual(50))) != 0)
                    {
                        hv_rangeMax = 51;
                    }
                    ho_CenterImageScaled.Dispose();
                    scale_image_range(ho_ImageReduced, out ho_CenterImageScaled, 60, hv_rangeMax + 30);
                    hv_oriceny = 252.75;
                    hv_oricenx = 262.394;
                    hv_Radius = 32;
                    //创建中心小孔形状模板
                    ho_CenterROI.Dispose();
                    HOperatorSet.GenCircle(out ho_CenterROI, hv_oriceny, hv_oricenx, hv_Radius);
                    ho_ImageClear.Dispose();
                    DrawRegion(ho_CenterImageScaled, ho_CenterROI, out ho_ImageClear);
                    HOperatorSet.CreateShapeModel(ho_ImageClear, "auto", (new HTuple(-90)).TupleRad()
                        , (new HTuple(90)).TupleRad(), "auto", "auto", "use_polarity", "auto",
                        "auto", out hv_CenterModelID);
                    ho_CenterContour.Dispose();
                    FindMatchShape(ho_ImageClear, out ho_CenterContour, hv_CenterModelID, hv_SearchParams,
                        out hv_oriFrow, out hv_oriFCol, out hv_oriFangle, out hv_oriFscale, out hv_orifScore,
                        out hv_ShapeLength);
                    //匹配中心小孔位置
                    ho_CenterContour.Dispose();
                    FindMatchShape(ho_CenterImageScaled, out ho_CenterContour, hv_CenterModelID,
                        hv_SearchParams, out hv_CenterFrow, out hv_CenterFCol, out hv_CenterFangle,
                        out hv_CenterFscale, out hv_CenterFScore, out hv_CenterFLength);
                }
                if ((int)(new HTuple(hv_PoleLength.TupleEqual(0))) != 0)
                {
                    hv_MatchResult = 0;
                    ho_ROI_0.Dispose();
                    ho_ImageClear.Dispose();
                    ho_PoleRegion.Dispose();
                    ho_CenterHole.Dispose();
                    ho_CenterHoleSearch.Dispose();
                    ho_ImagePaint.Dispose();
                    ho_Contours3.Dispose();
                    ho_ImageReduced.Dispose();
                    ho_CenterImageScaled.Dispose();
                    ho_CenterROI.Dispose();

                    return;
                }
                HOperatorSet.ClearShapeModel(hv_PoleModelID);
                HOperatorSet.ClearShapeModel(hv_CenterModelID);
                //获取计量模型的测量结果(内径)
                ho_InnerXld.Dispose();
                InnerMeasurement(out ho_InnerXld, hv_MetroHandle02, hv_WindowHandle, out hv_LengthInnerCircle,
                    out hv_InnerMarginRow, out hv_InnerMarginCol);
                ho_ROI_0.Dispose();
                ho_ImageClear.Dispose();
                ho_PoleRegion.Dispose();
                ho_CenterHole.Dispose();
                ho_CenterHoleSearch.Dispose();
                ho_ImagePaint.Dispose();
                ho_Contours3.Dispose();
                ho_ImageReduced.Dispose();
                ho_CenterImageScaled.Dispose();
                ho_CenterROI.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_ROI_0.Dispose();
                ho_ImageClear.Dispose();
                ho_PoleRegion.Dispose();
                ho_CenterHole.Dispose();
                ho_CenterHoleSearch.Dispose();
                ho_ImagePaint.Dispose();
                ho_Contours3.Dispose();
                ho_ImageReduced.Dispose();
                ho_CenterImageScaled.Dispose();
                ho_CenterROI.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void InnerMeasurement(out HObject ho_InnerXld, HTuple hv_MetroHandle02,
            HTuple hv_WindowHandle, out HTuple hv_LengthInnerCircle, out HTuple hv_InnerMarginRow,
            out HTuple hv_InnerMarginCol)
        {



            // Local iconic variables 

            HObject ho_InnerContours = null, ho_InnerXldC = null;
            HObject ho_centerXld = null, ho_RegionIn = null, ho_RegionOpening = null;
            HObject ho_RegionBackGround = null, ho_RegionOpeningC = null;
            HObject ho_RegionClosingC = null, ho_CenterBackGround = null;
            HObject ho_CenterAreas = null, ho_CenterArea = null;

            // Local control variables 

            HTuple hv_InnerMarginParam = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_InnerXld);
            HOperatorSet.GenEmptyObj(out ho_InnerContours);
            HOperatorSet.GenEmptyObj(out ho_InnerXldC);
            HOperatorSet.GenEmptyObj(out ho_centerXld);
            HOperatorSet.GenEmptyObj(out ho_RegionIn);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening);
            HOperatorSet.GenEmptyObj(out ho_RegionBackGround);
            HOperatorSet.GenEmptyObj(out ho_RegionOpeningC);
            HOperatorSet.GenEmptyObj(out ho_RegionClosingC);
            HOperatorSet.GenEmptyObj(out ho_CenterBackGround);
            HOperatorSet.GenEmptyObj(out ho_CenterAreas);
            HOperatorSet.GenEmptyObj(out ho_CenterArea);
            hv_InnerMarginRow = new HTuple();
            hv_InnerMarginCol = new HTuple();
            try
            {
                //-------------------------------------------------------
                //区域范围：焊缝区域
                //函数功能：通过计量模型匹配焊缝内径，并去除偏离点
                //可调参数：
                //参数调整建议：
                //-------------------------------------------------------
                HOperatorSet.GetMetrologyObjectResult(hv_MetroHandle02, "all", "all", "result_type",
                    "all_param", out hv_InnerMarginParam);
                HOperatorSet.TupleLength(hv_InnerMarginParam, out hv_LengthInnerCircle);
                if ((int)(new HTuple(hv_LengthInnerCircle.TupleGreater(0))) != 0)
                {
                    ho_InnerContours.Dispose();
                    HOperatorSet.GetMetrologyObjectMeasures(out ho_InnerContours, hv_MetroHandle02,
                        "all", "negative", out hv_InnerMarginRow, out hv_InnerMarginCol);
                    ho_InnerXldC.Dispose();
                    HOperatorSet.GetMetrologyObjectResultContour(out ho_InnerXldC, hv_MetroHandle02,
                        "all", "all", 1.5);
                    ho_centerXld.Dispose();
                    HOperatorSet.GenCrossContourXld(out ho_centerXld, hv_InnerMarginRow, hv_InnerMarginCol,
                        12, 0);
                    HOperatorSet.TupleConcat(hv_InnerMarginRow, hv_InnerMarginRow.TupleSelect(
                        0), out hv_InnerMarginRow);
                    HOperatorSet.TupleConcat(hv_InnerMarginCol, hv_InnerMarginCol.TupleSelect(
                        0), out hv_InnerMarginCol);
                    //去掉内径偏离点
                    ho_InnerXld.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_InnerXld, hv_InnerMarginRow, hv_InnerMarginCol);
                    ho_RegionIn.Dispose();
                    HOperatorSet.GenRegionContourXld(ho_InnerXld, out ho_RegionIn, "filled");
                    ho_RegionOpening.Dispose();
                    HOperatorSet.OpeningCircle(ho_RegionIn, out ho_RegionOpening, 10);
                    ho_RegionBackGround.Dispose();
                    HOperatorSet.Complement(ho_RegionOpening, out ho_RegionBackGround);
                    ho_RegionOpeningC.Dispose();
                    HOperatorSet.OpeningCircle(ho_RegionBackGround, out ho_RegionOpeningC, 30);
                    ho_RegionClosingC.Dispose();
                    HOperatorSet.ClosingCircle(ho_RegionOpeningC, out ho_RegionClosingC, 35);
                    ho_CenterBackGround.Dispose();
                    HOperatorSet.Complement(ho_RegionClosingC, out ho_CenterBackGround);
                    ho_CenterAreas.Dispose();
                    HOperatorSet.Connection(ho_CenterBackGround, out ho_CenterAreas);
                    ho_CenterArea.Dispose();
                    HOperatorSet.SelectShapeStd(ho_CenterAreas, out ho_CenterArea, "max_area",
                        70);
                    ho_InnerXld.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_CenterArea, out ho_InnerXld, "border");
                    HOperatorSet.SetColor(hv_WindowHandle, "green");
                    HOperatorSet.DispObj(ho_InnerXld, hv_WindowHandle);
                }
                //
                ho_InnerContours.Dispose();
                ho_InnerXldC.Dispose();
                ho_centerXld.Dispose();
                ho_RegionIn.Dispose();
                ho_RegionOpening.Dispose();
                ho_RegionBackGround.Dispose();
                ho_RegionOpeningC.Dispose();
                ho_RegionClosingC.Dispose();
                ho_CenterBackGround.Dispose();
                ho_CenterAreas.Dispose();
                ho_CenterArea.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_InnerContours.Dispose();
                ho_InnerXldC.Dispose();
                ho_centerXld.Dispose();
                ho_RegionIn.Dispose();
                ho_RegionOpening.Dispose();
                ho_RegionBackGround.Dispose();
                ho_RegionOpeningC.Dispose();
                ho_RegionClosingC.Dispose();
                ho_CenterBackGround.Dispose();
                ho_CenterAreas.Dispose();
                ho_CenterArea.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void WeldSeamCheck(HObject ho_Img, HTuple hv_WindowHandle, out HTuple hv_ResultArray)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_DisplayXld, ho_ImgRoi, ho_EmptyObject;
            HObject ho_ImageReduced, ho_ImageScaled, ho_ImageGauss;
            HObject ho_ImageMean, ho_ImageEmphasize, ho_ConnectedRegions;
            HObject ho_Region, ho_RegionDilation, ho_SeamRoi, ho_SeamMarginXld = null;
            HObject ho_RegionTrans, ho_RegionClosing, ho_RegionComplement;
            HObject ho_CompleClosing, ho_RegionErosion, ho_ImageMask;
            HObject ho_RegionSeam, ho_Rectangle, ho_ImgRect01, ho_ImgRect02;
            HObject ho_ImgCheck, ho_Roi, ho_RegionDifference, ho_Contours;
            HObject ho_MeauresContours = null, ho_ExternalXld = null, ho_CenterXld = null;
            HObject ho_OuterEdgeContours = null, ho_ExternalRegion = null;
            HObject ho_RegionPoint = null, ho_OuterMarginXld = null, ho_OuterMarginRegion = null;
            HObject ho_CenterCross = null, ho_CenterArea = null, ho_CenterSearch = null;
            HObject ho_RegionCirErosion = null, ho_SeRegions = null, ho_SelectedRegions = null;
            HObject ho_SearchRegion = null, ho_SearchConRegions = null;
            HObject ho_PoleContour = null, ho_PoleCenterSearch = null, ho_ImagePaint = null;
            HObject ho_Contours3 = null, ho_ScaleCircle = null, ho_PoleScaleSearch = null;
            HObject ho_CenterImageScaled = null, ho_ROI_0 = null, ho_ImageClear = null;
            HObject ho_CenterContour = null, ho_InnerXld = null, ho_ErosionRegion = null;
            HObject ho_ErosionImgSearch = null, ho_ImgScaledSearch = null;
            HObject ho_ImgSScaledSearch = null, ho_CenterImgSearch = null;
            HObject ho_BeadLengthLine = null, ho_WidthLineMin = null, ho_WidthLineMax = null;
            HObject ho_RegionP1 = null, ho_RegionP2 = null, ho_RegionBeadMargin = null;
            HObject ho_BreakRegion = null, ho_BeadRegion = null, ho_BeadRegionImage = null;
            HObject ho_RegionBreakXld = null;

            // Local control variables 

            HTuple ExpTmpLocalVar_DispRow01 = null, ExpTmpLocalVar_DispCol01 = null;
            HTuple ExpTmpLocalVar_Interval01 = null, ExpTmpLocalVar_DispRow02 = null;
            HTuple ExpTmpLocalVar_DispCol02 = null, ExpTmpLocalVar_Interval02 = null;
            HTuple ExpTmpLocalVar_searchParams = null, hv_Indices = null;
            HTuple hv_ResoW = null, hv_ResoH = null, hv_DispFont = null;
            HTuple hv_DispSize = null, hv_BeadLengthMin = null, hv_BeadLengthMax = null;
            HTuple hv_BeadWidthMin = null, hv_BeadWidthMax = null;
            HTuple hv_BeadDiameterMin = null, hv_BeadDiameterMax = null;
            HTuple hv_BeadHumpMin = null, hv_BeadHumpMax = null, hv_BeadSagMin = null;
            HTuple hv_BeadSagMax = null, hv_CenterOffMax = null, hv_PoreBreakMax = null;
            HTuple hv_IsUsing181 = null, hv_IsUsing178Q = null, hv_IsUsing178N = null;
            HTuple hv_SeamScaleFactor = null, hv_SeamGray = null, hv_SeamGrayDilation = null;
            HTuple hv_ImgCheckDilation = null, hv_HoleAreaRadius = null;
            HTuple hv_ErosionHoleRadius = null, hv_ScaleImageMinF = null;
            HTuple hv_ScaleImageMaxF = null, hv_ScaleImageMinS = null;
            HTuple hv_ScaleImageMaxS = null, hv_MeanMaskWidth = null;
            HTuple hv_MeanMaskHeight = null, hv_SegBreakShift = null;
            HTuple hv_angleStart = null, hv_angleExtent = null, hv_scaleMin = null;
            HTuple hv_scaleMax = null, hv_minScore = null, hv_numMatch = null;
            HTuple hv_maxOverlap = null, hv_subPixel = null, hv_numLevel = null;
            HTuple hv_greed = null, hv_IsEqual = null, hv_DispMessage = new HTuple();
            HTuple hv_ErrorCode = new HTuple(), hv_DispColor = new HTuple();
            HTuple hv_Width = null, hv_Height = null, hv_Num = null;
            HTuple hv_Area = null, hv_CenterRow = null, hv_CenterCol = null;
            HTuple hv_Row = null, hv_Column = null, hv_OuterMarginParam = null;
            HTuple hv_OuterMarginParamLength = null, hv_OuterMarginRow = new HTuple();
            HTuple hv_OuterMarginCol = new HTuple(), hv_BeadCenterRow = new HTuple();
            HTuple hv_BeadCenterCol = new HTuple(), hv_BeadRadius = new HTuple();
            HTuple hv_StartPhi = new HTuple(), hv_EndPhi = new HTuple();
            HTuple hv_PointOrder = new HTuple(), hv_DistanceOuterMin = new HTuple();
            HTuple hv_DistanceMax = new HTuple(), hv_Div = new HTuple();
            HTuple hv_Floor = new HTuple(), hv_Int = new HTuple();
            HTuple hv_And = new HTuple(), hv_Index = new HTuple();
            HTuple hv_SelectedRow = new HTuple(), hv_SelectedCol = new HTuple();
            HTuple hv_MinDistance = new HTuple(), hv_RowA = new HTuple();
            HTuple hv_ColA = new HTuple(), hv_RowB = new HTuple();
            HTuple hv_ColB = new HTuple(), hv_BeadRegionRadius = new HTuple();
            HTuple hv_ShiftArea = new HTuple(), hv_UsedThreshold = new HTuple();
            HTuple hv_PoleArea = new HTuple(), hv_PoleRow = new HTuple();
            HTuple hv_PoleColumn = new HTuple(), hv_CenterMarginRow = new HTuple();
            HTuple hv_CenterMarginCol = new HTuple(), hv_ScaledMean = new HTuple();
            HTuple hv_ScaledDeviation = new HTuple(), hv_oriceny = new HTuple();
            HTuple hv_oricenx = new HTuple(), hv_Radius = new HTuple();
            HTuple hv_CenterModelID = new HTuple(), hv_oriFrow = new HTuple();
            HTuple hv_oriFCol = new HTuple(), hv_oriFangle = new HTuple();
            HTuple hv_oriFscale = new HTuple(), hv_orifScore = new HTuple();
            HTuple hv_ShapeLength = new HTuple(), hv_CenterFrow = new HTuple();
            HTuple hv_CenterFCol = new HTuple(), hv_CenterFangle = new HTuple();
            HTuple hv_CenterFscale = new HTuple(), hv_CenterFScore = new HTuple();
            HTuple hv_CenterFLength = new HTuple(), hv_LengthInnerCircle = new HTuple();
            HTuple hv_InnerMarginRow = new HTuple(), hv_InnerMarginCol = new HTuple();
            HTuple hv_PoleFScore = new HTuple(), hv_PoleLength = new HTuple();
            HTuple hv_MatchResult = new HTuple(), hv_BeadLengthPix = new HTuple();
            HTuple hv_InnerRow = new HTuple(), hv_InnerCol = new HTuple();
            HTuple hv_resultArray = new HTuple(), hv_DistanceMin = new HTuple();
            HTuple hv_SeamWidthmin = new HTuple(), hv_SeamWidthmax = new HTuple();
            HTuple hv_SeamWidth = new HTuple(), hv_Indices1 = new HTuple();
            HTuple hv_Indices2 = new HTuple(), hv_MinDistanceMin = new HTuple();
            HTuple hv_RowPointMinA = new HTuple(), hv_ColPointMinA = new HTuple();
            HTuple hv_RowPointMinB = new HTuple(), hv_ColPointMinB = new HTuple();
            HTuple hv_MinDistanceMax = new HTuple(), hv_RowPointMaxA = new HTuple();
            HTuple hv_ColPointMaxA = new HTuple(), hv_RowPointMaxB = new HTuple();
            HTuple hv_ColPointMaxB = new HTuple(), hv_RowBeadLineCenter = new HTuple();
            HTuple hv_ColBeadLineCenter = new HTuple(), hv_PointNum = new HTuple();
            HTuple hv_CrossFrow = new HTuple(), hv_CrossFCol = new HTuple();
            HTuple hv_CenterDif = new HTuple(), hv_BreakRegionNum = new HTuple();
            HTuple hv_SeamDistanceMin = null, hv_SeamWidthLength = null;
            HTuple hv_BreakArea = new HTuple(), hv_RegionBreakRa = new HTuple();
            HTuple hv_RegionBreakRb = new HTuple(), hv_BreakSize = new HTuple();
            HTuple hv_Leg = new HTuple(), hv_Wid = new HTuple(), hv_Center = new HTuple();
            HTuple hv_Break = new HTuple(), hv_Dia = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_DisplayXld);
            HOperatorSet.GenEmptyObj(out ho_ImgRoi);
            HOperatorSet.GenEmptyObj(out ho_EmptyObject);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_ImageGauss);
            HOperatorSet.GenEmptyObj(out ho_ImageMean);
            HOperatorSet.GenEmptyObj(out ho_ImageEmphasize);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            HOperatorSet.GenEmptyObj(out ho_SeamRoi);
            HOperatorSet.GenEmptyObj(out ho_SeamMarginXld);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans);
            HOperatorSet.GenEmptyObj(out ho_RegionClosing);
            HOperatorSet.GenEmptyObj(out ho_RegionComplement);
            HOperatorSet.GenEmptyObj(out ho_CompleClosing);
            HOperatorSet.GenEmptyObj(out ho_RegionErosion);
            HOperatorSet.GenEmptyObj(out ho_ImageMask);
            HOperatorSet.GenEmptyObj(out ho_RegionSeam);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImgRect01);
            HOperatorSet.GenEmptyObj(out ho_ImgRect02);
            HOperatorSet.GenEmptyObj(out ho_ImgCheck);
            HOperatorSet.GenEmptyObj(out ho_Roi);
            HOperatorSet.GenEmptyObj(out ho_RegionDifference);
            HOperatorSet.GenEmptyObj(out ho_Contours);
            HOperatorSet.GenEmptyObj(out ho_MeauresContours);
            HOperatorSet.GenEmptyObj(out ho_ExternalXld);
            HOperatorSet.GenEmptyObj(out ho_CenterXld);
            HOperatorSet.GenEmptyObj(out ho_OuterEdgeContours);
            HOperatorSet.GenEmptyObj(out ho_ExternalRegion);
            HOperatorSet.GenEmptyObj(out ho_RegionPoint);
            HOperatorSet.GenEmptyObj(out ho_OuterMarginXld);
            HOperatorSet.GenEmptyObj(out ho_OuterMarginRegion);
            HOperatorSet.GenEmptyObj(out ho_CenterCross);
            HOperatorSet.GenEmptyObj(out ho_CenterArea);
            HOperatorSet.GenEmptyObj(out ho_CenterSearch);
            HOperatorSet.GenEmptyObj(out ho_RegionCirErosion);
            HOperatorSet.GenEmptyObj(out ho_SeRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SearchRegion);
            HOperatorSet.GenEmptyObj(out ho_SearchConRegions);
            HOperatorSet.GenEmptyObj(out ho_PoleContour);
            HOperatorSet.GenEmptyObj(out ho_PoleCenterSearch);
            HOperatorSet.GenEmptyObj(out ho_ImagePaint);
            HOperatorSet.GenEmptyObj(out ho_Contours3);
            HOperatorSet.GenEmptyObj(out ho_ScaleCircle);
            HOperatorSet.GenEmptyObj(out ho_PoleScaleSearch);
            HOperatorSet.GenEmptyObj(out ho_CenterImageScaled);
            HOperatorSet.GenEmptyObj(out ho_ROI_0);
            HOperatorSet.GenEmptyObj(out ho_ImageClear);
            HOperatorSet.GenEmptyObj(out ho_CenterContour);
            HOperatorSet.GenEmptyObj(out ho_InnerXld);
            HOperatorSet.GenEmptyObj(out ho_ErosionRegion);
            HOperatorSet.GenEmptyObj(out ho_ErosionImgSearch);
            HOperatorSet.GenEmptyObj(out ho_ImgScaledSearch);
            HOperatorSet.GenEmptyObj(out ho_ImgSScaledSearch);
            HOperatorSet.GenEmptyObj(out ho_CenterImgSearch);
            HOperatorSet.GenEmptyObj(out ho_BeadLengthLine);
            HOperatorSet.GenEmptyObj(out ho_WidthLineMin);
            HOperatorSet.GenEmptyObj(out ho_WidthLineMax);
            HOperatorSet.GenEmptyObj(out ho_RegionP1);
            HOperatorSet.GenEmptyObj(out ho_RegionP2);
            HOperatorSet.GenEmptyObj(out ho_RegionBeadMargin);
            HOperatorSet.GenEmptyObj(out ho_BreakRegion);
            HOperatorSet.GenEmptyObj(out ho_BeadRegion);
            HOperatorSet.GenEmptyObj(out ho_BeadRegionImage);
            HOperatorSet.GenEmptyObj(out ho_RegionBreakXld);
            try
            {
                //**输出参数初始化***********************************************************
                //Image
                //global object Regions                 //区域对象
                //引用全局变量
                //global tuple ImageParamNamee
                //global tuple ImageParamValue
                //global tuple ProductParamNamee
                //global tuple ProductParamValue
                //global tuple DispRow01
                //global tuple DispCol01
                //global tuple Interval01
                //global tuple DispRow02
                //global tuple DispCol02
                //global tuple Interval02
                //
                //引用模型匹配参数
                //global tuple MetroModels
                //global tuple MatchModels
                //global tuple searchParams
                //
                //图像分辨率参数
                HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "ResoW", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                    out hv_ResoW);
                //
                HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "ResoH", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                    out hv_ResoH);
                //
                HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "StartRow01", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                    out ExpTmpLocalVar_DispRow01);
                ExpSetGlobalVar_DispRow01(ExpTmpLocalVar_DispRow01);
                //结果显示参数
                HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "StartCol01", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                    out ExpTmpLocalVar_DispCol01);
                ExpSetGlobalVar_DispCol01(ExpTmpLocalVar_DispCol01);
                //
                HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "Interval01", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                    out ExpTmpLocalVar_Interval01);
                ExpSetGlobalVar_Interval01(ExpTmpLocalVar_Interval01);
                //
                HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "StartRow02", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                    out ExpTmpLocalVar_DispRow02);
                ExpSetGlobalVar_DispRow02(ExpTmpLocalVar_DispRow02);
                //
                HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "StartCol02", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                    out ExpTmpLocalVar_DispCol02);
                ExpSetGlobalVar_DispCol02(ExpTmpLocalVar_DispCol02);
                //
                HOperatorSet.TupleFind(ExpGetGlobalVar_ImageParamNamee(), "Interval02", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ImageParamValue().TupleSelect(hv_Indices),
                    out ExpTmpLocalVar_Interval02);
                ExpSetGlobalVar_Interval02(ExpTmpLocalVar_Interval02);
                //
                hv_DispFont = "黑体";
                hv_DispSize = -12;
                //
                //引用焊缝控制参数
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadLengthMin",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadLengthMin);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadLengthMax",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadLengthMax);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadWidthMin",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadWidthMin);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadWidthMax",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadWidthMax);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadDiameterMin",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadDiameterMin);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadDiameterMax",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadDiameterMax);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadHumpMin",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadHumpMin);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadHumpMax",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadHumpMax);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadSagMin", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadSagMin);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "BeadSagMax", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_BeadSagMax);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "CenterOffMax",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_CenterOffMax);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "PoreBreakMax",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_PoreBreakMax);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "IsUsing181", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_IsUsing181);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "IsUsing178Q",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_IsUsing178Q);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "IsUsing178N",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_IsUsing178N);
                //
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "SeamScaleFactor",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_SeamScaleFactor);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "SeamGray", out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_SeamGray);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "SeamGrayDilation",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_SeamGrayDilation);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "ImgCheckDilation",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_ImgCheckDilation);
                //
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "HoleAreaRadius",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_HoleAreaRadius);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "ErosionHoleRadius",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_ErosionHoleRadius);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "ScaleImageMinF",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_ScaleImageMinF);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "ScaleImageMaxF",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_ScaleImageMaxF);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "ScaleImageMinS",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_ScaleImageMinS);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "ScaleImageMaxS",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_ScaleImageMaxS);
                //
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "MeanMaskWidth",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_MeanMaskWidth);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "MeanMaskHeight",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_MeanMaskHeight);
                HOperatorSet.TupleFind(ExpGetGlobalVar_ProductParamNamee(), "SegBreakShift",
                    out hv_Indices);
                HOperatorSet.TupleNumber(ExpGetGlobalVar_ProductParamValue().TupleSelect(hv_Indices),
                    out hv_SegBreakShift);
                //
                //设置匹配模型匹配参数
                hv_angleStart = (new HTuple(-90)).TupleRad();
                hv_angleExtent = (new HTuple(90)).TupleRad();
                hv_scaleMin = 0.9;
                hv_scaleMax = 1.1;
                hv_minScore = 0.2;
                hv_numMatch = 1;
                hv_maxOverlap = 0.3;
                hv_subPixel = "least_squares";
                hv_numLevel = 5;
                hv_greed = 0.9;
                ExpTmpLocalVar_searchParams = new HTuple();
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_angleStart, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_angleExtent, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_scaleMin, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_scaleMax, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_minScore, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_numMatch, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_maxOverlap, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_subPixel, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_numLevel, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                HOperatorSet.TupleConcat(ExpGetGlobalVar_searchParams(), hv_greed, out ExpTmpLocalVar_searchParams);
                ExpSetGlobalVar_searchParams(ExpTmpLocalVar_searchParams);
                //
                //输出参数
                HOperatorSet.TupleGenConst(7, 0, out hv_ResultArray);
                if (hv_ResultArray == null)
                    hv_ResultArray = new HTuple();
                hv_ResultArray[0] = 1;
                ho_DisplayXld.Dispose();
                HOperatorSet.GenEmptyObj(out ho_DisplayXld);
                ho_ImgRoi.Dispose();
                HOperatorSet.GenEmptyObj(out ho_ImgRoi);
                //
                //
                //输入图像为空时返回
                ho_EmptyObject.Dispose();
                HOperatorSet.GenEmptyObj(out ho_EmptyObject);
                HOperatorSet.TestEqualObj(ho_Img, ho_EmptyObject, out hv_IsEqual);
                if ((int)(hv_IsEqual) != 0)
                {
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    hv_DispMessage = "输入图像为空，请检查确认！";
                    hv_ErrorCode = 04;
                    hv_DispColor = "red";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 0),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    DispMessageMatch(hv_WindowHandle);
                    ho_DisplayXld.Dispose();
                    ho_ImgRoi.Dispose();
                    ho_EmptyObject.Dispose();
                    ho_ImageReduced.Dispose();
                    ho_ImageScaled.Dispose();
                    ho_ImageGauss.Dispose();
                    ho_ImageMean.Dispose();
                    ho_ImageEmphasize.Dispose();
                    ho_ConnectedRegions.Dispose();
                    ho_Region.Dispose();
                    ho_RegionDilation.Dispose();
                    ho_SeamRoi.Dispose();
                    ho_SeamMarginXld.Dispose();
                    ho_RegionTrans.Dispose();
                    ho_RegionClosing.Dispose();
                    ho_RegionComplement.Dispose();
                    ho_CompleClosing.Dispose();
                    ho_RegionErosion.Dispose();
                    ho_ImageMask.Dispose();
                    ho_RegionSeam.Dispose();
                    ho_Rectangle.Dispose();
                    ho_ImgRect01.Dispose();
                    ho_ImgRect02.Dispose();
                    ho_ImgCheck.Dispose();
                    ho_Roi.Dispose();
                    ho_RegionDifference.Dispose();
                    ho_Contours.Dispose();
                    ho_MeauresContours.Dispose();
                    ho_ExternalXld.Dispose();
                    ho_CenterXld.Dispose();
                    ho_OuterEdgeContours.Dispose();
                    ho_ExternalRegion.Dispose();
                    ho_RegionPoint.Dispose();
                    ho_OuterMarginXld.Dispose();
                    ho_OuterMarginRegion.Dispose();
                    ho_CenterCross.Dispose();
                    ho_CenterArea.Dispose();
                    ho_CenterSearch.Dispose();
                    ho_RegionCirErosion.Dispose();
                    ho_SeRegions.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_SearchRegion.Dispose();
                    ho_SearchConRegions.Dispose();
                    ho_PoleContour.Dispose();
                    ho_PoleCenterSearch.Dispose();
                    ho_ImagePaint.Dispose();
                    ho_Contours3.Dispose();
                    ho_ScaleCircle.Dispose();
                    ho_PoleScaleSearch.Dispose();
                    ho_CenterImageScaled.Dispose();
                    ho_ROI_0.Dispose();
                    ho_ImageClear.Dispose();
                    ho_CenterContour.Dispose();
                    ho_InnerXld.Dispose();
                    ho_ErosionRegion.Dispose();
                    ho_ErosionImgSearch.Dispose();
                    ho_ImgScaledSearch.Dispose();
                    ho_ImgSScaledSearch.Dispose();
                    ho_CenterImgSearch.Dispose();
                    ho_BeadLengthLine.Dispose();
                    ho_WidthLineMin.Dispose();
                    ho_WidthLineMax.Dispose();
                    ho_RegionP1.Dispose();
                    ho_RegionP2.Dispose();
                    ho_RegionBeadMargin.Dispose();
                    ho_BreakRegion.Dispose();
                    ho_BeadRegion.Dispose();
                    ho_BeadRegionImage.Dispose();
                    ho_RegionBreakXld.Dispose();

                    return;
                }
                //
                //**图像预处理**
                //图像处理突出焊缝区域
                HOperatorSet.GetImageSize(ho_Img, out hv_Width, out hv_Height);
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_Img, ExpGetGlobalVar_Regions(), out ho_ImageReduced
                    );
                //焊缝区域灰度缩放系数
                //181 SeamScaleFactor:=-280,178Q SeamScaleFactor:= -200,178N SeamScaleFactor:=-150
                hv_SeamScaleFactor = -180;
                ho_ImageScaled.Dispose();
                HOperatorSet.ScaleImage(ho_ImageReduced, out ho_ImageScaled, 2.5, hv_SeamScaleFactor);
                ho_ImageGauss.Dispose();
                HOperatorSet.GaussFilter(ho_ImageScaled, out ho_ImageGauss, 11);
                ho_ImageMean.Dispose();
                HOperatorSet.MeanImage(ho_ImageGauss, out ho_ImageMean, 3, 3);
                ho_ImageEmphasize.Dispose();
                HOperatorSet.Emphasize(ho_ImageMean, out ho_ImageEmphasize, 200, 200, 1);
                //
                //筛选焊缝区域
                //焊缝区域灰度值
                //181 SeamGray:=120,178Q SeamGray:=140,178N SeamGray:=130
                //
                //add
                //mean_image (ImageReduced, ImageMean1, 30, 30)
                //dyn_threshold (ImageReduced, ImageMean1, RegionDynThresh, 20, 'dark')
                //kirsch_amp (ImageReduced, ImageEdgeAmp)
                //connection (ImageEdgeAmp, ConnectedRegions)
                //endadd
                //
                //
                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_ImageScaled, out ho_Region, 0, hv_SeamGray);
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.OpeningCircle(ho_Region, out ExpTmpOutVar_0, 1.5);
                    ho_Region.Dispose();
                    ho_Region = ExpTmpOutVar_0;
                }
                //181 SeamGrayDilation:=0.5 , 178N SeamGrayDilation:=7 , 178Q SeamGrayDilation:=0.5
                ho_RegionDilation.Dispose();
                HOperatorSet.DilationCircle(ho_Region, out ho_RegionDilation, hv_SeamGrayDilation);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_RegionDilation, out ho_ConnectedRegions);
                //
                //
                ho_SeamRoi.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SeamRoi, ((((new HTuple("roundness")).TupleConcat(
                    "circularity")).TupleConcat("ra")).TupleConcat("rb")).TupleConcat("area"),
                    "and", ((((new HTuple(0.4)).TupleConcat(0.3)).TupleConcat(150)).TupleConcat(
                    150)).TupleConcat(50000), ((((new HTuple(1)).TupleConcat(1)).TupleConcat(
                    450)).TupleConcat(450)).TupleConcat(999999));
                HOperatorSet.CountObj(ho_SeamRoi, out hv_Num);
                if ((int)(new HTuple(hv_Num.TupleNotEqual(1))) != 0)
                {
                    ho_SeamMarginXld.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Region, out ho_SeamMarginXld, "border_holes");
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_DisplayXld, ho_SeamMarginXld, out ExpTmpOutVar_0
                            );
                        ho_DisplayXld.Dispose();
                        ho_DisplayXld = ExpTmpOutVar_0;
                    }
                    HOperatorSet.SetColor(hv_WindowHandle, "red");
                    HOperatorSet.DispObj(ho_Img, hv_WindowHandle);
                    HOperatorSet.DispObj(ho_DisplayXld, hv_WindowHandle);
                    hv_ErrorCode = 04;
                    hv_DispMessage = "未找到焊缝区域";
                    hv_DispColor = "red";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 4),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    ho_DisplayXld.Dispose();
                    ho_ImgRoi.Dispose();
                    ho_EmptyObject.Dispose();
                    ho_ImageReduced.Dispose();
                    ho_ImageScaled.Dispose();
                    ho_ImageGauss.Dispose();
                    ho_ImageMean.Dispose();
                    ho_ImageEmphasize.Dispose();
                    ho_ConnectedRegions.Dispose();
                    ho_Region.Dispose();
                    ho_RegionDilation.Dispose();
                    ho_SeamRoi.Dispose();
                    ho_SeamMarginXld.Dispose();
                    ho_RegionTrans.Dispose();
                    ho_RegionClosing.Dispose();
                    ho_RegionComplement.Dispose();
                    ho_CompleClosing.Dispose();
                    ho_RegionErosion.Dispose();
                    ho_ImageMask.Dispose();
                    ho_RegionSeam.Dispose();
                    ho_Rectangle.Dispose();
                    ho_ImgRect01.Dispose();
                    ho_ImgRect02.Dispose();
                    ho_ImgCheck.Dispose();
                    ho_Roi.Dispose();
                    ho_RegionDifference.Dispose();
                    ho_Contours.Dispose();
                    ho_MeauresContours.Dispose();
                    ho_ExternalXld.Dispose();
                    ho_CenterXld.Dispose();
                    ho_OuterEdgeContours.Dispose();
                    ho_ExternalRegion.Dispose();
                    ho_RegionPoint.Dispose();
                    ho_OuterMarginXld.Dispose();
                    ho_OuterMarginRegion.Dispose();
                    ho_CenterCross.Dispose();
                    ho_CenterArea.Dispose();
                    ho_CenterSearch.Dispose();
                    ho_RegionCirErosion.Dispose();
                    ho_SeRegions.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_SearchRegion.Dispose();
                    ho_SearchConRegions.Dispose();
                    ho_PoleContour.Dispose();
                    ho_PoleCenterSearch.Dispose();
                    ho_ImagePaint.Dispose();
                    ho_Contours3.Dispose();
                    ho_ScaleCircle.Dispose();
                    ho_PoleScaleSearch.Dispose();
                    ho_CenterImageScaled.Dispose();
                    ho_ROI_0.Dispose();
                    ho_ImageClear.Dispose();
                    ho_CenterContour.Dispose();
                    ho_InnerXld.Dispose();
                    ho_ErosionRegion.Dispose();
                    ho_ErosionImgSearch.Dispose();
                    ho_ImgScaledSearch.Dispose();
                    ho_ImgSScaledSearch.Dispose();
                    ho_CenterImgSearch.Dispose();
                    ho_BeadLengthLine.Dispose();
                    ho_WidthLineMin.Dispose();
                    ho_WidthLineMax.Dispose();
                    ho_RegionP1.Dispose();
                    ho_RegionP2.Dispose();
                    ho_RegionBeadMargin.Dispose();
                    ho_BreakRegion.Dispose();
                    ho_BeadRegion.Dispose();
                    ho_BeadRegionImage.Dispose();
                    ho_RegionBreakXld.Dispose();

                    return;
                }
                ho_RegionTrans.Dispose();
                HOperatorSet.ShapeTrans(ho_SeamRoi, out ho_RegionTrans, "convex");
                ho_RegionClosing.Dispose();
                HOperatorSet.ClosingCircle(ho_RegionTrans, out ho_RegionClosing, 15);
                ho_RegionComplement.Dispose();
                HOperatorSet.Complement(ho_RegionClosing, out ho_RegionComplement);
                ho_CompleClosing.Dispose();
                HOperatorSet.ClosingCircle(ho_RegionComplement, out ho_CompleClosing, 20);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_CompleClosing, out ho_RegionErosion, 3.5);
                ho_ImageMask.Dispose();
                HOperatorSet.PaintRegion(ho_RegionErosion, ho_ImageScaled, out ho_ImageMask,
                    255, "fill");
                ho_RegionSeam.Dispose();
                HOperatorSet.Complement(ho_CompleClosing, out ho_RegionSeam);
                HOperatorSet.AreaCenter(ho_RegionSeam, out hv_Area, out hv_CenterRow, out hv_CenterCol);
                if ((int)(new HTuple(hv_Area.TupleLess(1))) != 0)
                {
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    DispMessageMatch(hv_WindowHandle);
                    hv_ErrorCode = 04;
                    ho_DisplayXld.Dispose();
                    ho_ImgRoi.Dispose();
                    ho_EmptyObject.Dispose();
                    ho_ImageReduced.Dispose();
                    ho_ImageScaled.Dispose();
                    ho_ImageGauss.Dispose();
                    ho_ImageMean.Dispose();
                    ho_ImageEmphasize.Dispose();
                    ho_ConnectedRegions.Dispose();
                    ho_Region.Dispose();
                    ho_RegionDilation.Dispose();
                    ho_SeamRoi.Dispose();
                    ho_SeamMarginXld.Dispose();
                    ho_RegionTrans.Dispose();
                    ho_RegionClosing.Dispose();
                    ho_RegionComplement.Dispose();
                    ho_CompleClosing.Dispose();
                    ho_RegionErosion.Dispose();
                    ho_ImageMask.Dispose();
                    ho_RegionSeam.Dispose();
                    ho_Rectangle.Dispose();
                    ho_ImgRect01.Dispose();
                    ho_ImgRect02.Dispose();
                    ho_ImgCheck.Dispose();
                    ho_Roi.Dispose();
                    ho_RegionDifference.Dispose();
                    ho_Contours.Dispose();
                    ho_MeauresContours.Dispose();
                    ho_ExternalXld.Dispose();
                    ho_CenterXld.Dispose();
                    ho_OuterEdgeContours.Dispose();
                    ho_ExternalRegion.Dispose();
                    ho_RegionPoint.Dispose();
                    ho_OuterMarginXld.Dispose();
                    ho_OuterMarginRegion.Dispose();
                    ho_CenterCross.Dispose();
                    ho_CenterArea.Dispose();
                    ho_CenterSearch.Dispose();
                    ho_RegionCirErosion.Dispose();
                    ho_SeRegions.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_SearchRegion.Dispose();
                    ho_SearchConRegions.Dispose();
                    ho_PoleContour.Dispose();
                    ho_PoleCenterSearch.Dispose();
                    ho_ImagePaint.Dispose();
                    ho_Contours3.Dispose();
                    ho_ScaleCircle.Dispose();
                    ho_PoleScaleSearch.Dispose();
                    ho_CenterImageScaled.Dispose();
                    ho_ROI_0.Dispose();
                    ho_ImageClear.Dispose();
                    ho_CenterContour.Dispose();
                    ho_InnerXld.Dispose();
                    ho_ErosionRegion.Dispose();
                    ho_ErosionImgSearch.Dispose();
                    ho_ImgScaledSearch.Dispose();
                    ho_ImgSScaledSearch.Dispose();
                    ho_CenterImgSearch.Dispose();
                    ho_BeadLengthLine.Dispose();
                    ho_WidthLineMin.Dispose();
                    ho_WidthLineMax.Dispose();
                    ho_RegionP1.Dispose();
                    ho_RegionP2.Dispose();
                    ho_RegionBeadMargin.Dispose();
                    ho_BreakRegion.Dispose();
                    ho_BeadRegion.Dispose();
                    ho_BeadRegionImage.Dispose();
                    ho_RegionBreakXld.Dispose();

                    return;
                }
                ho_Rectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle, hv_CenterRow - 255, hv_CenterCol - 255,
                    hv_CenterRow + 256, hv_CenterCol + 256);
                //
                //分割出焊缝区域图像用于AI检测
                ho_ImgRect01.Dispose();
                HOperatorSet.ReduceDomain(ho_Img, ho_Rectangle, out ho_ImgRect01);
                ho_ImgRoi.Dispose();
                HOperatorSet.CropDomain(ho_ImgRect01, out ho_ImgRoi);
                HOperatorSet.DispObj(ho_ImgRoi, hv_WindowHandle);
                //分割出焊缝区域图像用于传统检测
                ho_ImgRect02.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageMask, ho_Rectangle, out ho_ImgRect02);
                ho_ImgCheck.Dispose();
                HOperatorSet.CropDomain(ho_ImgRect02, out ho_ImgCheck);
                //
                //重新在大概分割后的图像中查找焊缝区域
                ho_Region.Dispose();
                HOperatorSet.Threshold(ho_ImgCheck, out ho_Region, 0, hv_SeamGray);
                //181 ImgCheckDilation:=0.5 , 178N ImgCheckDilation:=7 , 178Q ImgCheckDilation:=0.5
                ho_RegionDilation.Dispose();
                HOperatorSet.DilationCircle(ho_Region, out ho_RegionDilation, hv_ImgCheckDilation);
                ho_ConnectedRegions.Dispose();
                HOperatorSet.Connection(ho_RegionDilation, out ho_ConnectedRegions);
                ho_Roi.Dispose();
                HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_Roi, (((new HTuple("roundness")).TupleConcat(
                    "circularity")).TupleConcat("ra")).TupleConcat("rb"), "and", (((new HTuple(0.4)).TupleConcat(
                    0.3)).TupleConcat(150)).TupleConcat(150), (((new HTuple(1)).TupleConcat(
                    1)).TupleConcat(450)).TupleConcat(450));
                HOperatorSet.CountObj(ho_Roi, out hv_Num);
                if ((int)(new HTuple(hv_Num.TupleNotEqual(1))) != 0)
                {
                    ho_SeamMarginXld.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_Region, out ho_SeamMarginXld, "border_holes");
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_DisplayXld, ho_SeamMarginXld, out ExpTmpOutVar_0
                            );
                        ho_DisplayXld.Dispose();
                        ho_DisplayXld = ExpTmpOutVar_0;
                    }
                    HOperatorSet.SetColor(hv_WindowHandle, "red");
                    HOperatorSet.DispObj(ho_Img, hv_WindowHandle);
                    HOperatorSet.DispObj(ho_DisplayXld, hv_WindowHandle);
                    DispMessageMatch(hv_WindowHandle);
                    hv_ErrorCode = 04;
                    ho_DisplayXld.Dispose();
                    ho_ImgRoi.Dispose();
                    ho_EmptyObject.Dispose();
                    ho_ImageReduced.Dispose();
                    ho_ImageScaled.Dispose();
                    ho_ImageGauss.Dispose();
                    ho_ImageMean.Dispose();
                    ho_ImageEmphasize.Dispose();
                    ho_ConnectedRegions.Dispose();
                    ho_Region.Dispose();
                    ho_RegionDilation.Dispose();
                    ho_SeamRoi.Dispose();
                    ho_SeamMarginXld.Dispose();
                    ho_RegionTrans.Dispose();
                    ho_RegionClosing.Dispose();
                    ho_RegionComplement.Dispose();
                    ho_CompleClosing.Dispose();
                    ho_RegionErosion.Dispose();
                    ho_ImageMask.Dispose();
                    ho_RegionSeam.Dispose();
                    ho_Rectangle.Dispose();
                    ho_ImgRect01.Dispose();
                    ho_ImgRect02.Dispose();
                    ho_ImgCheck.Dispose();
                    ho_Roi.Dispose();
                    ho_RegionDifference.Dispose();
                    ho_Contours.Dispose();
                    ho_MeauresContours.Dispose();
                    ho_ExternalXld.Dispose();
                    ho_CenterXld.Dispose();
                    ho_OuterEdgeContours.Dispose();
                    ho_ExternalRegion.Dispose();
                    ho_RegionPoint.Dispose();
                    ho_OuterMarginXld.Dispose();
                    ho_OuterMarginRegion.Dispose();
                    ho_CenterCross.Dispose();
                    ho_CenterArea.Dispose();
                    ho_CenterSearch.Dispose();
                    ho_RegionCirErosion.Dispose();
                    ho_SeRegions.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_SearchRegion.Dispose();
                    ho_SearchConRegions.Dispose();
                    ho_PoleContour.Dispose();
                    ho_PoleCenterSearch.Dispose();
                    ho_ImagePaint.Dispose();
                    ho_Contours3.Dispose();
                    ho_ScaleCircle.Dispose();
                    ho_PoleScaleSearch.Dispose();
                    ho_CenterImageScaled.Dispose();
                    ho_ROI_0.Dispose();
                    ho_ImageClear.Dispose();
                    ho_CenterContour.Dispose();
                    ho_InnerXld.Dispose();
                    ho_ErosionRegion.Dispose();
                    ho_ErosionImgSearch.Dispose();
                    ho_ImgScaledSearch.Dispose();
                    ho_ImgSScaledSearch.Dispose();
                    ho_CenterImgSearch.Dispose();
                    ho_BeadLengthLine.Dispose();
                    ho_WidthLineMin.Dispose();
                    ho_WidthLineMax.Dispose();
                    ho_RegionP1.Dispose();
                    ho_RegionP2.Dispose();
                    ho_RegionBeadMargin.Dispose();
                    ho_BreakRegion.Dispose();
                    ho_BeadRegion.Dispose();
                    ho_BeadRegionImage.Dispose();
                    ho_RegionBreakXld.Dispose();

                    return;
                }
                ho_RegionTrans.Dispose();
                HOperatorSet.ShapeTrans(ho_Roi, out ho_RegionTrans, "convex");
                ho_RegionClosing.Dispose();
                HOperatorSet.ClosingCircle(ho_RegionTrans, out ho_RegionClosing, 15);
                ho_RegionDifference.Dispose();
                HOperatorSet.Difference(ho_ImgCheck, ho_RegionClosing, out ho_RegionDifference
                    );
                ho_CompleClosing.Dispose();
                HOperatorSet.ClosingCircle(ho_RegionDifference, out ho_CompleClosing, 20);
                ho_RegionErosion.Dispose();
                HOperatorSet.ErosionCircle(ho_CompleClosing, out ho_RegionErosion, 3.5);
                ho_ImageMask.Dispose();
                HOperatorSet.PaintRegion(ho_RegionErosion, ho_ImgCheck, out ho_ImageMask, 255,
                    "fill");
                ho_RegionSeam.Dispose();
                HOperatorSet.Difference(ho_ImgCheck, ho_CompleClosing, out ho_RegionSeam);
                HOperatorSet.AreaCenter(ho_RegionSeam, out hv_Area, out hv_CenterRow, out hv_CenterCol);
                if ((int)(new HTuple(hv_Area.TupleLess(1))) != 0)
                {
                    hv_ErrorCode = 04;
                    DispMessageMatch(hv_WindowHandle);
                    ho_DisplayXld.Dispose();
                    ho_ImgRoi.Dispose();
                    ho_EmptyObject.Dispose();
                    ho_ImageReduced.Dispose();
                    ho_ImageScaled.Dispose();
                    ho_ImageGauss.Dispose();
                    ho_ImageMean.Dispose();
                    ho_ImageEmphasize.Dispose();
                    ho_ConnectedRegions.Dispose();
                    ho_Region.Dispose();
                    ho_RegionDilation.Dispose();
                    ho_SeamRoi.Dispose();
                    ho_SeamMarginXld.Dispose();
                    ho_RegionTrans.Dispose();
                    ho_RegionClosing.Dispose();
                    ho_RegionComplement.Dispose();
                    ho_CompleClosing.Dispose();
                    ho_RegionErosion.Dispose();
                    ho_ImageMask.Dispose();
                    ho_RegionSeam.Dispose();
                    ho_Rectangle.Dispose();
                    ho_ImgRect01.Dispose();
                    ho_ImgRect02.Dispose();
                    ho_ImgCheck.Dispose();
                    ho_Roi.Dispose();
                    ho_RegionDifference.Dispose();
                    ho_Contours.Dispose();
                    ho_MeauresContours.Dispose();
                    ho_ExternalXld.Dispose();
                    ho_CenterXld.Dispose();
                    ho_OuterEdgeContours.Dispose();
                    ho_ExternalRegion.Dispose();
                    ho_RegionPoint.Dispose();
                    ho_OuterMarginXld.Dispose();
                    ho_OuterMarginRegion.Dispose();
                    ho_CenterCross.Dispose();
                    ho_CenterArea.Dispose();
                    ho_CenterSearch.Dispose();
                    ho_RegionCirErosion.Dispose();
                    ho_SeRegions.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_SearchRegion.Dispose();
                    ho_SearchConRegions.Dispose();
                    ho_PoleContour.Dispose();
                    ho_PoleCenterSearch.Dispose();
                    ho_ImagePaint.Dispose();
                    ho_Contours3.Dispose();
                    ho_ScaleCircle.Dispose();
                    ho_PoleScaleSearch.Dispose();
                    ho_CenterImageScaled.Dispose();
                    ho_ROI_0.Dispose();
                    ho_ImageClear.Dispose();
                    ho_CenterContour.Dispose();
                    ho_InnerXld.Dispose();
                    ho_ErosionRegion.Dispose();
                    ho_ErosionImgSearch.Dispose();
                    ho_ImgScaledSearch.Dispose();
                    ho_ImgSScaledSearch.Dispose();
                    ho_CenterImgSearch.Dispose();
                    ho_BeadLengthLine.Dispose();
                    ho_WidthLineMin.Dispose();
                    ho_WidthLineMax.Dispose();
                    ho_RegionP1.Dispose();
                    ho_RegionP2.Dispose();
                    ho_RegionBeadMargin.Dispose();
                    ho_BreakRegion.Dispose();
                    ho_BeadRegion.Dispose();
                    ho_BeadRegionImage.Dispose();
                    ho_RegionBreakXld.Dispose();

                    return;
                }
                //
                //模板匹配参数设置
                //TemplateMatching ()
                //测量模型先测量焊缝外边缘
                HOperatorSet.SetMetrologyObjectParam(ExpGetGlobalVar_MetroModels().TupleSelect(
                    0), "all", "measure_threshold", 21);
                HOperatorSet.SetMetrologyObjectParam(ExpGetGlobalVar_MetroModels().TupleSelect(
                    1), "all", "measure_threshold", 25);
                //
                //对齐测量模型，应用预处理后图像进行测量
                HOperatorSet.AlignMetrologyModel(ExpGetGlobalVar_MetroModels().TupleSelect(
                    0), hv_CenterRow, hv_CenterCol, 0);
                HOperatorSet.SetMetrologyObjectParam(ExpGetGlobalVar_MetroModels().TupleSelect(
                    0), "all", (new HTuple("measure_length1")).TupleConcat("measure_length2"),
                    (new HTuple(90)).TupleConcat(5));
                HOperatorSet.ApplyMetrologyModel(ho_ImgCheck, ExpGetGlobalVar_MetroModels().TupleSelect(
                    0));
                ho_Contours.Dispose();
                HOperatorSet.GetMetrologyObjectMeasures(out ho_Contours, ExpGetGlobalVar_MetroModels().TupleSelect(
                    0), "all", "all", out hv_Row, out hv_Column);
                HOperatorSet.GetMetrologyObjectResult(ExpGetGlobalVar_MetroModels().TupleSelect(
                    0), "all", "all", "result_type", "all_param", out hv_OuterMarginParam);
                HOperatorSet.TupleLength(hv_OuterMarginParam, out hv_OuterMarginParamLength);
                //
                //找到外边缘后测量内边缘并匹配观察孔
                if ((int)(new HTuple(hv_OuterMarginParamLength.TupleGreater(0))) != 0)
                {
                    ho_MeauresContours.Dispose();
                    HOperatorSet.GetMetrologyObjectMeasures(out ho_MeauresContours, ExpGetGlobalVar_MetroModels().TupleSelect(
                        0), "all", "positive", out hv_OuterMarginRow, out hv_OuterMarginCol);
                    ho_ExternalXld.Dispose();
                    HOperatorSet.GetMetrologyObjectResultContour(out ho_ExternalXld, ExpGetGlobalVar_MetroModels().TupleSelect(
                        0), "all", "all", 1.5);
                    ho_CenterXld.Dispose();
                    HOperatorSet.GenCrossContourXld(out ho_CenterXld, hv_OuterMarginRow, hv_OuterMarginCol,
                        12, 0);
                    //焊缝边缘提取
                    ho_OuterEdgeContours.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_RegionTrans, out ho_OuterEdgeContours,
                        "border");
                    //焊缝外边缘尺寸
                    HOperatorSet.FitCircleContourXld(ho_OuterEdgeContours, "atukey", -1, 0, 0,
                        3, 2, out hv_BeadCenterRow, out hv_BeadCenterCol, out hv_BeadRadius,
                        out hv_StartPhi, out hv_EndPhi, out hv_PointOrder);
                    HOperatorSet.TupleConcat(hv_OuterMarginRow, hv_OuterMarginRow.TupleSelect(
                        0), out hv_OuterMarginRow);
                    HOperatorSet.TupleConcat(hv_OuterMarginCol, hv_OuterMarginCol.TupleSelect(
                        0), out hv_OuterMarginCol);
                    //去除偏离比较远点的点，如焊渣等干扰
                    HOperatorSet.DistancePc(ho_ExternalXld, hv_OuterMarginRow, hv_OuterMarginCol,
                        out hv_DistanceOuterMin, out hv_DistanceMax);
                    HOperatorSet.TupleDiv(hv_DistanceOuterMin, 5, out hv_Div);
                    HOperatorSet.TupleFloor(hv_Div, out hv_Floor);
                    HOperatorSet.TupleInt(hv_Floor, out hv_Int);
                    HOperatorSet.TupleAnd(hv_Int, 1, out hv_And);
                    HOperatorSet.TupleFind(hv_And, 1, out hv_Indices);
                    if ((int)(new HTuple(hv_Indices.TupleGreaterEqual(0))) != 0)
                    {
                        ho_ExternalRegion.Dispose();
                        HOperatorSet.GenRegionContourXld(ho_ExternalXld, out ho_ExternalRegion,
                            "margin");
                        for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Indices.TupleLength()
                            )) - 1); hv_Index = (int)hv_Index + 1)
                        {
                            HOperatorSet.TupleSelect(hv_OuterMarginRow, hv_Indices.TupleSelect(hv_Index),
                                out hv_SelectedRow);
                            HOperatorSet.TupleSelect(hv_OuterMarginCol, hv_Indices.TupleSelect(hv_Index),
                                out hv_SelectedCol);
                            ho_RegionPoint.Dispose();
                            HOperatorSet.GenRegionPoints(out ho_RegionPoint, hv_SelectedRow, hv_SelectedCol);
                            HOperatorSet.DistanceRrMin(ho_RegionPoint, ho_ExternalRegion, out hv_MinDistance,
                                out hv_RowA, out hv_ColA, out hv_RowB, out hv_ColB);
                            HOperatorSet.TupleReplace(hv_OuterMarginRow, hv_Indices.TupleSelect(hv_Index),
                                hv_RowB, out hv_OuterMarginRow);
                            HOperatorSet.TupleReplace(hv_OuterMarginCol, hv_Indices.TupleSelect(hv_Index),
                                hv_ColB, out hv_OuterMarginCol);
                        }
                    }
                    ho_OuterMarginXld.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_OuterMarginXld, hv_OuterMarginRow,
                        hv_OuterMarginCol);
                    ho_OuterMarginRegion.Dispose();
                    HOperatorSet.GenRegionContourXld(ho_OuterMarginXld, out ho_OuterMarginRegion,
                        "margin");
                    HOperatorSet.RegionFeatures(ho_OuterMarginRegion, "radius", out hv_BeadRegionRadius);
                    ho_CenterCross.Dispose();
                    HOperatorSet.GenCrossContourXld(out ho_CenterCross, hv_OuterMarginParam.TupleSelect(
                        0), hv_OuterMarginParam.TupleSelect(1), 36, 0);
                    HOperatorSet.SetColor(hv_WindowHandle, "green");
                    HOperatorSet.DispObj(ho_OuterMarginXld, hv_WindowHandle);
                    HOperatorSet.DispObj(ho_CenterCross, hv_WindowHandle);
                    //
                    //观察孔大致区域
                    //181 HoleAreaRadius:=100,178Q HoleAreaRadius:= 150,178N HoleAreaRadius:= 130
                    ho_CenterArea.Dispose();
                    HOperatorSet.GenCircle(out ho_CenterArea, hv_OuterMarginParam.TupleSelect(
                        0), hv_OuterMarginParam.TupleSelect(1), hv_HoleAreaRadius);
                    ho_CenterSearch.Dispose();
                    HOperatorSet.ReduceDomain(ho_ImgRoi, ho_CenterArea, out ho_CenterSearch);
                    //观察孔区域缩小，根据孔内白色区域面积大小区分
                    ho_RegionCirErosion.Dispose();
                    HOperatorSet.ErosionCircle(ho_CenterSearch, out ho_RegionCirErosion, 8);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ReduceDomain(ho_CenterSearch, ho_RegionCirErosion, out ExpTmpOutVar_0
                            );
                        ho_CenterSearch.Dispose();
                        ho_CenterSearch = ExpTmpOutVar_0;
                    }
                    ho_SeRegions.Dispose();
                    HOperatorSet.Threshold(ho_CenterSearch, out ho_SeRegions, 160, 255);
                    ho_ConnectedRegions.Dispose();
                    HOperatorSet.Connection(ho_SeRegions, out ho_ConnectedRegions);
                    ho_SelectedRegions.Dispose();
                    HOperatorSet.SelectShapeStd(ho_ConnectedRegions, out ho_SelectedRegions,
                        "max_area", 70);
                    HOperatorSet.RegionFeatures(ho_SelectedRegions, "area", out hv_ShiftArea);
                    //根据白色区域的大小，来判定观察孔是否在正中央
                    if ((int)(new HTuple(hv_ShiftArea.TupleLess(580))) != 0)
                    {
                        //* 观察孔在正中央
                        //自动全局阈值分割
                        ho_SearchRegion.Dispose();
                        HOperatorSet.BinaryThreshold(ho_CenterSearch, out ho_SearchRegion, "max_separability",
                            "dark", out hv_UsedThreshold);
                        ho_SearchConRegions.Dispose();
                        HOperatorSet.Connection(ho_SearchRegion, out ho_SearchConRegions);
                        ho_SelectedRegions.Dispose();
                        HOperatorSet.SelectShapeStd(ho_SearchConRegions, out ho_SelectedRegions,
                            "max_area", 70);
                        ho_PoleContour.Dispose();
                        HOperatorSet.ShapeTrans(ho_SelectedRegions, out ho_PoleContour, "outer_circle");
                        ho_PoleCenterSearch.Dispose();
                        HOperatorSet.ReduceDomain(ho_CenterSearch, ho_PoleContour, out ho_PoleCenterSearch
                            );
                        //观察孔区域缩小
                        HOperatorSet.AreaCenter(ho_PoleContour, out hv_PoleArea, out hv_PoleRow,
                            out hv_PoleColumn);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ErosionCircle(ho_PoleContour, out ExpTmpOutVar_0, 48);
                            ho_PoleContour.Dispose();
                            ho_PoleContour = ExpTmpOutVar_0;
                        }
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.ReduceDomain(ho_PoleCenterSearch, ho_PoleContour, out ExpTmpOutVar_0
                                );
                            ho_PoleCenterSearch.Dispose();
                            ho_PoleCenterSearch = ExpTmpOutVar_0;
                        }
                        ho_ImagePaint.Dispose();
                        HOperatorSet.PaintRegion(ho_PoleContour, ho_PoleCenterSearch, out ho_ImagePaint,
                            255, "fill");
                        //设置焊缝内边缘模型参数
                        HOperatorSet.AlignMetrologyModel(ExpGetGlobalVar_MetroModels().TupleSelect(
                            1), hv_OuterMarginParam.TupleSelect(0), hv_OuterMarginParam.TupleSelect(
                            1), 0);
                        HOperatorSet.SetMetrologyObjectParam(ExpGetGlobalVar_MetroModels().TupleSelect(
                            1), "all", (new HTuple("measure_length1")).TupleConcat("measure_length2"),
                            (new HTuple(90)).TupleConcat(5));
                        HOperatorSet.ApplyMetrologyModel(ho_ImagePaint, ExpGetGlobalVar_MetroModels().TupleSelect(
                            1));
                        ho_Contours3.Dispose();
                        HOperatorSet.GetMetrologyObjectMeasures(out ho_Contours3, ExpGetGlobalVar_MetroModels().TupleSelect(
                            1), "all", "positive", out hv_CenterMarginRow, out hv_CenterMarginCol);
                        //进一步缩小观察孔区域，减小周围干扰
                        ho_ScaleCircle.Dispose();
                        HOperatorSet.GenCircle(out ho_ScaleCircle, hv_PoleRow, hv_PoleColumn, 60);
                        ho_PoleScaleSearch.Dispose();
                        HOperatorSet.ReduceDomain(ho_PoleCenterSearch, ho_ScaleCircle, out ho_PoleScaleSearch
                            );
                        HOperatorSet.Intensity(ho_ScaleCircle, ho_PoleScaleSearch, out hv_ScaledMean,
                            out hv_ScaledDeviation);
                        //根据中心小孔灰度值差异，缩放灰度值
                        if ((int)(new HTuple(hv_ScaledMean.TupleLess(80))) != 0)
                        {
                            ho_CenterImageScaled.Dispose();
                            scale_image_range(ho_PoleScaleSearch, out ho_CenterImageScaled, 20, 60);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.MeanImage(ho_CenterImageScaled, out ExpTmpOutVar_0, 3, 3);
                                ho_CenterImageScaled.Dispose();
                                ho_CenterImageScaled = ExpTmpOutVar_0;
                            }
                        }
                        else if ((int)((new HTuple(hv_ScaledMean.TupleGreaterEqual(
                            80))).TupleAnd(new HTuple(hv_ScaledMean.TupleLess(120)))) != 0)
                        {
                            ho_CenterImageScaled.Dispose();
                            scale_image_range(ho_PoleScaleSearch, out ho_CenterImageScaled, 60, 120);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.MeanImage(ho_CenterImageScaled, out ExpTmpOutVar_0, 3, 3);
                                ho_CenterImageScaled.Dispose();
                                ho_CenterImageScaled = ExpTmpOutVar_0;
                            }
                        }
                        else if ((int)((new HTuple(hv_ScaledMean.TupleGreaterEqual(
                            120))).TupleAnd(new HTuple(hv_ScaledMean.TupleLess(145)))) != 0)
                        {
                            ho_CenterImageScaled.Dispose();
                            scale_image_range(ho_PoleScaleSearch, out ho_CenterImageScaled, 90, 150);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.MeanImage(ho_CenterImageScaled, out ExpTmpOutVar_0, 3, 3);
                                ho_CenterImageScaled.Dispose();
                                ho_CenterImageScaled = ExpTmpOutVar_0;
                            }
                        }
                        else
                        {
                            ho_CenterImageScaled.Dispose();
                            scale_image_range(ho_PoleScaleSearch, out ho_CenterImageScaled, 120,
                                210);
                            {
                                HObject ExpTmpOutVar_0;
                                HOperatorSet.MeanImage(ho_CenterImageScaled, out ExpTmpOutVar_0, 3, 3);
                                ho_CenterImageScaled.Dispose();
                                ho_CenterImageScaled = ExpTmpOutVar_0;
                            }
                        }
                        //创建中心小孔形状模板
                        hv_oriceny = 252.75;
                        hv_oricenx = 262.394;
                        hv_Radius = 32;
                        ho_ROI_0.Dispose();
                        HOperatorSet.GenCircle(out ho_ROI_0, hv_oriceny, hv_oricenx, hv_Radius);
                        ho_ImageClear.Dispose();
                        DrawRegion(ho_CenterImageScaled, ho_ROI_0, out ho_ImageClear);
                        HOperatorSet.CreateShapeModel(ho_ImageClear, "auto", (new HTuple(-90)).TupleRad()
                            , (new HTuple(90)).TupleRad(), "auto", "auto", "use_polarity", "auto",
                            "auto", out hv_CenterModelID);
                        ho_CenterContour.Dispose();
                        FindMatchShape(ho_ImageClear, out ho_CenterContour, hv_CenterModelID, ExpGetGlobalVar_searchParams(),
                            out hv_oriFrow, out hv_oriFCol, out hv_oriFangle, out hv_oriFscale,
                            out hv_orifScore, out hv_ShapeLength);
                        //匹配中心小孔位置
                        ho_CenterContour.Dispose();
                        FindMatchShape(ho_CenterImageScaled, out ho_CenterContour, hv_CenterModelID,
                            ExpGetGlobalVar_searchParams(), out hv_CenterFrow, out hv_CenterFCol,
                            out hv_CenterFangle, out hv_CenterFscale, out hv_CenterFScore, out hv_CenterFLength);
                        HOperatorSet.ClearShapeModel(hv_CenterModelID);
                        //获取计量模型的测量结果(内径)
                        ho_InnerXld.Dispose();
                        InnerMeasurement(out ho_InnerXld, ExpGetGlobalVar_MetroModels().TupleSelect(
                            1), hv_WindowHandle, out hv_LengthInnerCircle, out hv_InnerMarginRow,
                            out hv_InnerMarginCol);
                        if ((int)((new HTuple((new HTuple((new HTuple(hv_CenterFLength.TupleEqual(
                            0))).TupleOr(new HTuple(hv_ShapeLength.TupleEqual(0))))).TupleOr(new HTuple(hv_CenterFScore.TupleEqual(
                            0))))).TupleOr(new HTuple(hv_LengthInnerCircle.TupleEqual(0)))) != 0)
                        {
                            if (hv_ResultArray == null)
                                hv_ResultArray = new HTuple();
                            hv_ResultArray[0] = -1;
                            DispMessageMatch(hv_WindowHandle);
                            hv_ErrorCode = 04;
                            ho_DisplayXld.Dispose();
                            ho_ImgRoi.Dispose();
                            ho_EmptyObject.Dispose();
                            ho_ImageReduced.Dispose();
                            ho_ImageScaled.Dispose();
                            ho_ImageGauss.Dispose();
                            ho_ImageMean.Dispose();
                            ho_ImageEmphasize.Dispose();
                            ho_ConnectedRegions.Dispose();
                            ho_Region.Dispose();
                            ho_RegionDilation.Dispose();
                            ho_SeamRoi.Dispose();
                            ho_SeamMarginXld.Dispose();
                            ho_RegionTrans.Dispose();
                            ho_RegionClosing.Dispose();
                            ho_RegionComplement.Dispose();
                            ho_CompleClosing.Dispose();
                            ho_RegionErosion.Dispose();
                            ho_ImageMask.Dispose();
                            ho_RegionSeam.Dispose();
                            ho_Rectangle.Dispose();
                            ho_ImgRect01.Dispose();
                            ho_ImgRect02.Dispose();
                            ho_ImgCheck.Dispose();
                            ho_Roi.Dispose();
                            ho_RegionDifference.Dispose();
                            ho_Contours.Dispose();
                            ho_MeauresContours.Dispose();
                            ho_ExternalXld.Dispose();
                            ho_CenterXld.Dispose();
                            ho_OuterEdgeContours.Dispose();
                            ho_ExternalRegion.Dispose();
                            ho_RegionPoint.Dispose();
                            ho_OuterMarginXld.Dispose();
                            ho_OuterMarginRegion.Dispose();
                            ho_CenterCross.Dispose();
                            ho_CenterArea.Dispose();
                            ho_CenterSearch.Dispose();
                            ho_RegionCirErosion.Dispose();
                            ho_SeRegions.Dispose();
                            ho_SelectedRegions.Dispose();
                            ho_SearchRegion.Dispose();
                            ho_SearchConRegions.Dispose();
                            ho_PoleContour.Dispose();
                            ho_PoleCenterSearch.Dispose();
                            ho_ImagePaint.Dispose();
                            ho_Contours3.Dispose();
                            ho_ScaleCircle.Dispose();
                            ho_PoleScaleSearch.Dispose();
                            ho_CenterImageScaled.Dispose();
                            ho_ROI_0.Dispose();
                            ho_ImageClear.Dispose();
                            ho_CenterContour.Dispose();
                            ho_InnerXld.Dispose();
                            ho_ErosionRegion.Dispose();
                            ho_ErosionImgSearch.Dispose();
                            ho_ImgScaledSearch.Dispose();
                            ho_ImgSScaledSearch.Dispose();
                            ho_CenterImgSearch.Dispose();
                            ho_BeadLengthLine.Dispose();
                            ho_WidthLineMin.Dispose();
                            ho_WidthLineMax.Dispose();
                            ho_RegionP1.Dispose();
                            ho_RegionP2.Dispose();
                            ho_RegionBeadMargin.Dispose();
                            ho_BreakRegion.Dispose();
                            ho_BeadRegion.Dispose();
                            ho_BeadRegionImage.Dispose();
                            ho_RegionBreakXld.Dispose();

                            return;
                        }
                    }
                    else
                    {
                        //* 观察孔有偏移
                        //缩小观察孔半径 181 ErosionHoleRadius:=1.5, 178Q ErosionHoleRadius:=30,178N ErosionHoleRadius:=1.5
                        ho_ErosionRegion.Dispose();
                        HOperatorSet.ErosionCircle(ho_RegionCirErosion, out ho_ErosionRegion, hv_ErosionHoleRadius);
                        ho_ErosionImgSearch.Dispose();
                        HOperatorSet.ReduceDomain(ho_CenterSearch, ho_ErosionRegion, out ho_ErosionImgSearch
                            );
                        //
                        //不同项目号所对应查找极柱和中心小孔
                        //第一次灰度缩放下限 181 ScaleImageMinF:=50, 178Q ScaleImageMinF:=100,178N ScaleImageMinF:=50
                        //第一次灰度缩放上限 181 ScaleImageMaxF:=300, 178Q ScaleImageMaxF:=400,178N ScaleImageMaxF:=300
                        //第二次灰度缩放下限 181 ScaleImageMinS:=20, 178Q ScaleImageMinS:=100,178N ScaleImageMinS:=20
                        //第二次灰度缩放上限 181 ScaleImageMaxS:=200, 178Q ScaleImageMaxS:=130,178N ScaleImageMaxS:=200
                        //均值滤波模板宽度 181 MeanMaskWidth:=3, 178Q MeanMaskWidth:=7,178N MeanMaskWidth:=3
                        //均值滤波模板高度 181 MeanMaskHeight:=3, 178Q MeanMaskHeight:=7,178N MeanMaskHeight:=3
                        ho_ImgScaledSearch.Dispose();
                        scale_image_range(ho_ErosionImgSearch, out ho_ImgScaledSearch, hv_ScaleImageMinF,
                            hv_ScaleImageMaxF);
                        ho_ImgSScaledSearch.Dispose();
                        scale_image_range(ho_ImgScaledSearch, out ho_ImgSScaledSearch, hv_ScaleImageMinS,
                            hv_ScaleImageMaxS);
                        ho_CenterImgSearch.Dispose();
                        HOperatorSet.MeanImage(ho_ImgSScaledSearch, out ho_CenterImgSearch, hv_MeanMaskWidth,
                            hv_MeanMaskHeight);
                        //
                        //不同项目号所对应查找极柱和中心小孔
                        //181项目
                        if ((int)(hv_IsUsing181) != 0)
                        {
                            ho_PoleContour.Dispose(); ho_CenterContour.Dispose(); ho_InnerXld.Dispose();
                            FindPoleAndHoles(ho_ErosionImgSearch, ho_ImgSScaledSearch, ho_ImgCheck,
                                out ho_PoleContour, out ho_CenterContour, out ho_InnerXld, ExpGetGlobalVar_MetroModels().TupleSelect(
                                1), hv_OuterMarginParam.TupleSelect(0), hv_OuterMarginParam.TupleSelect(
                                1), ExpGetGlobalVar_searchParams(), hv_WindowHandle, out hv_ShapeLength,
                                out hv_CenterFScore, out hv_CenterFLength, out hv_CenterFrow, out hv_CenterFCol,
                                out hv_oriFrow, out hv_oriFCol, out hv_oriceny, out hv_oricenx, out hv_InnerMarginRow,
                                out hv_InnerMarginCol);
                            hv_PoleFScore = 1;
                            hv_PoleLength = 1;
                            hv_LengthInnerCircle = 1;
                        }
                        //
                        //178Q项目
                        hv_MatchResult = 1;
                        if ((int)(hv_IsUsing178Q) != 0)
                        {
                            ho_InnerXld.Dispose(); ho_BeadLengthLine.Dispose(); ho_WidthLineMin.Dispose(); ho_WidthLineMax.Dispose(); ho_PoleContour.Dispose(); ho_CenterContour.Dispose();
                            FindPoleAndHolesB(ho_ImgRoi, ho_CenterImgSearch, ho_ImgCheck, ho_ImgSScaledSearch,
                                ho_OuterMarginXld, out ho_InnerXld, out ho_BeadLengthLine, out ho_WidthLineMin,
                                out ho_WidthLineMax, out ho_PoleContour, out ho_CenterContour, ExpGetGlobalVar_MetroModels().TupleSelect(
                                1), hv_OuterMarginParam.TupleSelect(0), hv_OuterMarginParam.TupleSelect(
                                1), hv_WindowHandle, hv_DispFont, hv_DispSize, out hv_ShapeLength,
                                out hv_CenterFScore, out hv_CenterFLength, out hv_PoleFScore, out hv_PoleLength,
                                out hv_BeadLengthPix, out hv_InnerRow, out hv_InnerCol, out hv_InnerMarginRow,
                                out hv_InnerMarginCol, out hv_CenterFrow, out hv_CenterFCol, out hv_oriFrow,
                                out hv_oriFCol, out hv_oriceny, out hv_oricenx, out hv_LengthInnerCircle,
                                out hv_MatchResult);
                            if ((int)(new HTuple(hv_MatchResult.TupleEqual(0))) != 0)
                            {
                                if (hv_ResultArray == null)
                                    hv_ResultArray = new HTuple();
                                hv_ResultArray[0] = -1;
                                DispMessageMatch(hv_WindowHandle);
                                hv_ErrorCode = 04;
                                ho_DisplayXld.Dispose();
                                ho_ImgRoi.Dispose();
                                ho_EmptyObject.Dispose();
                                ho_ImageReduced.Dispose();
                                ho_ImageScaled.Dispose();
                                ho_ImageGauss.Dispose();
                                ho_ImageMean.Dispose();
                                ho_ImageEmphasize.Dispose();
                                ho_ConnectedRegions.Dispose();
                                ho_Region.Dispose();
                                ho_RegionDilation.Dispose();
                                ho_SeamRoi.Dispose();
                                ho_SeamMarginXld.Dispose();
                                ho_RegionTrans.Dispose();
                                ho_RegionClosing.Dispose();
                                ho_RegionComplement.Dispose();
                                ho_CompleClosing.Dispose();
                                ho_RegionErosion.Dispose();
                                ho_ImageMask.Dispose();
                                ho_RegionSeam.Dispose();
                                ho_Rectangle.Dispose();
                                ho_ImgRect01.Dispose();
                                ho_ImgRect02.Dispose();
                                ho_ImgCheck.Dispose();
                                ho_Roi.Dispose();
                                ho_RegionDifference.Dispose();
                                ho_Contours.Dispose();
                                ho_MeauresContours.Dispose();
                                ho_ExternalXld.Dispose();
                                ho_CenterXld.Dispose();
                                ho_OuterEdgeContours.Dispose();
                                ho_ExternalRegion.Dispose();
                                ho_RegionPoint.Dispose();
                                ho_OuterMarginXld.Dispose();
                                ho_OuterMarginRegion.Dispose();
                                ho_CenterCross.Dispose();
                                ho_CenterArea.Dispose();
                                ho_CenterSearch.Dispose();
                                ho_RegionCirErosion.Dispose();
                                ho_SeRegions.Dispose();
                                ho_SelectedRegions.Dispose();
                                ho_SearchRegion.Dispose();
                                ho_SearchConRegions.Dispose();
                                ho_PoleContour.Dispose();
                                ho_PoleCenterSearch.Dispose();
                                ho_ImagePaint.Dispose();
                                ho_Contours3.Dispose();
                                ho_ScaleCircle.Dispose();
                                ho_PoleScaleSearch.Dispose();
                                ho_CenterImageScaled.Dispose();
                                ho_ROI_0.Dispose();
                                ho_ImageClear.Dispose();
                                ho_CenterContour.Dispose();
                                ho_InnerXld.Dispose();
                                ho_ErosionRegion.Dispose();
                                ho_ErosionImgSearch.Dispose();
                                ho_ImgScaledSearch.Dispose();
                                ho_ImgSScaledSearch.Dispose();
                                ho_CenterImgSearch.Dispose();
                                ho_BeadLengthLine.Dispose();
                                ho_WidthLineMin.Dispose();
                                ho_WidthLineMax.Dispose();
                                ho_RegionP1.Dispose();
                                ho_RegionP2.Dispose();
                                ho_RegionBeadMargin.Dispose();
                                ho_BreakRegion.Dispose();
                                ho_BeadRegion.Dispose();
                                ho_BeadRegionImage.Dispose();
                                ho_RegionBreakXld.Dispose();

                                return;
                            }
                        }
                        //
                        //178N项目
                        if ((int)(hv_IsUsing178N) != 0)
                        {
                            ho_InnerXld.Dispose(); ho_PoleContour.Dispose();
                            FindPoleAndHolesC(ho_ImgRoi, ho_CenterImgSearch, ho_ImgCheck, out ho_InnerXld,
                                out ho_PoleContour, ExpGetGlobalVar_MetroModels().TupleSelect(1),
                                hv_WindowHandle, hv_OuterMarginParam.TupleSelect(0), hv_OuterMarginParam.TupleSelect(
                                1), out hv_CenterFLength, out hv_CenterFScore, out hv_ShapeLength,
                                out hv_PoleFScore, out hv_PoleLength, out hv_LengthInnerCircle, out hv_InnerMarginRow,
                                out hv_InnerMarginCol);
                        }
                        //
                        //模型匹配失败或者未选项目号时报错
                        if ((int)((new HTuple((new HTuple(hv_IsUsing181.TupleEqual(0))).TupleAnd(
                            new HTuple(hv_IsUsing178Q.TupleEqual(0))))).TupleAnd(new HTuple(hv_IsUsing178N.TupleEqual(
                            0)))) != 0)
                        {
                            if (hv_resultArray == null)
                                hv_resultArray = new HTuple();
                            hv_resultArray[0] = -1;
                            hv_DispMessage = "请至少选择一种项目型号！";
                            hv_DispColor = "red";
                            DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 0),
                                ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                                hv_DispFont, hv_DispSize);
                            DispMessageMatch(hv_WindowHandle);
                            hv_ErrorCode = 04;
                            ho_DisplayXld.Dispose();
                            ho_ImgRoi.Dispose();
                            ho_EmptyObject.Dispose();
                            ho_ImageReduced.Dispose();
                            ho_ImageScaled.Dispose();
                            ho_ImageGauss.Dispose();
                            ho_ImageMean.Dispose();
                            ho_ImageEmphasize.Dispose();
                            ho_ConnectedRegions.Dispose();
                            ho_Region.Dispose();
                            ho_RegionDilation.Dispose();
                            ho_SeamRoi.Dispose();
                            ho_SeamMarginXld.Dispose();
                            ho_RegionTrans.Dispose();
                            ho_RegionClosing.Dispose();
                            ho_RegionComplement.Dispose();
                            ho_CompleClosing.Dispose();
                            ho_RegionErosion.Dispose();
                            ho_ImageMask.Dispose();
                            ho_RegionSeam.Dispose();
                            ho_Rectangle.Dispose();
                            ho_ImgRect01.Dispose();
                            ho_ImgRect02.Dispose();
                            ho_ImgCheck.Dispose();
                            ho_Roi.Dispose();
                            ho_RegionDifference.Dispose();
                            ho_Contours.Dispose();
                            ho_MeauresContours.Dispose();
                            ho_ExternalXld.Dispose();
                            ho_CenterXld.Dispose();
                            ho_OuterEdgeContours.Dispose();
                            ho_ExternalRegion.Dispose();
                            ho_RegionPoint.Dispose();
                            ho_OuterMarginXld.Dispose();
                            ho_OuterMarginRegion.Dispose();
                            ho_CenterCross.Dispose();
                            ho_CenterArea.Dispose();
                            ho_CenterSearch.Dispose();
                            ho_RegionCirErosion.Dispose();
                            ho_SeRegions.Dispose();
                            ho_SelectedRegions.Dispose();
                            ho_SearchRegion.Dispose();
                            ho_SearchConRegions.Dispose();
                            ho_PoleContour.Dispose();
                            ho_PoleCenterSearch.Dispose();
                            ho_ImagePaint.Dispose();
                            ho_Contours3.Dispose();
                            ho_ScaleCircle.Dispose();
                            ho_PoleScaleSearch.Dispose();
                            ho_CenterImageScaled.Dispose();
                            ho_ROI_0.Dispose();
                            ho_ImageClear.Dispose();
                            ho_CenterContour.Dispose();
                            ho_InnerXld.Dispose();
                            ho_ErosionRegion.Dispose();
                            ho_ErosionImgSearch.Dispose();
                            ho_ImgScaledSearch.Dispose();
                            ho_ImgSScaledSearch.Dispose();
                            ho_CenterImgSearch.Dispose();
                            ho_BeadLengthLine.Dispose();
                            ho_WidthLineMin.Dispose();
                            ho_WidthLineMax.Dispose();
                            ho_RegionP1.Dispose();
                            ho_RegionP2.Dispose();
                            ho_RegionBeadMargin.Dispose();
                            ho_BreakRegion.Dispose();
                            ho_BeadRegion.Dispose();
                            ho_BeadRegionImage.Dispose();
                            ho_RegionBreakXld.Dispose();

                            return;
                        }
                        //
                        if ((int)((new HTuple((new HTuple((new HTuple((new HTuple((new HTuple(hv_CenterFLength.TupleEqual(
                            0))).TupleOr(new HTuple(hv_CenterFScore.TupleEqual(0))))).TupleOr(new HTuple(hv_ShapeLength.TupleEqual(
                            0))))).TupleOr(new HTuple(hv_PoleFScore.TupleEqual(0))))).TupleOr(new HTuple(hv_PoleLength.TupleEqual(
                            0))))).TupleOr(new HTuple(hv_LengthInnerCircle.TupleEqual(0)))) != 0)
                        {
                            if (hv_ResultArray == null)
                                hv_ResultArray = new HTuple();
                            hv_ResultArray[0] = -1;
                            DispMessageMatch(hv_WindowHandle);
                            hv_ErrorCode = 04;
                            ho_DisplayXld.Dispose();
                            ho_ImgRoi.Dispose();
                            ho_EmptyObject.Dispose();
                            ho_ImageReduced.Dispose();
                            ho_ImageScaled.Dispose();
                            ho_ImageGauss.Dispose();
                            ho_ImageMean.Dispose();
                            ho_ImageEmphasize.Dispose();
                            ho_ConnectedRegions.Dispose();
                            ho_Region.Dispose();
                            ho_RegionDilation.Dispose();
                            ho_SeamRoi.Dispose();
                            ho_SeamMarginXld.Dispose();
                            ho_RegionTrans.Dispose();
                            ho_RegionClosing.Dispose();
                            ho_RegionComplement.Dispose();
                            ho_CompleClosing.Dispose();
                            ho_RegionErosion.Dispose();
                            ho_ImageMask.Dispose();
                            ho_RegionSeam.Dispose();
                            ho_Rectangle.Dispose();
                            ho_ImgRect01.Dispose();
                            ho_ImgRect02.Dispose();
                            ho_ImgCheck.Dispose();
                            ho_Roi.Dispose();
                            ho_RegionDifference.Dispose();
                            ho_Contours.Dispose();
                            ho_MeauresContours.Dispose();
                            ho_ExternalXld.Dispose();
                            ho_CenterXld.Dispose();
                            ho_OuterEdgeContours.Dispose();
                            ho_ExternalRegion.Dispose();
                            ho_RegionPoint.Dispose();
                            ho_OuterMarginXld.Dispose();
                            ho_OuterMarginRegion.Dispose();
                            ho_CenterCross.Dispose();
                            ho_CenterArea.Dispose();
                            ho_CenterSearch.Dispose();
                            ho_RegionCirErosion.Dispose();
                            ho_SeRegions.Dispose();
                            ho_SelectedRegions.Dispose();
                            ho_SearchRegion.Dispose();
                            ho_SearchConRegions.Dispose();
                            ho_PoleContour.Dispose();
                            ho_PoleCenterSearch.Dispose();
                            ho_ImagePaint.Dispose();
                            ho_Contours3.Dispose();
                            ho_ScaleCircle.Dispose();
                            ho_PoleScaleSearch.Dispose();
                            ho_CenterImageScaled.Dispose();
                            ho_ROI_0.Dispose();
                            ho_ImageClear.Dispose();
                            ho_CenterContour.Dispose();
                            ho_InnerXld.Dispose();
                            ho_ErosionRegion.Dispose();
                            ho_ErosionImgSearch.Dispose();
                            ho_ImgScaledSearch.Dispose();
                            ho_ImgSScaledSearch.Dispose();
                            ho_CenterImgSearch.Dispose();
                            ho_BeadLengthLine.Dispose();
                            ho_WidthLineMin.Dispose();
                            ho_WidthLineMax.Dispose();
                            ho_RegionP1.Dispose();
                            ho_RegionP2.Dispose();
                            ho_RegionBeadMargin.Dispose();
                            ho_BreakRegion.Dispose();
                            ho_BeadRegion.Dispose();
                            ho_BeadRegionImage.Dispose();
                            ho_RegionBreakXld.Dispose();

                            return;
                        }
                    }
                    //观察孔上各点到焊缝外圈的距离
                    HOperatorSet.DistancePc(ho_OuterMarginXld, hv_InnerMarginRow, hv_InnerMarginCol,
                        out hv_DistanceMin, out hv_DistanceMax);
                    HOperatorSet.TupleMin(hv_DistanceMin, out hv_SeamWidthmin);
                    HOperatorSet.TupleMax(hv_DistanceMin, out hv_SeamWidthmax);
                    HOperatorSet.TupleMean(hv_DistanceMin, out hv_SeamWidth);
                    //焊缝宽度显示(最远和最近)
                    HOperatorSet.TupleFind(hv_DistanceMin, hv_SeamWidthmin, out hv_Indices1);
                    HOperatorSet.TupleFind(hv_DistanceMin, hv_SeamWidthmax, out hv_Indices2);
                    HOperatorSet.GetContourXld(ho_InnerXld, out hv_InnerRow, out hv_InnerCol);
                    ho_RegionP1.Dispose();
                    HOperatorSet.GenRegionPoints(out ho_RegionP1, hv_InnerRow.TupleSelect(hv_Indices1),
                        hv_InnerCol.TupleSelect(hv_Indices1));
                    ho_RegionP2.Dispose();
                    HOperatorSet.GenRegionPoints(out ho_RegionP2, hv_InnerRow.TupleSelect(hv_Indices2),
                        hv_InnerCol.TupleSelect(hv_Indices2));
                    ho_RegionBeadMargin.Dispose();
                    HOperatorSet.GenRegionContourXld(ho_OuterMarginXld, out ho_RegionBeadMargin,
                        "margin");
                    HOperatorSet.DistanceRrMin(ho_RegionP1, ho_RegionBeadMargin, out hv_MinDistanceMin,
                        out hv_RowPointMinA, out hv_ColPointMinA, out hv_RowPointMinB, out hv_ColPointMinB);
                    HOperatorSet.DistanceRrMin(ho_RegionP2, ho_RegionBeadMargin, out hv_MinDistanceMax,
                        out hv_RowPointMaxA, out hv_ColPointMaxA, out hv_RowPointMaxB, out hv_ColPointMaxB);
                    ho_WidthLineMin.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_WidthLineMin, hv_RowPointMinA.TupleConcat(
                        hv_RowPointMinB), hv_ColPointMinA.TupleConcat(hv_ColPointMinB));
                    ho_WidthLineMax.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_WidthLineMax, hv_RowPointMaxA.TupleConcat(
                        hv_RowPointMaxB), hv_ColPointMaxA.TupleConcat(hv_ColPointMaxB));
                    //
                    //焊缝长度显示
                    hv_RowBeadLineCenter = new HTuple();
                    hv_ColBeadLineCenter = new HTuple();
                    HOperatorSet.TupleLength(hv_InnerRow, out hv_PointNum);
                    HTuple end_val452 = hv_PointNum - 1;
                    HTuple step_val452 = 1;
                    for (hv_Index = 0; hv_Index.Continue(end_val452, step_val452); hv_Index = hv_Index.TupleAdd(step_val452))
                    {
                        ho_RegionPoint.Dispose();
                        HOperatorSet.GenRegionPoints(out ho_RegionPoint, hv_InnerRow.TupleSelect(
                            hv_Index), hv_InnerCol.TupleSelect(hv_Index));
                        //焊缝内圈到外圈的最近距离和对应点
                        HOperatorSet.DistanceRrMin(ho_RegionPoint, ho_RegionBeadMargin, out hv_DistanceMin,
                            out hv_RowA, out hv_ColA, out hv_RowB, out hv_ColB);
                        HOperatorSet.TupleConcat(hv_RowBeadLineCenter, (hv_RowA + hv_RowB) * 0.5, out hv_RowBeadLineCenter);
                        HOperatorSet.TupleConcat(hv_ColBeadLineCenter, (hv_ColA + hv_ColB) * 0.5, out hv_ColBeadLineCenter);
                    }
                    //拟合圆代表焊缝长度
                    ho_BeadLengthLine.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_BeadLengthLine, hv_RowBeadLineCenter,
                        hv_ColBeadLineCenter);
                    HOperatorSet.LengthXld(ho_BeadLengthLine, out hv_BeadLengthPix);
                    //
                    //局部检测显示
                    if ((int)(new HTuple(hv_CenterFLength.TupleGreater(0))) != 0)
                    {
                        if ((int)(hv_IsUsing178Q.TupleOr(hv_IsUsing181)) != 0)
                        {
                            hv_CrossFrow = (hv_CenterFrow - hv_oriFrow) + hv_oriceny;
                            hv_CrossFCol = (hv_CenterFCol - hv_oriFCol) + hv_oricenx;
                            ho_CenterCross.Dispose();
                            HOperatorSet.GenCrossContourXld(out ho_CenterCross, hv_CrossFrow, hv_CrossFCol,
                                20, 0);
                            HOperatorSet.DistancePp(hv_CrossFrow, hv_CrossFCol, hv_OuterMarginParam.TupleSelect(
                                0), hv_OuterMarginParam.TupleSelect(1), out hv_CenterDif);
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.SetColor(HDevWindowStack.GetActive(), "green");
                            }
                            if (HDevWindowStack.IsOpen())
                            {
                                HOperatorSet.DispObj(ho_CenterContour, HDevWindowStack.GetActive());
                            }
                        }

                        DisplayImageAsOriginRatio(ho_ImgRoi, hv_WindowHandle);
                        HOperatorSet.DispObj(ho_OuterMarginXld, hv_WindowHandle);
                        HOperatorSet.SetColor(hv_WindowHandle, "green");
                        HOperatorSet.DispObj(ho_PoleContour, hv_WindowHandle);
                        HOperatorSet.DispObj(ho_CenterCross, hv_WindowHandle);
                        HOperatorSet.DispObj(ho_InnerXld, hv_WindowHandle);

                    }
                    else
                    {

                        hv_CenterDif = 0;
                        HOperatorSet.AlignMetrologyModel(ExpGetGlobalVar_MetroModels().TupleSelect(
                            1), hv_OuterMarginParam.TupleSelect(0), hv_OuterMarginParam.TupleSelect(
                            1), 0);
                        HOperatorSet.SetMetrologyObjectParam(ExpGetGlobalVar_MetroModels().TupleSelect(
                            1), "all", "measure_threshold", 10);
                        HOperatorSet.ApplyMetrologyModel(ho_ImgCheck, ExpGetGlobalVar_MetroModels().TupleSelect(
                            1));
                    }
                    //
                    //
                    //爆孔区域提取
                    ho_BreakRegion.Dispose();
                    HOperatorSet.GenEmptyRegion(out ho_BreakRegion);
                    ho_BeadRegion.Dispose();
                    HOperatorSet.GenRegionContourXld(ho_OuterMarginXld, out ho_BeadRegion, "filled");
                    ho_RegionDifference.Dispose();
                    HOperatorSet.Difference(ho_BeadRegion, ho_CenterArea, out ho_RegionDifference
                        );
                    ho_BeadRegionImage.Dispose();
                    HOperatorSet.ReduceDomain(ho_ImgRoi, ho_RegionDifference, out ho_BeadRegionImage
                        );
                    ho_BreakRegion.Dispose(); ho_RegionBreakXld.Dispose();
                    SegBreak(ho_BeadRegionImage, ho_CenterArea, ho_InnerXld, ho_OuterMarginXld,
                        out ho_BreakRegion, out ho_RegionBreakXld, hv_SegBreakShift, out hv_BreakRegionNum);
                    //
                }
                else
                {
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    DispMessageMatch(hv_WindowHandle);
                    hv_ErrorCode = 04;
                    ho_DisplayXld.Dispose();
                    ho_ImgRoi.Dispose();
                    ho_EmptyObject.Dispose();
                    ho_ImageReduced.Dispose();
                    ho_ImageScaled.Dispose();
                    ho_ImageGauss.Dispose();
                    ho_ImageMean.Dispose();
                    ho_ImageEmphasize.Dispose();
                    ho_ConnectedRegions.Dispose();
                    ho_Region.Dispose();
                    ho_RegionDilation.Dispose();
                    ho_SeamRoi.Dispose();
                    ho_SeamMarginXld.Dispose();
                    ho_RegionTrans.Dispose();
                    ho_RegionClosing.Dispose();
                    ho_RegionComplement.Dispose();
                    ho_CompleClosing.Dispose();
                    ho_RegionErosion.Dispose();
                    ho_ImageMask.Dispose();
                    ho_RegionSeam.Dispose();
                    ho_Rectangle.Dispose();
                    ho_ImgRect01.Dispose();
                    ho_ImgRect02.Dispose();
                    ho_ImgCheck.Dispose();
                    ho_Roi.Dispose();
                    ho_RegionDifference.Dispose();
                    ho_Contours.Dispose();
                    ho_MeauresContours.Dispose();
                    ho_ExternalXld.Dispose();
                    ho_CenterXld.Dispose();
                    ho_OuterEdgeContours.Dispose();
                    ho_ExternalRegion.Dispose();
                    ho_RegionPoint.Dispose();
                    ho_OuterMarginXld.Dispose();
                    ho_OuterMarginRegion.Dispose();
                    ho_CenterCross.Dispose();
                    ho_CenterArea.Dispose();
                    ho_CenterSearch.Dispose();
                    ho_RegionCirErosion.Dispose();
                    ho_SeRegions.Dispose();
                    ho_SelectedRegions.Dispose();
                    ho_SearchRegion.Dispose();
                    ho_SearchConRegions.Dispose();
                    ho_PoleContour.Dispose();
                    ho_PoleCenterSearch.Dispose();
                    ho_ImagePaint.Dispose();
                    ho_Contours3.Dispose();
                    ho_ScaleCircle.Dispose();
                    ho_PoleScaleSearch.Dispose();
                    ho_CenterImageScaled.Dispose();
                    ho_ROI_0.Dispose();
                    ho_ImageClear.Dispose();
                    ho_CenterContour.Dispose();
                    ho_InnerXld.Dispose();
                    ho_ErosionRegion.Dispose();
                    ho_ErosionImgSearch.Dispose();
                    ho_ImgScaledSearch.Dispose();
                    ho_ImgSScaledSearch.Dispose();
                    ho_CenterImgSearch.Dispose();
                    ho_BeadLengthLine.Dispose();
                    ho_WidthLineMin.Dispose();
                    ho_WidthLineMax.Dispose();
                    ho_RegionP1.Dispose();
                    ho_RegionP2.Dispose();
                    ho_RegionBeadMargin.Dispose();
                    ho_BreakRegion.Dispose();
                    ho_BeadRegion.Dispose();
                    ho_BeadRegionImage.Dispose();
                    ho_RegionBreakXld.Dispose();

                    return;
                }
                //
                //**数值结果计算**
                //焊缝长度（焊缝拟合圆的长度）
                if (hv_ResultArray == null)
                    hv_ResultArray = new HTuple();
                hv_ResultArray[1] = hv_BeadLengthPix * hv_ResoW;
                //
                //焊缝宽度（焊缝内圈上的点到焊缝外圈的最短距离）(不允许50%焊缝宽度<2.6)
                hv_SeamDistanceMin = new HTuple();
                if (hv_ResultArray == null)
                    hv_ResultArray = new HTuple();
                hv_ResultArray[2] = hv_SeamWidthmin * hv_ResoW;
                HOperatorSet.TupleLength(hv_InnerRow, out hv_SeamWidthLength);
                //SeamWidthLength := SeamWidthLength+0.01
                //SeamWidthLim := 2.6/ResoW
                //for index := 0 to |InnerRow|-1 by 1
                //distance_pc (OuterMarginXld, InnerRow[index], InnerCol[index], DistanceMin, DistanceMax)
                //tuple_concat (SeamDistanceMin, DistanceMin, SeamDistanceMin)
                //endfor
                //tuple_less_elem (SeamDistanceMin, SeamWidthLim, Less)
                //tuple_find (Less, 1, SeamWidthIndices)
                //tuple_length (SeamWidthIndices, SeamWidthLength2)
                //SeamWidthRatio := SeamWidthLength2/SeamWidthLength
                //
                //焊缝偏移（焊缝外圈的圆心与中心小孔的圆心的距离）
                //ResultArray[3] := CenterDif * ResoW
                if (hv_ResultArray == null)
                    hv_ResultArray = new HTuple();
                hv_ResultArray[3] = 0;
                //爆孔直径测量（爆孔的长短半轴中最大的2倍）
                if ((int)(new HTuple(hv_BreakRegionNum.TupleEqual(0))) != 0)
                {
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[4] = 0;
                }
                else
                {
                    ho_RegionBreakXld.Dispose();
                    HOperatorSet.GenContourRegionXld(ho_BreakRegion, out ho_RegionBreakXld, "border_holes");
                    HOperatorSet.AreaCenter(ho_BreakRegion, out hv_BreakArea, out hv_Row, out hv_Column);
                    HOperatorSet.RegionFeatures(ho_BreakRegion, "ra", out hv_RegionBreakRa);
                    HOperatorSet.RegionFeatures(ho_BreakRegion, "rb", out hv_RegionBreakRb);
                    HOperatorSet.TupleMax2(hv_RegionBreakRa, hv_RegionBreakRb, out hv_BreakSize);
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[4] = (hv_BreakSize * 2) * hv_ResoW;
                }
                //
                //焊缝直径（焊缝半径的2倍）
                if (hv_ResultArray == null)
                    hv_ResultArray = new HTuple();
                hv_ResultArray[6] = (hv_BeadRegionRadius * 2) * hv_ResoW;
                //
                //
                //**输出数据统计**
                //SeamLength := BeadLengthPix * ResoW
                //SeamWidth := SeamWidthmin * ResoW
                //CenterDif := CenterDif * ResoW
                //SeamHole := BreakSize * 2 * ResoW
                //SeamDiameter := BeadRegionRadius * 2 * ResoW
                //
                //
                //**检测结果判断并显示**
                //焊缝长度判断
                if ((int)((new HTuple(((hv_ResultArray.TupleSelect(1))).TupleGreaterEqual(hv_BeadLengthMin))).TupleAnd(
                    new HTuple(((hv_ResultArray.TupleSelect(1))).TupleLessEqual(hv_BeadLengthMax)))) != 0)
                {
                    hv_DispMessage = ("焊缝长度：OK" + (((hv_ResultArray.TupleSelect(1))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "green";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 1),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    HOperatorSet.SetColor(hv_WindowHandle, hv_DispColor);
                    HOperatorSet.DispObj(ho_BeadLengthLine, hv_WindowHandle);
                    hv_Leg = 0;
                }
                else
                {
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    hv_DispMessage = ("焊缝长度：NG" + (((hv_ResultArray.TupleSelect(1))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "red";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 1),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    HOperatorSet.SetColor(hv_WindowHandle, hv_DispColor);
                    HOperatorSet.DispObj(ho_BeadLengthLine, hv_WindowHandle);
                    hv_Leg = 1;
                }
                //
                //焊缝宽度判断
                //* if (ResultArray[2] >= BeadWidthMin and ResultArray[2] <= BeadWidthMax and SeamWidthRatio < 0.5)
                if ((int)((new HTuple(((hv_ResultArray.TupleSelect(2))).TupleGreaterEqual(hv_BeadWidthMin))).TupleAnd(
                    new HTuple(((hv_ResultArray.TupleSelect(2))).TupleLessEqual(hv_BeadWidthMax)))) != 0)
                {
                    hv_DispMessage = ("焊缝宽度：OK" + (((hv_ResultArray.TupleSelect(2))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "green";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 2),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    HOperatorSet.SetColor(hv_WindowHandle, hv_DispColor);
                    HOperatorSet.DispObj(ho_WidthLineMin, hv_WindowHandle);
                    HOperatorSet.DispObj(ho_WidthLineMax, hv_WindowHandle);
                    hv_Wid = 0;
                }
                else
                {
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    hv_DispMessage = ("焊缝宽度：NG" + (((hv_ResultArray.TupleSelect(2))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "red";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 2),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    HOperatorSet.SetColor(hv_WindowHandle, hv_DispColor);
                    HOperatorSet.DispObj(ho_WidthLineMin, hv_WindowHandle);
                    HOperatorSet.DispObj(ho_WidthLineMax, hv_WindowHandle);
                    hv_Wid = 1;
                }
                //
                //焊缝偏移判断
                if ((int)(new HTuple(((hv_ResultArray.TupleSelect(3))).TupleLessEqual(hv_CenterOffMax))) != 0)
                {
                    hv_DispMessage = ("焊缝偏移：OK" + (((hv_ResultArray.TupleSelect(3))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "green";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 3),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    //set_color (WindowHandle, DispColor)
                    //disp_obj (CenterContour, WindowHandle)
                    hv_Center = 0;
                }
                else
                {
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    hv_DispMessage = ("焊缝偏移：NG" + (((hv_ResultArray.TupleSelect(3))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "red";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 3),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    //set_color (WindowHandle, DispColor)
                    //disp_obj (CenterContour, WindowHandle)
                    hv_Center = 1;
                }
                //
                //焊缝爆孔判断
                if ((int)(new HTuple(((hv_ResultArray.TupleSelect(4))).TupleLess(hv_PoreBreakMax))) != 0)
                {
                    hv_DispMessage = ("焊缝爆孔：OK" + (((hv_ResultArray.TupleSelect(4))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "green";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 4),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    HOperatorSet.SetColor(hv_WindowHandle, hv_DispColor);
                    HOperatorSet.DispObj(ho_RegionBreakXld, hv_WindowHandle);
                    hv_Break = 0;
                }
                else
                {
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    hv_DispMessage = ("焊缝爆孔：NG" + (((hv_ResultArray.TupleSelect(4))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "red";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 4),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    HOperatorSet.SetColor(hv_WindowHandle, hv_DispColor);
                    HOperatorSet.DispObj(ho_RegionBreakXld, hv_WindowHandle);
                    hv_Break = 1;
                }
                //
                //焊缝直径判断
                if ((int)((new HTuple(((hv_ResultArray.TupleSelect(6))).TupleGreaterEqual(hv_BeadDiameterMin))).TupleAnd(
                    new HTuple(((hv_ResultArray.TupleSelect(6))).TupleLessEqual(hv_BeadDiameterMax)))) != 0)
                {
                    hv_DispMessage = ("焊缝外径：OK" + (((hv_ResultArray.TupleSelect(6))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "green";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 5),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    HOperatorSet.SetColor(hv_WindowHandle, hv_DispColor);
                    HOperatorSet.DispObj(ho_OuterMarginXld, hv_WindowHandle);
                    hv_Dia = 0;
                }
                else
                {
                    if (hv_ResultArray == null)
                        hv_ResultArray = new HTuple();
                    hv_ResultArray[0] = -1;
                    hv_DispMessage = ("焊缝外径：NG" + (((hv_ResultArray.TupleSelect(6))).TupleString(
                        "5.1f"))) + "mm";
                    hv_DispColor = "red";
                    DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * 5),
                        ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), hv_DispColor,
                        hv_DispFont, hv_DispSize);
                    HOperatorSet.SetColor(hv_WindowHandle, hv_DispColor);
                    HOperatorSet.DispObj(ho_OuterMarginXld, hv_WindowHandle);
                    hv_Dia = 1;
                }
                //
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_DisplayXld.Dispose();
                ho_ImgRoi.Dispose();
                ho_EmptyObject.Dispose();
                ho_ImageReduced.Dispose();
                ho_ImageScaled.Dispose();
                ho_ImageGauss.Dispose();
                ho_ImageMean.Dispose();
                ho_ImageEmphasize.Dispose();
                ho_ConnectedRegions.Dispose();
                ho_Region.Dispose();
                ho_RegionDilation.Dispose();
                ho_SeamRoi.Dispose();
                ho_SeamMarginXld.Dispose();
                ho_RegionTrans.Dispose();
                ho_RegionClosing.Dispose();
                ho_RegionComplement.Dispose();
                ho_CompleClosing.Dispose();
                ho_RegionErosion.Dispose();
                ho_ImageMask.Dispose();
                ho_RegionSeam.Dispose();
                ho_Rectangle.Dispose();
                ho_ImgRect01.Dispose();
                ho_ImgRect02.Dispose();
                ho_ImgCheck.Dispose();
                ho_Roi.Dispose();
                ho_RegionDifference.Dispose();
                ho_Contours.Dispose();
                ho_MeauresContours.Dispose();
                ho_ExternalXld.Dispose();
                ho_CenterXld.Dispose();
                ho_OuterEdgeContours.Dispose();
                ho_ExternalRegion.Dispose();
                ho_RegionPoint.Dispose();
                ho_OuterMarginXld.Dispose();
                ho_OuterMarginRegion.Dispose();
                ho_CenterCross.Dispose();
                ho_CenterArea.Dispose();
                ho_CenterSearch.Dispose();
                ho_RegionCirErosion.Dispose();
                ho_SeRegions.Dispose();
                ho_SelectedRegions.Dispose();
                ho_SearchRegion.Dispose();
                ho_SearchConRegions.Dispose();
                ho_PoleContour.Dispose();
                ho_PoleCenterSearch.Dispose();
                ho_ImagePaint.Dispose();
                ho_Contours3.Dispose();
                ho_ScaleCircle.Dispose();
                ho_PoleScaleSearch.Dispose();
                ho_CenterImageScaled.Dispose();
                ho_ROI_0.Dispose();
                ho_ImageClear.Dispose();
                ho_CenterContour.Dispose();
                ho_InnerXld.Dispose();
                ho_ErosionRegion.Dispose();
                ho_ErosionImgSearch.Dispose();
                ho_ImgScaledSearch.Dispose();
                ho_ImgSScaledSearch.Dispose();
                ho_CenterImgSearch.Dispose();
                ho_BeadLengthLine.Dispose();
                ho_WidthLineMin.Dispose();
                ho_WidthLineMax.Dispose();
                ho_RegionP1.Dispose();
                ho_RegionP2.Dispose();
                ho_RegionBeadMargin.Dispose();
                ho_BreakRegion.Dispose();
                ho_BeadRegion.Dispose();
                ho_BeadRegionImage.Dispose();
                ho_RegionBreakXld.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void DispMessageMatch(HTuple hv_WindowHandle)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple ExpTmpLocalVar_DispColor = null, ExpTmpLocalVar_DispFont = null;
            HTuple ExpTmpLocalVar_DispSize = null, hv_DispMessage = null;
            HTuple hv_i = null;
            // Initialize local and output iconic variables 
            //global tuple DispRow01
            //global tuple Interval01
            //global tuple DispCol01
            //global tuple Interval01
            //global tuple DispColor
            //global tuple DispFont
            //global tuple DispSize
            ExpTmpLocalVar_DispFont = "黑体";
            ExpSetGlobalVar_DispFont(ExpTmpLocalVar_DispFont);
            ExpTmpLocalVar_DispSize = -12;
            ExpSetGlobalVar_DispSize(ExpTmpLocalVar_DispSize);
            hv_DispMessage = "焊缝长度：NG";
            ExpTmpLocalVar_DispColor = "red";
            ExpSetGlobalVar_DispColor(ExpTmpLocalVar_DispColor);
            //
            for (hv_i = 1; (int)hv_i <= 5; hv_i = (int)hv_i + 1)
            {
                DispMessageUserDefine(hv_WindowHandle, hv_DispMessage, ExpGetGlobalVar_DispRow01() + (ExpGetGlobalVar_Interval01() * hv_i),
                    ExpGetGlobalVar_DispCol01(), ExpGetGlobalVar_Interval01(), ExpGetGlobalVar_DispColor(),
                    ExpGetGlobalVar_DispFont(), ExpGetGlobalVar_DispSize());
            }
            //

            return;
        }

        public void DisplayImageAsOriginRatio(HObject ho_Image, HTuple hv_WindowHandle)
        {




            // Local iconic variables 

            // Local control variables 

            HTuple hv_Row = new HTuple(), hv_Col = new HTuple();
            HTuple hv_winWidth = new HTuple(), hv_winHeight = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_picWHRatio = new HTuple(), hv_winWHRatio = new HTuple();
            HTuple hv_imgRow01 = new HTuple(), hv_imgCol01 = new HTuple();
            HTuple hv_imgRow02 = new HTuple(), hv_imgCol02 = new HTuple();
            HTuple hv_Exception = null;
            // Initialize local and output iconic variables 
            //dev_set_part用于设置图形窗口中要显示的图像部分。
            //Row1和Column1指定左上角，
            //Row2和Column2指定要显示的图像部分的右下角。
            //
            try
            {
                HOperatorSet.ClearWindow(hv_WindowHandle);
                HOperatorSet.SetWindowParam(hv_WindowHandle, "save_depth_buffer", "true");
                //get_window_extents (WindowHandle, Row, Col, winWidth, winHeight)
                //set_window_extents (WindowHandle, 0, 0, winWidth, winHeight)
                HOperatorSet.SetSystem("int_zooming", "false");
                //
                HOperatorSet.GetWindowExtents(hv_WindowHandle, out hv_Row, out hv_Col, out hv_winWidth,
                    out hv_winHeight);
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                //
                hv_picWHRatio = (1.0 * hv_Width) / hv_Height;
                hv_winWHRatio = (1.0 * hv_winWidth) / hv_winHeight;
                //
                //如果图片宽高比 大于 窗口宽高比
                //则宽度方向顶格
                if ((int)(new HTuple(hv_picWHRatio.TupleGreaterEqual(hv_winWHRatio))) != 0)
                {
                    hv_imgRow01 = (-(((hv_winHeight * hv_Width) / hv_winWidth) - hv_Height)) * 0.5;
                    hv_imgCol01 = -1;
                    hv_imgRow02 = hv_Height + ((((hv_winHeight * hv_Width) / hv_winWidth) - hv_Height) * 0.5);
                    hv_imgCol02 = hv_Width.Clone();
                    HOperatorSet.SetPart(hv_WindowHandle, hv_imgRow01, hv_imgCol01, hv_imgRow02,
                        hv_imgCol02);
                    HOperatorSet.ClearWindow(hv_WindowHandle);
                    HOperatorSet.DispObj(ho_Image, hv_WindowHandle);
                }
                //
                //如果图片宽高比 小于 窗口宽高比
                //则高度方向顶格
                //
                if ((int)(new HTuple(hv_picWHRatio.TupleLess(hv_winWHRatio))) != 0)
                {
                    hv_imgRow01 = -1;
                    hv_imgCol01 = (-(((hv_winWidth * hv_Height) / hv_winHeight) - hv_Width)) * 0.5;
                    hv_imgRow02 = hv_Height.Clone();
                    hv_imgCol02 = hv_Width + ((((hv_winWidth * hv_Height) / hv_winHeight) - hv_Width) * 0.5);
                    HOperatorSet.SetPart(hv_WindowHandle, hv_imgRow01, hv_imgCol01, hv_imgRow02,
                        hv_imgCol02);
                    HOperatorSet.ClearWindow(hv_WindowHandle);
                    HOperatorSet.DispObj(ho_Image, hv_WindowHandle);
                }
                //
            }
            // catch (Exception) 
            catch (HalconException HDevExpDefaultException1)
            {
                HDevExpDefaultException1.ToHTuple(out hv_Exception);
                //
            }
            //

            return;
        }

        static public void DispMessageUserDefine(HTuple hv_WindowHandle, HTuple hv_MessageInfo,
                            HTuple hv_Row, HTuple hv_Col, HTuple hv_RowHeight, HTuple hv_Color, HTuple hv_Front,
                            HTuple hv_FrontSize)
        {
            // Local control variables 

            HTuple hv_Substrings = null, hv_Length = null;
            HTuple hv_I = null;
            // Initialize local and output iconic variables 

            HOperatorSet.SetColor(hv_WindowHandle, hv_Color);
            HOperatorSet.SetFont(hv_WindowHandle, "-" + hv_Front + hv_FrontSize + "-");

            HOperatorSet.TupleSplit(hv_MessageInfo, "\r\n", out hv_Substrings);

            HOperatorSet.TupleLength(hv_Substrings, out hv_Length);
            HTuple end_val7 = hv_Length - 1;
            HTuple step_val7 = 1;
            for (hv_I = 0; hv_I.Continue(end_val7, step_val7); hv_I = hv_I.TupleAdd(step_val7))
            {
                HOperatorSet.SetTposition(hv_WindowHandle, hv_Row + (hv_I * hv_RowHeight), hv_Col);
                HOperatorSet.WriteString(hv_WindowHandle, hv_Substrings.TupleSelect(hv_I));
            }

            //显示完设置字体默认颜色
            HOperatorSet.SetColor(hv_WindowHandle, "green");

            return;
        }

    }

}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
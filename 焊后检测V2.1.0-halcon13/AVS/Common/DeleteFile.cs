using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS
{
    class DeleteFile
    {
        //删除文件夹里文件再删除文件夹
        public void DeleteOverTimeFiles(string path, int Days)
        {
            try
            {
                IEnumerable<System.IO.DirectoryInfo> directorys = GetAllFileDirectory(path);
                if (Days < directorys.Count())
                {
                    int i = 0;
                    foreach (var d in directorys)
                        if (i++ < directorys.Count() - Days)
                        {
                            IEnumerable<System.IO.FileInfo> files = GetAllFile(Convert.ToString(d.FullName));
                            foreach (var f in files)
                            {
                                File.Delete(f.FullName);
                                //Global.AddLog("文件删除成功：" + f.FullName);
                            }
                            Directory.Delete(d.FullName, true);
                            //Global.AddLog("文件删除成功：" + d.FullName);
                        }
                }

            }
            catch (Exception ex)
            {
                Global.AddLog("过期文件删除出错：" + ex.Message.ToString());
            }
        }

        public void DeleteOverTimeFiles(string path, int Days ,string key,string sideStr)
        {
            try
            {
                IEnumerable<System.IO.DirectoryInfo> directorys = GetAllFileDirectory(path);
                if (Days < directorys.Count())
                {
                    int i = 0;
                    foreach (var d in directorys)
                        if (i++ < directorys.Count() - Days)
                        {
                            IEnumerable<System.IO.DirectoryInfo> directorysModule = GetAllFileDirectory(Convert.ToString(d.FullName));
                            foreach (var f in directorysModule)
                            {
                                if(key == "Originallmage")
                                {
                                    if (Directory.Exists($"{f.FullName}\\OK\\{sideStr}\\{key}")) 
                                        Directory.Delete($"{f.FullName}\\OK\\{sideStr}\\{key}", true);
                                }
                                   
                                else if(key == "ResultImage")
                                {
                                    if (Directory.Exists($"{f.FullName}\\OK\\{sideStr}\\{key}"))
                                        Directory.Delete($"{f.FullName}\\OK\\{sideStr}\\{key}", true);
                                    if (Directory.Exists($"{f.FullName}\\NG\\{sideStr}"))
                                        Directory.Delete($"{f.FullName}\\NG\\{sideStr}", true);
                                }
                                Global.AddLog("文件删除成功：" + f.FullName);
                            }
                            if (key == "ResultImage") Directory.Delete(d.FullName,true);
                          
                            Global.AddLog("文件删除成功：" + d.FullName);
                        }
                }

            }
            catch (Exception ex)
            {
                Global.AddLog("过期文件删除出错：" + ex.Message.ToString());
            }
        }


        //获取路径下的文件夹
        private IEnumerable<System.IO.DirectoryInfo> GetAllFileDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path))
                throw new System.IO.DirectoryNotFoundException();
            DirectoryInfo[] dirNames = null;

            List<System.IO.DirectoryInfo> directorys = new List<System.IO.DirectoryInfo>();

            DirectoryInfo di = new DirectoryInfo(path);
            dirNames = di.GetDirectories("*");

            SortAsFileCreationTime(ref dirNames);//按照创建时间升序排列
            //SortAsFileCreationTime(ref dirNames);
            foreach (DirectoryInfo name in dirNames)
            {
                directorys.Add(name);
            }
            return directorys;
        }

        //获取路径下的所有文件
        private IEnumerable<System.IO.FileInfo> GetAllFile(string path)
        {
            if (!System.IO.Directory.Exists(path))
                throw new System.IO.DirectoryNotFoundException();

            string[] fileNames = null;
            List<System.IO.FileInfo> files = new List<System.IO.FileInfo>();
            fileNames = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (string name in fileNames)
            {
                files.Add(new System.IO.FileInfo(name));
            }
            return files;
        }


        /// <summary>
        /// 按创建时间排序（顺序）
        /// </summary>
        /// <param name="arrFi">待排序数组</param>
        private void SortAsFileCreationTime(ref DirectoryInfo[] arrFi)
        {
            Array.Sort(arrFi, delegate (DirectoryInfo x, DirectoryInfo y) { return x.CreationTime.CompareTo(y.CreationTime); });
        }

        /// <summary>
        /// 按创建时间排序（倒序）
        /// </summary>
        /// <param name="arrFi">待排序数组</param>
        private void SortAsFileCreationTime1(ref DirectoryInfo[] arrFi)
        {
            Array.Sort(arrFi, delegate (DirectoryInfo x, DirectoryInfo y) { return y.CreationTime.CompareTo(x.CreationTime); });
        }
    }

}

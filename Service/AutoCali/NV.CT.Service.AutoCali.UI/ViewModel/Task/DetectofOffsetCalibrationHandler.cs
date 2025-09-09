using NV.CT.Service.Common;
using System;
using System.Text;

namespace NV.CT.Service.AutoCali.UI.Logic
{
    /// <summary>
    /// 探测器位置对齐校准
    /// </summary>
    public class DetectofOffsetCalibrationHandler
    {
        private string rawDataDir;

        public DetectofOffsetCalibrationHandler(string rawDataDir)
        {
            this.rawDataDir = rawDataDir;
        }

        public void Execute()
        {
            //string destDir = rawDataDir.Trim('\\') + "_T03";
            //bool isContinue = RawDataHelper.ConvertFromV2ToV1(rawDataDir, destDir);
            //if (!isContinue)
            //{
            //    Console.WriteLine("Abort execute because failed to Convert to RawData of T03 format!");
            //    return;
            //}

            StringBuilder msg = new StringBuilder();
            msg.AppendLine($"请在系统外使用工具进行校准:");
            msg.AppendLine($"  步骤1.使用DetectorCaliDataConver，转换生数据:{rawDataDir}");
            msg.AppendLine($"  步骤2.使用CalibrateDetectorOffset，计算偏移");
            DialogService.Instance.ShowInfo(msg.ToString());
        }
    }

    public static class DirectoryUtil
    {
        public static bool TryCreateDirectory(string destFolder)
        {
            bool result = false;

            try
            {
                //如果目标路径不存在,则创建目标路径
                if (!System.IO.Directory.Exists(destFolder))
                {
                    System.IO.Directory.CreateDirectory(destFolder);
                }

                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return result;
        }

        /// <summary>
        /// 复制文件夹及文件
        /// </summary>
        /// <param name="sourceDir">原文件路径</param>
        /// <param name="destDir">目标文件路径</param>
        /// <returns></returns>
        public static bool CopyDirectory(string sourceDir, string destDir)
        {
            bool result = false;

            try
            {
                //如果目标路径不存在,则创建目标路径
                if (!System.IO.Directory.Exists(destDir))
                {
                    System.IO.Directory.CreateDirectory(destDir);
                }

                //得到原文件根目录下的所有文件
                string[] files = System.IO.Directory.GetFiles(sourceDir);
                foreach (string file in files)
                {
                    string name = System.IO.Path.GetFileName(file);
                    string dest = System.IO.Path.Combine(destDir, name);
                    System.IO.File.Copy(file, dest);//复制文件
                }

                //得到原文件根目录下的所有文件夹
                string[] folders = System.IO.Directory.GetDirectories(sourceDir);
                foreach (string folder in folders)
                {
                    string name = System.IO.Path.GetFileName(folder);
                    string dest = System.IO.Path.Combine(destDir, name);
                    CopyDirectory(folder, dest);//构建目标路径,递归复制文件
                }

                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return result;
        }
    }

    public static class RawDataHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <returns></returns>
        public static bool ConvertFromV2ToV1(string sourceDir, string destDir)
        {
            bool result = false;

            try
            {
                Console.WriteLine($"converting the RawData from v2 to v1, sourceDir:{sourceDir}, destDir:{destDir}");
                if (!DirectoryUtil.TryCreateDirectory(destDir))
                {
                    return result;
                }

                //Convert the RawData from v2.0 to v1.0
                //Todo:remove. Simulate copy folder
                if (!DirectoryUtil.CopyDirectory(sourceDir, destDir))
                {
                    return result;
                }

                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return result;
        }
    }
}

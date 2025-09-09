using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic
{
    /// <summary>
    /// 在扫描完成后，调用删图工具库，自动删除指定帧数量的图；
    /// 同时支持启用PreOffset功能：将生数据减本底（同步计算的PreOffset的均值文件）并另存；
    /// </summary>
    public class AutoDeleteHandler
    {
        public static readonly string Suffix_AfterDelete = "AfterDelete";
        public static readonly string Suffix_BeforeDelete = "BeforeDelete";

        private ushort autoDeleteFrames;
        private bool isPreOffsetBySoftware;
        private static LogWrapper logWrapper = new(ServiceCategory.AutoCali);

        private ScanParam scanParam;
        private string rawDataDir;// = scanParam.RawDataDirectory;
        private string rawDataDirAfterDelete;
        private string parentDir;
        private string rawDataDirName;
        private ushort autoDeletePreOffsetFrames;

        public AutoDeleteHandler(ScanParam scanParam, ushort autoDeleteFrames, bool isPreOffsetBySoftware, ushort autoDeletePreOffsetFrames = 5)
        {
            this.scanParam = scanParam;
            this.rawDataDir = scanParam.RawDataDirectory?.TrimEnd('\\');
            parentDir = Directory.GetParent(rawDataDir)?.FullName;
            rawDataDirName = Path.GetFileName(rawDataDir);

            this.autoDeleteFrames = autoDeleteFrames;
            this.isPreOffsetBySoftware = isPreOffsetBySoftware;
            this.autoDeletePreOffsetFrames = autoDeletePreOffsetFrames;
        }

        private string GetRawDataPathAppendSuffix(string folderSuffix)
        {
            string rawDataPathAppendSuffix = Path.Combine(parentDir, $"{rawDataDirName}_{folderSuffix}");

            int count = 0;
            while (Directory.Exists(rawDataPathAppendSuffix))
            {
                rawDataPathAppendSuffix = Path.Combine(parentDir, $"{rawDataDirName}_{folderSuffix}_" + (++count));
            }

            return rawDataPathAppendSuffix;
        }

        private string GetRawDataPathAfterDelete_AfterGlow()
        {
            string newRawDataPathAfterDelete = string.Empty;

            if (scanParam.RawDataType != RawDataType.AfterGlow)
            {
                throw new ArgumentException($"scanParam.RawDataType({scanParam.RawDataType}) is not the expected value 'AfterGlow'");
            }

            string gainAfterGlowDir = $"{parentDir}_{Suffix_AfterDelete}";
            //int count = 0;
            //while (Directory.Exists(gainAfterGlowDir))
            //{
            //    gainAfterGlowDir = $"{parentDir}_{Suffix_AfterDelete}_{++count}";
            //}
            newRawDataPathAfterDelete = Path.Combine(gainAfterGlowDir, rawDataDirName);

            if (Directory.Exists(newRawDataPathAfterDelete))
            {
                Directory.Delete(newRawDataPathAfterDelete, true);
            }

            return newRawDataPathAfterDelete;
        }

        private bool AutoDelete()
        {
            bool isSuccess = true;
            logWrapper.Debug($"Delete the raw data frames ...");

            if (!isPreOffsetBySoftware)
            {
                isSuccess = RawDataHelper.Instance.Delete(rawDataDir, rawDataDirAfterDelete, autoDeleteFrames);
                //RawDataDeleteHelper.Instance.Delete(rawDataDir, rawDataDirAfterDelete, autoDeleteFrames);
            }
            else
            {
                throw new NotImplementedException("不支持删图时减PreOffset，期望 IsPreOffsetBySoftware=false");
            }
            return isSuccess;
        }

        public bool Execute()
        {
            bool isSuccess = true;

            try
            {
                if (autoDeleteFrames < 1)
                {
                    logWrapper.Debug($"Break 'AutoDeleteHandle' due to the param 'autoDeleteFrames({autoDeleteFrames})' invald;");
                    return isSuccess;
                }

                //1.创建 自动删图后的文件夹
                rawDataDirAfterDelete = (scanParam.RawDataType == RawDataType.AfterGlow)
                    ? GetRawDataPathAfterDelete_AfterGlow()
                    : GetRawDataPathAppendSuffix(Suffix_AfterDelete);
                if (null == Directory.CreateDirectory(rawDataDirAfterDelete))
                {
                    //创建失败
                    isSuccess = false;
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                //2.自动删图
                //await Task.Run(TryAutoDeleteRawDataFrames);
                isSuccess = AutoDelete();

                stopwatch.Stop();
                logWrapper.Debug($"End of TryAutoDeleteRawDataFrames, used {stopwatch.ElapsedMilliseconds}ms");

                //3.将自动删图后的新目录更新到 scanParam.RawDataDirectory
                logWrapper.Debug($"After delete the raw data frames, set to ScanParam.RawDataDirectory='{rawDataDirAfterDelete}'");
                scanParam.RawDataDirectory = rawDataDirAfterDelete;
                return isSuccess;

                ////3.将自动删图后的生数据 和 原始生数据 目录交换
                //stopwatch.Restart();
                //string swapTempRawDataPath = $"{rawDataDir}_{Suffix_BeforeDelete}";
                //Directory.Move(rawDataDir, swapTempRawDataPath);

                //stopwatch.Stop();
                //logWrapper.Debug($"End of Directory.Move(rawDataDir, swapTempRawDataPath), used {stopwatch.ElapsedMilliseconds}ms");

                //stopwatch.Restart();
                //Directory.Move(rawDataDirAfterDelete, swapTempRawDataPath);

                //stopwatch.Stop();
                //logWrapper.Debug($"End of Directory.Move(rawDataDirAfterDelete, swapTempRawDataPath), used {stopwatch.ElapsedMilliseconds}ms");

                //return true;
            }
            catch (Exception ex)
            {
                logWrapper.Debug($"Failed to Delete the raw data frames, [RawDataDirectory]{rawDataDirAfterDelete}, [Error]{ex}");
            }

            return isSuccess;
        }
    }
}

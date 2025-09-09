using RawData;
using System;
using System.IO;
using System.Linq;

namespace NV.CT.Service.Common.Helper
{
    public class RawDataDeleteHelper
    {
        private static readonly Lazy<RawDataDeleteHelper> lazyInstance = new Lazy<RawDataDeleteHelper>(() => new RawDataDeleteHelper());

        private RawDataDeleteHelper()
        {            
            //// 创建 LoggerFactory 实例
            //var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder..AddConsole(); // 添加 Console 日志提供程序
            //}); 
            //logger= loggerFactory.CreateLogger<Program>();
        }

        // 创建 Logger 实例
        //ILogger logger;

        public static RawDataDeleteHelper Instance => lazyInstance.Value;

        public bool Delete(string SoucePath, string targetPath, ushort delFrameNum)
        {
            return Delete(SoucePath, targetPath, delFrameNum,
                false, 0, string.Empty, string.Empty, 0);
        }

        public bool Delete(string sourceDir, string destDir, ushort delFrameNum,
            bool isPreOffset, short GainType /* 0: Fix24PC, 1:DynamicGain*/,
            string preOffsetDataPath_1, string preOffsetDataPath_2, ushort preOffsetDelFrameNum)
        {
            RawData.PreOffsetDataPath preOffsetDataPath = new PreOffsetDataPath()
            {
                FixedGainPreOffsetDataPath = string.Empty,
                DynamicGainPreOffsetDataPath = string.Empty,
                ForceDynamicPreOffsetDataPath = string.Empty
            };

            //验证路径是否有效
            if (isPreOffset)
            {
                string firstRawDataFile = GetFirstRawDataFile(preOffsetDataPath_1);
                if (string.IsNullOrEmpty(firstRawDataFile))
                {
                    Console.WriteLine($"PreOffset File not exists. PreOffsetDataPath_1:{preOffsetDataPath_1}");
                    return false;
                }
                preOffsetDataPath.FixedGainPreOffsetDataPath = firstRawDataFile;
                preOffsetDataPath.DynamicGainPreOffsetDataPath = firstRawDataFile;

                if (GainType == 1/* DynamicGain */ )
                {
                    firstRawDataFile = GetFirstRawDataFile(preOffsetDataPath_2);
                    if (string.IsNullOrEmpty(firstRawDataFile))
                    {
                        Console.WriteLine($"PreOffset File not exists. PreOffsetDataPath_2:{preOffsetDataPath_2}");
                        return false;
                    }
                    preOffsetDataPath.ForceDynamicPreOffsetDataPath = firstRawDataFile;
                }
            }

            if (!Directory.Exists(destDir))
            {
                Console.WriteLine($"Destination dir not exist, and then create it. [Path]'{destDir}'");
                Directory.CreateDirectory(destDir);
            }

            //已验证参数
            RawData.RawDataDeleter rawDataDeleter = new RawData.RawDataDeleter();
            var resultValue = rawDataDeleter.DataDel(sourceDir, destDir, delFrameNum,
                isPreOffset, GainType, preOffsetDataPath, preOffsetDelFrameNum);
            if(resultValue != 0)
            {
                Console.WriteLine($"Error. Got the result:{resultValue} from Native Invoke RawDataDeleter.DataDel");
            }

            return resultValue == 0;
        }

        private string? GetFirstRawDataFile(string rawDataDir)
        {
            string? rawDataFile = null;
            if (!Directory.Exists(rawDataDir))
            {
                Console.WriteLine($"Folder not exist. Path:{rawDataDir}");
            }
            else
            {
                rawDataFile = Directory.GetFiles(rawDataDir, "*.raw")?.FirstOrDefault();
                if (string.IsNullOrEmpty(rawDataFile))
                {
                    Console.WriteLine($"Folder has not the raw data files. Path:{rawDataDir}");
                }
            }
            return rawDataFile;
        }
    }
}

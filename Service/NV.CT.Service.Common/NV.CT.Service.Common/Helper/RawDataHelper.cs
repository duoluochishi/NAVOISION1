using RawData;
using RawDataTools;
using System;
using System.IO;
using System.Linq;

namespace NV.CT.Service.Common.Helper
{
    public class RawDataHelper
    {
        private static readonly Lazy<RawDataHelper> lazyInstance = new Lazy<RawDataHelper>(() => new RawDataHelper());

        private RawDataHelper()
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

        public static RawDataHelper Instance => lazyInstance.Value;

        public bool Delete(string sourceDir, string destDir, ushort deleteFrameNum)
        {
            bool resultValue = RawDataTool.RawDataDel(sourceDir, destDir, deleteFrameNum);
            return resultValue;
        }

        public bool Delete(string sourceDir, string destDir, ushort delFrameNum,
            bool isPreOffset, short GainType /* 0: Fix24PC, 1:DynamicGain*/,
            string preOffsetDataPath_1, string preOffsetDataPath_2, ushort preOffsetDelFrameNum)
        {
            throw new NotImplementedException();
        }
    }
}

using NV.CT.Service.HardwareTest.Share.Models;
using RawDataHelperWrapper;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public class RawDataReadWriteHelper
    {
        public static RawDataReadWriteHelper Instance { get; private set; } = new();

        private RawDataReadWriteHelper(){ }

        private RawDataReaderWrapper RawDataReader { get; set; } = null!;

        /// <summary>
        /// 根据文件夹路径读取生数据
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="progressNotifier"></param>
        /// <returns></returns>
        public DataResponse<RawDataReadResult> Read(string directory, RawDataIOProgess progressNotifier) 
        {
            RawDataReader = new RawDataReaderWrapper(directory);

            var parseResult = RawDataReader.ParseDataSource();

            if (!parseResult.IsSuccessful) 
            {
                return new(false, parseResult.ErrorCodeStr, default!);
            }

            var getResult = RawDataReader.GetRawDataList(progressNotifier);

            if (!getResult.IsSuccessful) 
            {
                return new(false, getResult.ErrorCodeStr, default!);
            }

            var readResult = new RawDataReadResult
            {
                RawDataInfo = parseResult.Message,
                ScanSeries = getResult.Message
            };

            return new(true, $"Finished data reading by directory - {directory}", readResult);
        }

        /// <summary>
        /// 根据一组生数据路径读取生数据
        /// </summary>
        /// <returns></returns>
        public DataResponse<RawDataReadResult> Read(IEnumerable<string> paths, RawDataIOProgess progressNotifier) 
        {
            RawDataReader = new RawDataReaderWrapper(paths.ToList(), false);

            var parseResult = RawDataReader.ParseDataSource();

            if (!parseResult.IsSuccessful)
            {
                return new(false, parseResult.ErrorCodeStr, default!);
            }

            var getResult = RawDataReader.GetRawDataList(progressNotifier);

            if (!getResult.IsSuccessful)
            {
                return new(false, getResult.ErrorCodeStr, default!);
            }

            var readResult = new RawDataReadResult
            {
                RawDataInfo = parseResult.Message,
                ScanSeries = getResult.Message
            };

            return new(true, $"Finished data reading by paths.", readResult);
        }

        /// <summary>
        /// 切图
        /// </summary>
        /// <param name="progressNotifier"></param>
        /// <returns></returns>
        public DataResponse<ScanSeries> CutImage(RawDataIOProgess progressNotifier, bool useLocalCutXmlConfigFile = false) 
        {
            //切图
            var cutResult = RawDataReader.GetCutRawData(progressNotifier, useLocalCutXmlConfigFile);
            //校验
            if (!cutResult.IsSuccessful)
            {
                return new(false, cutResult.ErrorCodeStr, default!);
            }

            return new(true, $"Finished image cut", cutResult.Message);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release() 
        {
            RawDataReader?.ReleaseRawDataList();
        }
    }

    /// <summary>
    /// 生数据读取结果
    /// </summary>
    public class RawDataReadResult 
    {
        public RawDataInfo? RawDataInfo { get; set; }
        public ScanSeries? ScanSeries { get; set; }
    }

}

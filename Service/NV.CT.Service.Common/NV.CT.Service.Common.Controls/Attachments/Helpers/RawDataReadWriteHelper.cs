using NV.CT.Service.Models;
using RawDataHelperWrapper;

namespace NV.CT.Service.Common.Controls.Attachments.Helpers
{
    public class RawDataReadWriteHelper
    {
        public static RawDataReadWriteHelper Instance { get; private set; } = new();

        private RawDataReadWriteHelper(){ }

        private RawDataReaderWrapper RawDataReader { get; set; } = null!;

        public DataResponse<ScanSeries> Read(string directory, RawDataIOProgess progressNotifier) 
        {
            //初始化reader实例
            RawDataReader = new RawDataReaderWrapper(directory);
            //解析文件夹
            var parseResult = RawDataReader.ParseDataSource();
            //校验
            if (!parseResult.IsSuccessful) 
            {
                return new(false, parseResult.ErrorCodeStr, default!);
            }
            //读取数据
            var readResult = RawDataReader.GetRawDataList(progressNotifier);
            //校验
            if (!readResult.IsSuccessful) 
            {
                return new(false, readResult.ErrorCodeStr, default!);
            }

            return new(true, $"Finished data reading, directory: {directory}", readResult.Message);
        }

        public DataResponse<ScanSeries> RunImageCut(RawDataIOProgess progressNotifier) 
        {
            //切图
            var cutResult = RawDataReader.GetCutRawData(progressNotifier);
            //校验
            if (!cutResult.IsSuccessful)
            {
                return new(false, cutResult.ErrorCodeStr, default!);
            }

            return new(true, $"Finished image cut", cutResult.Message);
        }

        public void Release() 
        {
            RawDataReader?.ReleaseRawDataList();
        }
    }
}

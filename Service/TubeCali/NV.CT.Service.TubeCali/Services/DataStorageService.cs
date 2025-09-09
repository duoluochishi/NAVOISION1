using System;
using System.IO;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.TubeCali.Services.Interface;
using NV.MPS.Environment;

namespace NV.CT.Service.TubeCali.Services
{
    public class DataStorageService(ILogService logService) : IDataStorageService
    {
        private static readonly string DataFolderPath = Path.Combine(RuntimeConfig.Console.ServiceData.Path, ConstInfo.TubeCalibration);
        private static readonly string HistoryFolderPath = Path.Combine(DataFolderPath, ConstInfo.History);

        public void AddHistoryInfo(DateTime dateTime, string message)
        {
            var filePath = Path.Combine(HistoryFolderPath, $"{dateTime:yyyy-MM-dd}.txt");

            try
            {
                if (!Directory.Exists(HistoryFolderPath))
                {
                    Directory.CreateDirectory(HistoryFolderPath);
                }

                File.AppendAllText(filePath, $"{dateTime:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
            }
            catch (Exception e)
            {
                logService.Error(ServiceCategory.SourceComponentCali, $"[TubeCali] Write history file exception: {filePath} ", e);
            }
        }
    }
}
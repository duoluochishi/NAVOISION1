using Microsoft.Extensions.Options;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Repository;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Share.Defaults;
using System;
using System.Collections.Generic;
using System.IO;

namespace NV.CT.Service.HardwareTest.Repositories
{
    public class DetectorTemperatureDataRepository : IRepository<DetectorTemperatureData>
    {
        private readonly ILogService logService;
        private readonly HardwareTestConfigOptions hardwareTestConfigService;

        public DetectorTemperatureDataRepository(
            ILogService logService, 
            IOptions<HardwareTestConfigOptions> hardwareTestOptions)
        {
            this.logService = logService;
            this.hardwareTestConfigService = hardwareTestOptions.Value;
        }

        #region Field & Properties

        /** 读写锁 **/
        private readonly object locker = new object();
        /** 文件名 **/
        private string HistoryDataFileName => $"DetectorTemperatureData_{DateTime.Now.ToString("yyyy_MM_dd")}.txt";
        /** 文件路径 **/
        private string HistoryDataFilePath
            => Path.Combine(
                hardwareTestConfigService.DataDirectoryPath,
                ComponentDefaults.DataAcquisition, HistoryDataFileName);

        #endregion

        public DetectorTemperatureData? Add(DetectorTemperatureData entity)
        {
            if (entity is not null) 
            {
                lock (locker) 
                {
                    using (FileStream fileStream = new FileStream(HistoryDataFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        /** 移动至文件结尾 **/
                        fileStream.Seek(0, SeekOrigin.End);
                        /** 写文件 **/
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.WriteLine(entity.TemperatureInformation);
                        }
                    }
                }
            }

            return entity;
        }

        public DetectorTemperatureData Delete(DetectorTemperatureData entity)
        {
            throw new NotImplementedException();
        }

        public DetectorTemperatureData? Get(DetectorTemperatureData entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DetectorTemperatureData> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DetectorTemperatureData> GetBetweenTimeSpan(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public DetectorTemperatureData Update(DetectorTemperatureData entity)
        {
            throw new NotImplementedException();
        }
    }
}

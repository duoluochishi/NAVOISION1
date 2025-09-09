using Microsoft.Extensions.Options;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Attachments.Repository;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Share.Defaults;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NV.CT.Service.HardwareTest.Repositories
{
    public class XRaySourceHistoryDataRepository : IRepository<XRaySourceHistoryData>
    {

        private readonly ILogService logService;
        private readonly HardwareTestConfigOptions hardwareTestConfigService;
        
        public XRaySourceHistoryDataRepository(
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
        private const string HistoryDataFileName = $"{nameof(XRaySourceHistoryData)}.txt";
        /** 文件路径 **/
        private string HistoryDataFilePath
            => Path.Combine(
                hardwareTestConfigService.DataDirectoryPath,
                ComponentDefaults.XRaySourceComponent, 
                HistoryDataFileName);

        #endregion

        public XRaySourceHistoryData? Add(XRaySourceHistoryData entity)
        {
            if (entity is not null)
            {
                lock (locker) 
                {
                    using (FileStream fileStream = new FileStream(HistoryDataFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        /** 移动至文件结尾 **/
                        fileStream.Seek(0, SeekOrigin.End);
                        /** 序列化 **/
                        string result = JsonSerializer.Serialize(entity, typeof(XRaySourceHistoryData));
                        /** 写文件 **/
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            writer.WriteLine(result);
                        }
                    }
                }
            }

            return entity;
        }

        public XRaySourceHistoryData Delete(XRaySourceHistoryData entity)
        {
            throw new System.NotImplementedException();
        }

        public XRaySourceHistoryData Get(XRaySourceHistoryData entity)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<XRaySourceHistoryData> GetAll()
        {
            var entities = new List<XRaySourceHistoryData>();

            using (FileStream fileStream = new FileStream(HistoryDataFilePath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                /** 移动至文件开头 **/
                fileStream.Seek(0, SeekOrigin.Begin);
                /** 读文件 **/
                using (StreamReader reader = new StreamReader(fileStream)) 
                {
                    while (!reader.EndOfStream) 
                    {
                        /** 读取行 **/
                        string? line = reader.ReadLine();
                        /** 若无内容，直接结束读取 **/
                        if (string.IsNullOrWhiteSpace(line)) break;
                        /** 反序列化 **/
                        try 
                        {
                            XRaySourceHistoryData? entity = JsonSerializer.Deserialize<XRaySourceHistoryData>(line);

                            if (entity is not null) 
                            {
                                entities.Add(entity);
                            }
                        }   
                        catch (Exception ex) 
                        {
                            logService.Warn(
                                ServiceCategory.HardwareTest, 
                                $"[{ComponentDefaults.XRaySourceComponent}] Fail to deserialize line {line} to XRaySourceHistoryData, Stack: {ex}");
                            break;
                        }                        
                    }
                }
            }

            return entities;
        }

        public IEnumerable<XRaySourceHistoryData> GetBetweenTimeSpan(DateTime start, DateTime end)
        {
            var entities = new List<XRaySourceHistoryData>();

            using (FileStream fileStream = new FileStream(HistoryDataFilePath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                /** 移动至文件开头 **/
                fileStream.Seek(0, SeekOrigin.Begin);
                /** 读文件 **/
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while (!reader.EndOfStream)
                    {
                        /** 读取行 **/
                        string? line = reader.ReadLine();
                        /** 若无内容，直接结束读取 **/
                        if (string.IsNullOrWhiteSpace(line)) break;
                        /** 反序列化 **/
                        try
                        {
                            XRaySourceHistoryData? entity = JsonSerializer.Deserialize<XRaySourceHistoryData>(line);

                            if (entity is not null)
                            {
                                if (entity.TimeStamp.Between(start, end))
                                {
                                    entities.Add(entity);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logService.Warn(
                                ServiceCategory.HardwareTest,
                                $"[{ComponentDefaults.XRaySourceComponent}] Fail to deserialize line {line} to XRaySourceHistoryData, Exception Stack: {ex}");
                            break;
                        }
                    }
                }
            }

            return entities;
        }

        public XRaySourceHistoryData Update(XRaySourceHistoryData entity)
        {
            throw new System.NotImplementedException();
        }

    }
}

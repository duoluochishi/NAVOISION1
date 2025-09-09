using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Utilities;

namespace NV.CT.Service.QualityTest.Services.Impls
{
    internal class DataStorageService : IDataStorageService
    {
        private readonly ILogService _logger;

        public DataStorageService(ILogService logger)
        {
            _logger = logger;
        }

        public DateTime? GetReportLastSessionDate()
        {
            if (!File.Exists(Global.ReportLastSessionDatePath))
            {
                return null;
            }

            var content = File.ReadAllText(Global.ReportLastSessionDatePath);
            return DateTime.TryParse(content, out var date) ? date : null;
        }

        public void SaveReportLastSessionDate()
        {
            var now = DateTime.Now;
            var reportHeadInfo = Global.ServiceProvider.GetRequiredService<ReportHeadInfoModel>();
            reportHeadInfo.LastSessionDate = now;
            Save(Global.ReportLastSessionDatePath, now.ToString(CultureInfo.CurrentUICulture));
        }

        public void GenerateHistory(IEnumerable<ItemModel> items)
        {
            try
            {
                Save(Global.History_ItemsPath, SerializeUtility.JsonSerialize(items, new ItemParamConfigModelConverter()));
            }
            catch (Exception ex)
            {
                _logger.Error(ServiceCategory.QualityTest, "GenerateHistory Exception", ex);
            }
        }

        private void Save(string path, string content)
        {
            var folder = Path.GetDirectoryName(path);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder!);
            }

            using var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            using var sw = new StreamWriter(fs);
            sw.Write(content);
        }
    }
}
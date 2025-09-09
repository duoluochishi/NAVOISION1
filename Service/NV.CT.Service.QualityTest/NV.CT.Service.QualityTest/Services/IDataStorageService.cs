using System;
using System.Collections.Generic;
using NV.CT.Service.QualityTest.Models;

namespace NV.CT.Service.QualityTest.Services
{
    public interface IDataStorageService
    {
        DateTime? GetReportLastSessionDate();
        void SaveReportLastSessionDate();
        void GenerateHistory(IEnumerable<ItemModel> items);
    }
}
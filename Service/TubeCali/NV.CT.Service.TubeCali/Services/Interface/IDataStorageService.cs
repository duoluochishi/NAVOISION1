using System;

namespace NV.CT.Service.TubeCali.Services.Interface
{
    public interface IDataStorageService
    {
        public void AddHistoryInfo(DateTime dateTime, string message);
    }
}
using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using NV.CT.Service.TubeWarmUp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.Interfaces
{
    public interface IDataService
    {
        IEnumerable<WarmUpHistory> GetWarmUpHistories();
        WarmUpHistory AddWarmUpHistory(WarmUpHistory history);

        IEnumerable<WarmUpTask> GetWarmUpTasks();
        WarmUpTask AddWarmUpTask(WarmUpTask task);
        bool UpdateWarmUpTask(WarmUpTask task);
        bool DeleteWarmUpTask(WarmUpTask task);

        ScanParamDto GetScanParam();
    }
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;

//using NV.CT.DataRepository.Contract.Entities;

namespace NV.CT.DatabaseService.Contract;
public interface IReconTaskService
{
    bool Insert(ReconTaskModel model);

    bool InsertMany(List<ReconTaskModel> list);

    bool Update(ReconTaskModel model);

    bool UpdateStatus(ReconTaskModel model);

    bool UpdateReconTaskStatus((string ScanId, string ReconId) conditionFields, (OfflineTaskStatus TaskStatus, DateTime StartTime, DateTime EndTime) updateFields);

    bool UpdateTaskStatus(string studyId, string reconId, OfflineTaskStatus taskStatus, DateTime startTime, DateTime endTime);

    bool Delete(ReconTaskModel model);

    bool DeleteByGuid(string reconGuid);

    bool DeleteReconAndSeries(string studyId, string scanId, string reconId);

    ReconTaskModel Get(string ID);

    ReconTaskModel? Get2(string studyId, string scanId, string reconId);

    List<ReconTaskEntity> GetOfflineList();

    List<ReconTaskModel> GetAll(string studyId);

    int GetLatestSeriesNumber(string studyId, int originalSeriesNumber);
}
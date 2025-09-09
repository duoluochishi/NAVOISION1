//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.DatabaseService.Contract;
public interface IScanTaskService
{
    bool Insert(ScanTaskModel model);

    bool InsertMany(List<ScanTaskModel> list);

    bool Update(ScanTaskModel model);

    bool UpdateStatus(ScanTaskModel model);

    bool Delete(ScanTaskModel model);

    ScanTaskModel Get(string ID);

    ScanTaskModel Get2(string studyID, string measurementId, string frameOfReferenceUid, string scanRangeID);

    ScanTaskModel Get3(string studyID, string scanRangeID);

    List<ScanTaskModel> GetAll(string studyId);
}
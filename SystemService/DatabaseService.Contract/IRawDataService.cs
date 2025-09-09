//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.DatabaseService.Contract;

public interface IRawDataService
{
    public event EventHandler<EventArgs<(RawDataModel, DataOperateType)>>? Refresh;

    public bool Add(RawDataModel rawDataModel);

    public bool Update(RawDataModel rawDataModel);

    public bool Delete(string Id);

    public bool UpdateExportStatusById(string id, bool isExported);

    public List<RawDataModel> GetRawDataListByStudyId(string studyId);

}



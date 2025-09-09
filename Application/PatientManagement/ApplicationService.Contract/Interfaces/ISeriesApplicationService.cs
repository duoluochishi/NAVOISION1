//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.PatientManagement.ApplicationService.Contract.Models;

namespace NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;

public interface ISeriesApplicationService
{
    List<SeriesModel> GetSeriesByStudyId(string studyId);
    string GetSeriesReportPathByStudyId(string studyId);
    void RaiseSelectItemChanged(string[] strings);
    bool Delete(SeriesModel seriesModel);
    bool UpdateArchiveStatus(List<SeriesModel> seriesModels);
    void SeriesItemChanged();
    SeriesModel[] GetSeriesBySeriesIds(string[] seriesIds);
    SeriesModel GetSeriesBySeriesInstanceUID(string seriesInstanceUID);
    event EventHandler<EventArgs<(SeriesModel, DataOperateType)>> Refresh;
    event EventHandler? ReportItemChanged;
    event EventHandler<EventArgs<string[]>> SelectItemChanged;
}
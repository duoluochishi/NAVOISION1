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

//using NV.CT.DataRepository.Contract.Entities;

namespace NV.CT.DatabaseService.Contract;

public interface ISeriesService
{
    bool Add(SeriesModel seriesModel);

    bool Delete(SeriesModel seriesModel);
    string GetSeriesIdByStudyId(string studyId);
    string GetSeriesReportPathByStudyId(string studyId);
    bool UpdateArchiveStatus(List<SeriesModel> seriesModels);
    bool UpdatePrintStatus(List<SeriesModel> seriesModels);
    List<SeriesModel> GetSeriesByStudyId(string studyId);
    List<SeriesModel> GetTopoTomoSeriesByStudyId(string studyId);
    List<string> GetSeriesIdsByStudyId(string studyId);
    SeriesModel GetSeriesById(string Id);
    int GetSeriesCountByStudyId(string studyId);
    int GetArchiveStatusHasCompletedSeriesCountByStudyId(string studyId, string seriesId);
    int GetArchiveingSeriesCountByStudyId(string studyId, string seriesId);

    int GetPrintStatusHasCompletedSeriesCountByStudyId(string studyId, string seriesId);
    int GetPrintingSeriesCountByStudyId(string studyId, string seriesId);

    event EventHandler<EventArgs<(SeriesModel, DataOperateType)>> Refresh;
    bool SetSeriesArchiveFail();

    SeriesModel[] GetSeriesBySeriesIds(string[] seriesIds);

    SeriesModel? GetSeriesByReconId(string reconId);
    SeriesModel? GetSeriesBySeriesInstanceUID(string seriesInstanceUID);
    SeriesModel? GetScreenshotSeriesByImageType(string studyID, string imageType);
    bool UpdateScreenshotSeriesByImageType(SeriesModel seriesModel);
}


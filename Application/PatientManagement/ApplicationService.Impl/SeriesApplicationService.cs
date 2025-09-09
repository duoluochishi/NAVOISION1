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
using AutoMapper;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.DatabaseService.Contract;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.ApplicationService.Contract.Models;

namespace NV.CT.PatientManagement.ApplicationService.Impl;

public class SeriesApplicationService : ISeriesApplicationService
{
    private readonly IMapper _mapper;
    private readonly ISeriesService _seriesService;
    private readonly IStudyService _studyService;
    private readonly IReconTaskService _reconTaskService;

    public event EventHandler<EventArgs<string[]>>? SelectItemChanged;
    public event EventHandler<EventArgs<(SeriesModel, DataOperateType)>>? Refresh;
    public event EventHandler? ReportItemChanged;

    public SeriesApplicationService(IMapper mapper, ISeriesService seriesService, IStudyService studyService, IReconTaskService reconTaskService)
    {
        _mapper = mapper;
        _seriesService = seriesService;
        _studyService = studyService;
        _seriesService.Refresh += OnRefresh;
        _reconTaskService = reconTaskService;
    }

    private void OnRefresh(object? sender, EventArgs<(DatabaseService.Contract.Models.SeriesModel, DataOperateType)> e)
    {
        Contract.Models.SeriesModel seriesModel = _mapper.Map<Contract.Models.SeriesModel>(e.Data.Item1);
        Refresh?.Invoke(this, new EventArgs<(Contract.Models.SeriesModel, DataOperateType)>((seriesModel, e.Data.Item2)));
    }

    public bool Delete(SeriesModel seriesModel)
    {
        var result = DeleteSeries(seriesModel.Id);
        return result;
    }

    public List<SeriesModel> GetSeriesByStudyId(string studyId)
    {
        var result = _seriesService.GetSeriesByStudyId(studyId);
        var list = new List<SeriesModel>();
        result.ForEach(a =>
        {
            list.Add(_mapper.Map<SeriesModel>(a));
        });
        return list;
    }


    public SeriesModel[] GetSeriesBySeriesIds(string[] seriesIds)
    {
        var result = _seriesService.GetSeriesBySeriesIds(seriesIds);
        var list = new List<SeriesModel>();
        result.ForEach(a =>
        {
            list.Add(_mapper.Map<SeriesModel>(a));
        });
        return list.ToArray();
    }
    public SeriesModel GetSeriesBySeriesInstanceUID(string seriesInstanceUID)
    {
        var result= _seriesService.GetSeriesBySeriesInstanceUID(seriesInstanceUID);
        return _mapper.Map<SeriesModel>(result);
    }
    public void RaiseSelectItemChanged(string[] strings)
    {
        SelectItemChanged?.Invoke(this, new EventArgs<string[]>(strings));
    }

    public string GetSeriesReportPathByStudyId(string studyId)
    {
        return _seriesService.GetSeriesReportPathByStudyId(studyId);
    }

    public bool UpdateArchiveStatus(List<SeriesModel> seriesModels)
    {
        return _seriesService.UpdateArchiveStatus(_mapper.Map<List<SeriesModel>, List<DatabaseService.Contract.Models.SeriesModel>>(seriesModels));
    }

    public void SeriesItemChanged()
    {
        ReportItemChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool DeleteSeries(string seriesId)
    {
        //todo: 仅实现实际业务逻辑，扩展未实现（审计日志，删除数据的历史记录）

        var seriesInfo = _seriesService.GetSeriesById(seriesId);
        if (seriesInfo is null) return false;

        if (seriesInfo.SeriesType == "DoseReport" || seriesInfo.SeriesType == "Dose SR") return false;
        
        var studyInfo = _studyService.GetStudyById(seriesInfo.InternalStudyId);
        if (studyInfo.IsProtected) return false;

        //todo: 待检测离线重建、图像浏览、胶片打印是否打开此检查

        var reconInfo = _reconTaskService.Get2(seriesInfo.InternalStudyId, seriesInfo.ScanId, seriesInfo.ReconId);

        if (reconInfo is not null)
        {
            if (reconInfo.IsRTD) return false;

            _reconTaskService.DeleteByGuid(reconInfo.Id);
        }

        //删除目录下文件
        DirectoryHelper.DeleteDirectory(seriesInfo.SeriesPath);
        _seriesService.Delete(seriesInfo);

        return true;
    }
}
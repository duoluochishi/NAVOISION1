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
using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.DatabaseService.Impl.Repository;

namespace NV.CT.DatabaseService.Impl;

internal class SeriesService : ISeriesService
{
    private readonly IMapper _mapper;
    private readonly SeriesRepository _seriesRepository;
    public event EventHandler<EventArgs<(SeriesModel, DataOperateType)>>? Refresh;
    private readonly ILogger<SeriesService> _logger;

    public SeriesService(IMapper mapper, SeriesRepository seriesRepository, ILogger<SeriesService> logger)
    {
        _mapper = mapper;
        _seriesRepository = seriesRepository;
        _logger = logger;
    }

    public bool Add(SeriesModel seriesModel)
    {
        _logger.LogInformation("InsertMany series");
        var seriesEntity = _mapper.Map<SeriesEntity>(seriesModel);
        var result = _seriesRepository.Add(seriesEntity);

        if (result)
        {
            Refresh?.Invoke(this, new EventArgs<(SeriesModel, DataOperateType)>((seriesModel, DataOperateType.Add)));
        }
        return result;
    }

    public  bool Delete(SeriesModel seriesModel)
    {
        _logger.LogInformation("Delete series");
        var seriesEntity = _mapper.Map<SeriesEntity>(seriesModel);
        var savedResult = _seriesRepository.Delete(seriesEntity);
        //_seriesRepository.DeleteDicomSeries(seriesEntity);
        if (savedResult)
        {
            Refresh?.Invoke(this, new EventArgs<(SeriesModel, DataOperateType)>((seriesModel, DataOperateType.Delete)));
        }
        return savedResult;
    }

    public List<SeriesModel> GetSeriesByStudyId(string studyId)
    {
        if (string.IsNullOrEmpty(studyId))
        {
            return new List<SeriesModel>();
        }

        var seriesEntities = _seriesRepository.GetSeriesByStudyId(studyId);
        return _mapper.Map<List<SeriesModel>>(seriesEntities);
    }
    public List<SeriesModel> GetTopoTomoSeriesByStudyId(string studyId)
    {
        if (string.IsNullOrEmpty(studyId))
        {
            return new List<SeriesModel>();
        }
        var seriesEntities = _seriesRepository.GetTopoTomoSeriesByStudyId(studyId);
        return _mapper.Map<List<SeriesModel>>(seriesEntities);
    }
    public string GetSeriesIdByStudyId(string studyId)
    {
        return _seriesRepository.GetSeriesIdByStudyId(studyId);
    }
    public string GetSeriesReportPathByStudyId(string studyId)
    {
        return _seriesRepository.GetSeriesReportPathByStudyId(studyId);
    }

    public bool UpdateArchiveStatus(List<SeriesModel> seriesModels)
    {
        _logger.LogInformation("UpdateSeriesArchiveStatus");
        var seriesEntities = _mapper.Map<List<SeriesModel>, List<SeriesEntity>>(seriesModels);
        var savedResult = _seriesRepository.UpdateArchiveStatus(seriesEntities);
        if (savedResult)
        {
            foreach (var item in seriesModels)
            {
                Refresh?.Invoke(this, new EventArgs<(SeriesModel, DataOperateType)>((item, DataOperateType.Update)));
            }
        }
        return savedResult;
    }

    public bool UpdatePrintStatus(List<SeriesModel> seriesModels)
    {
        _logger.LogInformation("UpdateSeriesPrintStatus");
        var seriesEntities = _mapper.Map<List<SeriesModel>, List<SeriesEntity>>(seriesModels);
        var savedResult = _seriesRepository.UpdatePrintStatus(seriesEntities);
        if (savedResult)
        {
            foreach (var item in seriesModels)
            {
                Refresh?.Invoke(this, new EventArgs<(SeriesModel, DataOperateType)>((item, DataOperateType.Update)));
            }
        }
        return savedResult;
    }


    public List<string> GetSeriesIdsByStudyId(string studyId)
    {
        return _seriesRepository.GetSeriesIdsByStudyId(studyId);
    }

    public SeriesModel GetSeriesById(string Id)
    {
        var seriesEntity = _seriesRepository.GetSeriesById(Id);
        return _mapper.Map<SeriesModel>(seriesEntity);
    }

    public int GetArchiveStatusHasCompletedSeriesCountByStudyId(string studyId, string seriesId)
    {
        return _seriesRepository.GetArchiveStatusHasCompletedSeriesCountByStudyId(studyId, seriesId);
    }

    public int GetPrintStatusHasCompletedSeriesCountByStudyId(string studyId, string seriesId)
    {
        return _seriesRepository.GetPrintStatusHasCompletedSeriesCountByStudyId(studyId, seriesId);
    }

    public int GetSeriesCountByStudyId(string studyId)
    {
        return _seriesRepository.GetSeriesCountByStudyId(studyId);
    }
    public int GetArchiveingSeriesCountByStudyId(string studyId, string seriesId)
    {
        return _seriesRepository.GetArchiveingSeriesCountByStudyId(studyId, seriesId);
    }

    public int GetPrintingSeriesCountByStudyId(string studyId, string seriesId)
    {
        return _seriesRepository.GetPrintingSeriesCountByStudyId(studyId, seriesId);
    }

    public bool SetSeriesArchiveFail()
    {
        return _seriesRepository.SetSeriesArchiveFail();
    }

    public SeriesModel[] GetSeriesBySeriesIds(string[] seriesIds)
    {
        var seriesEntities = _seriesRepository.GetSeriesByIds(seriesIds);
        return _mapper.Map<SeriesModel[]>(seriesEntities);
    }

    public SeriesModel? GetSeriesByReconId(string reconId)
    {
        var seriesEntities = _seriesRepository.GetSeriesByReconId(reconId);
        if (seriesEntities is not null)
        {
            return _mapper.Map<SeriesModel>(seriesEntities);
        }
        return null;
    }
    public SeriesModel? GetSeriesBySeriesInstanceUID(string seriesInstanceUID)
    {
        var seriesEntities = _seriesRepository.GetSeriesBySeriesInstanceUID(seriesInstanceUID);
        if (seriesEntities is not null)
        {
            return _mapper.Map<SeriesModel>(seriesEntities);
        }
        return null;
    }
    public SeriesModel? GetScreenshotSeriesByImageType(string studyID, string imageType)
    {
        var seriesEntities = _seriesRepository.GetScreenshotSeriesByImageType(studyID,imageType);
        if (seriesEntities is not null)
        {
            return _mapper.Map<SeriesModel>(seriesEntities);
        }
        return null;
    }
    public bool UpdateScreenshotSeriesByImageType(SeriesModel seriesModel)
    {
        var seriesEntity = _mapper.Map<SeriesEntity>(seriesModel);
        if (seriesEntity is not null) 
        {
            var savedResult = _seriesRepository.UpdateScreenshotSeriesByImageType(seriesEntity);           
            if (savedResult)
            {
                Refresh?.Invoke(this, new EventArgs<(SeriesModel, DataOperateType)>((seriesModel, DataOperateType.Update)));
            }
            return savedResult;
        }
        return false;
    }
}
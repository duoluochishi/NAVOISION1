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
using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.DatabaseService.Impl.Repository;

namespace NV.CT.DatabaseService.Impl;

internal class RawDataService : IRawDataService
{
    private readonly IMapper _mapper;
    private readonly RawDataRepository _rawDataRepository;
    public event EventHandler<EventArgs<(RawDataModel, DataOperateType)>>? Refresh;
    private readonly ILogger<RawDataService> _logger;

    public RawDataService(IMapper mapper, ILogger<RawDataService> logger, RawDataRepository rawDataRepository )
    {
        _mapper = mapper;
        _logger = logger;
        _rawDataRepository = rawDataRepository;
    }

    public bool Add(RawDataModel rawDataModel)
    {
        _logger.LogInformation("Add RawDataModel");
        var rawDataEntity = _mapper.Map<RawDataEntity>(rawDataModel);
        var result = _rawDataRepository.Add(rawDataEntity);
        if (result)
        {
            Refresh?.Invoke(this, new EventArgs<(RawDataModel, DataOperateType)>((rawDataModel, DataOperateType.Add)));
        }
        return result;
    }

    public bool Update(RawDataModel rawDataModel)
    {
        _logger.LogInformation("Update RawDataModel");
        var rawDataEntity = _mapper.Map<RawDataEntity>(rawDataModel);
        var result = _rawDataRepository.Update(rawDataEntity);
        if (result)
        {
            Refresh?.Invoke(this, new EventArgs<(RawDataModel, DataOperateType)>((rawDataModel, DataOperateType.Update)));
        }
        return result;
    }

    public bool Delete(string id)
    {
        _logger.LogInformation("Delete RawDataModel");
        var result = _rawDataRepository.Delete(id);
        if (result)
        {
            Refresh?.Invoke(this, new EventArgs<(RawDataModel, DataOperateType)>((new RawDataModel() { Id = id }, DataOperateType.Delete)));
        }
        return result;
    }

    public List<RawDataModel> GetRawDataListByStudyId(string studyId)
    {
        _logger.LogInformation($"GetRawDataListByStudyId for studyId:{studyId}");
        var rawDataEntityList = _rawDataRepository.GetRawDataListByStudyId(studyId);
        return _mapper.Map<List<RawDataModel>>(rawDataEntityList);
    }

    public bool UpdateExportStatusById(string id, bool isExported)
    {
        _logger.LogInformation($"UpdateExportStatusById for Id:{id}");
        var result = _rawDataRepository.UpdateExportStatusById(id, isExported);
        if (result)
        {
            Refresh?.Invoke(this, new EventArgs<(RawDataModel, DataOperateType)>((new RawDataModel() { Id = id, IsExported = isExported }, DataOperateType.UpdateExportStatus)));
        }
        return result;
    }

}
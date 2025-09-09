//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
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

public class ScanTaskService : IScanTaskService
{
    private readonly IMapper _mapper;
    private readonly ScanTaskRepository _scanTaskRepository;
    public event EventHandler<EventArgs<(ScanTaskModel, DataOperateType)>>? Refresh;
    private readonly ILogger<ScanTaskService> _logger;

    public ScanTaskService(IMapper mapper, ScanTaskRepository scanTaskRepository, ILogger<ScanTaskService> logger)
    {
        _mapper = mapper;
        _scanTaskRepository = scanTaskRepository;
        _logger = logger;
    }

    public bool Insert(ScanTaskModel model)
    {
        //TODO:操作多个表的时候，尽量使用事务封装
        _logger.LogInformation("InsertMany scan task");
        var scanTaskEntity = _mapper.Map<ScanTaskEntity>(model);

        var savedResult = _scanTaskRepository.Insert(scanTaskEntity);
        if (savedResult)
        {
            Refresh?.Invoke(this, new EventArgs<(ScanTaskModel, DataOperateType)>((model, DataOperateType.Add)));
        }
        return savedResult;
    }

    public bool InsertMany(List<ScanTaskModel> list)
    {
        //TODO:操作多个表的时候，尽量使用事务封装
        _logger.LogInformation("InsertMany scan task");
        List<ScanTaskEntity> scanTaskEntities = new List<ScanTaskEntity>();

        foreach (var model in list)
        {
            scanTaskEntities.Add(_mapper.Map<ScanTaskEntity>(model));
        }
        var savedResult = _scanTaskRepository.Insert(scanTaskEntities);
        if (savedResult)
        {
            Refresh?.Invoke(this, new EventArgs<(ScanTaskModel, DataOperateType)>((list[0], DataOperateType.Add)));
        }
        return savedResult;
    }

    public ScanTaskModel Get(string studyId)
    {
        var entity = _scanTaskRepository.Get(studyId);
        return _mapper.Map<ScanTaskModel>(entity);
    }

    public ScanTaskModel Get3(string studyID, string scanRangeID)
    {
        var entity = _scanTaskRepository.Get(studyID, scanRangeID);
        return _mapper.Map<ScanTaskModel>(entity);
    }

    public ScanTaskModel Get2(string studyID, string measurementId, string frameOfReferenceUid, string scanRangeID)
    {
        var entity = _scanTaskRepository.Get(studyID, measurementId, frameOfReferenceUid, scanRangeID);
        return _mapper.Map<ScanTaskModel>(entity);
    }

    public bool Update(ScanTaskModel model)
    {
        _logger.LogInformation("Update scan task");
        var scanTaskEntity = _mapper.Map<ScanTaskEntity>(model);
        var savedResult = _scanTaskRepository.Update(scanTaskEntity);
        if (savedResult)
        {
            Refresh?.Invoke(this, new EventArgs<(ScanTaskModel, DataOperateType)>((model, DataOperateType.Update)));
        }
        return savedResult;
    }

    public bool UpdateStatus(ScanTaskModel model)
    {
        _logger.LogInformation("Update scan task status");
        var scanTaskEntity = _mapper.Map<ScanTaskEntity>(model);
        var savedResult = _scanTaskRepository.UpdateStatus(scanTaskEntity);
        if (savedResult)
        {
            Refresh?.Invoke(this, new EventArgs<(ScanTaskModel, DataOperateType)>((model, DataOperateType.Update)));
        }
        return savedResult;
    }

    public bool Delete(ScanTaskModel model)
    {
        _logger.LogInformation("Delete scan task");
        var scanTaskEntity = _mapper.Map<ScanTaskEntity>(model);
        var savedResult = _scanTaskRepository.Delete(scanTaskEntity);
        if (savedResult)
        {
            Refresh?.Invoke(this, new EventArgs<(ScanTaskModel, DataOperateType)>((model, DataOperateType.Delete)));
        }
        return savedResult;
    }

    public List<ScanTaskModel> GetAll(string studyId)
    {
        var scanTasks = _scanTaskRepository.GetAll(studyId);
        return _mapper.Map<List<ScanTaskModel>>(scanTasks);
    }
}
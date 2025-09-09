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

public class ReconTaskService : IReconTaskService
{
	private readonly IMapper _mapper;
	private readonly ReconTaskRepository _reconTaskRepository;
	private readonly SeriesRepository _seriesRepository;
	public event EventHandler<EventArgs<(ReconTaskModel, DataOperateType)>>? Refresh;
	private readonly ILogger<ReconTaskService> _logger;

	public ReconTaskService(IMapper mapper, ReconTaskRepository reconTaskRepository, SeriesRepository seriesRepository, ILogger<ReconTaskService> logger)
	{
		_mapper = mapper;
		_reconTaskRepository = reconTaskRepository;
		_seriesRepository = seriesRepository;
		_logger = logger;
	}

	public bool Insert(ReconTaskModel model)
	{
		//TODO:操作多个表的时候，尽量使用事务封装
		_logger.LogInformation("InsertMany recon task");
		var reconTaskEntity = _mapper.Map<ReconTaskEntity>(model);

		var savedResult = _reconTaskRepository.Insert(reconTaskEntity);
		if (savedResult)
		{
			Refresh?.Invoke(this, new EventArgs<(ReconTaskModel, DataOperateType)>((model, DataOperateType.Add)));
		}
		return savedResult;
	}

	public bool InsertMany(List<ReconTaskModel> list)
	{
		//TODO:操作多个表的时候，尽量使用事务封装
		_logger.LogInformation("InsertMany recon task");
		List<ReconTaskEntity> reconTaskEntities = new List<ReconTaskEntity>();

		foreach (var model in list)
		{
			reconTaskEntities.Add(_mapper.Map<ReconTaskEntity>(model));
		}
		var savedResult = _reconTaskRepository.Insert(reconTaskEntities);
		if (savedResult)
		{
			Refresh?.Invoke(this, new EventArgs<(ReconTaskModel, DataOperateType)>((list[0], DataOperateType.Add)));
		}
		return savedResult;
	}

	public ReconTaskModel Get(string guid)
	{
		var entity = _reconTaskRepository.Get(guid);
		return _mapper.Map<ReconTaskModel>(entity);
	}

	public ReconTaskModel? Get2(string studyId, string scanId, string reconId)
	{
		var entity = _reconTaskRepository.Get(studyId, scanId, reconId);
		if (entity is not null)
			return _mapper.Map<ReconTaskModel>(entity);
		return null;
	}

	public List<ReconTaskEntity> GetOfflineList()
	{
		return _reconTaskRepository.GetOfflineList();
	}

	public bool Update(ReconTaskModel model)
	{
		_logger.LogInformation("Update recon task");
		var reconTaskEntity = _mapper.Map<ReconTaskEntity>(model);
		var savedResult = _reconTaskRepository.Update(reconTaskEntity);
		if (savedResult)
		{
			Refresh?.Invoke(this, new EventArgs<(ReconTaskModel, DataOperateType)>((model, DataOperateType.Update)));
		}
		return savedResult;
	}

	public bool UpdateStatus(ReconTaskModel model)
	{
		_logger.LogInformation("update recon task status");
		var reconTaskEntity = _mapper.Map<ReconTaskEntity>(model);
		var savedResult = _reconTaskRepository.UpdateStatus(reconTaskEntity);
		if (savedResult)
		{
			Refresh?.Invoke(this, new EventArgs<(ReconTaskModel, DataOperateType)>((model, DataOperateType.Update)));
		}
		return savedResult;
	}

	public bool UpdateTaskStatus(string studyId, string reconId, OfflineTaskStatus taskStatus, DateTime startTime, DateTime endTime)
	{
		var result = _reconTaskRepository.UpdateTaskStatus(studyId, reconId, taskStatus, startTime, endTime);
		return result;
	}

	public bool UpdateReconTaskStatus((string ScanId, string ReconId) conditionFields, (OfflineTaskStatus TaskStatus, DateTime StartTime, DateTime EndTime) updateFields)
	{
		_logger.LogInformation($"Update offline recon task status: ({conditionFields.ScanId}, {conditionFields.ReconId}, {updateFields.TaskStatus})");
		var savedResult = _reconTaskRepository.UpdateReconTaskStatus(conditionFields, updateFields);
		//TODO: 事件回发，需要支持（跨进程）
		return savedResult;
	}

	public bool Delete(ReconTaskModel model)
	{
		_logger.LogInformation("Delete recon task");
		var reconTaskEntity = _mapper.Map<ReconTaskEntity>(model);
		var savedResult = _reconTaskRepository.Delete(reconTaskEntity);
		if (savedResult)
		{
			Refresh?.Invoke(this, new EventArgs<(ReconTaskModel, DataOperateType)>((model, DataOperateType.Delete)));
		}
		return savedResult;
	}

	public bool DeleteByGuid(string reconGuid)
	{
		return _reconTaskRepository.DeleteByGuid(reconGuid);
	}


    public bool DeleteReconAndSeries(string studyId, string scanId, string reconId)
	{
		_reconTaskRepository.Delete(studyId, scanId, reconId);
		_seriesRepository.Delete(studyId, scanId, reconId);
		return true;
	}

	public List<ReconTaskModel> GetAll(string studyId)
	{
		var reconTasks = _reconTaskRepository.GetAll(studyId);
		return _mapper.Map<List<ReconTaskModel>>(reconTasks);
	}

    public int GetLatestSeriesNumber(string studyId, int originalSeriesNumber)
	{
		var latestSeriesNumber = _reconTaskRepository.GetLatestSeriesNumber(studyId, originalSeriesNumber);
		return latestSeriesNumber;
	}
}
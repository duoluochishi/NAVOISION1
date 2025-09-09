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
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.DatabaseService.Impl.Repository;

namespace NV.CT.DatabaseService.Impl;

public class JobTaskService : IJobTaskService
{
    private readonly IMapper _mapper;
    private readonly JobTaskRepository _jobTaskRepository;
    private readonly ILogger<JobTaskService> _logger;

    public JobTaskService(IMapper mapper, JobTaskRepository jobTaskRepository, ILogger<JobTaskService> logger)
    {
        _mapper = mapper;
        _jobTaskRepository = jobTaskRepository;
        _logger = logger;

    }

    public bool Insert(JobTaskModel jobTaskModel)
    {
        var jobTaskEntity = _mapper.Map<JobTaskEntity>(jobTaskModel);
        return _jobTaskRepository.Insert(jobTaskEntity);
    }

    public bool Update(JobTaskModel jobTaskModel)
    {
        var jobTaskEntity = _mapper.Map<JobTaskEntity>(jobTaskModel);
        return _jobTaskRepository.Update(jobTaskEntity);
    }

    public JobTaskModel? GetJobById(string jobId, string jobTaskType)
    {
        var entity = _jobTaskRepository.GetJobById(jobId, jobTaskType);
        if (entity is null)
        {
            return null;
        }

        return _mapper.Map<JobTaskModel>(entity);
    }


    public bool UpdateTaskStatusByWorkflowId(string workflowId, string taskStatus)
    {
        _logger.LogTrace($"Update TaskStatus by workflowId:{workflowId}");
        return _jobTaskRepository.UpdateTaskStatusByWorkflowId(workflowId, taskStatus);
    }

    public bool UpdateTaskStatusByJobId(string jobId, string jobStatus)
    {
        _logger.LogTrace($"Update TaskStatus by jobId:{jobId}");
        return _jobTaskRepository.UpdateTaskStatusByJobId(jobId, jobStatus);
    }

    public bool UpdatePriority(string jobId, string jobType, int priority)
    {
        return _jobTaskRepository.UpdatePriority(jobId, jobType, priority);
    }

    public JobTaskModel? FetchNextAvailableJob(string jobTaskType)
    {
        var entity = _jobTaskRepository.FetchNextAvailableJob(jobTaskType);
        if (entity is null)
        {
            return null;
        }

        return _mapper.Map<JobTaskModel>(entity); 

    }

    public JobTaskModel? FetchAvailableJobById(string jobId, string jobTaskType)
    {
        var entity = _jobTaskRepository.FetchAvailableJobById(jobId, jobTaskType);
        if (entity is null)
        {
            return null;
        }

        return _mapper.Map<JobTaskModel>(entity);

    }

    public bool Delete(string jobId, string jobTaskType)
    {
        return _jobTaskRepository.Delete(jobId, jobTaskType);
    }
    
    public List<JobTaskModel> GetJobTasksByTypeAndStatus(string jobType, List<string> jobTaskStatusList, string beginDatetime, string endDatetime, string? patientName, string? bodyPart)
    {
        List<JobTaskEntity> result;
        if (jobType == JobTaskType.PrintJob.ToString())
        {
            result = _jobTaskRepository.GetJobTasksByTypeAndStatusWithConditions(jobType, jobTaskStatusList, beginDatetime, endDatetime, patientName, bodyPart);            
        }
        else
        {
            result = _jobTaskRepository.GetJobTasksByTypeAndStatus(jobType, jobTaskStatusList, beginDatetime, endDatetime);
        }

        var list = new List<JobTaskModel>();
        list = _mapper.Map<List<JobTaskEntity>, List<JobTaskModel>>(result);
        return list;
    }

    public JobTaskModel GetJobTaskByWorkflowId(string workflowId)
    {
        _logger.LogTrace("GetJobTaskByWorkflowId");
        var result = _jobTaskRepository.GetJobTaskByWorkflowId(workflowId);
        return _mapper.Map<JobTaskModel>(result);
    }

    public int GetCountOfJobs(string jobType, string jobTaskStatus)
    {
        return _jobTaskRepository.GetCountOfJobs(jobType, jobTaskStatus);
    }

}
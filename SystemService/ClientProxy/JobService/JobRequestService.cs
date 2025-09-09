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
using NV.MPS.Communication;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;

namespace NV.CT.ClientProxy.Job;

public class JobRequestService : IJobRequestService
{
    private readonly JobClientProxy _clientProxy;

    public JobRequestService(JobClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public bool CancelJob(string jobId, JobTaskType jobType)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.CancelJob),
            Data = Tuple.Create(jobId, jobType).ToJson(),
        });
        if (commandResponse.Success)
        {
            return Convert.ToBoolean(commandResponse.Data);
        }

        return false;
    }

    public bool DeleteJob(string jobId, JobTaskType jobType)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.DeleteJob),
            Data = Tuple.Create(jobId, jobType).ToJson(),
        });
        if (commandResponse.Success)
        {
            return Convert.ToBoolean(commandResponse.Data);
        }

        return false;
    }

    public JobTaskCommandResult EnqueueJobRequest(BaseJobRequest jobRequest)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.EnqueueJobRequest),
            Data = jobRequest.ToJson(),
        });
        if (commandResponse.Success)
        {
            return commandResponse.Data.DeserializeTo<JobTaskCommandResult>();
        }

        return default;
    }

    public JobTaskInfo? GetJobById(string jobId, JobTaskType jobType)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.GetJobById),
            Data = Tuple.Create(jobId, jobType).ToJson(),
        });
        if (commandResponse.Success)
        {
            return commandResponse.Data.DeserializeTo<JobTaskInfo>();
        }

        return default;
    }

    public JobTaskInfo? FetchAvailableJobById(string jobId, JobTaskType jobType)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.FetchAvailableJobById),
            Data = Tuple.Create(jobId, jobType).ToJson()
        }); 
        if (commandResponse.Success)
        {
            return commandResponse.Data.DeserializeTo<JobTaskInfo>();
        }

        return default;
    }

    public JobTaskInfo? FetchNextAvailableJob(JobTaskType jobType)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.FetchNextAvailableJob),
            Data = jobType.ToJson(),
        });
        if (commandResponse.Success)
        {
            return commandResponse.Data.DeserializeTo<JobTaskInfo>();
        }

        return default;
    }

    public List<JobTaskInfo> GetJobsByTypeAndStatus(QueryJobRequest queryJobRequest)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.GetJobsByTypeAndStatus),
            Data = queryJobRequest.ToJson(),
        });
        if (commandResponse.Success)
        {
            return commandResponse.Data.DeserializeTo<List<JobTaskInfo>>();
        }

        return default;
    }

    public int GetCountOfJobs(JobTaskType jobType, JobTaskStatus jobTaskStatus)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.GetCountOfJobs),
            Data = Tuple.Create(jobType, jobTaskStatus).ToJson(),
        });
        if (commandResponse.Success)
        {
            return Convert.ToInt32(commandResponse.Data);
        }

        return 0;
    }

    public bool MoveToTopPriority(string jobId, JobTaskType jobType)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.MoveToTopPriority),
            Data = Tuple.Create(jobId, jobType).ToJson(),
        });
        if (commandResponse.Success)
        {
            return Convert.ToBoolean(commandResponse.Data);
        }

        return false;
    }

    public bool PauseJob(string jobId, JobTaskType jobType)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.PauseJob),
            Data = Tuple.Create(jobId, jobType).ToJson(),
        });
        if (commandResponse.Success)
        {
            return Convert.ToBoolean(commandResponse.Data);
        }

        return false;
    }

    public bool RunJob(string jobId, JobTaskType jobType)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IJobRequestService).Namespace,
            SourceType = nameof(IJobRequestService),
            ActionName = nameof(IJobRequestService.RunJob),
            Data = Tuple.Create(jobId, jobType).ToJson(),
        });
        if (commandResponse.Success)
        {
            return Convert.ToBoolean(commandResponse.Data);
        }

        return false;
    }
}
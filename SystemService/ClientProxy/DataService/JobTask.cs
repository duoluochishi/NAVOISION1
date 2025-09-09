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
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.ClientProxy.DataService
{
    public class JobTask : IJobTaskService
    {
        private readonly MCSServiceClientProxy _dataDataServiceClientProxy;
        public JobTask(MCSServiceClientProxy dataDataServiceClientProxy)
        {
            _dataDataServiceClientProxy = dataDataServiceClientProxy;
        }

        public List<JobTaskModel> GetJobTasksByTypeAndStatus(string jobType, List<string> jobTaskStatusList, string beginDatetime, string endDatetime, string? patientName, string? bodyPart)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.GetJobTasksByTypeAndStatus),
                Data = Tuple.Create(jobType, jobTaskStatusList, beginDatetime, endDatetime, patientName, bodyPart).ToJson()
            });
            if (commandResponse.Success)
            {
                return commandResponse.Data.DeserializeTo<List<JobTaskModel>>();
            }

            return default;
        }


        public bool Delete(string jobId, string jobType)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.Delete),
                Data = Tuple.Create(jobId, jobType).ToJson()
            });
            if (commandResponse.Success)
            {
                return Convert.ToBoolean(commandResponse.Data);
            }

            return false;
        }

        public bool Insert(JobTaskModel jobTaskModel)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.Insert),
                Data = jobTaskModel.ToJson()
            });
            if (commandResponse.Success)
            {
                return Convert.ToBoolean(commandResponse.Data);
            }

            return false;
        }

        public bool Update(JobTaskModel jobTaskModel)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.Update),
                Data = jobTaskModel.ToJson()
            });
            if (commandResponse.Success)
            {
                return Convert.ToBoolean(commandResponse.Data);
            }

            return false;
        }

        public bool UpdateTaskStatusByWorkflowId(string workflowId, string taskStatus)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.UpdateTaskStatusByWorkflowId),
                Data = Tuple.Create(workflowId, taskStatus).ToJson()
            });
            if (commandResponse.Success)
            {
                var result = Convert.ToBoolean(commandResponse.Data);
                return result;
            }

            return false;
        }

        public JobTaskModel GetJobTaskByWorkflowId(string workflowId)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.GetJobTaskByWorkflowId),
                Data = workflowId,
            });
            if (commandResponse.Success)
            {
                return commandResponse.Data.DeserializeTo<JobTaskModel>();
            }

            return default;
        }

        public JobTaskModel? FetchNextAvailableJob(string jobTaskType)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.FetchNextAvailableJob),
                Data = jobTaskType,
            });
            if (commandResponse.Success)
            {
                return commandResponse.Data.DeserializeTo<JobTaskModel>();
            }

            return default;
        }

        public JobTaskModel? FetchAvailableJobById(string jobId, string jobTaskType)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.FetchAvailableJobById),
                Data = Tuple.Create(jobId, jobTaskType).ToJson(),
            });
            if (commandResponse.Success)
            {
                return commandResponse.Data.DeserializeTo<JobTaskModel>();
            }

            return default;
        }

        public bool UpdatePriority(string jobId, string jobType, int priority)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.UpdatePriority),
                Data = Tuple.Create(jobId, jobType, priority).ToJson(),
            });
            if (commandResponse.Success)
            {
                return Convert.ToBoolean(commandResponse.Data);
            }

            return false;
        }

        public int GetCountOfJobs(string jobType, string jobTaskStatus)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.GetCountOfJobs),
                Data = Tuple.Create(jobType, jobTaskStatus).ToJson(),
            });
            if (commandResponse.Success)
            {
                return Convert.ToInt32(commandResponse.Data);
            }

            return 0;
        }

        public bool UpdateTaskStatusByJobId(string jobId, string jobStatus)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.UpdateTaskStatusByJobId),
                Data = Tuple.Create(jobId, jobStatus).ToJson()
            });
            if (commandResponse.Success)
            {
                var result = Convert.ToBoolean(commandResponse.Data);
                return result;
            }

            return false;
        }

        public JobTaskModel? GetJobById(string jobId, string jobTaskType)
        {
            var commandResponse = _dataDataServiceClientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IJobTaskService).Namespace,
                SourceType = nameof(IJobTaskService),
                ActionName = nameof(IJobTaskService.GetJobById),
                Data = Tuple.Create(jobId, jobTaskType).ToJson(),
            });
            if (commandResponse.Success)
            {
                return commandResponse.Data.DeserializeTo<JobTaskModel>();
            }

            return default;
        }
    }
}

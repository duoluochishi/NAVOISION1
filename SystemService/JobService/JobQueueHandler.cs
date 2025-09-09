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
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Interfaces;
using NV.CT.Language;
using NV.CT.MessageService.Contract;

namespace NV.CT.JobService
{
    public class JobQueueHandler : IJobQueueHandler
    {
        private readonly IMapper _mapper;
        private readonly IMessageService _messageService;
        private readonly IJobTaskService _jobTaskService;

        public JobQueueHandler(IMapper mapper,
                               IMessageService messageService,
                               IJobTaskService jobTaskService)
        {
            _mapper = mapper;
            _messageService = messageService;
            _jobTaskService = jobTaskService;
        }

        public bool EnqueueJobRequest(JobTaskInfo jobTaskinfo)
        {
            var jobTaskModel = _mapper.Map<JobTaskModel>(jobTaskinfo);
            return this._jobTaskService.Insert(jobTaskModel);
        }

        public JobTaskInfo? GetJobById(string jobId, JobTaskType jobType)
        {
            var jobTaskModel = this._jobTaskService.GetJobById(jobId, jobType.ToString());
            if (jobTaskModel is null)
            {
                return null;
            }

            return _mapper.Map<JobTaskInfo>(jobTaskModel);
        }


        public JobTaskInfo? FetchNextAvailableJob(JobTaskType jobType)
        {
            var jobTaskModel = this._jobTaskService.FetchNextAvailableJob(jobType.ToString());
            if (jobTaskModel is null)
            {
                return null;
            }

            return _mapper.Map<JobTaskInfo>(jobTaskModel);
        }

        public JobTaskInfo? FetchAvailableJobById(string jobId, JobTaskType jobType)
        {
            var jobTaskModel = this._jobTaskService.FetchAvailableJobById(jobId, jobType.ToString());
            if (jobTaskModel is null)
            {
                return null;
            }

            return _mapper.Map<JobTaskInfo>(jobTaskModel);
        }

        public bool SetPrioirty(string jobId, JobTaskType jobType, PriorityType priorityType)
        {
            if (string.IsNullOrEmpty(jobId)) return false;

            return this._jobTaskService.UpdatePriority(jobId, jobType.ToString(), (int)priorityType);
        }

        public void UpdateTaskStatusByJobId(string jobId, JobTaskStatus taskStatus)
        {
            if (!string.IsNullOrEmpty(jobId))
            {
                this._jobTaskService.UpdateTaskStatusByJobId(jobId, taskStatus.ToString());
            }
        }

        public List<JobTaskInfo> GetJobsByTypeAndStatus(QueryJobRequest queryJobRequest)
        {
            var jobTaskModelList = this._jobTaskService.GetJobTasksByTypeAndStatus(queryJobRequest.JobType.ToString(),
                                                                                   queryJobRequest.JobTaskStatusList.Select(s => s.ToString()).ToList(),
                                                                                   queryJobRequest.BeginDateTime.ToString(),
                                                                                   queryJobRequest.EndDateTime.ToString(),
                                                                                   queryJobRequest.PatientName,
                                                                                   queryJobRequest.BodyPart);
            return _mapper.Map<List<JobTaskInfo>>(jobTaskModelList);
        }

        public bool DeleteJob(string jobId, JobTaskType jobType)
        {
            if (string.IsNullOrEmpty(jobId)) return false;

            return this._jobTaskService.Delete(jobId, jobType.ToString());
        }

        public bool PauseJob(string jobId, JobTaskType jobType)
        {
            if (string.IsNullOrEmpty(jobId)) return false;

            this._messageService.SendMessage(new MessageInfo()
            {
                Sender = this.MapJobTypeToMessageType(jobType),
                Level = MessageLevel.Info,
                Content = JobOperationType.Process.ToString(),
                SendTime = DateTime.Now
            });
            return true;
        }

        public bool RunJob(string jobId, JobTaskType jobType)
        {
            if (string.IsNullOrEmpty(jobId)) return false;

            this._messageService.SendMessage(new MessageInfo()
            {
                Sender = this.MapJobTypeToMessageType(jobType),
                Level = MessageLevel.Info,
                Content = JobOperationType.Process.ToString(),
                SendTime = DateTime.Now
            });
            return true;
        }

        public int GetCountOfJobs(JobTaskType jobType, JobTaskStatus jobTaskStatus)
        {
            return this._jobTaskService.GetCountOfJobs(jobType.ToString(), jobTaskStatus.ToString());
        }

        private MessageSource MapJobTypeToMessageType(JobTaskType jobTaskType) => jobTaskType switch {
            JobTaskType.ImportJob => MessageSource.ImportJob,
            JobTaskType.ExportJob => MessageSource.ExportJob,
            JobTaskType.ArchiveJob => MessageSource.ArchiveJob,
            JobTaskType.PrintJob => MessageSource.PrintJob,
            _ => MessageSource.Unknown
        };

        public void SendJobTaskMessage(string jobId, MessageType messageType, MessageSource messageSource, JobTaskStatus jobTaskStatus, string messageContent, int processedCount, int totalCount)
        {
            //发送消息通知
            var jobTaskMessage = new JobTaskMessage()
            {
                JobId = jobId,
                MessageType = messageType,
                JobStatus = jobTaskStatus,
                Content = string.Empty,
                ProgressedCount = processedCount,
                TotalCount = totalCount,
            };

            MessageInfo messageInfo = new MessageInfo()
            {
                Sender = messageSource,
                Level = MessageLevel.Info,
                Content = messageContent,
                SendTime = DateTime.Now,
                Remark = jobTaskMessage.ToJson()
            };
            this._messageService.SendMessage(messageInfo);
        }

        public void SendJobTaskMessage(string jobId, MessageType messageType, MessageSource messageSource, JobTaskStatus jobTaskStatus, string messageContent, string errorMessage, int processedCount, int totalCount)
        {
            //待完善处理，仅MessageSource.ImportJob和MessageSource.ExportJob
            var jobTaskMessage = new JobTaskMessage()
            {
                JobId = jobId,
                MessageType = messageType,
                JobStatus = jobTaskStatus,
                Content = messageContent,
                ProgressedCount = processedCount,
                TotalCount = totalCount,
            };
            MessageInfo messageInfo = new MessageInfo()
            {
                Sender = messageSource,
                Level = MessageLevel.Info,
                SendTime = DateTime.Now,
                Remark = jobTaskMessage.ToJson(),
            };
            string message = messageContent;
            if (!string.IsNullOrEmpty(messageContent))
            {
                message = $"{LanguageResource.Content_ImportingFor} [{messageContent}]";
            }
            if (jobTaskStatus == JobTaskStatus.Processing && processedCount == 0)
            {
                message = $"{LanguageResource.Content_ImportingFor} [{messageContent}]";
            }
            else if (jobTaskStatus == JobTaskStatus.Completed)
            {
                message = $"{LanguageResource.Content_Importing_DoneFor} [{messageContent}]";
            }
            else if (jobTaskStatus == JobTaskStatus.Cancelled)
            {
                message = $"{LanguageResource.Content_Canceled_ImportingFor} [{messageContent}]";
            }
            else if (jobTaskStatus == JobTaskStatus.Failed)
            {
                message = $"{LanguageResource.Content_FailedToImportFor} [{errorMessage}]";
            }
            messageInfo.Content = message;
            this._messageService.SendMessage(messageInfo);
        }
    }
}
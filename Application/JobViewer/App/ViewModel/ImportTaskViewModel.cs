using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.JobViewer.Models;
using NV.CT.MessageService.Contract;
using NV.CT.UI.ViewModel;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.JobViewer.ViewModel;

public class ImportTaskViewModel : BaseViewModel
{
    private readonly ILogger<ImportTaskViewModel> _logger;
    private readonly IMessageService _messageService;
    private readonly IMapper _mapper;
    private readonly IJobRequestService _jobRequestService;

    private ObservableCollection<ImportTaskInfo>? _importJobTaskInfoList;
    public ObservableCollection<ImportTaskInfo> ImportJobTaskInfoList
    {
        get
        {
            return _importJobTaskInfoList;
        }
        set
        {
            this.SetProperty(ref _importJobTaskInfoList, value);
        }
    }

    public ImportTaskViewModel( ILogger<ImportTaskViewModel> logger,
                                IMapper mapper,
                                IMessageService messageService,
                                IJobRequestService jobRequestService)
    {
        this._logger = logger;
        this._mapper = mapper;
        this._messageService = messageService;
        this._jobRequestService = jobRequestService;
        this._importJobTaskInfoList = new ObservableCollection<ImportTaskInfo>();
        this._messageService.MessageNotify += OnNotifyMessage;
        Commands.Add(JobViewerConstants.COMMAND_REFRESH_IMPORT, new DelegateCommand(GetAllImportTaskList));
        this.GetAllImportTaskList();
    }

    private void OnNotifyMessage(object? sender, MessageInfo e)
    {
        if (e.Sender != MessageSource.ImportJob)
        {
            return;
        }

        if (e.Remark is null || e.Remark.Count() == 0)
        {
            return;
        }

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            this.RefreshImportJobSource(e.Remark, e.SendTime, e.Content);
        });
    }

    private void RefreshImportJobSource(string jobTaskJson, DateTime timeStamp, string messageContent)
    {
        var jobTaskMessage = JsonConvert.DeserializeObject<JobTaskMessage>(jobTaskJson);
        var foundTaskInfo = this.ImportJobTaskInfoList.FirstOrDefault(t => t.JobId == jobTaskMessage?.JobId);
        if (foundTaskInfo is not null) //如果存在，则更新Task状态
        {
            //如果StartTime未更新，则从数据库中取
            if (foundTaskInfo.StartTime is null)
            {
                var foundTask = this._jobRequestService.GetJobById(jobTaskMessage?.JobId, JobTaskType.ImportJob);
                foundTaskInfo.StartTime = foundTask?.StartedDateTime;
            }
            foundTaskInfo.JobStatus = jobTaskMessage.JobStatus.ToString();

            float progress = ((float)jobTaskMessage.ProgressedCount / (float)jobTaskMessage.TotalCount) * 100f;
            foundTaskInfo.SetForegroundColor(jobTaskMessage.JobStatus, progress);
            if (jobTaskMessage.JobStatus == JobTaskStatus.Failed ||
                jobTaskMessage.JobStatus == JobTaskStatus.Cancelled ||
                jobTaskMessage.JobStatus == JobTaskStatus.PartlyCompleted ||
                jobTaskMessage.JobStatus == JobTaskStatus.Completed)
            {
                foundTaskInfo.EndTime = timeStamp;
            }
            foundTaskInfo.ErrorMessage = messageContent;
        }
        else
        {
            //如果不存在，则取出该Task添加到数据源
            var foundTask = this._jobRequestService.GetJobById(jobTaskMessage?.JobId, JobTaskType.ImportJob);
            if (foundTask is null)
            {
                this._logger.LogWarning($"Failed to fetch ImportJob with jobId:{jobTaskMessage?.JobId}");
                return;
            }
            this.ImportJobTaskInfoList.Insert(0, this.MapImportTask(foundTask));
        }
    }

    private void GetAllImportTaskList()
    {
        this.ImportJobTaskInfoList.Clear();

        var beginDateTime = DateTime.Now.AddMonths(-1);
        var endDateTime = DateTime.Now.AddDays(1);
        //默认查询近1个月
        var queryJobRequest = new QueryJobRequest()
        {
            JobType = JobTaskType.ImportJob,
            JobTaskStatusList = new System.Collections.Generic.List<JobTaskStatus>() { JobTaskStatus.Queued,
                                                                                       JobTaskStatus.Processing,
                                                                                       JobTaskStatus.Completed,
                                                                                       JobTaskStatus.PartlyCompleted,
                                                                                       JobTaskStatus.Failed,
                                                                                       JobTaskStatus.Cancelled,
                                                                                       JobTaskStatus.Paused
                                                                                      },
            BeginDateTime = new DateTime(beginDateTime.Year, beginDateTime.Month, beginDateTime.Day, 0, 0, 0),
            EndDateTime = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, 0, 0, 0),
        };

        var fetchedTasks = this._jobRequestService.GetJobsByTypeAndStatus(queryJobRequest);
        foreach (var fetchedTask in fetchedTasks)
        {
            this.ImportJobTaskInfoList.Add(this.MapImportTask(fetchedTask));
        }
    }

    private ImportTaskInfo MapImportTask(JobTaskInfo jobTaskInfo)
    {
        var importTaskInfo = new ImportTaskInfo();
        importTaskInfo.JobId = jobTaskInfo.Id;
        importTaskInfo.WorkflowId = jobTaskInfo.WorkflowId;
        importTaskInfo.JobStatus = jobTaskInfo.JobStatus.ToString();
        importTaskInfo.Priority = jobTaskInfo.Priority;
        importTaskInfo.CreatedTime = jobTaskInfo.CreateTime;
        importTaskInfo.StartTime = jobTaskInfo.StartedDateTime;
        importTaskInfo.EndTime = jobTaskInfo.FinishedDateTime;

        var jobParameter = JsonConvert.DeserializeObject<ImportJobRequest>(jobTaskInfo.Parameter);
        if (jobParameter is null)
        {
            this._logger.LogError($"Parameter of job is null with jobId:{jobTaskInfo.Id}");
            return importTaskInfo;
        }
        importTaskInfo.SourcePath = jobParameter.SourcePath;
        importTaskInfo.TaskDescription = $"[{jobTaskInfo.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")}] {jobParameter.VirtualSourcePath}";

        importTaskInfo.SetForegroundColor(jobTaskInfo.JobStatus);

        return importTaskInfo;
    }
}

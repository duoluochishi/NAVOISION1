using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.JobViewer.Models;
using NV.CT.MessageService.Contract;
using NV.CT.UI.ViewModel;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.JobViewer.ViewModel;

public class ExportTaskViewModel : BaseViewModel
{
    private readonly ILogger<ExportTaskViewModel> _logger;
    private readonly IMessageService _messageService;
    private readonly IMapper _mapper;
    private readonly IJobRequestService _jobRequestService;
    private readonly IStudyService _studyService;
    private readonly ISeriesService _seriesService;

    private ObservableCollection<ExportTaskInfo>? _exportTaskInfoList;
    public ObservableCollection<ExportTaskInfo> ExportTaskInfoList
    {
        get
        {
            return _exportTaskInfoList;
        }
        set
        {
            this.SetProperty(ref _exportTaskInfoList, value);
        }

    }

    public ExportTaskViewModel( ILogger<ExportTaskViewModel> logger,
                                IMapper mapper,
                                IMessageService messageService,
                                IJobTaskService jobTaskService,
                                IJobRequestService jobRequestService,
                                IStudyService studyService,
                                ISeriesService seriesService)
    {
        this._logger = logger;
        this._mapper = mapper;
        this._messageService = messageService;
        this._jobRequestService = jobRequestService;
        this._studyService = studyService;
        this._seriesService = seriesService;
        this._exportTaskInfoList = new ObservableCollection<ExportTaskInfo>();
        this._messageService.MessageNotify += OnNotifyMessage;
        Commands.Add(JobViewerConstants.COMMAND_REFRESH_EXPORT, new DelegateCommand(GetAllExportTaskList));
        this.GetAllExportTaskList();
    }

    private void OnNotifyMessage(object? sender, MessageInfo e)
    {
        if (e.Sender != MessageSource.ExportJob)
        {
            return;
        }

        if (e.Remark is null || e.Remark.Count() == 0)
        {
            return;
        }

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            this.RefreshExportJobSource(e.Remark , e.SendTime, e.Content);
        });
    }

    private void RefreshExportJobSource(string jobTaskJson, DateTime timeStamp, string messageContent)
    {
        var jobTaskMessage = JsonConvert.DeserializeObject<JobTaskMessage>(jobTaskJson);
        var foundTaskInfo = this.ExportTaskInfoList.FirstOrDefault(t => t.JobId == jobTaskMessage?.JobId);
        if (foundTaskInfo is not null) //如果存在，则更新Task状态
        {
            //如果StartTime未更新，则从数据库中取
            if (foundTaskInfo.StartTime is null)
            {
                var foundTask = this._jobRequestService.GetJobById(jobTaskMessage?.JobId, JobTaskType.ExportJob);
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
            var foundTask = this._jobRequestService.GetJobById(jobTaskMessage?.JobId, JobTaskType.ExportJob);
            if (foundTask is null)
            {
                this._logger.LogWarning($"Failed to fetch ExportJob with jobId:{jobTaskMessage?.JobId}");
                return;
            }

            this.ExportTaskInfoList.Insert(0, this.MapExportTask(foundTask));
        }
    }

    private void GetAllExportTaskList()
    {
        this.ExportTaskInfoList.Clear();

        var beginDateTime = DateTime.Now.AddMonths(-1);
        var endDateTime = DateTime.Now.AddDays(1);
        //默认查询近1个月
        var queryJobRequest = new QueryJobRequest()
        {
            JobType = JobTaskType.ExportJob,
            JobTaskStatusList = new List<JobTaskStatus>() { JobTaskStatus.Queued,
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
            this.ExportTaskInfoList.Add(this.MapExportTask(fetchedTask));
        }
    }

    private ExportTaskInfo MapExportTask(JobTaskInfo jobTaskInfo)
    {
        var exportTaskInfo = new ExportTaskInfo();
        exportTaskInfo.JobId = jobTaskInfo.Id;
        exportTaskInfo.WorkflowId = jobTaskInfo.WorkflowId;
        exportTaskInfo.JobStatus = jobTaskInfo.JobStatus.ToString();
        exportTaskInfo.Priority = jobTaskInfo.Priority;
        exportTaskInfo.CreatedTime = jobTaskInfo.CreateTime;
        exportTaskInfo.StartTime = jobTaskInfo.StartedDateTime;
        exportTaskInfo.EndTime = jobTaskInfo.FinishedDateTime;

        var jobParameter = JsonConvert.DeserializeObject<ExportJobRequest>(jobTaskInfo.Parameter);
        if (jobParameter is null)
        {
            this._logger.LogError($"Parameter of job is null with jobId:{jobTaskInfo.Id}");
            return exportTaskInfo;
        }

        if (jobParameter.OperationLevel == OperationLevel.Study) //如果是基于Study级别做归档
        {
            var studies = this._studyService.GetStudiesByIds(new string[] { jobParameter.StudyId });
            if (studies is null || studies.Length == 0)
            {
                this._logger.LogError($"Failed to fetch study with StudyId:{jobParameter.StudyId}");
                return exportTaskInfo;
            }

            exportTaskInfo.BodyPart = studies[0].BodyPart;

            var taskDescription = studies[0].StudyDescription;
            exportTaskInfo.TaskDescription = string.IsNullOrEmpty(taskDescription) ?
                                              exportTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss") :
                                              $"{taskDescription} {exportTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss")}";

        }
        else //如果是基于Series级别做归档
        {
            var seriesList = this._seriesService.GetSeriesBySeriesIds(jobParameter.SeriesIdList.ToArray());
            if (seriesList is null || seriesList.Length == 0)
            {
                this._logger.LogError($"Failed to fetch seriesList with StudyId:{jobParameter.StudyId}");
                return exportTaskInfo;
            }

            exportTaskInfo.BodyPart = seriesList[0].BodyPart;
            var taskDescription = seriesList[0].SeriesDescription;
            exportTaskInfo.TaskDescription = string.IsNullOrEmpty(taskDescription) ?
                                              exportTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss") :
                                              $"{taskDescription} {exportTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss")}";
        }

        exportTaskInfo.OperationLevel = jobParameter.OperationLevel.ToString();
        exportTaskInfo.PatientNames = string.Join(",", jobParameter.PatientNames);
        exportTaskInfo.OutputVirtualPath = jobParameter.OutputVirtualPath;

        exportTaskInfo.SetForegroundColor(jobTaskInfo.JobStatus);

        return exportTaskInfo;
    }
}

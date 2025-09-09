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
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.JobViewer.ViewModel;

public class ArchiveTaskViewModel : BaseViewModel
{
    private readonly ILogger<ArchiveTaskViewModel> _logger;
    private readonly IMessageService _messageService;
    private readonly IMapper _mapper;
    private readonly IJobRequestService _jobRequestService;
    private readonly IPatientService _patientService;
    private readonly IStudyService _studyService;
    private readonly ISeriesService _seriesService;

    private ObservableCollection<ArchiveJobTaskInfo>? _archiveJobTaskInfoList;
    public ObservableCollection<ArchiveJobTaskInfo> ArchiveJobTaskInfoList
    {
        get 
        {
            return _archiveJobTaskInfoList;
        }
        set 
        {
            this.SetProperty(ref _archiveJobTaskInfoList, value);
        }

    }

    public ArchiveTaskViewModel(ILogger<ArchiveTaskViewModel> logger,
                                IMapper mapper,
                                IMessageService messageService,
                                IJobTaskService jobTaskService,
                                IJobRequestService jobRequestService,
                                IPatientService patientService,
                                IStudyService studyService,
                                ISeriesService seriesService)
    {
        this._logger = logger;
        this._mapper = mapper;
        this._messageService = messageService;
        this._patientService = patientService;
        this._jobRequestService = jobRequestService;
        this._studyService = studyService;
        this._seriesService = seriesService;
        this._archiveJobTaskInfoList = new ObservableCollection<ArchiveJobTaskInfo>();
        this._messageService.MessageNotify += OnNotifyMessage;
        Commands.Add(JobViewerConstants.COMMAND_REFRESH_ARCHIVE, new DelegateCommand(GetAllArchiveTaskList));
        this.GetAllArchiveTaskList();

    }

    private void OnNotifyMessage(object? sender, MessageInfo e)
    {
        if (e.Sender != MessageSource.ArchiveJob)
        {
            return;
        }

        if (e.Remark is null || e.Remark.Count() == 0)
        {
            return;
        }

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            this.RefreshArchiveJobSource(e.Remark, e.SendTime, e.Content);
        });
    }

    private void RefreshArchiveJobSource(string jobTaskJson, DateTime timeStamp, string messageContent)
    {
        var jobTaskMessage = JsonConvert.DeserializeObject<JobTaskMessage>(jobTaskJson);
        var foundTaskInfo = this.ArchiveJobTaskInfoList.FirstOrDefault(t => t.JobId == jobTaskMessage?.JobId);
        if (foundTaskInfo is not null) //如果存在，则更新Task状态
        {
            //如果StartTime未更新，则从数据库中取
            if (foundTaskInfo.StartTime is null)
            {
                var foundTask = this._jobRequestService.GetJobById(jobTaskMessage?.JobId, JobTaskType.ArchiveJob);
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
            var foundTask = this._jobRequestService.GetJobById(jobTaskMessage?.JobId, JobTaskType.ArchiveJob);
            if (foundTask is null)
            {
                this._logger.LogWarning($"Failed to fetch ArchiveJob with jobId:{jobTaskMessage?.JobId}");
                return;
            }

            this.ArchiveJobTaskInfoList.Insert(0, this.MapArchiveTask(foundTask));
        }
    }

    private void GetAllArchiveTaskList()
    {
        this.ArchiveJobTaskInfoList.Clear();

        var beginDateTime = DateTime.Now.AddMonths(-1);
        var endDateTime = DateTime.Now.AddDays(1);
        //默认查询近1个月
        var queryJobRequest = new QueryJobRequest()
        {
            JobType = JobTaskType.ArchiveJob,
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
            this.ArchiveJobTaskInfoList.Add(this.MapArchiveTask(fetchedTask));
        }
    }

    private ArchiveJobTaskInfo MapArchiveTask(JobTaskInfo jobTaskInfo)
    {
        var archiveTaskInfo = new ArchiveJobTaskInfo();
        archiveTaskInfo.JobId = jobTaskInfo.Id;
        archiveTaskInfo.WorkflowId = jobTaskInfo.WorkflowId;
        archiveTaskInfo.JobStatus = jobTaskInfo.JobStatus.ToString();
        archiveTaskInfo.Priority = jobTaskInfo.Priority;
        archiveTaskInfo.CreatedTime = jobTaskInfo.CreateTime;
        archiveTaskInfo.StartTime = jobTaskInfo.StartedDateTime;
        archiveTaskInfo.EndTime = jobTaskInfo.FinishedDateTime;

        var jobParameter = JsonConvert.DeserializeObject<ArchiveJobRequest>(jobTaskInfo.Parameter);
        if (jobParameter is null)
        {
            this._logger.LogError($"Parameter of job is null with jobId:{jobTaskInfo.Id}");
            return archiveTaskInfo;
        }

        var studies = this._studyService.GetStudiesByIds(new string[] { jobParameter.StudyId });
        if (studies is null || studies.Length == 0)
        {
            this._logger.LogError($"Failed to fetch study with StudyId:{jobParameter.StudyId}");
            return archiveTaskInfo;
        }

        var patient = this._patientService.GetPatientById(studies[0].InternalPatientId);
        if (patient is null)
        {
            this._logger.LogError($"Failed to fetch patient with PatientId:{studies[0].InternalPatientId}");
            return archiveTaskInfo;
        }
    
        if (jobParameter.ArchiveLevel == ArchiveLevel.Study) //如果是基于Study级别做归档
        {
            archiveTaskInfo.BodyPart = studies[0].BodyPart;   

            var taskDescription = studies[0].StudyDescription;
            archiveTaskInfo.TaskDescription = string.IsNullOrEmpty(taskDescription) ? 
                                              archiveTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss") :
                                              $"{taskDescription} {archiveTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss")}";           
        }
        else //如果是基于Series级别做归档
        {
            var seriesList = this._seriesService.GetSeriesBySeriesIds(jobParameter.SeriesIdList.ToArray());
            if (seriesList is null || seriesList.Length == 0)
            {
                this._logger.LogError($"Failed to fetch seriesList with StudyId:{JsonConvert.SerializeObject(jobParameter)}");
                return archiveTaskInfo;
            }

            archiveTaskInfo.BodyPart = seriesList[0].BodyPart;
            var taskDescription = seriesList[0].SeriesDescription;
            archiveTaskInfo.TaskDescription = string.IsNullOrEmpty(taskDescription) ?
                                              archiveTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss") :
                                              $"{taskDescription} {archiveTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss")}";
        }

        archiveTaskInfo.ArchiveLevel = jobParameter.ArchiveLevel.ToString();
        archiveTaskInfo.Host = jobParameter.Host;
        archiveTaskInfo.Port = jobParameter.Port.ToString();
        archiveTaskInfo.AECaller = jobParameter.AECaller;
        archiveTaskInfo.AETitle = jobParameter.AETitle; 
        archiveTaskInfo.PatientID = patient.PatientId;
        archiveTaskInfo.PatientName = patient.PatientName;

        archiveTaskInfo.SetForegroundColor(jobTaskInfo.JobStatus);

        return archiveTaskInfo;
    }
}

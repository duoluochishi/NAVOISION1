using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Helpers;
using NV.CT.DatabaseService.Contract;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.JobViewer.Models;
using NV.CT.Language;
using NV.CT.MessageService.Contract;
using NV.CT.UI.ViewModel;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.JobViewer.ViewModel;

public class PrintTaskViewModel : BaseViewModel
{
    private readonly ILogger<PrintTaskViewModel> _logger;
    private readonly IMessageService _messageService;
    private readonly IMapper _mapper;
    private readonly IJobRequestService _jobRequestService;
    private readonly IPatientService _patientService;
    private readonly IStudyService _studyService;

    private ObservableCollection<PrintJobTaskInfo>? _printTaskInfoList;
    public ObservableCollection<PrintJobTaskInfo> PrintJobTaskInfoList
    {
        get 
        {
            return _printTaskInfoList;
        }
        set 
        {
            _printTaskInfoList = value;
            RaisePropertyChanged();
        }

    }
        
    private DateTime _beginDate = DateTime.Now.AddMonths(-1);
    public DateTime BeginDate
    {
        get => _beginDate;
        set => this.SetProperty(ref _beginDate, value);    
    }

    private DateTime _endDate = DateTime.Now;
    public DateTime EndDate
    {
        get => _endDate;
        set => this.SetProperty(ref _endDate, value);
    }

    private string _patientName = string.Empty;
    public string PatientName
    {
        get => _patientName;
        set => this.SetProperty(ref _patientName, value);
    }

    private List<string> _bodyParts = new List<string>();
    public List<string> BodyParts
    {
        get => _bodyParts;
        set => this.SetProperty(ref _bodyParts, value);
    }

    private string _selectedBodyPart = LanguageResource.Content_SelectAll;
    public string SelectedBodyPart
    {
        get =>_selectedBodyPart;
        set => this.SetProperty(ref _selectedBodyPart, value);
    }

    public PrintTaskViewModel(ILogger<PrintTaskViewModel> logger, 
                              IMapper mapper, 
                              IMessageService messageService,
                              IJobTaskService jobTaskService,
                              IJobRequestService jobRequestService,
                              IPatientService patientService,
                              IStudyService studyService)
    {
        this._logger = logger;
        this._mapper = mapper;
        this._messageService = messageService;
        this._patientService = patientService;
        this._jobRequestService = jobRequestService;
        this._studyService = studyService;
        this._printTaskInfoList = new ObservableCollection<PrintJobTaskInfo>();
        this._messageService.MessageNotify += OnNotifyMessage;
        Commands.Add(JobViewerConstants.COMMAND_REFRESH_PRINT, new DelegateCommand(GetAllPrintTaskList));

        Initialize();
    }

    private void Initialize()
    {
        BodyParts.Add(LanguageResource.Content_SelectAll);
        BodyParts.Add(BodyPart.Abdomen.ToString());
        BodyParts.Add(BodyPart.Arm.ToString());
        BodyParts.Add(BodyPart.Breast.ToString());
        BodyParts.Add(BodyPart.Head.ToString());
        BodyParts.Add(BodyPart.Leg.ToString());
        BodyParts.Add(BodyPart.Neck.ToString());
        BodyParts.Add(BodyPart.Pelvis.ToString());
        BodyParts.Add(BodyPart.Shoulder.ToString());
        BodyParts.Add(BodyPart.Spine.ToString());
        BodyParts.Add(BodyPart.BArm.ToString());
        BodyParts.Add(BodyPart.BAbdomen.ToString());
        BodyParts.Add(BodyPart.BBreast.ToString());
        BodyParts.Add(BodyPart.BHead.ToString());
        BodyParts.Add(BodyPart.BLeg.ToString());
        BodyParts.Add(BodyPart.BNeck.ToString());
        SelectedBodyPart = LanguageResource.Content_SelectAll; 
        this.GetAllPrintTaskList();
    }

    private void OnNotifyMessage(object? sender, MessageInfo e)
    {
        if (e.Sender != MessageSource.PrintJob)
        {
            return;
        }

        if (e.Remark is null || e.Remark.Count() == 0)
        {
            return;
        }

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            this.RefreshPrintJobSource(e.Remark, e.SendTime, e.Content);
        });
    }

    private void RefreshPrintJobSource(string jobTaskJson, DateTime timeStamp, string messageContent)
    {
        var jobTaskMessage = JsonConvert.DeserializeObject<JobTaskMessage>(jobTaskJson);
        var foundPrintTaskInfo = this.PrintJobTaskInfoList.FirstOrDefault(t => t.JobId == jobTaskMessage?.JobId);        
        if (foundPrintTaskInfo is not null) //如果存在，则更新Task状态
        {
            //如果StartTime未更新，则从数据库中取
            if (foundPrintTaskInfo.StartTime is null)
            {
                var foundTask = this._jobRequestService.GetJobById(jobTaskMessage?.JobId, JobTaskType.PrintJob);
                foundPrintTaskInfo.StartTime = foundTask?.StartedDateTime;
            }

            foundPrintTaskInfo.JobStatus = jobTaskMessage.JobStatus.ToString();

            float progress = ((float)jobTaskMessage.ProgressedCount / (float)jobTaskMessage.TotalCount) * 100f;
            foundPrintTaskInfo.SetForegroundColor(jobTaskMessage.JobStatus, progress);
            if (jobTaskMessage.JobStatus == JobTaskStatus.Failed ||
                jobTaskMessage.JobStatus == JobTaskStatus.Cancelled ||
                jobTaskMessage.JobStatus == JobTaskStatus.PartlyCompleted ||
                jobTaskMessage.JobStatus == JobTaskStatus.Completed)
            {
                foundPrintTaskInfo.EndTime = timeStamp;
            }

            foundPrintTaskInfo.ErrorMessage = messageContent;

        }
        else
        {
            //如果不存在，则取出该Task添加到数据源
            var foundTask = this._jobRequestService.GetJobById(jobTaskMessage?.JobId, JobTaskType.PrintJob);
            if (foundTask is null)
            {
                this._logger.LogWarning($"Failed to fetch PrintJob with jobId:{jobTaskMessage?.JobId}");
                return;
            }

            this.PrintJobTaskInfoList.Insert(0, this.MapPrintTask(foundTask));
        }
    }

    private void GetAllPrintTaskList()
    {
        this.PrintJobTaskInfoList.Clear();

        var endDateTime = EndDate.AddDays(1);
        //默认查询近1个月
        var queryJobRequest = new QueryJobRequest()
        {
            JobType = JobTaskType.PrintJob,
            JobTaskStatusList = new System.Collections.Generic.List<JobTaskStatus>() { JobTaskStatus.Queued,
                                                                                        JobTaskStatus.Processing,
                                                                                        JobTaskStatus.Completed,
                                                                                        JobTaskStatus.PartlyCompleted,
                                                                                        JobTaskStatus.Failed,
                                                                                        JobTaskStatus.Cancelled,
                                                                                        JobTaskStatus.Paused
                                                                                    },
            BeginDateTime = new DateTime(BeginDate.Year, BeginDate.Month, BeginDate.Day, 0, 0, 0),
            EndDateTime = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, 0, 0, 0),
            PatientName = PatientName,
            BodyPart = SelectedBodyPart == LanguageResource.Content_SelectAll ? string.Empty : SelectedBodyPart.ToString(),
        };

        var fetchedTasks = this._jobRequestService.GetJobsByTypeAndStatus(queryJobRequest);
        foreach (var fetchedTask in fetchedTasks)
        {           
            this.PrintJobTaskInfoList.Add(this.MapPrintTask(fetchedTask));          
        }
    }

    private PrintJobTaskInfo MapPrintTask(JobTaskInfo jobTaskInfo)
    {
        var printTaskInfo = new PrintJobTaskInfo();
        printTaskInfo.JobId = jobTaskInfo.Id;
        printTaskInfo.WorkflowId = jobTaskInfo.WorkflowId;
        printTaskInfo.JobStatus = jobTaskInfo.JobStatus.ToString();
        printTaskInfo.Priority = jobTaskInfo.Priority;
        printTaskInfo.CreatedTime = jobTaskInfo.CreateTime;
        printTaskInfo.StartTime = jobTaskInfo.StartedDateTime;
        printTaskInfo.EndTime = jobTaskInfo.FinishedDateTime;

        var jobParameter = JsonConvert.DeserializeObject<PrintJobRequest>(jobTaskInfo.Parameter);
        if (jobParameter is null)
        {
            this._logger.LogError($"Parameter of job is empty with jobId:{jobTaskInfo.Id}");
            return printTaskInfo;
        }

        var patient = this._patientService.GetPatientById(jobParameter.PatientId);
        if (patient is null)
        {
            this._logger.LogError($"Failed to fetch patient with PatientId:{jobParameter.PatientId}");
            return printTaskInfo;
        }

        var studies = this._studyService.GetStudiesByIds(new string[] { jobParameter.StudyId });
        if (studies is null || studies.Length == 0)
        {
            this._logger.LogError($"Failed to fetch study with StudyId:{jobParameter.StudyId}");
            return printTaskInfo;
        }

        printTaskInfo.AEPrinter = jobParameter is null ? string.Empty : jobParameter.CalledAE;
        printTaskInfo.PatientID = patient is null ? string.Empty : patient.PatientId;
        printTaskInfo.PatientName = patient is null ? string.Empty : patient.PatientName;
        printTaskInfo.NumbersOfCopies = jobParameter is null ? 0 : jobParameter.NumberOfCopies;
        printTaskInfo.PageSize = jobParameter is null ? string.Empty : jobParameter.PageSize;
        printTaskInfo.BodyPart = studies[0].BodyPart;
        printTaskInfo.TaskDescription = string.IsNullOrEmpty(studies[0].StudyDescription) ? printTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss") :
                                                                                            $"{studies[0].StudyDescription} {printTaskInfo.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss")}";
        printTaskInfo.SetForegroundColor(jobTaskInfo.JobStatus);

        return printTaskInfo;
    }
}

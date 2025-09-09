//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/10/18 13:45:36    V1.0.0        胡安
// </summary>
//-----------------------------------------------------------------------

using Autofac;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigService.Contract;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.JobService.Contract;
using NV.CT.Language;
using NV.CT.PatientBrowser.ApplicationService.Contract.Interfaces;
using NV.CT.PatientBrowser.Models;
using NV.CT.PatientBrowser.View.English;
using NV.CT.SyncService.Contract;
using NV.CT.UI.Controls;
using NV.CT.WorkflowService.Contract;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NV.CT.PatientBrowser.ViewModel;

public class WorkListViewModel : UI.ViewModel.BaseViewModel
{
    private readonly IStudyApplicationService _studyService;
    private readonly IMapper _mapper;
    private readonly IPatientConfigService _patientConfigService;
    private readonly IWorkflow _workflow;
    private readonly IDataSync _dataSync;
    private readonly IJobRequestService _jobRequestService;
    private readonly IAuthorization _authorizationService;
    private readonly ILogger<WorkListViewModel> _logger;
    private string _startQueryDate = string.Empty;
    private string _endQueryDate = string.Empty;
    private SearchTimeType _searchTimeType = SearchTimeType.Today;
    FiltrationWindow? _filtrationWindow;
    private ObservableCollection<VStudyModel> _cachedAllStudies = new ObservableCollection<VStudyModel>();

    private string? _selectedStudyIdFromDataSync = null; //用以区别用户主动选择和接收DataSync同步选择事件的场景，避免交叉干扰
    private object _studyIdFromDataSyncLock = new object();
    private bool _canIgnoreDataSyncEvent = false;
    private string _previousSelectedStudyId = string.Empty;

    private ObservableCollection<VStudyModel> _studies = new ObservableCollection<VStudyModel>();
    public ObservableCollection<VStudyModel> Studies
    {
        get => this._studies;
        set => SetProperty(ref this._studies, value);
    }

    private string _scheduledDescription = string.Empty;
    public string ScheduledDescription
    {
        get => this._scheduledDescription;
        set => SetProperty(ref this._scheduledDescription, value);
    }

    private VStudyModel? _selectedItem = null;
    public VStudyModel? SelectedItem
    {
        get => this._selectedItem;
        set
        {
            SetProperty(ref this._selectedItem, value);
            if (this._selectedItem is not null)
            {
                this._previousSelectedStudyId = this._selectedItem.StudyId;
            }
        }
    }

    public WorkListViewModel(IStudyApplicationService studyService, 
                             IMapper mapper,
                             ILogger<WorkListViewModel> logger,
                             IWorkflow workflow, 
                             IPatientConfigService patientConfigService,
                             IJobRequestService jobRequestService,
                             IAuthorization authorizationService,
                             IDataSync dataSync)
    {
        _dataSync = dataSync;
        _studyService = studyService;
        _mapper = mapper;
        _logger = logger;
        _workflow = workflow;
        _patientConfigService = patientConfigService;
        _jobRequestService = jobRequestService;
        _authorizationService = authorizationService;
        _studyService.RefreshWorkList += OnStudyServiceRefreshWorkList;
        _workflow.WorkflowStatusChanged += OnWorkflowStatusChanged;
        _studyService.RefreshRearchDateType += OnStudyServiceRefreshRearchDateType;
        _dataSync.SelectStudyChanged += OnDataSyncSelectStudyChanged;

        Commands.Add(PatientConstString.COMMNAD_REFRESH_WORK_LIST, new DelegateCommand(OnRefreshWorkList));
        Commands.Add(PatientConstString.COMMAND_SEARCH, new DelegateCommand<string>(OnSerach));
        Commands.Add(PatientConstString.COMMAND_FILTER, new DelegateCommand<object>(OnFiltrationWindowDialog));
        Commands.Add(PatientConstString.COMMAND_SELECT_WORKLIST_PATIENT, new DelegateCommand(OnSelectedPatientChanged));

        _searchTimeType = _patientConfigService.GetConfigs().PatientQueryConfig.PatientQueryTimeType;
        InitPatientQuery(_searchTimeType);
        InitializeWorkList();
    }

    private void OnSelectedPatientChanged()
    {
        NotifySelectedPatientChanged();
    }

    [UIRoute]
    private void OnStudyServiceRefreshRearchDateType(object? sender, CTS.EventArgs<SearchTimeType> e)
    {
        if (e is null)
        {
            return;
        }
        _searchTimeType = e.Data;
        OnRefreshWorkList();
    }

    [UIRoute]
    private void OnWorkflowStatusChanged(object? sender, string e)
    {
        if (!string.IsNullOrEmpty(e) && (e.Equals(WorkflowStatus.ExaminationClosing.ToString()) || e.Equals(WorkflowStatus.ExaminationStarting.ToString())))
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                OnRefreshWorkList();
            });
        }
    }

    [UIRoute]
    private void OnStudyServiceRefreshWorkList(object? sender, CTS.EventArgs<(ApplicationService.Contract.Models.PatientModel patientModel, ApplicationService.Contract.Models.StudyModel studyModel, DataOperateType dataOperateType)> e)
    {
        //如果是更新操作，则记住选择项
        this._previousSelectedStudyId = e.Data.dataOperateType == DataOperateType.Update ? e.Data.studyModel.Id : string.Empty;

        if (e.Data.dataOperateType == DataOperateType.Add)
        {
            //2024-04-12 根据型检要求，如果是增加患者信息，则保存后应当立即清空输入框，以便再次继续登记新的患者信息，所以这里区别新增操作和其它操作
            bool isAddOperatoin = true;
            InitializeWorkList(isAddOperatoin);
        }
        else if (e.Data.dataOperateType == DataOperateType.Update || e.Data.dataOperateType == DataOperateType.ResumeExamination || e.Data.dataOperateType == DataOperateType.UpdateStudyStatus)
        {
            //如果是更新模式，则恢复选中上次的选中项
            bool isAddOperatoin = false;
            InitializeWorkList(isAddOperatoin);
        }
        else if (e.Data.dataOperateType == DataOperateType.Delete)
        {
            this._previousSelectedStudyId = string.Empty;
            bool isAddOperatoin = false;
            InitializeWorkList(isAddOperatoin);
        }
        else if (e.Data.dataOperateType == DataOperateType.AddFromHisRis)
        {
            //区别于分支DataOperateType.Add：如果是从HIS或RIS获取到了新数据,不需要清空输入框
            this.RefreshWorkListByHisRis();
        }        
    }

    /// <summary>
    /// 模糊查询
    /// </summary>
    /// <param name="keyword"></param>
    [UIRoute]
    public void OnSerach(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            Studies = this._cachedAllStudies;
            return;
        }
        var key = keyword.ToLower();
        Func<VStudyModel, bool> func = r =>
        {
            if (string.IsNullOrEmpty(r.PatientName))
            {
                r.PatientName = string.Empty;
            }
            if (string.IsNullOrEmpty(r.PatientId))
            {
                r.PatientId = string.Empty;
            }
            return (r.PatientName.ToLower().Contains(key) || r.PatientId.ToLower().Contains(key));
        };
        Studies = new ObservableCollection<VStudyModel>(this._cachedAllStudies.Where(func).ToList());
    }

    /// <summary>
    /// 刷新WorkList
    /// </summary>
    private void OnRefreshWorkList()
    {
        InitializeWorkList(false);
    }

    /// <summary>
    /// 初始化WorkList
    /// </summary>
    public void InitializeWorkList(bool isAddOperation = false)
    {
        this.LoadWorkList();

        if (Studies.Count == 0)
        {
            var patientModel = new ApplicationService.Contract.Models.PatientModel();
            var studyModel = new ApplicationService.Contract.Models.StudyModel();
            _studyService.RaiseSelectItemChanged((patientModel, studyModel));
            return;
        }

        if (isAddOperation)
        {
            //2024-04-12 根据型检要求，如果是增加患者信息，则保存后应当立即清空输入框，以便再次继续登记新的患者信息，所以这里区别新增操作和其它操作
            var patientModel = new ApplicationService.Contract.Models.PatientModel();
            var studyModel = new ApplicationService.Contract.Models.StudyModel();
            _studyService.RaiseSelectItemChanged((patientModel, studyModel));
            this._canIgnoreDataSyncEvent = true;
            _dataSync?.SelectStudy(Studies[0].StudyId); //通知dataSync变更事件
            return;            
        }

        if (!string.IsNullOrEmpty(this._previousSelectedStudyId))
        {
            //如果是更新，则记住上次选择项
            var findStudy = Studies.FirstOrDefault(n => n.StudyId == this._previousSelectedStudyId);
            //SelectedItem = findStudy is not null ? findStudy : Studies[0];
            NotifySelectedPatientChanged();
        }
        else
        {
            //SelectedItem = Studies[0]; //选定SelectedItem时会通知dataSync变更事件
            NotifySelectedPatientChanged();
        }
    }

    private void RefreshWorkListByHisRis()
    {
        this.LoadWorkList();

        //如果编辑区是修改状态,则需要恢复上次选择项; 如果编辑区是新增状态，则无需任何操作;
        var paViewModel = Global.Instance.ServiceProvider.GetRequiredService<PatientInfoViewModel>();
        if (paViewModel.SelectedVStudyEntity is not null && !string.IsNullOrEmpty(paViewModel.SelectedVStudyEntity.Pid))
        {
            //如果编辑区是修改状态,则需要恢复上次选择项
            if (string.IsNullOrEmpty(this._previousSelectedStudyId))
            {
                NotifyPatientChanged();
                return;
            }

            //如果是更新，则记住上次选择项
            var findStudy = Studies.FirstOrDefault(n => n.StudyId == this._previousSelectedStudyId);
            //SelectedItem = findStudy is not null ? findStudy : Studies[0];
            NotifySelectedPatientChanged();
        }else
        {
            NotifyPatientChanged();
        }
    }

    private void LoadWorkList()
    {
        InitPatientQuery(this._searchTimeType);
        var result = _studyService.GetPatientStudyListWithNotStartedStudyDate(this._startQueryDate, this._endQueryDate);
        var list = result.Select(r =>
        {
            string firstName = string.Empty;
            string lastName = string.Empty;
            if (r.patientModel.PatientName.Contains("^"))
            {
                string[] arr = r.patientModel.PatientName.Split('^');
                firstName = arr[0];
                lastName = arr[1];
            }
            else
            {
                lastName = r.Item1.PatientName;
            }
            return new VStudyModel
            {
                PatientName = r.patientModel.PatientName,
                Age = r.studyModel.Age.ToString(),
                AgeType = r.studyModel.AgeType,
                Weight = r.studyModel.PatientWeight?.ToString(),
                Height = r.studyModel.PatientSize?.ToString(),
                Pid = r.patientModel.Id,
                StudyId = r.studyModel.Id,
                PatientId = r.patientModel.PatientId,
                LastName = lastName,
                FirstName = firstName,
                StudyInstanceUID = r.studyModel.StudyInstanceUID,
                Gender = (int)r.patientModel.PatientSex,
                CreateTime = r.patientModel.CreateTime,
                AdmittingDiagnosis = r.studyModel.AdmittingDiagnosisDescription,
                Ward = r.studyModel.Ward,
                ReferringPhysicianName = r.studyModel.ReferringPhysicianName,
                BodyPart = r.studyModel.BodyPart,
                HisStudyID = r.studyModel.StudyId,
                AccessionNo = r.studyModel.AccessionNo,
                Comments = r.studyModel.Comments,
                InstitutionName = r.studyModel.InstitutionName,
                InstitutionAddress = r.studyModel.InstitutionAddress,
                PatientType = r.studyModel.PatientType,
                StudyDescription = r.studyModel.StudyDescription,
            };
        }).ToList();
        this._cachedAllStudies = list.ToObservableCollection();
        Studies = this._cachedAllStudies;

        //RGT同步
        _dataSync.RefreshWorkList(_mapper.Map<List<(DatabaseService.Contract.Models.PatientModel,DatabaseService.Contract.Models.StudyModel)>>(result));
    }

    private void InitPatientQuery(SearchTimeType patientQueryTimeType)
    {
        DateTime startDateTime = DateTime.Now;
        DateTime endDateTime = DateTime.Now;
        switch (patientQueryTimeType)
        {
            case SearchTimeType.Yesterday:
                startDateTime = DateTime.Now.AddDays(-1);
                endDateTime = startDateTime;
                break;
            case SearchTimeType.DayBeforeYesterday:
                startDateTime = DateTime.Now.AddDays(-2);
                endDateTime = startDateTime;
                break;
            case SearchTimeType.Last7Days:
                startDateTime = DateTime.Now.AddDays(-7);
                endDateTime = DateTime.Now;
                break;
            case SearchTimeType.Last30Days:
                startDateTime = DateTime.Now.AddDays(-30);
                endDateTime = DateTime.Now;
                break;
            case SearchTimeType.All:
                startDateTime = DateTime.MinValue;
                endDateTime = DateTime.Now;
                break;
            case SearchTimeType.Today:
            default:
                startDateTime = DateTime.Now;
                endDateTime = DateTime.Now;
                break;
        }
        InitQueryByDate(startDateTime, endDateTime);
    }

    private void InitQueryByDate(DateTime startDateTime, DateTime endDateTime)
    {
        switch (_searchTimeType)
        {
            case SearchTimeType.Today:
                ScheduledDescription = $"{LanguageResource.Content_ScheduledFor} {LanguageResource.Content_Today} ( {startDateTime.ToString("yyyy/MM/dd")} )";
                break;
            case SearchTimeType.Yesterday:
                ScheduledDescription = $"{LanguageResource.Content_ScheduledFor} {LanguageResource.Content_Yesterday} ( {startDateTime.ToString("yyyy/MM/dd")} )";
                break;
            case SearchTimeType.DayBeforeYesterday:
                ScheduledDescription = $"{LanguageResource.Content_ScheduledFor} {LanguageResource.Content_DayBeforeYesterday} ( {startDateTime.ToString("yyyy/MM/dd")} )";
                break;
            case SearchTimeType.Last7Days:
                ScheduledDescription = $"{LanguageResource.Content_ScheduledFor} {LanguageResource.Content_WithinTheLast7Days} ( {startDateTime.ToString("yyyy/MM/dd")}----{endDateTime.ToString("yyyy/MM/dd")} )";
                break;
            case SearchTimeType.Last30Days:
                ScheduledDescription = $"{LanguageResource.Content_ScheduledFor} {LanguageResource.Content_WithinTheLast30Days} ( {startDateTime.ToString("yyyy/MM/dd")}----{endDateTime.ToString("yyyy/MM/dd")} )";
                break;
            case SearchTimeType.All:
                ScheduledDescription = $"{LanguageResource.Content_ScheduledFor} {LanguageResource.Content_All}";
                break;
            default:
                ScheduledDescription = $"{LanguageResource.Content_ScheduledFor} {LanguageResource.Content_All}";
                break;
        }

        this._startQueryDate = $"{startDateTime.ToString("yyyy-MM-dd")} 00:00:00";
        this._endQueryDate = $"{endDateTime.ToString("yyyy-MM-dd")} 23:59:59";
    }

    /// <summary>
    /// RGT 选中 Study
    /// </summary>
    private void OnDataSyncSelectStudyChanged(object? sender, string e)
    {
        //如果当前不需要处理RGT发来的事件，则重置标记位并返回
        if (this._canIgnoreDataSyncEvent)
        {
            this._canIgnoreDataSyncEvent = false;
            return;
        }            

        var foundStudy = Studies.FirstOrDefault(n => n.StudyId == e);
        if (foundStudy is null)
            return;

        if (e == SelectedItem?.StudyId)
            return;

        _selectedStudyIdFromDataSync = e; //设置标记，告知SelectedItem属性改变值时不要再发变化事件，避免干扰
        SelectedItem = foundStudy;
        NotifySelectedPatientChanged();
    }

    private void OnFiltrationWindowDialog(object element)
    {
        if (_filtrationWindow is null)
        {
            _filtrationWindow = new();
            _filtrationWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            var button = element as Button;
            if (button != null)
            {
                var positionOfButton = button.PointFromScreen(new Point(0, 0));
                _filtrationWindow.Left = Math.Abs(positionOfButton.X);
                _filtrationWindow.Top = Math.Abs(positionOfButton.Y) + 40;
            }
        }
        _filtrationWindow.ShowWindowDialog(true);
    }

    private void NotifySelectedPatientChanged()
    {
        if (SelectedItem is null)
        {
            //_logger.LogDebug("value is null");
            _selectedStudyIdFromDataSync = null;
            NotifyPatientChanged();
            return;
        }

        lock (_studyIdFromDataSyncLock)
        {
            _studyService.RaiseSelectItemChanged((_mapper.Map<ApplicationService.Contract.Models.PatientModel>(SelectedItem),
                                                  _mapper.Map<ApplicationService.Contract.Models.StudyModel>(SelectedItem)));
            if (string.IsNullOrEmpty(_selectedStudyIdFromDataSync))
            {
                _dataSync?.SelectStudy(SelectedItem.StudyId); //如果不是DataSync时间被动引发的改变，则通知dataSync变更事件
            }
            else
            {
                _selectedStudyIdFromDataSync = null; //如果是DataSync引发的改变，这里不需要再触发dataSync变更事件，同时复原标记_selectedStudyIdFromDataSync
            }

        }


    }
    private void NotifyPatientChanged()
    {
        _studyService.RaiseSelectItemChanged((_mapper.Map<ApplicationService.Contract.Models.PatientModel>(null),
                                                  _mapper.Map<ApplicationService.Contract.Models.StudyModel>(null)));
    }

}
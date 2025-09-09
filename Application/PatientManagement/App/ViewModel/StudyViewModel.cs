//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using Autofac.Core;
using Newtonsoft.Json;
using NV.CT.AppService.Contract;
using NV.CT.ClientProxy.DataService;
using NV.CT.ConfigService.Contract;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.Language;
using NV.CT.MessageService.Contract;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.ApplicationService.Impl;
using NV.CT.PatientManagement.Helpers;
using NV.CT.PatientManagement.Models;
using NV.CT.UI.Controls.Archive;
using NV.CT.UI.Controls.Export;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections;
using System.Diagnostics.Metrics;

namespace NV.CT.PatientManagement.ViewModel;

public class StudyViewModel : BaseViewModel
{
	#region Members
	private readonly ILogger<StudyViewModel> _logger;
	private readonly IApplicationCommunicationService _applicationCommunicationService;
	private readonly IViewer _viewer;
    private readonly IScanTaskService _scanService;
    private ObservableCollection<VStudyModel> _vStudies = new();
	private ObservableCollection<VStudyModel> _gVStudies = new();
	private IList? _selectedItems;
	private VStudyModel? _selectedItem;
	private readonly IStudyApplicationService _studyApplicationService;
	private readonly IMapper _mapper;
	private readonly IDialogService _dialogService;
	private readonly IMessageService _messageService;
	private readonly IJobRequestService _jobRequestService;
	private readonly ISeriesService _seriesService;
	private readonly IPrint _printService;
	private readonly IAuthorization _authorizationService;
	private IFilterConfigService _filterConfigService;
	private DelegateCommand _deleteCommand;
	private DelegateCommand _resumeExamCommand;
	private DelegateCommand _correctCommand;
	private DelegateCommand _rawDataManageCommand;
	private DelegateCommand _exportCommand;
	private DelegateCommand _fileArchiveCommand;
	private StudyListFilterWindow? _filtrationWindow;
	private StudyListColumnsConfigWindow? _columnsConfigWindow;

	private const string DATE_FORMAT_STRING = "yyyy/MM/dd";
	private readonly string DATE_UNLIMITED_STRING = LanguageResource.Content_Unlimited;

	#endregion

	#region Properties

	public ObservableCollection<VStudyModel> VStudies
	{
		get => _vStudies;
		set
		{
			_vStudies = value;
			RaisePropertyChanged();
		}
	}

	public VStudyModel? SelectedItem
	{
		get => _selectedItem;
		set
		{
			SetProperty(ref _selectedItem, value);
			_studyApplicationService.RaiseSelectItemChanged(_selectedItem is null ? string.Empty : _selectedItem.StudyId);
		}
	}

	private bool _isViewerEnabled;
	public bool IsViewerEnabled
	{
		get => _isViewerEnabled;
		set => SetProperty(ref _isViewerEnabled, value);
	}

	private bool _isReconEnabled;
	public bool IsReconEnabled
	{
		get => _isReconEnabled;
		set => SetProperty(ref _isReconEnabled, value);
	}

	private bool _isFilmEnabled;
	public bool IsFilmEnabled
	{
		get => _isFilmEnabled;
		set => SetProperty(ref _isFilmEnabled, value);
	}

	private string _localDataDescription = string.Empty;
	public string LocalDataDescription
	{
		get => _localDataDescription;
		set => SetProperty(ref _localDataDescription, value);
	}

	public SearchTimeType SearchTimeType = SearchTimeType.Today;

	private bool _isPermissionArchiveEnabled;
	public bool IsPermissionArchiveEnabled
	{
		get => _isPermissionArchiveEnabled;
		set => SetProperty(ref _isPermissionArchiveEnabled, value);
	}

	private bool _isPermissionExportEnabled;
	public bool IsPermissionExportEnabled
	{
		get => _isPermissionExportEnabled;
		set => SetProperty(ref _isPermissionExportEnabled, value);
	}

	private bool _isPermissionDeletionEnabled;
	public bool IsPermissionDeletionEnabled
	{
		get => _isPermissionDeletionEnabled;
		set => SetProperty(ref _isPermissionDeletionEnabled, value);
	}

	private bool _isPermissionCorrectionEnabled;
	public bool IsPermissionCorrectionEnabled
	{
		get => _isPermissionCorrectionEnabled;
		set => SetProperty(ref _isPermissionCorrectionEnabled, value);
	}

	private bool _isPermissionExportRawEnabled;
	public bool IsPermissionExportRawEnabled
	{
		get => _isPermissionExportRawEnabled;
		set => SetProperty(ref _isPermissionExportRawEnabled, value);
	}

	private bool _isPermissionProtectionEnabled;
	public bool IsPermissionProtectionEnabled
	{
		get => _isPermissionProtectionEnabled;
		set => SetProperty(ref _isPermissionProtectionEnabled, value);
	}

	private bool _isPermissionImportEnabled;
	public bool IsPermissionImportEnabled
	{
		get => _isPermissionImportEnabled;
		set => SetProperty(ref _isPermissionImportEnabled, value);
	}

	public string CurrentUserAccount
	{
		get;
		set;
	} = string.Empty;

	#endregion

	#region Constructors

	public StudyViewModel(IStudyApplicationService studyApplicationService,
						  IMapper mapper,
						  IDialogService dialogService,
						  IApplicationCommunicationService applicationCommunicationService,
						  IViewer viewer,
						  ILogger<StudyViewModel> logger,
						  IMessageService messageService,
						  ISeriesService seriesService,
						  IPrint printService,
						  IAuthorization authorizationService,
						  IJobRequestService jobRequestService,
						  IFilterConfigService filterConfigService,
                          IScanTaskService scanService)
	{
		_logger = logger;
		_applicationCommunicationService = applicationCommunicationService;
		_mapper = mapper;
		_studyApplicationService = studyApplicationService;
		_dialogService = dialogService;
		_viewer = viewer;
		_selectedItems = new List<VStudyModel>();
		_messageService = messageService;
		_seriesService = seriesService;
		_printService = printService;
		_authorizationService = authorizationService;
		_jobRequestService = jobRequestService;
		_filterConfigService = filterConfigService;
		_scanService = scanService;

		//右键菜单功能
		_deleteCommand = new DelegateCommand(OnDelete);
		Commands.Add(PatientManagementConstants.COMMAND_DELETE, _deleteCommand);
		_resumeExamCommand = new DelegateCommand(OnResumeExamination);
		Commands.Add(PatientManagementConstants.COMMAND_RESUME, _resumeExamCommand);
		_correctCommand = new DelegateCommand(OnCorrect);
		Commands.Add(PatientManagementConstants.COMMAND_CORRECT, _correctCommand);
		_rawDataManageCommand = new DelegateCommand(OnRawDataManage);
		Commands.Add(PatientManagementConstants.COMMAND_RAW_DATA_MANAGE, _rawDataManageCommand);
		_exportCommand = new DelegateCommand(OnOpenExportWindow);
		Commands.Add(PatientManagementConstants.COMMAND_OPEN_DATA_EXPORT, _exportCommand);
		_fileArchiveCommand = new DelegateCommand(OnOpenFileArchiveWindow);
		Commands.Add(PatientManagementConstants.COMMAND_OPEN_FILE_ARCHIVE, _fileArchiveCommand);
		Commands.Add(PatientManagementConstants.COMMAND_REFRESH_WORKLIST, new DelegateCommand(OnRefreshWorkList));
		Commands.Add(PatientManagementConstants.COMMAND_SEARCH, new DelegateCommand<string>(OnSearch));
		Commands.Add(PatientManagementConstants.COMMAND_OPEN_RECON, new DelegateCommand(OnOpenReconProcess));
		Commands.Add(PatientManagementConstants.COMMAND_OPEN_PRINT, new DelegateCommand(OnOpenPrintProcess));
		Commands.Add(PatientManagementConstants.COMMAND_OPEN_VIEWER, new DelegateCommand(OnOpenViewer));
		Commands.Add(PatientManagementConstants.COMMAND_SWITCH_LOCKED_STATUS, new DelegateCommand<string>(OnSwitchLockedStatus));
		Commands.Add(PatientManagementConstants.COMMAND_SELECTION_CHANGED, new DelegateCommand<object>(OnSelectionChanged));
		Commands.Add(PatientManagementConstants.COMMAND_IMPORT, new DelegateCommand(OnImport));
		Commands.Add(PatientManagementConstants.COMMAND_FILTER, new DelegateCommand<object>(OnFiltrationWindowDialog));
		Commands.Add(PatientManagementConstants.COMMAND_COLUMNS_CONFIG, new DelegateCommand<object>(OnColumnsConfigWindowDialog));
		Commands.Add(PatientManagementConstants.COMMAND_IMPORT_RAWDATA, new DelegateCommand(OnImportRawdata));

		_studyApplicationService.RefreshPatientManagementStudyList += OnStudyServiceRefreshPatientManagementStudyList;
		_messageService.MessageNotify += OnNotifyMessage;
		_authorizationService.CurrentUserChanged += OnCurrentUserChanged;
		_filterConfigService.ConfigRefreshed += OnFilterConfigRefreshed;
		_applicationCommunicationService.ApplicationStatusChanged += OnApplicationStatusChanged;
		Task.Run(() => {
            this.SetPermissions();
            this.ResetButtonEnableStatus();
            this.QueryStudyListByFilterConfig();
        });
	}

	~StudyViewModel()
	{
		_studyApplicationService.RefreshPatientManagementStudyList -= OnStudyServiceRefreshPatientManagementStudyList;
		_messageService.MessageNotify -= OnNotifyMessage;
		_authorizationService.CurrentUserChanged -= OnCurrentUserChanged;
		_filterConfigService.ConfigRefreshed -= OnFilterConfigRefreshed;
		_applicationCommunicationService.ApplicationStatusChanged -= OnApplicationStatusChanged;
	}

	#endregion

	#region Events

	private void OnApplicationStatusChanged(object? sender, ApplicationResponse response)
	{
		switch (response.Status)
		{
			case ProcessStatus.Starting:
				this.ResetCanExecuteStatus(response, false);
				break;
			case ProcessStatus.Started:
				this.ResetCanExecuteStatus(response, true);
				break;
			default:
				break;
		}
	}

	private void ResetCanExecuteStatus(ApplicationResponse response, bool isEnabled)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			if (response.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_VIEWER)
			{
				IsViewerEnabled = isEnabled;
			}
			else if (response.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_RECON)
			{
				IsReconEnabled = isEnabled;
			}
			else if (response.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_PRINT)
			{
				IsFilmEnabled = isEnabled;
			}

		});
	}

	private void OnNotifyMessage(object? sender, MessageInfo messageInfo)
	{
		if (messageInfo.Sender == MessageSource.ImportJob)
		{
			this.HandleImportJob(messageInfo);
		}
	}

	private void HandleImportJob(MessageInfo messageInfo)
	{
		if (null == messageInfo.Remark || messageInfo.Remark.Length == 0)
		{
			_logger.LogError($"Received paramter is empty.");
			return;
		}

		JobTaskMessage importMessageInfo;
		try
		{
			importMessageInfo = JsonConvert.DeserializeObject<JobTaskMessage>(messageInfo.Remark);
		}
		catch (Exception ex)
		{
			_logger.LogError($"Received paramter's type is not JobTaskMessage. The error is:{ex.Message}.");
			return;
		}

		if (importMessageInfo?.JobStatus == JobTaskStatus.Completed)
		{
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				OnRefreshWorkList();
			});
		}
	}

	private void OnOpenReconProcess()
	{
		if (SelectedItem is null)
			return;

		this.IsReconEnabled = false;
		Task.Run(() =>
		{
			var studyId = SelectedItem.StudyId;
			_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_RECON,
				 studyId
			));
		});
        AuditLogger.Log(new AuditLogInfo
        {
            CreateTime = DateTime.Now,
            EventType = "Action",
            EntryPoint = $"{this.GetType().Name}.{nameof(OnOpenReconProcess)}",
            UserName = Environment.UserName,
            Description = "",
            OriginalValues = JsonConvert.SerializeObject(SelectedItem.StudyId),
            ReturnValues = JsonConvert.SerializeObject(null)
        });
    }

	private void OnOpenPrintProcess()
	{
		if (SelectedItem is null)
			return;

		_logger.LogInformation($"Start print process for StudyID:{SelectedItem.StudyId}");

		if (!_printService.ChceckExists())
		{
			_logger.LogInformation("No existing print task for now.");
			this.IsFilmEnabled = false;
			Task.Run(() =>
			{
				_printService.StartPrint(SelectedItem.StudyId);
				_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_PRINT));
			});
		}
		else
		{
			_dialogService.ShowDialog(true, MessageLeveles.Warning,
									  LanguageResource.Message_Info_CloseWarningTitle,
									  LanguageResource.Message_Warning_CurrentPrinting, arg =>
			{
				if (arg.Result == ButtonResult.OK)
				{
					this.IsFilmEnabled = false;
					Task.Run(() =>
					{
						_printService.StartPrint(SelectedItem.StudyId);
						_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_PRINT));
					});
				}
			}, ConsoleSystemHelper.WindowHwnd);
		}
        AuditLogger.Log(new AuditLogInfo
        {
            CreateTime = DateTime.Now,
            EventType = "Action",
            EntryPoint = $"{this.GetType().Name}.{nameof(OnOpenPrintProcess)}",
            UserName = Environment.UserName,
            Description = "",
            OriginalValues = JsonConvert.SerializeObject(SelectedItem.StudyId),
            ReturnValues = JsonConvert.SerializeObject(null)
        });
    }

	/// <summary>
	/// 打开Viewer进程
	/// </summary>
	private void OnOpenViewer()
	{
		if (SelectedItem is null)
			return;
		if (!this.IsViewerEnabled)
		{
			return;
		}
		this.IsViewerEnabled = false;
		Task.Run(() =>
		{
			var studyId = SelectedItem.StudyId;
			_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_VIEWER));
			_viewer.StartViewer(studyId);
		});
		AuditLogger.Log(new AuditLogInfo
		{
			CreateTime = DateTime.Now,
			EventType = "Action",
			EntryPoint = $"{this.GetType().Name}.{nameof(OnOpenViewer)}",
			UserName = Environment.UserName,
			Description = "",
			OriginalValues = JsonConvert.SerializeObject(SelectedItem.StudyId),
			ReturnValues = JsonConvert.SerializeObject(null)
		});
	}

	/// <summary>
	/// 导入
	/// </summary>
	private void OnImport()
	{
		var dataImportWindow = Global.Instance.ServiceProvider.GetRequiredService<DataImportWindow>();
		if (dataImportWindow == null)
			return;

		var vieModel = dataImportWindow.DataContext as DataImportViewModel;
		if (vieModel == null)
			return;

		//在弹出窗体前刷新目录树
		vieModel.LoadAllDirectories();
		dataImportWindow.ShowWindowDialog();
	}

	private void OnImportRawdata()
	{
		var rawDataImportWindow = Global.Instance.ServiceProvider.GetRequiredService<RawDataImportWindow>();
		if (rawDataImportWindow == null)
			return;

		var vieModel = rawDataImportWindow.DataContext as RawDataImportViewModel;
		if (vieModel == null)
			return;

		//在弹出窗体前重置窗体信息
		vieModel.Reset();
		rawDataImportWindow.ShowWindowDialog();

	}

	private void OnSelectionChanged(object obj)
	{
		if (obj == null)
		{
			return;
		}
		HideCorrectWindow();
        this.SetPermissions();

		_selectedItems = (IList)obj;

		var seriesViewListViewModel = Global.Instance.ServiceProvider.GetService<SeriesViewModel>();
		if (seriesViewListViewModel == null)
		{
			return;
		}

		if (_selectedItems.Count > 1)
		{
			IsViewerEnabled = IsReconEnabled = IsFilmEnabled = false;
		}
		else if (_selectedItems.Count == 1)
		{
			if (seriesViewListViewModel.SeriesModels.Any(s => s.SeriesType == Constants.SERIES_TYPE_IMAGE))
			{
				IsViewerEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_IMAGE_VIEWER);
				IsFilmEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_PRINT);
			}
			else
			{
				IsViewerEnabled = IsFilmEnabled = false;
			}

			IsReconEnabled = IsStudyFinished(SelectedItem) && !IsStudyImported(SelectedItem) && AuthorizationHelper.ValidatePermission(SystemPermissionNames.EXAM_OFFLINE_RECON);//仅当扫描结束时，才可以进行重建
		}
		else
		{
			IsViewerEnabled = IsReconEnabled = IsFilmEnabled = false;
		}



		this.RefreshPermssions();
	}

	private void RefreshPermssions()
	{
		var seriesViewListViewModel = Global.Instance.ServiceProvider.GetService<SeriesViewModel>();
		if (seriesViewListViewModel == null)
		{
			return;
		}

		//refresh permissions of Archive and Export
		if (seriesViewListViewModel.SeriesModels.Count > 0)
		{
			this.IsPermissionArchiveEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_ARCHIVE) && CheckStudyIsFinished();
			this.IsPermissionExportEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_EXPORT) && CheckStudyIsFinished();
		}
		else
		{
			IsPermissionArchiveEnabled = false;
			IsPermissionExportEnabled = false;
		}

	}

	/// <summary>
	/// 恢复检查之后 刷新列表
	/// </summary>
	private void OnStudyServiceRefreshPatientManagementStudyList(object? sender,
																 EventArgs<(ApplicationService.Contract.Models.PatientModel,
																 ApplicationService.Contract.Models.StudyModel,
																 DataOperateType)> e)
	{
		var vStudyModel = _mapper.Map<VStudyModel>(e.Data.Item1);
		_mapper.Map(e.Data.Item2, vStudyModel);

		var result = this.LoadStudyPatientIfNotExist(vStudyModel);
		if (!result)
		{
			_logger.LogDebug($"Result of LoadStudyPatientIfNotExist is false.");
			return;
		}

		if (e.Data.Item3 == DataOperateType.SwitchLockStatus)
		{
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				var foundStudy = VStudies.FirstOrDefault(s => s.StudyId == vStudyModel.StudyId);
				if (foundStudy != null)
				{
					foundStudy.IsProtected = vStudyModel.IsProtected;
				}
			});
		}
		else if (e.Data.Item3 == DataOperateType.UpdateArchiveStatus)
		{
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				var foundStudy = VStudies.FirstOrDefault(s => s.StudyId == vStudyModel.StudyId);
				if (foundStudy != null)
				{
					foundStudy.ArchiveStatus = vStudyModel.ArchiveStatus;
				}
			});
		}
		else if (e.Data.Item3 == DataOperateType.UpdatePrintStatus)
		{
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				var foundStudy = VStudies.FirstOrDefault(s => s.StudyId == vStudyModel.StudyId);
				if (foundStudy != null)
				{
					foundStudy.PrintStatus = vStudyModel.PrintStatus;
				}
			});
		}
		else if (e.Data.Item3 == DataOperateType.Delete)
		{
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				var foundStudy = VStudies.FirstOrDefault(s => s.StudyId == vStudyModel.StudyId);
				if (foundStudy != null)
				{
					VStudies.Remove(foundStudy);
					SelectedItem = VStudies.FirstOrDefault();
				}

				this.SetPermissions();
				this.ResetButtonEnableStatus();
			});
		}
		else if (e.Data.Item3 == DataOperateType.UpdateStudyStatus)
		{
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				var foundStudy = VStudies.FirstOrDefault(s => s.StudyId == vStudyModel.StudyId);
				if (foundStudy != null)
				{
				   var studyModel=GetStudyModel(vStudyModel.StudyId);
                    foundStudy.StudyStatus = vStudyModel.StudyStatus;
					if (studyModel is not null)
					{
                        foundStudy.BodyPart = studyModel.BodyPart;
                    }
                }

				this.SetPermissions();
				this.ResetButtonEnableStatus();
			});
		}
	}
	private ApplicationService.Contract.Models.StudyModel GetStudyModel(string studyID)
	{
        var studyModels=  _studyApplicationService.GetStudiesByIds(new string[] { studyID });
		if (studyModels is not null)
		{
			return studyModels[0];

		}
		else
			return new ApplicationService.Contract.Models.StudyModel();
    }

	private bool LoadStudyPatientIfNotExist(VStudyModel vStudyModel)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			var foundStudy = VStudies.FirstOrDefault(s => s.StudyId == vStudyModel.StudyId);
			if (foundStudy == null)
			{
				var studyModels = _studyApplicationService.GetStudiesByIds(new string[] { vStudyModel.StudyId });
				var addedStudy = studyModels.FirstOrDefault();
				if (addedStudy is null)
				{
					_logger.LogDebug($"No study found with LastName:{vStudyModel.LastName}");
					return false;
				}
				if (addedStudy.StudyStatus == WorkflowStatus.NotStarted.ToString())
				{
					_logger.LogDebug($"Ignored beacuse study status is NotStarted");
					return false;
				}

				var addedPatient = _studyApplicationService.GetPatientModelById(addedStudy.InternalPatientId);
				var patientName = ParsePatientName(addedPatient.PatientName);
                addedPatient.FirstName= patientName.FirstName;
                addedPatient.LastName = patientName.LastName;

                foundStudy = _mapper.Map<VStudyModel>(addedPatient);
				_mapper.Map(addedStudy, foundStudy);

				VStudies.Insert(0, foundStudy);
			}
			return true;
		});

		return true;
	}
	private PatientName ParsePatientName(string patientName) 
	{
        string firstName = string.Empty;
        string lastName=string.Empty;
        if (patientName.Contains("^"))
        {
            string[] arr = patientName.Split('^');
            firstName = arr[0];
            lastName = arr[1];
        }
        else
        {
            lastName = patientName;
        }
        var  _patientName=new PatientName(firstName, lastName);
        return _patientName;
    }

	/// <summary>
	/// 根据Filter配置获取Study列表记录
	/// </summary>
	public void QueryStudyListByFilterConfig()
	{
		try
		{
			var filterConfig = this._filterConfigService.GetConfigs();
			var searchDateRange = this.GetSearchDateRange(filterConfig.StudyDateRange.DateRangeType, filterConfig.StudyDateRange.BeginDate, filterConfig.StudyDateRange.EndDate);
			var queryFilter = new StudyListFilterModel()
			{
				DateRangeBeginDate = searchDateRange.beginDate,
				DateRangeEndDate = searchDateRange.endDate,
				IsInProgressCheckedOfStudyStatus = filterConfig.StudyStatus.IsInProgressChecked,
				IsFinishedCheckedOfStudyStatus = filterConfig.StudyStatus.IsFinishedChecked,
				IsAbnormalCheckedOfStudyStatus = filterConfig.StudyStatus.IsAbnormalChecked,
				IsNotYetCheckedOfPrintStatus = filterConfig.PrintStatus.IsNotyetChecked,
				IsFinishedCheckedOfPrintStatus = filterConfig.PrintStatus.IsFinishedChecked,
				IsFailedCheckedOfPrintStatus = filterConfig.PrintStatus.IsFailedChecked,
				IsNotYetCheckedOfArchiveStatus = filterConfig.ArchiveStatus.IsNotyetChecked,
				IsFinishedCheckedOfArchiveStatus = filterConfig.ArchiveStatus.IsFinishedChecked,
				IsPartlyFinishedCheckedOfArchiveStatus = filterConfig.ArchiveStatus.IsPartlyFinishedChecked,
				IsFailedCheckedOfArchiveStatus = filterConfig.ArchiveStatus.IsFailedChecked,
				IsCorrectedChecked = filterConfig.CorrectionStatus.IsCorrected,
				IsUncorrectedChecked = filterConfig.CorrectionStatus.IsUncorrected,
				IsUnlockedChecked = filterConfig.LockStatus.IsUnlockedChecked,
				IsLockedChecked = filterConfig.LockStatus.IsLockedChecked,
				IsMaleChecked = filterConfig.Sex.IsMaleChecked,
				IsFemaleChecked = filterConfig.Sex.IsFemaleChecked,
				IsOtherChecked = filterConfig.Sex.IsOtherChecked,
				IsLocalChecked = filterConfig.PatientType.IsLocalChecked,
				IsPreRegChecked = filterConfig.PatientType.IsPreRegChecked,
				IsEmergencyChecked = filterConfig.PatientType.IsEmergencyChecked,
				BirthdayRangeBeginDate = ConvertStringToDateTime(filterConfig.BirthdayDateRange.BeginDate),
				BirthdayRangeEndDate = ConvertStringToDateTime(filterConfig.BirthdayDateRange.EndDate),
				SortedColumnName = filterConfig.SortedColumn.ColumnName,
				IsAscendingSort = filterConfig.SortedColumn.IsAscending,
			};

			var result = _studyApplicationService.GetPatientStudyListByFilter(queryFilter);

			var list = result.Select(r =>
			{
				var firstName = string.Empty;
				string lastName;
				if (r.Item1.PatientName.Contains("^"))
				{
					string[] arr = r.Item1.PatientName.Split('^');
					firstName = arr[0];
					lastName = arr[1];
				}
				else
				{
					lastName = r.Item1.PatientName;
				}

				int bodyPartKey = -1;
				if (Enum.TryParse<BodyPart>(r.Item2.BodyPart, true, out var bodyPart))
				{
					bodyPartKey = (int)bodyPart;
				}

				return new VStudyModel
				{
					PatientName = r.Item1.PatientName,
					Age = r.Item2.Age,
					AgeType = (AgeType)r.Item2.AgeType,
					Weight = r.Item2.PatientWeight,
					Height = r.Item2.PatientSize,
					Pid = r.Item1.Id,
					StudyId = r.Item2.Id,
					PatientId = r.Item1.PatientId,
					LastName = lastName,
					FirstName = firstName,
					Gender = ((int)r.Item1.PatientSex),
					CreateTime = r.Item1.CreateTime,
					AdmittingDiagnosis = r.Item2.AdmittingDiagnosisDescription,
					Ward = r.Item2.Ward,
					BodyPart = r.Item2.BodyPart,
					BodyPartKey = bodyPartKey,
					HisStudyId = r.Item2.StudyId,
					AccessionNo = r.Item2.AccessionNo,
					Comments = r.Item2.Comments,
					InstitutionName = r.Item2.InstitutionName,
					PatientType = r.Item2.PatientType,
					InstitutionAddress = r.Item2.InstitutionAddress,
					Birthday = r.Item1.PatientBirthDate,
					StudyDescription = r.Item2.StudyDescription,
					StudyStatus = r.Item2.StudyStatus,
					IsProtected = r.Item2.IsProtected,
					ArchiveStatus = (JobTaskStatus)r.Item2.ArchiveStatus,
					PrintStatus = (JobTaskStatus)r.Item2.PrintStatus,
					CorrectStatus = r.Item2.CorrectStatus,
					StudyDate = r.Item2.StudyDate,
					StudyTime = r.Item2.StudyTime,
					ReferringPhysician = r.Item2.ReferringPhysicianName,
					ExamStartTime = r.Item2.ExamStartTime,
					ExamEndTime = r.Item2.ExamEndTime,
					StudyCreateTime = r.Item2.CreateTime,
				};
			}).ToList();
			_gVStudies = list.ToObservableCollection();
			VStudies = _gVStudies;
			SelectedItem = _gVStudies.FirstOrDefault();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Failed in QueryStudyListByFilterConfig with error : {ex.Message}");
		}
	}

	private (DateTime beginDate, DateTime endDate) GetSearchDateRange(SearchTimeType searchTimeType, string customBeginDate, string customEndDate)
	{
		var beginDateValue = DateTime.Now;
		var endDateValue = DateTime.Now;

		switch (searchTimeType)
		{
			case SearchTimeType.Today:
				beginDateValue = DateTime.Now;
				endDateValue = DateTime.Now;
				LocalDataDescription = $"{LanguageResource.Content_LocalDataFor} {LanguageResource.Content_Today} ( {beginDateValue.ToString(DATE_FORMAT_STRING)} )";
				break;
			case SearchTimeType.Yesterday:
				beginDateValue = DateTime.Now.AddDays(-1);
				endDateValue = DateTime.Now.AddDays(-1);
				LocalDataDescription = $"{LanguageResource.Content_LocalDataFor} {LanguageResource.Content_Yesterday} ( {beginDateValue.ToString(DATE_FORMAT_STRING)} )";
				break;
			case SearchTimeType.DayBeforeYesterday:
				beginDateValue = DateTime.Now.AddDays(-2);
				endDateValue = DateTime.Now.AddDays(-2);
				LocalDataDescription = $"{LanguageResource.Content_LocalDataFor} {LanguageResource.Content_DayBeforeYesterday} ( {beginDateValue.ToString(DATE_FORMAT_STRING)} )";
				break;
			case SearchTimeType.Last7Days:
				beginDateValue = DateTime.Now.AddDays(-7);
				endDateValue = DateTime.Now;
				LocalDataDescription = $"{LanguageResource.Content_LocalDataFor} {LanguageResource.Content_WithinTheLast7Days} ( {beginDateValue.ToString(DATE_FORMAT_STRING)}----{endDateValue.ToString(DATE_FORMAT_STRING)} )";
				break;
			case SearchTimeType.Last30Days:
				beginDateValue = DateTime.Now.AddDays(-30);
				endDateValue = DateTime.Now;
				LocalDataDescription = $"{LanguageResource.Content_LocalDataFor} {LanguageResource.Content_WithinTheLast30Days} ( {beginDateValue.ToString(DATE_FORMAT_STRING)}----{endDateValue.ToString(DATE_FORMAT_STRING)} )";
				break;
			case SearchTimeType.All:
				beginDateValue = DateTime.MinValue;
				endDateValue = DateTime.Now;
				LocalDataDescription = $"{LanguageResource.Content_LocalDataFor} {LanguageResource.Content_All}";
				break;
			case SearchTimeType.Custom:
				DateTime.TryParse(customBeginDate, out beginDateValue);
				DateTime.TryParse(customEndDate, out endDateValue);

				string customBeginDateFormat = beginDateValue.ToString(DATE_FORMAT_STRING);
				if (string.IsNullOrEmpty(customBeginDate) || beginDateValue == DateTime.MinValue)
				{
					beginDateValue = DateTime.MinValue;
					customBeginDateFormat = DATE_UNLIMITED_STRING;
				}

				string customEndDateFormat = endDateValue.ToString(DATE_FORMAT_STRING);
				if (string.IsNullOrEmpty(customEndDate) || endDateValue == DateTime.MinValue)
				{
					endDateValue = DateTime.Today;
					customEndDateFormat = DATE_UNLIMITED_STRING;
				}

				LocalDataDescription = $"{LanguageResource.Content_LocalDataFor} {LanguageResource.Content_Custom} ( {customBeginDateFormat}----{customEndDateFormat} )";
				break;
			default:
				DateTime.TryParse(customBeginDate, out beginDateValue);
				DateTime.TryParse(customEndDate, out endDateValue);
				LocalDataDescription = $"{LanguageResource.Content_LocalDataFor} {LanguageResource.Content_Custom} ( {beginDateValue.ToString(DATE_FORMAT_STRING)}----{endDateValue.ToString(DATE_FORMAT_STRING)} )";
				break;
		}

		return new(beginDateValue, endDateValue);
	}

	/// <summary>
	/// 刷新WorkList
	/// </summary>
	private void OnRefreshWorkList()
	{
		this.QueryStudyListByFilterConfig();
	}

	private void OnSwitchLockedStatus(string studyId)
	{
		if (string.IsNullOrEmpty(studyId))
			return;

		_studyApplicationService.SwitchLockStatus(studyId);
	}

	private void OnSearch(string keyword)
	{
		if (string.IsNullOrEmpty(keyword))
		{
			VStudies = _gVStudies;
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
		VStudies = new ObservableCollection<VStudyModel>(_gVStudies.Where(func).ToList());
	}

	private void OnDelete()
	{
		if (SelectedItem == null)
		{
			_dialogService.ShowDialog(false,
									  MessageLeveles.Info,
									  LanguageResource.Message_Info_CloseInformationTitle,
									  LanguageResource.Message_Info_MustSelectAStudy,
									  null,
									  ConsoleSystemHelper.WindowHwnd);
			return;
		}

		if (SelectedItem.IsProtected)
		{
			_dialogService.ShowDialog(false,
									  MessageLeveles.Info,
									  LanguageResource.Message_Info_CloseInformationTitle,
									  LanguageResource.Message_Info_Deletion_Protected,
									  null,
									  ConsoleSystemHelper.WindowHwnd);
			return;
		}

		_dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, LanguageResource.Message_Confirm_Delete, arg =>
					{
						if (arg.Result == ButtonResult.OK)
						{
							_studyApplicationService.Delete(_mapper.Map<ApplicationService.Contract.Models.StudyModel>(SelectedItem));
						}
					}, ConsoleSystemHelper.WindowHwnd);
	}

	/// <summary>
	/// 导出弹窗
	/// </summary>
	public void OnOpenExportWindow()
	{
		if (SelectedItem == null)
		{
			this._logger.LogWarning("No selected study in OnOpenExportWindow.");
			return;
		}

		var exportWindow = Global.Instance.ServiceProvider.GetRequiredService<ExportWindow>();
		var exportViewModel = exportWindow.DataContext as ExportWindowViewModel;

		var callback = new Action<ExportSelectionModel>(exportSelectionModel =>
		{
			this.BeginExport(exportSelectionModel);
		});
		exportViewModel?.Show(callback);
		exportWindow.ShowWindowDialog();

	}

	/// <summary>
	/// 归档弹窗
	/// </summary>
	private void OnOpenFileArchiveWindow()
	{
		if (SelectedItem is null)
		{
			this._logger.LogWarning("No selected study in OnOpenFileArchiveWindow.");
			return;
		}

		var archiveWindow = Global.Instance.ServiceProvider.GetRequiredService<ArchiveWindow>();
		if (archiveWindow is null)
		{
			this._logger.LogWarning("archiveWindow is null in OnOpenFileArchiveWindow.");
			return;
		}
		var archiveViewModel = archiveWindow.DataContext as ArchiveWindowViewModel;

		var callback = new Action<ArchiveModel,bool,bool>((archiveModel,useTls,anonymous )=>
		{
			this.BeginArchive(archiveModel, useTls, anonymous);
		});
		archiveViewModel?.Show(callback);

		archiveWindow.ShowWindowDialog();
	}

	/// <summary>
	/// 恢复检查
	/// </summary>
	private void OnResumeExamination()
	{
		if (SelectedItem == null)
			return;

		var studyModel = _studyApplicationService.GetStudyModelByPatientIdAndStudyStatus(SelectedItem.Pid, WorkflowStatus.NotStarted.ToString());
		if (studyModel != null)
		{
			try
			{
				_dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_CloseInformationTitle, LanguageResource.Message_Info_PatientAlreadyExists, _ => { }
					, ConsoleSystemHelper.WindowHwnd);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"resume error : {ex.Message}");
			}

			return;
		}

		_dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, LanguageResource.Message_Confirm_ResumeExamination, arg =>
			{
				if (arg.Result == ButtonResult.OK)
				{
					var newStudy = new NV.CT.PatientManagement.Models.StudyModel();
					newStudy.Id = Guid.NewGuid().ToString();
					newStudy.StudyInstanceUID = UIDHelper.CreateStudyInstanceUID();
					newStudy.InternalPatientId = SelectedItem.Pid;
					newStudy.StudyStatus = WorkflowStatus.NotStarted.ToString();
					// newStudy.LoginID =
					newStudy.Height = SelectedItem.Height;
					newStudy.Weight = SelectedItem.Weight;
					newStudy.AdmittingDiagnosis = SelectedItem.AdmittingDiagnosis;
					newStudy.Ward = SelectedItem.Ward;
					newStudy.ReferringPhysician = SelectedItem.ReferringPhysician;
					newStudy.BodyPart = SelectedItem.BodyPart;
					newStudy.AccessionNo = SelectedItem.AccessionNo;
					newStudy.Comments = SelectedItem.Comments;
					newStudy.InstitutionName = SelectedItem.InstitutionName;
					newStudy.InstitutionAddress = SelectedItem.InstitutionAddress;
					newStudy.Age = SelectedItem.Age;
					newStudy.AgeType = (int)SelectedItem.AgeType;
					newStudy.PatientType = SelectedItem.PatientType.HasValue ? SelectedItem.PatientType.Value : (int)PatientType.Local;
					newStudy.StudyDescription = SelectedItem.StudyDescription;
					newStudy.StudyId = UIDHelper.CreateStudyID();
					newStudy.StudyId_Dicom = newStudy.StudyId;
                    _studyApplicationService.ResumeExamination(
						_mapper.Map<ApplicationService.Contract.Models.StudyModel>(newStudy), SelectedItem.StudyId);
				}
			}, ConsoleSystemHelper.WindowHwnd);
	}

	private void OnCorrect()
	{
		if (SelectedItem == null)
		{
			return;
		}

		var win = Global.Instance.ServiceProvider.GetRequiredService<CorrectWindow>();
		if (win is null)
			return;

		var viewModel = win.DataContext as CorrectViewModel;
		if (viewModel is null)
			return;

		viewModel.SetSelectedStudy(SelectedItem);
		win.ShowWindowDialog();

    }
    private void HideCorrectWindow()
    {
        if (SelectedItem == null)
        {
            return;
        }
        var win = Global.Instance.ServiceProvider.GetRequiredService<CorrectWindow>();
        if (win is null)
            return;
        win.Hide();
    }

    private void OnRawDataManage()
	{
		if (SelectedItem == null)
		{
			return;
		}

		var win = Global.Instance.ServiceProvider.GetRequiredService<RawDataManagementWindow>();
		if (win is null)
			return;

		var viewModel = win.DataContext as RawDataManagementViewModel;
		if (viewModel is null)
			return;
        var callback = new Action<RawDataExport>(rawDataExport =>
        {
            this.AddRawDataExportTask(rawDataExport);
        });
        viewModel.SetSelectedStudy(SelectedItem, callback);
		win.ShowWindowDialog();
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
				_filtrationWindow.Left = Math.Abs(positionOfButton.X) - _filtrationWindow.Width;
				_filtrationWindow.Top = Math.Abs(positionOfButton.Y) + 40;
			}
		}
		var viewModel = _filtrationWindow.DataContext as StudyListFilterViewModel;
		viewModel.Initialize();
		_filtrationWindow.ShowWindowDialog(true);
	}

	private void OnColumnsConfigWindowDialog(object element)
	{

		if (_columnsConfigWindow is null)
		{
			_columnsConfigWindow = new();
			_columnsConfigWindow.WindowStartupLocation = WindowStartupLocation.Manual;
			var button = element as Button;
			if (button != null)
			{
				var positionOfButton = button.PointFromScreen(new Point(0, 0));
				_columnsConfigWindow.Left = Math.Abs(positionOfButton.X) - _columnsConfigWindow.Width;
				_columnsConfigWindow.Top = Math.Abs(positionOfButton.Y) + 40;
			}
		}

		var studyListColumnsConfigViewModel = Global.Instance.ServiceProvider.GetRequiredService<StudyListColumnsConfigViewModel>();
		studyListColumnsConfigViewModel.Initialize();

		_columnsConfigWindow.ShowWindowDialog(true);
	}

	private void OnCurrentUserChanged(object? sender, CT.Models.UserModel e)
	{
		this.CurrentUserAccount = e is null ? string.Empty : e.Account;
		this.SetPermissions();
		this.ResetButtonEnableStatus();
		this.RefreshPermssions();
	}

	private void OnFilterConfigRefreshed(object? sender, EventArgs e)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			this.QueryStudyListByFilterConfig();
		});
	}

	#endregion

	#region Private methods
	private DateTime? GetBirthday(int ageType, int age)
	{
		switch (ageType)
		{
			case (int)AgeType.Year:
				return DateTime.Now.AddYears(-age);
			case (int)AgeType.Month:
				return DateTime.Now.AddMonths(-age);
			case (int)AgeType.Week:
				return DateTime.Now.AddDays(-age * 7);
			case (int)AgeType.Day:
				return DateTime.Now.AddDays(-age);
			default:
				return default;
		}
	}

	private bool IsStudyFinished(VStudyModel studyModel)
	{
		var StudyStatus = Enum.Parse<WorkflowStatus>(studyModel.StudyStatus);
		if (StudyStatus == WorkflowStatus.ExaminationClosed || StudyStatus == WorkflowStatus.ExaminationClosing || StudyStatus == WorkflowStatus.ExaminationDiscontinue)
			return true;
		return false;
	}
    private bool IsStudyImported(VStudyModel studyModel)
	{
        var scanTasks = _scanService.GetAll(studyModel.StudyId);
		if (scanTasks.Count==0&& studyModel.PatientType!=0)
		{
			return true;
		}else
		{
            return false;
        }
	}


    private void SetPermissions()
	{
		this.IsPermissionDeletionEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_DATA_DELETION) && CheckStudyIsFinished();
		this.IsPermissionCorrectionEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_CORRECT_INFO) && CheckStudyIsFinished();
		this.IsPermissionExportRawEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_EXPORT_RAW) && CheckStudyIsFinished();
		this.IsPermissionArchiveEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_ARCHIVE) && CheckStudyIsFinished();
		this.IsPermissionExportEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_EXPORT) && CheckStudyIsFinished();

		this.IsPermissionImportEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_IMPORT);
		this.IsPermissionProtectionEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_DATA_PROTECTION);
	}

	private void BeginArchive(ArchiveModel archiveConfigModel,bool useTls,bool anonymous)
	{
		if (archiveConfigModel is null)
		{
			this._logger.LogWarning("Parameter archiveConfigModel of BeginArchive is null.");
			return;
		}
		var seriesModelList=_seriesService.GetSeriesByStudyId(SelectedItem.StudyId).OrderBy(r => r.SeriesNumber);
        var targetSeriesIdList = seriesModelList.Select(s => s.Id).ToList();
		var rtdSeriesId = _seriesService.GetSeriesIdByStudyId(SelectedItem.StudyId);
		targetSeriesIdList.Remove(rtdSeriesId);
        //如果没有任何可导入的序列，则提示并返回
        if (targetSeriesIdList is null || targetSeriesIdList.Count == 0)
		{
			_dialogService.ShowDialog(false, MessageLeveles.Error,
									  LanguageResource.Message_Info_CloseInformationTitle,
									  LanguageResource.Message_Error_CannotArchiveNoAnySeries,
									  null, ConsoleSystemHelper.WindowHwnd);

			return;
		}
		Task.Run(() => {
            foreach (var targetSeriesId in targetSeriesIdList)
            {
                AddArchiveTask(SelectedItem.Pid, SelectedItem.StudyId, new List<string> { targetSeriesId }, ArchiveLevel.Series, archiveConfigModel, useTls, anonymous);
				Thread.Sleep(PatientManagementConstants.THREAD_SLEEP_TIME);
            }
        });
	}

	private void AddArchiveTask(string patientId, string studyId, List<string> targetSeriesIdList, ArchiveLevel archiveLevel, ArchiveModel archiveConfigModel, bool useTls, bool anonymous)
	{
		var archiveJobTask = new ArchiveJobRequest();
		archiveJobTask.Id = Guid.NewGuid().ToString();
		archiveJobTask.WorkflowId = Guid.NewGuid().ToString();
		archiveJobTask.InternalPatientID = patientId;
		archiveJobTask.InternalStudyID = studyId;
		archiveJobTask.Priority = 5;
		archiveJobTask.JobTaskType = JobTaskType.ArchiveJob;
		archiveJobTask.Creator = _authorizationService.GetCurrentUser() is null ? string.Empty : _authorizationService.GetCurrentUser().Account;
		archiveJobTask.ArchiveLevel = archiveLevel;
		archiveJobTask.StudyId = studyId;
		archiveJobTask.SeriesIdList = targetSeriesIdList;
		archiveJobTask.Host = archiveConfigModel.Host;
		archiveJobTask.Port = int.Parse(archiveConfigModel.Port);
		archiveJobTask.AECaller = archiveConfigModel.AECaller;
		archiveJobTask.AETitle = archiveConfigModel.AETitle;
		archiveJobTask.DicomTransferSyntax = archiveConfigModel.TransferSyntax;
		archiveJobTask.UseTls = useTls;
		archiveJobTask.Anonymous = anonymous;

        archiveJobTask.Parameter = archiveJobTask.ToJson();

        var result = this._jobRequestService.EnqueueJobRequest(archiveJobTask);
        if (result.Status == CommandExecutionStatus.Success)
        {
			//_dialogService.ShowDialog(false, MessageLeveles.Info,
			//						  LanguageResource.Message_Info_CloseInformationTitle,
			//						  LanguageResource.Message_Info_ArchiveTaskStarted,
			//						  null, ConsoleSystemHelper.WindowHwnd);
		}
        else
        {
			//_dialogService.ShowDialog(false, MessageLeveles.Error,
			//						  LanguageResource.Message_Info_CloseInformationTitle,
			//						  LanguageResource.Message_Error_ArchiveTaskFailed,
			//						  null, ConsoleSystemHelper.WindowHwnd);
		}
    }

	private void BeginExport(ExportSelectionModel exportSelectionModel)
	{
		if (exportSelectionModel is null)
		{
			this._logger.LogWarning("Parameter exportSelectionModel of BeginExport is null.");
			return;
		}

		var targetSeriesList = _seriesService.GetSeriesByStudyId(SelectedItem.StudyId).ToList();
		//如果没有任何可导入的序列，则提示并返回
		if (targetSeriesList is null || targetSeriesList.Count == 0)
		{
			_dialogService.ShowDialog(false, MessageLeveles.Error,
									  LanguageResource.Message_Info_CloseInformationTitle,
									  LanguageResource.Message_Error_NoSelectedSeries,
									  null, ConsoleSystemHelper.WindowHwnd);
			return;
		}

		this.AddExportTask(SelectedItem.StudyId, targetSeriesList, exportSelectionModel);
	}

	private void AddExportTask(string studyId, List<NV.CT.DatabaseService.Contract.Models.SeriesModel> targetSeriesList, ExportSelectionModel exportSelectionModel)
	{
		var exportJobRequest = new ExportJobRequest();

		foreach (var selectedSeries in targetSeriesList)
		{
			exportJobRequest.InputFolders.Add(selectedSeries.SeriesPath);
		}
		exportJobRequest.PatientNames = FetchPatientNameList(new string[] { studyId });
		exportJobRequest.IsAnonymouse = exportSelectionModel.IsAnonymouse;
		exportJobRequest.IsExportedToDICOM = exportSelectionModel.IsExportedToDICOM;
		exportJobRequest.IsExportedToImage = exportSelectionModel.IsExportedToImage;
		exportJobRequest.OutputVirtualPath = exportSelectionModel.OutputVirtualPath;
		exportJobRequest.OutputFolder = exportSelectionModel.OutputFolder;
		exportJobRequest.IsBurnToCDROM = exportSelectionModel.IsBurnToCDROM;
		exportJobRequest.IsAddViewer = exportSelectionModel.IsAddViewer;
		exportJobRequest.DicomTransferSyntax = exportSelectionModel.DicomTransferSyntax;
		exportJobRequest.PictureType = exportSelectionModel.PictureType;

		exportJobRequest.Id = Guid.NewGuid().ToString();
		exportJobRequest.WorkflowId = Guid.NewGuid().ToString();
		exportJobRequest.InternalPatientID = string.Empty;
		exportJobRequest.InternalStudyID = string.Empty;
		exportJobRequest.Priority = 5;
		exportJobRequest.JobTaskType = JobTaskType.ExportJob;
		exportJobRequest.Creator = _authorizationService.GetCurrentUser() is null ? string.Empty : _authorizationService.GetCurrentUser().Account;
		exportJobRequest.OperationLevel = OperationLevel.Study;
		exportJobRequest.StudyId = studyId;
		exportJobRequest.SeriesIdList = targetSeriesList.Select(s => s.Id).ToList();
		exportJobRequest.Parameter = exportJobRequest.ToJson();

        var result = this._jobRequestService.EnqueueJobRequest(exportJobRequest);
        if (result.Status == CommandExecutionStatus.Success)
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Info_ExportingTaskStarted,
                                      null, ConsoleSystemHelper.WindowHwnd);
        }
        else
        {
            _dialogService.ShowDialog(false, MessageLeveles.Error,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Error_ExportTaskFailed,
                                      null, ConsoleSystemHelper.WindowHwnd);
        }
    }

	private void AddRawDataExportTask(RawDataExport rawDataExport)
	{
        var exportJobRequest = new ExportJobRequest();
		foreach (var selectedRawdata in rawDataExport.RawDataModels)
		{
			exportJobRequest.InputFolders.Add(selectedRawdata.Path);
			exportJobRequest.SeriesIdList.Add(selectedRawdata.Id);
		}
		exportJobRequest.RTDDicomFolders.AddRange(rawDataExport.RTDSeriesPathList);
        exportJobRequest.PatientNames = FetchPatientNameList(new string[] { rawDataExport.StudyID});
        exportJobRequest.IsAnonymouse = false;
        exportJobRequest.IsExportedToDICOM = false;
        exportJobRequest.IsExportedToImage = false;
		exportJobRequest.IsExportedToRawData = true;
        exportJobRequest.OutputVirtualPath = rawDataExport.OutputDir;
        exportJobRequest.OutputFolder = rawDataExport.OutputDir;
		exportJobRequest.IsBurnToCDROM = false;
        exportJobRequest.IsAddViewer = false;
        exportJobRequest.DicomTransferSyntax = string.Empty;
        exportJobRequest.PictureType = null;

        exportJobRequest.Id = Guid.NewGuid().ToString();
        exportJobRequest.WorkflowId = Guid.NewGuid().ToString();
        exportJobRequest.InternalPatientID = string.Empty;
        exportJobRequest.InternalStudyID = string.Empty;
        exportJobRequest.Priority = 5;
        exportJobRequest.JobTaskType = JobTaskType.ExportJob;
        exportJobRequest.Creator = _authorizationService.GetCurrentUser() is null ? string.Empty : _authorizationService.GetCurrentUser().Account;
        exportJobRequest.OperationLevel = OperationLevel.Study;
        exportJobRequest.StudyId = rawDataExport.StudyID;
        exportJobRequest.Parameter = exportJobRequest.ToJson();

        var result = this._jobRequestService.EnqueueJobRequest(exportJobRequest);
        if (result.Status == CommandExecutionStatus.Success)
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Info_ExportingTaskStarted,
                                      null, ConsoleSystemHelper.WindowHwnd);
        }
        else
        {
            _dialogService.ShowDialog(false, MessageLeveles.Error,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Error_ExportTaskFailed,
                                      null, ConsoleSystemHelper.WindowHwnd);
        }
    }

	private List<string> FetchPatientNameList(string[] studyIds)
	{
		var patientNameList = this.VStudies.Where(s => studyIds.Contains(s.StudyId)).Select(s => s.PatientName).ToList();
		return patientNameList;
	}

	#endregion

	#region Control Enable

	private bool CheckStudyIsFinished()
	{
		if (SelectedItem is null)
			return false;
		return IsStudyFinished(SelectedItem);
	}

	public bool IsRawDataEnable()
	{
		return CheckStudyIsFinished();
	}
	public bool IsViewReportEnable()
	{
		return CheckStudyIsFinished();
	}
	public bool IsDoseOperationEnable()
	{
		return CheckStudyIsFinished();
	}


	private void ResetButtonEnableStatus()
	{
		if (SelectedItem is null)
		{
			IsViewerEnabled = IsReconEnabled = IsFilmEnabled = false;
			return;
		}

		var seriesViewListViewModel = Global.Instance.ServiceProvider.GetService<SeriesViewModel>();
		if (seriesViewListViewModel is null)
		{
			IsViewerEnabled = IsReconEnabled = IsFilmEnabled = false;
			return;
		}

		if (seriesViewListViewModel.SeriesModels.Any(s => s.SeriesType == Constants.SERIES_TYPE_IMAGE))
		{
			IsViewerEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_IMAGE_VIEWER);
			IsFilmEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_PRINT);
			//IsReconEnabled = IsStudyFinished(SelectedItem) && AuthorizationHelper.ValidatePermission(SystemPermissionNames.EXAM_OFFLINE_RECON); //仅当扫描结束时，才可以进行重建
		}
		else
		{
			IsViewerEnabled = IsFilmEnabled = false;
		}

		//考虑导入生数据场景，允许没有序列时也能打开离线重建
		IsReconEnabled = IsStudyFinished(SelectedItem) && AuthorizationHelper.ValidatePermission(SystemPermissionNames.EXAM_OFFLINE_RECON); //仅当扫描结束时，才可以进行重建
	}

	private DateTime? ConvertStringToDateTime(string dateTime)
	{
		if (string.IsNullOrEmpty(dateTime))
		{
			return null;
		}

		DateTime convertedTime;
		if (DateTime.TryParse(dateTime, out convertedTime))
		{
			return convertedTime;
		}
		else
		{
			return null;
		}
	}

	#endregion
}
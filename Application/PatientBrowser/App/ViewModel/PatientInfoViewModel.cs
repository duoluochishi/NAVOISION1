//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.AppService.Contract;
using NV.CT.ClientProxy.Application;
using NV.CT.ClientProxy.DataService;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.DatabaseService.Contract;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.Language;
using NV.CT.Logging;
using NV.CT.PatientBrowser.ApplicationService.Contract.Interfaces;
using NV.CT.PatientBrowser.Models;
using NV.CT.SyncService.Contract;
using NV.CT.UI.ViewModel;
using NV.CT.WorkflowService.Contract;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NV.CT.PatientBrowser.ViewModel;

public class PatientInfoViewModel : BaseViewModel
{
	private readonly IWorkflow _workflow;
	private readonly IApplicationCommunicationService _applicationCommunicationService;
	private readonly IStudyApplicationService _studyApplicationService;
	private readonly IMapper _mapper;
	private readonly ILogger<PatientInfoViewModel> _logger;
	private readonly IDialogService _dialogService;
	private readonly IPatientConfigService _patientConfigService;
	private readonly IDataSync _dataSync;
	private readonly IStudyService _studyService;
	private readonly IAuthorization _authorizationService;
	private readonly IPatientService _patientService;
	private readonly PatientConfig _patientConfig;
	private bool _saveButtonStatus = false;
	private bool _examButtonStatus = false;
	private bool _emergencyButtonStatus = false;
	private bool _patientInfoFormIsEnabled = true;
	private bool _needStartExam = false;
    private WorklistInfo _worklist;
    private readonly IJobRequestService _jobRequestService;
    //private CT.Models.UserModel _previousUser; //记录前用户信息，在切换用户事件中进行判断，如果是null，则识别为首次登录。

    #region Properties

    private List<AgeType> _ageTypes = new List<AgeType>();
	public List<AgeType> AgeTypes
	{
		get => _ageTypes;
		set => SetProperty(ref _ageTypes, value);
	}

	private Dictionary<int, string> _genders = new Dictionary<int, string>();
	public Dictionary<int, string> Genders
	{
		get => _genders;
		set => SetProperty(ref _genders, value);
	}

	private VStudyModel _selectedVStudyEntity = new VStudyModel();
	public VStudyModel SelectedVStudyEntity
	{
		get => _selectedVStudyEntity;
		set => SetProperty(ref _selectedVStudyEntity, value);
	}
	public bool PatientInfoFormIsEnabled
	{
		get => _patientInfoFormIsEnabled;
		set => SetProperty(ref _patientInfoFormIsEnabled, value);
	}

	private bool _isEnabledFirstName = true;
	public bool IsEnabledFirstName
	{
		get => _isEnabledFirstName;
		set => SetProperty(ref _isEnabledFirstName, value);
	}

	private bool _isEnabledSex = true;
	public bool IsEnabledSex
	{
		get => _isEnabledSex;
		set => SetProperty(ref _isEnabledSex, value);
	}

	private bool _isEnabledHeight = true;
	public bool IsEnabledHeight
	{
		get => _isEnabledHeight;
		set => SetProperty(ref _isEnabledHeight, value);
	}

	private bool _isEnabledWeight = true;
	public bool IsEnabledWeight
	{
		get => _isEnabledWeight;
		set => SetProperty(ref _isEnabledWeight, value);
	}

	private bool _isEnabledAdmittingDiagnosis = true;
	public bool IsEnabledAdmittingDiagnosis
	{
		get => _isEnabledAdmittingDiagnosis;
		set => SetProperty(ref _isEnabledAdmittingDiagnosis, value);
	}

	private bool _isEnabledWard = true;
	public bool IsEnabledWard
	{
		get => _isEnabledWard;
		set => SetProperty(ref _isEnabledWard, value);
	}

	private bool _isEnabledDescription = true;
	public bool IsEnabledDescription
	{
		get => _isEnabledDescription;
		set => SetProperty(ref _isEnabledDescription, value);
	}

	private bool _isEnabledAccessionNo = true;
	public bool IsEnabledAccessionNo
	{
		get => _isEnabledAccessionNo;
		set => SetProperty(ref _isEnabledAccessionNo, value);
	}

	private bool _isEnabledHisStudyID = true;
	public bool IsEnabledHisStudyID
	{
		get => _isEnabledHisStudyID;
		set => SetProperty(ref _isEnabledHisStudyID, value);
	}

	private bool _isEnabledComments = true;
	public bool IsEnabledComments
	{
		get => _isEnabledComments;
		set => SetProperty(ref _isEnabledComments, value);
	}

	private bool _isEnabledInstitutionName = true;
	public bool IsEnabledInstitutionName
	{
		get => _isEnabledInstitutionName;
		set => SetProperty(ref _isEnabledInstitutionName, value);
	}

	private bool _isEnabledInstitutionAddress = true;
	public bool IsEnabledInstitutionAddress
	{
		get => _isEnabledInstitutionAddress;
		set => SetProperty(ref _isEnabledInstitutionAddress, value);
	}

	private bool _isEnabledReferringPhysician = true;
	public bool IsEnabledReferringPhysician
	{
		get => _isEnabledReferringPhysician;
		set => SetProperty(ref _isEnabledReferringPhysician, value);
	}

	private bool _isEnabledPerformingPhysician = true;
	public bool IsEnabledPerformingPhysician
	{
		get => _isEnabledPerformingPhysician;
		set => SetProperty(ref _isEnabledPerformingPhysician, value);
	}


	private bool _isPermissionEmergencyEnabled = true;
	public bool IsPermissionEmergencyEnabled
	{
		get => _isPermissionEmergencyEnabled;
		set => SetProperty(ref _isPermissionEmergencyEnabled, value);
	}

	private bool _isPermissionExamEnabled = true;
	public bool IsPermissionExamEnabled
	{
		get => _isPermissionExamEnabled;
		set => SetProperty(ref _isPermissionExamEnabled, value);
	}


	#endregion

	public bool IsDevEnvironment
	{
		get => RuntimeConfig.IsDevelopment;
	}
	public PatientInfoViewModel(IStudyApplicationService studyApplicationService,
								IMapper mapper,
								IDialogService dialogSerwice,
								ILogger<PatientInfoViewModel> logger,
								IWorkflow workflow,
								IApplicationCommunicationService applicationCommunicationService,
								IPatientConfigService patientConfigService,
								IDataSync dataSync,
								IStudyService studyService,
								IAuthorization authorizationService,
								IPatientService patientService,
                                IJobRequestService jobRequestService)
	{
		_dataSync = dataSync;
		_workflow = workflow;
		_applicationCommunicationService = applicationCommunicationService;
		_studyApplicationService = studyApplicationService;
		_mapper = mapper;
		_dialogService = dialogSerwice;
		_patientConfigService = patientConfigService;
		_logger = logger;
		_studyService = studyService;
		_authorizationService = authorizationService;
		_patientService = patientService;
        _jobRequestService = jobRequestService;
        _patientConfig = _patientConfigService.GetConfigs();
		Initialize();
		InitPatientPage(_patientConfig.DisplayItems.Items);

		_studyApplicationService.SelectItemChanged += OnSelectItemChanged;
		_workflow.WorkflowStatusChanged += OnWorkflowStatusChanged;
		_dataSync.NormalExamStarted += OnDataSyncStartExamStarted;
		_dataSync.EmergencyExamStarted += OnDataSyncStartEmergencyExamOccured;
		_applicationCommunicationService.ApplicationStatusChanged += OnApplicationStatusChanged;
		_workflow.EmergencyExamStarted += Workflow_EmergencyExamStarted;
		_authorizationService.CurrentUserChanged += OnCurrentUserChanged;
	}

	/// <summary>
	/// MCS通知RGT客户端开启检查完毕
	/// </summary>
	private void NotifyNormalExam()
	{
		_dataSync.NotifyNormalExam();
	}

	/// <summary>
	/// MCS通知RGT客户端急诊完毕
	/// </summary>
	private void NotifyEmergencyExam()
	{
		_dataSync.NotifyEmergencyExam();
	}

	[UIRoute]
	private void OnWorkflowStatusChanged(object? sender, string e)
	{
		//if (Global.Instance.IsAddEmergencyPatient && !_workflow.CheckExist())
		//{
		//    //var getGeneratedIdAndName = GetPatientIdAndPatientName();
		//    //string studyId = _studyService.GotoEmergencyExamination(getGeneratedIdAndName.Item1, getGeneratedIdAndName.Item2);
		//    //if (!string.IsNullOrEmpty(studyId))
		//    //{
		//    //    _workflow.StartWorkflow(studyId);
		//    //    _logger.LogInformation("Start Exam Preparation");
		//    //    _applicationCommunicationService.Start(new ApplicationRequest { ApplicationName = ApplicationParameterNames.APPLICATIONNAME_EXAMINATION, Parameters = string.Empty, Status = ProcessStatus.None });
		//    //}
		//}
		Global.Instance.IsAddEmergencyPatient = false;
	}

	/// <summary>
	/// 当前选择项变更事件
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[UIRoute]
	private void OnSelectItemChanged(object? sender, EventArgs<(ApplicationService.Contract.Models.PatientModel patientModel, ApplicationService.Contract.Models.StudyModel studyModel)> e)
	{
		PatientInfoFormIsEnabled = true;
		var vStudyModel = _mapper.Map<VStudyModel>(e.Data.patientModel);
		if (vStudyModel is not null && !string.IsNullOrEmpty(vStudyModel.Pid))
		{
			vStudyModel.AgeType = e.Data.studyModel.AgeType;
			_mapper.Map(e.Data.studyModel, vStudyModel);

			vStudyModel.PatientId = e.Data.patientModel.PatientId;
            vStudyModel.PatientName = e.Data.patientModel.PatientName.Trim();;
            SelectedVStudyEntity = vStudyModel;
		}
		else
		{
			string patientID = SelectedVStudyEntity.PatientId;
            OnAddNewClick();
			QueryPatientInfo(patientID);
		}
	}

	/// <summary>
	/// 初始化
	/// </summary>
	public void Initialize()
	{
		Commands.Add(PatientConstString.COMMAND_ADD_NEW, new DelegateCommand(OnAddNewClick));
		Commands.Add(PatientConstString.COMMAND_SAVE, new DelegateCommand(OnSaveClick));
		Commands.Add(PatientConstString.COMMAND_DELETE, new DelegateCommand(OnDeleteClick));
		Commands.Add(PatientConstString.COMMAND_ADD_PROCEDURE, new DelegateCommand(OnAddProcedureClick));
		Commands.Add(PatientConstString.COMMAND_GO_TO_EXAM, new DelegateCommand(OnExamClick));
		Commands.Add(PatientConstString.COMMAND_ADD_EMERGENCY_PATIENT, new DelegateCommand(OnAddEmergencyPatient));
		Commands.Add(PatientConstString.COMMAND_CLEAR, new DelegateCommand(OnClear));
		Commands.Add(PatientConstString.COMMAND_ADD_ANONYMOUS_PATIENT, new DelegateCommand(OnAddAnonymousPatient));
		Commands.Add(PatientConstString.COMMAND_CLICK_PATIENT_ID, new DelegateCommand(OnClickPatientID));

		Commands.Add(PatientConstString.COMMAND_FAST_GENERATE, new DelegateCommand(OnFastGenerate));
        Commands.Add(PatientConstString.COMMAND_REFRESH_WORKLIST, new DelegateCommand<string>(OnRefreshWorkList));
        //TODO   这是临时用法，以后抽空用Converter实现
        var dictionaryGender = EnumExtension.ToDictionary<int>(typeof(Gender));
		foreach (var gender in dictionaryGender)
		{
			switch (gender.Key)
			{
				case (int)Gender.Male:
					Genders.Add(gender.Key, LanguageResource.Content_GenderMale);
					break;
				case (int)Gender.Female:
					Genders.Add(gender.Key, LanguageResource.Content_GenderFemale);
					break;
				case (int)Gender.Other:
					Genders.Add(gender.Key, LanguageResource.Content_GenderOther);
					break;
				default:
					Genders.Add(gender.Key, LanguageResource.Content_GenderOther);
					break;
			}
		}

		var ageTypeStringList = Enum.GetNames(typeof(AgeType));
		foreach (var ageTypeString in ageTypeStringList)
		{
			AgeTypes.Add((AgeType)Enum.Parse(typeof(AgeType), ageTypeString, true));
		}

		SelectedVStudyEntity.PatientId = this.GetPatientId();
		this.SetPermissions();
		this.StartAbnormalStudyIfExists();

        var worklistConfig = UserConfig.WorklistConfig;
        _worklist = worklistConfig.Worklists.FirstOrDefault(r => r.IsDefault);
    }

	private void SetPermissions()
	{
		this.IsPermissionExamEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_REGISTRATION_CREATE_NEW_EXAM);
		this.IsPermissionEmergencyEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_REGISTRATION_EMERGENCY_EXAM);
	}

	/// <summary>
	/// 检查是否需要打开非正常关闭的检查
	/// </summary>
	/// <returns></returns>
	private void StartAbnormalStudyIfExists()
	{
		if (this.IsPermissionExamEnabled == false)
		{
			_logger.LogTrace("Failed to run StartAbnormalStudyIfExists because IsPermissionExamEnabled is false.");
			return;
		}

		_logger.LogTrace("start GetStudyIdWithAbnormalClosed ");
		var studyIdWithAbnormal = _studyService.GetStudyIdWithAbnoramlClosed();

		//如果不存在非正常关闭的检查，则返回false
		if (string.IsNullOrEmpty(studyIdWithAbnormal))
		{
			_logger.LogTrace("No abnormal study found.");
			return;
		}

		Task.Run(() =>
		{
			Task.Delay(50).Wait();
			_logger.LogTrace("Start study in StartAbnormalStudyIfExists");
			_workflow.StartWorkflow(studyIdWithAbnormal);
			_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION, string.Empty));

			//RGT
			NotifyNormalExam();

			_logger.LogTrace("Start study successfully in StartAbnormalStudyIfExists");
		});
	}

	private void OnClickPatientID()
	{
		if (string.IsNullOrWhiteSpace(SelectedVStudyEntity.PatientId))
		{
			SelectedVStudyEntity.PatientId = this.GetPatientId();
		}
	}
	private void OnRefreshWorkList(string patientID)
	{
		if (!QueryPatientInfo(patientID))
		{
			FetchAutoWorklist();
            SelectedVStudyEntity.PatientId=patientID;
        }
    }
	private bool QueryPatientInfo(string patientID)
	{
        if (string.IsNullOrWhiteSpace(SelectedVStudyEntity.PatientId)) return false;
        var result = _studyApplicationService.GetPatientStudyListWithNotStarted();
        var worklistInfo = result.FirstOrDefault(r => r.Item1.PatientId == patientID);
        if (worklistInfo.Item1 is not null)
        {
            var vStudyModel = _mapper.Map<VStudyModel>(worklistInfo.Item1);
            if (vStudyModel is not null && !string.IsNullOrEmpty(vStudyModel.Pid))
            {
                vStudyModel.AgeType = worklistInfo.Item2.AgeType;
                _mapper.Map(worklistInfo.Item2, vStudyModel);
                vStudyModel.PatientId = worklistInfo.Item1.PatientId;
                vStudyModel.PatientName = worklistInfo.Item1.PatientName.Trim();
                if (vStudyModel.PatientName.Contains("^"))
                {
                    string[] arr = vStudyModel.PatientName.Split('^');
                    vStudyModel.FirstName = arr[0];
                    vStudyModel.LastName = arr[1];
                }else
				{
                    vStudyModel.LastName = vStudyModel.PatientName;
                }
                SelectedVStudyEntity = vStudyModel;
				return true ;
            }
			return false;
        }
		return false;
    }
    private void FetchAutoWorklist()
    {
        if (_worklist is null) return;
        var jobRequest = new WorklistJobRequest();
        jobRequest.Id = Guid.NewGuid().ToString();
        jobRequest.WorkflowId = Guid.NewGuid().ToString();
        jobRequest.Priority = 5;
        jobRequest.JobTaskType = JobTaskType.WorklistJob;
        jobRequest.Creator = string.Empty;
        jobRequest.Host = _worklist.IP;
        jobRequest.Port = _worklist.Port;
        jobRequest.AECaller = _worklist.Name;
        jobRequest.AETitle = _worklist.AETitle;
        jobRequest.PatientName = string.Empty;
        jobRequest.PatientId = string.Empty;
        jobRequest.PatientSex = string.Empty;
        jobRequest.AccessionNumber = string.Empty;
        jobRequest.ReferringPhysicianName = string.Empty;
        jobRequest.ReferringPhysicianName = string.Empty;
        jobRequest.ReferringPhysicianName = string.Empty;
        jobRequest.StudyDateStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        jobRequest.StudyDateEnd = jobRequest.StudyDateStart; 
        jobRequest.Parameter = jobRequest.ToJson();

        this._jobRequestService.EnqueueJobRequest(jobRequest);
    }

    private void OnApplicationStatusChanged(object? sender, ApplicationResponse e)
	{
		if (e.Status == ProcessStatus.Closed && e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_EXAMINATION && _needStartExam)
		{
			if (_emergencyButtonStatus)
			{
				return;
			}

			try
			{
				_emergencyButtonStatus = true;
				if (!CheckExist())
				{
					var getGeneratedIdAndName = GetPatientIdAndPatientName();
					string studyId =
						_studyApplicationService.GotoEmergencyExamination(getGeneratedIdAndName.Item1
							, getGeneratedIdAndName.Item2);
					if (!string.IsNullOrEmpty(studyId))
					{
						_workflow.StartWorkflow(studyId);
						_logger.LogInformation("Start Exam Preparation");
						_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION
							,
							string.Empty
						));

						//RGT
						NotifyEmergencyExam();

						_needStartExam = false;
					}
				}
			}
			finally
			{
				_emergencyButtonStatus = false;
			}
		}
		else if (e.Status == ProcessStatus.Starting && e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_EXAMINATION)
		{
			this.ResetCanExecuteStatus(false);
		}
		else if (e.Status == ProcessStatus.Started && e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_EXAMINATION)
		{
			this.ResetCanExecuteStatus(true);
		}

		//如果是PB进程,并且进程完全启动完成
		else if (e.Status == ProcessStatus.Started &&
				 e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER)
		{
			HandleEmergencyExam();
		}
	}

	private void HandleEmergencyExam()
	{
		var isEmergencyExam = _workflow.IsEmergencyExam();
		//如果这里还是急诊,需要模拟去打开急诊功能
		_logger.LogInformation($"Emergency exam triggered by self check emergency exam with : {isEmergencyExam}");
		if (isEmergencyExam)
		{
			//call emergency patient
			OnAddEmergencyPatient();

			//反置状态
			_workflow.LeaveEmergencyExam();
		}
	}

	/// <summary>
	/// 在进程已经打开的情况下,订阅 workflow的 emergency exam started事件
	/// </summary>
	private void Workflow_EmergencyExamStarted(object? sender, EventArgs e)
	{
		_logger.LogInformation("workflow emergency exam");

		HandleEmergencyExam();
	}

	private void ResetCanExecuteStatus(bool isEnabled)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			IsPermissionExamEnabled = true;
		});
	}

	private void OnDataSyncStartEmergencyExamOccured(object? sender, EventArgs e)
	{
		Application.Current?.Dispatcher?.Invoke(OnAddEmergencyPatient);
	}

	/// <summary>
	/// RGT发送过来的点击 Exam事件
	/// </summary>
	private void OnDataSyncStartExamStarted(object? sender, EventArgs e)
	{
		Application.Current?.Dispatcher?.Invoke(OnExamClick);
	}

	/// <summary>
	/// 新增一条病人和study
	/// </summary>
	public void OnAddNewClick()
	{
		SelectedVStudyEntity = new VStudyModel();
		SelectedVStudyEntity.Gender = (int)Gender.Male;
		SelectedVStudyEntity.AgeType = AgeType.Year;
		SelectedVStudyEntity.PatientId = GetPatientId();
		SelectedVStudyEntity.ExamButtonStatus = false;
		PatientInfoFormIsEnabled = true;
		AuditLogger.Log(new AuditLogInfo
		{
			CreateTime = DateTime.Now,
			EventType = "Action",
			EntryPoint = $"{this.GetType().Name}.{nameof(OnAddNewClick)}",
			UserName = Environment.UserName,
			Description = $"Add new patient info, patientid :{SelectedVStudyEntity.PatientId}",
			OriginalValues = JsonConvert.SerializeObject(null),
			ReturnValues = JsonConvert.SerializeObject(null)
		});
	}
	private void UpdatePatientInfo()
	{
        SelectedVStudyEntity = new VStudyModel();
        SelectedVStudyEntity.Gender = (int)Gender.Male;
        SelectedVStudyEntity.AgeType = AgeType.Year;
        SelectedVStudyEntity.PatientId = GetPatientId();
        SelectedVStudyEntity.ExamButtonStatus = false;
        PatientInfoFormIsEnabled = true;
    }
	private string GetPatientId()
	{
		if (_patientConfig is not null && _patientConfig.PatientIdConfig is not null)
		{
			//if (patientConfig.PatientIdConfig.SuffixType == SuffixType.TimeFormat)
			//{
			return $"{_patientConfig.PatientIdConfig.Prefix}_{_patientConfig.PatientIdConfig.Infix}{IdGenerator.NextRandomID()}";
			//}
		}
		return IdGenerator.Next();
	}

	private Tuple<string, string> GetPatientIdAndPatientName()
	{

		if (_patientConfig is not null && _patientConfig.PatientIdConfig is not null)
		{
			string id = IdGenerator.Next();
			var patientId = $"{_patientConfig.PatientIdConfig.Prefix}_{_patientConfig.PatientIdConfig.Infix}{id}";
			var patientName = $"Name{_patientConfig.PatientIdConfig.Infix}{id}";
			return new Tuple<string, string>(patientId, patientName);
		}

		string defaultId = IdGenerator.Next();
		return new Tuple<string, string>(defaultId, defaultId);

	}

	/// <summary>
	/// 新增扫描计划
	/// </summary>
	public void OnAddProcedureClick()
	{
		var worklistViewModel = Global.Instance.ServiceProvider.GetRequiredService<WorkListViewModel>();
		if (worklistViewModel.SelectedItem is null)
		{
			_dialogService.ShowDialog(false, MessageLeveles.Warning,
									  LanguageResource.Message_Info_CloseConfirmTitle,
									  LanguageResource.Message_Info_Selected_Patient_Required,
									  null, ConsoleSystemHelper.WindowHwnd);
			return;
		}

		PatientInfoFormIsEnabled = false;
		var newStudyModel = worklistViewModel.SelectedItem.Clone<VStudyModel>();
		newStudyModel.StudyDescription = newStudyModel.AccessionNo = newStudyModel.HisStudyID = newStudyModel.Comments = string.Empty;
		SelectedVStudyEntity = newStudyModel;
		AuditLogger.Log(new AuditLogInfo
		{
			CreateTime = DateTime.Now,
			EventType = "Action",
			EntryPoint = $"{this.GetType().Name}.{nameof(OnAddProcedureClick)}",
			UserName = Environment.UserName,
			Description = $"Add procedure, studyid:{SelectedVStudyEntity.StudyId}",
			OriginalValues = JsonConvert.SerializeObject(null),
			ReturnValues = JsonConvert.SerializeObject(null)
		});
	}

	/// <summary>
	/// 新增急诊病人信息
	/// </summary>
	public void OnAddEmergencyPatient()
	{
		if (_emergencyButtonStatus)
		{
			return;
		}
		try
		{
			string studyId = string.Empty;
			_emergencyButtonStatus = true;
			if (!CheckExist())
			{
				Task.Run(() =>
				{
					var getGeneratedIdAndName = GetPatientIdAndPatientName();
					studyId = _studyApplicationService.GotoEmergencyExamination(getGeneratedIdAndName.Item1, getGeneratedIdAndName.Item2);
					if (!string.IsNullOrEmpty(studyId))
					{
						_workflow.StartWorkflow(studyId);
						_logger.LogInformation("Start Exam Preparation");
						_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION));

						//RGT
						NotifyEmergencyExam();
					}
				});
			}
			else
			{
				_dialogService.ShowDialog(true, MessageLeveles.Warning,
										 LanguageResource.Message_Info_CloseConfirmTitle,
										 LanguageResource.Message_Warning_AlreadyScanning,
										 arg =>
										{
											if (arg.Result == ButtonResult.OK)
											{
												Global.Instance.IsAddEmergencyPatient = true;

												studyId = _workflow.GetCurrentStudy();
												_studyService.UpdateStudyStatus(studyId, WorkflowStatus.ExaminationClosed);
												_workflow.CloseWorkflow();
												_applicationCommunicationService.Close(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION, string.Empty, true));

												_needStartExam = true;
											}
										}, ConsoleSystemHelper.WindowHwnd);
			}
			AuditLogger.Log(new AuditLogInfo
			{
				CreateTime = DateTime.Now,
				EventType = "Action",
				EntryPoint = $"{this.GetType().Name}.{nameof(OnAddEmergencyPatient)}",
				UserName = Environment.UserName,
				Description = $"Add emergency patient, studyid:{studyId}",
				OriginalValues = JsonConvert.SerializeObject(null),
				ReturnValues = JsonConvert.SerializeObject(null)
			});
		}
		finally
		{
			_emergencyButtonStatus = false;
		}
	}

	/// <summary>
	/// 删除
	/// </summary>
	public void OnDeleteClick()
	{
		var worklistViewModel = Global.Instance.ServiceProvider.GetRequiredService<WorkListViewModel>();
		if (worklistViewModel.SelectedItem is null)
		{
			_dialogService.ShowDialog(false, MessageLeveles.Warning,
									  LanguageResource.Message_Info_CloseConfirmTitle,
									  LanguageResource.Message_Info_Selected_Patient_Required,
									  null, ConsoleSystemHelper.WindowHwnd);
			return;
		}

		_dialogService.ShowDialog(true, MessageLeveles.Warning,
								  LanguageResource.Message_Info_CloseConfirmTitle,
								  LanguageResource.Message_Warning_DeleteProcedure, arg =>
								  {
									  if (arg.Result == ButtonResult.OK)
									  {
										  var study = new ApplicationService.Contract.Models.StudyModel();
										  study = _mapper.Map<ApplicationService.Contract.Models.StudyModel>(worklistViewModel.SelectedItem);
										  _studyApplicationService.Delete(_mapper.Map<ApplicationService.Contract.Models.StudyModel>(study));
                                          AuditLogger.Log(new AuditLogInfo
										  {
											  CreateTime = DateTime.Now,
											  EventType = "Action",
											  EntryPoint = $"{this.GetType().Name}.{nameof(OnDeleteClick)}",
											  UserName = Environment.UserName,
											  Description = $"Delete procedure, studyid:{study.Id}",
											  OriginalValues = JsonConvert.SerializeObject(null),
											  ReturnValues = JsonConvert.SerializeObject(null)
										  });
									  }
								  }, ConsoleSystemHelper.WindowHwnd);
	}

	private bool CheckConditionForStartExam()
	{
		var flag = true;
		if (_applicationCommunicationService.IsExistsProcess(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME, ApplicationParameterNames.APPLICATIONNAME_SERVICETOOLS)) ||
            _applicationCommunicationService.IsExistsProcess(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME, ApplicationParameterNames.APPLICATIONNAME_DAILY))
            )
		{
			_dialogService.ShowDialog(false, MessageLeveles.Warning,
				LanguageResource.Message_Info_CloseWarningTitle,
				LanguageResource.Message_Warning_NoStartExamination,
				null,
				ConsoleSystemHelper.WindowHwnd);
			flag = false;
		}
		return flag;
	}

	/// <summary>
	/// 仅供研发阶段提供开发效率使用
	/// </summary>
	private void OnFastGenerate()
	{
		if (!CheckConditionForStartExam())
		{
			return;
		}
		if (_patientConfig is null || _patientConfig.PatientIdConfig is null)
		{
			//_dialogService.ShowDialog(false, MessageLeveles.Info,
			//                          LanguageResource.Message_Info_CloseInformationTitle,
			//                         "There is something wrong with PatientConfig or PatientIdConfig!", null, ConsoleSystemHelper.WindowHwnd);
			return;
		}

		_logger.LogInformation("Start fast generate.");

		if (!CheckExist())
		{
			Task.Run(() =>
			{
				string id = IdGenerator.Next();
				var patientId = $"{_patientConfig.PatientIdConfig.Prefix}_{_patientConfig.PatientIdConfig.Infix}{id}";
				var patientName = $"Test{_patientConfig.PatientIdConfig.Infix}{id}";

				SelectedVStudyEntity = new VStudyModel();
				SelectedVStudyEntity.LastName = patientName;
				SelectedVStudyEntity.Gender = (int)Gender.Other;
				SelectedVStudyEntity.PatientId = patientId;
				SelectedVStudyEntity.ExamButtonStatus = false;
				SelectedVStudyEntity.Age = "50";
				SelectedVStudyEntity.Birthday = DateTime.Now.AddYears(-50);

				if (!SaveChange(true))
				{
					return;
				}

				_workflow.StartWorkflow(SelectedVStudyEntity.StudyId);
				_logger.LogInformation("Start fast generation");
				_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION));

				//RGT
				NotifyNormalExam();
			});
		}
		else
		{
			_dialogService.ShowDialog(false, MessageLeveles.Info,
									  LanguageResource.Message_Info_CloseInformationTitle,
									  LanguageResource.Message_Error_CurrentScanning,
									  null, ConsoleSystemHelper.WindowHwnd);
		}

		_logger.LogInformation("Fast generation finished.");

	}

	public void OnSaveClick()
	{
		if (_saveButtonStatus)
		{
			return;
		}
		try
		{
			_saveButtonStatus = true;
			SaveChange();
			PatientInfoFormIsEnabled = true;
		}
		finally
		{
			_saveButtonStatus = false;
		}
	}

	public bool SaveChange(bool isGotoExam = false)
	{
		if (SelectedVStudyEntity is null)
		{
			return false;
		}
		SelectedVStudyEntity.ValidateAll();
		if (SelectedVStudyEntity.HasErrors)
		{
			return false;
		}
		if (PatientInfoFormIsEnabled == true)
		{
			var patient = _mapper.Map<ApplicationService.Contract.Models.PatientModel>(SelectedVStudyEntity);
			patient.PatientBirthDate = SelectedVStudyEntity.Birthday.Value.Date;
			var study = new ApplicationService.Contract.Models.StudyModel();
			if (string.IsNullOrEmpty(SelectedVStudyEntity.Weight))
			{
				SelectedVStudyEntity.Weight = null;
			}
			if (string.IsNullOrEmpty(SelectedVStudyEntity.Height))
			{
				SelectedVStudyEntity.Height = null;
			}

			study = _mapper.Map<ApplicationService.Contract.Models.StudyModel>(SelectedVStudyEntity);
			if (string.IsNullOrEmpty(SelectedVStudyEntity.FirstName))
			{
				patient.PatientName = SelectedVStudyEntity.LastName;
			}
			else
			{
				patient.PatientName = SelectedVStudyEntity.FirstName + "^" + SelectedVStudyEntity.LastName;
			}
			if (isGotoExam)
			{
				study.StudyDate = DateTime.Now;
				study.StudyTime = DateTime.Now;
				study.StudyStatus = WorkflowStatus.ExaminationStarting.ToString();
			}

			study.StudyId = string.IsNullOrEmpty(SelectedVStudyEntity.HisStudyID) ? UIDHelper.CreateStudyID() : SelectedVStudyEntity.HisStudyID;

			if (string.IsNullOrEmpty(SelectedVStudyEntity.Pid))//新增
			{
				patient.Id = Guid.NewGuid().ToString();
				patient.CreateTime = DateTime.Now;
				study.Id = Guid.NewGuid().ToString();
				study.InternalPatientId = patient.Id;
				study.StudyInstanceUID = UIDHelper.CreateStudyInstanceUID();
				if (!isGotoExam)
				{
					study.StudyStatus = WorkflowStatus.NotStarted.ToString();
				}
				study.PatientType = (int)NV.CT.CTS.Enums.PatientType.Local;//TBD
				study.StudyDate = study.StudyTime = DateTime.Now;
				SelectedVStudyEntity.StudyId = study.Id;

				var patientInfo = _mapper.Map<ApplicationService.Contract.Models.PatientModel>(patient);
				var result = this.CheckExistingPatient(true, patientInfo);
				if (result.isExisting)
				{
					Application.Current?.Dispatcher?.Invoke(()=> { 
						_dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_CloseInformationTitle, result.message, null, ConsoleSystemHelper.WindowHwnd);
                        IsPermissionExamEnabled = true;
                    });                  
                    return false;
				}

				_studyApplicationService.Insert(false, isGotoExam, patientInfo, _mapper.Map<ApplicationService.Contract.Models.StudyModel>(study));
			}
			else
			{
				var patientInfo = _mapper.Map<ApplicationService.Contract.Models.PatientModel>(patient);
				var result = this.CheckExistingPatient(false, patientInfo);
				if (result.isExisting)
				{
                    Application.Current?.Dispatcher?.Invoke(() => {
						_dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_CloseInformationTitle, result.message, null, ConsoleSystemHelper.WindowHwnd);
                        IsPermissionExamEnabled = true;
                    });
                    return false;
				}
				_studyApplicationService.Update(isGotoExam, patientInfo, _mapper.Map<ApplicationService.Contract.Models.StudyModel>(study));
			}
		}
		else
		{
			if (SelectedVStudyEntity is null)
			{
				return false;//TBD
			}
			ApplicationService.Contract.Models.PatientModel patient = new ApplicationService.Contract.Models.PatientModel();
			patient = _mapper.Map<ApplicationService.Contract.Models.PatientModel>(SelectedVStudyEntity);
			ApplicationService.Contract.Models.StudyModel study = new ApplicationService.Contract.Models.StudyModel();
			study = _mapper.Map<ApplicationService.Contract.Models.StudyModel>(SelectedVStudyEntity);
			study.Id = Guid.NewGuid().ToString();
			study.InternalPatientId = SelectedVStudyEntity.Pid;
			study.PatientType = (int)NV.CT.CTS.Enums.PatientType.Local;
			study.StudyInstanceUID = UIDHelper.CreateStudyInstanceUID();
			study.StudyDate = DateTime.Now;
			study.StudyTime = DateTime.Now;
			study.StudyStatus = isGotoExam ? WorkflowStatus.ExaminationStarting.ToString() : WorkflowStatus.NotStarted.ToString();
			study.StudyId = string.IsNullOrEmpty(SelectedVStudyEntity.HisStudyID) ? UIDHelper.CreateStudyID() : SelectedVStudyEntity.HisStudyID;

			var result = this.CheckExistingPatient(true, patient);
			if (result.isExisting)
			{
                Application.Current?.Dispatcher?.Invoke(() => { 
					_dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_CloseInformationTitle, result.message, null, ConsoleSystemHelper.WindowHwnd);
                    IsPermissionExamEnabled = true;
                });             
                return false;
			}

			_studyApplicationService.Insert(true, isGotoExam, _mapper.Map<ApplicationService.Contract.Models.PatientModel>(patient), _mapper.Map<ApplicationService.Contract.Models.StudyModel>(study));
		}
		return true;
	}

	private (bool isExisting, string message) CheckExistingPatient(bool isAddOperation, ApplicationService.Contract.Models.PatientModel patientModel)
	{
		bool isExisting = false;
		string message = string.Empty;

		var existingPatientList = _patientService.GetExistingPatientList(patientModel.PatientId, patientModel.PatientName, patientModel.PatientSex, patientModel.PatientBirthDate);
		if (isAddOperation && existingPatientList.Count > 0)
		{
			isExisting = true;
			message = LanguageResource.Message_Info_Patient_Not_Existing + Environment.NewLine + LanguageResource.Message_Info_Input_Different_Patient_Info;
			return (isExisting, message);
		}

		if (!isAddOperation && existingPatientList.Any(p => p.Id != patientModel.Id))
		{
			isExisting = true;
			message = LanguageResource.Message_Info_Patient_Not_Existing + Environment.NewLine + LanguageResource.Message_Info_Input_Different_Patient_Info;
			return (isExisting, message);
		}

		return (isExisting, message);
	}

	private bool CheckExist()
	{
		bool examProcessState= _applicationCommunicationService.IsExistsProcess(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION));
		bool workFlowState = _workflow.CheckExist();
        _logger.LogInformation($"examProcessState: {examProcessState} , workFlowState: {workFlowState}");
        return examProcessState || workFlowState;
       
    }
    private bool CheckExamProcessExist()
    {
        bool examProcessState = _applicationCommunicationService.IsExistsProcess(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION));
		_logger.LogInformation($"examProcessState: {examProcessState}");
        return examProcessState ;

    }
	public void OnExamClick()
	{
		if (_examButtonStatus)
		{
			return;
		}

		try
		{
			if (!CheckConditionForStartExam())
			{
				return;
			}
			_examButtonStatus = true;
			_logger.LogInformation("GoToExam");

			if (!CheckExist() && SelectedVStudyEntity is not null)
			{
				IsPermissionExamEnabled = false;

				Task.Run(() =>
				{
					if (!SaveChange(true))
					{
						return;
					}

					_workflow.StartWorkflow(SelectedVStudyEntity.StudyId);
					_logger.LogInformation("Start Exam Preparation");

					_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION));

					//RGT
					NotifyNormalExam();
				});
			}
			else
			{
				if (CheckExamProcessExist())
				{
					_dialogService.ShowDialog(false, MessageLeveles.Info,
						LanguageResource.Message_Info_CloseInformationTitle,
						LanguageResource.Message_Error_CurrentScanning,
						null,
						ConsoleSystemHelper.WindowHwnd);
					return;
				}
				var currentStudy = _workflow.GetCurrentStudy();
				var selectedStudyEntity = SelectedVStudyEntity != null ? SelectedVStudyEntity.ToJson() : "";
				_logger.LogInformation(
					$"current study start failed : study {currentStudy} , SelectedVStudyEntity:{selectedStudyEntity} ");
				//_dialogService.ShowDialog(false, MessageLeveles.Info,
				//	LanguageResource.Message_Info_CloseInformationTitle,
				//	LanguageResource.Message_Error_CurrentScanning,
				//	null,
				//	ConsoleSystemHelper.WindowHwnd);

				_dialogService.ShowDialog(false, MessageLeveles.Info,
					LanguageResource.Message_Info_CloseInformationTitle,
					"Examination terminated unexpectedly , you can goto patient management to resume this patient!\r\nNow will start a new examination !",
					dialogResult =>
					{
						_examButtonStatus = false;

						//恢复之前异常的检查,
						_workflow.RepairAbnormalStudy();

						//开启新的检查
						if (!CheckExist() && SelectedVStudyEntity is not null)
						{
							IsPermissionExamEnabled = false;

							//Task.Run(() =>
							//{
							if (!SaveChange(true))
							{
								return;
							}

							_workflow.StartWorkflow(SelectedVStudyEntity.StudyId);
							_logger.LogInformation("Start Exam Preparation");
							_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION));

							//RGT
							NotifyNormalExam();
							//});
						}
					},
					ConsoleSystemHelper.WindowHwnd);
			}
			AuditLogger.Log(new AuditLogInfo
			{
				CreateTime = DateTime.Now,
				EventType = "Action",
				EntryPoint = $"{this.GetType().Name}.{nameof(OnExamClick)}",
				UserName = Environment.UserName,
				Description = $"Start study, studyid:{SelectedVStudyEntity.StudyId}",
				OriginalValues = JsonConvert.SerializeObject(null),
				ReturnValues = JsonConvert.SerializeObject(null)
			});
		}
		finally
		{
			_examButtonStatus = false;
		}
	}

	public void OnClear()
	{
		if (this.SelectedVStudyEntity is null)
		{
			return;
		}

		this.ClearInformation();
	}

	private void ClearInformation()
	{
		SelectedVStudyEntity.FirstName = string.Empty;
		SelectedVStudyEntity.Height = null;
		SelectedVStudyEntity.Weight = null;
		SelectedVStudyEntity.AdmittingDiagnosis = string.Empty;
		SelectedVStudyEntity.Ward = string.Empty;
		SelectedVStudyEntity.ReferringPhysicianName = string.Empty;
		SelectedVStudyEntity.StudyDescription = string.Empty;
		SelectedVStudyEntity.AccessionNo = string.Empty;
		SelectedVStudyEntity.Comments = string.Empty;
		SelectedVStudyEntity.InstitutionName = string.Empty;
		SelectedVStudyEntity.InstitutionAddress = string.Empty;

	}

	private void InitPatientPage(List<PatientItem> patientItems)
	{
		foreach (var patientItem in patientItems)
		{
			switch (patientItem.ItemName)
			{
				case PatientConstString.PATIENT_FIRST_NAME:
					IsEnabledFirstName = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_GENDER:
					IsEnabledSex = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_HEIGHT:
					IsEnabledHeight = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_WEIGHT:
					IsEnabledWeight = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_ADMITTING_DIAGNOSIS:
					IsEnabledAdmittingDiagnosis = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_WARD:
					IsEnabledWard = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_DESCRIPTION:
					IsEnabledDescription = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_ACCESSION_NO:
					IsEnabledAccessionNo = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_ID:
					IsEnabledHisStudyID = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_COMMENTS:
					IsEnabledComments = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_INSTITUTION_NAME:
					IsEnabledInstitutionName = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_INSTITUTION_ADDRESS:
					IsEnabledInstitutionAddress = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_REFERRING_PHSICIAN:
					IsEnabledReferringPhysician = patientItem.CheckState;
					break;
				case PatientConstString.PATIENT_PERFORMING_PHYSICIAN:
					IsEnabledPerformingPhysician = patientItem.CheckState;
					break;
				default:
					break;
			}
		}
	}

	private void OnAddAnonymousPatient()
	{
		SelectedVStudyEntity = new VStudyModel();
		SelectedVStudyEntity.LastName = "Anonymous";
		SelectedVStudyEntity.Gender = (int)Gender.Other;
		SelectedVStudyEntity.PatientId = GetPatientId();
		SelectedVStudyEntity.ExamButtonStatus = false;
		PatientInfoFormIsEnabled = true;
		AuditLogger.Log(new AuditLogInfo
		{
			CreateTime = DateTime.Now,
			EventType = "Action",
			EntryPoint = $"{this.GetType().Name}.{nameof(OnAddAnonymousPatient)}",
			UserName = Environment.UserName,
			Description = $"add anonymous patient, patientid:{SelectedVStudyEntity.PatientId}",
			OriginalValues = JsonConvert.SerializeObject(null),
			ReturnValues = JsonConvert.SerializeObject(null)
		});
	}

	private void OnCurrentUserChanged(object? sender, CT.Models.UserModel? userModel)
	{
		this.SetPermissions();
	}

}
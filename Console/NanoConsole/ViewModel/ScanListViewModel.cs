//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.FacadeProxy.Common.Enums;
using PatientModel = NV.CT.DatabaseService.Contract.Models.PatientModel;
using StudyModel = NV.CT.DatabaseService.Contract.Models.StudyModel;

namespace NV.CT.NanoConsole.ViewModel;

public class ScanListViewModel : BaseViewModel
{
	private readonly ILogger<ScanListViewModel> _logger;
	private readonly IWorkflow _workflow;
	private readonly IStudyService _study;
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly IDialogService _dialogService;
	private readonly IRealtimeStatusProxyService _realtimeStatusProxyService;

	public ScanListViewModel(ILogger<ScanListViewModel> logger, IWorkflow workflow, IStudyService study, IConsoleApplicationService consoleAppService,
		IDialogService dialogService, IRealtimeStatusProxyService realtimeStatusProxyService)
	{
		_logger = logger;
		_dialogService = dialogService;
		_workflow = workflow;
		_realtimeStatusProxyService = realtimeStatusProxyService;
		_workflow.StudyChanged += StudyChanged;
		//_workflow.WorkflowStatusChanged += _workflow_WorkflowStatusChanged;
		_study = study;
		_consoleAppService = consoleAppService;
		_study.UpdateStudyInformation += OnStudyInformationUpdated;
		_realtimeStatusProxyService.RealtimeStatusChanged += RealtimeStatusChanged;

		Commands.Add("ScanPatientSelectCommand", new DelegateCommand<ScanStudyModel>(ScanPatientSelectCommand));

		_logger.LogInformation($"test viewmodel loaded event in ScanListModel");
		LoadScanWorklist();
	}

	/// <summary>
	/// ViewModel loaded hook on user control loaded
	/// </summary>
	public void ViewModel_Loaded(object sender, EventArgs e)
	{
	}

	[UIRoute]
	private void RealtimeStatusChanged(object? sender, EventArgs<RealtimeInfo> e)
	{
		switch (e.Data.Status)
		{
			case RealtimeStatus.Init:
			case RealtimeStatus.Standby:
				TaskListEnable = true;
				break;
			case RealtimeStatus.Validated:
			case RealtimeStatus.ParamConfig:
			case RealtimeStatus.MovingPartEnable:
			case RealtimeStatus.MovingPartEnabling:
			case RealtimeStatus.MovingPartEnabled:
			case RealtimeStatus.ExposureEnable:
			case RealtimeStatus.ExposureStarted:
				TaskListEnable = false; //扫描中不允许关闭
				break;
			case RealtimeStatus.ExposureSpoting:
				TaskListEnable = false; //扫描中不允许关闭
				break;
			case RealtimeStatus.ExposureSpotingIdle:
			case RealtimeStatus.ExposureFinished:
			case RealtimeStatus.ScanStopping:
			case RealtimeStatus.Error:
				TaskListEnable = false; //扫描中不允许关闭
				break;
			case RealtimeStatus.NormalScanStopped:
			case RealtimeStatus.EmergencyScanStopped:
				TaskListEnable = true;       //曝光结束或者急停结束，关闭按钮点亮起来
				break;
		}

	}


	private void ScanPatientSelectCommand(ScanStudyModel study)
	{
		UIInvoke(() =>
		{
			if (study == null)
				return;

			NewStudyId = study.StudyId;
			var hintText = "";
			var currentStudyId = _workflow.GetCurrentStudy();
			if (string.IsNullOrEmpty(currentStudyId))
			{
				hintText = "Are you sure to start a new study ? ";
				_dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm", hintText, arg =>
				{
					if (arg.Result != ButtonResult.OK) return;
					currentStudyId = study.StudyId;
					_workflow?.StartWorkflow(currentStudyId);
					Global.Instance.StudyId = currentStudyId;
					Thread.Sleep(50);

					_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION);

					_consoleAppService.IsSwitchExamination = false;
				}, ConsoleSystemHelper.WindowHwnd);
				return;
			}

			if (!string.IsNullOrEmpty(currentStudyId) && study.StudyId != currentStudyId)
			{
				hintText = "Are you sure want to close the current study and start a new one ? ";
			}

			_dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm", hintText, arg =>
			{
				if (arg.Result != ButtonResult.OK) return;
				//Todo: 剂量报告没有生成，不能关闭检查

				if (study.StudyId == currentStudyId) return;
				if (!string.IsNullOrEmpty(currentStudyId))//关闭当前病人
				{
					//FuncCommon.GetDoseReport(Global.Instance.StudyId);                            
					_workflow.CloseWorkflow();
					_consoleAppService.ExaminationClosedAndStartNewExamination += ConsoleAppServiceExaminationClosedAndStartNewExamination;
					_consoleAppService.IsSwitchExamination = true;
					//关闭未结束的病人
					_consoleAppService.CloseApp(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION);
					_study.UpdateStudyStatus(currentStudyId, WorkflowStatus.ExaminationClosed);
					currentStudyId = string.Empty;
				}
				//if (!string.IsNullOrEmpty(study.StudyId))
				//{//开启新的病人检查
				//    _workflow?.StartWorkflow(study.StudyId);
				//    currentStudyId = _workflow.GetCurrentStudy();
				//    Global.Instance.StudyId = currentStudyId;
				//}
			}, ConsoleSystemHelper.WindowHwnd);
		});
	}

	private void ConsoleAppServiceExaminationClosedAndStartNewExamination(object? sender, EventArgs e)
	{
		_workflow?.StartWorkflow(NewStudyId);
		var currentStudyId = _workflow?.GetCurrentStudy();
		if (string.IsNullOrEmpty(currentStudyId))
			return;

		Global.Instance.StudyId = currentStudyId;
		Thread.Sleep(120);
		_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION);
		_consoleAppService.IsSwitchExamination = false;
	}

	private void OnStudyInformationUpdated(object? sender, EventArgs<(PatientModel, StudyModel, DataOperateType)> e)
	{
		if (!string.IsNullOrEmpty(e.Data.Item2.Id))
		{
			_logger.LogInformation($"UpdateStudyInformation called");
			LoadScanWorklist();
		}
	}

	private void StudyChanged(object? sender, string e)
	{
		if (!string.IsNullOrEmpty(e))
		{
			_logger.LogInformation($"StudyChanged called");
			LoadScanWorklist();
		}
	}

	//[UIRoute]
	public void LoadScanWorklist()
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			var tempResult = _study.GetPatientStudyListWithNotStarted();
			var result = new List<ScanStudyModel>();
			tempResult.ForEach(a =>
			{
				if (!a.Item2.IsDeleted)
				{
					result.Add(new ScanStudyModel
					{
						StudyId = a.Item2.Id,
						PatientId = a.Item1.PatientId,
						PatientName = a.Item1.PatientName,
						BodyPart = a.Item2.BodyPart,
						Birthday = a.Item2.Birthday,
						PatientSex = a.Item1.PatientSex,
						StudyStatus = a.Item2.StudyStatus
					});
				}
			});

			_logger.LogInformation($"data:{result.ToJson()}");

			PatientModels.Clear();
			PatientModels = result.ToObservableCollection();

			ScanTotal = $"Scan {result.Count}";

			Total = result.Count;

			_logger.LogInformation($"LoadScanWorklist method called {result.Count}");
		}, DispatcherPriority.Render);
	}

	private string _scanTotal = string.Empty;
	public string ScanTotal
	{
		get => _scanTotal;
		set => SetProperty(ref _scanTotal, value);
	}

	private int _total;
	public int Total
	{
		get => _total;
		set => SetProperty(ref _total, value);
	}

	private ObservableCollection<ScanStudyModel> _patientModels = new();
	public ObservableCollection<ScanStudyModel> PatientModels
	{
		get => _patientModels;
		set => SetProperty(ref _patientModels, value);
	}

	private string newStudyId = string.Empty;
	public string NewStudyId
	{
		get => newStudyId;
		set => SetProperty(ref newStudyId, value);
	}

	private bool _taskListEnable = true;
	/// <summary>
	/// 能不能进行双击列表，很明显名字是从Examination里面拷来的
	/// </summary>
	public bool TaskListEnable
	{
		get => _taskListEnable;
		set => SetProperty(ref _taskListEnable, value);
	}

	//TODO:这里还需要考虑一下，是否有这个业务
	//private void _workflow_WorkflowStatusChanged(object? sender, string e)
	//{
	//	if (string.IsNullOrEmpty(e)) return;
	//	if (e == WorkflowStatus.ExaminationStarting.ToString())
	//	{
	//		//更新卡片信息
	//		var studyId = _workflow.GetCurrentStudy();
	//		RealtimePatientCardShow(studyId);
	//	}
	//	else if (e == WorkflowStatus.ExaminationClosing.ToString())
	//	{
	//		LoadScanWorklist();
	//	}
	//}
}
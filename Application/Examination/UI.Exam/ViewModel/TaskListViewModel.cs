//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.AppService.Contract;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SyncService.Contract;
using NV.CT.UI.Exam.Extensions;
using NV.MPS.Environment;
using System.Windows.Forms;

namespace NV.CT.UI.Exam.ViewModel;

public class TaskListViewModel : BaseViewModel
{	
	private readonly IWorkflow _workflow;
	private readonly IDataSync _dataSync;
	private readonly ILogger<TaskListViewModel> _logger;
	private readonly ISelectionManager _scanSelectManager;
	protected readonly IProtocolHostService _protocolHostService;
	protected readonly IStudyHostService _studyHostService;
	private readonly IMeasurementStatusService _measurementStatusService;
	private readonly ILayoutManager _layoutManager;
	private readonly IPageService _pageService;
	private readonly IDialogService _dialogService;
	private readonly IUIRelatedStatusService _uIRelatedStatusService;
	private readonly IApplicationCommunicationService _applicationCommunicationService;
	private AutoPositioningWindow? _autoPositioningWindow;
	public string SelectTaskModelID = string.Empty;
	ProtocolSaveAsWindow? _protocolSaveAsWindow;
	private string _taskListTitle = string.Empty;
	public string TaskListTitle
	{
		get => _taskListTitle;
		set => SetProperty(ref _taskListTitle, value);
	}

	private bool _isEnhanceTitle;
	public bool IsEnhanceTitle
	{
		get => _isEnhanceTitle;
		set => SetProperty(ref _isEnhanceTitle, value);
	}

	private ObservableCollection<TaskViewModel> _taskList = new();
	public ObservableCollection<TaskViewModel> TaskList
	{
		get => _taskList;
		set => SetProperty(ref _taskList, value);
	}

	private TaskViewModel? _selectedTask;
	public TaskViewModel? SelectedTask
	{
		get => _selectedTask;
		set => SetProperty(ref _selectedTask, value);
	}

	public TaskViewModel? CopyTaskModel { get; set; }

	private bool _taskListEnable = true;
	/// <summary>
	/// TaskList控件的状态
	/// </summary>
	public bool TaskListEnable
	{
		get => _taskListEnable;
		set => SetProperty(ref _taskListEnable, value);
	}

	private bool _isProtocolEnable = true;
	public bool IsProtocolEnable
	{
		get => _isProtocolEnable;
		set => SetProperty(ref _isProtocolEnable, value);
	}

	private bool _isValidateEnable = false;
	public bool IsValidateEnable
	{
		get => _isValidateEnable;
		set => SetProperty(ref _isValidateEnable, value);
	}

	private int CopyTaskIndex = -1;

	public TaskListViewModel(ILogger<TaskListViewModel> logger,
		ISelectionManager scanSelectManager,
		IProtocolHostService protocolHostService,
		IStudyHostService studyHostService,
		IMeasurementStatusService measurementStatusService,
		ILayoutManager layoutManager,
		IWorkflow workflow,
		IDialogService dialogService,
		IUIRelatedStatusService uiRelatedStatusService,
		IApplicationCommunicationService applicationCommunicationService,
		IPageService pageService,
		IDataSync dataSync)
	{
		_workflow = workflow;
		_logger = logger;
		_dataSync = dataSync;
		_dialogService = dialogService;
		_layoutManager = layoutManager;
		_scanSelectManager = scanSelectManager;
		_protocolHostService = protocolHostService;
		_studyHostService = studyHostService;
		_measurementStatusService = measurementStatusService;
		_uIRelatedStatusService = uiRelatedStatusService;
		_applicationCommunicationService = applicationCommunicationService;
		_pageService = pageService;

		TaskListTitle = $"{_studyHostService.Instance.PatientName}";

		Commands.Add(CommandParameters.COMMAND_BACK_PROTOCOL_SELECT, new DelegateCommand(BackProtocolSelect, () => true));
		Commands.Add(CommandParameters.COMMAND_SAVE_AS, new DelegateCommand(SaveAs, IsSaveAs));
		Commands.Add(CommandParameters.COMMAND_OPEN_RECON, new DelegateCommand(OpenRecon, () => true));
		Commands.Add(CommandParameters.COMMAND_SCAN_CLOSED, new DelegateCommand<int?>(ScanClose, _ => true));
		Commands.Add(CommandParameters.COMMAND_GET_PROTOCOL_CONTENT, new DelegateCommand(GetProtocolContentCommand, () => true));
		Commands.Add(CommandParameters.COMMAND_OPEN_CAMERA, new DelegateCommand(OpenCameraCommand, IsOpenCamera));

		Commands.Add(CommandParameters.COMMAND_CUT, new DelegateCommand(CutTask, () => true));
		Commands.Add(CommandParameters.COMMAND_COPY, new DelegateCommand(CopyTask, () => true));
		Commands.Add(CommandParameters.COMMAND_PASTE, new DelegateCommand(PasteTask, () => true));
		Commands.Add(CommandParameters.COMMAND_REPEAT, new DelegateCommand(RepeatTask, () => true));
		Commands.Add(CommandParameters.COMMAND_TASK_LIST_ITEM, new DelegateCommand(TaskListItemCommand, () => true));
		Commands.Add(CommandParameters.COMMAND_ISVALIDATED, new DelegateCommand(IsValidatedCommand, () => true));

		_protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;
		_protocolHostService.PerformStatusChanged += ProtocolHostService_PerformStatusChanged;

		_scanSelectManager.SelectionScanChanged += SelectScanChanged;
		_scanSelectManager.SelectionCleared += SelectManager_SelectionCleared;
		_measurementStatusService.MeasurementLoaded += MeasurementLoaded;

		_measurementStatusService.MeasurementCanceled += MeasurementStatusService_MeasurementCanceled;
		_measurementStatusService.MeasurementDone += MeasurementStatusService_MeasurementDone;
		_measurementStatusService.MeasurementAborted += MeasurementStatusService_MeasurementAborted;

		_protocolHostService.DescriptorChanged -= ProtocolModificationService_DescriptorChanged;
		_protocolHostService.DescriptorChanged += ProtocolModificationService_DescriptorChanged;
		_pageService.CurrentPageChanged += PageService_CurrentPageChanged;
		_uIRelatedStatusService.RealtimeStatusChanged += RealtimeStatusChanged;
		_applicationCommunicationService.NotifyApplicationClosing += ApplicationCommunicationService_NotifyApplicationClosing;
		_studyHostService.StudyChanged += StudyHostService_StudyChanged;
		_applicationCommunicationService.ApplicationStatusChanged += ApplicationCommunicationService_ApplicationStatusChanged;
	}

	private void SelectManager_SelectionCleared(object? sender, EventArgs e)
	{
		SelectedTask = null;
	}

	private void ApplicationCommunicationService_ApplicationStatusChanged(object? sender, ApplicationResponse e)
	{
		//同步协议数据到接入扫描应用
		if (e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_INTERVENTIONSCAN && e.Status == ProcessStatus.Started)
		{
			var sc = _protocolHostService.Models.LastOrDefault(t => t.Scan.Children.LastOrDefault(v => v.Status == PerformStatus.Performed) is not null);
			if (sc.Scan is not null &&
				sc.Scan.Children.LastOrDefault(v => v.Status == PerformStatus.Performed) is ReconModel recon)
			{
				//通知RGT序列数据
				_dataSync.NotifySeriesData(_protocolHostService.Instance, recon.Descriptor.Id);
			}
		}
	}

	[UIRoute]
	private void StudyHostService_StudyChanged(object? sender, EventArgs<Examination.ApplicationService.Contract.Models.StudyModel> e)
	{
		TaskListTitle = $"{_studyHostService.Instance?.PatientName}";
	}

	[UIRoute]
	private void ApplicationCommunicationService_NotifyApplicationClosing(object? sender, ApplicationResponse e)
	{
		if (!(e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_EXAMINATION ||
			  e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_RECON))
			return;


		if ((e.ExtraParameter is not null && e.ExtraParameter.ToString() == true.ToString()) || !e.NeedConfirm)
		{
			ScanCloseWithNoConfirm(e.ProcessId);
		}
		else
		{
			ScanClose(e.ProcessId);
		}

	}

	[UIRoute]
	private void RealtimeStatusChanged(object? sender, EventArgs<RealtimeInfo> e)
	{
		switch (e.Data.Status)
		{
			case RealtimeStatus.Init:
			case RealtimeStatus.Standby:
				TaskListEnable = true;
				IsProtocolEnable = true;
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
			default:
				break;
		}

		PageEnable = _scanSelectManager.CurrentSelection.Scan?.Status == PerformStatus.Unperform;
	}

	[UIRoute]
	public virtual void ProtocolHostService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		//修改界面状态
		if (e.Data.Model is ScanModel theScanModel)
		{
			_logger.LogInformation($"OnPerformStatusChanged received BaseModel ID is: {JsonConvert.SerializeObject(theScanModel.Descriptor.Id)}, newStatus is {e.Data.NewStatus}");
			var findTask = TaskList.FirstOrDefault(n => n.ScanId == theScanModel.Descriptor.Id);
			if (findTask is null)
				return;
			//其他状态 正常修改Scan任务显示图标
			findTask.FailureReason = e.Data.Model.FailureReason;
			findTask.ScanTaskStatus = e.Data.NewStatus;
		}
	}

	/// <summary>
	/// 只有在真正曝光中才会改 每个Scan的图标状态
	/// </summary>
	[UIRoute]
	public virtual void SetScanPerforming()
	{
		if (_selectedTask is not null)
		{
			_logger.LogInformation($"SetScanPerforming Performing");
			_selectedTask.ScanTaskStatus = PerformStatus.Performing;
		}
	}

	[UIRoute]
	private void ProtocolHostService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
	{
		if (e is null)
		{
			return;
		}
		if (e.Data.ChangeType == StructureChangeType.Delete || e.Data.ChangeType == StructureChangeType.Add || e.Data.ChangeType == StructureChangeType.Replace)
		{
			ProtocolStructureChanged();
		}
	}

	[UIRoute]
	private void PageService_CurrentPageChanged(object? sender, EventArgs<ScanTaskAvailableLayout> e)
	{
		(Commands[CommandParameters.COMMAND_OPEN_CAMERA] as DelegateCommand)?.RaiseCanExecuteChanged();
	}

	[UIRoute]
	private void ProtocolModificationService_DescriptorChanged(object? sender, EventArgs<BaseModel> e)
	{
		if (e.Data is null)
		{
			return;
		}
		if (e.Data is ScanModel)
		{
			var model = TaskList.FirstOrDefault(t => t.ScanId.Equals(e.Data.Descriptor.Id));
			if (model is not null)
			{
				model.TaskName = e.Data.Descriptor.Name;
			}
		}
		if (e.Data is ProtocolModel)
		{
			SetTaskListTitleByProtocol();
		}
	}

	[UIRoute]
	private void MeasurementStatusService_MeasurementCanceled(object? sender, EventArgs<string> e)
	{
		IsProtocolEnable = true;
	}

	[UIRoute]
	private void MeasurementStatusService_MeasurementDone(object? sender, EventArgs<string> e)
	{
		IsProtocolEnable = true;
	}

	[UIRoute]
	private void MeasurementStatusService_MeasurementAborted(object? sender, EventArgs<(string MeasurementId, bool ModelType, string ScanId, string ReconId, FailureReasonType ReasonType)> e)
	{
		IsProtocolEnable = true;
	}

	private void TaskListItemCommand()
	{
		if (!string.IsNullOrEmpty(SelectedTask?.ScanId))
		{
			_scanSelectManager.ClearSelectedRecon();
			_scanSelectManager.SelectScan(SelectedTask.ForId, SelectedTask.MeasurementId, SelectedTask.ScanId);
		}
	}

	[UIRoute]
	private void MeasurementLoaded(object? sender, EventArgs<string> e)
	{
		PageEnable = false;
		TaskListEnable = false;
		IsProtocolEnable = false;
	}

	[UIRoute]
	private void SelectScanChanged(object? sender, EventArgs<ScanModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		if (e is not null && e.Data is not null && e.Data.Descriptor is not null)
		{
			var scanModel = TaskList.FirstOrDefault(n => n.ScanId == e.Data.Descriptor.Id);
			if (scanModel is not null)
			{
				SelectedTask = scanModel;
				SelectTaskModelID = scanModel.ScanId;
			}
		}
		//只要不是未扫描，就是 扫描中，扫描完成都要禁止页面
		PageEnable = e.Data?.Status == PerformStatus.Unperform;
		IsProtocolEnable = true;

		//通知RGT
		NotifyRgtScanChanged(e.Data);
	}

	/// <summary>
	/// 选中检查变化后,通知到RGT客户端
	/// </summary>
	private void NotifyRgtScanChanged(ScanModel? scanModel)
	{
		var studyModel = _studyHostService.Instance;
		var protocolModel = _protocolHostService.Instance;
		if (studyModel is not null && scanModel is not null && protocolModel is not null)
		{
			var ret = new RgtScanModel();
			ret.Protocol = protocolModel.Descriptor.Name;
			ret.ScanType = scanModel.ScanImageType.ToString();
			ret.Kv = scanModel.Kilovolt.FirstOrDefault();
			ret.Ma = scanModel.Milliampere.FirstOrDefault();
			ret.PatientPosition = protocolModel.Children[0].PatientPosition.ToString();

			ret.ScanLength = (uint)UnitConvert.Micron2Millimeter(scanModel.ScanLength);
			ret.CTDlvol = scanModel.DoseEstimatedCTDI;
			ret.DLP = scanModel.DoseEstimatedDLP;
			ret.ScanTime = scanModel.FrameTime;
			ret.DelayTime = scanModel.ExposureDelayTime;
			ret.ExposureTime = scanModel.ExposureTime;
			ret.FrameTime = scanModel.FrameTime;
			ret.BodyPart = protocolModel.BodyPart.ToString();

			_dataSync.SelectionScanChange(ret);
		}
	}

	[UIRoute]
	private void ProtocolStructureChanged()
	{
		TaskListEnable = true;
		TaskList.Clear();
		if (_protocolHostService.Models is null || _protocolHostService.Models.Count == 0)
		{
			SetTaskListTitle();
			return;
		}
		IsEnhanceTitle = _protocolHostService.Instance.IsEnhanced;
		SetTaskListTitleByProtocol();
		foreach (var model in _protocolHostService.Models)
		{
			TaskViewModel scanTask = new TaskViewModel();
			scanTask.TaskName = model.Scan.Descriptor.Name;
			scanTask.StudyId = _studyHostService.StudyId;
			scanTask.ScanId = model.Scan.Descriptor.Id;
			scanTask.ProtocolId = _protocolHostService.Instance.Descriptor.Id;
			scanTask.ForId = model.Frame.Descriptor.Id;
			scanTask.MeasurementId = model.Measurement.Descriptor.Id;

			scanTask.FailureReason = model.Scan.FailureReason;
			scanTask.ScanTaskStatus = model.Scan.Status;
			//todo:调查扫描任务复制后状态问题
			_logger.LogDebug($"ScanTask of StructureChanged status: {model.Scan.Descriptor.Id}, {model.Scan.Status.ToString()}");
			scanTask.PatientPosition = model.Frame.PatientPosition;
			scanTask.ScanOption = model.Scan.ScanOption;
			scanTask.IsEnhance = model.Scan.IsEnhanced;
			if (model.Measurement.Children.Count > 1)
			{
				//TODO:连扫处理，待完善处理或确认
				scanTask.IsFirst = model.Scan == model.Measurement.Children.FirstOrDefault();
				scanTask.IsLast = model.Scan == model.Measurement.Children.LastOrDefault();
			}
			TaskList.Add(scanTask);
		}

		if (TaskList.Count == 0)
		{
			SetTaskListTitle();
		}

		AfterTaskListCreated();

		if (!string.IsNullOrEmpty(SelectTaskModelID))
		{
			var model = TaskList.FirstOrDefault(t => t.ScanId.Equals(SelectTaskModelID));
			if (model is not null)
			{
				SelectedTask = model;
			}
		}
		var vmExecutionControl = Global.ServiceProvider?.GetRequiredService<ScanControlsViewModel>();
		vmExecutionControl?.ValidateScanControl();
		GetCopyModel();
		(Commands[CommandParameters.COMMAND_SAVE_AS] as DelegateCommand)?.RaiseCanExecuteChanged();
	}

	/// <summary>
	/// do nothing, used for recon to handle
	/// </summary>
	public virtual void AfterTaskListCreated() { }

	private void SetTaskListTitleByProtocol()
	{
		if (_protocolHostService.Instance.IsAdult)
		{
			TaskListTitle = $"{_protocolHostService.Instance.Descriptor.Name}({LanguageResource.Header_ProtocolAdult})";
		}
		else
		{
			TaskListTitle = $"{_protocolHostService.Instance.Descriptor.Name}({LanguageResource.Header_ProtocolChild})";
		}
	}

	private void SetTaskListTitle()
	{
		IsEnhanceTitle = false;
		TaskListTitle = $"{_studyHostService.Instance.PatientName}";
	}

	private void GetCopyModel()
	{
		if (TaskList.Count == 0)
		{
			CopyTaskModel = new TaskViewModel();
		}
		else
		{
			if (CopyTaskModel is not null
				&& !string.IsNullOrEmpty(CopyTaskModel.ForId)
				&& !string.IsNullOrEmpty(CopyTaskModel.MeasurementId)
				&& !string.IsNullOrEmpty(CopyTaskModel.ScanId))
			{
				var model = TaskList.FirstOrDefault(t => t.ForId.Equals(CopyTaskModel.ForId)
														 && t.MeasurementId.Equals(CopyTaskModel.MeasurementId)
														 && t.ScanId.Equals(CopyTaskModel.ScanId));
				if (model is null)
				{
					CopyTaskModel = new TaskViewModel();
				}
			}
		}
	}

	public void BackProtocolSelect()
	{
		_layoutManager.SwitchToView(ScanTaskAvailableLayout.ProtocolSelection);
	}

	public void SaveAs()
	{
		if (TaskList.Count == 0)
		{
			return;
		}
		if (_protocolSaveAsWindow is null)
		{
			_protocolSaveAsWindow = new ProtocolSaveAsWindow();
		}

		Global.ServiceProvider.GetRequiredService<ProtocolSaveAsViewModel>().InitProtocol();
		WindowDialogShow.DialogShow(_protocolSaveAsWindow);
	}

	public void OpenRecon()
	{
		_layoutManager.SwitchToView(ScanTaskAvailableLayout.Recon);
		_scanSelectManager.Clear();
	}

	public void ScanCloseWithNoConfirm(int? processId)
	{
		if (processId is not null && Process.GetCurrentProcess().Id != processId)
		{
			return;
		}

		try
		{
			if (_workflow.CheckExist())
			{
				RealCloseLogic();
			}
			else
			{
				//不存在
				Process.GetCurrentProcess().Kill();
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("TaskListViewModel ScanCloseWithNoConfirm Error { 0}", ex.Message);
		}
	}

	public virtual void ScanClose(int? processId)
	{
		if (processId is not null && Process.GetCurrentProcess().Id != processId)
		{
			return;
		}

		var isWorking = _protocolHostService.Models.Any(s => s.Measurement.Status == PerformStatus.Waiting || s.Measurement.Status == PerformStatus.Performing);

		if (isWorking)
		{
            _dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Confirm_Title, "There are tasks currently in progress. Please wait.", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
		}

		try
		{
			//TODO:错误码弹窗测试代码，后续会删除
			//_dialogService.ShowErrorDialog("MCS007000017", "扫描图像加载失败（空文件夹、文件无法识别）!", MessageLeveles.Error, ConsoleSystemHelper.WindowHwnd, "1、请检查MRS连接；\r\n2、请检查网络连接；\r\n3、请检查磁盘空间.", arg =>
			//{
			//    if (arg.Result == ButtonResult.OK)
			//    {
			//        //RealCloseLogic();
			//    }
			//});

			if (_workflow.CheckExist())
			{
				var item = _protocolHostService.Models.FirstOrDefault(t => t.Scan.Status != PerformStatus.Performed);
				string message = LanguageResource.Message_Confirm_SureClose;
				if (item.Scan is not null)
				{
					message = "The existence of outstanding scanning tasks," + LanguageResource.Message_Confirm_SureClose;
				}
				_dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Confirm_Title, message, arg =>
				{
					if (arg.Result == ButtonResult.OK)
					{
						RealCloseLogic();
					}
				}, ConsoleSystemHelper.WindowHwnd);
			}
			else
			{
				//不存在
				Process.GetCurrentProcess().Kill();
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("TaskListViewModel ScanClose Error { 0}", ex.Message);
		}
	}

	private void RealCloseLogic()
	{
		_studyHostService.UpdateStudyStatus(_studyHostService.StudyId, WorkflowStatus.ExaminationClosed);

		_workflow.CloseWorkflow();

		// workflow关闭重置 SyncScreen
		Global.ServiceProvider.GetService<IScreenSync>()?.Resume();

		_dataSync.NotifyExamClose();

		Process.GetCurrentProcess().Kill();
	}

	[Obsolete(message: "do not need this logic")]
	private bool IsScanClosed(int? processId)
	{
		bool flag = true;
		var model = _protocolHostService.Models.FirstOrDefault(t => t.Scan.Status == PerformStatus.Waiting || t.Scan.Status == PerformStatus.Performing);
		if (model.Scan is not null && !string.IsNullOrEmpty(model.Scan.Descriptor.Id))
		{
			flag = false;
		}
		return flag;
	}

	public void GetProtocolContentCommand()
	{
		var directory = Path.Combine(RuntimeConfig.Console.Backup.Path, "..\\temp");

		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		var fileName = Path.Combine(directory, $"protocol_{IdGenerator.Next(0)}.xml");

		ProtocolHelper.SaveProtocol(fileName, _protocolHostService.Instance);
	}

	public void CutTask()
	{
		if (SelectedTask is not null
			&& !string.IsNullOrEmpty(SelectedTask.ScanId))
		{
			List<string> scanIds = new List<string>();
			if (SelectedTask.ScanOption == ScanOption.NVTestBolusBase)  //保证同时删除测试序列
			{
				int index = TaskList.IndexOf(SelectedTask);
				if (TaskList.Count - 1 > index
					&& TaskList[index + 1].ScanOption == ScanOption.NVTestBolus)
				{
					scanIds.Add(TaskList[index + 1].ScanId);
				}
			}
			if (SelectedTask.ScanOption == ScanOption.NVTestBolus)      //保证同时删除基底序列
			{
				int index = TaskList.IndexOf(SelectedTask);
				if (index > 0
					&& TaskList[index - 1].ScanOption == ScanOption.NVTestBolusBase && TaskList[index - 1].ScanTaskStatus != PerformStatus.Performed)
				{
					scanIds.Add(TaskList[index - 1].ScanId);
				}
			}
			scanIds.Add(SelectedTask.ScanId);

			_protocolHostService.DeleteScan(scanIds);
			_scanSelectManager.SelectScan();
			if (TaskList.IsEmpty())
			{
				SelectedTask = null;
				//jump to Protocol Selection Page
				//this.GetViewModel<ScanMainViewModel>().IsScanMainShow = false;1	
				_layoutManager.SwitchToView(ScanTaskAvailableLayout.ProtocolSelection);
			}
		}
	}

	public void CopyTask()
	{
		if (SelectedTask is not null
			&& !string.IsNullOrEmpty(SelectedTask.ScanId))
		{
			CopyTaskModel = new TaskViewModel();
			CopyTaskModel.ScanId = SelectedTask.ScanId;
			CopyTaskModel.TaskName = SelectedTask.TaskName;
			CopyTaskModel.ForId = SelectedTask.ForId;
			CopyTaskModel.MeasurementId = SelectedTask.MeasurementId;
			CopyTaskModel.TaskName = SelectedTask.TaskName;
			CopyTaskModel.ScanOption = SelectedTask.ScanOption;
			CopyTaskModel.FailureReason = SelectedTask.FailureReason;
			CopyTaskModel.ScanTaskStatus = SelectedTask.ScanTaskStatus;
			CopyTaskIndex = TaskList.IndexOf(SelectedTask);
		}
	}

	public void PasteTask()
	{
		if (CopyTaskModel is not null
			&& !string.IsNullOrEmpty(CopyTaskModel.ForId)
			&& !string.IsNullOrEmpty(CopyTaskModel.MeasurementId)
			&& !string.IsNullOrEmpty(CopyTaskModel.ScanId)
			&& TaskList.FirstOrDefault(t => t.ForId.Equals(CopyTaskModel.ForId)
			&& t.MeasurementId.Equals(CopyTaskModel.MeasurementId)
			&& t.ScanId.Equals(CopyTaskModel.ScanId)) is not null
			&& SelectedTask is not null)
		{
			List<string> scanIds = new List<string>();
			string dScanId = SelectedTask.ScanId;
			if (CopyTaskModel.ScanOption == ScanOption.NVTestBolus)      //保证同时先拷贝基底序列
			{
				if (CopyTaskIndex > 0)
				{
					scanIds.Add(TaskList[CopyTaskIndex - 1].ScanId);
					scanIds.Add(CopyTaskModel.ScanId);
				}
			}
			else if (CopyTaskModel.ScanOption == ScanOption.NVTestBolusBase)  //保证同步拷贝测试序列
			{
				if (CopyTaskIndex + 1 < TaskList.Count - 1)
				{
					scanIds.Add(CopyTaskModel.ScanId);
					scanIds.Add(TaskList[CopyTaskIndex + 1].ScanId);
				}
			}
			else
			{
				scanIds.Add(CopyTaskModel.ScanId);
			}
			if (SelectedTask.ScanOption == ScanOption.NVTestBolusBase)     //保证基底序列跟测试序列中间不会新增别的序列
			{
				int index = TaskList.IndexOf(SelectedTask);
				if (index < TaskList.Count)
				{
					dScanId = TaskList[index + 1].ScanId;
				}
			}
			if (scanIds.Count > 1)
			{
				_protocolHostService.PasteScan(scanIds, dScanId);
			}
			else
			{
				_protocolHostService.PasteScan(CopyTaskModel.ScanId, dScanId);
			}
		}
	}

	public void RepeatTask()
	{
		if (SelectedTask is not null
			&& !string.IsNullOrEmpty(SelectedTask.ForId)
			&& !string.IsNullOrEmpty(SelectedTask.MeasurementId)
			&& !string.IsNullOrEmpty(SelectedTask.ScanId))
		{
			List<string> scanIds = new List<string>();
			if (SelectedTask.ScanOption == ScanOption.NVTestBolus)      //保证同时先拷贝基底序列
			{
				int index = TaskList.IndexOf(SelectedTask);
				if (index > 0
					&& TaskList[index - 1].ScanOption == ScanOption.NVTestBolusBase)
				{
					scanIds.Add(TaskList[index - 1].ScanId);
				}
			}
			scanIds.Add(SelectedTask.ScanId);
			if (SelectedTask.ScanOption == ScanOption.NVTestBolusBase)  //保证同步拷贝测试序列
			{
				int index = TaskList.IndexOf(SelectedTask);
				if (TaskList.Count > index
					&& TaskList[index + 1].ScanOption == ScanOption.NVTestBolus)
				{
					scanIds.Add(TaskList[index + 1].ScanId);
				}
			}
			_protocolHostService.RepeatScan(scanIds);
		}
	}

	public void OpenCameraCommand()
	{
		if (_autoPositioningWindow is null)
		{
			_autoPositioningWindow = Global.ServiceProvider?.GetRequiredService<AutoPositioningWindow>();
		}
		if (_autoPositioningWindow is null)
		{
			return;
		}
		_autoPositioningWindow.Hide();
		_autoPositioningWindow.ShowDialog();
	}

	private bool IsOpenCamera()
	{
		return _pageService.CurrentPage == ScanTaskAvailableLayout.ScanDefault;
	}

	private bool IsSaveAs()
	{
		return TaskList.Count > 0;
	}

	private void IsValidatedCommand()
	{
		_uIRelatedStatusService.IsValidatedChanged(IsValidateEnable);
	}
}
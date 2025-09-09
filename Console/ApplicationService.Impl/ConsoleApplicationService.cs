using NV.CT.CTS.Models;

namespace NV.CT.Console.ApplicationService.Impl;

public class ConsoleApplicationService : IConsoleApplicationService
{
	private readonly ILogger<ConsoleApplicationService> _logger;
	private readonly IApplicationCommunicationService _appCommunicationService;
	private readonly IStudyService _study;
	private readonly IWorkflow _workflow;
	private readonly IPrint _printService;
	private readonly IViewer _viewerService;
	private readonly ILayoutManager _layoutManager;
	private readonly List<SubProcessSetting> _subProcesses;
	public List<ControlHandleModel> ControlHandleModelList { get; set; } = new();
	private readonly Dictionary<ProcessStartPart, List<string>> _consoleFixedApplications = new();
	public Dictionary<ProcessStartPart, ObservableCollection<CardModel>> CardModels { get; set; } = new();
	public bool IsSwitchExamination { get; set; }
	public string PreStartPrintStudyId { get; set; } = string.Empty;
	public string PreStartViewerStudyId { get; set; } = string.Empty;
	public event EventHandler? UiApplicationActiveStarting;
	public event EventHandler<ControlHandleModel>? UiApplicationActiveStatusChanged;
	public event EventHandler? ExaminationClosedAndStartNewExamination;
	public event EventHandler<ApplicationResponse>? ApplicationClosing;

	public ConsoleApplicationService(ILogger<ConsoleApplicationService> logger, IApplicationCommunicationService appCommunicationService, IStudyService study, IWorkflow workflow, IOptions<List<SubProcessSetting>> subProcesses, IPrint printService, ILayoutManager layoutManager, IViewer viewerService)
	{
		_logger = logger;
		_study = study;
		_printService = printService;
		_viewerService = viewerService;
		_workflow = workflow;
		_appCommunicationService = appCommunicationService;
		_appCommunicationService.ApplicationStatusChanged += ProcessStatusChangedHandler;
		_appCommunicationService.NotifyApplicationClosing += NotifyApplicationClosing;
		_printService.StudyChanged += StudyChanged;
		_viewerService.ViewerChanged += ViewerChanged;
		_layoutManager = layoutManager;

		_subProcesses = subProcesses.Value;

		var masterFixedApplication = new List<string>();
		var auxFixedApplication = new List<string>();
		masterFixedApplication.Add(ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER);

		auxFixedApplication.Add(ApplicationParameterNames.APPLICATIONNAME_PATIENTMANAGEMENT);
		auxFixedApplication.Add(ApplicationParameterNames.APPLICATIONNAME_JOBVIEWER);

		_consoleFixedApplications.Add(ProcessStartPart.Master, masterFixedApplication);
		_consoleFixedApplications.Add(ProcessStartPart.Auxilary, auxFixedApplication);

		CardModels.Add(ProcessStartPart.Master, new ObservableCollection<CardModel>());
		CardModels.Add(ProcessStartPart.Auxilary, new ObservableCollection<CardModel>());
	}

	private void StudyChanged(object? sender, string e)
	{
		PreStartPrintStudyId = e;
		var controlHandleModel = GetControlHandleModel(ApplicationParameterNames.APPLICATIONNAME_PRINT, string.Empty);

		//进程存在，激活，更新状态
		if (null != controlHandleModel)
		{
			//这里是否可以先从列表删除，再添加
			//controlHandleModel.ProcessId = response.ProcessId;
			//controlHandleModel.ControlHandle = response.ControlHandle;
			controlHandleModel.ActiveStatus = ControlActiveStatus.Active;
			//更新激活样式
			var startPart =
				CardModels[ProcessStartPart.Master]
					.Any(t => t.ItemName == ApplicationParameterNames.APPLICATIONNAME_PRINT)
					? ProcessStartPart.Master
					: ProcessStartPart.Auxilary;
			UpdateDynamicCard(controlHandleModel, startPart);
			UiApplicationActiveStatusChanged?.Invoke(this, controlHandleModel);
		}
	}
	private void ViewerChanged(object? sender, string e)
	{
		PreStartViewerStudyId = ParseParameters(e);
		var controlHandleModel = GetControlHandleModel(ApplicationParameterNames.APPLICATIONNAME_VIEWER, string.Empty);

		//进程存在，激活，更新状态
		if (null != controlHandleModel)
		{
			//这里是否可以先从列表删除，再添加
			//controlHandleModel.ProcessId = response.ProcessId;
			//controlHandleModel.ControlHandle = response.ControlHandle;
			controlHandleModel.ActiveStatus = ControlActiveStatus.Active;
			//更新激活样式
			var startPart =
				CardModels[ProcessStartPart.Master]
					.Any(t => t.ItemName == ApplicationParameterNames.APPLICATIONNAME_VIEWER)
					? ProcessStartPart.Master
					: ProcessStartPart.Auxilary;
			UpdateDynamicCard(controlHandleModel, startPart);
			UiApplicationActiveStatusChanged?.Invoke(this, controlHandleModel);
		}
	}
	private void NotifyApplicationClosing(object? sender, ApplicationResponse e)
	{
		ApplicationClosing?.Invoke(sender, e);
	}

	#region 进程启动相关回掉
	/// <summary>
	/// 进程状态变化事件，进程开启，进程关闭都会触发
	/// </summary>
	private void ProcessStatusChangedHandler(object? sender, ApplicationResponse e)
	{
		var itemName = e.ApplicationName != ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME ? e.ApplicationName : e.Parameters;
		var subProcess = _subProcesses.FirstOrDefault(t => t.ProcessName == itemName);
		if (subProcess is null)
			return;

		var startPart = Enum.Parse<ProcessStartPart>(subProcess.StartPart);
		switch (e.Status)
		{
			case ProcessStatus.Started:
				HandleProcessStarted(e, startPart);
				break;
			case ProcessStatus.Closed:
				HandleProcessClosed(e, startPart);
				break;
			case ProcessStatus.Failure:
				HandleProcessStartFailure(e, startPart);
				break;
		}
	}

	/// <summary>
	/// 处理启动进程失败的情况
	/// </summary>
	private void HandleProcessStartFailure(ApplicationResponse response, ProcessStartPart startPart)
	{
		var failureModel = new ControlHandleModel
		{
			ItemName = response.ApplicationName,
			Parameters = response.Parameters,
			ProcessId = 0,
			ControlHandle = IntPtr.Zero,
			ActiveStatus = ControlActiveStatus.None,
			ProcessStartContainer = startPart
		};

		UiApplicationActiveStatusChanged?.Invoke(this, failureModel);
	}

	private void HandleProcessStarted(ApplicationResponse response, ProcessStartPart startPart)
	{
		_logger.LogDebug($"StartProcess show window: {response.ApplicationName}");
		var controlHandleModel = GetControlHandleModel(response.ApplicationName, response.Parameters);
		//进程存在，激活，更新状态
		if (null != controlHandleModel)
		{
			//这里是否可以先从列表删除，再添加
			controlHandleModel.ProcessId = response.ProcessId;
			controlHandleModel.ControlHandle = response.ControlHandle;
			controlHandleModel.ActiveStatus = ControlActiveStatus.Active;
			controlHandleModel.ProcessStartContainer = startPart;
			//更新激活样式
			UpdateDynamicCard(controlHandleModel, startPart);

			//将所有和controlHandleModel同平台的其他对象设置为 Deactive状态
			ControlHandleModelList.Where(n => n.ProcessStartContainer == controlHandleModel.ProcessStartContainer && n.ControlKey != controlHandleModel.ControlKey).ForEach(
				item =>
				{
					item.ActiveStatus = ControlActiveStatus.Deactive;
				});
		}
		//进程不存在，将刚启动的进程加入到列表
		else
		{
			controlHandleModel = new ControlHandleModel
			{
				ItemName = response.ApplicationName,
				Parameters = response.ApplicationName != ApplicationParameterNames.APPLICATIONNAME_PRINT ? response.Parameters : string.Empty,
				ProcessId = response.ProcessId,
				ControlHandle = response.ControlHandle,
				ActiveStatus = ControlActiveStatus.Active,
				ControlModelType = ControlModelType.Process,
				ProcessStartContainer = startPart
			};

			ControlHandleModelList.Add(controlHandleModel);
			
			//将所有和controlHandleModel同平台的其他对象设置为 Deactive状态
			ControlHandleModelList.Where(n => n.ProcessStartContainer == controlHandleModel.ProcessStartContainer && n.ControlKey != controlHandleModel.ControlKey).ForEach(
				item =>
				{
					item.ActiveStatus = ControlActiveStatus.Deactive;
				});
			
			//区分是哪个区域的进程
			if (!_consoleFixedApplications[startPart].Contains(response.ApplicationName))
			{
				//生成新卡片Item，加入到卡片列表
				var cardModel = BuildCardModel(controlHandleModel);
				cardModel.ItemStatus = 1;
				//
				BuildDynamicCard(cardModel, ControlActiveStatus.Active, startPart);
				UpdateDynamicCard(controlHandleModel, startPart);
			}
		}

		UiApplicationActiveStatusChanged?.Invoke(this, controlHandleModel);

		//通知grpc当前被激活进程
		_appCommunicationService.NotifyActiveApplication(controlHandleModel);
	}

	private void HandleProcessClosed(ApplicationResponse response, ProcessStartPart startPart)
	{
		var controlHandleModel = GetControlHandleModel(response.ApplicationName, response.Parameters);

		ControlHandleModel? resControlHandleModel = null;
		if (null != controlHandleModel)
		{
			controlHandleModel.ActiveStatus = ControlActiveStatus.None;
			resControlHandleModel = new ControlHandleModel
			{
				ItemName = controlHandleModel.ItemName,
				Parameters = response.Parameters,
				ControlHandle = IntPtr.Zero,
				ActiveStatus = controlHandleModel.ActiveStatus,
				ControlModelType = controlHandleModel.ControlModelType,
				ProcessStartContainer = startPart
			};
			ControlHandleModelList.Remove(controlHandleModel);
		}

		if (null != resControlHandleModel)
		{
			//从卡片列表中删除Item
			DeleteDynamicCard(controlHandleModel, startPart);

			UiApplicationActiveStatusChanged?.Invoke(this, resControlHandleModel);
		}

		var isReturnDefaultWindow = true;
		//原检查关闭后，打开新检查
		if (response.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_EXAMINATION && IsSwitchExamination)
		{
			IsSwitchExamination = false;
			isReturnDefaultWindow = false;
			ExaminationClosedAndStartNewExamination?.Invoke(this, null);
		}

		if (isReturnDefaultWindow)
		{
			StartProcess(response.ProcessStartPart is ProcessStartPart.Master ? ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER : ApplicationParameterNames.APPLICATIONNAME_PATIENTMANAGEMENT, string.Empty);
		}
	}

	#endregion

	#region 动态卡片相关
	private void DeleteDynamicCard(ControlHandleModel e, ProcessStartPart startPart)
	{
		var target = CardModels[startPart].FirstOrDefault(n => n.CardKey == e.ControlKey);

		if (target is null)
			return;

		CardModels[startPart].Remove(target);
	}

	private void UpdateDynamicCard(ControlHandleModel e, ProcessStartPart startPart)
	{
		var card = CardModels[startPart].FirstOrDefault(t => t.CardKey == e.ControlKey);

		if (null == card)
			return;

		//todo: 优化代码
		if (e.ItemName == ApplicationParameterNames.APPLICATIONNAME_PRINT || e.ItemName == ApplicationParameterNames.APPLICATIONNAME_VIEWER)
		{
			int i = CardModels[startPart].IndexOf(card);
			var studyId = e.ItemName == ApplicationParameterNames.APPLICATIONNAME_PRINT ? PreStartPrintStudyId : PreStartViewerStudyId;
			var item = _study.Get(studyId);
			if (null != item.Study)
			{
				card.PatientId = item.Patient.PatientId;
				card.PatientName = item.Patient.PatientName;
				card.Age = item.Study.Age;
				card.AgeType = ((AgeType)item.Study.AgeType).ToString();
			}
			card.ItemStatus = e.ActiveStatus == ControlActiveStatus.Active ? 1 : 0;
			CardModels[startPart][i] = card;
		}

		card.ItemStatus = e.ActiveStatus == ControlActiveStatus.Active ? 1 : 0;
	}

	private void BuildDynamicCard(CardModel cardModel, ControlActiveStatus activeStatus, ProcessStartPart startPart)
	{
		var isContainCard = CardModels[startPart].Any(t => t.ItemName == cardModel.ItemName && t.CardParameters == cardModel.CardParameters);

		if (activeStatus != ControlActiveStatus.Active || isContainCard)
			return;

		//添加卡片
		if (cardModel.ItemName != ApplicationParameterNames.APPLICATIONNAME_EXAMINATION)
		{
			CardModels[startPart].Add(cardModel);
		}
		else
		{
			CardModels[startPart].Insert(0, cardModel);
		}
	}

	private CardModel BuildCardModel(ControlHandleModel e)
	{
		var currentCardTabModel = new CardModel();
		if (e.ItemName is ApplicationParameterNames.APPLICATIONNAME_EXAMINATION or ApplicationParameterNames.APPLICATIONNAME_RECON or ApplicationParameterNames.APPLICATIONNAME_VIEWER or ApplicationParameterNames.APPLICATIONNAME_PRINT)
		{
			string studyId;
			if (e.ItemName == ApplicationParameterNames.APPLICATIONNAME_EXAMINATION)
			{
				studyId = _workflow.GetCurrentStudy();
			}
			else
			{
				studyId = ParseStudyId(e.ItemName, e.Parameters);
			}
			var item = _study.Get(studyId);
			if (null != item.Study)
			{
				currentCardTabModel.PatientId = item.Patient.PatientId;
				currentCardTabModel.PatientName = item.Patient.PatientName;
				currentCardTabModel.Age = item.Study.Age;
				currentCardTabModel.AgeType = ((AgeType)item.Study.AgeType).ToString();
			}
		}
		else
		{
			currentCardTabModel.IsConfig = true;
			currentCardTabModel.ConfigName = e.ItemName != ApplicationParameterNames.APPLICATIONNAME_PROTOCOLMANAGEMENT && e.ItemName != ApplicationParameterNames.APPLICATIONNAME_INTERVENTIONSCAN ? e.Parameters : e.ItemName;
		}
		currentCardTabModel.ItemName = e.ItemName;
		currentCardTabModel.CardParameters = e.Parameters;
		currentCardTabModel.IsExamination = (e.ItemName == ApplicationParameterNames.APPLICATIONNAME_EXAMINATION);
		currentCardTabModel.ProcessId = e.ProcessId;
		currentCardTabModel.ItemStatus = 1;

		return currentCardTabModel;
	}

	#endregion

	public void StartControl(string applicationName, string parameters)
	{
		if (!ControlHandleModelList.Any(x => x.ItemName == applicationName && x.Parameters == parameters))
		{
			ControlHandleModelList.Add(new ControlHandleModel
			{
				ItemName = applicationName,
				Parameters = parameters,
				ProcessId = int.MinValue,
				ActiveStatus = ControlActiveStatus.None,
				ControlHandle = IntPtr.Zero,
				ControlModelType = ControlModelType.Control
			});
		}
		var controlHandleModel = ControlHandleModelList.FirstOrDefault(x => x.ItemName == applicationName && x.Parameters == parameters);
		if (controlHandleModel is not null)
		{
			//将找到的对象设置为 active
			controlHandleModel.ActiveStatus = ControlActiveStatus.Active;

			//将所有和controlHandleModel同平台的其他对象设置为 Deactive状态
			ControlHandleModelList.Where(n=>n.ProcessStartContainer==controlHandleModel.ProcessStartContainer && n.ControlKey!=controlHandleModel.ControlKey).ForEach(
				item =>
				{
					item.ActiveStatus = ControlActiveStatus.Deactive;
				});

			UiApplicationActiveStatusChanged?.Invoke(this, controlHandleModel);

			//通知grpc当前被激活进程
			_appCommunicationService.NotifyActiveApplication(controlHandleModel);
		}
	}

	public void StartProcess(string applicationName, string parameters)
	{
		var controlHandleModel = GetControlHandleModel(applicationName, parameters);
		if (null == controlHandleModel)
		{
			//进程不存在，通知底层启动进程
			_appCommunicationService.Start(new ApplicationRequest(applicationName, parameters));
		}
		else
		{
			//进程存在，激活更新状态
			controlHandleModel.ActiveStatus = ControlActiveStatus.Active;

			//将所有和controlHandleModel同平台的其他对象设置为 Deactive状态
			ControlHandleModelList.Where(n => n.ProcessStartContainer == controlHandleModel.ProcessStartContainer && n.ControlKey != controlHandleModel.ControlKey).ForEach(
				item =>
				{
					item.ActiveStatus = ControlActiveStatus.Deactive;
				});

			//根据卡片Item,更新激活状态
			var itemName = controlHandleModel.ItemName != ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME ? controlHandleModel.ItemName : controlHandleModel.Parameters;
			var subProcess = _subProcesses.FirstOrDefault(t => t.ProcessName == itemName);
			var startPart = Enum.Parse<ProcessStartPart>(subProcess.StartPart);
			CardModels[startPart].ForEach(card =>
			{
				card.ItemStatus = card.ProcessId != controlHandleModel.ProcessId ? 0 : 1;
			});
			UpdateDynamicCard(controlHandleModel, startPart);
			
			UiApplicationActiveStatusChanged?.Invoke(this, controlHandleModel);
			
			//通知grpc当前被激活进程
			_appCommunicationService.NotifyActiveApplication(controlHandleModel);

			_appCommunicationService.Active(controlHandleModel);
		}
	}

	public void StartApp(Screens screen)
	{
		UiApplicationActiveStarting?.Invoke(this, EventArgs.Empty);

		_layoutManager.Goto(screen);

		StartControl(screen.ToString(), string.Empty);
	}

	public void StartApp(string appName, string parameters = "")
	{
		UiApplicationActiveStarting?.Invoke(this, EventArgs.Empty);

		StartProcess(appName, parameters);
	}

	public void CloseApp(string appName, string parameters = "", bool needConfirm = true)
	{
		//有啥意义??

		//var controlHandleModel = GetControlHandleModel(appName, parameters);
		//if (controlHandleModel == null) return;
		//if (!controlHandleModel.IsConsoleControl)
		//{
		//	_appCommunicationService.Close(new ApplicationRequest(appName, parameters));
		//}

		_appCommunicationService.Close(new ApplicationRequest(appName, parameters, needConfirm));
	}

	/// <summary>
	/// 依次关闭各个非核心模块进程或者卡片
	/// </summary>
	public void CloseAllApp()
	{
		//_logger.LogInformation($"cardModels:{CardModels.ToJson()},ControlHandleModelList:{ControlHandleModelList.ToJson()} , _consoleFixedApplications={_consoleFixedApplications.ToJson()}");

		//关闭 主控台 非核心进程
		foreach (var item in CardModels[ProcessStartPart.Master])
		{
			_logger.LogInformation($"close master [{item.ItemName}-{item.CardParameters}]");
			CloseApp(item.ItemName, item.CardParameters, false);
		}
		//关闭 副控台 非核心进程
		foreach (var item in CardModels[ProcessStartPart.Auxilary])
		{
			_logger.LogInformation($"close auxilary [{item.ItemName}-{item.CardParameters}]");
			CloseApp(item.ItemName, item.CardParameters, false);
		}
	}

	/// <summary>
	/// 获取进程信息
	/// </summary>
	public ControlHandleModel? GetControlHandleModel(string applicationName, string parameters)
	{
		return ControlHandleModelList.FirstOrDefault(c => c.ItemName == applicationName && c.Parameters == parameters);
	}

	/// <summary>
	/// 将主副控台句柄下发到进程服务里面,进程启动用得到
	/// </summary>
	public void SetWindowHwnd(ProcessStartPart processStartPart, IntPtr windowHwnd)
	{
		_appCommunicationService.SetWindowHwnd(new ConsoleHwndRequest { StartPart = processStartPart, WindowHwnd = windowHwnd });
	}


	#region 界面跳转添加的事件

	public event EventHandler? MoveTableStarted;
	public event EventHandler? ChangePasswordStarted;
	public event EventHandler<bool>? ToggleHeaderFooterChanged;
	public event EventHandler? ShowSelfCheckSummaryStarted;
	public event EventHandler? EnterIntoMainStatusChanged;

	/// <summary>
	/// 用户发起移床命令
	/// </summary>
	public void StartMoveTable()
	{
		MoveTableStarted?.Invoke(this, EventArgs.Empty);
	}

	public void ToggleHeaderFooter(bool toggle)
	{
		ToggleHeaderFooterChanged?.Invoke(this, toggle);
	}

	public void RequestChangePassword()
	{
		ChangePasswordStarted?.Invoke(this, EventArgs.Empty);
	}

	public void ShowSelfCheckSummary()
	{
		ShowSelfCheckSummaryStarted?.Invoke(this, EventArgs.Empty);
	}

	public void EnterIntoMain()
	{
		EnterIntoMainStatusChanged?.Invoke(this, EventArgs.Empty);
	}

	public void SwitchToPlatform(ProcessStartPart processStartPart)
	{
		var platformCurrentActivatedApp= ControlHandleModelList.FirstOrDefault(c => c.ActiveStatus==ControlActiveStatus.Active && c.ProcessStartContainer==processStartPart);
		if (platformCurrentActivatedApp != null)
		{
			_appCommunicationService.NotifyActiveApplication(platformCurrentActivatedApp);
		}
	}

	#endregion

	#region 工具方法

	private string ParseStudyId(string applicationName, string parameters)
	{
		if (applicationName == ApplicationParameterNames.APPLICATIONNAME_PRINT)
		{
			return PreStartPrintStudyId;
		}
		else if (applicationName == ApplicationParameterNames.APPLICATIONNAME_VIEWER)
		{
			return PreStartViewerStudyId;
		}
		else
		{
			return parameters;
		}
	}

	private string ParseParameters(string parameters)
	{
		string splitParameter;
		if (parameters.Contains(","))
		{
			splitParameter = parameters.Split(",")[0];
		}
		else
		{
			splitParameter = parameters;
		}
		return splitParameter;
	}


	#endregion

	#region need to delete

	///// <summary>
	///// 关闭子进程后返回到首窗口
	///// </summary>
	///// <param name="isMainConsole"></param>
	//private void ReturnDefaultWindow(bool isMainConsole)
	//{
	//	StartProcess(isMainConsole ? ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER : ApplicationParameterNames.APPLICATIONNAME_PATIENTMANAGEMENT, string.Empty);
	//}

	//public bool IsExaminationOpened()
	//{
	//	return !_appCommunicationService.IsExistsProcess(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION,
	//		string.Empty));
	//}

	///// <summary>
	///// 将本地控制台控件的句柄 加入 到列表
	///// </summary>
	//public void AddConsoleControlHwnd(string applicationName, string parameters, IntPtr controlHwnd)
	//{
	//	if (!ControlHandleModelList.Any(c => c.ItemName == applicationName && c.Parameters == parameters))
	//	{
	//		ControlHandleModelList.Add(new ControlHandleModel
	//		{
	//			ItemName = applicationName,
	//			Parameters = parameters,
	//			ProcessId = int.MinValue,
	//			ActiveStatus = ControlActiveStatus.None,
	//			ControlHandle = controlHwnd,
	//			IsConsoleControl = true,
	//			CloseAndActiveName = string.Empty
	//		});
	//	}
	//}

	//public IntPtr GetControlHwnd(string applicationName, string parameters, bool isConsoleControl)
	//{
	//	var controlHwnd = IntPtr.Zero;
	//	var controlHandleModel = GetControlHandleModel(applicationName, parameters);
	//	if (null != controlHandleModel)
	//	{
	//		controlHwnd = controlHandleModel.ControlHandle;
	//	}
	//	else
	//	{
	//		var applicationInfo = _appCommunicationService.GetApplicationInfo(new ApplicationRequest(applicationName, parameters));
	//		if (applicationInfo is not null)
	//		{
	//			controlHwnd = applicationInfo.ControlHwnd;
	//		}
	//	}
	//	return controlHwnd;
	//}


	//public void ResetCardStyle(bool isMainConsole, TopPartition topPartition)
	//{
	//	if (topPartition == TopPartition.DynamicCard)
	//	{
	//		(isMainConsole ? CardModels[ProcessStartPart.Master] : CardModels[ProcessStartPart.Auxilary]).ForEach(e =>
	//		{
	//			e.ItemStatus = 0;
	//		});
	//	}
	//	CardStyleReset?.Invoke(this, new EventArgs<(bool, TopPartition)>((isMainConsole, topPartition)));
	//}

	//public void StartApp(string appName)
	//{
	//	UiApplicationActiveStarting?.Invoke(this,EventArgs.Empty);

	//	StartProcess(appName, string.Empty, false);
	//}

	//public void CloseApp(string appName)
	//{
	//	CloseHwnd(appName, string.Empty);
	//}

	//public void StartProcess(string applicationName, string parameters)
	//{
	//	StartProcess(applicationName, parameters, false);
	//}

	///// <summary>
	///// Deprecated Do not use this method
	///// </summary>
	//public ControlHandleModel GetControlViewHost(string applicationName, string parameters)
	//{
	//	var controlHandleModel = GetControlHandleModel(applicationName, parameters);
	//	return controlHandleModel;
	//}

	//public event EventHandler<EventArgs<(bool, TopPartition)>>? CardStyleReset;

	#endregion
}

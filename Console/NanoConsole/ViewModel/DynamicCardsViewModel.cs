namespace NV.CT.NanoConsole.ViewModel;

public class DynamicCardsViewModel : BaseViewModel
{
	private readonly ILogger<DynamicCardsViewModel> _logger;
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly IWorkflow _workflow;
	private readonly List<SubProcessSetting> _subProcesses;
	/// <summary>
	/// 主控台当前固定进程
	/// </summary>
	private SubProcessSetting? _currentFixedProcess;

	//private readonly List<string> _fixedApplication = new();

	public DynamicCardsViewModel(IConsoleApplicationService consoleAppService, IWorkflow workflow, 
		IOptions<List<SubProcessSetting>> subProcesses,
		ILogger<DynamicCardsViewModel> logger)
	{
		_logger = logger;
		_consoleAppService = consoleAppService;
		_subProcesses = subProcesses.Value;
		_workflow = workflow;
		_workflow.StudyChanged += Workflow_StudyChanged;
		_consoleAppService.UiApplicationActiveStatusChanged += ConsoleAppServiceUiAppActiveStatusChanged;

		//_fixedApplication.Add(ApplicationParameterNames.NANOCONSOLE_CONTENTWINDOW_HOME);
		//_fixedApplication.Add(ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER);
		//_fixedApplication.Add(ApplicationParameterNames.NANOCONSOLE_CONTENTWINDOW_SETTING);

		Commands.Add("OpenCommand", new DelegateCommand<CardTabModel>(OpenCommand));
		Commands.Add("CloseCommand", new DelegateCommand<CardTabModel>(CloseCommand));

		//_logger.LogInformation($"NanoConsole SubProcess is {_subProcesses.ToJson()}");
	}

	private void OpenCommand(CardTabModel cardTabModel)
	{
		try
		{
			if (!string.IsNullOrEmpty(cardTabModel.CardParameters))
			{
				if (!string.IsNullOrEmpty(cardTabModel.ItemName))
				{
					_consoleAppService.StartApp(cardTabModel.ItemName, cardTabModel.CardParameters);
				}
			}
			else
			{
				_consoleAppService.StartApp(cardTabModel.ItemName);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "error occured while StartApp in DynamicCardViewModel");
		}
	}

	private void CloseCommand(CardTabModel cardTabModel)
	{
		if (!string.IsNullOrEmpty(cardTabModel.CardParameters))
		{
			if (!string.IsNullOrEmpty(cardTabModel.ItemName))
			{
				//如果待关闭进程当前是未激活状态，则先激活
				if (cardTabModel.ItemStatus != 1)
				{
					_consoleAppService.StartApp(cardTabModel.ItemName, cardTabModel.CardParameters);
				}
				_consoleAppService.CloseApp(cardTabModel.ItemName, cardTabModel.CardParameters);
			}
		}
		else
		{
			_consoleAppService.CloseApp(cardTabModel.ItemName);
		}
	}

	private void ConsoleAppServiceUiAppActiveStatusChanged(object? sender, ControlHandleModel e)
	{
		//当前为主控台,不是主控台的丢弃
		if (e.ProcessStartContainer != ProcessStartPart.Master)
			return;

		var itemName = e.ItemName != ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME ? e.ItemName : e.Parameters;

		//如果该进程是"固定的" 意味着进程不是动态卡片部分
		var isFixedProcess = _subProcesses.FirstOrDefault(n => n.ProcessName == itemName && n.IsFixed);
		if (isFixedProcess != null)
		{
			//针对 不在配置中的进程,比如 SystemSetting和PatientBrowser等固定进程
			Application.Current.Dispatcher.Invoke(() =>
			{
				DynamicCardTabModelList.ForEach(tab =>
				{
					//全都设置为 not active状态
					tab.ItemStatus = 0;
				});
			});

			_currentFixedProcess = isFixedProcess;

			var allStatus = DynamicCardTabModelList.Select(n => n.ItemStatus).ToList();
			_logger.LogInformation($"NanoConsole [DynamicCard] fixed app status:{allStatus.ToJson()}, fixed item:{_currentFixedProcess?.ProcessName}-{_currentFixedProcess?.FileName}");

			return;
		}

		_currentFixedProcess = null;

		//_logger.LogInformation($"NanoConsole AppService active changed , ItemName is {itemName}");

		//if (_subProcesses.Where(t => t.StartPart == ProcessStartPart.Master.ToString() && !_fixedApplication.Contains(t.ProcessName)).All(t => t.ProcessName != itemName)) 
		//	return;

		//到这里,只处理和 "动态卡片" 相关的打开和关闭事件

		switch (e.ActiveStatus)
		{
			case ControlActiveStatus.Active:
				{
					var cardModel = _consoleAppService.CardModels[ProcessStartPart.Master].FirstOrDefault(t => t.ItemName == e.ItemName && t.CardParameters == e.Parameters);

					//int k = _consoleAppService.CardModels[ProcessStartPart.Master].IndexOf(cardModel);

					Application.Current.Dispatcher.Invoke(() =>
					{
						if (DynamicCardTabModelList.Any(t => t.ItemName == e.ItemName && t.CardParameters == e.Parameters))
						{
							//如果当前进程 在 动态卡片集合里面
							DynamicCardTabModelList.ForEach(tab =>
							{
								var target = _consoleAppService.CardModels[ProcessStartPart.Master].FirstOrDefault(t => t.ItemName == tab.ItemName && t.CardParameters == tab.CardParameters)?.ItemStatus;
								if (target.HasValue)
									tab.ItemStatus = target.Value;
							});

							var allStatus = DynamicCardTabModelList
								.Select(n => n.ItemStatus).ToList();
							_logger.LogInformation($"NanoConsole [DynamicCard] active status:{allStatus.ToJson()}, fixed item:{_currentFixedProcess?.ProcessName}-{_currentFixedProcess?.FileName}");
						}
						else
						{
							//TODO : 有待验证
							if (cardModel is null)
								return;

							//如果当前进程 不在 动态卡片集合里面
							var cardTabModel = BuildCardModel(cardModel);

							//检查插入第一位,其他的往后加
							if (cardTabModel.ItemName == ApplicationParameterNames.APPLICATIONNAME_EXAMINATION)
							{
								DynamicCardTabModelList.Insert(0, cardTabModel);
							}
							else
							{
								DynamicCardTabModelList.Add(cardTabModel);
							}

							//原先逻辑
							//DynamicCardTabModelList.ForEach(tab =>
							//{
							//	var target = _consoleAppService.CardModels[ProcessStartPart.Master].FirstOrDefault(t => t.ItemName == tab.ItemName && t.CardParameters == tab.CardParameters)?.ItemStatus;
							//	if (target.HasValue)
							//		tab.ItemStatus = target.Value;
							//});

							//新的逻辑 同步卡片的状态来自于ControlHandleModelList数据
							DynamicCardTabModelList.ForEach(tab =>
							{
								var target = _consoleAppService.ControlHandleModelList.FirstOrDefault(t => t.ItemName == tab.ItemName && t.Parameters == tab.CardParameters);
								if (target is not null)
									tab.ItemStatus = target.ActiveStatus==ControlActiveStatus.Active?1:0;
							});

							var allStatus = DynamicCardTabModelList.Select(n => n.ItemStatus).ToList();
							_logger.LogInformation($"NanoConsole [DynamicCard] no instance status:{allStatus.ToJson()}, fixed item:{_currentFixedProcess?.ProcessName}-{_currentFixedProcess?.FileName}");
						}
					});

					break;
				}
			case ControlActiveStatus.None:
				//处理关闭某个卡片
				Application.Current.Dispatcher.Invoke(() =>
				{
					//var cardTabModelList = DynamicCardTabModelList.ToList();
					//var cardList = cardTabModelList.FindAll(t => (t.ItemName + t.CardParameters) != e.ItemName + e.Parameters).ToList();
					//DynamicCardTabModelList = new ObservableCollection<CardTabModel>(cardList);

					var filteredList = DynamicCardTabModelList.Where(n => n.CardKey != e.ControlKey).ToList();
					DynamicCardTabModelList = filteredList.ToObservableCollection();

					var allStatus = DynamicCardTabModelList
						.Select(n => n.ItemStatus).ToList();
					_logger.LogInformation($"NanoConsole [DynamicCard] None status:{allStatus.ToJson()}, fixed item:{_currentFixedProcess?.ProcessName}-{_currentFixedProcess?.FileName}");

					//TODO:do not need this
					//_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER);

				});
				break;
		}

	}

	private CardTabModel BuildCardModel(CardModel cardModel)
	{
		var cardTabModel = new CardTabModel();
		cardTabModel.ProcessId = cardModel.ProcessId;
		cardTabModel.PatientName = cardModel.PatientName;
		cardTabModel.PatientId = cardModel.PatientId;
		cardTabModel.Age = cardModel.Age;
		cardTabModel.AgeType = cardModel.AgeType;
		cardTabModel.ItemStatus = cardModel.ItemStatus;
		cardTabModel.StatusBackground = cardModel.StatusBackground;
		cardTabModel.IconGeometry = cardModel.IconGeometry;
		cardTabModel.ItemName = cardModel.ItemName;
		cardTabModel.CardParameters = cardModel.CardParameters;
		cardTabModel.ConfigName = cardModel.ConfigName;
		cardTabModel.IsConfig = cardModel.IsConfig;
		cardTabModel.IsExamination = cardModel.IsExamination;

		return cardTabModel;
	}

	private string _newStudyId = string.Empty;
	public string NewStudyId
	{
		get => _newStudyId;
		set => SetProperty(ref _newStudyId, value);
	}

	private CardTabModel _currentCardTabModel = new();
	public CardTabModel CurrentCardTabModel
	{
		get => _currentCardTabModel;
		set => SetProperty(ref _currentCardTabModel, value);
	}

	private ObservableCollection<CardTabModel> _cardTabList = new();
	public ObservableCollection<CardTabModel> DynamicCardTabModelList
	{
		get => _cardTabList;
		set => SetProperty(ref _cardTabList, value);
	}

	private string _dynamicCardColor = "#5A5A89";
	public string DynamicCardColor
	{
		get => _dynamicCardColor;
		set => SetProperty(ref _dynamicCardColor, value);
	}

	private void Workflow_StudyChanged(object? sender, string e)
	{
		if (!string.IsNullOrEmpty(e))
		{
			//Todo: 确保close事件
			NewStudyId = e;
		}
	}

	#region not used code
	//private ScanCardModel _scanCardModelItem = new();
	//public ScanCardModel ScanCardModelItem
	//{
	//	get => _scanCardModelItem;
	//	set => SetProperty(ref _scanCardModelItem, value);
	//}

	//private void ConsoleAppServiceCardStyleReset(object? sender, EventArgs<(bool, TopPartition)> e)
	//{
	//	if (e.Data.Item1 && e.Data.Item2 == TopPartition.DynamicCard)
	//	{
	//		Application.Current.Dispatcher.Invoke(() =>
	//		{
	//			DynamicCardTabModelList.ForEach(tab =>
	//			{
	//				tab.ItemStatus = 0;
	//			});
	//		});
	//	}
	//}


	//private void RealtimePatientCardShow(string studyId)
	//{
	//	//获取StudyModel, PatientName, PatientID, Age
	//	if (!string.IsNullOrEmpty(studyId))
	//	{
	//		var ret = _study.Get(studyId);
	//		var studyModel = ret.Study;
	//		var patientModel = ret.Patient;

	//		ScanCardModelItem.PatientID = patientModel.PatientId;
	//		ScanCardModelItem.PatientName = patientModel.PatientName;
	//		ScanCardModelItem.PatientAge = studyModel.Age;
	//		//var ageType = ((AgeType)studyModel.AgeType).ToString();

	//		//_scanListService.DeletePatientByScanList(studyModel.Id);
	//	}
	//	else
	//	{
	//		ScanCardModelItem.PatientID = string.Empty;
	//		ScanCardModelItem.PatientName = string.Empty;
	//		ScanCardModelItem.PatientAge = 0;
	//		ScanCardModelItem.PatientAgeType = string.Empty;
	//	}
	//}
	#endregion
}
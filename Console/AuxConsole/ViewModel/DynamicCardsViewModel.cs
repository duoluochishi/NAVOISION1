using NV.CT.CTS.Models;

namespace NV.CT.AuxConsole.ViewModel;

public class DynamicCardsViewModel : BaseViewModel
{
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly List<SubProcessSetting> _subProcesses;
	private readonly List<string> _fixedApplication = new();
	private readonly ILogger<DynamicCardsViewModel> _logger;
	private ControlHandleModel? _currentActiveControlHandleModel;

	public DynamicCardsViewModel(IConsoleApplicationService consoleAppService, IOptions<List<SubProcessSetting>> subProcesses, ILogger<DynamicCardsViewModel> logger)
	{
		_logger = logger;
		_consoleAppService = consoleAppService;
		_consoleAppService.UiApplicationActiveStatusChanged += ConsoleAppServiceUiAppActiveStatusChanged;
		//_consoleAppService.CardStyleReset += ConsoleAppServiceCardStyleReset;

		_subProcesses = subProcesses.Value;

		_fixedApplication.Add(ApplicationParameterNames.APPLICATIONNAME_PATIENTMANAGEMENT);
		_fixedApplication.Add(ApplicationParameterNames.APPLICATIONNAME_JOBVIEWER);

		Commands.Add("OpenCommand", new DelegateCommand<CardTabModel>(OpenCommand));
		Commands.Add("CloseCommand", new DelegateCommand<CardTabModel>(CloseCommand));
	}

	private void OpenCommand(CardTabModel cardTabModel)
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

		//_consoleAppService.ResetCardStyle(false, TopPartition.FrontendCard);
		//_consoleAppService.ResetCardStyle(false, TopPartition.BackendCard);
	}

	private void CloseCommand(CardTabModel cardTabModel)
	{
		if (!string.IsNullOrEmpty(cardTabModel.CardParameters))
		{
			if (string.IsNullOrEmpty(cardTabModel.ItemName))
				return;

			//如果待关闭进程当前是未激活状态，则先激活
			if (cardTabModel.ItemStatus != 1)
			{
				_consoleAppService.StartApp(cardTabModel.ItemName, cardTabModel.CardParameters);
			}
			_consoleAppService.CloseApp(cardTabModel.ItemName, cardTabModel.CardParameters);
		}
		else
		{
			_consoleAppService.CloseApp(cardTabModel.ItemName);
		}
	}

	/// <summary>
	/// 进程激活事件回调
	/// </summary>
	private void ConsoleAppServiceUiAppActiveStatusChanged(object? sender, ControlHandleModel e)
	{
		//当前是副控台, 不是副控台的事件丢弃
		if (e.ProcessStartContainer != ProcessStartPart.Auxilary)
			return;

		var itemName = e.ItemName != ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME ? e.ItemName : e.Parameters;
		if (_subProcesses.Where(t => t.StartPart == ProcessStartPart.Auxilary.ToString() &&
			!_fixedApplication.Contains(t.ProcessName)).Any(t => t.ProcessName == itemName))
		{
			_currentActiveControlHandleModel = null;

			switch (e.ActiveStatus)
			{
				case ControlActiveStatus.Active:
					{
						//绑定Card数据
						var cardModel = _consoleAppService.CardModels[ProcessStartPart.Auxilary].FirstOrDefault(t => t.ItemName == e.ItemName && t.CardParameters == e.Parameters);
						var k = 0;
						k = _consoleAppService.CardModels[ProcessStartPart.Auxilary].IndexOf(cardModel);
						Application.Current.Dispatcher.Invoke(() =>
						{
							if (DynamicCardTabList.Any(t => t.ItemName == e.ItemName && t.CardParameters == e.Parameters))
							{
								var n = 0;
								
								DynamicCardTabList.ForEach(tab =>
								{
									tab.ItemStatus = _consoleAppService.CardModels[ProcessStartPart.Auxilary][n]
										.ItemStatus;
									tab.PatientId = _consoleAppService.CardModels[ProcessStartPart.Auxilary][n]
										.PatientId;
									tab.PatientName = _consoleAppService.CardModels[ProcessStartPart.Auxilary][n]
										.PatientName;
									tab.Age = _consoleAppService.CardModels[ProcessStartPart.Auxilary][n]
										.Age;
									tab.AgeType = _consoleAppService.CardModels[ProcessStartPart.Auxilary][n]
										.AgeType;
									n++;
								});
								
								var allStatus = _consoleAppService.CardModels[ProcessStartPart.Auxilary]
									.Select(n => n.ItemStatus).ToList();
								_logger.LogInformation($"Auxilary [DynamicCard] active status:{allStatus.ToJson()}, fixed item:{_currentActiveControlHandleModel?.ItemName}-{_currentActiveControlHandleModel?.ActiveStatus}");
							}
							else
							{
								var cardTabModel = BuildCardModel(cardModel);
								if (k == 0)
								{
									DynamicCardTabList.Insert(0, cardTabModel);
								}
								else
								{
									DynamicCardTabList.Add(cardTabModel);
								}

								DynamicCardTabList.ForEach(tab =>
								{
									var targetItem = _consoleAppService.CardModels[ProcessStartPart.Auxilary]
										.FirstOrDefault(t =>
											t.ItemName == tab.ItemName && t.CardParameters == tab.CardParameters);
									if(targetItem is not null)
										tab.ItemStatus = targetItem.ItemStatus;
								});

								var allStatus = _consoleAppService.CardModels[ProcessStartPart.Auxilary]
									.Select(n => n.ItemStatus).ToList();
								_logger.LogInformation($"Auxilary [DynamicCard] no instance status:{allStatus.ToJson()}, fixed item:{_currentActiveControlHandleModel?.ItemName}-{_currentActiveControlHandleModel?.ActiveStatus}");

							}
						});

						//_consoleAppService.ResetCardStyle(false, TopPartition.FrontendCard);
						//_consoleAppService.ResetCardStyle(false, TopPartition.BackendCard);

						break;
					}
				case ControlActiveStatus.None:
					Application.Current.Dispatcher.Invoke(() =>
					{
						var tmpList = DynamicCardTabList.ToList();
						//var cardTab = cardTabModelList.FindNext(t => (t.ItemName + t.CardParameters) == (e.ItemName + e.Parameters));

						var cardList = tmpList.FindAll(t => (t.ItemName + t.CardParameters) != (e.ItemName + e.Parameters)).ToList();
						DynamicCardTabList = new ObservableCollection<CardTabModel>(cardList);
					});

					//副控台回到默认进程
					_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_PATIENTMANAGEMENT);

					var allStatus = _consoleAppService.CardModels[ProcessStartPart.Auxilary]
						.Select(n => n.ItemStatus).ToList();
					_logger.LogInformation($"Auxilary [DynamicCard] None status:{allStatus.ToJson()} , fixed item:{_currentActiveControlHandleModel?.ItemName}-{_currentActiveControlHandleModel?.ActiveStatus}");

					//_consoleAppService.ResetCardStyle(false, TopPartition.DynamicCard);
					break;
			}
		}
		else
		{
			//针对 不在配置中的进程,比如 JobViewer和PatientManagement等固定进程
			Application.Current.Dispatcher.Invoke(() =>
			{
				DynamicCardTabList.ForEach(tab =>
				{
					//全都设置为 not active状态
					tab.ItemStatus = 0;
				});
			});

			//给固定fixedControlHandleModel赋值
			_currentActiveControlHandleModel = e;

			var allStatus = _consoleAppService.CardModels[ProcessStartPart.Auxilary]
				.Select(n => n.ItemStatus).ToList();
			_logger.LogInformation($"Auxilary [DynamicCard] fixed app status:{allStatus.ToJson()} , fixed item:{_currentActiveControlHandleModel.ItemName}-{_currentActiveControlHandleModel.ActiveStatus}");
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

	private CardTabModel _currentCard = new();
	public CardTabModel CurrentCard
	{
		get => _currentCard;
		set => SetProperty(ref _currentCard, value);
	}

	private ObservableCollection<CardTabModel> _dynamicCardTabList = new();
	public ObservableCollection<CardTabModel> DynamicCardTabList
	{
		get => _dynamicCardTabList;
		set => SetProperty(ref _dynamicCardTabList, value);
	}

	#region not used

	//private void ConsoleAppServiceCardStyleReset(object? sender, EventArgs<(bool, TopPartition)> e)
	//{
	//	if (!e.Data.Item1 && e.Data.Item2 == TopPartition.DynamicCard)
	//	{
	//		Application.Current.Dispatcher.Invoke(() =>
	//		{
	//			DynamicCardTabList.ForEach(tab =>
	//			{
	//				tab.ItemStatus = 0;
	//			});
	//		});
	//	}
	//}

	#endregion
}
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums.SelfCheck;
using NV.CT.FacadeProxy.Common.Models.SelfCheck;

namespace NV.CT.NanoConsole.ViewModel;

public class SelfCheckViewModel : BaseViewModel
{
	private readonly ISelfCheckService _selfCheckService;
	private readonly ILogger<SelfCheckViewModel> _logger;
	private readonly IUserService _userService;
	private readonly ILayoutManager? _layoutManager;
	private readonly IDetectorTemperatureService _detectorTemperatureService;
	private readonly IComponentStatusProxyService _componentStatusProxyService;

	public SelfCheckViewModel(ISelfCheckService selfCheckService, ILogger<SelfCheckViewModel> logger, IUserService userService, IDetectorTemperatureService detectorTemperatureService, IComponentStatusProxyService componentStatusProxyService)
	{
		_logger = logger;
		_selfCheckService = selfCheckService;
		_userService = userService;
		_componentStatusProxyService = componentStatusProxyService;
		_detectorTemperatureService = detectorTemperatureService;
		_layoutManager = CTS.Global.ServiceProvider.GetService<ILayoutManager>();
		if (_layoutManager != null)
		{
			_layoutManager.LayoutChanged += LayoutChanged;
		}

		_selfCheckService.SelfCheckStatusChanged += SelfCheckStatusChanged;
		SelfCheckProxy.Instance.SelfCheckStatusChanged += Instance_SelfCheckStatusChanged;

		Commands.Add("EmergencyLoginCommand", new DelegateCommand(EmergencyLogin));
		Commands.Add("RedoSelfCheck", new DelegateCommand(RedoSelfCheck));
		Commands.Add("AddOrUpdateDeviceComponent",new DelegateCommand(AddOrUpdateDeviceComponent));
		Commands.Add("GetDeviceHistory",new DelegateCommand(GetDeviceHistory));

		InitOnce();

		GetSelfCheckResults();
	}

	private void GetDeviceHistory()
	{
		var list = new List<ComponentInfo>
		{
			new()
			{
				Id = 1, SerialNumber = "detector_unit_1", BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),ComponentType = DeviceComponentType.DetectorUnit
			},
			new()
			{
				Id = 3, SerialNumber = "detector_unit_3", BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),ComponentType = DeviceComponentType.DetectorUnit
			},
			new()
			{
				Id = 5, SerialNumber = "collimator_5", BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),ComponentType = DeviceComponentType.Collimator
			}
		};
		var resultList=SystemConfig.GetComponentHistory(list);
		_logger.LogInformation($"his is {resultList.ToJson()}");
	}

	private void AddOrUpdateDeviceComponent()
	{
		var list = new List<ComponentInfo>
		{
			new()
			{
				Id = 1, SerialNumber = "detector_unit_1", BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),ComponentType = DeviceComponentType.DetectorUnit
			},
			new()
			{
				Id = 3, SerialNumber = "detector_unit_3", BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),ComponentType = DeviceComponentType.DetectorUnit
			},
			new()
			{
				Id = 5, SerialNumber = "collimator_5", BeginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),ComponentType = DeviceComponentType.Collimator
			}
		};
		SystemConfig.AddOrUpdateDeviceComponent(list);
	}

	private void Instance_SelfCheckStatusChanged(object? sender, FacadeProxy.Common.EventArguments.SelfCheckEventArgs e)
	{
		var result = new SelfCheckResult();
		result.CheckName = e.SelfCheckInfo.PartType.ToString();
		result.CheckStatus = e.SelfCheckInfo.Status;
		result.DetailedSelfCheckInfos = e.SelfCheckInfo.DetailedSelfCheckInfos;

		RefreshSelfCheckItem(result);
	}

	private void LayoutChanged(object? sender, Screens e)
	{
		if (e == Screens.SelfCheckSimple)
		{
			_logger.LogInformation("SelfCheckViewModel enter into SelfCheckSimple and call GetSelfCheckResults");
			GetSelfCheckResults();
		}
	}

	private void InitOnce()
	{
		//init self check results with first default status
		//TODO:临时移除 DMS
		//TODO: 临时移除 Panel , UPS ExtBoard 让自检达成100%
		var allEnums = Enum.GetNames<SelfCheckPartType>().Where(n => n.ToLower() != "dms" && n.ToLower()!="panel" && n.ToLower()!="extboard" && n.ToLower()!="ups").ToList();
		allEnums.ForEach(checkItem =>
		{
			CheckItems.Add(new SelfCheckResult(Enum.Parse<SelfCheckPartType>(checkItem), SelfCheckStatus.Unknown, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		});

		////for debug only
		//if (IsDevelopment)
		//{
		//	PrepareTestData();
		//}

		//下发设置探测器温度
		Task.Run(() =>
		{
			try
			{
				_detectorTemperatureService.SetDetectorTargetTemperature(SystemConfig.DetectorTemperatureConfig.DetectorModules);
			}
			catch (Exception ex)
			{
				_logger.LogError($"set detector temperature error {ex.Message}", ex);
			}
		});
	}

	public void GetSelfCheckResults()
	{
		//获取一次更新状态
		var tmp = _selfCheckService.GetSelfCheckResults();
		//_logger.LogInformation($"SelfCheckViewModel GetSelfCheckResults:{tmp.ToJson()}");
		tmp.ForEach(RefreshSelfCheckItem);
	}

	private void EmergencyLogin()
	{
		if (_userService.EmergencyLogin())
		{
			_layoutManager?.Goto(Screens.Emergency);
		}
	}

	private void RedoSelfCheck()
	{
		try
		{
			_logger.LogInformation("RedoSelfCheck started");
			CheckItems.ForEach(item =>
			{
				//重置每个部件状态
				item.CheckStatus = SelfCheckStatus.Unknown;
			});

			//TODO:临时移除 DMS
			//TODO: 临时移除 Panel , UPS ExtBoard 让自检达成100%
			SelfCheckProxy.Instance.RunSelfCheck(SelfCheckPartType.CTBox | SelfCheckPartType.IFBox | SelfCheckPartType.PDU | SelfCheckPartType.MtControl | SelfCheckPartType.TubeIntf1 | SelfCheckPartType.TubeIntf2 | SelfCheckPartType.TubeIntf3 | SelfCheckPartType.TubeIntf4 | SelfCheckPartType.TubeIntf5 | SelfCheckPartType.TubeIntf6 | SelfCheckPartType.Table | SelfCheckPartType.AuxBoard | SelfCheckPartType.ControlBox | SelfCheckPartType.MRS | SelfCheckPartType.MCS);
		}
		catch (Exception ex)
		{
			_logger.LogError($"Call RedoSelfCheck error : {ex.Message}-{ex.StackTrace}");
		}
	}

	private void SelfCheckStatusChanged(object? sender, SelfCheckResult e)
	{
		//_logger.LogInformation($"SelfCheckViewModel self check status changed to {e.ToJson()}");

		RefreshSelfCheckItem(e);
	}

	private void RefreshSelfCheckItem(SelfCheckResult e)
	{
		var targetItem = CheckItems.FirstOrDefault(n => n.CheckName == e.CheckName);
		if (targetItem == null)
			return;

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			targetItem.CheckStatus = e.CheckStatus;
			targetItem.DetailedSelfCheckInfos = e.DetailedSelfCheckInfos;
			targetItem.Timestamp = e.Timestamp;

			Progress = CheckItems.Count(n => n.CheckStatus == SelfCheckStatus.Success) * 100 / CheckItems.Count;

			//error timeout排序在前面
			var sorted = CheckItems.OrderBy(item =>
			{
				return item.CheckStatus switch
				{
					SelfCheckStatus.Error => 0,
					SelfCheckStatus.Timeout => 1,
					_ => 2
				};
			}).ToList();

			// 先断开 UI 的绑定（避免枚举冲突），再批量替换
			var tmp = new ObservableCollection<SelfCheckResult>(sorted);
			CheckItems.Clear();
			foreach (var item in tmp)
			{
				CheckItems.Add(item);
			}
		});
	}

	/// <summary>
	/// 所有自检查项列表
	/// </summary>
	private ObservableCollection<SelfCheckResult> _checkItems = new();
	public ObservableCollection<SelfCheckResult> CheckItems
	{
		get => _checkItems;
		set => SetProperty(ref _checkItems, value);
	}

	private SelfCheckResult? _selectedItem;
	public SelfCheckResult? SelectedItem
	{
		get => _selectedItem;
		set => SetProperty(ref _selectedItem, value);
	}

	private int _progress;
	public int Progress
	{
		get => _progress;
		set => SetProperty(ref _progress, value);
	}

	#region test method
	private void PrepareTestData()
	{

		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.MRS, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.CTBox, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.IFBox, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		var t = new SelfCheckResult(SelfCheckPartType.PDU, SelfCheckStatus.Error, DateTime.Now,
			new List<DetailedSelfCheckInfo>());

		//var detailedMsg = @"AuxBoardNetworkConnecionCheck,AuxBoardLeftPanelStatusCheck,AuxBoardRightPanelStatusCheck,AuxBoardBreathNivagationLightStatusCheck,AuxBoardWirelessPanelStatusCheck";

		//t.ErrorMessageTip = detailedMsg;
		//CheckItems.Add();


		var tmp1 = new List<DetailedSelfCheckInfo>();
		tmp1.Add(new DetailedSelfCheckInfo()
		{
			ItemType = DetailedSelfCheckItemType.MtControlHardwareVersionCheck,
			Status = SelfCheckStatus.Error
		});
		tmp1.Add(new DetailedSelfCheckInfo()
		{
			ItemType = DetailedSelfCheckItemType.MtControlCANCommunicationCheck,
			Status = SelfCheckStatus.Error
		});
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.MtControl, SelfCheckStatus.Error, DateTime.Now, tmp1));

		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.TubeIntf1, SelfCheckStatus.InProgress, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.TubeIntf2, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.TubeIntf3, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.TubeIntf4, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.TubeIntf5, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.TubeIntf6, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()));

		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.Table, SelfCheckStatus.InProgress, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.AuxBoard, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()));
		CheckItems.Add(new SelfCheckResult(SelfCheckPartType.ControlBox, SelfCheckStatus.Success, DateTime.Now, new List<DetailedSelfCheckInfo>()
		{
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxCANCommunicationCheck,Status = SelfCheckStatus.Error},
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxDoorStatusCheck,Status = SelfCheckStatus.Error},
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxFirmwareVersionCheck,Status = SelfCheckStatus.Error},
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxFrontPanelStatusCheck,Status = SelfCheckStatus.Error},
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxNetworkConnecionCheck,Status = SelfCheckStatus.Error},
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxPedalPrepareCheck,Status = SelfCheckStatus.Error},
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxPedalReadyCheck,Status = SelfCheckStatus.Error},
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxHardwareVersionCheck,Status = SelfCheckStatus.Error},
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxRearPanelStatusCheck,Status = SelfCheckStatus.Error},
			new DetailedSelfCheckInfo(){ItemType = DetailedSelfCheckItemType.CTBoxShutdownStatusCheck,Status = SelfCheckStatus.Error},
		}));

		CheckItems = CheckItems.OrderBy(item =>
		{
			return item.CheckStatus switch
			{
				SelfCheckStatus.Error => 0,
				SelfCheckStatus.Timeout => 1,
				_ => 2
			};
		}).ToList().ToObservableCollection();
	}
	#endregion

}


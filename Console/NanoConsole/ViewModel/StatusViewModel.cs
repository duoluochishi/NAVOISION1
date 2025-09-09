using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigService.Models.SystemConfig;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.MessageService.Contract;
using NV.CT.SystemInterface.MCSRuntime.Contract;
using NV.CT.SystemInterface.MCSRuntime.Contract.Disk;

namespace NV.CT.NanoConsole.ViewModel;

public class StatusViewModel : BaseViewModel
{
	#region Members
	private const string HAS_MESSAGE_ICON = "pack://application:,,,/NV.CT.UI.Controls;component/Icons/message.png";
	private const string NO_MESSAGE_ICON = "pack://application:,,,/NV.CT.UI.Controls;component/Icons/no_message.png";

	private readonly ILogger<StatusViewModel> _logger;
	private readonly ISpecialDiskService _specialDiskService;
	private readonly IHeatCapacityService _heatCapacityService;
	private readonly IMessageService _messageService;
	private readonly IRealtimeStatusProxyService _realtimeStatusProxyService;
	#endregion

	#region property
	private bool _isOpenDoor;
	public bool IsOpenDoor
	{
		get => _isOpenDoor;
		set => SetProperty(ref _isOpenDoor, value);
	}

	private int _tubeStatusLevel;
	public int TubeStatusLevel
	{
		get => _tubeStatusLevel;
		set => SetProperty(ref _tubeStatusLevel, value);
	}

	/// <summary>
	/// MCS磁盘空间警告级别
	/// </summary>
	private int _mcsFreeSpaceWarnLevel;
	public int McsFreeSpaceWarnLevel
	{
		get => _mcsFreeSpaceWarnLevel;
		set => SetProperty(ref _mcsFreeSpaceWarnLevel, value);
	}

	private string _mcsHardFreeSpace = "Hard free space";
	public string McsHardFreeSpace
	{
		get => _mcsHardFreeSpace;
		set => SetProperty(ref _mcsHardFreeSpace, value);
	}

	private int _rawFreeSpaceTip;
	public int RawFreeSpaceTip
	{
		get => _rawFreeSpaceTip;
		set => SetProperty(ref _rawFreeSpaceTip, value);
	}

	private string _mrsFreeSpace = "MRS free space";
	public string MrsFreeSpace
	{
		get => _mrsFreeSpace;
		set => SetProperty(ref _mrsFreeSpace, value);
	}

	private bool _ipadConnectStatus;
	public bool IpadConnectStatus
	{
		get => _ipadConnectStatus;
		set => SetProperty(ref _ipadConnectStatus, value);
	}

	private bool _controlBoxConnectStatus;
	public bool ControlBoxConnectStatus
	{
		get => _controlBoxConnectStatus;
		set => SetProperty(ref _controlBoxConnectStatus, value);
	}

	private bool _mrsConnectStatus;
	public bool MrsConnectStatus
	{
		get => _mrsConnectStatus;
		set => SetProperty(ref _mrsConnectStatus, value);
	}

	private bool _panelConnectStatus;
	public bool PanelConnectStatus
	{
		get => _panelConnectStatus;
		set => SetProperty(ref _panelConnectStatus, value);
	}

	private string _tubeToolTip = string.Empty;
	public string TubeToolTip
	{
		get => _tubeToolTip;
		set => SetProperty(ref _tubeToolTip, value);
	}

	private string _messageIconSource = NO_MESSAGE_ICON;
	public string MessageIconSource
	{
		get => _messageIconSource;
		set => SetProperty(ref _messageIconSource, value);
	}

	private bool _isOfflineConnected;
	public bool IsOfflineConnected
	{
		get => _isOfflineConnected;
		set
		{
			if (_isOfflineConnected != value)
			{
				_isOfflineConnected = value;
				Application.Current?.Dispatcher.Invoke(() =>
				{
					MrsConnectStatus = value;
				});
			}
		}
	}
	#endregion

	private readonly IOfflineConnectionService _offlineConnectionService;
	private readonly IControlBoxStatusService _controlBoxStatusService;
	private readonly IDoorStatusService _doorStatusService;
	private readonly IOfflineTaskProxyService _offlineTaskProxyService;

	public StatusViewModel(ILogger<StatusViewModel> logger,
						   IHeatCapacityService heatCapacityService,
						   IMessageService messageService,
						   IOfflineConnectionService offlineConnectionService,
						   ISpecialDiskService specialDiskService,
						   IControlBoxStatusService controlBoxStatusService,
						   IDoorStatusService doorStatusService,
						   IOfflineTaskProxyService offlineTaskProxyService,
						   IRealtimeStatusProxyService realtimeStatusProxyService)
	{
		_logger = logger;
		_specialDiskService = specialDiskService;
		_offlineConnectionService = offlineConnectionService;
		_heatCapacityService = heatCapacityService;
		_messageService = messageService;
		_controlBoxStatusService = controlBoxStatusService;
		_doorStatusService = doorStatusService;
		_offlineTaskProxyService = offlineTaskProxyService;
		_realtimeStatusProxyService = realtimeStatusProxyService;

		Commands.Add("ShowMessageWindowCommand", new DelegateCommand(PopupMessageWindow));
		Commands.Add("MCSEnter", new DelegateCommand(McsEnter));
		Commands.Add("MRSEnter", new DelegateCommand(MrsEnter));

		IsOfflineConnected = _offlineConnectionService.IsConnected;
		ControlBoxConnectStatus = _controlBoxStatusService.Status != PartStatus.Disconnection;

		_offlineConnectionService.ConnectionStatusChanged += OnConnectionStatusChanged;
		_controlBoxStatusService.StatusChanged += OnControlBoxStatusChanged;
		_heatCapacityService.HeatCapacityChanged += OnHeatCapacityChanged;
		_doorStatusService.StatusChanged += OnApplicationDoorStatusChanged;
		_messageService.MessageNotify -= OnNotifyMessage;
		_messageService.MessageNotify += OnNotifyMessage;
		_specialDiskService.FreeDiskSpaceWarnedEvent += SpecialDiskService_FreeDiskSpaceWarnedEvent;

		_realtimeStatusProxyService.DeviceErrorOccurred -= RealtimeStatusProxyService_DeviceErrorOccurred;
		_realtimeStatusProxyService.DeviceErrorOccurred += RealtimeStatusProxyService_DeviceErrorOccurred;

		MrsEnter();
	}

	[UIRoute]
	private void RealtimeStatusProxyService_DeviceErrorOccurred(object? sender, EventArgs<List<string>> e)
	{
		if (e is null || e.Data is null || e.Data.Count == 0)
		{
			return;
		}
		if (!_messageService.IsStatusMessagePageOpen)
		{
			MessageIconSource = HAS_MESSAGE_ICON;
		}
	}

	private void MrsEnter()
	{
		Task.Run(() =>
		{
			try
			{
				var (systemDisk, appDisk, dataDisk) = _offlineTaskProxyService.GetDiskInfo();
				if (systemDisk != null && appDisk != null && dataDisk != null)
				{
					//MRS磁盘报警信息用MCS配置的数值，就不要再配置这些东西了
					var minFreeSpace = Math.Min(Math.Min(systemDisk.FreeSpaceRate, appDisk.FreeSpaceRate), dataDisk.FreeSpaceRate);
					var tmp = (int)_specialDiskService.GetWarnLevel((100 - minFreeSpace));

					_logger.LogInformation($"MRS raw free space tip level : {tmp}");
					Application.Current?.Dispatcher?.Invoke(() =>
					{
						RawFreeSpaceTip = tmp;

						MrsFreeSpace =
							$"MRS system disk {systemDisk.FreeSpaceRate:N1}% , app disk {appDisk.FreeSpaceRate:N1}% , data disk {dataDisk.FreeSpaceRate:N1}%";
					});
				}
				else
				{
					Application.Current?.Dispatcher?.Invoke(() =>
					{
						MrsFreeSpace = "MRS free disk is not available!";
					});
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Get mrs free disk error {ex.Message} - {ex.StackTrace}");
				Application.Current?.Dispatcher?.Invoke(() =>
				{
					MrsFreeSpace = "MRS free disk error!";
				});
			}
		});
	}

	private void McsEnter()
	{
		if (_specialDiskService.E is not null && _specialDiskService.F is not null)
		{
			McsHardFreeSpace = $"MCS free space E available {_specialDiskService.EFreeSpaceRate:N1}%, F available {_specialDiskService.FFreeSpaceRate:N1}%";
		}
		else
		{
			McsHardFreeSpace = "MCS free disk is not available!";
		}
	}

	private void SpecialDiskService_FreeDiskSpaceWarnedEvent(object? sender, DiskSpaceWarnLevel warnLevel)
	{
		_logger.LogInformation($"disk space warn event received,warn level:{warnLevel}");

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			McsFreeSpaceWarnLevel = (int)warnLevel;
		});
	}

	[UIRoute]
	private void OnNotifyMessage(object? sender, MessageInfo e)
	{
		if (e is not null)
		{
			return;
		}
		if (!_messageService.IsStatusMessagePageOpen)
		{
			MessageIconSource = HAS_MESSAGE_ICON;
		}
	}

	private void OnHeatCapacityChanged(object? sender, List<SystemInterface.MRSIntegration.Contract.Models.Tube> e)
	{
		SetTubeHeatCapacityStatus(e);
	}

	private void OnApplicationDoorStatusChanged(object? sender, bool e)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			IsOpenDoor = !e;
		});
	}

	private void OnControlBoxStatusChanged(object? sender, PartStatus newStatus)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			ControlBoxConnectStatus = newStatus != PartStatus.Disconnection;
		});
	}

	private void OnConnectionStatusChanged(object? sender, CTS.EventArgs<ServiceStatusInfo> e)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			IsOfflineConnected = e.Data.Connected;
		});
	}

	/// <summary>
	/// 0-10  orange
	/// 10-80 Green
	/// 80-90 orange
	/// 90-100 red
	/// </summary>
	/// <param name="e"></param>
	private void SetTubeHeatCapacityStatus(List<SystemInterface.MRSIntegration.Contract.Models.Tube> e)
	{
		if (e.Count > 0)
		{
			float maxHeatCapacity = e.Max(h => h.RaySource.HeatCapacity);
			float minHeatCapacity = e.Min(t => t.RaySource.HeatCapacity);
			double min = SystemConfig.SourceComponentConfig.SourceComponent.PreheatCapThreshold.Value;
			double max = SystemConfig.SourceComponentConfig.SourceComponent.AlertHeatCapThreshold.Value;
			double ora = SystemConfig.SourceComponentConfig.SourceComponent.NotifyHeatCapThreshold.Value;
			double displayValue;
			if (maxHeatCapacity >= max || minHeatCapacity < min)
			{
				TubeStatusLevel = 2;
				if (maxHeatCapacity >= max)
				{
					displayValue = maxHeatCapacity;
					TubeToolTip = $"Tube Capacity:{displayValue:N1}%";
				}
				else if (minHeatCapacity < min)
				{
					displayValue = minHeatCapacity;
					TubeToolTip = $"Tube Capacity:{displayValue:N1}%";
				}
			}
			else if (maxHeatCapacity >= ora)
			{
				TubeStatusLevel = 1;
				displayValue = maxHeatCapacity;
				TubeToolTip = $"Tube Capacity:{displayValue:N1}%";
			}
			else
			{
				TubeStatusLevel = 0;
				TubeToolTip = $"Tube Capacity:normal ({maxHeatCapacity:N1}%)";
			}
		}
	}

	private void PopupMessageWindow()
	{
		var messageNoticeWindow = CTS.Global.ServiceProvider?.GetRequiredService<MessagesWindow>();
		if (messageNoticeWindow is null)
		{
			return;
		}
		MessageIconSource = NO_MESSAGE_ICON;
		_messageService.StatusMessagePageOpen(true.ToString());
		messageNoticeWindow.WindowStartupLocation = WindowStartupLocation.Manual;
		messageNoticeWindow.Left = SystemParameters.WorkArea.Size.Width - messageNoticeWindow.Width - 150;
		messageNoticeWindow.Top = SystemParameters.WorkArea.Size.Height - messageNoticeWindow.Height - 10;

		var wih = new WindowInteropHelper(messageNoticeWindow);
		if (ConsoleSystemHelper.WindowHwnd != IntPtr.Zero)
		{
			if (wih.Owner == IntPtr.Zero)
				wih.Owner = ConsoleSystemHelper.WindowHwnd;

			if (!messageNoticeWindow.IsVisible)
			{
				//隐藏底部状态栏
				messageNoticeWindow.Topmost = true;
				messageNoticeWindow.ShowDialog();
			}
		}
		else
		{
			if (Application.Current.MainWindow is not null && wih.Owner == IntPtr.Zero)
			{
				wih.Owner = new WindowInteropHelper(Application.Current.MainWindow).Handle;
			}
			if (!messageNoticeWindow.IsVisible)
			{
				//必须加这个，不加就会导致状态栏出来
				messageNoticeWindow.Topmost = true;
				messageNoticeWindow.Show();
				messageNoticeWindow.Activate();
			}
		}
	}
}
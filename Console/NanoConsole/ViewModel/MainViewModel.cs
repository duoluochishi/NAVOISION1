//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ErrorCodes;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Models.MouseKeyboard;
using NV.CT.UI.Controls.Common;
namespace NV.CT.NanoConsole.ViewModel;

public class MainViewModel : BaseViewModel
{
	private readonly Dictionary<int, ViewHost> _viewHostDictionary = new();
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly IRealtimeStatusProxyService _realtimeStatusProxyService;
	private readonly IRealtimeConnectionService _realtimeConnectionService;
	private readonly List<SubProcessSetting> _subProcesses;
	private readonly ILogger<MainViewModel> _logger;
	private readonly ILayoutManager _layoutManager;
	private readonly IDialogService _dialogService;
	private readonly IWorkflow _workflow;
	private readonly IInputListener _inputListener;
	private readonly IUserService _userService;
	private bool _isInEmergency;

	public MainViewModel(IConsoleApplicationService consoleAppService, IOptions<List<SubProcessSetting>> subProcesses
		, IRealtimeStatusProxyService realtimeStatusProxyService, ILogger<MainViewModel> logger
		, ILayoutManager layoutManager, IDialogService dialogService, IInputListener inputListener, IUserService userService, IWorkflow workflow, IRealtimeConnectionService realtimeConnectionService)
	{
		_logger = logger;
		_workflow = workflow;
		_userService = userService;
		_layoutManager = layoutManager;
		_inputListener = inputListener;
		_realtimeStatusProxyService = realtimeStatusProxyService;
		_consoleAppService = consoleAppService;
		_subProcesses = subProcesses.Value;
		_dialogService = dialogService;
		_realtimeConnectionService = realtimeConnectionService;

		_inputListener.IdleTimeOccured += InputListener_IdleTimeOccured;
		_workflow.UnlockScreenChanged += Workflow_UnlockScreenChanged;
		_workflow.LockScreenChanged += Workflow_LockScreenChanged;
		_consoleAppService = CTS.Global.ServiceProvider.GetRequiredService<IConsoleApplicationService>();

		_realtimeStatusProxyService.EmergencyStopped += RealtimeStatusProxyService_EmergencyStopped;
		_realtimeStatusProxyService.RealtimeStatusChanged += RealtimeStatusProxyService_RealtimeStatusChanged;
		_realtimeStatusProxyService.DeviceErrorOccurred += DeviceErrorOccurred;
		_realtimeConnectionService.DeviceConnectionStatusChanged += DeviceConnectionStatusChanged;
		_realtimeConnectionService.ReconConnectionStatusChanged += ReconConnectionStatusChanged;

		//_realtimeStatusProxyService.ErrorStopped += ErrorStopped;
		_consoleAppService.UiApplicationActiveStatusChanged += ConsoleAppServiceUiAppActiveStatusChanged;

		_layoutManager.LayoutChanged += LayoutManager_LayoutChanged;

		_userService.CurrentUserChanged += UserService_CurrentUserChanged;

		LoadFirstPage();
	}

	private void HandleDeviceDisconnected(bool deviceConnected)
	{
		if (!deviceConnected)
		{
			var errorMsg = "Device disconnected!";
			_logger.LogError(errorMsg);

			UIInvoke(() =>
			{
				var messageLevel = MessageLeveles.Error;
				var messageTitle = LanguageResource.Message_Error_Title;

				_dialogService.ShowDialog(false, messageLevel, messageTitle,
					errorMsg, _ => { }
					, ConsoleSystemHelper.WindowHwnd);
			});
		}
	}

	/// <summary>
	/// 扫描过程中硬件断连 , 通知走这里
	/// </summary>
	private void ReconConnectionStatusChanged(object? sender, bool deviceConnected)
	{
		HandleDeviceDisconnected(deviceConnected);
	}

	/// <summary>
	/// 非扫描过程中,硬件设备断连
	/// </summary>
	private void DeviceConnectionStatusChanged(object? sender, bool deviceConnected)
	{
		HandleDeviceDisconnected(deviceConnected);
	}

	/// <summary>
	/// 用户主动点击锁屏,此时定时器停止
	/// </summary>
	private void Workflow_LockScreenChanged(object? sender, EventArgs e)
	{
		if (!IsDevelopment)
		{
			_inputListener.Stop();
		}
	}

	/// <summary>
	/// 用户解锁了屏幕,此时定时器重新开始计时
	/// </summary>
	private void Workflow_UnlockScreenChanged(object? sender, string e)
	{
		if (!IsDevelopment)
		{
			_inputListener.Start();
		}
	}

	/// <summary>
	/// 用户未操作超时时间到,锁屏
	/// </summary>
	private void InputListener_IdleTimeOccured(object? sender, ListenerStatus e)
	{
		_workflow.LockScreen();
		_layoutManager.Goto(Screens.LockScreen);
	}

	private void UserService_CurrentUserChanged(object? sender, UserModel? e)
	{
		if (e != null && e.Behavior == UserBehavior.Login)
		{
			if (!IsDevelopment)
			{
				_inputListener.Start();
			}
		}
		else if (e != null && e.Behavior == UserBehavior.Logout)
		{
			if (!IsDevelopment)
			{
				_inputListener.Stop();
			}
		}
	}

	/// <summary>
	/// 设备错误
	/// </summary>
	private void DeviceErrorOccurred(object? sender, EventArgs<List<string>> e)
	{
		if (e is null || e.Data is null || e.Data.Count == 0)
		{
			return;
		}

		//处于"急停"状态,不再弹出错误弹窗
		if (_isInEmergency)
			return;

		StringBuilder sb = new StringBuilder();
		var maxLevel = ErrorLevel.None;
		foreach (var errorCode in e.Data)
		{
			var error = ErrorCodeHelper.GetErrorCode(errorCode);
			if (error is not null)
			{
				sb.AppendLine($"{error.Code}, Description:{error.Description};");
				if (error.Level > maxLevel)
				{
					maxLevel = error.Level;
				}
			}
		}
		if (sb.Length > 0)
		{
			var errorMsg = sb.ToString();

			_logger.LogError($"Console MainViewmodel DeviceErrorOccurred {errorMsg}");

			UIInvoke(() =>
			{
				var messageLevel = MessageLeveles.Info;
				var messageTitle = LanguageResource.Message_Info_Title;
				if (maxLevel is ErrorLevel.Error || maxLevel is ErrorLevel.Fatal)
				{
					messageLevel = MessageLeveles.Error;
					messageTitle = LanguageResource.Message_Error_Title;
				}
				else if (maxLevel is ErrorLevel.Warning)
				{
					messageLevel = MessageLeveles.Warning;
					messageTitle = LanguageResource.Message_Warning_Title;
				}

				_dialogService.ShowDialog(false, messageLevel, messageTitle,
					errorMsg, _ => { }
					, ConsoleSystemHelper.WindowHwnd);
			});
		}
	}

	private void LoadFirstPage()
	{
		ContentContainer = CTS.Global.ServiceProvider.GetService(typeof(WelcomeControl));
	}

	/// <summary>
	/// 根据Screen显示不同界面，不管是控件还是进程
	/// 控件走layout changed事件回掉进行渲染
	/// </summary>
	private void LayoutManager_LayoutChanged(object? sender, Screens e)
	{

		bool showTrivial = false;
		Type? type = null;
		if (e == Screens.Welcome)
		{
			type = typeof(WelcomeControl);
			showTrivial = false;
		}
		else if (e == Screens.SelfCheckSimple)
		{
			type = typeof(SelfCheckSimpleControl);
			showTrivial = false;
		}
		else if (e == Screens.SelfCheckDetail)
		{
			type = typeof(SelfCheckControl);
			showTrivial = false;
		}
		else if (e == Screens.Login)
		{
			type = typeof(LoginControl);
			showTrivial = false;
		}
		else if (e == Screens.SystemSetting)
		{
			type = typeof(SettingHomeControl);
			showTrivial = true;
		}
		else if (e == Screens.Home)
		{
			type = typeof(SystemHomeControl);
			showTrivial = true;
		}
		else if (e == Screens.Shutdown)
		{
			type = typeof(ShutdownControl);
			showTrivial = false;
		}
		else if (e == Screens.LockScreen)
		{
			type = typeof(LockControl);
			showTrivial = false;
		}
		else if (e == Screens.Emergency)
		{
			// 跳转到PB然后点击Emergency急诊
			_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER);
			// 进入急诊workflow
			_workflow.EnterEmergencyExam();

			showTrivial = true;
		}
		else if (e == Screens.Main)
		{
			//main是虚拟页面，默认加载PatientBrowser
			LoadPatientBrowser();
			showTrivial = true;
		}

		if (type is null)
			return;

		_consoleAppService.ToggleHeaderFooter(showTrivial);

		UIInvoke(() =>
		{
			ContentContainer = CTS.Global.ServiceProvider.GetService(type);
		});
	}

	public void LoadPatientBrowser()
	{
		_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER);
	}

	public void LoadConfigSettings()
	{
		_layoutManager.Goto(Screens.SystemSetting);
	}

	/// <summary>
	/// 主控台Main页面 根据进程打开状态变化来决定容器内容显示
	/// 进程走 UiApplicationActiveStatusChanged进行渲染
	/// </summary>
	private void ConsoleAppServiceUiAppActiveStatusChanged(object? sender, ControlHandleModel e)
	{
		_logger.LogDebug($"StartProcess show window:{JsonConvert.SerializeObject(e)}");

		var itemName = e.ItemName == ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME ? e.Parameters : e.ItemName;

		if (!_subProcesses.Where(t => t.StartPart == ProcessStartPart.Master.ToString()).Any(t => t.ProcessName == itemName))
			return;

		_logger.LogDebug($"StartProcess show window:{itemName}");

		if (e.ActiveStatus != ControlActiveStatus.Active)
			return;

		var appInfo = _consoleAppService.GetControlHandleModel(e.ItemName, e.Parameters);

		IntPtr intPtr = IntPtr.Zero;
		if (appInfo != null)
		{
			intPtr = appInfo.ControlHandle;
		}
		if (intPtr != IntPtr.Zero)
		{
			Application.Current.Dispatcher.BeginInvoke(() =>
			{
				//TODO: YM
				//var controlHandleModel = _consoleAppService.GetControlViewHost(e.ItemName, e.Parameters);

				var processId = appInfo.ProcessId;
				ViewHost viewHost;
				if (_viewHostDictionary.ContainsKey(processId))
				{
					viewHost = _viewHostDictionary[processId];
				}
				else
				{
					viewHost = new ViewHost(appInfo.ControlHandle);
					_viewHostDictionary[appInfo.ProcessId] = viewHost;
				}
				ContentContainer = viewHost;

			});
		}
	}

	/// <summary>
	/// content holder
	/// </summary>
	private object? _contentContainer;
	public object? ContentContainer
	{
		get => _contentContainer;
		set => SetProperty(ref _contentContainer, value);
	}


	#region 急停相关

	/// <summary>
	/// 急停恢复
	/// </summary>
	private void RealtimeStatusProxyService_RealtimeStatusChanged(object? sender, EventArgs<CTS.Models.RealtimeInfo> e)
	{
		switch (e.Data.Status)
		{
			case RealtimeStatus.Standby:
				Application.Current?.Dispatcher?.Invoke(() =>
				{
					_isInEmergency = false;
					_logger.LogDebug($"RealtimeStatusChanged to Standby,EmergencyWindow will dismiss , isInEmergency={_isInEmergency}");
					CTS.Global.ServiceProvider.GetService<EmergencyWindow>()?.Hide();
				});
				break;
		}
	}

	/// <summary>
	/// 处理急停
	/// </summary>
	private void RealtimeStatusProxyService_EmergencyStopped(object? sender, EventArgs<CTS.Models.RealtimeInfo> e)
	{
		_isInEmergency = true;

		_logger.LogDebug($"MainViewModel EmergencyStopped , isInEmergency={_isInEmergency}");

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			CTS.Global.ServiceProvider.GetService<EmergencyWindow>()?.Show();
		});
	}

	#endregion

	//private void ErrorStopped(object? sender, EventArgs<RealtimeInfo> e)
	//{
	//	if (e is null || e.Data is null)
	//	{
	//		return;
	//	}
	//	StringBuilder sb = new StringBuilder();
	//	foreach (var errorCode in e.Data.ErrorCodes)
	//	{
	//		var error = ErrorCodeHelper.GetErrorCode(errorCode);
	//		if (error is not null)
	//		{
	//			sb.AppendLine($"{error.Code},Description:{error.Description};");
	//		}
	//	}
	//	if (sb.Length > 0)
	//	{
	//		var errorMsg = sb.ToString();

	//		_logger.LogError($"Console MainViewmodel ErrorStopped {errorMsg}");

	//		UIInvoke(() =>
	//		{
	//			_dialogService.ShowDialog(false, MessageLeveles.Error, LanguageResource.Message_Error_Title, errorMsg, _ => { }
	//				, ConsoleSystemHelper.WindowHwnd);
	//		});
	//	}
	//}
}
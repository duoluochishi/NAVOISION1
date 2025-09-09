//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
namespace NV.CT.NanoConsole.ViewModel;

public class SettingHomeViewModel : BaseViewModel
{
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly IDialogService _dialogService;
	private readonly IAuthorization _authorization;
	public SettingHomeViewModel(IConsoleApplicationService consoleAppService, IDialogService dialogService, IOptions<List<SettingLinkItem>> settingLinkItems,
		IAuthorization authorization)
	{
		_consoleAppService = consoleAppService;
		SettingLinkListData = settingLinkItems.Value;
		_dialogService = dialogService;
		_authorization = authorization;
		ResourceDictionary rd = UI.Controls.LoadingResource.LoadingInControl()[1];
		for (int i = 0; i < SettingLinkListData.Count; i++)
		{
			SettingLinkListData[i].SetIcon(rd);
		}
		Commands.Add("OpenCommand", new DelegateCommand<string>(OpenCommand));
		Commands.Add("CloseCommand", new DelegateCommand<string>(CloseCommand));

		AdjustmentShow = Visibility.Hidden;
		PermissionVerification();

		_authorization.CurrentUserChanged += Authorization_CurrentUserChanged;
	}

	private void Authorization_CurrentUserChanged(object? sender, Models.UserModel? e)
	{
		PermissionVerification();
	}

	private void PermissionVerification()
	{
		if (SettingLinkListData is null || SettingLinkListData.Count == 0)
		{
			return;
		}
		var item = SettingLinkListData.FirstOrDefault(t => t.AppCode.Equals("DailyTools"));
		if (item is SettingLinkItem daily)
		{
			DailyPV(daily);
		}
		var protocolItem = SettingLinkListData.FirstOrDefault(t => t.AppCode.Equals("ProtocolManagement"));
		if (protocolItem is SettingLinkItem protocolManagement)
		{
			ProtocolManagementPV(protocolManagement);
		}
		var logItem = SettingLinkListData.FirstOrDefault(t => t.AppCode.Equals("LogManagement"));
		if (logItem is SettingLinkItem logManagement)
		{
			LogManagementPV(logManagement);
		}
		var serviceItem = SettingLinkListData.FirstOrDefault(t => t.AppCode.Equals("ServiceTools"));
		if (serviceItem is SettingLinkItem serviceTools)
		{
			ServiceToolsPV(serviceTools);
		}
		var userItem = SettingLinkListData.FirstOrDefault(t => t.AppCode.Equals("UserConfig"));
		if (userItem is SettingLinkItem userConfig)
		{
			UserConfigPV(userConfig);
		}
		var systemItem = SettingLinkListData.FirstOrDefault(t => t.AppCode.Equals("SystemConfig"));
		if (systemItem is SettingLinkItem systemConfig)
		{
			SystemConfigPV(systemConfig);
		}
	}

	private void DailyPV(SettingLinkItem settingLinkItem)
	{
		settingLinkItem.IsEnabled = _authorization.IsAuthorized(SystemPermissionNames.DEVICE_MAINTENANCE_DAILY_CALIBRATION);
	}

	private void ProtocolManagementPV(SettingLinkItem settingLinkItem)
	{
		settingLinkItem.IsEnabled = _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_PROTOCOL_MANAGEMENT);
	}

	//日志浏览不需要权限呢，管理权限在应用里体现
	private void LogManagementPV(SettingLinkItem settingLinkItem)
	{
		settingLinkItem.IsEnabled = true;
	}

	private void ServiceToolsPV(SettingLinkItem settingLinkItem)
	{
		settingLinkItem.IsEnabled = _authorization.IsAuthorized(SystemPermissionNames.DEVICE_MAINTENANCE_DAILY_CALIBRATION)
			|| _authorization.IsAuthorized(SystemPermissionNames.DEVICE_MAINTENANCE_DEVICE_MAINTAIN)
			|| _authorization.IsAuthorized(SystemPermissionNames.DEVICE_MAINTENANCE_BACKUP_RESTORE)
			|| _authorization.IsAuthorized(SystemPermissionNames.DEVICE_MAINTENANCE_SERVICE_MAINTAIN)
			|| _authorization.IsAuthorized(SystemPermissionNames.DEVICE_MAINTENANCE_DEVICE_TEST)
			|| _authorization.IsAuthorized(SystemPermissionNames.DEVICE_MAINTENANCE_DEV_DEBUG);
	}

	private void UserConfigPV(SettingLinkItem settingLinkItem)
	{
		settingLinkItem.IsEnabled = _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_IMAGE_TEXT_CONFIG)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_PATIENT_REGIST_CONFIG)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_FILM_CONFIG)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_WINDOWING_PRESET)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_AUTO_ARCHIVE_CONFIG)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_AUTO_PRINT_CONFIG)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_AUTO_DELELE_CONFIG)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_HOSPITAL_SETTING)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_MAINAE_CONFIG)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_DICOM_CONFIG)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_COMMON_SETTING)
			|| _authorization.IsAuthorized(SystemPermissionNames.CLINICAL_TOOL_LOG_MANAGEMENT);
	}

	private void SystemConfigPV(SettingLinkItem settingLinkItem)
	{
		settingLinkItem.IsEnabled = _authorization.IsAuthorized(SystemPermissionNames.SYSTEM_PARAM_SETTING_SYSTEM_CONFIG)
			|| _authorization.IsAuthorized(SystemPermissionNames.SYSTEM_PARAM_SETTING_MODE_TYPE_CONFIG);
	}

	private void OpenCommand(string applicationName)
	{
		try
		{
			if (applicationName == ApplicationParameterNames.APPLICATIONNAME_SERVICETOOLS || applicationName==ApplicationParameterNames.APPLICATIONNAME_DAILY)//Calibrations
			{
				var appInfo =
					_consoleAppService.GetControlHandleModel(ApplicationParameterNames.APPLICATIONNAME_EXAMINATION);
				if (appInfo != null && appInfo.ControlHandle!=IntPtr.Zero)
				{
					_dialogService.ShowDialog(false, MessageLeveles.Warning,
						LanguageResource.Message_Info_CloseWarningTitle,
						LanguageResource.Message_Warning_NoStartServiceTools,
						null,
						ConsoleSystemHelper.WindowHwnd);
					return;
				}

				//原代码,不需要这样判断了
				//if (_consoleAppService.IsExaminationOpened())
				//{
				//	_dialogService.ShowDialog(false, MessageLeveles.Warning,
				//		LanguageResource.Message_Info_CloseWarningTitle,
				//		LanguageResource.Message_Warning_NoStartServiceTools,
				//		null,
				//		ConsoleSystemHelper.WindowHwnd);
				//	return;
				//}
			}
			string processName = SettingLinkListData.FirstOrDefault(x => x.CommandParameters == applicationName).FileName.Replace("NV.CT.", "").Replace(".exe", "");

			_consoleAppService.StartApp(processName, processName == applicationName ? string.Empty : applicationName);

		}
		catch (Exception ex)
		{
			CTS.Global.Logger?.LogError(ex.Message, ex);
		}
	}

	private void CloseCommand(string parameter)
	{
		_consoleAppService.CloseApp(ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME, parameter);
	}

	private List<SettingLinkItem>? _settingLinkListData;
	public List<SettingLinkItem>? SettingLinkListData
	{
		get => _settingLinkListData;
		set => SetProperty(ref _settingLinkListData, value);
	}

	private Visibility _adjustmentShow = Visibility.Visible;
	public Visibility AdjustmentShow
	{
		get => _adjustmentShow;
		set => SetProperty(ref _adjustmentShow, value);
	}
}
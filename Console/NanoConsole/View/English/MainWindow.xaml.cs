//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.UI.Controls.Controls;

namespace NV.CT.NanoConsole.View.English;

public partial class MainWindow
{
	private readonly ILogger<MainWindow> _logger;
	private readonly MainViewModel _mainViewModel;
	private readonly IAuthorization? _authorization;
	private readonly LoadingWindow? _loadingWindow;
	private readonly IConsoleApplicationService? _consoleApplicationService;
	private readonly IApplicationCommunicationService? _applicationCommunicationService;
	public MainWindow()
	{
		InitializeComponent();

		_logger = CTS.Global.ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
		_mainViewModel = CTS.Global.ServiceProvider.GetRequiredService<MainViewModel>();
		_authorization = CTS.Global.ServiceProvider.GetService<IAuthorization>();
		_consoleApplicationService = CTS.Global.ServiceProvider.GetService<IConsoleApplicationService>();
		if (_consoleApplicationService != null)
			_consoleApplicationService.ToggleHeaderFooterChanged += ToggleHeaderFooterChanged;

		if (_authorization != null)
		{
			_authorization.CurrentUserChanged += Authorization_CurrentUserChanged;
		}

		_loadingWindow = CTS.Global.ServiceProvider.GetService<LoadingWindow>();
		_applicationCommunicationService = CTS.Global.ServiceProvider.GetService<IApplicationCommunicationService>();
		if (_applicationCommunicationService is not null)
		{
			_applicationCommunicationService.ApplicationStatusChanged += ApplicationStatusChanged;
		}

		header.Height = 0;
		footer.Height = 0;

		DataContext = _mainViewModel;

		Loaded += MainWindow_Loaded;

		Activated += MainWindow_Activated;
	}

	private void MainWindow_Activated(object? sender, EventArgs e)
	{
		_logger.LogInformation("[Active] NanoConsole activated");

		_consoleApplicationService?.SwitchToPlatform(ProcessStartPart.Master);
	}

	private void MainWindow_Loaded(object sender, RoutedEventArgs e)
	{
		SetHwndToAppService();
	}

	private void ToggleHeaderFooterChanged(object? sender, bool e)
	{
		if (e)
		{
			ShowTrivial();
		}
		else
		{
			HideTrivial();
		}
	}

	/// <summary>
	/// 用户登入和登出，显示和隐藏 header 和 footer
	/// </summary>
	private void Authorization_CurrentUserChanged(object? sender, Models.UserModel? e)
	{
		if (e is not null && _authorization?.GetCurrentUser() != null)
		{
			ShowTrivial();

			//TODO: 普通用户进检查，服务用户进配置界面
			//检查一遍这里的逻辑
			//File.AppendAllText(@"F:/log.txt", loginedUser?.RoleList.ToJson() ?? "");
			var loginedUser = _authorization.GetCurrentUser();
			if (loginedUser != null && loginedUser.RoleList.Count > 0 && loginedUser.RoleList.Exists(n => n.Name.ToLower().Contains("service") || n.Name.ToLower().Contains("device")))
			{
				_mainViewModel.LoadConfigSettings();

			}
			else
			{
				//var allApps = _applicationCommunicationService?.GetAllApplication();
				//var existPatientBrowser = allApps?.ApplicationList.Exists(n =>
				//	n.ProcessName.Contains(ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER));
				//if (existPatientBrowser is false)
				//{
				_mainViewModel.LoadPatientBrowser();
				//}
			}

		}
		else if (e is null)
		{
			//用户注销
			HideTrivial();
		}
	}

	private void HideTrivial()
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			header.Height = 0;
			footer.Height = 0;
		});
	}

	private void ShowTrivial()
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			header.Height = 40;
			footer.Height = 40;
		});
	}

	private void SetHwndToAppService()
	{
		var hwnd = (new WindowInteropHelper(this)).Handle;

		ConsoleSystemHelper.WindowHwnd = hwnd;
		Global.Instance.SetWindowHwnd(hwnd);
	}

	[UIRoute]
	private void ApplicationStatusChanged(object? sender, ApplicationResponse e)
	{
		try
		{
			//如果是要开在副控台的进程，不处理
			if (e.ProcessStartPart is ProcessStartPart.Auxilary)
				return;

			_logger.LogInformation($"Primary Screen received app event {e.ToJson()}");
			switch (e.Status)
			{
				case ProcessStatus.Starting:
					_loadingWindow?.ShowLoading(true);
					break;
				case ProcessStatus.Started:
					_loadingWindow?.HideLoading();
					break;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"Primary Screen received app error message : {ex.Message},StackTrace:{ex.StackTrace}");
		}
	}

	///// <summary>
	///// check if screen render strange
	///// </summary>
	//private void SetWindowDisplay()
	//{
	//	WindowState = WindowState.Normal;
	//	System.Drawing.Rectangle workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
	//	Left = workingArea.Location.X;
	//	Top = workingArea.Location.Y;
	//	Width = workingArea.Width;
	//	Height = workingArea.Height;
	//	WindowState = WindowState.Maximized;
	//}

}
using NV.CT.UI.Controls.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS.Models;
using NV.CT.WorkflowService.Contract;
using MessageBox = System.Windows.MessageBox;

namespace NV.CT.AuxConsole.View.English;

public partial class MainWindow
{
	private readonly ILogger<MainWindow> _logger;
	private readonly MainViewModel? _vm;
	private bool _doubleScreenState;
	private readonly IApplicationCommunicationService? _applicationCommunicationService;
	private readonly IConsoleApplicationService? _consoleApplicationService;
	private readonly IWorkflow? _workflow;
	private readonly LoadingWindow? _loadingWindow;
	private Screen CurrentScreen
	{
		get
		{
			if (Screen.AllScreens.Length > 1)
			{
				return Screen.AllScreens.FirstOrDefault(s => s != Screen.PrimaryScreen);
			}

			return Screen.AllScreens[0];
		}
	}

	private int CurrentScreenLeft => CurrentScreen.WorkingArea.Left;
	private int CurrentScreenWidth => CurrentScreen.WorkingArea.Width;
	private int CurrentScreenHeight => CurrentScreen.WorkingArea.Height;

	public MainWindow()
	{
		InitializeComponent();

		_logger = CTS.Global.ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
		_loadingWindow = CTS.Global.ServiceProvider.GetService<LoadingWindow>();
		_applicationCommunicationService = CTS.Global.ServiceProvider.GetService<IApplicationCommunicationService>();
		_workflow = CTS.Global.ServiceProvider.GetService<IWorkflow>();
		_consoleApplicationService = CTS.Global.ServiceProvider.GetService<IConsoleApplicationService>();

		Loaded += Window_Loaded;
		if (_workflow!=null)
		{
			_workflow.LockScreenChanged += LockScreenChanged;
			_workflow.UnlockScreenChanged += UnlockScreenChanged;
		}
		
		if (_applicationCommunicationService is not null)
		{
			_applicationCommunicationService.ApplicationStatusChanged += ApplicationStatusChanged;
		}

		_vm = CTS.Global.ServiceProvider.GetService<MainViewModel>();
		if (_vm != null)
			_vm.RefreshContentControlSize += RefreshContentControlSize;

		DataContext = _vm;

		Activated += MainWindow_Activated;
	}

	private void MainWindow_Activated(object? sender, EventArgs e)
	{
		_logger.LogInformation("[Active] AuxilaryConsole activated");

		_consoleApplicationService?.SwitchToPlatform(ProcessStartPart.Auxilary);
	}

	[UIRoute]
	private void UnlockScreenChanged(object? sender, string nextScreen)
	{
		LockScreen.Visibility = Visibility.Collapsed;
		MainScreen.Visibility = Visibility.Visible;
	}

	[UIRoute]
	private void LockScreenChanged(object? sender, EventArgs e)
	{
		LockScreen.Visibility = Visibility.Visible;
		MainScreen.Visibility= Visibility.Collapsed;
	}

	private void ApplicationStatusChanged(object? sender, ApplicationResponse e)
	{
		//如果是要开在主控台的进程，不处理
		if (e.ProcessStartPart is ProcessStartPart.Master)
			return;

		_logger.LogInformation($"Secondary Screen received app event {e.ToJson()}");
		switch (e.Status)
		{
			case ProcessStatus.Starting:
				_loadingWindow?.ShowLoading(false);
				break;
			case ProcessStatus.Started:
				_loadingWindow?.HideLoading();
				break;
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		SetWindowSizeAndLocation();

		var auxPtr = (new WindowInteropHelper(this)).Handle;
		Dispatcher.Invoke(() =>
		{
			ConsoleSystemHelper.WindowHwnd = auxPtr;
			Global.Instance.SetWindowHwnd(auxPtr);
			_vm?.LoadPatientManagement();
		});

		SetAuxScreen();
	}

	private void SetWindowSizeAndLocation()
	{
		WindowState = WindowState.Normal;
		Width = CurrentScreenWidth;
		Height = CurrentScreenHeight;
		Left = CurrentScreenLeft;
		Top = 0;
		WindowState = WindowState.Maximized;
	}

	private void SetAuxScreen()
	{
		Dispatcher.Invoke(() =>
		{
			switch (Screen.AllScreens.Length)
			{
				case > 1 when !_doubleScreenState:
					_doubleScreenState = true;
					SetWindowSizeAndLocation();
					//
					_vm?.LoadPatientManagement();
					break;
				case <= 1 when _doubleScreenState:
					SetWindowSizeAndLocation();
					_doubleScreenState = false;
					_vm?.LoadPatientManagement();
					break;
			}

			//Todo:上次窗口分辨率与本次不同的时候，重新加载PatientManagement
			//if((SystemParameters.PrimaryScreenWidth * SystemParameters.PrimaryScreenHeight) != PreResolution)
			//{
			//    _mainViewModel.LoadPatientManagement();
			//    PreResolution = CurrentScreenWidth * CurrentScreenHeight;
			//    //PreResolution = SystemParameters.PrimaryScreenWidth * SystemParameters.PrimaryScreenHeight;
			//}
		});
	}

	private void RefreshContentControlSize(object? sender, ControlHandleModel e)
	{
		Dispatcher.Invoke(() =>
		{
			var uc = ((FrameworkElement)controlContainer.Content);
			if (uc is null) return;
			uc.Width = controlContainer.ActualWidth;
			uc.Height = controlContainer.ActualHeight;
		});
	}

	//private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	//{
		//Dispatcher.BeginInvoke(() =>
		//{
		//	controlContainer.Width = grdContent.ActualWidth;
		//	controlContainer.Height = grdContent.ActualHeight;
		//});
	//}

}

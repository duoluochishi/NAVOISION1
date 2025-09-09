using NV.CT.CTS.Models;
using NV.CT.WorkflowService.Contract;
using Application = System.Windows.Application;

namespace NV.CT.AuxConsole.ViewModel;

public class MainViewModel : BaseViewModel
{
	private readonly IWorkflow _workflow;
	private readonly Dictionary<int, ViewHost> _viewHostDictionary = new();
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly List<SubProcessSetting> _subProcesses;
	private readonly List<string> _fixedApplication = new();

	public event EventHandler<ControlHandleModel>? RefreshContentControlSize;
	public MainViewModel(IConsoleApplicationService consoleAppService, IOptions<List<SubProcessSetting>> subProcesses,IWorkflow workflow)
	{
		_workflow = workflow;
		_consoleAppService = consoleAppService;
		_subProcesses = subProcesses.Value;
		_fixedApplication.Add(ApplicationParameterNames.APPLICATIONNAME_PATIENTMANAGEMENT);
		_fixedApplication.Add(ApplicationParameterNames.APPLICATIONNAME_JOBVIEWER);

		//_workflow.LockScreenChanged += LockScreenChanged;
		//_workflow.UnlockScreenChanged += UnlockScreenChanged;

		_consoleAppService = CTS.Global.ServiceProvider.GetRequiredService<IConsoleApplicationService>();

		_consoleAppService.UiApplicationActiveStatusChanged += ConsoleAppServiceUiAppActiveStatusChanged;

	}

	//private void UnlockScreenChanged(object? sender, EventArgs e)
	//{
	//	var b = 20;
	//}

	//private void LockScreenChanged(object? sender, EventArgs e)
	//{
	//	var a = 10;
	//}


	private void ConsoleAppServiceUiAppActiveStatusChanged(object? sender, ControlHandleModel e)
	{
		var itemName = e.ItemName != ApplicationParameterNames.APPLICATIONNAME_SERVICEFRAME ? e.ItemName : e.Parameters;

		if (!_subProcesses.Where(t => t.StartPart == ProcessStartPart.Auxilary.ToString())
				.Any(t => t.ProcessName == itemName)) return;
		if (e.ActiveStatus == ControlActiveStatus.Active)
		{
			//if (e.IsConsoleControl) 
			//	return;

			if (e.ControlModelType is ControlModelType.Control)
				return;


			//IntPtr intPtr = _consoleAppService.GetControlHwnd(e.ItemName, e.Parameters, e.IsConsoleControl);

			IntPtr intPtr= IntPtr.Zero;
			var appInfo = _consoleAppService.GetControlHandleModel(e.ItemName, e.Parameters);
			if (appInfo != null)
			{
				intPtr = appInfo.ControlHandle;
			}

			if (intPtr != IntPtr.Zero)
			{
				Application.Current?.Dispatcher?.Invoke(() =>
				{
					//TODO: YM
					//var vmodel = _consoleAppService.GetControlViewHost(e.ItemName, e.Parameters);
					if (appInfo != null)
					{
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
						NewControl = viewHost;
					}
					RefreshContentControlSize?.Invoke(this, e);
				});
			}

		}
		else if (e.ActiveStatus == ControlActiveStatus.None)
		{
			//NewControl = null;
		}
	}

	public void LoadPatientManagement()
	{
		_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_PATIENTMANAGEMENT);
	}

	private object? _newControl;
	public object? NewControl
	{
		get => _newControl;
		set => SetProperty(ref _newControl, value);
	}
}

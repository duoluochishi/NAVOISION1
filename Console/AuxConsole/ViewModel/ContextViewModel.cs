//using NV.CT.CTS.Models;

//namespace NV.CT.AuxConsole.ViewModel;

//public class ContextViewModel : BaseViewModel
//{
//	private readonly IConsoleApplicationService _consoleAppService;
//	private readonly List<SubProcessSetting> _subProcesses;
//	public event EventHandler? RefreshContentControlSize;
//	private readonly Dictionary<int, ViewHost> _viewHostDictionary = new();
//	public ContextViewModel(IConsoleApplicationService consoleAppService, IOptions<List<SubProcessSetting>> subProcesses)
//	{
//		_subProcesses = subProcesses.Value;
//		_consoleAppService = consoleAppService;
//		_consoleAppService.UiApplicationActiveStatusChanged += ConsoleAppServiceUiAppActiveStatusChanged;

//	}

//	public void LoadPatientManagement()
//	{
//		_consoleAppService.StartApp(ApplicationParameterNames.APPLICATIONNAME_PATIENTMANAGEMENT);
//	}

//	private void ConsoleAppServiceUiAppActiveStatusChanged(object? sender, ControlHandleModel e)
//	{
//		if (!(_subProcesses.Where(t => t.StartMode == "Auxilary").Any(t => t.ProcessName == e.ItemName)))
//		{
//			if (e.ActiveStatus == CTS.Enums.ControlActiveStatus.Active)
//			{
//				var appInfo = _consoleAppService.GetControlHandleModel(e.ItemName, e.Parameters);
//				IntPtr intPtr=IntPtr.Zero;
//				if (appInfo != null)
//				{
//					intPtr = appInfo.ControlHandle;
//				}

//				if (intPtr != IntPtr.Zero)
//				{
//					NewContentControl = null;
//					//if (!e.IsConsoleControl)
//					if (e.ControlModelType is ControlModelType.Process)
//					{
//						Application.Current.Dispatcher.BeginInvoke(() =>
//						{
//							//TODO: YM
//							//var vmodel = _consoleAppService.GetControlViewHost(e.ItemName, e.Parameters);

//							var processId = appInfo.ProcessId;
//							ViewHost viewHost;
//							if (_viewHostDictionary.ContainsKey(processId))
//							{
//								viewHost = _viewHostDictionary[processId];
//							}
//							else
//							{
//								viewHost = new ViewHost(appInfo.ControlHandle);
//								_viewHostDictionary[appInfo.ProcessId] = viewHost;
//							}
//							NewContentControl = viewHost;

//							RefreshContentControlSize?.Invoke(this, null);
//						});
//					}
//				}
//			}
//			else if (e.ActiveStatus == CTS.Enums.ControlActiveStatus.None)
//			{
//				NewContentControl = null;
//			}
//		}
//	}

//	private object? _newContentControl;
//	public object? NewContentControl
//	{
//		get => _newContentControl;
//		set => SetProperty(ref _newContentControl, value);
//	}
//}

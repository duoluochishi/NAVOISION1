using NV.CT.CTS.Models;

namespace NV.CT.AuxConsole.ViewModel;

public class BackendFixedItemsViewModel : BaseViewModel
{
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly ILogger<BackendFixedItemsViewModel> _logger;
	public BackendFixedItemsViewModel(IConsoleApplicationService consoleAppService,ILogger<BackendFixedItemsViewModel> logger)
	{
		_logger=logger;
		_consoleAppService = consoleAppService;

		_consoleAppService.UiApplicationActiveStatusChanged += UiApplicationActiveStatusChanged;

		Commands.Add("OpenCommand", new DelegateCommand<string>(OpenCommand));
	}

	private void UiApplicationActiveStatusChanged(object? sender, ControlHandleModel e)
	{
		_logger.LogInformation($"ui active changed {e.ToJson()}");

		if (e.ProcessStartContainer is ProcessStartPart.Master)
			return;

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			JobViewerCardActive = e is { ItemName: ApplicationParameterNames.APPLICATIONNAME_JOBVIEWER, ActiveStatus: ControlActiveStatus.Active } ;
		});
	}

	private void OpenCommand(string applicationName)
	{
		if (!string.IsNullOrEmpty(applicationName))
		{
			_consoleAppService.StartApp(applicationName);
		}
	}

	private bool _jobViewerCardActive;
	public bool JobViewerCardActive
	{
		get=> _jobViewerCardActive;
		set => SetProperty(ref _jobViewerCardActive, value);
	}
}
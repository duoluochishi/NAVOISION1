using NV.CT.CTS.Models;

namespace NV.CT.AuxConsole.ViewModel;

public class FrontendFixedItemsViewModel : BaseViewModel
{
	private readonly IConsoleApplicationService _consoleAppService;
	private readonly ILogger<FrontendFixedItemsViewModel> _logger;
	public FrontendFixedItemsViewModel(IConsoleApplicationService consoleAppService, ILogger<FrontendFixedItemsViewModel> logger)
	{
		_logger = logger;
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
			PatientManagementActive = e is { ItemName: ApplicationParameterNames.APPLICATIONNAME_PATIENTMANAGEMENT, ActiveStatus: ControlActiveStatus.Active };
		});
	}

	private void OpenCommand(string applicationName)
	{
		if (!string.IsNullOrEmpty(applicationName))
		{
			_consoleAppService.StartApp(applicationName);
		}
	}

	private bool _patientManagementActive;
	public bool PatientManagementActive
	{
		get => _patientManagementActive;
		set => SetProperty(ref _patientManagementActive, value);
	}
}

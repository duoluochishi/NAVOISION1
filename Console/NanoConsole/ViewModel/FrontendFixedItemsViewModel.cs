//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.ViewModel;

public class FrontendFixedItemsViewModel : BaseViewModel
{
	private readonly ILogger<FrontendFixedItemsViewModel>? _logger;
	private readonly IConsoleApplicationService _consoleAppService;
	public FrontendFixedItemsViewModel(IConsoleApplicationService consoleAppService, ILogger<FrontendFixedItemsViewModel> logger)
	{
		_logger = logger;
		_consoleAppService = consoleAppService;
		
		Commands.Add("OpenCommand", new DelegateCommand<string>(OpenCommand));
		
		_consoleAppService.UiApplicationActiveStatusChanged += UiApplicationActiveStatusChanged;
	}

	private void UiApplicationActiveStatusChanged(object? sender, ControlHandleModel e)
	{
		if (e.ProcessStartContainer is ProcessStartPart.Auxilary)
			return;

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			PatientBrowserCardActive = e.ItemName == ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER &&
			                           e.ActiveStatus == ControlActiveStatus.Active;

			HomeCardActive=e.ItemName==Screens.Home.ToString() && e.ActiveStatus== ControlActiveStatus.Active;
		});
	}

	private void OpenCommand(string applicationName)
	{
		try
		{
			switch (applicationName)
			{
				case ApplicationParameterNames.NANOCONSOLE_CONTENTWINDOW_HOME:
					_consoleAppService.StartApp(Screens.Home);

					break;
				case ApplicationParameterNames.APPLICATIONNAME_PATIENTBROWSER:
					_consoleAppService.StartApp(applicationName);
					break;
			}
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex.Message, ex);
		}
	}

	private bool _homeCardActive;
	public bool HomeCardActive
	{
		get => _homeCardActive;
		set => SetProperty(ref _homeCardActive, value);
	}

	private bool _patientBrowserCardActive;
	public bool PatientBrowserCardActive
	{
		get => _patientBrowserCardActive;
		set => SetProperty(ref _patientBrowserCardActive, value);
	}

}
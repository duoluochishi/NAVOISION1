namespace NV.CT.NanoConsole.ViewModel;

public class ShutdownViewModel : BaseViewModel
{
	private readonly ILayoutManager _layoutManager;
	private readonly IShutdownService _shutdownService;
	private readonly ILogger<ShutdownViewModel> _logger;
	private readonly IConsoleApplicationService _consoleApplicationService;
	public ShutdownViewModel(ILogger<ShutdownViewModel> logger, IShutdownService shutdownService, ILayoutManager layoutManager, IConsoleApplicationService consoleApplicationService)
	{
		_logger = logger;
		_layoutManager = layoutManager;
		_shutdownService = shutdownService;
		_consoleApplicationService = consoleApplicationService;

		Commands.Add("CancelCommand", new DelegateCommand(CancelCommand));
		Commands.Add("ForceShutdownCommand", new DelegateCommand(ForceShutdownCommand));

		Init();
	}

	private void CancelCommand()
	{
		_layoutManager.Back();
		_consoleApplicationService.ToggleHeaderFooter(true);
	}

	private void ForceShutdownCommand()
	{
		//force shutdown
	}

	private void Init()
	{
		var listCanShutdown = _shutdownService.CanShutdown();

		_logger.LogInformation($"can shutdown result {listCanShutdown.ToJson()}");

		//TODO
		CanShutdown = listCanShutdown.All(n => n.Status == CommandExecutionStatus.Success);

		_logger.LogInformation($"can shutdown result is {CanShutdown}");
		if (CanShutdown)
		{
			_shutdownService.Shutdown();
		}
		else
		{
			var preventShutdownList =
				listCanShutdown.Where(n => n.Status != CommandExecutionStatus.Success).SelectMany(n => n.Details).ToList();

			_logger.LogInformation($"can shutdown false,prevent task list is {preventShutdownList.ToJson()}");
		}
	}

	private bool _canShutdown;
	public bool CanShutdown
	{
		get => _canShutdown;
		set
		{
			SetProperty(ref _canShutdown, value);
			Title = value ? "SHUTTING DOWN" : "ALSO NEED TO CLOSE A TASK";
		}
	}

	private string _title = string.Empty;
	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	/// <summary>
	/// 所有阻塞项
	/// </summary>
	private ObservableCollection<PreventItem> _preventItems = new();
	public ObservableCollection<PreventItem> PreventItems
	{
		get => _preventItems;
		set => SetProperty(ref _preventItems, value);
	}

	private string _copyright = $"COPYRIGHT \u00a9{DateTime.Now.Year} COMPOUND EYE CT ALL RIGHTS RESERVED.";
	public string Copyright
	{
		get => _copyright;
		set => SetProperty(ref _copyright, value);
	}
}
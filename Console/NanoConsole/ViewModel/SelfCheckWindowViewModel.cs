//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.ViewModel;

public class SelfCheckWindowViewModel : BaseViewModel
{
	private readonly ILogger<SelfCheckWindowViewModel> _logger;
	private readonly IConsoleApplicationService _consoleApplicationService;

	public SelfCheckWindowViewModel(ILogger<SelfCheckWindowViewModel> logger, IConsoleApplicationService consoleApplicationService
		)
	{
		_logger = logger;
		_consoleApplicationService = consoleApplicationService;
		_consoleApplicationService.ShowSelfCheckSummaryStarted += ShowSelfCheckWindowStarted;

		Commands.Add("CloseCommand", new DelegateCommand<object>(CloseClicked, _ => true));
		Commands.Add("DragMoveCommand", new DelegateCommand<object>(DragMove, _ => true));
	}

	/// <summary>
	/// 显示自检汇总页面,重新调用一次自检结果
	/// </summary>
	private void ShowSelfCheckWindowStarted(object? sender, EventArgs e)
	{
		_logger.LogInformation($"show self check summary started");
		Application.Current?.Dispatcher?.Invoke(() =>
		{
		});
	}

	public void DragMove(object parameter)
	{
		if ((parameter as Window) is not null)
		{
			((Window)parameter).DragMove();
		}
	}

	public void CloseClicked(object parameter)
	{
		if (parameter is Window window)
		{
			window.Hide();
		}
	}
}
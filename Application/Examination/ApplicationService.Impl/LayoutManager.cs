using Microsoft.Extensions.Logging;

using NV.CT.CTS;
using NV.CT.SyncService.Contract;

namespace NV.CT.Examination.ApplicationService.Impl;

public class LayoutManager : ILayoutManager
{
	private readonly ILogger<LayoutManager> _logger;
	public ScanTaskAvailableLayout PreviousLayout { get; set; }
	public ScanTaskAvailableLayout CurrentLayout { get; set; }

	private readonly IScreenSync _screenSync;

	public LayoutManager(ILogger<LayoutManager> logger, IScreenSync screenSync)
	{
		_logger = logger;
		_screenSync = screenSync;
	}

	public void SwitchToView(ScanTaskAvailableLayout layout)
	{
		_logger.LogInformation($"LayoutManager current is {CurrentLayout}, switch to {layout} view");

		PreviousLayout = CurrentLayout;
		CurrentLayout = layout;
		LayoutChanged?.Invoke(this, new EventArgs<ScanTaskAvailableLayout>(layout));

		//TODO:need to check
		//Exam进程里面只有这两个页面需要同步
		if (layout == ScanTaskAvailableLayout.ScanDefault || layout == ScanTaskAvailableLayout.ProtocolSelection)
		{
			_screenSync.SwitchTo(layout.ToString());
		}
	}

	public void Back()
	{
		_logger.LogInformation($"LayoutManager back to {PreviousLayout}");
		CurrentLayout = PreviousLayout;
		LayoutChanged?.Invoke(this, new EventArgs<ScanTaskAvailableLayout>(PreviousLayout));
	}

	public event EventHandler<EventArgs<ScanTaskAvailableLayout>>? LayoutChanged;
}
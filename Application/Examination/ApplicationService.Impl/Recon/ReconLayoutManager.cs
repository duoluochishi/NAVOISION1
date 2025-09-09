using Microsoft.Extensions.Logging;
using NV.CT.CTS;

namespace NV.CT.Examination.ApplicationService.Impl.Recon;

public class ReconLayoutManager : ILayoutManager
{
	private readonly ILogger<ReconLayoutManager> _logger;
	public ScanTaskAvailableLayout PreviousLayout { get; set; }
	public ScanTaskAvailableLayout CurrentLayout { get; set; }

	public event EventHandler<EventArgs<ScanTaskAvailableLayout>>? LayoutChanged;
	public ReconLayoutManager(ILogger<ReconLayoutManager> logger)
	{
		_logger = logger;
		//默认进入MRP重建页面，也就是Recon页面
		CurrentLayout = ScanTaskAvailableLayout.Recon;
	}
	public void Back()
	{
		_logger.LogInformation($"ReconLayoutManager back to {PreviousLayout}");
		CurrentLayout = PreviousLayout;
		LayoutChanged?.Invoke(this, new EventArgs<ScanTaskAvailableLayout>(PreviousLayout));
	}

	public void SwitchToView(ScanTaskAvailableLayout layout)
	{
		_logger.LogInformation($"ReconLayoutManager current layout is {CurrentLayout}, switch to {layout} view");

		PreviousLayout = CurrentLayout;
		CurrentLayout = layout;
		LayoutChanged?.Invoke(this, new EventArgs<ScanTaskAvailableLayout>(layout));
	}
}

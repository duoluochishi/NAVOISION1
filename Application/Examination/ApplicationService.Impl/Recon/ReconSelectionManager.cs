using Microsoft.Extensions.Logging;

namespace NV.CT.Examination.ApplicationService.Impl.Recon;

public class ReconSelectionManager : SelectionManager
{
	public ReconSelectionManager(IProtocolHostService protocolHostService, ILogger<SelectionManager> logger) : base(protocolHostService, logger)
	{
	}
}
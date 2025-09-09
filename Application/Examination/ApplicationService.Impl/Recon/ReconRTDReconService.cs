using Microsoft.Extensions.Logging;

using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Impl.Recon;

public class ReconRTDReconService : RTDReconService
{
	public ReconRTDReconService(ILogger<RTDReconService> logger, IRealtimeReconProxyService realtimeProxyService) : base(logger, realtimeProxyService)
	{
	}
}

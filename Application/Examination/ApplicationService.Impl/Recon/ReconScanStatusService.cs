using Microsoft.Extensions.Logging;

using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Impl.Recon;

public class ReconScanStatusService : ScanStatusService
{
	public ReconScanStatusService(ILogger<ScanStatusService> logger, IRealtimeReconProxyService realtimeProxyService) : base(logger, realtimeProxyService)
	{
	}
}

using Microsoft.Extensions.Logging;

namespace NV.CT.Examination.ApplicationService.Impl.Recon;

public class ReconMeasurementStatusService : MeasurementStatusService
{
	public ReconMeasurementStatusService(ILogger<MeasurementStatusService> logger, IProtocolHostService protocolHostService) : base(logger, protocolHostService)
	{
	}
}

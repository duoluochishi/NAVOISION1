using Microsoft.Extensions.Logging;

using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.Examination.ApplicationService.Impl.Recon;

public class ReconScanControlService : ScanControlService
{
	public ReconScanControlService(ILogger<ScanControlService> logger, IMapper mapper, IStudyHostService studyHost, IProtocolHostService protocolHostService, IScanStatusService scanStatusService, IMeasurementStatusService measurementStatusService, IRealtimeReconProxyService realtimeProxyService, IAuthorization authorizationService) : base(logger, mapper, studyHost, protocolHostService, scanStatusService, measurementStatusService, realtimeProxyService, authorizationService)
	{
	}
}

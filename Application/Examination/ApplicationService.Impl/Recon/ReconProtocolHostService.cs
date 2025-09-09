using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Impl.Recon;

public class ReconProtocolHostService : ProtocolHostService
{
	public ReconProtocolHostService(IProtocolStructureService protocolStructureService, IProtocolModificationService protocolModificationService, IProtocolPerformStatusService protocolPerformStatusService) : base(protocolStructureService, protocolModificationService, protocolPerformStatusService)
	{
	}
}

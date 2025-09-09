using NV.MPS.UI.Dialog.Service;

namespace NV.CT.Examination.ApplicationService.Impl.Recon;

public class ReconImageOperationService : ImageOperationService
{
	public ReconImageOperationService(IProtocolHostService protocolHostService, IDialogService dialogService) : base(protocolHostService, dialogService)
	{
	}
}

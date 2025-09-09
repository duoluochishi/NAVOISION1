using Microsoft.Extensions.Logging;

using NV.CT.Examination.ApplicationService.Contract.ScanControl.Rule;

namespace NV.CT.Examination.ApplicationService.Impl.ScanControl;

public class ReconAllButtonControlEnableService : ButtonControlEnableService
{
    private readonly ILogger<ReconAllButtonControlEnableService> _logger;
    public ReconAllButtonControlEnableService(ILogger<ReconAllButtonControlEnableService> logger, HasUnFinishedRTDReconRule hasUnFinishedRtdReconRule, OfflineReadyRule offlineReadyRule) : base(logger)
    {
        _logger = logger;

        RegisterRule(offlineReadyRule);
        RegisterRule(hasUnFinishedRtdReconRule);
    }
}
using Microsoft.Extensions.Logging;

namespace NV.CT.Examination.ApplicationService.Impl.ScanControl;

public class CancelButtonControlEnableService : ButtonControlEnableService
{
    private readonly ILogger<CancelButtonControlEnableService> _logger;
    public CancelButtonControlEnableService(ILogger<CancelButtonControlEnableService> logger) : base(logger)
    {
        _logger = logger;
    }
}

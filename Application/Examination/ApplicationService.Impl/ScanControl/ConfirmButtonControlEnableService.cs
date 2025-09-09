using Microsoft.Extensions.Logging;

namespace NV.CT.Examination.ApplicationService.Impl.ScanControl;

public class ConfirmButtonControlEnableService : ButtonControlEnableService
{
    private readonly ILogger<ConfirmButtonControlEnableService> _logger;
    public ConfirmButtonControlEnableService(ILogger<ConfirmButtonControlEnableService> logger) : base(logger)
    {
        _logger = logger;
    }
}

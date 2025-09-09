using Microsoft.Extensions.Logging;

using NV.CT.Examination.ApplicationService.Contract.ScanControl.Rule;

namespace NV.CT.Examination.ApplicationService.Impl.ScanControl;

public class GoButtonControlEnableService : ButtonControlEnableService
{
	private readonly ILogger<GoButtonControlEnableService> _logger;
	public GoButtonControlEnableService(ILogger<GoButtonControlEnableService> logger, LayoutRule layoutRule, HasUnperformedScanRule hasUnperformedScanRule, SystemReadyRule systemReadyRule) : base(logger)
	{
		_logger = logger;

		RegisterRule(layoutRule);
		RegisterRule(hasUnperformedScanRule);

		//TODO:开发阶段，暂时不用这条规则，会导致界面看不到具体原因，为什么不能点Go按钮
		//RegisterRule(systemReadyRule);
	}
}

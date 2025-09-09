//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.CommonAttributeUI.AOPAttribute;

namespace NV.CT.Recon.ViewModel;

public class ReconScanControlsViewModel : ScanControlsViewModel
{
	private readonly ILogger<ScanControlsViewModel> _logger;
	private readonly IProtocolHostService _protocolHostService;
	private readonly ILayoutManager _layoutManager;
	private readonly IScreenManagement _screenManagement;
	private readonly IDialogService _dialogService;
	public ReconScanControlsViewModel(ILogger<ScanControlsViewModel> logger,
		ISelectionManager selectionManager,
		IScanControlService scanControlService,
		IUIRelatedStatusService uiRelatedStatusService,
		IStudyHostService studyHostService,
		ITablePositionService tablePositionService,
		IProtocolHostService protocolHostService,
		IOfflineReconService offlineReconService,
		IDialogService dialogService,
		ILayoutManager layoutManager,
		ISystemReadyService systemReadyService,
		IPageService pageService,
		IUIControlStatusService uiControlStatusService,
		IGoService goService,
		IScreenManagement screenManagement,
		IGoValidateDialogService goValidateDialogService,
		IApplicationCommunicationService applicationCommunicationService,
		IHeatCapacityService heatCapacityService,
		IFrontRearCoverStatusService frontRearCoverStatusService,
		IDetectorTemperatureService detectorTemperatureService,
		IRealtimeStatusProxyService realtimeStatusProxyService,
		IMapper mapper,
		IOfflineConnectionService offlineConnectionService,
		IDoseEstimateService doseEstimateService,
		IVoiceService voiceService) : base(logger, selectionManager, scanControlService, uiRelatedStatusService, studyHostService, protocolHostService, offlineReconService, dialogService, layoutManager, systemReadyService, pageService, uiControlStatusService, goService, screenManagement, goValidateDialogService, applicationCommunicationService, heatCapacityService, tablePositionService, frontRearCoverStatusService, detectorTemperatureService, realtimeStatusProxyService, mapper, offlineConnectionService, doseEstimateService,voiceService)
	{
		_logger = logger;
		_layoutManager = layoutManager;
		_protocolHostService = protocolHostService;
		_screenManagement = screenManagement;
		_dialogService = dialogService;

		//取消订阅协议结构变更事件，recover patient会引起界面变化
		_protocolHostService.StructureChanged -= ProtocolHostService_StructureChanged;
	}

	[UIRoute]
	public override void ValidateScanControl(string sourceMethod = "")
	{
		//不能简单调用，简单调用会频繁改变Go的状态，导致界面出现闪烁现象

		//override IsGoEnable property ,always false in [Recon] situation
		IsGoEnable = false;
		IsCancelEnable = false;

		//ReconAll可用条件：是ProtocolHost里面所有的Scan对应的 RTD已经完成，还有其他非RTD没完成的
		IsReconAllEnable = _protocolHostService.Models.Select(n => n.Scan).Any(n => n.Status == PerformStatus.Performed && n.Children.Any(recon => !recon.IsRTD && recon.Status == PerformStatus.Unperform));
	}

	public override void Close()
	{
		_logger.LogInformation($"Recon close");

		_layoutManager.SwitchToView(ScanTaskAvailableLayout.ScanDefault);
	}
}
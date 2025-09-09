//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2025/8/29 14:28:29     V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.UI.Exam.ViewModel;

public class UIRelatedStatusServiceExtension
{
	private readonly IProtocolHostService _protocolHostService;
	private readonly IUIRelatedStatusService _uIRelatedStatusService;
	private readonly ILogger<UIRelatedStatusServiceExtension> _logger;

	Stopwatch realtimeStatus = Stopwatch.StartNew();
	Stopwatch scanSW = Stopwatch.StartNew();

	private string RealtimeScanID = string.Empty;
	private string PerformScanID = string.Empty;
	public UIRelatedStatusServiceExtension(IUIRelatedStatusService uIRelatedStatusService,
		IProtocolHostService protocolHostService,
		ILogger<UIRelatedStatusServiceExtension> logger)
	{
		_uIRelatedStatusService = uIRelatedStatusService;
		_protocolHostService = protocolHostService;
		_logger = logger;
		_protocolHostService.PerformStatusChanged -= ProtocolHostService_PerformStatusChanged;
		_protocolHostService.PerformStatusChanged += ProtocolHostService_PerformStatusChanged;
		_uIRelatedStatusService.RealtimeStatusChanged -= UIRelatedStatusService_RealtimeStatusChanged;
		_uIRelatedStatusService.RealtimeStatusChanged += UIRelatedStatusService_RealtimeStatusChanged;
	}

	private void UIRelatedStatusService_RealtimeStatusChanged(object? sender, EventArgs<RealtimeInfo> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		var item = _protocolHostService.Models.FirstOrDefault(t => t.Scan.Descriptor.Id.Equals(e.Data.ScanId));
		if (item.Scan is null)
		{
			return;
		}
		double scanTime = ScanTimeHelper.GetScanTime(item.Scan);
		switch (e.Data.Status)
		{
			case RealtimeStatus.ExposureStarted:
				if (!RealtimeScanID.Equals(e.Data.ScanId))
				{
					realtimeStatus.Restart();
				}
				break;
			case RealtimeStatus.ExposureFinished:
				realtimeStatus.Stop();
				_logger.LogInformation($"Perform a scan exposure id {e.Data.ScanId} type {item.Scan.ScanOption.ToString()} took {realtimeStatus.ElapsedMilliseconds} ms");
				break;
			case RealtimeStatus.NormalScanStopped:
			case RealtimeStatus.EmergencyScanStopped:
			case RealtimeStatus.Error:
				realtimeStatus.Stop();
				_logger.LogInformation($"Perform a scanning id {e.Data.ScanId} type {item.Scan.ScanOption.ToString()} took {realtimeStatus.ElapsedMilliseconds} ms,Estimated time is:{scanTime} s");
				break;
			default:
				break;
		}
		RealtimeScanID = e.Data.ScanId;
	}

	private void ProtocolHostService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		if (e is null || e.Data.Model is null)
		{
			return;
		}
		if (e.Data.Model is ScanModel scan)
		{
			if (e.Data.NewStatus == PerformStatus.Performing)
			{
				if (!PerformScanID.Equals(scan.Descriptor.Id))
				{
					scanSW.Restart();
				}
			}
			if (e.Data.NewStatus == PerformStatus.Performed)
			{
				scanSW.Stop();
				double scanTime = ScanTimeHelper.GetScanTime(scan);
				_logger.LogInformation($"PerformStatusChanged a scanning task {scan.Descriptor.Id} type {scan.ScanOption.ToString()} took {scanSW.ElapsedMilliseconds} ms,Estimated time is:{scanTime} s");
			}
			PerformScanID = scan.Descriptor.Id;
		}
	}
}
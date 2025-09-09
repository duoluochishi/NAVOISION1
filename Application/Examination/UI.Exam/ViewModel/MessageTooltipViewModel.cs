//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;
using System.Windows.Threading;

namespace NV.CT.UI.Exam.ViewModel;

public class MessageTooltipViewModel : BaseViewModel
{
	private readonly IUIRelatedStatusService _uiRelatedStatusService;
	private readonly IScanControlService _scanControlService;
	private readonly ISystemReadyService _systemReadyService;
	private DispatcherTimer _timer;
	private int _duration = 30;
	private bool _isScanning = false;
	public bool IsScanning
	{
		get => _isScanning;
		set => SetProperty(ref _isScanning, value);
	}

	private bool _working = false;

	private bool _isBeforeScan;
	public bool IsBeforeScan
	{
		get => _isBeforeScan;
		set => SetProperty(ref _isBeforeScan, value);
	}

	private bool _isMoveTable;
	public bool IsMoveTable
	{
		get => _isMoveTable;
		set => SetProperty(ref _isMoveTable, value);
	}

	private string _scanMessage = string.Empty;
	public string ScanMessage
	{
		get => _scanMessage;
		set => SetProperty(ref _scanMessage, value);
	}

	public MessageTooltipViewModel(IUIRelatedStatusService uiRelatedStatusService,
		 IScanControlService scanControlService,
		 ISystemReadyService systemReadyService)
	{
		_uiRelatedStatusService = uiRelatedStatusService;
		_scanControlService = scanControlService;
		_systemReadyService = systemReadyService;
		_uiRelatedStatusService.RealtimeStatusChanged -= ExamStatusChanged;
		_uiRelatedStatusService.RealtimeStatusChanged += ExamStatusChanged;
		_uiRelatedStatusService.EmergencyStopped += UIRelatedStatusService_EmergencyStopped;
		_uiRelatedStatusService.ErrorStopped += UIRelatedStatusService_ErrorStopped;
		_systemReadyService.StatusChanged -= SystemReadyService_StatusChanged;
		_systemReadyService.StatusChanged += SystemReadyService_StatusChanged;
		_timer = new DispatcherTimer();
		_timer.Interval = TimeSpan.FromSeconds(1);
		_timer.Tick += Timer_Tick;
	}

	[UIRoute]
	private void SystemReadyService_StatusChanged(object? sender, EventArgs<(bool status, bool isSyatemStatus)> e)
	{
		if (e is not null && e.Data.isSyatemStatus)
		{
			if (!_working && !_systemReadyService.Status)
			{
				ScanMessage = _systemReadyService.LatestFailReason;
			}
			else
			{
				ScanMessage = string.Empty;
			}
		}
		ResetInitialStatus(string.Empty);
	}

	[UIRoute]
	private void UIRelatedStatusService_ErrorStopped(object? sender, EventArgs<RealtimeInfo> e)
	{
		ResetInitialStatus("Error Stopped!");
	}

	[UIRoute]
	private void UIRelatedStatusService_EmergencyStopped(object? sender, EventArgs<RealtimeInfo> e)
	{
		ResetInitialStatus("Emergency Stopped!");
	}

	private void ResetInitialStatus(string message)
	{
		IsScanning = false;
		IsBeforeScan = false;
		IsMoveTable = false;
		_duration = 30;
		_timer.Stop();
		ScanMessage = message;
	}

	private void Timer_Tick(object? sender, EventArgs e)
	{
		if (_duration >= 0)
		{
			_duration--;
			ScanMessage = $"{LanguageResource.Message_Info_ScanMessageExposureing}({_duration})";
		}
		if (_duration <= 0)
		{
			_scanControlService.CancelMeasurement();
			_timer.Stop();
		}
	}

	[UIRoute]
	private void ExamStatusChanged(object? sender, EventArgs<RealtimeInfo> e)
	{
		var realtimeInfo = e.Data;
		if (realtimeInfo is null) return;
		switch (realtimeInfo.Status)
		{
			case RealtimeStatus.None:
			case RealtimeStatus.Init:
				_working = false;
				break;
			case RealtimeStatus.Standby:
				_working = false;
				ResetInitialStatus(string.Empty);
				break;
			case RealtimeStatus.MovingPartEnable:
				IsScanning = false;
				IsBeforeScan = false;
				IsMoveTable = true;
				_working = true;
				ScanMessage = LanguageResource.Message_Info_ScanMessageMoveTable;
				break;
			case RealtimeStatus.ExposureEnable:
				IsBeforeScan = true;
				IsScanning = false;
				IsMoveTable = false;
				_working = true;
				_duration = 30;
				_timer.Start();
				ScanMessage = $"{LanguageResource.Message_Info_ScanMessageExposureing}({_duration})";
				break;
			case RealtimeStatus.ExposureStarted:
				ResetInitialStatus(string.Empty);
				_working = true;
				break;
			case RealtimeStatus.ExposureSpoting:
				IsBeforeScan = false;
				IsScanning = true;
				IsMoveTable = false;
				_working = true;
				ScanMessage = LanguageResource.Message_Info_ScanMessageScanning;
				break;
			case RealtimeStatus.ExposureSpotingIdle:
			case RealtimeStatus.ExposureFinished:
				_working = true;
				break;
			case RealtimeStatus.NormalScanStopped:
			case RealtimeStatus.EmergencyScanStopped:
			case RealtimeStatus.Error:
			default:
				_working = true;
				ResetInitialStatus(string.Empty);
				break;
		}
	}
}
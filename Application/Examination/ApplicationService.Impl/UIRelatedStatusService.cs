using NV.CT.CTS.Models;

namespace NV.CT.Examination.ApplicationService.Impl;

public class UIRelatedStatusService : IUIRelatedStatusService
{
	private readonly ISystemReadyService _systemReadyService;

	public UIRelatedStatusService(ISystemReadyService systemReadyService)
	{
		_systemReadyService = systemReadyService;

        _systemReadyService.DoorStatusChanged += SystemReadyService_DoorStatusChanged;
        _systemReadyService.RealtimeStatusChanged += SystemReadyService_RealtimeStatusChanged;
        _systemReadyService.CycleStatusChanged += SystemReadyService_CycleStatusChanged;
        _systemReadyService.EmergencyStopped += SystemReadyService_EmergencyStopped;
        _systemReadyService.ErrorStopped += SystemReadyService_ErrorStopped;
	}

	public event EventHandler<CTS.EventArgs<RealtimeInfo>>? RealtimeStatusChanged;
	public event EventHandler<CTS.EventArgs<DeviceSystem>>? CycleStatusChanged;
	public event EventHandler<CTS.EventArgs<RealtimeInfo>>? EmergencyStopped;
	public event EventHandler<CTS.EventArgs<RealtimeInfo>>? ErrorStopped;
	public event EventHandler<bool>? DoorStatusChanged;

	private void SystemReadyService_DoorStatusChanged(object? sender, bool e)
	{
		DoorStatusChanged?.Invoke(sender, e);
	}
	private void SystemReadyService_CycleStatusChanged(object? sender, CTS.EventArgs<DeviceSystem> e)
	{
		//TODO: 确认后再调整
		CycleStatusChanged?.Invoke(this, new CTS.EventArgs<DeviceSystem>(e.Data));
	}

	private void SystemReadyService_RealtimeStatusChanged(object? sender, CTS.EventArgs<RealtimeInfo> e)
	{
		//TODO: 确认后再调整
		RealtimeStatusChanged?.Invoke(this, new CTS.EventArgs<RealtimeInfo>(e.Data));
	}

	private void SystemReadyService_EmergencyStopped(object? sender, CTS.EventArgs<RealtimeInfo> e)
	{
		//TODO: 确认后再调整
		EmergencyStopped?.Invoke(this, e);
	}

	private void SystemReadyService_ErrorStopped(object? sender, CTS.EventArgs<RealtimeInfo> e)
	{
		////TODO:是否为开门(_doorStatusService.Status == false)
		//if (_realtimeStatusProxyService.PreviewStatus != FacadeProxy.Common.Enums.RealtimeStatus.Standby
		//    && !_doorStatusService.IsClosed)
		//{
		//    ErrorStopped?.Invoke(this, e);
		//}

		ErrorStopped?.Invoke(this, e);
	}

	public bool IsValidated { get; set; }

	public bool IsDoorClosed => _systemReadyService.CurrentDoorStatus;

	public void IsValidatedChanged(bool isValidated)
	{
		IsValidated = isValidated;
	}
}
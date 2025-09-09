//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/6/14 15:38:12     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Impl;

public class SystemReadyService : ISystemReadyService
{
	private readonly ILogger<SystemReadyService> _logger;

	private readonly IRealtimeConnectionService _realtimeConnectionService;

	private readonly IRealtimeStatusProxyService _realtimeStatusService;

	private readonly IDoorStatusService _doorStatusService;

	private readonly ICTBoxStatusService _ctboxStatusService;

	private readonly ITablePositionService _tablePositionService;

	public bool Status { get; private set; }
	public string LatestFailReason { get; private set; } = string.Empty;

	public event EventHandler<EventArgs<(bool status, bool isSyatemStatus)>>? StatusChanged;

    public event EventHandler<CTS.EventArgs<RealtimeInfo>>? RealtimeStatusChanged;
    public event EventHandler<CTS.EventArgs<DeviceSystem>>? CycleStatusChanged;
    public event EventHandler<CTS.EventArgs<RealtimeInfo>>? EmergencyStopped;
    public event EventHandler<CTS.EventArgs<RealtimeInfo>>? ErrorStopped;
    public event EventHandler<bool>? DoorStatusChanged;

    private bool _currentConnectionStatus = false;

	private bool _currentRealtimeStatus = false;

	private bool _currentDoorClosed = false;

	private bool _currentCTBoxStatus = false;

	private bool _currentTableStatus = false;

	private bool _currentTableLocked = false;

	public bool CurrentConnectionStatus => _currentConnectionStatus;

	public bool CurrentRealtimeStatus => _currentRealtimeStatus;

	public bool CurrentDoorStatus => _currentDoorClosed;

	public bool CurrentCTBoxStatus => _currentCTBoxStatus;

	public bool CurrentTableStatus => _currentTableStatus;

	public DeviceMonitorConfig _monitorConfig { get; set; }

	public SystemReadyService(ILogger<SystemReadyService> logger, IOptions<DeviceMonitorConfig> monitorConfig, IRealtimeConnectionService realtimeConnectionService, IRealtimeStatusProxyService realtimeStatusService, IDoorStatusService doorStatusService, ICTBoxStatusService ctboxStatusService, ITablePositionService tablePositionService)
	{
		_logger = logger;
		_realtimeConnectionService = realtimeConnectionService;
		_realtimeStatusService = realtimeStatusService;
		_ctboxStatusService = ctboxStatusService;
		_doorStatusService = doorStatusService;
		_tablePositionService = tablePositionService;

		_monitorConfig = monitorConfig.Value;

		_currentConnectionStatus = _monitorConfig.MonitorReconConnectionStatus ? _realtimeConnectionService.IsReconConnected : true;
		_currentDoorClosed = _monitorConfig.MonitorDoorClosed ? _doorStatusService.IsClosed : true;
		_currentTableStatus = _monitorConfig.MonitorTableStatus ? _tablePositionService.CurrentTableStatus != PartStatus.Disconnection : true;
		_currentCTBoxStatus = _monitorConfig.MonitorCTBox ? _ctboxStatusService.Status != PartStatus.Disconnection : true;
		_currentRealtimeStatus = _realtimeStatusService.CurrentStatus == RealtimeStatus.Standby;
		_currentTableLocked = _monitorConfig.MonitorTableStatus ? _tablePositionService.CurrentTablePosition.Locked : false;

		SetStatus("Initialize", true);

		_realtimeConnectionService.ReconConnectionStatusChanged += RealtimeReconConnectionStatusChanged;
		_realtimeStatusService.RealtimeStatusChanged += RealtimeStatusService_RealtimeStatusChanged;
        _realtimeStatusService.CycleStatusChanged += RealtimeStatusService_CycleStatusChanged;
        _realtimeStatusService.EmergencyStopped += RealtimeStatusService_EmergencyStopped;
        _realtimeStatusService.ErrorStopped += RealtimeStatusService_ErrorStopped;
		_doorStatusService.StatusChanged += DoorStatusService_DoorStatusChanged;
		_ctboxStatusService.StatusChanged += CTBoxStatusChanged;
		_tablePositionService.TableStatusChanged += TableStatusChanged;
		_tablePositionService.TableLocked += TableLocked;
	}

    private void RealtimeStatusService_ErrorStopped(object? sender, EventArgs<RealtimeInfo> e)
    {
		ErrorStopped?.Invoke(this, e);
    }

    private void RealtimeStatusService_EmergencyStopped(object? sender, EventArgs<RealtimeInfo> e)
    {
		EmergencyStopped?.Invoke(this, e);
    }

    private void RealtimeStatusService_CycleStatusChanged(object? sender, EventArgs<DeviceSystem> e)
    {
		CycleStatusChanged?.Invoke(this, e);
    }

    private void TableLocked(object? sender, bool e)
	{
		if (_monitorConfig.MonitorTableStatus && e != _currentTableLocked)
		{
			_currentTableLocked = e;
			SetStatus(nameof(TableLocked), true);
		}
	}

	private void TableStatusChanged(object? sender, PartStatus e)
	{
		//todo:走配置
		if (_monitorConfig.MonitorTableStatus)
		{
			_currentTableStatus = e != PartStatus.Disconnection;
			SetStatus(nameof(TableStatusChanged), true);
		}
	}

	private void CTBoxStatusChanged(object? sender, PartStatus e)
	{
		//todo:走配置
		if (_monitorConfig.MonitorCTBox)
		{
			_currentCTBoxStatus = e != PartStatus.Disconnection;
			SetStatus(nameof(CTBoxStatusChanged), true);
		}
	}

	private void DoorStatusService_DoorStatusChanged(object? sender, bool e)
	{
		if (_monitorConfig.MonitorDoorClosed)
		{
			_currentDoorClosed = e;
			SetStatus(nameof(DoorStatusChanged), true);
		}
		DoorStatusChanged?.Invoke(this, e);
	}

	private void RealtimeStatusService_RealtimeStatusChanged(object? sender, CTS.EventArgs<CTS.Models.RealtimeInfo> e)
	{
		_logger.LogDebug($"SystemReadyService.RealtimeStatus: {_currentRealtimeStatus}, new status: {e.Data.Status}");
		_currentRealtimeStatus = e.Data.Status == RealtimeStatus.Standby;
		SetStatus(nameof(RealtimeStatusChanged), false);
		RealtimeStatusChanged?.Invoke(this, e);
	}

	private void RealtimeReconConnectionStatusChanged(object? sender, bool e)
	{
		_currentConnectionStatus = e;
		SetStatus(nameof(RealtimeReconConnectionStatusChanged), true);
	}

	private void SetStatus(string sourceMethod, bool isSystemStatus)
	{
		var lastStatus = _currentConnectionStatus && _currentRealtimeStatus && _currentDoorClosed && _currentCTBoxStatus && _currentTableStatus && !_currentTableLocked;
		_logger.LogInformation($"SystemReadyService.SetStatus({sourceMethod}): from {Status} to {lastStatus}, ConnectionStatus: {_currentConnectionStatus}, RealtimeStatus: {_currentRealtimeStatus}, DoorClosedStatus: {_currentDoorClosed}, CTBoxStatus: {_currentCTBoxStatus}, TableLocked: {_currentTableLocked}, TableStatus: {_currentTableStatus}");

		if (!_currentConnectionStatus)
		{
			LatestFailReason = "Acq recon connection status is not connected!";
		}
		if (!_currentRealtimeStatus)
		{
			LatestFailReason = "Realtime status is not standby!";
		}
		if (!_currentDoorClosed)
		{
			LatestFailReason = "Door is not closed!";
		}
		if (!_currentCTBoxStatus)
		{
			LatestFailReason = "CTBox connection status is not connected!";
		}
		if (_currentTableLocked)
		{
			LatestFailReason = "Table is locked!";
		}
		if (!_currentTableStatus)
		{
			LatestFailReason = "Table connection status is not connected!";
		}

		if (Status != lastStatus)
		{
			Status = lastStatus;
		}

		StatusChanged?.Invoke(this, new EventArgs<(bool status, bool isSyatemStatus)>((Status, isSystemStatus)));
	}
}
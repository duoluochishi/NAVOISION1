//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/25 16:58:16     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Exception;

namespace NV.CT.Examination.ApplicationService.Impl;

public class ScanStatusService : IScanStatusService
{
    private readonly ILogger<ScanStatusService> _logger;
    private readonly IRealtimeReconProxyService _realtimeProxyService;

    private StateMachine<SystemStatus, ScanTriggerType> stateMachine;

    private List<string> _cancelledScans = new List<string>();

    public ScanStatusService(ILogger<ScanStatusService> logger, IRealtimeReconProxyService realtimeProxyService)
    {
        _logger = logger;

        _realtimeProxyService = realtimeProxyService;

        stateMachine = new StateMachine<SystemStatus, ScanTriggerType>(_realtimeProxyService.SystemStatus, _logger);

        //初始化状态为None，保证第一个状态能够正常触发。
        stateMachine.Configure(SystemStatus.None)
            .Permit(SystemStatus.Standby, ScanTriggerType.Ready)
            .Permit(SystemStatus.None, ScanTriggerType.None)
            .Permit(SystemStatus.Init, ScanTriggerType.None);

        stateMachine.Configure(SystemStatus.Init)
            .Permit(SystemStatus.Init, ScanTriggerType.None)
            .Permit(SystemStatus.Standby, ScanTriggerType.Ready);

        //todo: Standby --> Preparing，临时通过，按实际业务不符合要求
        stateMachine.Configure(SystemStatus.Standby)
            .Permit(SystemStatus.Init, ScanTriggerType.None)
            .Permit(SystemStatus.Standby, ScanTriggerType.None)
            .Permit(SystemStatus.Validated, ScanTriggerType.Loaded)
            .Permit(SystemStatus.Preparing, ScanTriggerType.Loaded)
            .Permit(SystemStatus.EmergencyStopped, ScanTriggerType.None)
            .Permit(SystemStatus.ErrorScanStopped, ScanTriggerType.None);

        stateMachine.Configure(SystemStatus.Validated)
            .Permit(SystemStatus.Preparing, ScanTriggerType.None)
            .Permit(SystemStatus.NormalScanStopped, ScanTriggerType.Cancelled)
            .Permit(SystemStatus.EmergencyStopped, ScanTriggerType.Cancelled)
            .Permit(SystemStatus.ErrorScanStopped, ScanTriggerType.Cancelled);

        //todo: Preparing --> Validated，临时通过，待查具体原因，偶发时序处理问题，暂不影响业务，无实际业务
        stateMachine.Configure(SystemStatus.Preparing)
            .Permit(SystemStatus.Validated, ScanTriggerType.None)
            .Permit(SystemStatus.Scanning, ScanTriggerType.Scanning)
            .Permit(SystemStatus.NormalScanStopped, ScanTriggerType.Cancelled)
            .Permit(SystemStatus.EmergencyStopped, ScanTriggerType.Cancelled)
            .Permit(SystemStatus.ErrorScanStopped, ScanTriggerType.Cancelled);

        stateMachine.Configure(SystemStatus.Scanning)
            .Permit(SystemStatus.Exposuring, ScanTriggerType.None)
            .Permit(SystemStatus.NormalScanStopped, ScanTriggerType.Cancelled)
            .Permit(SystemStatus.EmergencyStopped, ScanTriggerType.Cancelled)
            .Permit(SystemStatus.ErrorScanStopped, ScanTriggerType.Cancelled);

        //todo: Exposuring --> NormalScanStopped，和嵌入式讨论后，增加此路由规则
        stateMachine.Configure(SystemStatus.Exposuring)
            .Permit(SystemStatus.ExposuringIdle, ScanTriggerType.None)
            .Permit(SystemStatus.ExposuringFinished, ScanTriggerType.None)
            .Permit(SystemStatus.NormalScanStopped, ScanTriggerType.Done)
            .Permit(SystemStatus.EmergencyStopped, ScanTriggerType.Aborted)
            .Permit(SystemStatus.ErrorScanStopped, ScanTriggerType.Aborted);

        //todo:此处到NormalStopped或Finished状态，都不正常，需要和底层确认
        //todo: ExposuringIdle --> NormalScanStopped，同Exposuring --> NormalScanStopped一样
        stateMachine.Configure(SystemStatus.ExposuringIdle)
            .Permit(SystemStatus.Exposuring, ScanTriggerType.None)
            .Permit(SystemStatus.NormalScanStopped, ScanTriggerType.Done)
            .Permit(SystemStatus.EmergencyStopped, ScanTriggerType.Aborted)
            .Permit(SystemStatus.ErrorScanStopped, ScanTriggerType.Aborted);


        //todo:下一个状态的业务处理需进一步分析
        //todo: Helical => ExposuringFinished --> ExposuringIdle
        stateMachine.Configure(SystemStatus.ExposuringFinished)
            .Permit(SystemStatus.ExposuringIdle, ScanTriggerType.None)
            .Permit(SystemStatus.NormalScanStopped, ScanTriggerType.Done)
            .Permit(SystemStatus.EmergencyStopped, ScanTriggerType.Aborted)
            .Permit(SystemStatus.ErrorScanStopped, ScanTriggerType.Aborted);

        stateMachine.Configure(SystemStatus.NormalScanStopped)
            .Permit(SystemStatus.Preparing, ScanTriggerType.Loaded)
            .Permit(SystemStatus.EmergencyStopped, ScanTriggerType.Aborted)
            .Permit(SystemStatus.ErrorScanStopped, ScanTriggerType.Aborted)
            .Permit(SystemStatus.Standby, ScanTriggerType.Ready);

        stateMachine.Configure(SystemStatus.EmergencyStopped)
            .Permit(SystemStatus.Init, ScanTriggerType.Done)
            .Permit(SystemStatus.Standby, ScanTriggerType.Ready);

        stateMachine.Configure(SystemStatus.ErrorScanStopped)
            .Permit(SystemStatus.Standby, ScanTriggerType.Ready);

        stateMachine.Configure(ScanTriggerType.Loaded, (preview, current, command, parameters) =>
        {
            var statusInfo = parameters as CTS.Models.SystemStatusInfo;
            if (CurrentScan?.Descriptor.Id != statusInfo?.ScanId)
            {
                CurrentScan = CurrentMeasurement?.Children.FirstOrDefault(scan => scan.Descriptor.Id == statusInfo?.ScanId);
            }
        });
        //todo: 增加了Waiting状态，有下参(MeasurementLoad)处理
        stateMachine.Configure(ScanTriggerType.Scanning, (preview, current, command, parameters) =>
        {
            var statusInfo = parameters as CTS.Models.SystemStatusInfo;
            ScanStarted?.Invoke(this, new CTS.EventArgs<string>(statusInfo?.ScanId));
        });
        stateMachine.Configure(ScanTriggerType.Cancelled, (preview, current, command, parameters) =>
        {
            var statusInfo = parameters as CTS.Models.SystemStatusInfo;
            var isUserCancelled = _cancelledScans.Contains(statusInfo.ScanId);
            ScanCancelled?.Invoke(this, new CTS.EventArgs<(string, string, bool)>((CurrentMeasurement.Descriptor.Id, statusInfo?.ScanId, isUserCancelled)));
        });
        stateMachine.Configure(ScanTriggerType.Done, (preview, current, command, parameters) =>
        {
            var statusInfo = parameters as CTS.Models.SystemStatusInfo;
            var isUserCancelled = _cancelledScans.Contains(statusInfo?.ScanId);
            ScanDone?.Invoke(this, new CTS.EventArgs<(string, RealDoseInfo, bool)>((statusInfo?.ScanId, statusInfo?.DoseInfo, isUserCancelled)));
        });
        stateMachine.Configure(ScanTriggerType.Aborted, (preview, current, command, parameters) =>
        {
            var statusInfo = parameters as CTS.Models.SystemStatusInfo;
            var isUserCancelled = _cancelledScans.Contains(statusInfo.ScanId);
            ScanAborted?.Invoke(this, new CTS.EventArgs<(string, RealDoseInfo, bool, bool)>((statusInfo?.ScanId, statusInfo?.DoseInfo, current == SystemStatus.EmergencyStopped, isUserCancelled)));
        });
        stateMachine.Configure(ScanTriggerType.Ready, (preview, current, command, parameters) =>
        {
            CurrentScan = null;
            CurrentMeasurement = null;
            _cancelledScans.Clear();
        });

        _realtimeProxyService.SystemStatusChanged += OnProxyService_SystemStatusChanged;
        _realtimeProxyService.RawDataSaved += OnProxyService_RawDataSaved;
    }

    private void OnProxyService_RawDataSaved(object? sender, EventArgs<RealtimeReconInfo> e)
    {
        RawDataSaved?.Invoke(this, new EventArgs<(string ScanId, string ImagePath)>((e.Data.ScanId, e.Data.ImagePath)));
    }

    private void OnProxyService_SystemStatusChanged(object? sender, CTS.EventArgs<CTS.Models.SystemStatusInfo> e)
    {
        _logger.LogInformation("RealtimeReconProxyService.SystemStatusChanged: {0}", JsonConvert.SerializeObject(new { Thread = Thread.CurrentThread.ManagedThreadId, ScanId = e.Data.ScanId, e.Data.Status, StatusString = e.Data.Status.ToString() }));

        try
        {
            stateMachine.Next(e.Data.Status, e.Data);
        }
        catch (NanoException ex)
        {
            _logger.LogWarning(ex, $"StateMachine.Next exception: {ex.Message}");
        }
    }

    public void CancelMeasurement()
    {
        if (CurrentMeasurement is null) return;

        if (CurrentScan is null) return;

        _cancelledScans.Add(CurrentScan.Descriptor.Id);
    }

    public MeasurementModel CurrentMeasurement { get; set; }

    public ScanModel CurrentScan { get; set; }

    public SystemStatus PreviewStatus => stateMachine.Preview;

    public SystemStatus CurrentStatus => stateMachine.Current;

    public event EventHandler<EventArgs<string>>? ScanStarted;

    public event EventHandler<EventArgs<(string, string, bool)>>? ScanCancelled;

    public event EventHandler<EventArgs<(string ScanId, RealDoseInfo DoseInfo, bool IsCancelled)>>? ScanDone;

    public event EventHandler<EventArgs<(string ScanId, RealDoseInfo DoseInfo, bool IsEmergencyStopped, bool IsUserCancelled)>>? ScanAborted;

    public event EventHandler<EventArgs<(string ScanId, string ImagePath)>>? RawDataSaved;
}

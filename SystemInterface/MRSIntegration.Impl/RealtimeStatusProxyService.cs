//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/10 13:13:27     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.SystemInterface.MRSIntegration.Contract.Models;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class RealtimeStatusProxyService : IRealtimeStatusProxyService
{
    private readonly IMapper _mapper;
    private readonly ILogger<RealtimeStatusProxyService> _logger;
    private readonly IRealtimeProxyService _proxyService;

    private readonly WorkingBox _workingRealtimeStatus;
    private readonly WorkingBox _workingCycleStatus;

    public RealtimeStatus PreviewStatus { get; private set; }

    public RealtimeStatus CurrentStatus { get; private set; }

    public bool IsDoorClosed { get; private set; }

    public bool IsFrontRearCoverClosed { get; private set; }

    public bool IsDetectorTemperatureNormalStatus { get; private set; }

    public event EventHandler<EventArgs<DeviceSystem>> CycleStatusChanged;
    public event EventHandler<EventArgs<RealtimeInfo>> RealtimeStatusChanged;
    public event EventHandler<EventArgs<RealtimeInfo>> EmergencyStopped;
    public event EventHandler<EventArgs<RealtimeInfo>> ErrorStopped;
    public event EventHandler<EventArgs<List<string>>> DeviceErrorOccurred;
    public event EventHandler<EventArgs<List<string>>> ScanReconErrorOccurred;

    public RealtimeStatusProxyService(IMapper mapper, ILogger<RealtimeStatusProxyService> logger, IRealtimeProxyService proxyService)
    {
        _mapper = mapper;
        _logger = logger;
        _proxyService = proxyService;
        _workingRealtimeStatus = new WorkingBox();
        _workingCycleStatus = new WorkingBox();
        _proxyService.CycleStatusChanged += RealtimeProxy_CycleStatusChanged;
        _proxyService.RealTimeStatusChanged += RealtimeProxy_RealtimeStatusChanged;
        _proxyService.DeviceErrorOccurred += RealtimeProxy_DeviceErrorOccurred;
        _proxyService.ScanReconErrorOccurred += RealtimeProxy_ScanReconErrorOccurred;
        CurrentStatus = proxyService.RealtimeStatus;
    }

    private void RealtimeProxy_ScanReconErrorOccurred(object? sender, List<string> e)
    {
        if (CurrentStatus != RealtimeStatus.EmergencyScanStopped)
        {
            ScanReconErrorOccurred?.Invoke(this, new EventArgs<List<string>>(e));
        }
    }

    private void RealtimeProxy_DeviceErrorOccurred(object? sender, List<string> e)
    {
        if (CurrentStatus != RealtimeStatus.EmergencyScanStopped)
        {
            DeviceErrorOccurred?.Invoke(this, new EventArgs<List<string>>(e));
        }
    }

    private void RealtimeProxy_CycleStatusChanged(object sender, CycleStatusArgs cycleStatusArgs)
    {
        _workingCycleStatus.Raise(() => {
            IsDoorClosed = cycleStatusArgs.Device.DoorClosed;
            IsFrontRearCoverClosed = cycleStatusArgs.Device.Gantry.FrontRearCoverAllClosed;
            IsDetectorTemperatureNormalStatus = cycleStatusArgs.Device.IsDetectorTemperatureNormal;
            try
            {
                CycleStatusChanged?.Invoke(this, new EventArgs<DeviceSystem>(new DeviceSystem
                {
                    SystemStatus = cycleStatusArgs.Device.SystemStatus,
                    RealtimeStatus = cycleStatusArgs.Device.RealtimeStatus,
                    DoorClosed = cycleStatusArgs.Device.DoorClosed,
                    IsDetectorTemperatureNormalStatus = cycleStatusArgs.Device.IsDetectorTemperatureNormal,
                    PDU = _mapper.Map<DevicePart>(cycleStatusArgs.Device.PDU),
                    CTBox = _mapper.Map<DevicePart>(cycleStatusArgs.Device.CTBox),
                    IFBox = _mapper.Map<DevicePart>(cycleStatusArgs.Device.IFBox),
                    Gantry = _mapper.Map<Gantry>(cycleStatusArgs.Device.Gantry),
                    AuxBoard = _mapper.Map<DevicePart>(cycleStatusArgs.Device.AuxBoard),
                    ExtBoard = _mapper.Map<DevicePart>(cycleStatusArgs.Device.ExtBoard),
                    TubeInfts = cycleStatusArgs.Device.TubeIntfs.Select(t => _mapper.Map<TubeIntf>(t)).ToList(),
                    Table = _mapper.Map<Table>(cycleStatusArgs.Device.Table),
                    Tubes = cycleStatusArgs.Device.XRaySources.Select(t => new Tube
                    {
                        Number = t.Number,
                        //限制：Status无效，不可使用
                        Status = PartStatus.Normal,
                        RaySource = new RaySource
                        {
                            OilTemperature = t.XRaySourceOilTemp,
                            HeatCapacity = t.XRaySourceHeatCap
                        }
                    }).ToList(),
                    ControlBox = _mapper.Map<DevicePart>(cycleStatusArgs.Device.ControlBox),
                    Detector = new Detector
                    {
                        DetectorModules = cycleStatusArgs.Device.Detector.DetectorModules.Select(m => new DetectorModule(m.Number)
                        {
                            TransmissionBoardStatus = m.TransmissionBoardStatus,
                            ProcessingBoards = m.ProcessingBoards.Select(p => new ProcessingBoard {
                                Temperature = p.Temperature,
                            }).ToArray(),
                            DetectBoards = m.DetectBoards.Select(d => new DetectBoard {
                                Chip1Temperature = d.Chip1Temperature,
                                Chip2Temperature = d.Chip2Temperature,
                            }).ToArray(),
                            TemperatureControlBoard = new TemperatureControlBoard {
                                Humidity = m.TemperatureControlBoard.Humidity,
                                Powers = m.TemperatureControlBoard.Powers
                            },
                            DetAcqMode = m.DetAcqMode
                        }).ToArray(),
                        AcqCards = cycleStatusArgs.Device.Detector.AcqCards.Select(a => new AcqCard()).ToArray()
                    },
                    RTDRecon = _mapper.Map<DevicePart>(cycleStatusArgs.Device.RTDRecon)
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError($"RealtimeFacadeProxy.CycleStatusChanged: {ex.Message} => {ex.StackTrace}");
            }
        });
    }

    private void RealtimeProxy_RealtimeStatusChanged(object sender, FacadeProxy.Common.Arguments.RealtimeEventArgs realtimeArgs)
    {
        _workingRealtimeStatus.Raise(() =>
        {
            PreviewStatus = CurrentStatus;
            CurrentStatus = realtimeArgs.Status;
            var eventInfo = new RealtimeInfo
            {
                Status = realtimeArgs.Status,
                ScanId = realtimeArgs.ScanUID
            };
            if (eventInfo.Status == RealtimeStatus.Error)
            {
                eventInfo.ErrorCodes = realtimeArgs.GetErrorCodes().ToList();
            }

            try
            {
                if (eventInfo.Status == RealtimeStatus.EmergencyScanStopped)
                {
                    EmergencyStopped?.Invoke(this, new EventArgs<RealtimeInfo>(eventInfo));
                }
                else if (eventInfo.Status == RealtimeStatus.Error)
                {
                    ErrorStopped?.Invoke(this, new EventArgs<RealtimeInfo>(eventInfo));
                }
                else
                {
                    //TODO: 另外两种待处理，暂时未定
                    RealtimeStatusChanged?.Invoke(this, new EventArgs<RealtimeInfo>(eventInfo));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"RealtimeFacadeProxy.RealtimeStatusChanged: {ex.Message} => {ex.StackTrace}");
            }
        });
    }
}

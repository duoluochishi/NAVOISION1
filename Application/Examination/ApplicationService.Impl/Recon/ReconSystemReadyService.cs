using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Impl.Recon;

public class ReconSystemReadyService : SystemReadyService
{
	public ReconSystemReadyService(ILogger<SystemReadyService> logger, IOptions<DeviceMonitorConfig> monitorConfig, IRealtimeConnectionService realtimeConnectionService, IRealtimeStatusProxyService realtimeStatusService, IDoorStatusService doorStatusService, ICTBoxStatusService ctboxStatusService, ITablePositionService tablePositionService) : base(logger, monitorConfig, realtimeConnectionService, realtimeStatusService, doorStatusService, ctboxStatusService, tablePositionService)
	{
	}
}

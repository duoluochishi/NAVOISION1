using NV.CT.CTS.Models;
//using NV.CT.SystemInterface.MRS.Contract.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IUIRelatedStatusService
{
	event EventHandler<CTS.EventArgs<RealtimeInfo>> RealtimeStatusChanged;

	event EventHandler<CTS.EventArgs<DeviceSystem>> CycleStatusChanged;

	event EventHandler<CTS.EventArgs<RealtimeInfo>> EmergencyStopped;

	event EventHandler<CTS.EventArgs<RealtimeInfo>> ErrorStopped;

	event EventHandler<bool>? DoorStatusChanged;

	bool IsValidated { get; set; }

	void IsValidatedChanged(bool isValidated);

	bool IsDoorClosed { get; }
}
using NV.CT.CTS.Models;
using NV.CT.Protocol.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IRTDReconService
{
    ReconModel? CurrentActiveRecon { get; }

    event EventHandler<CTS.EventArgs<RealtimeReconInfo>> ReconLoaded;

    event EventHandler<CTS.EventArgs<RealtimeReconInfo>> ReconReconning;

    event EventHandler<CTS.EventArgs<RealtimeReconInfo>> ImageReceived;

    event EventHandler<CTS.EventArgs<RealtimeReconInfo>> ReconCancelled;

    event EventHandler<CTS.EventArgs<RealtimeReconInfo>> ReconDone;

    event EventHandler<CTS.EventArgs<RealtimeReconInfo>> ReconAborted;
}
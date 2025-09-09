namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IFrontRearCoverStatusService
{
    bool IsClosed { get; }

    event EventHandler<bool> StatusChanged;
}

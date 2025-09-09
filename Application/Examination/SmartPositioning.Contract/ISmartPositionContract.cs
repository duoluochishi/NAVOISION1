using NV.CT.SmartPositioning.Common;
namespace NV.CT.SmartPositioning.Contract;

public interface ISmartPositionContract
{
    event EventHandler<SmartPositioningResponse> UploadPositioningChanged;

    void StartSmartPositionService();

    CommandResult SendSmartPositioningCommand(RequestCommand requestCommand);
}
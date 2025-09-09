using Grpc.Core;
using NV.CT.SmartPositioning.Common;

namespace NV.CT.SmartPositioning.Contract;

public class SmartPositioningServiceImpl : SmartPositioningService.SmartPositioningServiceBase
{
    public event EventHandler<SmartPositioningResponse> UploadPositioningEvent;

    async public override Task<Google.Protobuf.WellKnownTypes.Empty> UploadPositioning(SmartPositioningResponse request, ServerCallContext context)
    {
        UploadPositioningEvent?.Invoke(this, request);
        return await Task.FromResult(new Google.Protobuf.WellKnownTypes.Empty());
    }

    public override Task<CommandResult> SendCommand(RequestCommand request, ServerCallContext context)
    {
        CommandResult commandResult = new CommandResult();
        commandResult.SourceCommand = request;
        commandResult.ResultID = request.RequestID + 1;
        commandResult.Result = true;
        commandResult.ResultInfo = "Successful!";

        return Task.FromResult(commandResult);
    }
}
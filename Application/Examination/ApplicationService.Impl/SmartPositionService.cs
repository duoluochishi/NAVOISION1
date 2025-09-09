using Grpc.Core;
using Microsoft.Extensions.Logging;
using NV.CT.SmartPositioning.Common;
using NV.CT.SmartPositioning.Contract;
using NV.MPS.Exception;
using static NV.CT.SmartPositioning.Common.SmartPositioningService;
using Channel = Grpc.Core.Channel;
using ISmartPositioningService = NV.CT.SmartPositioning.Common.SmartPositioningService;
namespace NV.CT.Examination.ApplicationService.Impl;

public class SmartPositionService : ISmartPositionContract
{
    public event EventHandler<SmartPositioningResponse>? UploadPositioningChanged;
    private readonly SmartPositioningServiceImpl _examService;
    private readonly ILogger<SmartPositioningServiceImpl> _logger;
    private readonly Server _smartService;

    private readonly SmartPositioningServiceClient _smartClient;

    public SmartPositionService(SmartPositioningServiceImpl examService,
        ILogger<SmartPositioningServiceImpl> logger)
    {
        _logger = logger;
        try
        {
            _examService = examService;
            _smartService = new Server
            {
                Services = { ISmartPositioningService.BindService(_examService) },
                Ports = { new ServerPort("localhost", 35001, ServerCredentials.Insecure) }
            };
            _smartClient = new SmartPositioningServiceClient(new Channel("localhost", 35002, ChannelCredentials.Insecure));
            _examService.UploadPositioningEvent += ExamService_UploadPositioningEvent;
        }
        catch (NanoException ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }

    private void ExamService_UploadPositioningEvent(object? sender, SmartPositioningResponse e)
    {
        UploadPositioningChanged?.Invoke(sender, e);
    }

    public void StartSmartPositionService()
    {
        if (_smartService is not null && _smartService.Services.Any())
        {
            _smartService.Start();
        }
    }

    public CommandResult SendSmartPositioningCommand(RequestCommand requestCommand)
    {
        CommandResult commandResult = new CommandResult();
        commandResult.Result = false;
        commandResult.SourceCommand = requestCommand;
        if (requestCommand is not null)
        {
            try
            {
                commandResult = _smartClient.SendCommand(requestCommand);
            }
            catch (NanoException ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
        return commandResult;
    }
}
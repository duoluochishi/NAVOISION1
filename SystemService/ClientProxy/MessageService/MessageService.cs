//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.MPS.Communication;
using NV.CT.CTS.Extensions;
using NV.CT.MessageService.Contract;

namespace NV.CT.ClientProxy.MessageService;

public class MessageService : IMessageService
{
    public bool IsStatusMessagePageOpen { get; }

    private readonly MCSServiceClientProxy _clientProxy;
    public MessageService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public event EventHandler<MessageInfo>? MessageNotify;

    public void SendMessage(MessageInfo messageInfo)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IMessageService).Namespace,
            SourceType = nameof(IMessageService),
            ActionName = nameof(IMessageService.SendMessage),
            Data = messageInfo.ToJson()
        });
    }

    public void StatusMessagePageOpen(string isShow)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IMessageService).Namespace,
            SourceType = nameof(IMessageService),
            ActionName = nameof(IMessageService.StatusMessagePageOpen),
            Data = isShow
        });
    }
}
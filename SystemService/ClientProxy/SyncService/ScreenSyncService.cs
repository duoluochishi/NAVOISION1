//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.MPS.Communication;
using NV.CT.SyncService.Contract;

namespace NV.CT.ClientProxy.SyncService;

public class ScreenSyncService : IScreenSync
{
    private readonly SyncServiceClientProxy _clientProxy;
    public ScreenSyncService(SyncServiceClientProxy proxy)
    {
        _clientProxy = proxy;
    }

    public string GetPreviousLayout()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScreenSync).Namespace,
            SourceType = nameof(IScreenSync),
            ActionName = nameof(IScreenSync.GetPreviousLayout),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            return commandResponse.Data;
        }
        return string.Empty;
    }

    public string GetCurrentLayout()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScreenSync).Namespace,
            SourceType = nameof(IScreenSync),
            ActionName = nameof(IScreenSync.GetCurrentLayout),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            return commandResponse.Data;
        }
        return string.Empty;
    }

    public void SwitchTo(string syncScreens)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScreenSync).Namespace,
            SourceType = nameof(IScreenSync),
            ActionName = nameof(IScreenSync.SwitchTo),
            Data = syncScreens
        });
    }

    public void Back()
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScreenSync).Namespace,
            SourceType = nameof(IScreenSync),
            ActionName = nameof(IScreenSync.Back),
            Data = string.Empty
        });
    }

    public void Go()
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScreenSync).Namespace,
            SourceType = nameof(IScreenSync),
            ActionName = nameof(IScreenSync.Go),
            Data = string.Empty
        });
    }

    public void Resume()
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScreenSync).Namespace,
            SourceType = nameof(IScreenSync),
            ActionName = nameof(IScreenSync.Resume),
            Data = string.Empty
        });
    }


#pragma warning disable 67
    public event EventHandler<string>? ScreenChanged;
#pragma warning restore 67

}
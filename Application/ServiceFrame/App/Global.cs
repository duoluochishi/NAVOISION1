using Microsoft.Extensions.DependencyInjection;
using NV.CT.ClientProxy;
using NV.MPS.Communication;
using System;

namespace NV.CT.ServiceFrame;

public class Global
{
    private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

    public string ModelName = string.Empty;

    public static Global Instance => _instance.Value;

    private Global()
    {
        _clientInfo = new ClientInfo { Id = $"[ServiceFrame]-{DateTime.Now:yyyyMMddHHmmss}" };
    }

    private MCSServiceClientProxy? _serviceClientProxy;
    private ClientInfo _clientInfo;

    public void Subscribe()
    {
        _serviceClientProxy = ServiceFramework.Global.Instance.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
        _serviceClientProxy?.Subscribe(_clientInfo);
    }
    public void Unsubscribe()
    {
        _serviceClientProxy?.Unsubscribe(_clientInfo);
    }
}
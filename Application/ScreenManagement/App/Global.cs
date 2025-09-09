//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using NV.CT.ClientProxy;
using NV.MPS.Communication;
using NV.CT.CTS.Extensions;
using System;

namespace NV.CT.ScreenManagement;

public class Global
{
    private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

    private ClientInfo _clientInfo;
    private MCSServiceClientProxy? _serviceClientProxy;

    public static Global Instance => _instance.Value;

    private Global()
    {
        _clientInfo = new ClientInfo { Id = $"[ScreenManagement]_{IdGenerator.Next(0)}" };
        _serviceClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
    }

    public void Subscribe()
    {
        _serviceClientProxy?.Subscribe(_clientInfo);
    }

    public void Unsubscribe()
    {
        if (_clientInfo != null)
        {
            _serviceClientProxy?.Unsubscribe(_clientInfo);
        }
    }

}
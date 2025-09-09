//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.AppService.Contract;
using NV.MPS.Communication;

namespace NV.CT.ClientProxy.Application;

public class ScreenManagementService : IScreenManagement
{
    private readonly MCSServiceClientProxy _clientProxy;
    public ScreenManagementService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public void Lock(string reason)
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScreenManagement).Namespace,
            SourceType = nameof(IScreenManagement),
            ActionName = nameof(IScreenManagement.Lock),
            Data = reason
        });

    }

    public void Unlock(string reason)
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScreenManagement).Namespace,
            SourceType = nameof(IScreenManagement),
            ActionName = nameof(IScreenManagement.Unlock),
            Data = reason
        });
    }

    public event EventHandler<string>? LockScreenStatusChanged;
    public event EventHandler<string>? UnlockScreenStatusChanged;
}
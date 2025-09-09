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
using Newtonsoft.Json;
using NV.MPS.Communication;
using NV.CT.CTS.Extensions;
using NV.CT.Models;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.ClientProxy.Workflow;
public class Authorization : IAuthorization
{
    private readonly MCSServiceClientProxy _clientProxy;

    public event EventHandler<UserModel?>? CurrentUserChanged;

    public Authorization(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }
    public UserModel? GetCurrentUser()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IAuthorization).Namespace,
            SourceType = nameof(IAuthorization),
            ActionName = nameof(IAuthorization.GetCurrentUser),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<UserModel?>(commandResponse.Data);
            return res;
        }
        return null;
    }

    public AuthorizationResult AuthenticationCurrentUser(string permissionCode)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IAuthorization).Namespace,
            SourceType = nameof(IAuthorization),
            ActionName = nameof(IAuthorization.AuthenticationCurrentUser),
            Data = permissionCode
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<AuthorizationResult>(commandResponse.Data);
            return res;
        }
        return new AuthorizationResult(false, "", false);
    }

    public AuthorizationResult AuthenticationOtherUser(AuthorizationRequest request)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IAuthorization).Namespace,
            SourceType = nameof(IAuthorization),
            ActionName = nameof(IAuthorization.AuthenticationOtherUser),
            Data = request.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<AuthorizationResult>(commandResponse.Data);
            return res;
        }
        return new AuthorizationResult(false, "", false);
    }

    public bool IsAuthorized(string permissionCode)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IAuthorization).Namespace,
            SourceType = nameof(IAuthorization),
            ActionName = nameof(IAuthorization.IsAuthorized),
            Data = permissionCode
        });
        if (commandResponse.Success)
        {
            return Convert.ToBoolean(commandResponse.Data);
        }
        return false;
    }
}
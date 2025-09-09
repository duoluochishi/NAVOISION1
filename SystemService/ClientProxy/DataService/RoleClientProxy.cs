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
using NV.CT.DatabaseService.Contract;
using NV.CT.Models;

namespace NV.CT.ClientProxy.DataService;

public class RoleClientProxy : IRoleService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public RoleClientProxy(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public bool Insert(RoleModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRoleService).Namespace,
            SourceType = nameof(IRoleService),
            ActionName = nameof(IRoleService.Insert),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool Update(RoleModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRoleService).Namespace,
            SourceType = nameof(IRoleService),
            ActionName = nameof(IRoleService.Update),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool Delete(RoleModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRoleService).Namespace,
            SourceType = nameof(IRoleService),
            ActionName = nameof(IRoleService.Delete),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public List<RoleModel> GetAllWithRight()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRoleService).Namespace,
            SourceType = nameof(IRoleService),
            ActionName = nameof(IRoleService.GetAllWithRight),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<RoleModel>>(commandResponse.Data);
            return res;
        }
        return new List<RoleModel>();
    }

    public List<RoleModel> GetAll()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRoleService).Namespace,
            SourceType = nameof(IRoleService),
            ActionName = nameof(IRoleService.GetAll),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<RoleModel>>(commandResponse.Data);
            return res;
        }
        return new List<RoleModel>();
    }

    public RoleModel GetRoleById(string roleId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRoleService).Namespace,
            SourceType = nameof(IRoleService),
            ActionName = nameof(IRoleService.GetRoleById),
            Data = roleId
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<RoleModel>(commandResponse.Data);
            return res;
        }
        return new RoleModel() { IsDeleted = true };
    }

    public RoleModel GetRoleByName(string name)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRoleService).Namespace,
            SourceType = nameof(IRoleService),
            ActionName = nameof(IRoleService.GetRoleByName),
            Data = name
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<RoleModel>(commandResponse.Data);
            return res;
        }
        return new RoleModel() { IsDeleted = true };
    }

    public bool InsertRoleAndRight(RoleModel entity, List<PermissionModel> permissions)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRoleService).Namespace,
            SourceType = nameof(IRoleService),
            ActionName = nameof(IRoleService.InsertRoleAndRight),
            Data = Tuple.Create(entity, permissions).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool UpdateRoleAndRight(RoleModel entity, List<PermissionModel> permissions)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRoleService).Namespace,
            SourceType = nameof(IRoleService),
            ActionName = nameof(IRoleService.UpdateRoleAndRight),
            Data = Tuple.Create(entity, permissions).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }
} 
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
using NV.CT.Models.User;

namespace NV.CT.ClientProxy.DataService;

public class UserClientProxy : IUserService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public event EventHandler? UserLogoutSuccess;
    public UserClientProxy(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public event EventHandler<UserModel>? CurrentUserChanged;

    public bool Insert(UserModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.Insert),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool Update(UserModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.Update),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool Delete(UserModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.Delete),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public List<UserModel> GetAll()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.GetAll),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<UserModel>>(commandResponse.Data);
            return res;
        }
        return new List<UserModel>();
    }

    public UserModel GetUserRolePermissionList(string userID)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.GetUserRolePermissionList),
            Data = userID
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<UserModel>(commandResponse.Data);
            return res;
        }
        return new UserModel() { IsDeleted = true };
    }

    public AuthorizationResult Login(AuthorizationRequest req)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.Login),
            Data = req.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<AuthorizationResult>(commandResponse.Data);
            return res;
        }
        return new AuthorizationResult(false, "", false);
    }

    public AuthorizationResult ServiceUserLogin(string encryptedContent)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.ServiceUserLogin),
            Data = encryptedContent
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<AuthorizationResult>(commandResponse.Data);
            return res;
        }
        return new AuthorizationResult(false, "", false);
    }


    public AuthorizationResult LogOut(AuthorizationRequest req)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.LogOut),
            Data = req.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<AuthorizationResult>(commandResponse.Data);
            return res;
        }
        return new AuthorizationResult(false, "", false);
    }

    public UserModel GetUserModel(AuthorizationRequest req)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.GetUserModel),
            Data = req.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<UserModel>(commandResponse.Data);
            return res;
        }
        return new UserModel { IsDeleted = true };
    }

    public bool EmergencyLogin()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.EmergencyLogin),
            Data = String.Empty
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

	public bool IsPasswordCorrect(IsPasswordCorrectRequest req)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.IsPasswordCorrect),
            Data = req.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
	}

	public bool UpdatePassword(UpdatePasswordRequest req)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.UpdatePassword),
            Data = req.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
	}

    public bool ResetLoginTimes(string userName)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.ResetLoginTimes),
            Data = userName
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
	}

    public UserModel GetUserById(string userEntityId)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.GetUserById),
            Data = userEntityId
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<UserModel>(commandResponse.Data);
            return res;
        }
        return new UserModel { IsDeleted = true };
	}

	public bool IncreWrongPassLoginTimes(string userName)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.IncreWrongPassLoginTimes),
            Data = userName
		});
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool LockUserByName(string userName)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.LockUserByName),
            Data = userName
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
	}

    public UserModel? GetUserByUserName(string userName)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.GetUserByUserName),
            Data = userName
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<UserModel?>(commandResponse.Data);
            return res;
        }

        return null;
    }

	public bool ToggleLockStatus(UserModel userModel)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IUserService).Namespace,
            SourceType = nameof(IUserService),
            ActionName = nameof(IUserService.ToggleLockStatus),
            Data = userModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
	}
}
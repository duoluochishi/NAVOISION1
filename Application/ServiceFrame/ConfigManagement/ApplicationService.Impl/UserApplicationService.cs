//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.DatabaseService.Contract;
using NV.CT.Models;
using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.ConfigManagement.ApplicationService.Impl;

public class UserApplicationService : IUserApplicationService
{
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;
    private readonly IPermissionService _permissionService;
    public event EventHandler<EventArgs<(OperationType operation, UserModel userModel)>>? UserInfoChanged;
    public event EventHandler<EventArgs<(OperationType operation, RoleModel roleModel)>>? RoleInfoChanged;
    public event EventHandler? UserListReload;
    public event EventHandler? RoleListReload;

    public UserApplicationService(IRoleService roleServcie, 
        IUserService userService,
        IPermissionService permissionService)
    {
        _roleService = roleServcie;
        _userService = userService;
        _permissionService = permissionService;
    }

    public void SetUserInfo(OperationType operation, UserModel userModel)
    {
        UserInfoChanged?.Invoke(this, new EventArgs<(OperationType operation, UserModel userModel)>((operation, userModel)));
    }

    public void ReloadUserList()
    {
        UserListReload?.Invoke(this, new EventArgs());
    }

    public List<UserModel> GetAllUser()
    {           //Tablets
        return _userService.GetAll().OrderBy(u => u.Account).ToList();
    }

    public UserModel GetUserRolePermissionList(string userID)
    {
        return _userService.GetUserRolePermissionList(userID);
    }

    public bool AddUser(UserModel userModel)
    {
        return _userService.Insert(userModel);
    }
    public bool UpdateUser(UserModel userModel)
    {
        return _userService.Update(userModel);
    }

    public bool DeleteUser(UserModel userModel)
    {
        return _userService.Delete(userModel);
    }

    public void SetRoleInfo(OperationType operation, RoleModel roleModel)
    {
        RoleInfoChanged?.Invoke(this, new EventArgs<(OperationType operation, RoleModel roleModel)>((operation, roleModel)));
    }

    public void ReloadRoleList()
    {
        RoleListReload?.Invoke(this, new EventArgs());
    }

    public List<RoleModel> GetAllRole()
    {
        return _roleService.GetAll().OrderByDescending(r => r.IsFactory).ThenBy(r => r.Level).ThenBy(r => r.CreateTime).ToList();
    }

    public RoleModel GetRoleById(string roleId)
    {
        return _roleService.GetRoleById(roleId);
    }

    public bool AddRole(RoleModel roleModel)
    {
        return _roleService.InsertRoleAndRight(roleModel, roleModel.PermissionList);
    }

    public bool UpdateRole(RoleModel roleModel)
    {
        return _roleService.UpdateRoleAndRight(roleModel, roleModel.PermissionList);
    }

    public bool DeleteRole(RoleModel roleModel)
    {
        return _roleService.Delete(roleModel);
    }

    public List<PermissionModel> GetAllPermission()
    {
        return _permissionService.GetAll();
    }

	public bool ToggleLockStatus(UserModel userModel)
    {
        return _userService.ToggleLockStatus(userModel);
    }
}
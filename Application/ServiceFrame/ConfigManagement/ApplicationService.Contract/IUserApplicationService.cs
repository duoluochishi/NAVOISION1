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
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.Models;
namespace NV.CT.ConfigManagement.ApplicationService.Contract;

public interface IUserApplicationService
{
    event EventHandler<EventArgs<(OperationType operation, UserModel userModel)>> UserInfoChanged;

    event EventHandler<EventArgs<(OperationType operation, RoleModel roleModel)>> RoleInfoChanged;

    event EventHandler UserListReload;

    event EventHandler RoleListReload;

    void SetUserInfo(OperationType operation, UserModel userModel);

    void SetRoleInfo(OperationType operation, RoleModel roleModel);

    void ReloadUserList();

    void ReloadRoleList();

    List<UserModel> GetAllUser();

    UserModel GetUserRolePermissionList(string userID);

    bool AddUser(UserModel userModel);

    bool UpdateUser(UserModel userModel);

    bool DeleteUser(UserModel userModel);

    bool ToggleLockStatus(UserModel userModel);

    List<RoleModel> GetAllRole();

    RoleModel GetRoleById(string roleId);

    bool AddRole(RoleModel roleModel);

    bool UpdateRole(RoleModel roleModel);

    bool DeleteRole(RoleModel roleModel);

    List<PermissionModel> GetAllPermission();
}
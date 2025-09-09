//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/2 16:13:58           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.Models;

public class UserModel : BaseInfo
{
    public UserBehavior Behavior { get; set; } = UserBehavior.Logout;

    public string Account { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string UserName
    {
        get => FirstName + (!string.IsNullOrEmpty(LastName) ? "^" : "") + LastName;
    }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string UserRoleName { get; set; } = string.Empty;

    public Gender Sex { get; set; } = Gender.Male;

    public string Comments { get; set; } = string.Empty;

    public bool IsFactory { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public bool IsLocked { get; set; } = false;
    public int WrongPassLoginTimes { get; set; }

	public List<RoleModel> RoleList { get; set; } = new List<RoleModel>();

    public List<PermissionModel> AllUserPermissionList { get; set; } = new List<PermissionModel>();
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/2 16:14:52           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.Models;
public class RoleModel : BaseInfo
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsFactory { get; set; } = false;

    public int UserCount { get; set; } = 0;

    public PermissionLevel Level { get; set; } = PermissionLevel.Normal;

    public bool IsDeleted { get; set; }

    public List<PermissionModel> PermissionList { get; set; } = new List<PermissionModel>();
}
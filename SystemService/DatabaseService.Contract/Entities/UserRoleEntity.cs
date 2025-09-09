//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/10/20 14:26:45           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace NV.CT.DatabaseService.Contract.Entities;

public class UserRoleEntity : BaseEntity
{
	public string UserId { get; set; } = string.Empty;

    public string UserAccount { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;

}


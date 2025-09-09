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

namespace NV.CT.DatabaseService.Contract.Entities;

public class UserEntity : BaseEntity
{
    public string Account { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public Gender Sex { get; set; } = Gender.Male;

    public string Comments { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    public bool IsFactory { get; set; } = false;

    public bool IsLocked { get; set; } = false;

    public string UserRoleName { get; set; } = string.Empty;
    public int WrongPassLoginTimes { get; set; }
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/10/20 14:19:29           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.DatabaseService.Contract.Entities;

public class BaseEntity
{
    public string Id { get; set; } = string.Empty;

    public string Creator { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string Updater { get; set; } = string.Empty;

    public DateTime UpdateTime { get; set; } = DateTime.Now;
}

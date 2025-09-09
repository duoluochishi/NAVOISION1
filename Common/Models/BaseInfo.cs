//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/2 17:14:16           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Models;
public class BaseInfo
{
    public string Id { get; set; } = string.Empty;

    public string Creator { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; } = DateTime.Now;
}
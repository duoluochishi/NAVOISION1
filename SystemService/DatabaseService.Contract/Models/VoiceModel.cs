//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/9/5 13:55:19           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.DatabaseService.Contract.Models;

public class VoiceModel
{
    public string Id { get; set; } = string.Empty;

    public ushort InternalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string BodyPart { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public bool IsFront { get; set; }

    public ushort VoiceLength { get; set; }

    public string Language { get; set; } = string.Empty;

    public bool IsFactory { get; set; }

    public bool IsDefault { get; set; }

    public bool IsValid { get; set; }

    public string Creator { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; }
}

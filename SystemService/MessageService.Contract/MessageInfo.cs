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
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;

namespace NV.CT.MessageService.Contract;

public class MessageInfo
{
    public string Id { get; }

    public MessageLevel Level { get; set; } = MessageLevel.Info;

    public MessageSource Sender { get; set; } = MessageSource.Unknown;

    public string Content { get; set; } = null;

    public DateTime SendTime { get; set; } = DateTime.Now;

    public string Remark { get; set; } = null;

    public MessageInfo()
    {
        Id = IdGenerator.Next();
    }
}
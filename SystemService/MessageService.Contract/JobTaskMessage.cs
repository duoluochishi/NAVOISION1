//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.MessageService.Contract;
public class JobTaskMessage
{
    public string JobId { get; set; } = string.Empty;

    public MessageType MessageType { get; set; }        

    public JobTaskStatus JobStatus { get; set; }

    public int ProgressedCount { get; set; }

    public int TotalCount { get; set; }

    public int ErrorCode { get; set; } = 0;

    public string Content { get; set; } = string.Empty;
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/17 17:20:53           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.CTS.Models
{
    public class BaseCommandResult
    {
        /// <summary>
        /// 命令结果
        /// </summary>
        public CommandExecutionStatus Status { get; set; }

        /// <summary>
        /// (string, string) => (Code, Message)
        /// </summary>
        public List<(string Code, string Message)> Details { get; set; } = new();
    }

    public class RealtimeCommandResult : BaseCommandResult
    {
    }

    public class OfflineCommandResult : BaseCommandResult
    {
        public string TaskId { get; set; }

        public string ReconId { get; set; } = string.Empty;
    }

    public class DicomFileImportCommandResult : BaseCommandResult
    {

    }

    public class DicomFileExportCommandResult : BaseCommandResult { }

    public class FileArchiveCommandResult: BaseCommandResult { }

    public class ImagePrintCommandResult : BaseCommandResult { }

    public class LoadImageInstanceCommandResult : BaseCommandResult
    {
    }

    public class JobTaskCommandResult : BaseCommandResult { }

}

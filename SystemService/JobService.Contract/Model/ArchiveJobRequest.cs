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

namespace NV.CT.Job.Contract.Model
{
    public class ArchiveJobRequest : BaseJobRequest
    {
        public string StudyId { get; set; } = string.Empty;

        public List<string> SeriesIdList { get; set; }

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string AECaller { get; set; } = string.Empty;

        public string AETitle { get; set; } = string.Empty;

        /// <summary>
        /// 标记：基于Study级别归档，或是基于Series级别归档
        /// </summary>
        public ArchiveLevel ArchiveLevel { get; set; } = ArchiveLevel.Study;

        public string DicomTransferSyntax { get; set; }

        public bool UseTls { get; set; }
        public bool Anonymous { get; set; }

        public ArchiveJobRequest()
        {
            SeriesIdList = new List<string>();
        }

    }
}

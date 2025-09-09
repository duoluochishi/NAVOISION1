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

namespace NV.CT.DatabaseService.Contract.Models
{
    public class RawDataModel
    {

        public string Id { get; set; } = string.Empty;

        public string InternalStudyId { get; set; } = string.Empty;

        public string FrameOfReferenceUID { get; set; } = string.Empty;

        public string ScanId { get; set; } = string.Empty;

        public string ScanName { get; set; } = string.Empty;

        public string BodyPart { get; set; } = string.Empty;

        public string ProtocolName { get; set; } = string.Empty;

        public string PatientPosition { get; set; } = string.Empty;

        public string ScanModel { get; set; } = string.Empty;

        public DateTime? ScanEndTime { get; set; }

        public string Path { get; set; } = string.Empty;

        public bool IsExported { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public string Creator { get; set; }
        public DateTime CreateTime { get; set; }

    }
}

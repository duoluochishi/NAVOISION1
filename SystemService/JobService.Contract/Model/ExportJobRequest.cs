//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/5/16 13:45:36    V1.0.0         胡安
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS.Enums;

namespace NV.CT.Job.Contract.Model
{
    public class ExportJobRequest : BaseJobRequest
    {
        public string StudyId { get; set; } = string.Empty;

        public List<string> SeriesIdList { get; set; }

        public List<string> PatientNames { get; set; }

        public List<string> InputFolders { get; set; }
        public List<string> RTDDicomFolders { get; set; }

        public bool IsExportedToDICOM { get; set; } = false;

        public bool IsExportedToImage { get; set; } = false;

        public bool IsExportedToRawData { get; set; } = false;

        public bool IsCorrected { get; set; } = false;

        public bool IsAnonymouse { get; set; } = false;

        public string OutputFolder { get; set; } = string.Empty;

        public string OutputVirtualPath { get; set; } = string.Empty;

        public bool IsBurnToCDROM { get; set; } = false;

        public bool IsAddViewer { get; set; } = false;

        public FileExtensionType? PictureType { get; set; } = null;

        public string DicomTransferSyntax { get; set; }   

        /// <summary>
        /// 标记：基于Study级别操作，或是基于Series级别操作
        /// </summary>
        public OperationLevel OperationLevel { get; set; } = OperationLevel.Study;

        public ExportJobRequest()
        {
            InputFolders = new List<string>();
            SeriesIdList=new List<string>();
            RTDDicomFolders = new List<string>();
        }
    }
}

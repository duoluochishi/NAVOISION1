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
using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.Job.Contract.Model
{
    public class PrintJobRequest : BaseJobRequest
    {
        public string PatientId { get; set; } = string.Empty;

        public string StudyId { get; set; } = string.Empty;

        public string SeriesID { get; set; } = string.Empty;

        public string Host { get; set; }

        public int Port { get; set; }

        public string CallingAE { get; set; }

        public string CalledAE { get; set; }

        public string PageSize { get; set; }

        public bool IsColor { get; set; } = false;

        public int NumberOfCopies { get; set; } = 1;

        public string Orientation { get; set; } = Constants.DICOM_ORIENTATION_PORTRAIT;

        public List<string> ImagePathList { get; set; }
        
        public bool IsShowHeader { get; set; }

        public bool IsShowFooter { get; set; }

        public string Layout { get; set; } = string.Empty;

        public string PrintConfigPath { get; set; } = string.Empty;


        public PrintJobRequest()
        {
            ImagePathList = new List<string>();
        }

    }
}

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

namespace NV.CT.DatabaseService.Contract.Models
{
    public class SeriesModel
    {

        public string Id { get; set; } = string.Empty;

        public string InternalStudyId { get; set; } = string.Empty;

        public string ScanTaskId { get; set; } = string.Empty;

        public string ReconTaskId { get; set; } = string.Empty;

        public string SeriesNumber { get; set; } = string.Empty;

        public DateTime? SeriesDate { get; set; }

        public DateTime? SeriesTime { get; set; }

        public int StoreState { get; set; }

        public string SeriesDescription { get; set; } = string.Empty;

        public string WindowType { get; set; } = string.Empty;

        public string SeriesInstanceUID { get; set; } = string.Empty;

        private int _imageCount;
        public int ImageCount
        {
            get
            {
                if (SeriesDescription == "SR" || SeriesDescription == "DoseReport")
                {
                    return 1;
                }
                return _imageCount;
            }
            set => _imageCount = value;
        }

        private int _count;
        public int Count
        {
            get
            {
                if (SeriesDescription == "SR" || SeriesDescription == "DoseReport")
                {
                    return 1;
                }
                return _count;
            }
            set => _count = value;
        }

        public int SeriesStatus { get; set; }

        public string ReportPath { get; set; } = string.Empty;

        public int ArchiveStatus { get; set; }

        public int PrintStatus { get; set; }

        public int CorrectStatus { get; set; }

        public string BodyPart { get; set; } = string.Empty;
        public string Modality { get; set; } = string.Empty;
        public string SeriesType { get; set; } = string.Empty;
        public string FrameOfReferenceUID { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public string WindowWidth { get; set; } = string.Empty;
        public string WindowLevel { get; set; } = string.Empty;
        public DateTime ReconStartDate { get; set; }
        public DateTime ReconEndDate { get; set; }
        public bool IsDeleted { get; set; }
        public string PatientPosition { get; set; } = string.Empty;
        public string ProtocolName { get; set; } = string.Empty;
        public string ScanId { get; set; } = string.Empty;
        public string ReconId { get; set; } = string.Empty;
        public bool IsProtected { get; set; }

        public string SeriesPath { get; set; } = string.Empty;
    }
}

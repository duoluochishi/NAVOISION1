using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.NP.Tools.DataTransfer.Service
{
    public class ConstStrings
    {
        public const string SQLITE_NAME = "db_export.db";
        public const string SERIESSERVICE_TYPENAME = "NV.CT.DatabaseService.Impl.SeriesService";
        public const string RAWDATASERVICE_TYPENAME = "NV.CT.DatabaseService.Impl.RawDataService";

        public const string SERIES_ROOT_NAME = "DataMCS";
        public const string RAW_DATA_ROOT_NAME = "RawData";
        public const string DICOM_ROOT_NAME = "Dicom";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.PatientManagement.Models
{
    public class RawDataExport
    {
        public string StudyID { get; set; } = string.Empty;

        public List<NV.CT.DatabaseService.Contract.Models.RawDataModel> RawDataModels { get; set; }=new List<NV.CT.DatabaseService.Contract.Models.RawDataModel>();

        public string OutputDir { get; set; } = string.Empty;

        public List<string> RTDSeriesPathList { get; set; } = new List<string>();
    }
}

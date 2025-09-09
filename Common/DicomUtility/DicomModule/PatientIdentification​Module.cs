//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom;
using NV.CT.DicomUtility.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DicomUtility.DicomModule
{
    public class PatientIdentification​Module : IDicomDatasetUpdater
    {
        public string PatientName { get; set; }
        public string PatientID { get; set; }
        public string IssuerOfPatientID { get; set; }

        //public SQ IssuerOfPatientIDQualifierSequence{get;set;}
        //public SQ OtherPatientIDsSequence {get;set;}

        public void Read(DicomDataset ds)
        {
            PatientName = DicomContentHelper.GetDicomTag<string>(ds,DicomTag.PatientName);
            PatientID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.PatientID);
            IssuerOfPatientID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.IssuerOfPatientID);
            //IssuerOfPatientIDQualifierSequence.Read(ds);
            //OtherPatientIDsSequence.Read(ds);
        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.PatientName, PatientName);
            ds.AddOrUpdate(DicomTag.PatientID, PatientID);
            ds.AddOrUpdate(DicomTag.IssuerOfPatientID, IssuerOfPatientID);
            //IssuerOfPatientIDQualifierSequence.Update(ds);
            //OtherPatientIDsSequence.Update(ds);
        }
    }
}

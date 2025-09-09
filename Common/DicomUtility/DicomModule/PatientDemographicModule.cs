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
using NV.CT.DicomUtility.DicomCodeStringLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DicomUtility.DicomModule
{
    public class PatientDemographicModule : IDicomDatasetUpdater
    {
        public DateTime PatientBirthDate { get; set; }

        public PatientSexCS PatientSex { get; set; }

        //public SQ PatientPrimaryLanguageCodeSequence

        public double PatientWeight { get; set; }

        public double PatientSize { get; set; } 
        public string ConfidentialityConstraintOnPatientDataDescription { get; set; }
        public string CountryOfResidence { get; set; }

        public string RegionOfResidence { get; set; }

        public void Read(DicomDataset ds)
        {
            PatientBirthDate = DicomContentHelper.GetDicomDateTime(ds, DicomTag.PatientBirthDate, DicomTag.PatientBirthTime);
            PatientSex = DicomContentHelper.GetDicomTag<PatientSexCS>(ds,DicomTag.PatientSex);
            //PatientPrimaryLanguageCodeSequence.Read(ds);
            PatientWeight = DicomContentHelper.GetDicomTag<double>(ds, DicomTag.PatientWeight);
            PatientSize = DicomContentHelper.GetDicomTag<double>(ds, DicomTag.PatientSize);
            ConfidentialityConstraintOnPatientDataDescription = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ConfidentialityConstraintOnPatientDataDescription);
            CountryOfResidence = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.CountryOfResidence);
            RegionOfResidence = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.RegionOfResidence);
        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.PatientBirthDate, PatientBirthDate);
            ds.AddOrUpdate(DicomTag.PatientSex, PatientSex);
            //PatientPrimaryLanguageCodeSequence.Update(ds);
            ds.AddOrUpdate(DicomTag.PatientWeight, PatientWeight);
            ds.AddOrUpdate(DicomTag.PatientSize, PatientSize);
            ds.AddOrUpdate(DicomTag.ConfidentialityConstraintOnPatientDataDescription, ConfidentialityConstraintOnPatientDataDescription);
            ds.AddOrUpdate(DicomTag.CountryOfResidence, CountryOfResidence);
            ds.AddOrUpdate(DicomTag.RegionOfResidence, RegionOfResidence);

        }
    }
}

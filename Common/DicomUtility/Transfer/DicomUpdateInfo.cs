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

namespace NV.CT.DicomUtility.Transfer
{
    public class DicomUpdateInfo
    {
        public string PatientID { get; }
        public string PatientName { get; }

        public string PatientBirthDate { get; }

        public string PatientBirthTime { get; }

        public string PatientSex { get; }

        public string PatientAge { get; }

        public string PatientSize { get; }

        public string PatientWeight { get; }

        public string AccessionNumber { get; }

        public string ReferringPhysicianName { get; }

        public string StudyDescription { get; }

        public DicomUpdateInfo(string patientID,  
                               string patientName,
                               string patientBirthDate,
                               string patientBirthTime,
                               string patientSex,
                               string patientAge,
                               string patientSize,
                               string patientWeight,
                               string accessionNumber,
                               string referringPhysicianName,
                               string studyDescription
                               )
        {
            PatientID = patientID;
            PatientName = patientName;
            PatientBirthDate = patientBirthDate;
            PatientBirthTime = patientBirthTime;
            PatientSex = patientSex;
            PatientAge = patientAge;
            PatientSize = patientSize;
            PatientWeight = patientWeight;
            AccessionNumber = accessionNumber;
            ReferringPhysicianName = referringPhysicianName;
            StudyDescription = studyDescription;
        }
    }
}

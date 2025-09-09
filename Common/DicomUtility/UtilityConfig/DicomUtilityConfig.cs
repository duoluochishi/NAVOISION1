//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/29 13:23:31     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using FellowOakDicom;
using FellowOakDicom.Media;
using NV.MPS.Configuration;

namespace NV.CT.DicomUtility.UtilityConfig
{
    public static class DicomUtilityConfig
    {
        public static void InitializeDicomUtilityConfig()
        {
            MapDicomDirectoryContent();
            ConfigDicomImplementVersion();
        }

        private static void MapDicomDirectoryContent()
        {
            DicomDirectoryRecordType.Patient.Tags.Add(DicomTag.PatientSize);
            DicomDirectoryRecordType.Patient.Tags.Add(DicomTag.PatientWeight);
            DicomDirectoryRecordType.Patient.Tags.Add(DicomTag.PatientAddress);

            DicomDirectoryRecordType.Study.Tags.Add(DicomTag.ContentDate);
            DicomDirectoryRecordType.Study.Tags.Add(DicomTag.ContentTime);
            DicomDirectoryRecordType.Study.Tags.Add(DicomTag.BodyPartExamined);

            DicomDirectoryRecordType.Series.Tags.Add(DicomTag.PatientPosition);
            DicomDirectoryRecordType.Series.Tags.Add(DicomTag.BodyPartExamined);
            DicomDirectoryRecordType.Series.Tags.Add(DicomTag.ImageType);
            DicomDirectoryRecordType.Series.Tags.Add(DicomTag.WindowWidth);
            DicomDirectoryRecordType.Series.Tags.Add(DicomTag.WindowCenter);
            DicomDirectoryRecordType.Series.Tags.Add(DicomTag.SOPClassUID);
            DicomDirectoryRecordType.Series.Tags.Add(DicomTag.ContentDate);
            DicomDirectoryRecordType.Series.Tags.Add(DicomTag.ContentTime);
            DicomDirectoryRecordType.Series.Tags.Add(DicomTag.SeriesNumber);
        }

        private static void ConfigDicomImplementVersion()
        {
            DicomImplementation.Version = ProductConfig.ProductSettingConfig.ProductSetting.VersionName;
            DicomImplementation.ClassUID = new DicomUID(ProductConfig.ProductSettingConfig.ProductSetting.ImplementationClassUID, "Implementation Class UID", DicomUidType.Unknown) ;
        }
    }
}

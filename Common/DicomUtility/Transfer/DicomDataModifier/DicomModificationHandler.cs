using FellowOakDicom;
using NV.CT.CTS.Helpers;

namespace NV.CT.DicomUtility.Transfer.DicomDataModifier
{
 
    /// <summary>
    /// Dicom信息修改表，不针对指定序列，能够对唯一性ID进行修改
    /// </summary>
    public class DicomModificationHandler:IDicomDataModificationHandler
    {
        private DicomDataset modifier;
        public DicomModificationHandler()
        {
            modifier = new DicomDataset();
        }

        public void AddCorrection<T>(DicomTag dicomTag, T value)
        {
            modifier.AddOrUpdate(dicomTag, value);
        }

        public void ClearCorrection()
        {
            modifier.Clear();
        }

        public void ModifyDicomFile(DicomFile dcmFile)
        {
            ModifyDicomData(dcmFile.Dataset);
        }

        public void ModifyDicomData(DicomDataset dataset)
        {

            foreach (var item in modifier)
            {
                dataset.AddOrUpdate(item);
            }
            if(true)      //根据当前内容填写部分信息，包括内容时间戳，SopInstanceUID等
            {
                dataset.AddOrUpdate(DicomTag.ContentDate, DateTime.Now);
                dataset.AddOrUpdate(DicomTag.ContentTime, DateTime.Now);
                dataset.AddOrUpdate(DicomTag.SOPInstanceUID, UIDHelper.CreateSOPInstanceUID());
            }
        }
    }
}

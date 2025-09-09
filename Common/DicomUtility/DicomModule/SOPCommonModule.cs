//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using FellowOakDicom;

namespace NV.CT.DicomUtility.DicomModule
{
    public class SOPCommonModule:IDicomDatasetUpdater
    {
        #region TYPE1
        public string SpecificCharacterSet { get;set; }                          //	(0008,0005)
        public string SOPClassUID { get; set; }                                 //(0008,0016)
        public string SOPInstanceUID { get; set; }                              //(0008,0018)
        #endregion TYPE1 

        #region TYPE3
        public int InstanceNumber { get; set; }
        #endregion TYPE3
        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.SpecificCharacterSet, SpecificCharacterSet);
            ds.AddOrUpdate(DicomTag.SOPClassUID, SOPClassUID);
            ds.AddOrUpdate(DicomTag.SOPInstanceUID, SOPInstanceUID);
            ds.AddOrUpdate(DicomTag.InstanceNumber, InstanceNumber);
        }
        public void Read(DicomDataset ds)
        {
            SpecificCharacterSet = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SpecificCharacterSet);
            SOPClassUID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SOPClassUID);
            SOPInstanceUID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SOPInstanceUID);
            InstanceNumber = DicomContentHelper.GetDicomTag<int>(ds, DicomTag.InstanceNumber);
        }
    }
}

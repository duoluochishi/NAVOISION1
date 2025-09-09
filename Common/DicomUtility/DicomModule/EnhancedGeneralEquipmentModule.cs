//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using FellowOakDicom;

namespace NV.CT.DicomUtility.DicomModule
{
    public class EnhancedGeneralEquipmentModule:IDicomDatasetUpdater
    {
        #region TYPE1
        public string Manufacturer { get; set; }                        	//(0008,0070)
        public string ManufacturerModelName { get; set; }                   //(0008,1090)
        public string DeviceSerialNumber { get; set; }                      //(0018,1000)

        public string SoftwareVersions { get; set; }                        //(0018,1020)
        #endregion TYPE1

        #region TYPE2

        #endregion TYPE2

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.Manufacturer, Manufacturer);
            ds.AddOrUpdate(DicomTag.ManufacturerModelName, ManufacturerModelName);
            ds.AddOrUpdate(DicomTag.DeviceSerialNumber, DeviceSerialNumber);
            ds.AddOrUpdate(DicomTag.SoftwareVersions, SoftwareVersions);
        }
        public void Read(DicomDataset ds)
        {
            Manufacturer = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.Manufacturer);
            ManufacturerModelName = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ManufacturerModelName);
            DeviceSerialNumber = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.DeviceSerialNumber);
            SoftwareVersions = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.SoftwareVersions);
        }
    }
}

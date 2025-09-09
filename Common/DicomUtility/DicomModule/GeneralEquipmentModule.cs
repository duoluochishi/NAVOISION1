//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using FellowOakDicom;

namespace NV.CT.DicomUtility.DicomModule
{
    public class GeneralEquipmentModule:IDicomDatasetUpdater
    {

        #region TYPE1
        #endregion TYPE1

        #region TYPE2
        public string Manufacturer { get; set; }                           //(0008,0070)


        #endregion TYPE2


        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.Manufacturer, Manufacturer);
        }

        public void Read(DicomDataset ds)
        {
            Manufacturer = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.Manufacturer);
        }
    }
}

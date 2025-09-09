//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using FellowOakDicom;


namespace NV.CT.DicomUtility.Contract
{
    public interface IDicomDatasetUpdater
    {
        public void Update(DicomDataset ds);
        public void Read(DicomDataset ds);
    }
}

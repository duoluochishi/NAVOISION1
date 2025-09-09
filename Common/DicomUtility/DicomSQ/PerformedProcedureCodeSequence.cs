//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using FellowOakDicom;

namespace NV.CT.DicomUtility.DicomSQ
{
    /// <summary>
    /// (0040,A372) 当前没有实际内容，空实现。
    /// </summary>
    public class PerformedProcedureCodeSequence : IDicomDatasetUpdater
    {
        //todo
        public void Update(DicomDataset ds)
        {
            //DicomItem di = new DicomSequence(DicomTag.PerformedProcedureCodeSequence);

            //ds.AddOrUpdate(di);
            //Todo: 实现CodeSequence更新
        }
        public void Read(DicomDataset ds)
        {
            //Todo: 实现Sequence更新。
        }
    }
}

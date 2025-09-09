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
using NV.CT.DicomUtility.DicomMacro;

namespace NV.CT.DicomUtility.DicomSQ
{
    public class ReferencedPerformedProtocol​SQ:IDicomDatasetUpdater
    {
        public SopInstanceReferenceMacro SopInstanceReferenceMacro { get; set; }

        public ReferencedPerformedProtocol​SQ()
        {
            SopInstanceReferenceMacro = new SopInstanceReferenceMacro();
        }

        public void Read(DicomDataset ds)
        {
            var sequenceDS = DicomContentHelper.TryGetDicomSequence(ds, DicomTag.ReferencedDefinedProtocolSequence);
            if (sequenceDS is not null && sequenceDS.Items.Count >= 1)
            {
                SopInstanceReferenceMacro.Read(sequenceDS.Items[0]);
            }
        }

        public void Update(DicomDataset ds)
        {
            if (string.IsNullOrEmpty(SopInstanceReferenceMacro.ReferencedSOPClassUID)
                && string.IsNullOrEmpty(SopInstanceReferenceMacro.ReferencedSOPInstanceUID))
            {
                return;
            }
            var macroDataset = new DicomDataset();
            SopInstanceReferenceMacro.Update(macroDataset);
            var sequence = new DicomSequence(DicomTag.ReferencedDefinedProtocolSequence, macroDataset);
            ds.AddOrUpdate(sequence);
        }
    }
}

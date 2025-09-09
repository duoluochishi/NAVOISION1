//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using FellowOakDicom;
using NV.CT.DicomUtility.DicomMacro;

namespace NV.CT.DicomUtility.DicomSQ
{
    /// <summary>
    /// //(0008,1111)
    /// </summary>
    public class ReferencedPerformedProcedureStepSequence:IDicomDatasetUpdater
    {

        #region TYPE1
        public SopInstanceReferenceMacro SopInstanceReferenceMacro { get;set; }

        public ReferencedPerformedProcedureStepSequence()
        {
            SopInstanceReferenceMacro = new SopInstanceReferenceMacro();
        }
        #endregion TYPE1

        #region TYPE2
        #endregion TYPE2

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
        public void Read(DicomDataset ds)
        {
            var sequenceDS = DicomContentHelper.TryGetDicomSequence(ds, DicomTag.ReferencedDefinedProtocolSequence);
            if (sequenceDS is not null && sequenceDS.Items.Count >= 1)
            {
                SopInstanceReferenceMacro.Read(sequenceDS.Items[0]);
            }
        }
    }
}

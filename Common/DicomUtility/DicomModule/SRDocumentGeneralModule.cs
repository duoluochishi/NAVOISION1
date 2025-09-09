//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;
using NV.CT.DicomUtility.DicomSQ;
using FellowOakDicom;
using System;

namespace NV.CT.DicomUtility.DicomModule
{
    public class SRDocumentGeneralModule:IDicomDatasetUpdater
    {
        #region TYPE1
        public DateTime ContentDate { get; set; }                             //(0008,0023)
        public DateTime ContentTime { get; set; }                             //(0008,0033)
        public CompletionFlagCS CompletionFlag { get; set; }                          //(0040,A491)
                                                                            
        public VerificationFlagCS VerificationFlag { get; set; }                        //(0040,A493)

        #endregion TYPE1


        #region TYPE1C
        //todo
        #endregion TYPE1C

        #region TYPE2
        public PerformedProcedureCodeSequence PerformedProcedureCodeSequence{get;set; }        //(0040,A372)

        #endregion TYPE2

        public SRDocumentGeneralModule()
        {
            PerformedProcedureCodeSequence = new PerformedProcedureCodeSequence();
        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.ContentDate, ContentDate);
            ds.AddOrUpdate(DicomTag.ContentTime, ContentTime);
            ds.AddOrUpdate(DicomTag.CompletionFlag, CompletionFlag);
            ds.AddOrUpdate(DicomTag.VerificationFlag, VerificationFlag);

            PerformedProcedureCodeSequence.Update(ds);
        }
        public void Read(DicomDataset ds)
        {
            ContentDate = DicomContentHelper.GetDicomDateTime(ds, DicomTag.ContentDate, DicomTag.ContentTime);
            ContentTime = ContentDate;
            CompletionFlag = DicomContentHelper.GetDicomTag<CompletionFlagCS>(ds, DicomTag.CompletionFlag);
            VerificationFlag = DicomContentHelper.GetDicomTag<VerificationFlagCS>(ds, DicomTag.VerificationFlag);
            PerformedProcedureCodeSequence.Read(ds);
        }
    }
}

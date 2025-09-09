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
using NV.CT.DicomUtility.DicomCodeStringLib;
using NV.CT.DicomUtility.DicomMacro;
using NV.CT.DicomUtility.DicomSQ;

namespace NV.CT.DicomUtility.DicomModule
{

    /// <summary>
    /// 来源Part04 K6.1.2.2
    /// 具体定义part03 C.4.11
    /// 在Part04中未列出的暂不考虑。
    /// </summary>
    public class RequestedProcedureModule : IDicomDatasetUpdater
    {
        public string RequestedProcedureID​ { get; set; }
        public string RequestedProcedureDescription { get; set; }
        public RequestedProcedureCodeSQ RequestedProcedureCodeSQ { get; set; }

        //public CodeSequence RequestedLateralityCodeSequence { get; set; }
        public string StudyInstanceUID​ { get; set; }

        public DateTime StudyDateTime { get; set; }
        public ReferencedStudySQ ReferencedStudySQ​ { get; set; }
        public PriorityCS RequestedProcedurePriority { get; set; }

        public string PatientTransportArrangements​ { get; set; }

        public RequestedProcedureModule()
        {
            RequestedProcedureCodeSQ = new RequestedProcedureCodeSQ();
            ReferencedStudySQ​ = new ReferencedStudySQ​();
        }

        public void Read(DicomDataset ds)
        {
            RequestedProcedureID = DicomContentHelper.GetDicomTag<string>(ds,DicomTag.RequestedProcedureID);
            RequestedProcedureDescription = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.RequestedProcedureDescription);
            RequestedProcedureCodeSQ.Read(ds);
            //RequestedLateralityCodeSequence.Read(ds);
            StudyInstanceUID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.StudyInstanceUID);
            StudyDateTime = DicomContentHelper.GetDicomDateTime(ds, DicomTag.StudyDate,DicomTag.StudyTime);
            ReferencedStudySQ​.Read(ds);
            RequestedProcedurePriority = DicomContentHelper.GetDicomTag<PriorityCS>(ds, DicomTag.RequestedProcedurePriority);
            PatientTransportArrangements = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.PatientTransportArrangements);

        }

        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.RequestedProcedureID, RequestedProcedureID);
            ds.AddOrUpdate(DicomTag.RequestedProcedureDescription, RequestedProcedureDescription);
            RequestedProcedureCodeSQ.Update(ds);
            //RequestedLateralityCodeSequence.Update(ds);
            ds.AddOrUpdate(DicomTag.StudyInstanceUID, StudyInstanceUID);
            ds.AddOrUpdate(DicomTag.StudyDate, StudyDateTime);
            ds.AddOrUpdate(DicomTag.StudyTime, StudyDateTime);
            ReferencedStudySQ​.Update(ds);
            ds.AddOrUpdate(DicomTag.RequestedProcedurePriority, RequestedProcedurePriority);
            ds.AddOrUpdate(DicomTag.PatientTransportArrangements, PatientTransportArrangements);
        }

    }
}

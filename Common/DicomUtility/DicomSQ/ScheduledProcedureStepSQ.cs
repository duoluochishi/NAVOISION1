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

namespace NV.CT.DicomUtility.DicomSQ
{

    /// <summary>
    /// 见Part03 C.4.10
    /// </summary>
    public class ScheduledProcedureStepSQ : IDicomDatasetUpdater
    {

        public string StationAETitle { get; set; }

        public string ScheduledStationName { get; set; }
        public string ScheduledProcedureStepLocation { get; set; }

        public ReferencedDefinedProtocolSQ​ ReferencedDefinedProtocolSQ { get; set; }
        public ReferencedPerformedProtocol​SQ ReferencedPerformedProtocol​SQ { get; set; }

        public DateTime ScheduledProcedureStepStartDataTime { get; set; }

        public DateTime ScheduledProcedureStepEndDateTime​ { get; set; }

        public string ScheduledPerformingPhysicianName { get; set; }

        public ScheduledPerformingPhysician​IdentificationSQ ScheduledPerformingPhysician​IdentificationSQ { get; set; }

        public string ScheduledProcedureStep​Description​ { get; set; }
        public ScheduledProtocolCodeSQ ScheduledProtocolCodeSQ { get; set; }

        public string ScheduledProcedureStepID { get; set; }

        public ScheduledProcedureStepStatusCS ScheduledProcedureStepStatus { get; set; }  //Term

        public string CommentsontheScheduled​ProcedureStep { get; set; }

        public ModalityCS Modality { get; set; }            //Term
        public string RequestedContrastAgent { get; set; }    

        public string PreMedication { get; set; }

        public string AnatomicalOrientationType { get; set; }       //Term

        public ScheduledProcedureStepSQ()
        {
            ReferencedDefinedProtocolSQ = new ReferencedDefinedProtocolSQ();
            ReferencedPerformedProtocol​SQ = new ReferencedPerformedProtocolSQ();
            ScheduledPerformingPhysician​IdentificationSQ = new ScheduledPerformingPhysicianIdentificationSQ();
            ScheduledProtocolCodeSQ = new ScheduledProtocolCodeSQ();
        }

        public void Read(DicomDataset ds)
        {
            //Scheduled Procedure Step Sequence​应该有且只有一个Item
            var sq = DicomContentHelper.TryGetDicomSequence(ds, DicomTag.ScheduledProcedureStepSequence);

            if(sq.Items.Count == 0)
            {
                return;
            }
            var sqDataset = sq.Items[0];

            StationAETitle = DicomContentHelper.GetDicomTag<string>(sqDataset,DicomTag.StationAETitle);
            ScheduledProcedureStepStartDataTime = DicomContentHelper.GetDicomDateTime(sqDataset, DicomTag.ScheduledProcedureStepStartDate, DicomTag.ScheduledProcedureStepStartTime);
            Modality = DicomContentHelper.GetDicomTag<ModalityCS>(sqDataset, DicomTag.Modality);
            ScheduledPerformingPhysicianName = DicomContentHelper.GetDicomTag<string>(sqDataset, DicomTag.ScheduledPerformingPhysicianName);
            ScheduledProcedureStepDescription = DicomContentHelper.GetDicomTag<string>(sqDataset, DicomTag.ScheduledProcedureStepDescription);
            ScheduledStationName = DicomContentHelper.GetDicomTag<string>(sqDataset, DicomTag.ScheduledStationName);
            ScheduledProcedureStepLocation = DicomContentHelper.GetDicomTag<string>(sqDataset, DicomTag.ScheduledProcedureStepLocation);
            ReferencedDefinedProtocolSQ.Read(sqDataset);
            ReferencedPerformedProtocolSQ.Read(sqDataset);
            //ScheduledProtocolCodeSequence
            PreMedication = DicomContentHelper.GetDicomTag<string>(sqDataset, DicomTag.PreMedication);
            ScheduledProcedureStepID​ = DicomContentHelper.GetDicomTag<string>(sqDataset, DicomTag.ScheduledProcedureStepID​);
            RequestedContrastAgent​ = DicomContentHelper.GetDicomTag<string>(sqDataset, DicomTag.RequestedContrastAgent​);
            ScheduledProcedureStepStatus = DicomContentHelper.GetDicomTag<ScheduledProcedureStepStatusCS>(sqDataset, DicomTag.ScheduledProcedureStepStatus);
            //Other attributes not considerred yet.
        }

        public void Update(DicomDataset ds)
        {
            var sqDataset = new DicomDataset();

            //Handle dataset udpate
            sqDataset.AddOrUpdate(DicomTag.ScheduledStationAETitle, StationAETitle);
            sqDataset.AddOrUpdate(DicomTag.ScheduledProcedureStepStartDate, ScheduledProcedureStepStartDataTime);
            sqDataset.AddOrUpdate(DicomTag.ScheduledProcedureStepStartTime, ScheduledProcedureStepStartDataTime);
            sqDataset.AddOrUpdate(DicomTag.Modality, Modality);
            sqDataset.AddOrUpdate(DicomTag.ScheduledPerformingPhysicianName, ScheduledPerformingPhysicianName);
            sqDataset.AddOrUpdate(DicomTag.ScheduledProcedureStepDescription, ScheduledProcedureStepDescription);
            sqDataset.AddOrUpdate(DicomTag.ScheduledStationName, ScheduledStationName);
            sqDataset.AddOrUpdate(DicomTag.ScheduledProcedureStepLocation, ScheduledProcedureStepLocation);
            ReferencedDefinedProtocolSQ.Update(sqDataset);      //??
            ReferencedPerformedProtocolSQ.Update(sqDataset);       //??
            ScheduledProtocolCodeSQ.Update(sqDataset);      //??
            sqDataset.AddOrUpdate(DicomTag.PreMedication, PreMedication);
            sqDataset.AddOrUpdate(DicomTag.ScheduledProcedureStepID, ScheduledProcedureStepID);
            sqDataset.AddOrUpdate(DicomTag.RequestedContrastAgent, RequestedContrastAgent);
            sqDataset.AddOrUpdate(DicomTag.ScheduledProcedureStepStatus, ScheduledProcedureStepStatus);

            var sq = new DicomSequence(DicomTag.ScheduledProcedureStepSequence, sqDataset);
            ds.AddOrUpdate(sq);
        }
    }
}

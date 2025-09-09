//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/13 10:22:32     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using FellowOakDicom;
using NV.CT.CTS;
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;
using Microsoft.Extensions.Logging;
using NV.MPS.Exception;

namespace NV.CT.DicomUtility.Transfer.ModalityWorklist
{

    /// <summary>
    /// Worklist查询结果，仅获取当前支持内容，其他忽略。
    /// </summary>
    public class WorklistResult
    {
        /// Scheduled Procedure Step

        public string Modality { get; private set; }
        public DateTime ScheduledProcedureStepStartDateTime { get; private set; }
        public string ScheduledPerformingPhysicianName { get; private set; }

        public string ScheduledProcedureStepDescription { get; private set; }

        public string ScheduledProcedureStepID { get; private set; }

        public string ScheduledStationName { get; private set; }

        public string ScheduledProcedureStepLocation { get; private set; }

        /// Requested Procedure

        public string StudyInstanceUID { get; private set; }
        public string RequestedProcedureDescription { get; private set; }

        public string RequestedProcedureID { get; private set; }         //RequestedProcedureID can be used as default "Study ID";

        //public PriorityCS RequestedProcedurePriority { get;  private set;}     //好像没用
        //public PriorityCS ReportingPriority { get; private set;}              //好像没用

        /// Imaging Service Request

        public string AccessionNumber { get; private set; }

        public string ReferringPhysicianName { get; private set; }

        public string RequestingPhysician { get; private set; }

        /// IVisit Identification

        public string AdmissionID { get; private set; }

        /// Visit Status
        public string CurrentPatientLocation { get; private set; }

        /// Visit Admission
        public string AdmittingDiagnosisDescription { get; private set; }

        /// Patient Identification
        public string PatientID { get; private set; }
        public string PatientName { get; private set; }

        /// Patient Demographic
        public DateTime PatientBirthDateTime { get; private set; }
        public PatientSexCS PatientSex { get; private set; }
        public double PatientSize { get; private set; }
        public double PatientWeight { get; private set; }
        public string PatientAddress { get; private set; }
        //public string MilitaryRank { get; private set; }                  //军衔，貌似没用
        public string EthnicGroup { get; private set; }
        public string PatientComments { get; private set; }        

        /// Patient Medical

        public string MedicalAlerts { get; private set; }

        public string Allergies { get; private set; }
        public PregnancyStatusCS PregnancyStatus { get; private set; }

        public SmokingStatusCS SmokingStatus { get; private set; }
        //public DateTime LastMenstrualDate { get; private set; }           //貌似没用到
        public string AdditionalPatientHistory { get; private set; }
        public string SpecialNeeds { get; private set; }

        public string InstitutionName { get; private set; }
        public string InstitutionAddress { get; private set; }


        public static WorklistResult Create(DicomDataset wlResultDataset)
        {
            try
            {
                var item = new WorklistResult();
                item.Read(wlResultDataset);
                return item;
            }
            catch (NanoException ex){
                Global.Logger?.LogWarning($"Fail to parse worklist result from dataset {ex.Message}");
                return null;
            }
        }
        
        private void Read(DicomDataset wlResultDataset)
        {
            ReadScheduledProcedureStep(wlResultDataset);
            ReadRequestedProcedure(wlResultDataset);
            ReadImagingServiceRequest(wlResultDataset);
            ReadVisit(wlResultDataset);
            ReadPatient(wlResultDataset);
            ReadMedicalInfo(wlResultDataset);
            ReadInstitutionInfo(wlResultDataset);
        }

        private void ReadScheduledProcedureStep(DicomDataset wlResultDataset)
        {
            var spsSQ = DicomContentHelper.TryGetDicomSequence(wlResultDataset, DicomTag.ScheduledProcedureStepSequence);
            if (spsSQ is not null && spsSQ.Items.Count >= 1)
            {
                //todo: 以后是否应该支持一个Requested Procedure 多个Scheduled Procedure
                if (spsSQ.Items.Count > 1)
                {
                    Global.Logger.LogWarning("[DICOM] It's not supported to handle more than one Scheduled Procedure in one Request Procedure. Only handle the first One.");
                }
                var ds = spsSQ.Items[0];
                Modality = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.Modality);
                ScheduledProcedureStepStartDateTime = DicomContentHelper.GetDicomDateTime(ds, DicomTag.ScheduledProcedureStepStartDate,DicomTag.ScheduledProcedureStepStartTime);
                ScheduledPerformingPhysicianName = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ScheduledPerformingPhysicianName);
                ScheduledProcedureStepDescription = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ScheduledProcedureStepDescription);
                ScheduledProcedureStepID = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ScheduledProcedureStepID);
                ScheduledStationName = DicomContentHelper.GetDicomTag<string>(ds, DicomTag.ScheduledStationName);
            }
            else
            {
                throw new NanoException("0x0000000000", "Scheduled Procedure Step not exists in worklist dataset");
            }
        }

        private void ReadRequestedProcedure(DicomDataset wlResultDataset)
        {
            StudyInstanceUID = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.StudyInstanceUID);
            RequestedProcedureDescription = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.RequestedProcedureDescription);
            RequestedProcedureID = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.RequestedProcedureID);            
        }

        private void ReadImagingServiceRequest(DicomDataset wlResultDataset)
        {
            AccessionNumber = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.AccessionNumber);
            ReferringPhysicianName = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.ReferringPhysicianName);
            RequestingPhysician = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.RequestingPhysician);
        }

        private void ReadVisit(DicomDataset wlResultDataset)
        {
            AdmissionID = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.AdmissionID);
            CurrentPatientLocation = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.CurrentPatientLocation);
            AdmittingDiagnosisDescription = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.AdmittingDiagnosesDescription);
        }

        private void ReadPatient(DicomDataset wlResultDataset)
        {
            PatientName = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.PatientName);
            PatientID = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.PatientID);
            PatientBirthDateTime = DicomContentHelper.GetDicomDateTime(wlResultDataset, DicomTag.PatientBirthDate,DicomTag.PatientBirthTime);
            PatientSex = DicomContentHelper.GetDicomTag<PatientSexCS>(wlResultDataset, DicomTag.PatientSex);
            PatientSize = DicomContentHelper.GetDicomTag<double>(wlResultDataset, DicomTag.PatientSize);
            PatientWeight = DicomContentHelper.GetDicomTag<double>(wlResultDataset, DicomTag.PatientWeight);
            PatientAddress = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.PatientAddress);            
            EthnicGroup = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.EthnicGroup);
            PatientComments = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.PatientComments);
        }
        private void ReadMedicalInfo(DicomDataset wlResultDataset)
        {
            MedicalAlerts = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.MedicalAlerts);
            Allergies = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.Allergies);
            PregnancyStatus = DicomContentHelper.GetDicomTag<PregnancyStatusCS>(wlResultDataset, DicomTag.PregnancyStatus);
            SmokingStatus = DicomContentHelper.GetDicomTag<SmokingStatusCS>(wlResultDataset, DicomTag.SmokingStatus);
            AdditionalPatientHistory = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.AdditionalPatientHistory);
            SpecialNeeds = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.SpecialNeeds);
        }
        private void ReadInstitutionInfo(DicomDataset wlResultDataset)
        {
            InstitutionName = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.InstitutionName);
            InstitutionAddress = DicomContentHelper.GetDicomTag<string>(wlResultDataset, DicomTag.InstitutionAddress);
        }
    }
}

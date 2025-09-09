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
using NV.CT.DicomUtility.DicomModule;

namespace NV.CT.DicomUtility.DicomIOD
{
    /// <summary>
    /// Part04 K.6-1  
    /// </summary>
    public class WorklistIOD : IDicomDatasetUpdater
    {
        public ScheduledProcedureStepModule ScheduledProcedureStepModule { get; set; }
        public RequestedProcedureModule RequestedProcedureModule { get; set; }

        public ImagingServiceRequestModule ImagingServiceRequestModule { get; set; }

        public VisitIdentificationModule VisitIdentificationModule { get; set; }

        public VisitStatusModule VisitStatusModule { get; set; }

        public VisitRelationshipModule VisitRelationshipModule { get; set; }

        public VisitAdmissionModule VisitAdmissionModule { get; set; }

        public PatientIdentificationModule PatientIdentificationModule { get; set; }


        public PatientDemographicModule PatientDemographicModule { get; set; }

        public PatientMedicalModule PatientMedicalModule { get; set; }

        public WorklistIOD()
        {
            ScheduledProcedureStepModule = new ScheduledProcedureStepModule();
            RequestedProcedureModule = new RequestedProcedureModule();
            ImagingServiceRequestModule = new ImagingServiceRequestModule();
            VisitIdentificationModule = new VisitIdentificationModule();
            VisitStatusModule = new VisitStatusModule();
            VisitRelationshipModule = new VisitRelationshipModule();
            VisitAdmissionModule = new VisitAdmissionModule();
            PatientIdentificationModule = new PatientIdentificationModule();
            PatientDemographicModule = new PatientDemographicModule();
            PatientMedicalModule = new PatientMedicalModule();
        }

        public void Read(DicomDataset ds)
        {
            ScheduledProcedureStepModule.Read(ds);
            RequestedProcedureModule.Read(ds);
            ImagingServiceRequestModule.Read(ds);
            VisitIdentificationModule.Read(ds);
            VisitStatusModule.Read(ds);
            VisitRelationshipModule.Read(ds);
            VisitAdmissionModule.Read(ds);
            PatientIdentificationModule.Read(ds);
            PatientDemographicModule.Read(ds);
            PatientMedicalModule.Read(ds);
        }

        public void Update(DicomDataset ds)
        {
            ScheduledProcedureStepModule.Update(ds);
            RequestedProcedureModule.Update(ds);
            ImagingServiceRequestModule.Update(ds);
            VisitIdentificationModule.Update(ds);
            VisitStatusModule.Update(ds);
            VisitRelationshipModule.Update(ds);
            VisitAdmissionModule.Update(ds);
            PatientIdentificationModule.Update(ds);
            PatientDemographicModule.Update(ds);
            PatientMedicalModule.Update(ds);
        }
    }
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/14 09:42:22     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.DicomUtility.DicomCodeStringLib;

namespace NV.CT.DicomUtility.Transfer.MPPS
{
    /// <summary>
    /// MPPS N-Create 创建信息，用于MPPS中N-Create过程创建的IN PROGRESS信息
    /// </summary>
    public class MPPSNCreateInfo
    {
        public string AffectedInstanceUid { get; set; } = string.Empty;
        /// MODULE: Performed Procedure Step Relationship​
        public string StudyInstanceUID { get; set; } = string.Empty;
        /// empty Referenced Study Sequence​
        public string AccessionNumber { get; set; } = string.Empty;
        public string RequestedProcedureID { get; set; } = string.Empty;
        /// empty Requested Procedure Code Sequence
        public string RequestedProcedureDescription​ { get; set; } = string.Empty;
        public string ScheduledProcedureStepID​ { get; set; } = string.Empty;
        public string ScheduledProcedureStepDescription { get; set; } = string.Empty;
        /// empty Scheduled Procedure Code Sequence

        public string PatientName { get; set; } = string.Empty;
        public string PatientID { get; set; } = string.Empty;
        public DateTime PatientBirthDate { get; set; } = DateTime.Now;
        public PatientSexCS PatientSex { get; set; } = PatientSexCS.O;
        /// empty Referenced Patient Sequence
        public string AdmissionID { get; set; } = string.Empty;

        /// MODULE: Performed Procedure Step Information​
        public string PerformedProcedureStepID​ { get; set; } = string.Empty;
        public string PerformedStationAETitle { get; set; } = string.Empty;
        public string PerformedStationName { get; set; } = string.Empty;
        public DateTime PerformedProcedureStepStartDateTime { get; set; } = DateTime.Now;
        public string PerformedProcedureStepDescription { get; set; } = string.Empty;

        /// empty Procedure Code Sequence
        public ModalityCS Modality { get; set; } = ModalityCS.CT;
        public string StudyID { get; set; } = string.Empty;
        /// empty Performed Protocol Code Sequence


        /// Image Acquisition Results​
    }
}

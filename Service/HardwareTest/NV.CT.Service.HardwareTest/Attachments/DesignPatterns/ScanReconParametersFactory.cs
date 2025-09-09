using System;
using Microsoft.VisualBasic;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.Common.Models.ScanReconModels;

namespace NV.CT.Service.HardwareTest.Attachments.DesignPatterns
{
    public static class ScanReconParametersFactory
    {
        /// <summary>
        /// 创建Patient
        /// </summary>
        /// <returns></returns>
        public static PatientModel CreatePatient()
        {
            var birthTime = new DateTime(2000, 1, 1, 12, 30, 30);
            return new PatientModel
            {
                Name = "HardwareTest",
                ID = "PID-0001",
                Sex = PatientSex.F,
                BirthDate = birthTime,
                BirthTime = birthTime,
                Age = $"{DateAndTime.DateDiff(DateInterval.Year, birthTime, DateTime.Now):000}Y",
                Size = 178,
                Weight = 60,
                AdditionalHistory = "Additional Patient History",
                MedicalAlerts = "Medical Alerts",
                ContrastAller = "Contrast Allert Comments",
                OtherPatientIDs = "Other ID",
                EthnicGroup = "汉",
                Comment = "Patient Comments",
                PregnancyStatus = PregnantStatus.None,
                PatientStatus = "Sober",
            };
        }

        /// <summary>
        /// 创建Study
        /// </summary>
        /// <returns></returns>
        public static StudyModel CreateStudy()
        {
            return new StudyModel
            {
                StudyInstanceUID = "2024010100000001",
                StudyID = "StudyID",
                AccessionNumber = "AccNO123",
                StudyDescription = "StudyDescription",
                AdmissionID = string.Empty,
                StudyDate = DateTime.UtcNow,
                StudyTime = DateTime.UtcNow,
                ReferringPhysiciansName = "Referring Physician Name",
                RequestingPhysician = "Requesttin Physician",
                RequestingService = "Requesting Service",
                RequestedProcedurePriority = RequestedProcedurePriority.HIGH,
                SpecialNeeds = "Special Needs",
                PhysiciansOfRecord = "PhysicianOfRecord",
                NameOfPhysiciansReadingStudy = "Physician Reading Study Name",
                RequestedProcedureID = "ReqProcedureID",
                RequestedProcedureDescription = "Request Procedure Description",
                ScheduledProcedureStepID = string.Empty,
                ScheduledProcedureStepDescription = string.Empty,
                OperatorsName = "OperatorName",
            };
        }
    }
}
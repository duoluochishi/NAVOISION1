using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Framework;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.Common.Models.ScanReconModels
{
    /// <inheritdoc cref="Study"/>
    public class StudyModel : ViewModelBase
    {
        #region Field

        private string _studyInstanceUID = string.Empty;
        private string _studyID = string.Empty;
        private string _accessionNumber = string.Empty;
        private string _admissionID = string.Empty;
        private string _studyDescription = string.Empty;
        private DateTime? _studyDate;
        private DateTime? _studyTime;
        private string _referringPhysiciansName = string.Empty;
        private string _requestingPhysician = string.Empty;
        private string _requestingService = string.Empty;
        private RequestedProcedurePriority? _requestedProcedurePriority;
        private string _specialNeeds = string.Empty;
        private string _physiciansOfRecord = string.Empty;
        private string _nameOfPhysiciansReadingStudy = string.Empty;
        private string _requestedProcedureID = string.Empty;
        private string _requestedProcedureDescription = string.Empty;
        private string _scheduledProcedureStepID = string.Empty;
        private string _scheduledProcedureStepDescription = string.Empty;
        private string _operatorsName = string.Empty;

        #endregion

        #region Property

        /// <inheritdoc cref="Study.StudyInstanceUID"/>
        public string StudyInstanceUID
        {
            get => _studyInstanceUID;
            set => SetProperty(ref _studyInstanceUID, value);
        }

        /// <inheritdoc cref="Study.StudyID"/>
        public string StudyID
        {
            get => _studyID;
            set => SetProperty(ref _studyID, value);
        }

        /// <inheritdoc cref="Study.AccessionNumber"/>
        public string AccessionNumber
        {
            get => _accessionNumber;
            set => SetProperty(ref _accessionNumber, value);
        }

        /// <inheritdoc cref="Study.AdmissionID"/>
        public string AdmissionID
        {
            get => _admissionID;
            set => SetProperty(ref _admissionID, value);
        }

        /// <inheritdoc cref="Study.StudyDescription"/>
        public string StudyDescription
        {
            get => _studyDescription;
            set => SetProperty(ref _studyDescription, value);
        }

        /// <inheritdoc cref="Study.StudyDate"/>
        public DateTime? StudyDate
        {
            get => _studyDate;
            set => SetProperty(ref _studyDate, value);
        }

        /// <inheritdoc cref="Study.StudyTime"/>
        public DateTime? StudyTime
        {
            get => _studyTime;
            set => SetProperty(ref _studyTime, value);
        }

        /// <inheritdoc cref="Study.ReferringPhysiciansName"/>
        public string ReferringPhysiciansName
        {
            get => _referringPhysiciansName;
            set => SetProperty(ref _referringPhysiciansName, value);
        }

        /// <inheritdoc cref="Study.RequestingPhysician"/>
        public string RequestingPhysician
        {
            get => _requestingPhysician;
            set => SetProperty(ref _requestingPhysician, value);
        }

        /// <inheritdoc cref="Study.RequestingService"/>
        public string RequestingService
        {
            get => _requestingService;
            set => SetProperty(ref _requestingService, value);
        }

        /// <inheritdoc cref="Study.RequestedProcedurePriority"/>
        public RequestedProcedurePriority? RequestedProcedurePriority
        {
            get => _requestedProcedurePriority;
            set => SetProperty(ref _requestedProcedurePriority, value);
        }

        /// <inheritdoc cref="Study.SpecialNeeds"/>
        public string SpecialNeeds
        {
            get => _specialNeeds;
            set => SetProperty(ref _specialNeeds, value);
        }

        /// <inheritdoc cref="Study.PhysiciansOfRecord"/>
        public string PhysiciansOfRecord
        {
            get => _physiciansOfRecord;
            set => SetProperty(ref _physiciansOfRecord, value);
        }

        /// <inheritdoc cref="Study.NameOfPhysiciansReadingStudy"/>
        public string NameOfPhysiciansReadingStudy
        {
            get => _nameOfPhysiciansReadingStudy;
            set => SetProperty(ref _nameOfPhysiciansReadingStudy, value);
        }

        /// <inheritdoc cref="Study.RequestedProcedureID"/>
        public string RequestedProcedureID
        {
            get => _requestedProcedureID;
            set => SetProperty(ref _requestedProcedureID, value);
        }

        /// <inheritdoc cref="Study.RequestedProcedureDescription"/>
        public string RequestedProcedureDescription
        {
            get => _requestedProcedureDescription;
            set => SetProperty(ref _requestedProcedureDescription, value);
        }

        /// <inheritdoc cref="Study.ScheduledProcedureStepID"/>
        public string ScheduledProcedureStepID
        {
            get => _scheduledProcedureStepID;
            set => SetProperty(ref _scheduledProcedureStepID, value);
        }

        /// <inheritdoc cref="Study.ScheduledProcedureStepDescription"/>
        public string ScheduledProcedureStepDescription
        {
            get => _scheduledProcedureStepDescription;
            set => SetProperty(ref _scheduledProcedureStepDescription, value);
        }

        /// <inheritdoc cref="Study.OperatorsName"/>
        public string OperatorsName
        {
            get => _operatorsName;
            set => SetProperty(ref _operatorsName, value);
        }

        #endregion

        public Study Converter()
        {
            return Global.Instance.ServiceProvider.GetRequiredService<IMapper>().Map<Study>(this);
        }
    }
}
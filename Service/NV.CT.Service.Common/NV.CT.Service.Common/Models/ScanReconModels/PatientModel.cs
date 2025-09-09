using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Framework;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.Common.Models.ScanReconModels
{
    /// <inheritdoc cref="Patient"/>
    public class PatientModel : ViewModelBase
    {
        #region Field

        private string _name = string.Empty;
        private string _id = string.Empty;
        private PatientSex? _sex;
        private DateTime? _birthDate;
        private DateTime? _birthTime;
        private string _age = string.Empty;
        private double? _size;
        private double? _weight;
        private string _additionalHistory = string.Empty;
        private string _medicalAlerts = string.Empty;
        private string _contrastAller = string.Empty;
        private string _otherPatientIDs = string.Empty;
        private string _ethnicGroup = string.Empty;
        private string _comment = string.Empty;
        private PregnantStatus? _pregnancyStatus;
        private string _patientStatus = string.Empty;

        #endregion

        #region Property

        /// <inheritdoc cref="Patient.Name"/>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <inheritdoc cref="Patient.ID"/>
        public string ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <inheritdoc cref="Patient.Sex"/>
        public PatientSex? Sex
        {
            get => _sex;
            set => SetProperty(ref _sex, value);
        }

        /// <inheritdoc cref="Patient.BirthDate"/>
        public DateTime? BirthDate
        {
            get => _birthDate;
            set => SetProperty(ref _birthDate, value);
        }

        /// <inheritdoc cref="Patient.BirthTime"/>
        public DateTime? BirthTime
        {
            get => _birthTime;
            set => SetProperty(ref _birthTime, value);
        }

        /// <inheritdoc cref="Patient.Age"/>
        public string Age
        {
            get => _age;
            set => SetProperty(ref _age, value);
        }

        /// <inheritdoc cref="Patient.Size"/>
        public double? Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        /// <inheritdoc cref="Patient.Weight"/>
        public double? Weight
        {
            get => _weight;
            set => SetProperty(ref _weight, value);
        }

        /// <inheritdoc cref="Patient.AdditionalHistory"/>
        public string AdditionalHistory
        {
            get => _additionalHistory;
            set => SetProperty(ref _additionalHistory, value);
        }

        /// <inheritdoc cref="Patient.MedicalAlerts"/>
        public string MedicalAlerts
        {
            get => _medicalAlerts;
            set => SetProperty(ref _medicalAlerts, value);
        }

        /// <inheritdoc cref="Patient.ContrastAller"/>
        public string ContrastAller
        {
            get => _contrastAller;
            set => SetProperty(ref _contrastAller, value);
        }

        /// <inheritdoc cref="Patient.OtherPatientIDs"/>
        public string OtherPatientIDs
        {
            get => _otherPatientIDs;
            set => SetProperty(ref _otherPatientIDs, value);
        }

        /// <inheritdoc cref="Patient.EthnicGroup"/>
        public string EthnicGroup
        {
            get => _ethnicGroup;
            set => SetProperty(ref _ethnicGroup, value);
        }

        /// <inheritdoc cref="Patient.Comment"/>
        public string Comment
        {
            get => _comment;
            set => SetProperty(ref _comment, value);
        }

        /// <inheritdoc cref="Patient.PregnancyStatus"/>
        public PregnantStatus? PregnancyStatus
        {
            get => _pregnancyStatus;
            set => SetProperty(ref _pregnancyStatus, value);
        }

        /// <inheritdoc cref="Patient.PatientStatus"/>
        public string PatientStatus
        {
            get => _patientStatus;
            set => SetProperty(ref _patientStatus, value);
        }

        #endregion

        public Patient Converter()
        {
            return Global.Instance.ServiceProvider.GetRequiredService<IMapper>().Map<Patient>(this);
        }
    }
}
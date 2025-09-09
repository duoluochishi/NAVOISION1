using NV.CT.DatabaseService.Contract.Models;
using NV.CT.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace NV.CT.NP.Tools.DataTransfer.Model
{
    public class VStudyModel : BaseViewModel
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _patientName = string.Empty;
        public string PatientName
        {
            get => _patientName;
            set => SetProperty(ref _patientName, value);
        }

        private string _patientId = string.Empty;
        public string PatientId
        {
            get => _patientId;
            set => SetProperty(ref _patientId, value);
        }

        private string _studyInstanceUID = string.Empty;
        public string StudyInstanceUID
        {
            get => _studyInstanceUID;
            set => SetProperty(ref _studyInstanceUID, value);
        }

        private DateTime? _studyTime;
        public DateTime? StudyTime
        {
            get => _studyTime;
            set => SetProperty(ref _studyTime, value);
        }

        private string _bodyPart = string.Empty;
        public string BodyPart
        {
            get => _bodyPart is null ? string.Empty : _bodyPart;
            set => SetProperty(ref _bodyPart, value);
        }

        private ExportStatus _exportStatus = ExportStatus.None;
        public ExportStatus ExportStatus
        {
            get => _exportStatus;
            set => SetProperty(ref _exportStatus, value);
        }

        private List<SeriesModel> _series;
        public List<SeriesModel> Series
        {
            get => _series;
            set => SetProperty(ref _series, value);
        }

        private List<RawDataModel> _rawData;
        public List<RawDataModel> RawData
        {
            get => _rawData;
            set => SetProperty(ref _rawData, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
    }

    public enum ExportStatus
    {
        None = 0,
        Success = 1,
        Fail = 2
    }
}

using NV.CT.CTS.Enums;
using System;
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Print.Models
{
    public class PrintingSeriesModel : BaseViewModel
    {
        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _internalStudyId = string.Empty;
        public string InternalStudyId
        {
            get => _internalStudyId;
            set => SetProperty(ref _internalStudyId, value);
        }

        private string _seriesInstanceUID = string.Empty;
        public string SeriesInstanceUID
        {
            get => _seriesInstanceUID;
            set => SetProperty(ref _seriesInstanceUID, value);
        }

        private string _bodyPart = string.Empty;
        public string BodyPart
        {
            get => _bodyPart;
            set => SetProperty(ref _bodyPart, value);
        }

        private string _modality = string.Empty;
        public string Modality
        {
            get => _modality;
            set => SetProperty(ref _modality, value);
        }

        private string _seriesType = string.Empty;
        public string SeriesType
        {
            get => _seriesType;
            set => SetProperty(ref _seriesType, value);
        }

        private string _imageType = string.Empty;
        public string ImageType
        {
            get => _imageType;
            set => SetProperty(ref _imageType, value);
        }

        private string _seriesDescription = string.Empty;
        public string SeriesDescription
        {
            get => _seriesDescription;
            set => SetProperty(ref _seriesDescription, value);
        }

        private string _patientPosition = string.Empty;
        public string PatientPosition
        {
            get => _patientPosition;
            set => SetProperty(ref _patientPosition, value);
        }

        private string _seriesPath = string.Empty;
        public string SeriesPath
        {
            get => _seriesPath;
            set => SetProperty(ref _seriesPath, value);
        }

        private DateTime? _reconStartDate;
        public DateTime? ReconStartDate
        {
            get => _reconStartDate;
            set => SetProperty(ref _reconStartDate, value);
        }

        private DateTime? _reconEndDate;
        public DateTime? ReconEndDate
        {
            get => _reconEndDate;
            set => SetProperty(ref _reconEndDate, value);
        }
        
    }
}

using System;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.QualityTest.Services;

namespace NV.CT.Service.QualityTest.Models
{
    public sealed class ReportHeadInfoModel : ViewModelBase
    {
        private DateTime _generationDate;
        private DateTime? _lastSessionDate;
        private string? _reason;
        private string? _customerName;
        private string? _customerAddress;
        private string? _systemSN;
        private string? _phantomSn;
        private string? _softwareVersion;
        private string? _executedBy;

        public ReportHeadInfoModel(IDataStorageService dataStorageService)
        {
            LastSessionDate = dataStorageService.GetReportLastSessionDate();
        }

        public DateTime GenerationDate
        {
            get => _generationDate;
            set => SetProperty(ref _generationDate, value);
        }

        public DateTime? LastSessionDate
        {
            get => _lastSessionDate;
            set => SetProperty(ref _lastSessionDate, value);
        }

        public string? Reason
        {
            get => _reason;
            set => SetProperty(ref _reason, value);
        }

        public string? CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public string? CustomerAddress
        {
            get => _customerAddress;
            set => SetProperty(ref _customerAddress, value);
        }

        public string? SystemSN
        {
            get => _systemSN;
            set => SetProperty(ref _systemSN, value);
        }

        public string? PhantomSN
        {
            get => _phantomSn;
            set => SetProperty(ref _phantomSn, value);
        }

        public string? SoftwareVersion
        {
            get => _softwareVersion;
            set => SetProperty(ref _softwareVersion, value);
        }

        public string? ExecutedBy
        {
            get => _executedBy;
            set => SetProperty(ref _executedBy, value);
        }
    }
}
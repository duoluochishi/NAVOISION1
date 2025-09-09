using NV.CT.CTS.Enums;
using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.ComponentModel;

namespace NV.CT.JobViewer.Models
{
    public class OfflineReconModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? StudyUID { get; set; } = string.Empty;

        private string? _patientId = string.Empty;
        public string? PatientId
        {
            get => _patientId;
            set
            {
                _patientId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PatientId)));
            }
        }

        private string? _scanId = string.Empty;
        public string? ScanId
        {
            get => _scanId;
            set
            {
                _scanId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScanId)));
            }
        }

        private string? _reconId = string.Empty;
        public string? ReconId
        {
            get => _reconId;
            set
            {
                _reconId = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReconId)));
            }
        }

        private string? _seriesUID = string.Empty;
        public string? SeriesUID
        {
            get => _seriesUID;
            set
            {
                _seriesUID = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SeriesUID)));
            }
        }

        public int TotalCount { get; set; }

        public int FinishCount { get; set; }

        private float _progress;

        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }

        public string? ImagePath { get; set; } = string.Empty;

        public string? LastImage { get; set; } = string.Empty;

        public bool IsOver { get; set; }

        private TaskPriority _priority = TaskPriority.Middle;
        public TaskPriority Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Priority)));
            }
        }

        private OfflineTaskStatus _status;
        public OfflineTaskStatus Status
        {
            get => _status;
            set
            {
                //if (value == OfflineReconStatus.Finished)
                //{
                //    Progress = 1;
                //}
                //else
                //{
                //    if (value == OfflineReconStatus.Created)
                //    {
                //        Progress = 0;
                //    }
                //}
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        private string _showStatus;
        public string ShowStatus
        {
            get => _showStatus;
            set
            {
                _showStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowStatus)));
            }
        }

        private bool _startEnabled = false;
        public bool StartEnabled
        {
            get => _startEnabled;
            set
            {
                _startEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartEnabled)));
            }
        }

        private bool _cancelEnabled = false;
        public bool CancelEnabled
        {
            get => _cancelEnabled;
            set
            {
                _cancelEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CancelEnabled)));
            }
        }

        private bool _deleteEnabled = false;
        public bool DeleteEnabled
        {
            get => _deleteEnabled;
            set
            {
                _deleteEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeleteEnabled)));
            }
        }

        private bool _upgradeEnabled = false;
        public bool UpgradeEnabled
        {
            get => _upgradeEnabled;
            set
            {
                _upgradeEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpgradeEnabled)));
            }
        }

        private bool _downgradeEnabled = false;
        public bool DowngradeEnabled
        {
            get => _downgradeEnabled;
            set
            {
                _downgradeEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DowngradeEnabled)));
            }
        }

        private bool _showOrange = false;
        public bool ShowOrange
        {
            get => _showOrange;
            set
            {
                _showOrange = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowOrange)));
            }
        }

        private bool _showBlue = false;
        public bool ShowBlue
        {
            get => _showBlue;
            set
            {
                _showBlue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowBlue)));
            }
        }

        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Index)));
            }

        }

        public string? MachineName { get; set; } = string.Empty;

        private string? _patientName = string.Empty;
        public string? PatientName
        {
            get => _patientName;
            set
            {
                _patientName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PatientName)));
            }
        }

        private string? _seriesDescription = string.Empty;
        public string? SeriesDescription
        {
            get => _seriesDescription;
            set
            {
                _seriesDescription = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SeriesDescription)));
            }
        }

        private DateTime _reconTaskDateTime;

        public DateTime ReconTaskDateTime
        {
            get => _reconTaskDateTime;
            set
            {
                _reconTaskDateTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReconTaskDateTime)));
            }
        }

        public DateTime CreateTime { get; set; }
    }
}

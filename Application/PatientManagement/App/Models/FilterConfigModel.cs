//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/2 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using Prism.Mvvm;

namespace NV.CT.PatientManagement.Models
{
    public class FilterConfigModel : BindableBase
    {
        private SearchTimeType _studyDateRangeType;
        public SearchTimeType StudyDateRangeType
        { 
            get => _studyDateRangeType;
            set
            {
                this.SetProperty(ref _studyDateRangeType, value);
                this.ResetStudyDateRange(value);
            } 
        }

        private DateTime? _studyDateRangeBeginDate;
        public DateTime? StudyDateRangeBeginDate
        {
            get => _studyDateRangeBeginDate;
            set => this.SetProperty(ref _studyDateRangeBeginDate, value);
        }

        private DateTime? _studyDateRangeEndDate;
        public DateTime? StudyDateRangeEndDate
        {
            get => _studyDateRangeEndDate;
            set => this.SetProperty(ref _studyDateRangeEndDate, value);
        }

        private bool _isInProgressCheckedOfStudyStatus = true;
        public bool IsInProgressCheckedOfStudyStatus
        { 
            get => _isInProgressCheckedOfStudyStatus; 
            set => this.SetProperty(ref _isInProgressCheckedOfStudyStatus, value);        
        }

        private bool _isFinishedCheckedOfStudyStatus = true;
        public bool IsFinishedCheckedOfStudyStatus
        {
            get => _isFinishedCheckedOfStudyStatus;
            set => this.SetProperty(ref _isFinishedCheckedOfStudyStatus, value);
        }

        private bool _isAbnormalCheckedOfStudyStatus = true;
        public bool IsAbnormalCheckedOfStudyStatus
        {
            get => _isAbnormalCheckedOfStudyStatus;
            set => this.SetProperty(ref _isAbnormalCheckedOfStudyStatus, value);
        }

        private bool _isNotYetCheckedOfPrintStatus = true;
        public bool IsNotYetCheckedOfPrintStatus
        {
            get => _isNotYetCheckedOfPrintStatus;
            set => this.SetProperty(ref _isNotYetCheckedOfPrintStatus, value);
        }

        private bool _isFinishedCheckedOfPrintStatus = true;
        public bool IsFinishedCheckedOfPrintStatus
        {
            get => _isFinishedCheckedOfPrintStatus;
            set => this.SetProperty(ref _isFinishedCheckedOfPrintStatus, value);
        }

        private bool _isFailedCheckedOfPrintStatus = true;
        public bool IsFailedCheckedOfPrintStatus
        {
            get => _isFailedCheckedOfPrintStatus;
            set => this.SetProperty(ref _isFailedCheckedOfPrintStatus, value);
        }

        private bool _isNotYetCheckedOfArchiveStatus = true;
        public bool IsNotYetCheckedOfArchiveStatus
        {
            get => _isNotYetCheckedOfArchiveStatus;
            set => this.SetProperty(ref _isNotYetCheckedOfArchiveStatus, value);
        }

        private bool _isFinishedCheckedOfArchiveStatus = true;
        public bool IsFinishedCheckedOfArchiveStatus
        {
            get => _isFinishedCheckedOfArchiveStatus;
            set => this.SetProperty(ref _isFinishedCheckedOfArchiveStatus, value);
        }

        private bool _isPartlyFinishedCheckedOfArchiveStatus = true;
        public bool IsPartlyFinishedCheckedOfArchiveStatus
        {
            get => _isPartlyFinishedCheckedOfArchiveStatus;
            set => this.SetProperty(ref _isPartlyFinishedCheckedOfArchiveStatus, value);
        }

        private bool _isFailedCheckedOfArchiveStatus = true;
        public bool IsFailedCheckedOfArchiveStatus
        {
            get => _isFailedCheckedOfArchiveStatus;
            set => this.SetProperty(ref _isFailedCheckedOfArchiveStatus, value);
        }

        private bool _isCorrectedChecked = true;
        public bool IsCorrectedChecked
        {
            get => _isCorrectedChecked;
            set => this.SetProperty(ref _isCorrectedChecked, value);
        }

        private bool _isUncorrectedChecked = true;
        public bool IsUncorrectedChecked
        {
            get => _isUncorrectedChecked;
            set => this.SetProperty(ref _isUncorrectedChecked, value);
        }

        private bool _isUnlockedChecked = true;
        public bool IsUnlockedChecked
        {
            get => _isUnlockedChecked;
            set => this.SetProperty(ref _isUnlockedChecked, value);
        }

        private bool _isLockedChecked = true;
        public bool IsLockedChecked
        {
            get => _isLockedChecked;
            set => this.SetProperty(ref _isLockedChecked, value);
        }

        private bool _isMaleChecked = true;
        public bool IsMaleChecked
        {
            get => _isMaleChecked;
            set => this.SetProperty(ref _isMaleChecked, value);
        }

        private bool _isFemaleChecked = true;
        public bool IsFemaleChecked
        {
            get => _isFemaleChecked;
            set => this.SetProperty(ref _isFemaleChecked, value);
        }

        private bool _isOtherChecked = true;
        public bool IsOtherChecked
        {
            get => _isOtherChecked;
            set => this.SetProperty(ref _isOtherChecked, value);
        }

        private bool _isLocalChecked = true;
        public bool IsLocalChecked
        {
            get => _isLocalChecked;
            set => this.SetProperty(ref _isLocalChecked, value);
        }

        private bool _isPreRegChecked = true;
        public bool IsPreRegChecked
        {
            get => _isPreRegChecked;
            set => this.SetProperty(ref _isPreRegChecked, value);
        }

        private bool _isEmergencyChecked = true;
        public bool IsEmergencyChecked
        {
            get => _isEmergencyChecked;
            set => this.SetProperty(ref _isEmergencyChecked, value);
        }

        private DateTime? _birthdayRangeBeginDate;
        public DateTime? BirthdayRangeBeginDate
        {
            get => _birthdayRangeBeginDate;
            set => this.SetProperty(ref _birthdayRangeBeginDate, value);
        }

        private DateTime? _birthdayRangeEndDate;
        public DateTime? BirthdayRangeEndDate
        {
            get => _birthdayRangeEndDate;
            set => this.SetProperty(ref _birthdayRangeEndDate, value);
        }

        private StudyListColumn _sortedColumnName;
        public StudyListColumn SortedColumnName
        {
            get => _sortedColumnName;
            set => this.SetProperty(ref _sortedColumnName, value);
        }

        private bool _isAscendingSort = false;
        public bool IsAscendingSort
        {
            get => _isAscendingSort;
            set => this.SetProperty(ref _isAscendingSort, value);
        }

        private bool _isStudyDateEnabled = false;
        public bool IsStudyDateEnabled
        {
            get
            {
                return _isStudyDateEnabled;
            }
            set
            {
                this.SetProperty(ref this._isStudyDateEnabled, value);
            }
        }

        private void ResetStudyDateRange(SearchTimeType searchTimeType)
        {
            switch (searchTimeType)
            {
                case SearchTimeType.Today:
                    this.StudyDateRangeBeginDate = DateTime.Now;
                    this.StudyDateRangeEndDate = DateTime.Now;
                    this.IsStudyDateEnabled = false;
                    break;
                case SearchTimeType.Yesterday:
                    this.StudyDateRangeBeginDate = DateTime.Now.AddDays(-1);
                    this.StudyDateRangeEndDate = DateTime.Now.AddDays(-1);
                    this.IsStudyDateEnabled = false;
                    break;
                case SearchTimeType.DayBeforeYesterday:
                    this.StudyDateRangeBeginDate = DateTime.Now.AddDays(-2);
                    this.StudyDateRangeEndDate = DateTime.Now.AddDays(-2);
                    IsStudyDateEnabled = false;
                    break;
                case SearchTimeType.Last7Days:
                    this.StudyDateRangeBeginDate = DateTime.Now.AddDays(-7);
                    this.StudyDateRangeEndDate = DateTime.Now;
                    this.IsStudyDateEnabled = false;
                    break;
                case SearchTimeType.Last30Days:
                    this.StudyDateRangeBeginDate = DateTime.Now.AddDays(-30);
                    this.StudyDateRangeEndDate = DateTime.Now;
                    this.IsStudyDateEnabled = false;
                    break;
                case SearchTimeType.All:
                    this.StudyDateRangeBeginDate = DateTime.MinValue;
                    this.StudyDateRangeEndDate = DateTime.Now;
                    IsStudyDateEnabled = false;
                    break;
                case SearchTimeType.Custom:
                    this.StudyDateRangeBeginDate = null;
                    this.StudyDateRangeEndDate = null;
                    this.IsStudyDateEnabled = true;
                    break;
                default:
                    this.StudyDateRangeBeginDate = null;
                    this.StudyDateRangeEndDate = null;
                    this.IsStudyDateEnabled = false;
                    break;  
            }
        
        }

    }

}

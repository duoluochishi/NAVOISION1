//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/2 11:01:27     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.Language;
using NV.CT.PatientManagement.Models;
using System.Numerics;

namespace NV.CT.PatientManagement.ViewModel
{
    public class StudyListFilterViewModel : BaseViewModel
    {
        private readonly IMapper _mapper;
        private readonly ILogger<StudyListFilterViewModel> _logger;
        private readonly IFilterConfigService _filterConfigService;
        private readonly string _dateTimeFormat = "yyyy-MM-dd";

        private FilterConfigModel _filterConfig = new FilterConfigModel();
        public FilterConfigModel FilterConfig
        { 
            get
            {
                return _filterConfig;
            }
            set
            {
                this.SetProperty(ref _filterConfig, value);
            }
        }

        private List<SearchTimeType> _dateRangeTypes = new List<SearchTimeType>();
        public List<SearchTimeType> DateRangeTypes
        { 
            get 
            { 
                return _dateRangeTypes; 
            }
            set 
            {
                this.SetProperty(ref this._dateRangeTypes, value);
            }        
        }

        private List<StudyListColumn> _sortedColumnsList = new List<StudyListColumn>();
        public List<StudyListColumn> SortedColumnsList
        {
            get
            {
                return _sortedColumnsList;
            }
            set
            {
                this.SetProperty(ref this._sortedColumnsList, value);
            }
        }

        private bool _isStatusSectionExpanded = false;
        public bool IsStatusSectionExpanded
        {
            get
            {
                return _isStatusSectionExpanded;
            }
            set
            {
                this.SetProperty(ref _isStatusSectionExpanded, value);
                this.ToolTipStatusSection = value ? LanguageResource.TooTip_Fold : LanguageResource.ToolTip_Expand;
            }
        }

        private string _toolTipStatusSection;
        public string ToolTipStatusSection
        {
            get
            {
                return _toolTipStatusSection;
            }
            set
            {
                this.SetProperty(ref _toolTipStatusSection, value);
            }
        }

        private bool _isOtherSectionExpanded = false;
        public bool IsOtherSectionExpanded
        {
            get
            {
                return _isOtherSectionExpanded;
            }
            set
            {
                this.SetProperty(ref _isOtherSectionExpanded, value);
                this.ToolTipOtherSection = value ? LanguageResource.TooTip_Fold : LanguageResource.ToolTip_Expand;
            }
        }

        private string _toolTipOtherSection;
        public string ToolTipOtherSection
        {
            get
            {
                return _toolTipOtherSection;
            }
            set
            {
                this.SetProperty(ref _toolTipOtherSection, value);                
            }
        }

        public StudyListFilterViewModel(IMapper mapper, ILogger<StudyListFilterViewModel> logger, IFilterConfigService filterConfigService)
        {             
            this._mapper = mapper;
            this._logger = logger;
            this._filterConfigService = filterConfigService;

            Commands.Add(PatientManagementConstants.COMMAND_SAVE, new DelegateCommand(OnSave));
            Commands.Add(PatientManagementConstants.COMMAND_CLEAR_DATETIME, new DelegateCommand<object>(OnClearDateTime));
            Commands.Add(PatientManagementConstants.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
            Commands.Add(PatientManagementConstants.COMMAND_EXPAND, new DelegateCommand<object>(OnExpand));


            this.Initialize();
        }

        public void Initialize()
        {
            DateRangeTypes.Clear();
            DateRangeTypes.AddRange(EnumHelper.GetAllItems<SearchTimeType>());
            SortedColumnsList.Clear();
            SortedColumnsList.AddRange(EnumHelper.GetAllItems<StudyListColumn>());

            var filterConfig = _filterConfigService.GetConfigs();
            FilterConfig.StudyDateRangeType = filterConfig.StudyDateRange.DateRangeType;
            if (FilterConfig.StudyDateRangeType is SearchTimeType.Custom)
            {
                FilterConfig.StudyDateRangeBeginDate = ConvertStringToDateTime(filterConfig.StudyDateRange.BeginDate);
                FilterConfig.StudyDateRangeEndDate = ConvertStringToDateTime(filterConfig.StudyDateRange.EndDate);
            }

            FilterConfig.IsInProgressCheckedOfStudyStatus = filterConfig.StudyStatus.IsInProgressChecked;
            FilterConfig.IsFinishedCheckedOfStudyStatus = filterConfig.StudyStatus.IsFinishedChecked;
            FilterConfig.IsAbnormalCheckedOfStudyStatus = filterConfig.StudyStatus.IsAbnormalChecked;
            FilterConfig.IsNotYetCheckedOfPrintStatus = filterConfig.PrintStatus.IsNotyetChecked;
            FilterConfig.IsFinishedCheckedOfPrintStatus = filterConfig.PrintStatus.IsFinishedChecked;
            FilterConfig.IsFailedCheckedOfPrintStatus = filterConfig.PrintStatus.IsFailedChecked;
            FilterConfig.IsNotYetCheckedOfArchiveStatus = filterConfig.ArchiveStatus.IsNotyetChecked;
            FilterConfig.IsFinishedCheckedOfArchiveStatus = filterConfig.ArchiveStatus.IsFinishedChecked;
            FilterConfig.IsPartlyFinishedCheckedOfArchiveStatus = filterConfig.ArchiveStatus.IsPartlyFinishedChecked;
            FilterConfig.IsFailedCheckedOfArchiveStatus = filterConfig.ArchiveStatus.IsFailedChecked;
            FilterConfig.IsCorrectedChecked = filterConfig.CorrectionStatus.IsCorrected;
            FilterConfig.IsUncorrectedChecked = filterConfig.CorrectionStatus.IsUncorrected;
            FilterConfig.IsUnlockedChecked = filterConfig.LockStatus.IsUnlockedChecked;
            FilterConfig.IsLockedChecked = filterConfig.LockStatus.IsLockedChecked;
            FilterConfig.IsMaleChecked = filterConfig.Sex.IsMaleChecked;
            FilterConfig.IsFemaleChecked = filterConfig.Sex.IsFemaleChecked;
            FilterConfig.IsOtherChecked = filterConfig.Sex.IsOtherChecked;
            FilterConfig.IsLocalChecked = filterConfig.PatientType.IsLocalChecked;
            FilterConfig.IsPreRegChecked = filterConfig.PatientType.IsPreRegChecked;
            FilterConfig.IsEmergencyChecked = filterConfig.PatientType.IsEmergencyChecked;
            FilterConfig.BirthdayRangeBeginDate = ConvertStringToDateTime(filterConfig.BirthdayDateRange.BeginDate);
            FilterConfig.BirthdayRangeEndDate = ConvertStringToDateTime(filterConfig.BirthdayDateRange.EndDate);
            FilterConfig.SortedColumnName = filterConfig.SortedColumn.ColumnName;
            FilterConfig.IsAscendingSort = filterConfig.SortedColumn.IsAscending;
        }

        private void OnSave()
        {
            var filterConfig = new FilterConfig();
            filterConfig.StudyDateRange.DateRangeType = FilterConfig.StudyDateRangeType;
            if (filterConfig.StudyDateRange.DateRangeType is SearchTimeType.Custom)
            {
                filterConfig.StudyDateRange.BeginDate = FilterConfig.StudyDateRangeBeginDate.HasValue ? FilterConfig.StudyDateRangeBeginDate.Value.ToString(_dateTimeFormat) : string.Empty;
                filterConfig.StudyDateRange.EndDate = FilterConfig.StudyDateRangeEndDate.HasValue ? FilterConfig.StudyDateRangeEndDate.Value.ToString(_dateTimeFormat) : string.Empty;
            }
            else
            {
                filterConfig.StudyDateRange.BeginDate = string.Empty;
                filterConfig.StudyDateRange.EndDate = string.Empty;
            }

            filterConfig.StudyStatus.IsInProgressChecked = FilterConfig.IsInProgressCheckedOfStudyStatus;
            filterConfig.StudyStatus.IsFinishedChecked = FilterConfig.IsFinishedCheckedOfStudyStatus;
            filterConfig.StudyStatus.IsAbnormalChecked = FilterConfig.IsAbnormalCheckedOfStudyStatus;
            filterConfig.PrintStatus.IsNotyetChecked = FilterConfig.IsNotYetCheckedOfPrintStatus;
            filterConfig.PrintStatus.IsFailedChecked = FilterConfig.IsFailedCheckedOfPrintStatus;
            filterConfig.PrintStatus.IsFinishedChecked = FilterConfig.IsFinishedCheckedOfPrintStatus;
            filterConfig.ArchiveStatus.IsNotyetChecked = FilterConfig.IsNotYetCheckedOfArchiveStatus;
            filterConfig.ArchiveStatus.IsPartlyFinishedChecked = FilterConfig.IsPartlyFinishedCheckedOfArchiveStatus;
            filterConfig.ArchiveStatus.IsFinishedChecked = FilterConfig.IsFinishedCheckedOfArchiveStatus;
            filterConfig.ArchiveStatus.IsFailedChecked = FilterConfig.IsFailedCheckedOfArchiveStatus;
            filterConfig.CorrectionStatus.IsCorrected = FilterConfig.IsCorrectedChecked;
            filterConfig.CorrectionStatus.IsUncorrected = FilterConfig.IsUncorrectedChecked;
            filterConfig.LockStatus.IsUnlockedChecked = FilterConfig.IsUnlockedChecked;
            filterConfig.LockStatus.IsLockedChecked = FilterConfig.IsLockedChecked;
            filterConfig.Sex.IsMaleChecked = FilterConfig.IsMaleChecked;
            filterConfig.Sex.IsFemaleChecked = FilterConfig.IsFemaleChecked;
            filterConfig.Sex.IsOtherChecked = FilterConfig.IsOtherChecked;
            filterConfig.PatientType.IsLocalChecked = FilterConfig.IsLocalChecked;
            filterConfig.PatientType.IsPreRegChecked = FilterConfig.IsPreRegChecked;
            filterConfig.PatientType.IsEmergencyChecked = FilterConfig.IsEmergencyChecked;
            filterConfig.BirthdayDateRange.BeginDate = FilterConfig.BirthdayRangeBeginDate.HasValue ? FilterConfig.BirthdayRangeBeginDate.Value.ToString(_dateTimeFormat) : string.Empty;
            filterConfig.BirthdayDateRange.EndDate = FilterConfig.BirthdayRangeEndDate.HasValue ? FilterConfig.BirthdayRangeEndDate.Value.ToString(_dateTimeFormat) : string.Empty;
            filterConfig.SortedColumn.ColumnName = FilterConfig.SortedColumnName;
            filterConfig.SortedColumn.IsAscending = FilterConfig.IsAscendingSort;

            _filterConfigService.Save(filterConfig);
        }

        private void OnClearDateTime(object param)
        {
            string parameter = param.ToString();
            if (parameter == "study")
            {
                FilterConfig.StudyDateRangeBeginDate = null;
                FilterConfig.StudyDateRangeEndDate = null;
            }
            else if (parameter == "birthday")
            {
                FilterConfig.BirthdayRangeBeginDate = null;
                FilterConfig.BirthdayRangeEndDate = null;
            }
        }

        private void OnExpand(object param)
        {         
            var parameter = param.ToString();
            if (parameter == "status")
            {
                this.IsStatusSectionExpanded = !this.IsStatusSectionExpanded;
            }
            else if (parameter == "other")
            {
                this.IsOtherSectionExpanded = !this.IsOtherSectionExpanded;
            }

        }

        public void Closed(object parameter)
        {
            if (parameter is Window window)
            {
                window.Hide();
            }
        }

        private DateTime? ConvertStringToDateTime(string dateTime)
        {
            if (string.IsNullOrEmpty(dateTime))
            {
                return null;
            }

            DateTime convertedTime;
            if (DateTime.TryParse(dateTime, out convertedTime))
            {
                return convertedTime;
            }
            else
            {
                return null;
            }
        }

    }
}

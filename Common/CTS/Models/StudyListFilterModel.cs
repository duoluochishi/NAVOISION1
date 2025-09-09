using NV.CT.CTS.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.CTS.Models
{
    public class StudyListFilterModel
    {
        public DateTime? DateRangeBeginDate { get; set; }

        public DateTime? DateRangeEndDate { get; set; }

        public bool IsInProgressCheckedOfStudyStatus { get; set; }

        public bool IsFinishedCheckedOfStudyStatus { get; set; }


        public bool IsAbnormalCheckedOfStudyStatus { get; set; }


        public bool IsNotYetCheckedOfPrintStatus { get; set; }


        public bool IsFinishedCheckedOfPrintStatus { get; set; }


        public bool IsFailedCheckedOfPrintStatus { get; set; }

        public bool IsNotYetCheckedOfArchiveStatus { get; set; }

        public bool IsFinishedCheckedOfArchiveStatus { get; set; }

        public bool IsPartlyFinishedCheckedOfArchiveStatus { get; set; }

        public bool IsFailedCheckedOfArchiveStatus { get; set; }

        public bool IsCorrectedChecked { get; set; }

        public bool IsUncorrectedChecked { get; set; }

        public bool IsUnlockedChecked { get; set; }

        public bool IsLockedChecked { get; set; }

        public bool IsMaleChecked { get; set; }

        public bool IsFemaleChecked { get; set; }

        public bool IsOtherChecked { get; set; }

        public bool IsLocalChecked { get; set; }

        public bool IsPreRegChecked { get; set; }

        public bool IsEmergencyChecked { get; set; }

        public DateTime? BirthdayRangeBeginDate { get; set; }

        public DateTime? BirthdayRangeEndDate { get; set; }

        public StudyListColumn SortedColumnName { get; set; }

        public bool IsAscendingSort { get; set; }

    }

}

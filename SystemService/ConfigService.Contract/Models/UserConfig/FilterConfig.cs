//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/2 10:05:13     V1.0.0        胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using System.Xml.Serialization;

namespace NV.CT.ConfigService.Models.UserConfig;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("FilterConfig", IsNullable = false)]
public class FilterConfig : BaseConfig
{
    [XmlElement("StudyDateRange")]
    public StudyDateRange StudyDateRange { get; set; } = new();

    [XmlElement("SortedColumn")]
    public SortedColumn SortedColumn { get; set; } = new();

    [XmlElement("StudyStatus")]
    public StudyStatus StudyStatus { get; set; } = new();

    [XmlElement("PrintStatus")]
    public PrintStatus PrintStatus { get; set; } = new();

    [XmlElement("ArchiveStatus")]
    public ArchiveStatus ArchiveStatus { get; set; } = new();

    [XmlElement("CorrectionStatus")]
    public CorrectionStatus CorrectionStatus { get; set; } = new();

    [XmlElement("LockStatus")]
    public LockStatus LockStatus { get; set; } = new();

    [XmlElement("Sex")]
    public Sex Sex { get; set; } = new();

    [XmlElement("PatientType")]
    public PatientType PatientType { get; set; } = new();

    [XmlElement("BirthdayDateRange")]
    public BirthdayDateRange BirthdayDateRange { get; set; } = new();
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("StudyDateRange", IsNullable = false)]
public class StudyDateRange
{
    [XmlAttribute]
    public SearchTimeType DateRangeType { get; set; }

    [XmlAttribute]
    public string BeginDate { get; set; }

    [XmlAttribute]
    public string EndDate { get; set; }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("SortedColumn", IsNullable = false)]
public class SortedColumn
{
    [XmlAttribute]
    public StudyListColumn ColumnName { get; set; }

    [XmlAttribute]
    public bool IsAscending { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("StudyStatus", IsNullable = false)]
public class StudyStatus
{
    [XmlAttribute]
    public bool IsInProgressChecked { get; set; }

    [XmlAttribute]
    public bool IsFinishedChecked { get; set; }

    [XmlAttribute]
    public bool IsAbnormalChecked { get; set; }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("PrintStatus", IsNullable = false)]
public class PrintStatus
{
    [XmlAttribute]
    public bool IsNotyetChecked { get; set; }

    [XmlAttribute]
    public bool IsFinishedChecked { get; set; }

    [XmlAttribute]
    public bool IsFailedChecked { get; set; }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("ArchiveStatus", IsNullable = false)]
public class ArchiveStatus
{
    [XmlAttribute]
    public bool IsNotyetChecked { get; set; }

    [XmlAttribute]
    public bool IsFinishedChecked { get; set; }

    [XmlAttribute]
    public bool IsPartlyFinishedChecked { get; set; }

    [XmlAttribute]
    public bool IsFailedChecked { get; set; }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CorrectionStatus", IsNullable = false)]
public class CorrectionStatus
{
    [XmlAttribute]
    public bool IsCorrected { get; set; }

    [XmlAttribute]
    public bool IsUncorrected { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("LockStatus", IsNullable = false)]
public class LockStatus
{
    [XmlAttribute]
    public bool IsLockedChecked { get; set; }

    [XmlAttribute]
    public bool IsUnlockedChecked { get; set; }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("Sex", IsNullable = false)]
public class Sex
{
    [XmlAttribute]
    public bool IsMaleChecked { get; set; }

    [XmlAttribute]
    public bool IsFemaleChecked { get; set; }

    [XmlAttribute]
    public bool IsOtherChecked { get; set; }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("PatientType", IsNullable = false)]
public class PatientType
{
    [XmlAttribute]
    public bool IsLocalChecked { get; set; }

    [XmlAttribute]
    public bool IsPreRegChecked { get; set; }

    [XmlAttribute]
    public bool IsEmergencyChecked { get; set; }

}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("BirthdayDateRange", IsNullable = false)]
public class BirthdayDateRange
{
    [XmlAttribute]
    public string BeginDate { get; set; }

    [XmlAttribute]
    public string EndDate { get; set; }

}
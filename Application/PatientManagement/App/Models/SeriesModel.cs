//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.PatientManagement.Helpers;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace NV.CT.PatientManagement.Models;

public class SeriesModel : INotifyPropertyChanged
{
    private string _id = string.Empty;
    public string Id
    {
        get => _id;
        set => _id = value;
    }

    private string _studyId = string.Empty;
    public string StudyId
    {
        get => _studyId;
        set
        {
            _studyId = value;
            OnChangedProperty("StudyId");
        }
    }

    private string _scanTaskId = string.Empty;
    public string ScanTaskId
    {
        get => _scanTaskId;
        set
        {
            _scanTaskId = value;
            OnChangedProperty("ScanTaskId");
        }
    }

    private string _reconTaskId = string.Empty;
    public string ReconTaskId
    {
        get => _reconTaskId;
        set
        {
            _reconTaskId = value;
            OnChangedProperty("ReconTaskId");
        }
    }

    private string _seriesNumber = string.Empty;
    public string SeriesNumber
    {
        get => _seriesNumber;
        set
        {
            _seriesNumber = value;
            OnChangedProperty("SeriesNumber");
        }
    }

    private DateTime? _seriesDate;
    public DateTime? SeriesDate
    {
        get => _seriesDate;
        set
        {
            _seriesDate = value;
            OnChangedProperty("SeriesDate");
        }
    }

    private DateTime? _seriesTime;
    public DateTime? SeriesTime
    {
        get => _seriesTime;
        set
        {
            _seriesTime = value;
            OnChangedProperty("SeriesTime");
        }
    }

    private int _storeState;
    public int StoreState
    {
        get => _storeState;
        set
        {
            _storeState = value;
            OnChangedProperty("StoreState");
        }
    }

    private string _seriesdescription = string.Empty;
    public string SeriesDescription
    {
        get => _seriesdescription;
        set
        {
            _seriesdescription = value;
            OnChangedProperty("SeriesDescription");
        }
    }

    private string _windowType = string.Empty;
    public string WindowType
    {
        get => _windowType;
        set
        {
            _windowType = value;
            OnChangedProperty("WindowType");
        }
    }

    private string _seriesInstanceUID = string.Empty;
    public string SeriesInstanceUID
    {
        get => _seriesInstanceUID;
        set
        {
            _seriesInstanceUID = value;
            OnChangedProperty("SeriesInstanceUID");
        }
    }

    private int _imageCount;
    public int ImageCount
    {
        get => (SeriesDescription == "SR" || SeriesDescription == "DoseReport") ? 1 : _imageCount;
        set
        {
            _imageCount = value;
            OnChangedProperty("ImageCount");
        }
    }

    private int _count;
    public int Count
    {
        get
        {
            if (SeriesDescription == "SR" || SeriesDescription == "DoseReport")
            {
                return 1;
            }
            return _count;
        }
        set
        {
            _count = value;
            OnChangedProperty("Count");
        }
    }

    private int _seriesStatus;
    public int SeriesStatus
    {
        get => _seriesStatus;
        set
        {
            _seriesStatus = value;
            OnChangedProperty("SeriesStatus");
        }
    }

    private string _reportPath = string.Empty;
    public string ReportPath
    {
        get => _reportPath;
        set
        {
            _reportPath = value;
            OnChangedProperty("ReportPath");
        }
    }

    private JobTaskStatus _archiveStatus;
    public JobTaskStatus ArchiveStatus
    {
        get => _archiveStatus;
        set
        {
            _archiveStatus = value;
            OnChangedProperty("ArchiveStatus");
        }
    }

    private JobTaskStatus _printStatus;
    public JobTaskStatus PrintStatus
    {
        get => _printStatus;
        set
        {
            _printStatus = value;
            OnChangedProperty("PrintStatus");
        }
    }

    private string _bodyPart = string.Empty;
    public string BodyPart
    {
        get => _bodyPart;
        set
        {
            _bodyPart = value;
            OnChangedProperty("BodyPart");
        }
    }

    private string _seriesType = string.Empty;
    public string SeriesType
    {
        set
        {
            _seriesType = value;
            OnChangedProperty("SeriesType");
        }
        get => _seriesType;
    }

    private string _imageType = string.Empty;
    public string ImageType
    {
        get => _imageType;
        set
        {
            _imageType = value;
            OnChangedProperty("ImageType");
        }
    }

    private string _seriesPath = string.Empty;
    public string SeriesPath
    {
        get => _seriesPath;
        set
        {
            _seriesPath = value;
            OnChangedProperty("SeriesPath");
        }
    }

    private string _patientPosition = string.Empty;
    public string PatientPosition
    {
        get => _patientPosition;
        set
        {
            _patientPosition = value;
            OnChangedProperty("PatientPosition");
        }
    }

    private DateTime? _reconEndDate;
    public DateTime? ReconEndDate
    {
        get
        {
            return _reconEndDate;
        }
        set
        {
            _reconEndDate = value;

        }
    }

    public string ScanId { get; set; } = string.Empty;
    public string ReconId { get; set; } = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnChangedProperty(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

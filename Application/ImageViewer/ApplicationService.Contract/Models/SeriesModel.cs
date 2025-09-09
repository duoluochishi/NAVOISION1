namespace NV.CT.ImageViewer.ApplicationService.Contract.Models;

public partial class SeriesModel
{
    private string id = string.Empty;
    private string internalstudyId = string.Empty;
    private string scanTaskId = string.Empty;
    private string reconTaskId = string.Empty;
    private string reconId = string.Empty;
    public string SeriesNumber { get; set; } = string.Empty;
    //暂不使用
    //private string seriesNumber;
    private DateTime? seriesDate;
    private DateTime? seriesTime;
    private int storeState;
    //暂不使用
    //private string parameters;
    private string seriesdescription = string.Empty;
    private string window = string.Empty;
    private string seriesInstanceUID = string.Empty;
    private int imageCount;
    private int seriesStatus;
    //暂不使用
    //private bool isChecked;
    //暂不使用
    //private int isArchive;
    private string reportPath = string.Empty;
    private int archiveStatus;
    private int printStatus;
    private int correctStatus;
    private int count;
    private string bodyPart = string.Empty;
    private string seriesType = string.Empty;
    private string imageType = string.Empty;
    private string seriesPath = string.Empty;
    private string patientPosition = string.Empty;
    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:False
    /// </summary>   

    public string Id
    {
        get => id;
        set => id = value;
    }


    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string InternalStudyId
    {
        get => internalstudyId;
        set => internalstudyId = value;
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string ScanTaskId
    {
        get => scanTaskId;
        set => scanTaskId = value;
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string ReconTaskId
    {
        get => reconTaskId;
        set => reconTaskId = value;
    }



    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public DateTime? SeriesDate
    {
        get => seriesDate;
        set => seriesDate = value;
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public DateTime? SeriesTime
    {
        get => seriesTime;
        set => seriesTime = value;
    }

    /// <summary>
    /// Desc:
    /// Default:1
    /// Nullable:False
    /// </summary>           
    public int StoreState
    {
        get => storeState;
        set => storeState = value;
    }



    public string SeriesDescription
    {
        get => seriesdescription;
        set => seriesdescription = value;
    }
    public string Window
    {
        get => window;
        set => window = value;
    }
    public string SeriesInstanceUID
    {
        get => seriesInstanceUID;
        set => seriesInstanceUID = value;
    }

    public int ImageCount
    {
        get
        {
            if (SeriesDescription == "SR" || SeriesDescription == "DoseReport")
            {
                return 1;
            }
            return imageCount;
        }
        set => imageCount = value;
    }

    public int Count
    {
        get
        {
            if (SeriesDescription == "SR" || SeriesDescription == "DoseReport")
            {
                return 1;
            }
            return count;
        }
        set => count = value;
    }

    public int SeriesStatus
    {
        get => seriesStatus;
        set => seriesStatus = value;
    }

    public string ReportPath
    {
        get => reportPath;
        set => reportPath = value;
    }

    public string ReconId
    {
        get => reconId;
        set => reconId = value;
    }

    public int ArchiveStatus
    {
        get => archiveStatus;
        set => archiveStatus = value;
    }
    public int PrintStatus
    {
        get => printStatus;
        set => printStatus = value;
    }

    public int CorrectStatus
    {
        get => correctStatus;
        set => correctStatus = value;
    }

    public string BodyPart
    {
        get => bodyPart;
        set => bodyPart = value;
    }
    public string SeriesType
    {
        set => seriesType = value;
        get => seriesType;
    }
    public string ImageType
    {
        get => imageType;
        set => imageType = value;
    }
    public string SeriesPath
    {
        get => seriesPath;
        set => seriesPath = value;
    }
    public string PatientPosition
    {
        get => patientPosition;
        set => patientPosition = value;
    }
}

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

namespace NV.CT.PatientManagement.ApplicationService.Contract.Models;

public partial class SeriesModel
{
    private string id;
    private string internalstudyId;
    private string scanTaskId;
    private string reconTaskId;

    private DateTime? seriesDate;
    private DateTime? seriesTime;
    private int storeState;

    //private string parameters;
    private string seriesdescription;
    private string windowType;
    private string seriesInstanceUID;
    private int imageCount;
    private int seriesStatus;

    //private bool isChecked;

    //private int isArchive;
    private string reportPath;
    private int archiveStatus;
    private int printStatus;
    private int correctStatus;
    private int count;
    private string bodyPart;
    private string seriesType;
    private string imageType;
    private string seriesPath;
    private string patientPosition;
    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:False
    /// </summary>
    public string Id
    {
       get
        {
            return id;
        }
        set
        {
            id=value;
         
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string InternalStudyId
    {
        get
        {
            return internalstudyId;
        }
        set
        {
            internalstudyId = value;

        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string ScanTaskId
    {
        get
        {
            return scanTaskId;
        }
        set
        {
            scanTaskId = value;
     
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string ReconTaskId
    {
        get
        {
            return reconTaskId;
        }
        set
        {
            reconTaskId = value;

        }
    }

    public string ScanId { get; set; } = string.Empty;
    public string ReconId { get; set; } = string.Empty;

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public DateTime? SeriesDate
    {
        get
        {
            return seriesDate;
        }
        set
        {
            seriesDate= value;
 
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public DateTime? SeriesTime
    {
        get
        {
            return seriesTime;
        }
        set
        {
            seriesTime=value;
    
        }
    }

    /// <summary>
    /// Desc:
    /// Default:1
    /// Nullable:False
    /// </summary>           
    public int StoreState
    {
        get
        {
            return storeState;
        }
        set
        {
            storeState= value;
          
        }
    }       

    public string SeriesDescription
    {
        get
        {
            return seriesdescription;
        }
        set
        {
            seriesdescription= value ;

        }
    }
    public string WindowType
    {
        get
        {
            return windowType;
        }
        set
        {
            windowType = value;

        }
    }
    public string SeriesInstanceUID
    {
        get
        {
            return seriesInstanceUID;
        }
        set
        {
            seriesInstanceUID = value;

        }
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
        set
        {
            imageCount = value;
    
        }
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
        set
        {
            count = value;

        }
    }

    public int SeriesStatus
    {
        get
        {
            return seriesStatus;
        }
        set
        {
            seriesStatus = value;

        }
    }
   
    public string ReportPath
    {             
        get
        {
            return reportPath;
        }
        set
        {
            reportPath = value;

        }
    }

    public int ArchiveStatus
    {
        get
        {
            return archiveStatus;
        }
        set
        {
            archiveStatus = value;

        }
    }
    public int PrintStatus
    {
        get
        {
            return printStatus;
        }
        set
        {
            printStatus = value;

        }
    }
    
    public int CorrectStatus
    {
        get
        {
            return correctStatus;
        }
        set
        {
            correctStatus = value;

        }
    }

    public string BodyPart
    {
        get { return bodyPart; }
        set { bodyPart = value; }
    }
    public string SeriesType
    {
        set { seriesType = value; }
        get { return seriesType; }
    }
    public string ImageType
    {
        get { return imageType; }
        set { imageType = value; }
    }

    public string SeriesNumber
    {
        get;
        set;
    }

    public string SeriesPath
    {
        get { return seriesPath; }
        set { seriesPath = value; }
    }
    public string PatientPosition
    {
        get { return patientPosition; }
        set { patientPosition = value; }
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

}

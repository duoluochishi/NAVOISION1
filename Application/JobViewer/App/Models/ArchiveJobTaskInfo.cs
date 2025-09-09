namespace NV.CT.JobViewer.Models;

public class ArchiveJobTaskInfo: JobTaskBaseInfo
{
    private string _bodyPart = string.Empty;
    public string BodyPart
    {
        get
        {
            return this._bodyPart;
        }
        set
        {
            this.SetProperty(ref this._bodyPart, value);
        }
    }

    private string _archiveLevel = string.Empty;
    public string ArchiveLevel
    {
        get
        {
            return this._archiveLevel;
        }
        set
        {
            this.SetProperty(ref this._archiveLevel, value);
        }
    }

    private string _patientID = string.Empty;
    public string PatientID
    {
        get
        {
            return this._patientID;
        }
        set
        {
            this.SetProperty(ref this._patientID, value);
        }
    }

    private string _patientName;
    public string PatientName
    {
        get
        {
            return _patientName;
        }
        set
        {
            SetProperty(ref _patientName, value);
        }
    }

    private string _host;
    public string Host
    {
        get
        {
            return _host;
        }
        set
        {
            SetProperty(ref _host, value);
        }
    }

    private string _port;
    public string Port
    {
        get
        {
            return _port;
        }
        set
        {
            SetProperty(ref _port, value);
        }
    }

    private string _aeCaller;
    public string AECaller
    {
        get
        {
            return _aeCaller;
        }
        set
        {
            SetProperty(ref _aeCaller, value);
        }
    }

    private string aeTitle;
    public string AETitle
    {
        get
        {
            return aeTitle;
        }
        set
        {
            SetProperty(ref aeTitle, value);
        }
    }
}

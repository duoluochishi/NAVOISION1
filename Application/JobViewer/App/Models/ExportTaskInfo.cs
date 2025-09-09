namespace NV.CT.JobViewer.Models;

public class ExportTaskInfo: JobTaskBaseInfo
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

    private string _operationLevel = string.Empty;
    public string OperationLevel
    {
        get
        {
            return this._operationLevel;
        }
        set
        {
            this.SetProperty(ref this._operationLevel, value);
        }
    }

    private string _outputVirtualPath = string.Empty;
    public string OutputVirtualPath
    {
        get
        {
            return this._outputVirtualPath;
        }
        set
        {
            this.SetProperty(ref this._outputVirtualPath, value);
        }
    }

    private string _patientNames;
    public string PatientNames
    {
        get
        {
            return _patientNames;
        }
        set
        {
            SetProperty(ref _patientNames, value);
        }
    }
}

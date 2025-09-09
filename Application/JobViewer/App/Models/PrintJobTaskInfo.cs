namespace NV.CT.JobViewer.Models;

public class PrintJobTaskInfo : JobTaskBaseInfo
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

    private string _aePrinter = string.Empty;
    public string AEPrinter
    {
        get
        {
            return this._aePrinter;
        }
        set
        {
            this.SetProperty(ref this._aePrinter, value);
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

    private string _patientName = string.Empty;
    public string PatientName
    {
        get
        {
            return this._patientName;
        }
        set
        {
            this.SetProperty(ref this._patientName, value);
        }
    }

    private int _numbersOfCopies = 0;
    public int NumbersOfCopies
    {
        get
        {
            return this._numbersOfCopies;
        }
        set
        {
            this.SetProperty(ref this._numbersOfCopies, value);
        }
    }

    private string _pageSize = string.Empty;
    public string PageSize
    {
        get
        {
            return this._pageSize;
        }
        set
        {
            this.SetProperty(ref this._pageSize, value);
        }
    }
}

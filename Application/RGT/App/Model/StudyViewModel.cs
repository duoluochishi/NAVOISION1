namespace NV.CT.RGT.Model;

public class StudyViewModel : BaseViewModel
{
    private DateTime _studyDate;
    public DateTime StudyDate
    {
        get => _studyDate;
        set => SetProperty(ref _studyDate, value);
    }

    private DateTime _birthday;
    public DateTime Birthday
    {
        get => _birthday;
        set => SetProperty(ref _birthday, value);
    }

    private string _name = string.Empty;
    public string PatientName
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _studyId = string.Empty;
    public string StudyId
    {
        get => _studyId;
        set => SetProperty(ref _studyId, value);
    }

    private string _patientId = string.Empty;
    public string PatientId
    {
        get => _patientId;
        set => SetProperty(ref _patientId, value);
    }

    private string _sex = string.Empty;
    public string Sex
    {
        get => _sex;
        set => SetProperty(ref _sex, value);
    }

    private int _age;
    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    private string _bodyPart = string.Empty;
    public string BodyPart
    {
        get => _bodyPart;
        set => SetProperty(ref _bodyPart, value);
    }

    private DateTime _createTime;
    public DateTime CreateTime
    {
        get => _createTime;
        set => SetProperty(ref _createTime, value);
    }
}
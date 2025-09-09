//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.Model;

public class ScanCardModel : INotifyPropertyChanged
{
    private string patientName = string.Empty;
    public string PatientName
    {
        get => patientName;
        set
        {
            patientName = value;
            OnChangedProperty("PatientName");
        }
    }
    private string patientId = string.Empty;
    public string PatientID
    {
        get => patientId;
        set
        {
            patientName = value;
            OnChangedProperty("PatientID");
        }
    }
    private int patientAge = 0;
    public int PatientAge
    {
        get => patientAge;
        set
        {
            patientAge = value;
            OnChangedProperty("PatientAge");
        }
    }

    private string patientAgeType = string.Empty;
    public string PatientAgeType
    {
        get => patientAgeType;
        set
        {
            patientAgeType = value;
            OnChangedProperty("PatientAgeType");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnChangedProperty(string name)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
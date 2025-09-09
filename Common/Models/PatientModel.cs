using NV.CT.CTS.Enums;

namespace NV.CT.Models;

public class PatientModel: BaseInfo
{ 
    public string PatientName { get; set; } = string.Empty;

    public string PatientId { get; set; } = string.Empty;

    public Gender PatientSex { get; set; }
       
    public string BodyPart { get; set; } = string.Empty;
}
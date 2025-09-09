namespace NV.CT.CTS.Models;

public class StudyQueryModel
{
	public string PatientId { get; set; } = string.Empty;
	public string BodyPart { get; set; } = string.Empty;
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
}
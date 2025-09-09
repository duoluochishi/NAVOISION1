using NV.CT.CTS.Enums;

namespace NV.CT.Console.ApplicationService.Contract.Models;

public class ScanStudyModel
{
	public string InternalPatientId { get; set; } = string.Empty;
	public string PatientName { get; set; } = string.Empty;

	public string FirstName { get; set; } = string.Empty;

	public string LastName { get; set; } = string.Empty;

	public string PatientId { get; set; } = string.Empty;

	public Gender PatientSex { get; set; }

	public DateTime CreateTime { get; set; }

	public DateTime Birthday { get; set; }

	public string BodyPart { get; set; } = string.Empty;

	public string StudyId { get; set; } = string.Empty;
	public string StudyStatus { get; set; } = string.Empty;
}
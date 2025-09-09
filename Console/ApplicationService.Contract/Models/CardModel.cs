using NV.CT.CTS.Enums;

namespace NV.CT.Console.ApplicationService.Contract.Models;

/// <summary>
/// 动态卡片模型
/// </summary>
public class CardModel
{
	public int ProcessId { get; set; } = 0;

	public string PatientName { get; set; } = string.Empty;

	public string PatientId { get; set; } = string.Empty;

	public int Age { get; set; } = 0;

	public string AgeType { get; set; } = string.Empty;

	public int ItemStatus { get; set; } = 0;

	public string StatusBackground { get; set; } = string.Empty;

	public string IconGeometry { get; set; } = string.Empty;

	public string ItemName { get; set; } = string.Empty;

	public string CardParameters { get; set; } = string.Empty;


	public string CardKey => ItemName + CardParameters;


	public string ConfigName { get; set; } = string.Empty;

	public bool IsConfig { get; set; } = false;

	public bool IsExamination { get; set; } = false;

	public ProcessStartPart ProcessStartPart { get; set; }
}

namespace NV.CT.Alg.ScanReconValidation.Model;

public class RawDataValidatorOutput
{
	/// <summary>
	/// 全部验证结果
	/// </summary>
	public List<ValidationResult> Results { get; set; }

	/// <summary>
	/// 是否验证通过
	/// </summary>
	public bool IsSuccess => Results.All(n => n.IsSuccess);

	/// <summary>
	/// 验证失败的列表
	/// </summary>
	public List<ValidationResult> FailedList => Results.Where(n => !n.IsSuccess).ToList();

	public RawDataValidatorOutput(List<ValidationResult> list)
	{
		Results = list;
	}
}

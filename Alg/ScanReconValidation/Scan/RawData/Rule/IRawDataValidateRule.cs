using NV.CT.Alg.ScanReconValidation.Model;

namespace NV.CT.Alg.ScanReconValidation.Scan.RawData.Rule;

/// <summary>
/// 生数据验证规则
/// </summary>
public interface IRawDataValidateRule
{
	Task<RawDataValidatorOutput> StartValidate(RawDataValidatorInput input);
}

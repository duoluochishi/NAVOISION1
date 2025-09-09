using NV.CT.Alg.ScanReconValidation.Model;

namespace NV.CT.Alg.ScanReconValidation.Scan.RawData;

public interface IRawDataValidator
{
	IEnumerable<RawDataValidatorOutput> StartValidate(RawDataValidatorInput input);
}
namespace NV.CT.Alg.ScanReconValidation.Model;

public class RawDataValidatorInput
{
	//public List<ParameterModel> Parameters { get; set; } = new();

	public string RawDataPath { get; set; }

	public RawDataValidatorInput(string path)
	{
		RawDataPath = path;
	}

}

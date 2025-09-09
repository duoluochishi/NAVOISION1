using NV.CT.Alg.ScanReconValidation.Model;
using NV.CT.Alg.ScanReconValidation.Scan.RawData.Rule;

namespace NV.CT.Alg.ScanReconValidation.Scan.RawData;

public class RawDataValidator : IRawDataValidator
{
	private readonly List<IRawDataValidateRule> _executors = new();

	public IEnumerable<RawDataValidatorOutput> StartValidate(RawDataValidatorInput input)
	{
		List<RawDataValidatorOutput> results = new List<RawDataValidatorOutput>();
		try
		{
			var tasks = new List<Task<RawDataValidatorOutput>>();
			foreach (var executor in _executors)
			{
				tasks.Add(Task.Run(() => executor.StartValidate(input)));
			}

			Task.WaitAll(tasks.ToArray());

			foreach (var task in tasks)
			{
				results.Add(task.Result);
			}
		}
		catch (AggregateException ex)
		{
			throw new Exception("RawDataValidator validate with exception", ex);
		}

		return results;
	}

	public void AddValidationRule(IRawDataValidateRule executor)
	{
		_executors.Add(executor);
	}
}

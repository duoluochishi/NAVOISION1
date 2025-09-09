using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using NV.CT.Alg.ScanReconValidation.Model;

using RawDataHelperWrapper;

namespace NV.CT.Alg.ScanReconValidation.Scan.RawData.Rule;

/// <summary>
/// 生数据头文件解析分析
/// </summary>
public class RawDataHeaderInfoValidationRule : IRawDataValidateRule
{
	public Task<RawDataValidatorOutput> StartValidate(RawDataValidatorInput input)
	{
		try
		{
			var listResults = new List<ValidationResult>();

			//for debug only
			var rootPath = @"E:\1.2.840.1.59.0.8559.2405280913051000.141\2405280913124010\";
			rootPath = input.RawDataPath;

			var scanContent = File.ReadAllText(Path.Combine(rootPath, "ScanParameter.json"));
			var scanParameterObj = JsonConvert.DeserializeObject<ScanParameter>(scanContent);

			var scanOption = Enum.Parse<ScanOption>(scanParameterObj.ScanOption, true);
			var scanMode = Enum.Parse<ScanMode>(scanParameterObj.ScanMode, true);

			RawDataReaderWrapper wrapper = new RawDataReaderWrapper(rootPath, true);
			wrapper.ParseDataSource();
			var res = wrapper.GetRawDataList((count, total) => { });
			var index = 1;
			foreach (var item in res.Message.SequenceDataList)
			{
				//验证序列编号
				listResults.Add(Validate_FrameNo(item.ExpData));

				//验证球管曝光顺序
				listResults.Add(Validate_SourceId(item.ExpData, scanMode));

				//验证床位相同
				listResults.Add(Validate_TablePosition_IsSame(item.ExpData, scanParameterObj, index));

				//验证床位和实际差距不大
				listResults.Add(Validate_TablePosition_EachHead(item.ExpData, scanParameterObj, index));

				listResults.Add(Validate_Gantry(item.ExpData, scanParameterObj));

				//验证 角度的Δ值
				listResults.Add(Validate_Gantry_AngleChange(item.ExpData, scanParameterObj));

				index++;
			}

			var finalResult = listResults.Where(n => !n.IsSuccess).ToList();

			return Task.FromResult(new RawDataValidatorOutput(listResults));
		}
		catch (Exception ex)
		{
			CTS.Global.Logger.LogError($"RawDataHeaderInfoValidationRule validate with error {ex.Message}-{ex.StackTrace}");
			return Task.FromResult(new RawDataValidatorOutput(new List<ValidationResult>()));
		}
	}

	public ValidationResult Validate_FrameNo(List<RawDataHelperWrapper.RawData> list)
	{
		var validationResult = new ValidationResult(ValidationType.FrameNo);

		var frameNos = list.Select(n => n.FrameNoInSeries).OrderBy(n => n).ToList();
		for (int i = 1; i <= frameNos.Count; i++)
		{
			if (frameNos[i - 1] != i)
			{
				validationResult.IsSuccess = false;
				validationResult.Message = $"frame index no {i} is not sequenced";
				break;
			}
		}

		return validationResult;
	}

	public ValidationResult Validate_SourceId(List<RawDataHelperWrapper.RawData> list, ScanMode scanMode)
	{
		var validationResult = new ValidationResult(ValidationType.SourceID);
		var sourceNos = list.Select(n => n.SourceId).ToList();
		switch (scanMode)
		{
			case ScanMode.Plain:
				break;
			case ScanMode.JumpExposure:
				//判断每3个数是连续的
				var firstTubeList = new List<int>();
				for (int i = 0; i < sourceNos.Count; i += 3)
				{
					var first = sourceNos[i];
					//firstTubeList.Add(first);

					var second = sourceNos[i + 1];
					var third = sourceNos[i + 2];
					var delta1 = second - first;
					var delta2 = third - second;
					if (delta1 != delta2)
					{
						validationResult.IsSuccess = false;
						validationResult.Message = $"sourceid {i} is not sequenced";
						break;
					}
				}

				break;
			default:
				break;
		}

		return validationResult;
	}

	/// <summary>
	/// 验证床位置是否一致
	/// </summary>
	public ValidationResult Validate_TablePosition_IsSame(List<RawDataHelperWrapper.RawData> list, ScanParameter scanParameter, int index)
	{
		var validationResult = new ValidationResult(ValidationType.TablePosition);

		var scanOption = Enum.Parse<ScanOption>(scanParameter.ScanOption.ToUpper());
		var tablePositions = list.Select(n => n.TablePosition).ToList();

		switch (scanOption)
		{
			case ScanOption.AXIAL:
				if (!IsSame(tablePositions))
				{
					validationResult.IsSuccess = false;
					validationResult.Message = $"tableposition is not all the same";
				}
				break;
			case ScanOption.DUALSCOUT:

				break;
			default:
				break;
		}

		return validationResult;
	}

	/// <summary>
	/// 验证床的位置
	/// </summary>
	public ValidationResult Validate_TablePosition_EachHead(List<RawDataHelperWrapper.RawData> list, ScanParameter scanParameter, int index)
	{
		var validationResult = new ValidationResult(ValidationType.TablePosition);

		var scanOption = Enum.Parse<ScanOption>(scanParameter.ScanOption, true);
		var tablePositions = list.Select(n => n.TablePosition).ToList();

		var tableStartPos = scanParameter.TableStartPos;
		var tableEndPos = scanParameter.TableEndPos;

		var tolerance = 1;

		switch (scanOption)
		{
			case ScanOption.AXIAL:
				var expectedTablePosition = tableStartPos + (index - 1) * scanParameter.TableFeed;
				var realTablePosition = tablePositions.FirstOrDefault();
				if (Math.Abs(expectedTablePosition / 10.0 - realTablePosition) > tolerance)
				{
					validationResult.IsSuccess = false;
					validationResult.Message = $"tableposition is not correct expected position is {expectedTablePosition / 10.0} , real table position is {realTablePosition} , head file index is {index}";
				}
				break;
			case ScanOption.DUALSCOUT:

				break;
			default:
				break;
		}

		return validationResult;
	}

	/// <summary>
	/// 验证Gantry旋转角度
	/// </summary>
	public ValidationResult Validate_Gantry(List<RawDataHelperWrapper.RawData> list, ScanParameter scanParameter)
	{
		var validationResult = new ValidationResult(ValidationType.GantryAngle);
		var gantryAngles = list.Select(n => n.GantryAngle).ToList();

		var angleDelta = (gantryAngles.LastOrDefault() - gantryAngles.FirstOrDefault());
		var tolerance = 1;
		var gantryAngle = 15;
		if (Math.Abs(angleDelta) > (gantryAngle + tolerance))
		{
			validationResult.IsSuccess = false;
			validationResult.Message = $"Gantry angle delta {angleDelta} larger than 1, start angle {gantryAngles.FirstOrDefault()},end angle {gantryAngles.LastOrDefault()}";
		}

		return validationResult;
	}

	/// <summary>
	/// 验证 gantry角度变化平顺性
	/// </summary>
	public ValidationResult Validate_Gantry_AngleChange(List<RawDataHelperWrapper.RawData> list, ScanParameter scanParameter)
	{
		var validationResult = new ValidationResult(ValidationType.GantryAngle);
		var gantryAngles = list.Select(n => n.GantryAngle).ToList();

		var degreeChanges = new List<float>();
		for (int i = 0; i < gantryAngles.Count - 1; i++)
		{
			degreeChanges.Add(gantryAngles[i + 1] - gantryAngles[i]);
		}

		if (degreeChanges.Count <= 1)
			return validationResult;

		var avg = degreeChanges.Average();
		var sr = StandardDeviation(degreeChanges);
		//离群范围
		var ranges = (avg - 3 * sr, avg + 3 * sr);
		foreach (var item in degreeChanges)
		{
			if (item > ranges.Item2 || item < ranges.Item1)
			{
				validationResult.IsSuccess = false;
				validationResult.Message = $"Gantry angle change delta is not stable , exist discrete point with value is {item} ";
				break;
			}
		}

		return validationResult;
	}

	private bool IsSame<T>(List<T> list)
	{
		if (list.Count == 0)
			return true;

		var sample = list[0];
		return list.All(n => n.Equals(sample));
	}

	private float StandardDeviation(List<float> list)
	{
		var avg = list.Average();
		var sum = 0.0;
		foreach (var item in list)
		{
			var delta = (double)(item - avg);
			sum += Math.Pow(delta, 2);
		}

		return (float)Math.Sqrt(sum / list.Count);
	}
}

namespace NV.CT.SystemInterface.MCSRuntime.Contract.Disk;

public class DiskAnalysisResult : IComparable
{
	public DiskSpaceWarnLevel WarnLevel { get; set; }
	public double ConsumptionRatio { get; set; }
	public string DiskName { get; set; }

	public DiskAnalysisResult(string diskName, DiskSpaceWarnLevel warnLevel, double consumption)
	{
		DiskName = diskName;
		WarnLevel = warnLevel;
		ConsumptionRatio = consumption;
	}

	public int CompareTo(object? obj)
	{
		if (obj is DiskAnalysisResult target)
		{
			if (WarnLevel > target.WarnLevel)
			{
				return 1;
			}

			if (WarnLevel == target.WarnLevel)
			{
				//两者相等，比较consumption
				if (ConsumptionRatio >= target.ConsumptionRatio)
					return 1;

				return -1;
			}

			return -1;
		}

		throw new Exception("Not a DiskAnalysisResult object,can't compare!");
	}
}

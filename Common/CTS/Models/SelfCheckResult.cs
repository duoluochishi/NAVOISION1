using NV.CT.FacadeProxy.Common.Enums.SelfCheck;
using NV.CT.FacadeProxy.Common.Models.SelfCheck;

namespace NV.CT.CTS.Models;


public class SelfCheckResult
{
	public SelfCheckResult()
	{
		CheckName = "";
		DetailedSelfCheckInfos = new ();
	}
	public SelfCheckResult(SelfCheckPartType partName, SelfCheckStatus checkStatus, DateTime timestamp, List<DetailedSelfCheckInfo> detailedSelfCheckInfos)
	{
		CheckName = partName.ToString();
		CheckStatus = checkStatus;
		Timestamp = timestamp;
		DetailedSelfCheckInfos = detailedSelfCheckInfos;
	}
	public SelfCheckResult(string name, SelfCheckStatus checkStatus,  DateTime timestamp, List<DetailedSelfCheckInfo> detailedSelfCheckInfos)
	{
		CheckName = name;
		CheckStatus = checkStatus;
		Timestamp = timestamp;
		DetailedSelfCheckInfos = detailedSelfCheckInfos;
	}
	public string CheckName { get; set; }
	public SelfCheckStatus CheckStatus { get; set; }
	public string CheckStatusString => Enum.GetName(CheckStatus) ?? string.Empty;
	public DateTime Timestamp { get; set; }

	public string ErrorMessageTip
	{
		get
		{
			var notPassedItemTypes = DetailedSelfCheckInfos
				.Where(n => n.Status == SelfCheckStatus.Error || n.Status == SelfCheckStatus.Timeout)
				.Select(n => Enum.GetName<DetailedSelfCheckItemType>(n.ItemType) ).ToList();
			return string.Join(" , ", notPassedItemTypes);
		}
	}

	public List<DetailedSelfCheckInfo> DetailedSelfCheckInfos { get; set; }
}


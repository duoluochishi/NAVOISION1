using NV.CT.FacadeProxy.Common.EventArguments;

namespace NV.CT.CTS.Models;

public class SelfcheckHelper
{
	public static SelfCheckResult TransferTo(SelfCheckEventArgs selfCheckArg)
	{
		var selfCheckResult = new SelfCheckResult();
		var selfCheckInfo = selfCheckArg.SelfCheckInfo;
		selfCheckResult.CheckName = selfCheckInfo.PartType.ToString();
		selfCheckResult.CheckStatus = selfCheckInfo.Status;
		selfCheckResult.Timestamp = DateTime.Now;
		return selfCheckResult;
	}
}
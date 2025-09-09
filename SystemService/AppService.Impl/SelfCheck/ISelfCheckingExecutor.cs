//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;

namespace NV.CT.AppService.Impl.SelfCheck;

public interface ISelfCheckingExecutor
{
	event EventHandler<SelfCheckResult>? SelfCheckStatusChanged;

	BaseCommandResult StartSelfChecking();

	List<SelfCheckResult> GetSelfCheckResults();
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

//using NV.CT.AppService.Contract.SelfCheck;

using NV.CT.CTS.Models;

namespace NV.CT.AppService.Contract;

/// <summary>
/// GRPC SelfCheck service
/// </summary>
public interface ISelfCheckService
{
	void StartSelfChecking();

	event EventHandler<SelfCheckResult>? SelfCheckStatusChanged;

	List<SelfCheckResult> GetSelfCheckResults();
}
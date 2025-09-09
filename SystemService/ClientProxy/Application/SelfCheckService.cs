//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Newtonsoft.Json;

using NV.CT.AppService.Contract;
using NV.CT.CTS.Models;

namespace NV.CT.ClientProxy.Application;

public class SelfCheckService : ISelfCheckService
{
	private readonly MCSServiceClientProxy _clientProxy;

	public SelfCheckService(MCSServiceClientProxy clientProxy)
	{
		_clientProxy = clientProxy;
	}

	public event EventHandler<SelfCheckResult>? SelfCheckStatusChanged;

	public List<SelfCheckResult> GetSelfCheckResults()
	{
		var commandResult = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(ISelfCheckService).Namespace,
			SourceType = nameof(ISelfCheckService),
			ActionName = nameof(ISelfCheckService.GetSelfCheckResults),
			Data = string.Empty
		});
		var list = JsonConvert.DeserializeObject<List<SelfCheckResult>>(commandResult.Data);
		return list;
	}

	public void StartSelfChecking()
	{
		_clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(ISelfCheckService).Namespace,
			SourceType = nameof(ISelfCheckService),
			ActionName = nameof(ISelfCheckService.StartSelfChecking),
			Data = string.Empty
		});
	}
}

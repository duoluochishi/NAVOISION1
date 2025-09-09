//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/22 15:56:32    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;

using NV.CT.AppService.Contract;
using NV.CT.CTS.Models;

namespace NV.CT.ClientProxy.Application;

public class ShutdownService : IShutdownService
{
	private readonly MCSServiceClientProxy _clientProxy;

	public ShutdownService(MCSServiceClientProxy clientProxy)
	{
		_clientProxy = clientProxy;
	}

	public event EventHandler? ShutdownStatusChanged;

	public void GetCurrentShutdownStatus()
	{
		//todo：待实现
	}

	public List<BaseCommandResult> CanShutdown()
	{
		var commandResponse = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IShutdownService).Namespace,
			SourceType = nameof(IShutdownService),
			ActionName = nameof(IShutdownService.CanShutdown),
			Data = string.Empty
		});
		if (commandResponse.Success)
		{
			var res = JsonConvert.DeserializeObject<List<BaseCommandResult>>(commandResponse.Data);
			return res;
		}
		return new List<BaseCommandResult>();
	}

	public List<BaseCommandResult> CanShutdownSystem()
	{
		var commandResponse = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IShutdownService).Namespace,
			SourceType = nameof(IShutdownService),
			ActionName = nameof(IShutdownService.CanShutdownSystem),
			Data = string.Empty
		});
		if (commandResponse.Success)
		{
			var res = JsonConvert.DeserializeObject<List<BaseCommandResult>>(commandResponse.Data);
			return res;
		}
		return new List<BaseCommandResult>();
	}

	public void Restart()
	{
		//todo：待实现
		_clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IShutdownService).Namespace,
			SourceType = nameof(IShutdownService),
			ActionName = nameof(IShutdownService.Restart),
			Data = string.Empty
		});
	}

	public BaseCommandResult Shutdown()
	{
		var res = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IShutdownService).Namespace,
			SourceType = nameof(IShutdownService),
			ActionName = nameof(IShutdownService.Shutdown),
			Data = string.Empty
		});


		return JsonConvert.DeserializeObject<BaseCommandResult>(res.Data);
	}

	public BaseCommandResult ShutdownSystem()
	{
		var res = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IShutdownService).Namespace,
			SourceType = nameof(IShutdownService),
			ActionName = nameof(IShutdownService.ShutdownSystem),
			Data = string.Empty
		});

		return JsonConvert.DeserializeObject<BaseCommandResult>(res.Data);
	}
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Newtonsoft.Json;
using NV.MPS.Communication;
using NV.CT.DatabaseService.Contract;
using NV.CT.Models;

namespace NV.CT.ClientProxy.DataService;

public class PermissionClientProxy : IPermissionService
{
	private readonly MCSServiceClientProxy _clientProxy;
	public PermissionClientProxy(MCSServiceClientProxy clientProxy)
	{
		_clientProxy = clientProxy;
	}

	public List<PermissionModel> GetAll()
	{
		var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IPermissionService).Namespace,
			SourceType = nameof(IPermissionService),
			ActionName = nameof(IPermissionService.GetAll),
			Data = string.Empty
		});

		if (commandResponse.Success)
		{
			var res = JsonConvert.DeserializeObject<List<PermissionModel>>(commandResponse.Data);
			return res;
		}
		return new List<PermissionModel>();
	}

	public PermissionModel GetById(string id)
	{
		var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IPermissionService).Namespace,
			SourceType = nameof(IPermissionService), 
			ActionName = nameof(IPermissionService.GetById),
			Data = id
		});
		if (commandResponse.Success)
		{
			var res = JsonConvert.DeserializeObject<PermissionModel>(commandResponse.Data);
			return res;
		}
		return new PermissionModel() { IsDeleted = true };
	}

	public PermissionModel GetByCode(string code)
	{
		var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IPermissionService).Namespace,
			SourceType = nameof(IPermissionService),
			ActionName = nameof(IPermissionService.GetByCode),
			Data = code
		});
		if (commandResponse.Success)
		{
			var res = JsonConvert.DeserializeObject<PermissionModel>(commandResponse.Data);
			return res;
		}
		return new PermissionModel() { IsDeleted = true };
	}
}
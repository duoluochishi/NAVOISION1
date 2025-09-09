//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/8/30 14:31:08           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.MPS.Communication;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;

namespace NV.CT.ClientProxy.DataService;

public class DoseCheck : IDoseCheckService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public DoseCheck(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public bool Add(DoseCheckModel doseCheckModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IDoseCheckService).Namespace,
            SourceType = nameof(IDoseCheckService),
            ActionName = nameof(IDoseCheckService.Add),
            Data = doseCheckModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

	public bool AddList(List<DoseCheckModel> doseCheckModels)
	{
		var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IDoseCheckService).Namespace,
			SourceType = nameof(IDoseCheckService),
			ActionName = nameof(IDoseCheckService.AddList),
			Data = doseCheckModels.ToJson()
		});
		if (commandResponse.Success)
		{
			var res = Convert.ToBoolean(commandResponse.Data);
			return res;
		}
		return false;
	}


	public bool Update(DoseCheckModel doseCheckModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IDoseCheckService).Namespace,
            SourceType = nameof(IDoseCheckService),
            ActionName = nameof(IDoseCheckService.Update),
            Data = doseCheckModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

	public bool UpdateList(List<DoseCheckModel> doseCheckModels)
	{
		var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IDoseCheckService).Namespace,
			SourceType = nameof(IDoseCheckService),
			ActionName = nameof(IDoseCheckService.UpdateList),
			Data = doseCheckModels.ToJson()
		});
		if (commandResponse.Success)
		{
			var res = Convert.ToBoolean(commandResponse.Data);
			return res;
		}
		return false;
	}

	public bool Delete(DoseCheckModel doseCheckModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IDoseCheckService).Namespace,
            SourceType = nameof(IDoseCheckService),
            ActionName = nameof(IDoseCheckService.Delete),
            Data = doseCheckModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public DoseCheckModel Get(string doseCheckId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IDoseCheckService).Namespace,
            SourceType = nameof(IDoseCheckService),
            ActionName = nameof(IDoseCheckService.Get),
            Data = doseCheckId
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<DoseCheckModel>(commandResponse.Data);
            return res;
        }
        return null;
    }

    public List<DoseCheckModel> GetAll()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IDoseCheckService).Namespace,
            SourceType = nameof(IDoseCheckService),
            ActionName = nameof(IDoseCheckService.GetAll),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<DoseCheckModel>>(commandResponse.Data);
            return res;
        }
        return null;
    }
}
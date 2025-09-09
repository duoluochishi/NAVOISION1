//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using Newtonsoft.Json;
using NV.MPS.Communication;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.Models;
using NV.CT.Models.User;

namespace NV.CT.ClientProxy.DataService;

public class LoginHistoryService : ILoginHistoryService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public LoginHistoryService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public bool Insert(LoginHistoryModel model)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ILoginHistoryService).Namespace,
            SourceType = nameof(ILoginHistoryService),
            ActionName = nameof(ILoginHistoryService.Insert),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
	}

    public List<LoginHistoryModel> GetLoginHistoryListByAccount(string account)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ILoginHistoryService).Namespace,
            SourceType = nameof(ILoginHistoryService),
            ActionName = nameof(ILoginHistoryService.GetLoginHistoryListByAccount),
            Data = account
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<LoginHistoryModel>>(commandResponse.Data);
            return res;
        }
        return new List<LoginHistoryModel>();
	}

    public LoginHistoryModel GetLastLogin()
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ILoginHistoryService).Namespace,
            SourceType = nameof(ILoginHistoryService),
            ActionName = nameof(ILoginHistoryService.GetLastLogin),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<LoginHistoryModel?>(commandResponse.Data);
            return res;
        }

        return null;
	}
}
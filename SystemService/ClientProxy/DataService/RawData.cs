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
using NV.MPS.Communication;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.ClientProxy.DataService;

public class RawData : IRawDataService
{
    private readonly MCSServiceClientProxy _clientProxy;
    public event EventHandler<EventArgs<(RawDataModel, DataOperateType)>>? Refresh;

    public RawData(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public bool Add(RawDataModel rawDataModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRawDataService).Namespace,
            SourceType = nameof(IRawDataService),
            ActionName = nameof(IRawDataService.Add),
            Data = rawDataModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool Update(RawDataModel rawDataModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRawDataService).Namespace,
            SourceType = nameof(IRawDataService),
            ActionName = nameof(IRawDataService.Update),
            Data = rawDataModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool Delete(string Id)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRawDataService).Namespace,
            SourceType = nameof(IRawDataService),
            ActionName = nameof(IRawDataService.Delete),
            Data = Id,
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool UpdateExportStatusById(string id, bool isExported)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRawDataService).Namespace,
            SourceType = nameof(IRawDataService),
            ActionName = nameof(IRawDataService.UpdateExportStatusById),
            Data = Tuple.Create(id, isExported).ToJson(),
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public List<RawDataModel> GetRawDataListByStudyId(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IRawDataService).Namespace,
            SourceType = nameof(IRawDataService),
            ActionName = nameof(IRawDataService.GetRawDataListByStudyId),
            Data = studyId,
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<RawDataModel>>();
            return res;
        }

        return default;

    }
}

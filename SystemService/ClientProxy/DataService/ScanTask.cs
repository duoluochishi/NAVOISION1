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
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.ClientProxy.DataService;

public class ScanTask : IScanTaskService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public ScanTask(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }


    public bool Insert(ScanTaskModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScanTaskService).Namespace,
            SourceType = nameof(IScanTaskService),
            ActionName = nameof(IScanTaskService.Insert),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool InsertMany(List<ScanTaskModel> list)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScanTaskService).Namespace,
            SourceType = nameof(IScanTaskService),
            ActionName = nameof(IScanTaskService.InsertMany),
            Data = list.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool Update(ScanTaskModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScanTaskService).Namespace,
            SourceType = nameof(IScanTaskService),
            ActionName = nameof(IScanTaskService.Update),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool UpdateStatus(ScanTaskModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScanTaskService).Namespace,
            SourceType = nameof(IScanTaskService),
            ActionName = nameof(IScanTaskService.UpdateStatus),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool Delete(ScanTaskModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScanTaskService).Namespace,
            SourceType = nameof(IScanTaskService),
            ActionName = nameof(IScanTaskService.Delete),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public ScanTaskModel Get(string ID)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScanTaskService).Namespace,
            SourceType = nameof(IScanTaskService),
            ActionName = nameof(IScanTaskService.Get),
            Data = ID
        });
        if (commandResponse.Success)
        {
            var res = (commandResponse.Data.DeserializeTo<ScanTaskModel>());
            return res;
        }

        return default;
    }

    public ScanTaskModel Get2(string studyID, string measurementId, string frameOfReferenceUid, string scanRangeID)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScanTaskService).Namespace,
            SourceType = nameof(IScanTaskService),
            ActionName = nameof(IScanTaskService.Get2),
            Data = Tuple.Create(studyID, measurementId, frameOfReferenceUid, scanRangeID).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = (commandResponse.Data.DeserializeTo<ScanTaskModel>());
            return res;
        }

        return default;
    }

    public ScanTaskModel Get3(string studyID, string scanRangeID)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScanTaskService).Namespace,
            SourceType = nameof(IScanTaskService),
            ActionName = nameof(IScanTaskService.Get3),
            Data = Tuple.Create(studyID, scanRangeID).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = (commandResponse.Data.DeserializeTo<ScanTaskModel>());
            return res;
        }

        return default;
    }

    public List<ScanTaskModel> GetAll(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IScanTaskService).Namespace,
            SourceType = nameof(IScanTaskService),
            ActionName = nameof(IScanTaskService.GetAll),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = (commandResponse.Data.DeserializeTo<List<ScanTaskModel>>());
            return res;
        }

        return default;
    }
}

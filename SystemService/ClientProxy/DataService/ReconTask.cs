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
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.ClientProxy.DataService;

public class ReconTask : IReconTaskService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public ReconTask(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public bool Insert(ReconTaskModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.Insert),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool InsertMany(List<ReconTaskModel> list)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.InsertMany),
            Data = list.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool Update(ReconTaskModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.Update),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool UpdateStatus(ReconTaskModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.UpdateStatus),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool UpdateTaskStatus(string studyId, string reconId, OfflineTaskStatus taskStatus, DateTime startTime, DateTime endTime)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.UpdateTaskStatus),
            Data = Tuple.Create(studyId, reconId, taskStatus, startTime, endTime).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool UpdateReconTaskStatus((string ScanId, string ReconId) conditionFields, (OfflineTaskStatus TaskStatus, DateTime StartTime, DateTime EndTime) updateFields)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.UpdateReconTaskStatus),
            Data = Tuple.Create(conditionFields, updateFields).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool DeleteReconAndSeries(string studyId, string scanId, string reconId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.DeleteReconAndSeries),
            Data = Tuple.Create(studyId, scanId, reconId).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool Delete(ReconTaskModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.Delete),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool DeleteByGuid(string reconGuid)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.DeleteByGuid),
            Data = reconGuid
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public ReconTaskModel Get(string ID)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.Get),
            Data = ID
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<ReconTaskModel>();
            return res;
        }

        return default;
    }

    public ReconTaskModel Get2(string studyId, string scanId, string reconId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.Get2),
            Data = Tuple.Create(studyId, scanId, reconId).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<ReconTaskModel>();
            return res;
        }

        return default;
    }

    public List<ReconTaskEntity> GetOfflineList()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.GetOfflineList),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<ReconTaskEntity>>();
            return res;
        }

        return default;
    }

    public List<ReconTaskModel> GetAll(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.GetAll),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<ReconTaskModel>>();
            return res;
        }

        return default;
    }

    public int GetLatestSeriesNumber(string studyId, int originalSeriesNumber)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconTaskService).Namespace,
            SourceType = nameof(IReconTaskService),
            ActionName = nameof(IReconTaskService.GetLatestSeriesNumber),
            Data = (studyId, originalSeriesNumber).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToInt32(commandResponse.Data);
            return res;
        }

        return default;
    }
}

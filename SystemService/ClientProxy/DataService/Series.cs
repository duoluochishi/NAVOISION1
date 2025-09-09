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
using NV.CT.FacadeProxy.Common.Models;

namespace NV.CT.ClientProxy.DataService;

public class Series : ISeriesService
{
    private readonly MCSServiceClientProxy _clientProxy;

#pragma warning disable 67
    public event EventHandler<EventArgs<(SeriesModel, DataOperateType)>>? Refresh;
#pragma warning restore 67
    public Series(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public bool Add(SeriesModel seriesModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.Add),
            Data = seriesModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool Delete(SeriesModel seriesModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.Delete),
            Data = seriesModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public List<SeriesModel> GetSeriesByStudyId(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetSeriesByStudyId),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<SeriesModel>>();
            return res;
        }

        return default;
    }
    public List<SeriesModel> GetTopoTomoSeriesByStudyId(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetTopoTomoSeriesByStudyId),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<SeriesModel>>();
            return res;
        }

        return default;
    }

    public SeriesModel[] GetSeriesBySeriesIds(string[] seriesIds)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetSeriesBySeriesIds),
            Data = seriesIds.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<SeriesModel[]>();
            return res;
        }

        return new SeriesModel[] { };
    }

    public SeriesModel GetSeriesByReconId(string reconId)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetSeriesByReconId),
            Data = reconId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<SeriesModel>();
            return res;
        }

        return new SeriesModel();
	}
    public SeriesModel GetSeriesBySeriesInstanceUID(string seriesInstanceUID)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetSeriesBySeriesInstanceUID),
            Data = seriesInstanceUID
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<SeriesModel>();
            return res;
        }

        return new SeriesModel();
    }
    public SeriesModel GetScreenshotSeriesByImageType(string studyID, string imageType)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetScreenshotSeriesByImageType),
            Data = Tuple.Create(studyID, imageType).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<SeriesModel>();
            return res;
        }

        return new SeriesModel();
    }
    public string GetSeriesReportPathByStudyId(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetSeriesReportPathByStudyId),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data;
            return res;
        }
        return default;
    }
    public string GetSeriesIdByStudyId(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetSeriesIdByStudyId),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data;
            return res;
        }
        return default;
    }
    public bool UpdateScreenshotSeriesByImageType(SeriesModel seriesModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.UpdateScreenshotSeriesByImageType),
            Data = seriesModel.ToJson(),
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return default;
    }
    public bool UpdateArchiveStatus(List<SeriesModel> seriesModels)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.UpdateArchiveStatus),
            Data = seriesModels.ToJson(),
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return default;
    }

    public bool UpdatePrintStatus(List<SeriesModel> seriesModels)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.UpdatePrintStatus),
            Data = seriesModels.ToJson(),
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return default;
    }

    public List<string> GetSeriesIdsByStudyId(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetSeriesIdsByStudyId),
            Data = studyId,
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<string>>();
            return res;
        }
        return default;
    }

    public SeriesModel GetSeriesById(string Id)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetSeriesById),
            Data = Id,
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<SeriesModel>();
            return res;
        }
        return default;
    }
    public int GetSeriesCountByStudyId(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetSeriesCountByStudyId),
            Data = studyId,
        });
        if (commandResponse.Success)
        {
            var res = int.Parse(commandResponse.Data);
            return res;
        }
        return -1;
    }
    public int GetArchiveStatusHasCompletedSeriesCountByStudyId(string studyId, string seriesId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetArchiveStatusHasCompletedSeriesCountByStudyId),
            Data = Tuple.Create(studyId, seriesId).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = int.Parse(commandResponse.Data);
            return res;
        }
        return -1;
    }

    public int GetPrintStatusHasCompletedSeriesCountByStudyId(string studyId, string seriesId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetPrintStatusHasCompletedSeriesCountByStudyId),
            Data = Tuple.Create(studyId, seriesId).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = int.Parse(commandResponse.Data);
            return res;
        }
        return -1;
    }


    public int GetArchiveingSeriesCountByStudyId(string studyId, string seriesId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetArchiveingSeriesCountByStudyId),
            Data = Tuple.Create(studyId, seriesId).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = int.Parse(commandResponse.Data);
            return res;
        }
        return -1;
    }

    public int GetPrintingSeriesCountByStudyId(string studyId, string seriesId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.GetPrintingSeriesCountByStudyId),
            Data = Tuple.Create(studyId, seriesId).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = int.Parse(commandResponse.Data);
            return res;
        }
        return -1;
    }


    public bool SetSeriesArchiveFail()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(ISeriesService).Namespace,
            SourceType = nameof(ISeriesService),
            ActionName = nameof(ISeriesService.SetSeriesArchiveFail),
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }
}

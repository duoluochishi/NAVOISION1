using Newtonsoft.Json;
using NV.CT.CTS;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Protocol.Models;
using NV.CT.SyncService.Contract;
using NV.MPS.Communication;

namespace NV.CT.ClientProxy.SyncService;

public class DataSyncService : IDataSync
{
	public event EventHandler<EventArgs<TablePositionInfo>>? TablePositionChanged;
	public event EventHandler<EventArgs<RgtScanModel>>? SelectionScanChanged;
	public event EventHandler<string>? ReplaceProtocolStarted;
	public event EventHandler? ReplaceProtocolFinished;
	public event EventHandler? ExamCloseFinished;
	public event EventHandler<string>? SelectStudyChanged;
	public event EventHandler? NormalExamStarted;
	public event EventHandler? NormalExamFinished;
	public event EventHandler<EventArgs<RealtimeInfo>>? RealtimeStatusChanged;
	public event EventHandler? EmergencyExamStarted;
	public event EventHandler? EmergencyExamFinished;
	public event EventHandler<string>? SelectHumanBodyStarted;
	public event EventHandler<SyncProtocolResponse>? SelectHumanBodyFinished;
	public event EventHandler<string>? SelectProtocolStarted;
	public event EventHandler<string>? SelectProtocolFinished;
	public event EventHandler<EventArgs<(ProtocolModel protocolModel, string currentReconID)>>? SeriesDataChanged;

	private readonly SyncServiceClientProxy _syncServiceClientProxy;

	public DataSyncService(SyncServiceClientProxy syncServiceClientProxy)
	{
		_syncServiceClientProxy = syncServiceClientProxy;
	}
	
	public void NotifyExamClose()
	{
		_syncServiceClientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.NotifyExamClose),
			Data = string.Empty
		});
	}

	public List<(PatientModel, StudyModel)> GetPatientStudyList()
	{
		var commandResponse = _syncServiceClientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.GetPatientStudyList),
			Data = string.Empty
		});
		if (commandResponse.Success)
		{
			var res = JsonConvert.DeserializeObject<List<(PatientModel, StudyModel)>>(commandResponse.Data);
			return res;
		}

		return new List<(PatientModel, StudyModel)>();
	}

	//public bool CheckExist()
	//{
	//	var commandResponse = _syncServiceClientProxy.ExecuteCommand(new CommandRequest()
	//	{
	//		Namespace = typeof(IDataSync).Namespace,
	//		SourceType = nameof(IDataSync),
	//		ActionName = nameof(IDataSync.CheckExist),
	//		Data = string.Empty
	//	});
	//	if (commandResponse.Success)
	//	{
	//		var res = Convert.ToBoolean(commandResponse.Data);
	//		return res;
	//	}
	//	return false;
	//}

	//public void StartWorkflow(string studyId)
	//{
	//    _syncServiceClientProxy.ExecuteCommand(new CommandRequest()
	//    {
	//        Namespace = typeof(IDataSync).Namespace,
	//        SourceType = nameof(IDataSync),
	//        ActionName = nameof(IDataSync.StartWorkflow),
	//        Data = studyId
	//    });
	//}

	//public bool Start(ApplicationRequest applicationRequest)
	//{
	//    bool res = false;
	//    var commandResponse = _syncServiceClientProxy.ExecuteCommand(new CommandRequest()
	//    {
	//        Namespace = typeof(IDataSync).Namespace,
	//        SourceType = nameof(IDataSync),
	//        ActionName = nameof(IDataSync.Start),
	//        Data = applicationRequest.ToJson()
	//    });
	//    if (commandResponse.Success)
	//    {
	//        res = Convert.ToBoolean(commandResponse.Data);
	//    }
	//    return res;
	//}

	//public ProtocolTemplateModel GetProtocolTemplate(string templateId)
	//{
	//	var response = _syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
	//	{
	//		Namespace = typeof(IDataSync).Namespace,
	//		SourceType = nameof(IDataSync),
	//		ActionName = nameof(IDataSync.GetProtocolTemplate),
	//		Data = templateId
	//	});
	//	if (response.Success)
	//	{
	//		return JsonConvert.DeserializeObject<ProtocolTemplateModel>(response.Data);
	//	}
	//	return default;
	//}

	public void ReplaceProtocol(string templateId)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.ReplaceProtocol),
			Data = templateId
		});
	}

	//public void ConfirmExam()
	//{
	//    _syncServiceClientProxy.ExecuteCommand(new Communication.CommandRequest
	//    {
	//        Namespace = typeof(IDataSync).Namespace,
	//        SourceType = nameof(IDataSync),
	//        ActionName = nameof(IDataSync.ConfirmExam),
	//        Data = string.Empty
	//    });
	//}

	public void RefreshWorkList(List<(PatientModel, StudyModel)> data)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.RefreshWorkList),
			Data = data.ToJson()
		});
	}

	public event EventHandler<List<(PatientModel, StudyModel)>>? WorkListChanged;

	public void SelectStudy(string studyId)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.SelectStudy),
			Data = studyId
		});
	}

	public void NormalExam()
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.NormalExam),
			Data = string.Empty
		});
	}
	public void NotifyNormalExam()
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.NotifyNormalExam),
			Data = string.Empty
		});
	}

	public (StudyModel, PatientModel) GetCurrentStudyInfo()
	{
		var response = _syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.GetCurrentStudyInfo),
			Data = string.Empty
		});

		if (response.Success)
		{
			return JsonConvert.DeserializeObject<(StudyModel, PatientModel)>(response.Data);
		}
		return default;
	}

	public void SelectHumanBody(string bodyPartName)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.SelectHumanBody),
			Data = bodyPartName
		});
	}


	public void NotifySelectHumanBody(SyncProtocolResponse res)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.NotifySelectHumanBody),
			Data = res.ToJson()
		});
	}


	public void SelectProtocol(string protocolTemplateId)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.SelectProtocol),
			Data = protocolTemplateId
		});
	}


	public void NotifySelectProtocol(string protocolTemplateId)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.NotifySelectProtocol),
			Data = protocolTemplateId
		});
	}


	public void NotifyReplaceProtocol()
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.NotifyReplaceProtocol),
			Data = string.Empty
		});
	}

	public void SelectionScanChange(RgtScanModel scanModel)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.SelectionScanChange),
			Data = scanModel.ToJson()
		});
	}

	public void NotifyRealtimeStatus(RealtimeInfo realtimeInfo)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.NotifyRealtimeStatus),
			Data = realtimeInfo.ToJson()
		});
	}

	public void EmergencyExam()
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.EmergencyExam),
			Data = string.Empty
		});
	}

	public void NotifyEmergencyExam()
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.NotifyEmergencyExam),
			Data = string.Empty
		});
	}

	//public void StartEmergencyExamDirectly()
	//{
	//	_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
	//	{
	//		Namespace = typeof(IDataSync).Namespace,
	//		SourceType = nameof(IDataSync),
	//		ActionName = nameof(IDataSync.StartEmergencyExamDirectly),
	//		Data = string.Empty
	//	});
	//}

	public void NotifySeriesData(ProtocolModel protocolModel, string currentReconId)
	{
		_syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
		{
			Namespace = typeof(IDataSync).Namespace,
			SourceType = nameof(IDataSync),
			ActionName = nameof(IDataSync.NotifySeriesData),
			Data = Tuple.Create(protocolModel, currentReconId).ToJson()
		});
	}

	public TablePositionInfo? CurrentTablePosition()
	{
		try
		{
			var response = _syncServiceClientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
			{
				Namespace = typeof(IDataSync).Namespace,
				SourceType = nameof(IDataSync),
				ActionName = nameof(IDataSync.CurrentTablePosition),
				Data = string.Empty
			});
			if (response.Success)
			{
				return JsonConvert.DeserializeObject<TablePositionInfo>(response.Data);
			}

			return default;
		}
		catch (Exception)
		{
			return null;
		}
	}
}
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Protocol.Models;
using NV.CT.SyncService.Contract;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.SyncService;

public class DataSyncService : IDataSync
{
	private readonly IStudyService _studyService;
	private readonly IWorkflow _workflow;
	private readonly ITablePositionService _tablePositionService;

	public DataSyncService(IStudyService studyService, IWorkflow workflow, ITablePositionService tablePositionService)
	{
		_workflow = workflow;
		_studyService = studyService;

		_tablePositionService = tablePositionService;
		_tablePositionService.TablePositionChanged += TablePositionService_TablePositionChanged;
	}

	#region patietn browser
	public void RefreshWorkList(List<(PatientModel, StudyModel)> data)
	{
		WorkListChanged?.Invoke(this, data);
	}

	public event EventHandler<List<(PatientModel, StudyModel)>>? WorkListChanged;

	public List<(PatientModel, StudyModel)> GetPatientStudyList()
	{
		var ret = _studyService.GetPatientStudyListWithNotStarted();
		return ret;

		//var list= new List<(PatientModel, StudyModel)>();
		//list.Add(new ValueTuple<PatientModel, StudyModel>(new PatientModel()
		//{
		//	BodyPart = "abdomen",
		//	Id = "123333",
		//	PatientName = "test user",
		//	PatientSex = Gender.Male
		//},new StudyModel()
		//{
		//	StudyId = "33334444",

		//}));
		//return list;
	}

	/// <summary>
	/// 病人选中
	/// </summary>
	public void SelectStudy(string studyId)
	{
		SelectStudyChanged?.Invoke(this, studyId);
	}

	public event EventHandler<string>? SelectStudyChanged;

	#endregion

	#region 常规检查
	public void NormalExam()
	{
		NormalExamStarted?.Invoke(this, EventArgs.Empty);
	}
	public event EventHandler? NormalExamStarted;

	public void NotifyNormalExam()
	{
		NormalExamFinished?.Invoke(this, EventArgs.Empty);
	}
	public event EventHandler? NormalExamFinished;

	#endregion

	#region 急诊
	public void EmergencyExam()
	{
		EmergencyExamStarted?.Invoke(this, EventArgs.Empty);
	}

	public event EventHandler? EmergencyExamStarted;

	public void NotifyEmergencyExam()
	{
		EmergencyExamFinished?.Invoke(this, EventArgs.Empty);
	}

	public event EventHandler? EmergencyExamFinished;

	#endregion

	#region 获取检查信息
	public (StudyModel?, PatientModel?) GetCurrentStudyInfo()
	{
		return _workflow.GetCurrentStudyInfo();
	}
	#endregion

	#region 部位选择
	/// <summary>
	/// RGT选中人体部位
	/// </summary>
	public void SelectHumanBody(string bodyPartName)
	{
		SelectHumanBodyStarted?.Invoke(this, bodyPartName);
	}

	public event EventHandler<string>? SelectHumanBodyStarted;

	/// <summary>
	/// MCS通知RGT客户端当前选中人体部位
	/// </summary>
	public void NotifySelectHumanBody(SyncProtocolResponse syncObj)
	{
		SelectHumanBodyFinished?.Invoke(this, syncObj);
	}
	public event EventHandler<SyncProtocolResponse>? SelectHumanBodyFinished;

	#endregion

	#region 协议选择
	/// <summary>
	/// RGT选中某个协议
	/// </summary>
	public void SelectProtocol(string protocolTemplateId)
	{
		SelectProtocolStarted?.Invoke(this, protocolTemplateId);
	}
	public event EventHandler<string>? SelectProtocolStarted;
	/// <summary>
	/// MCS通知RGT客户端当前选中协议是什么
	/// </summary>
	public void NotifySelectProtocol(string protocolTemplateId)
	{
		SelectProtocolFinished?.Invoke(this, protocolTemplateId);
	}

	public event EventHandler<string>? SelectProtocolFinished;
	#endregion

	#region 替换协议
	/// <summary>
	/// 替换协议
	/// </summary>
	public void ReplaceProtocol(string templateId)
	{
		ReplaceProtocolStarted?.Invoke(this, templateId);
	}
	public event EventHandler<string>? ReplaceProtocolStarted;
	/// <summary>
	/// MCS通知RGT协议替换完成
	/// </summary>
	public void NotifyReplaceProtocol()
	{
		ReplaceProtocolFinished?.Invoke(this, EventArgs.Empty);
	}

	public event EventHandler? ReplaceProtocolFinished;
	#endregion

	#region 扫描变化
	/// <summary>
	/// MCS通知RGT选中检查变化
	/// </summary>
	public void SelectionScanChange(RgtScanModel model)
	{
		SelectionScanChanged?.Invoke(this, new EventArgs<RgtScanModel>(model));
	}

	public event EventHandler<EventArgs<RgtScanModel>>? SelectionScanChanged;

	#endregion

	#region 检查结束

	public void NotifyExamClose()
	{
		ExamCloseFinished?.Invoke(this, EventArgs.Empty);
	}
	public event EventHandler? ExamCloseFinished;
	#endregion

	#region 床位置信息
	public TablePositionInfo CurrentTablePosition()
	{
		return _tablePositionService.CurrentTablePosition;
	}

	public event EventHandler<EventArgs<TablePositionInfo>>? TablePositionChanged;

	private void TablePositionService_TablePositionChanged(object? sender, EventArgs<TablePositionInfo> e)
	{
		TablePositionChanged?.Invoke(this, e);
	}
	#endregion

	#region 实时状态变化

	public void NotifyRealtimeStatus(RealtimeInfo realtimeInfo)
	{
		RealtimeStatusChanged?.Invoke(this, new EventArgs<RealtimeInfo>(realtimeInfo));
	}

	public event EventHandler<EventArgs<RealtimeInfo>>? RealtimeStatusChanged;

	#endregion

	#region 同步序列数据
	public void NotifySeriesData(ProtocolModel protocolModel, string currentReconId)
	{
		SeriesDataChanged?.Invoke(this, new EventArgs<(ProtocolModel protocolModel, string currentReconID)>((protocolModel, currentReconId)));
	}
	public event EventHandler<EventArgs<(ProtocolModel protocolModel, string currentReconID)>>? SeriesDataChanged;
	#endregion
}
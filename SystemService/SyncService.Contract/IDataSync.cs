using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Protocol.Models;

namespace NV.CT.SyncService.Contract;

/// <summary>
/// 设计原则:
///     MCS端负责发送事件过去给所有RGT客户端,客户端收到消息做自己的业务逻辑
///		MCS端不允许调用本接口里面定义的方法
///		MCS端接收到客户端发送过来的事件后,代理客户端来模拟调用本地逻辑代码,保证业务触发后,通过统一事件通知到所有客户端
///
///		RGT客户端订阅事件通知,业务逻辑对应的功能会通过方法调用发送到MCS那边去间接调用
/// </summary>
public interface IDataSync
{

	#region patient browser

	/// <summary>
	/// MCS触发数据通过事件发出去
	/// </summary>
	void RefreshWorkList(List<(PatientModel, StudyModel)> data);
	event EventHandler<List<(PatientModel, StudyModel)>>? WorkListChanged;

	/// <summary>
	/// RGT主动获取work list列表数据
	/// </summary>
	List<(PatientModel, StudyModel)> GetPatientStudyList();

	/// <summary>
	/// 选中某个病人
	/// </summary>
	void SelectStudy(string studyId);

	event EventHandler<string>? SelectStudyChanged;

	#endregion

	#region 常规检查
	/// <summary>
	/// RGT客户端开启检查调用
	/// </summary>
	void NormalExam();

	/// <summary>
	/// RGT通知到MCS开始检查事件发出
	/// </summary>
	public event EventHandler? NormalExamStarted;

	/// <summary>
	/// 通知RGT端 开始检查完成
	/// </summary>
	void NotifyNormalExam();

	/// <summary>
	/// MCS通知RGT客户端开始检查完成
	/// </summary>
	public event EventHandler? NormalExamFinished;

	#endregion

	#region 急诊
	/// <summary>
	/// RGT开始急诊
	/// </summary>
	void EmergencyExam();
	event EventHandler? EmergencyExamStarted;

	/// <summary>
	/// MCS通知RGT开始急诊完成
	/// </summary>
	void NotifyEmergencyExam();

	event EventHandler? EmergencyExamFinished;

	#endregion

	#region 获取检查信息
	/// <summary>
	/// 获取当前检查信息
	/// </summary>
	(StudyModel?, PatientModel?) GetCurrentStudyInfo();
	#endregion

	#region 部位选择
	/// <summary>
	/// RGT选中人体部位
	/// </summary>
	public void SelectHumanBody(string bodyPartName);
	public event EventHandler<string>? SelectHumanBodyStarted;
	/// <summary>
	/// MCS通知RGT客户端当前选中人体部位
	/// </summary>
	public void NotifySelectHumanBody(SyncProtocolResponse res);
	public event EventHandler<SyncProtocolResponse>? SelectHumanBodyFinished;
	#endregion

	#region 协议选择
	/// <summary>
	/// RGT选中某个协议
	/// </summary>
	void SelectProtocol(string protocolTemplateId);
	event EventHandler<string>? SelectProtocolStarted;

	/// <summary>
	/// MCS通知RGT客户端当前选中协议是什么
	/// </summary>
	void NotifySelectProtocol(string protocolTemplateId);
	event EventHandler<string>? SelectProtocolFinished;
	#endregion

	#region 替换协议
	/// <summary>
	/// 替换协议
	/// </summary>
	void ReplaceProtocol(string templateId);
	event EventHandler<string>? ReplaceProtocolStarted;

	/// <summary>
	/// MCS通知RGT协议替换完成
	/// </summary>
	void NotifyReplaceProtocol();
	event EventHandler? ReplaceProtocolFinished;
	#endregion

	#region 扫描变化	
	/// <summary>
	/// MCS通知RGT选中检查变化
	/// </summary>
	void SelectionScanChange(RgtScanModel scanModel);

	event EventHandler<EventArgs<RgtScanModel>>? SelectionScanChanged;

	#endregion

	#region 检查结束
	void NotifyExamClose();
	event EventHandler? ExamCloseFinished;
	#endregion

	#region 床位置相关

	/// <summary>
	/// 主动获取床位置信息
	/// </summary>
	TablePositionInfo? CurrentTablePosition();

	/// <summary>
	/// 床位变化事件
	/// </summary>
	event EventHandler<EventArgs<TablePositionInfo>>? TablePositionChanged;

	#endregion

	#region 实时状态变化
	void NotifyRealtimeStatus(RealtimeInfo realtimeInfo);
	event EventHandler<EventArgs<RealtimeInfo>>? RealtimeStatusChanged;

	#endregion

	#region 同步序列数据

	void NotifySeriesData(ProtocolModel protocolModel, string currentReconId);
	event EventHandler<EventArgs<(ProtocolModel protocolModel, string currentReconID)>>? SeriesDataChanged;

	#endregion

}
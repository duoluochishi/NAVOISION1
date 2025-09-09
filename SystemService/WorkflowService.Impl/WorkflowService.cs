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

using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Protocol;
using NV.CT.Protocol.Models;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.WorkflowService.Impl;

public class WorkflowService : IWorkflow
{
	private bool _isEmergency;
	private readonly ILogger<WorkflowService> _logger;
	private readonly ExamService _examService;
	private readonly IStudyService _studyService;
	private readonly ReportService _reportService;

	public event EventHandler<string>? StudyChanged;
	public event EventHandler<string>? WorkflowStatusChanged;
	public event EventHandler<string>? LockStatusChanged;

	public WorkflowService(ILogger<WorkflowService> logger, ExamService examService, ReportService reportService, IStudyService studyService)
	{
		_logger = logger;
		_studyService = studyService;
		_examService = examService;
		_examService.StudyChanged += ExamService_StudyChanged;
		_examService.WorkflowStatusChanged += ExamService_WorkflowStatusChanged;
		_examService.LockStatusChanged += ExamService_LockStatusChanged;
		_reportService = reportService;
	}

	private void ExamService_StudyChanged(object? sender, string e)
	{
		StudyChanged?.Invoke(this, e);
	}

	private void ExamService_WorkflowStatusChanged(object? sender, CTS.Enums.WorkflowStatus e)
	{
		WorkflowStatusChanged?.Invoke(this, e.ToString());
	}

	private void ExamService_LockStatusChanged(object? sender, bool e)
	{
		LockStatusChanged?.Invoke(this, e.ToString());
	}

	public bool CheckExist()
	{
		_logger.LogDebug($"Check examination exist: {!string.IsNullOrEmpty(_examService.StudyId)}");
		return !string.IsNullOrEmpty(_examService.StudyId);
	}

	public void CloseWorkflow()
	{
		_logger.LogInformation($"CloseWorkflow current study id is {_examService.StudyId}");
		var tempStudyId = _examService.StudyId;
		if (!string.IsNullOrEmpty(_examService.StudyId))
		{
			_logger.LogDebug($"Close examination: {_examService.StudyId}");
			Task.Run(() =>
			{
				_reportService.CreateReport(tempStudyId);
			});
			_examService.StudyId = string.Empty;
			_examService.WorkflowStatus = CTS.Enums.WorkflowStatus.ExaminationClosing;
			//TODO: 引发关闭事件，且切换状态
		}

		ExamClosed?.Invoke(this, tempStudyId);
	}

	public void StartWorkflow(string studyId)
	{
		_logger.LogDebug($"Start examination: {studyId}");
		_examService.StudyId = studyId;
		_examService.WorkflowStatus = CTS.Enums.WorkflowStatus.ExaminationStarting;

		ExamStarted?.Invoke(this, studyId);
	}

	public string GetCurrentStudy()
	{
		return _examService.StudyId;
	}

	public void Locking()
	{
		_examService.LockStatus = true;
	}

	public void Unlocking()
	{
		_examService.LockStatus = false;
	}

	public void SelectionScan(RgtScanModel scanModel)
	{
		SelectionScanChanged?.Invoke(this, new EventArgs<RgtScanModel>(scanModel));
	}

	public (StudyModel?, PatientModel?) GetCurrentStudyInfo()
	{
		if (_examService.StudyId is not null)
		{
			return _studyService.Get(_examService.StudyId);
		}

		return (new StudyModel(), new PatientModel());
	}

	public void LockScreen()
	{
		LockScreenChanged?.Invoke(this, EventArgs.Empty);
	}

	public void UnlockScreen(string nextScreen)
	{
		UnlockScreenChanged?.Invoke(this, nextScreen);
	}

	public void EnterEmergencyExam()
	{
		_isEmergency = true;

		EmergencyExamStarted?.Invoke(this, EventArgs.Empty);
	}

	public void LeaveEmergencyExam()
	{
		_isEmergency = false;
	}

	public bool IsEmergencyExam()
	{
		return _isEmergency;
	}

	public void RepairAbnormalStudy()
	{
		var tempStudyId = _examService.StudyId;
		if (!string.IsNullOrEmpty(_examService.StudyId))
		{
			_logger.LogDebug($"Close examination: {_examService.StudyId}");
			_examService.StudyId = string.Empty;
			_examService.WorkflowStatus = CTS.Enums.WorkflowStatus.ExaminationClosed;
			//TODO: 引发关闭事件，且切换状态

			//根据study id找到检查数据,更新协议状态
			var studyEntity = _studyService.GetStudyById(tempStudyId);
			var protocol = ProtocolHelper.Deserialize(studyEntity.Protocol);
			protocol.Status = PerformStatus.Performed;

			//更新协议
			var protocolContent = ProtocolHelper.Serialize(protocol);
			var studyModel = new DatabaseService.Contract.Models.StudyModel();
			studyModel.StudyDescription = protocol.Descriptor.Name;
			studyModel.Protocol = protocolContent;
			studyModel.Id = studyEntity.Id;
			studyModel.StudyId = tempStudyId;
			studyModel.BodyPart = protocol.BodyPart.ToString();
			_studyService.UpdateStudyProtocol(studyModel);

			//更新状态
			_studyService.UpdateStudyStatus(tempStudyId, WorkflowStatus.ExaminationClosed);

			Task.Run(() =>
			{
				_reportService.CreateReport(tempStudyId);
			});
		}
	}

	public event EventHandler? ConfirmExamChanged;
	public event EventHandler<EventArgs<RgtScanModel>>? SelectionScanChanged;
	public event EventHandler<string>? ExamStarted;
	public event EventHandler<string>? ExamClosed;
	public event EventHandler? LockScreenChanged;
	public event EventHandler<string>? UnlockScreenChanged;

	public event EventHandler? EmergencyExamStarted;
}
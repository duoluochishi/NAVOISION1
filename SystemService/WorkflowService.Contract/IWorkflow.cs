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
using NV.CT.CTS;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Protocol.Models;

namespace NV.CT.WorkflowService.Contract;

public interface IWorkflow
{
    bool CheckExist();

    void CloseWorkflow();

    void StartWorkflow(string studyId);

    string GetCurrentStudy();

    /// <summary>
    /// 修复异常的检查
    /// </summary>
    void RepairAbnormalStudy();

    (StudyModel?, PatientModel?) GetCurrentStudyInfo();

    /// <summary>
    /// 锁定检查,不是锁屏
    /// </summary>
    void Locking();

    void Unlocking();

    //void CreateReport(string studyId);

    //void ReplaceProtocol(string templateId);
    //void ConfirmExam();
    //event EventHandler? ConfirmExamChanged;

    event EventHandler<string>? StudyChanged;
    event EventHandler<string>? WorkflowStatusChanged;
    event EventHandler<string>? LockStatusChanged;

    void SelectionScan(RgtScanModel scanModel);

    /// <summary>
    /// 选中扫描变化事件
    /// </summary>
    public event EventHandler<EventArgs<RgtScanModel>>? SelectionScanChanged;

    public event EventHandler<string>? ExamStarted;
    public event EventHandler<string>? ExamClosed;

	/// <summary>
	/// 锁屏操作
	/// </summary>
	void LockScreen();
    public event EventHandler? LockScreenChanged;
    /// <summary>
    /// 解锁屏幕
    /// </summary>
    void UnlockScreen(string nextScreen);
    public event EventHandler<string>? UnlockScreenChanged;

    void EnterEmergencyExam();
    void LeaveEmergencyExam();
    bool IsEmergencyExam();

    public event EventHandler? EmergencyExamStarted;
}
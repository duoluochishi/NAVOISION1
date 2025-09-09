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
using NV.CT.CTS.Enums;

namespace NV.CT.WorkflowService.Impl;

public class ExamService
{

    public event EventHandler<string>? StudyChanged;

    private string _studyId = string.Empty;

    public string StudyId
    {
        get => _studyId;
        set
        {
            if (value != _studyId)
            {
                _studyId = value;
                StudyChanged?.Invoke(this, _studyId);
            }
        }
    }

    private WorkflowStatus _workflowStatus = WorkflowStatus.NotStarted;

    public event EventHandler<WorkflowStatus>? WorkflowStatusChanged;

    public WorkflowStatus WorkflowStatus
    {
        get => _workflowStatus;
        set
        {
            if (value != _workflowStatus)
            {
                _workflowStatus = value;
                WorkflowStatusChanged?.Invoke(this, _workflowStatus);
            }
        }
    }

    private bool _lockStatus = false;

    public event EventHandler<bool> LockStatusChanged;

    public bool LockStatus
    {
        get => _lockStatus;
        set
        {
            if (value != _lockStatus)
            {
                _lockStatus = value;
                LockStatusChanged?.Invoke(this, value);
            }
        }
    }
}
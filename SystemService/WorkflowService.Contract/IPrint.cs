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
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Protocol.Models;

namespace NV.CT.WorkflowService.Contract;

public interface IPrint
{
    public event EventHandler<string>? StudyChanged;
    public event EventHandler<EventArgs<(string, JobTaskStatus)>>? PrintStatusChanged;
    public event EventHandler<string>? PrintStarted;
    public event EventHandler<string>? PrintClosed;

    public bool ChceckExists();

    public void StartPrint(string studyId);

    public void ClosePrint();

    public string GetCurrentStudyId();
}
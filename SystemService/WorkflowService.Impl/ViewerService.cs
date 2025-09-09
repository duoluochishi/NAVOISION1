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
using NV.CT.WorkflowService.Contract;

namespace NV.CT.WorkflowService.Impl;

public class ViewerService : IViewer
{
    public string StudyId { get; set; }

    public event EventHandler<string> ViewerChanged;

    public void StartViewer(string studyId)
    {
        StudyId = studyId;
        ViewerChanged?.Invoke(this, studyId);
    }
    public bool CheckExists()
    {
        return !string.IsNullOrEmpty(this.StudyId);
    }
}

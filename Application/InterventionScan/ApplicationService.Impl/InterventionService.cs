//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/23 15:38:52           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.InterventionScan.ApplicationService.Contract;

namespace NV.CT.InterventionScan.ApplicationService.Impl;

public class InterventionService : IInterventionService
{
    public string ImageLayoutType { get; private set; } = string.Empty;
    public event EventHandler<EventArgs<(string commandStr, string parms)>>? OperationCommandChanged;
    public event EventHandler<EventArgs<(string path, bool isIntervention)>>? SetImagePathChanged;
    public event EventHandler<EventArgs<string>>? ImageSelectNeedleNameChanged;
    public event EventHandler<EventArgs<string>>? SelectNeedleChanged;
    public event EventHandler<EventArgs<string>>? DelectNeedleEvent;
    public event EventHandler<EventArgs<string>>? AddNeedleEvent;
    public void ImageOperationCommand(string commandStr, string parms)
    {
        if (!string.IsNullOrEmpty(commandStr) && commandStr.Equals("Layout"))
        {
            ImageLayoutType = parms;
        }
        OperationCommandChanged?.Invoke(this, new EventArgs<(string commandStr, string parms)>((commandStr, parms)));
    }

    public void SetImagePath(string path, bool isIntervention)
    {
        SetImagePathChanged?.Invoke(this, new EventArgs<(string path, bool IsIntervention)>((path, isIntervention)));
    }

    public void ImageSelectNeedleNameNotify(string needleName)
    {
        ImageSelectNeedleNameChanged?.Invoke(this, new EventArgs<string>(needleName));
    }

    public void SelectNeedleNotify(string needleName)
    {
        SelectNeedleChanged?.Invoke(this, new EventArgs<string>(needleName));
    }

    public void DelectNeedleNotify(string needleName)
    {
        DelectNeedleEvent?.Invoke(this, new EventArgs<string>(needleName));
    }

    public void AddNeedleNotify(string needleName)
    {
        AddNeedleEvent?.Invoke(this, new EventArgs<string>(needleName));
    }
}
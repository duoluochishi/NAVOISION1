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
namespace NV.CT.InterventionScan.ApplicationService.Contract;

public interface IInterventionService
{
    public string ImageLayoutType { get; }

    event EventHandler<EventArgs<(string commandStr, string parms)>> OperationCommandChanged;
    void ImageOperationCommand(string commandStr, string parms);

    event EventHandler<EventArgs<(string path, bool isIntervention)>> SetImagePathChanged;
    void SetImagePath(string imagePath, bool isIntervention);

    event EventHandler<EventArgs<string>> ImageSelectNeedleNameChanged;
    void ImageSelectNeedleNameNotify(string needleName);

    void AddNeedleNotify(string needleName);
    event EventHandler<EventArgs<string>> AddNeedleEvent;

    void SelectNeedleNotify(string needleName);
    event EventHandler<EventArgs<string>> SelectNeedleChanged;

    void DelectNeedleNotify(string needleName);
    event EventHandler<EventArgs<string>> DelectNeedleEvent;
}
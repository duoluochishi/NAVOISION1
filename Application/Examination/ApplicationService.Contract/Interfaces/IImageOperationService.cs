//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
//  2023/2/3 9:23:14        V1.0.0      Jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS;
namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IImageOperationService
{
    bool IsInverted { get; }

    event EventHandler<EventArgs<string>> ClickToolButtonChanged;
    event EventHandler<EventArgs<int>> SetImageSliceLocationChanged;
    event EventHandler<EventArgs<int>> ImageSliceIndexChanged;

    event EventHandler<EventArgs<int>> ImageCountChanged;
    event EventHandler<EventArgs<bool>> SwitchViewsChanged;

    event EventHandler<EventArgs<double>> CenterPositionChanged;

    event EventHandler<EventArgs<string>> OnSelectionReconIDChanged;

    event EventHandler<EventArgs<string>> TimeDensityInfoChangedNotify;

    event EventHandler<EventArgs<(string commandStr, string param)>> CommondToTimeDensityEvent;

    event EventHandler<EventArgs<string>> TimeDensityRoiRemoved;
    event EventHandler TimeDensityDeleteAllRoi;

    double CurrentPositon { get; }
    void DoToolsBarCommand(string commandStr);

    void SetImageSliceLocation(int index);

    void SetImageSliceIndex(int index);

    void SetImageCount(int maxNumber);

    void SetInverted();

    void SwitchViews();

    void SetCenterPositon(double currentPositon);

    void SetCurrentToCenterPositon();

    void SetSelectionReconID(string reconID);

    void SetTimeDensityInfoChanged(string param);

    void SetCommondToTimeDensity(string commandStr, string param);

    void SetTimeDensityRoiRemoved(string id);

    void DeleteAllTimeDensityRoi();
}
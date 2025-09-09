//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface ISmartPositioningService
{
    bool IsManual { get; set; }

    void GetRangePotition(PotisionStartEndLine potisionStartEndLine, double position);

    void SetRangePotitionChanged(PotisionStartEndLine potisionStartEndLine, double position);

    event EventHandler<EventArgs<(PotisionStartEndLine potisionStartEndLine, double position)>> RangePotitionChanged;

    event EventHandler<EventArgs<(PotisionStartEndLine potisionStartEndLine, double position)>> RangePotitionDistanceChanged;
}
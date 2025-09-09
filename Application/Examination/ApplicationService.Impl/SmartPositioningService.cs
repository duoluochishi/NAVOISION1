//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;

namespace NV.CT.Examination.ApplicationService.Impl;
public class SmartPositioningService : ISmartPositioningService
{
    public bool IsManual { get; set; }

    public event EventHandler<EventArgs<(PotisionStartEndLine potisionStartEndLine, double position)>>? RangePotitionChanged;

    public event EventHandler<EventArgs<(PotisionStartEndLine potisionStartEndLine, double position)>>? RangePotitionDistanceChanged;

    public void GetRangePotition(PotisionStartEndLine potisionStartEndLine, double position)
    {
        RangePotitionChanged?.Invoke(this, new EventArgs<(PotisionStartEndLine potisionStartEndLine, double position)>((potisionStartEndLine, position)));
    }

    public void SetRangePotitionChanged(PotisionStartEndLine potisionStartEndLine, double position)
    {
        RangePotitionDistanceChanged?.Invoke(this, new EventArgs<(PotisionStartEndLine potisionStartEndLine, double position)>((potisionStartEndLine, position)));
    }
}
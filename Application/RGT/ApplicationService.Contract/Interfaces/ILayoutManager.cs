using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.RGT.ApplicationService.Contract.Interfaces;

public interface ILayoutManager
{
    SyncScreens PreviousLayout { get; }
    SyncScreens CurrentLayout { get; }
    void SwitchToView(SyncScreens layout);
    void Back();
    void Go();

    event EventHandler<EventArgs<SyncScreens>>? LayoutChanged;
}
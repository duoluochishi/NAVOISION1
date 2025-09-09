using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces
{
    public interface ILayoutManager
    {
        ScanTaskAvailableLayout PreviousLayout { get; }
        ScanTaskAvailableLayout CurrentLayout { get; }
        void SwitchToView(ScanTaskAvailableLayout layout);
        void Back();
        event EventHandler<EventArgs<ScanTaskAvailableLayout>>? LayoutChanged;
    }
}

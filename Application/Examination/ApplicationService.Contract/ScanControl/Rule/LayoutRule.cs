using NV.CT.CTS.Enums;
using NV.CT.Examination.ApplicationService.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Contract.ScanControl.Rule;

public class LayoutRule : IUIControlEnableRule
{
    private readonly ILayoutManager _layoutManager;

    public LayoutRule(ILayoutManager layoutManager)
    {
        _layoutManager = layoutManager;
        _layoutManager.LayoutChanged += LayoutManager_LayoutChanged;
    }

    private void LayoutManager_LayoutChanged(object? sender, CTS.EventArgs<ScanTaskAvailableLayout> e)
    {
        UIStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsEnabled()
    {
        return _layoutManager.CurrentLayout == ScanTaskAvailableLayout.ScanDefault;
    }

    public string GetFailReason()
    {
        return "LayoutManager current layout is not ScanDefault.";
    }

    public event EventHandler? UIStatusChanged;
}

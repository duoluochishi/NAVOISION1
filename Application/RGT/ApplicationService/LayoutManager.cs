using Microsoft.Extensions.Logging;

using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.RGT.ApplicationService.Contract.Interfaces;
using NV.CT.SyncService.Contract;

namespace NV.CT.RGT.ApplicationService.Impl;

public class LayoutManager : ILayoutManager
{
    private readonly ILogger<LayoutManager> _logger;
    public SyncScreens PreviousLayout { get; set; }
    public SyncScreens CurrentLayout { get; set; }

    private readonly List<SyncScreens> AvailableLayouts;

    private readonly IScreenSync _screenSync;

    private int CurrentIndex;

    public LayoutManager(ILogger<LayoutManager> logger, IScreenSync screenSync)
    {
        _logger = logger;
        _screenSync = screenSync;

        AvailableLayouts = Enum.GetValues(typeof(SyncScreens)).Cast<SyncScreens>().ToList();

        Reset();
    }

    private void Reset()
    {
        CurrentIndex = 0;
        CurrentLayout = AvailableLayouts[CurrentIndex];
    }

    public void SwitchToView(SyncScreens layout)
    {
        _logger.LogInformation($"LayoutManager switch to {layout} view");

        var findIndex = AvailableLayouts.IndexOf(layout);
        if (findIndex != -1)
        {
            CurrentIndex = findIndex;
        }

        Change();
    }

    public void Back()
    {
        --CurrentIndex;
        if (CurrentIndex < 0)
        {
            CurrentIndex = 0;
        }

        Change();
    }

    public void Go()
    {
        ++CurrentIndex;
        if (CurrentIndex >= AvailableLayouts.Count)
        {
            CurrentIndex = AvailableLayouts.Count - 1;
        }

        Change();

        _screenSync.Go();
    }

    private void Change()
    {
        _logger.LogInformation($"LayoutManager current index:{CurrentIndex}");
        var targetLayout = AvailableLayouts[CurrentIndex];
        if (targetLayout != CurrentLayout)
        {
            PreviousLayout = CurrentLayout;
            CurrentLayout = targetLayout;
            LayoutChanged?.Invoke(this, new EventArgs<SyncScreens>(CurrentLayout));
        }
    }

    public void Resume()
    {
        Reset();
    }

    public event EventHandler<EventArgs<SyncScreens>>? LayoutChanged;
}
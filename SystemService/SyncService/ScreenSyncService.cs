using Microsoft.Extensions.Logging;

using NV.CT.CTS.Enums;
using NV.CT.SyncService.Contract;

namespace NV.CT.SyncService;

public class ScreenSyncService : IScreenSync
{
    private readonly ILogger<ScreenSyncService> _logger;
    public SyncScreens PreviousLayout { get; set; }
    public SyncScreens CurrentLayout { get; set; }

    private int _currentIndex;

    private readonly List<SyncScreens> _availableLayouts;

    public ScreenSyncService(ILogger<ScreenSyncService> logger)
    {
        _logger = logger;

        _availableLayouts = Enum.GetValues(typeof(SyncScreens)).Cast<SyncScreens>().ToList();

        Reset();
    }

    private void Reset()
    {
        _currentIndex = 0;
        CurrentLayout = _availableLayouts[_currentIndex];
    }

    public string GetPreviousLayout()
    {
        return PreviousLayout.ToString();
    }

    public string GetCurrentLayout()
    {
        return CurrentLayout.ToString();
    }

    public void SwitchTo(string syncScreens)
    {
        _logger.LogInformation($"ScreenSyncService switch to {syncScreens} view");

        var findIndex = _availableLayouts.IndexOf(Enum.Parse<SyncScreens>(syncScreens));
        if (findIndex == -1)
            return;

        _currentIndex = findIndex;

        Change();
    }

    public void Back()
    {
        --_currentIndex;
        if (_currentIndex < 0)
        {
            _currentIndex = 0;
        }

        Change();
    }

    public void Go()
    {
        ++_currentIndex;
        if (_currentIndex >= _availableLayouts.Count)
        {
            _currentIndex = _availableLayouts.Count - 1;
        }

        Change();
    }

    private void Change()
    {
        var targetLayout = _availableLayouts[_currentIndex];

        _logger.LogInformation($"ScreenSyncService current index:{_currentIndex} , targetLayout is {targetLayout}, currentLayout is {CurrentLayout}");

        if (targetLayout != CurrentLayout)
        {
            PreviousLayout = CurrentLayout;
            CurrentLayout = targetLayout;
            ScreenChanged?.Invoke(this, CurrentLayout.ToString());
        }
    }

    public void Resume()
    {
        Reset();
    }

    public event EventHandler<string>? ScreenChanged;
}
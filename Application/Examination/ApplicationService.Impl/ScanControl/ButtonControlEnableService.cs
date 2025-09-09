using Microsoft.Extensions.Logging;

using NV.CT.Examination.ApplicationService.Contract.ScanControl;

namespace NV.CT.Examination.ApplicationService.Impl.ScanControl;

public class ButtonControlEnableService : IUIControlEnableService
{
    private readonly ILogger _logger;
    private readonly List<IUIControlEnableRule> _rules;
    private IUIControlEnableRule? _breakRule;
    public ButtonControlEnableService(ILogger<ButtonControlEnableService> logger)
    {
        _logger = logger;
        _rules = new List<IUIControlEnableRule>();
    }

    public void RegisterRule(IUIControlEnableRule rule)
    {
        _rules.Add(rule);
        rule.UIStatusChanged += Rule_UIStatusChanged;
    }

    private void Rule_UIStatusChanged(object? sender, EventArgs e)
    {
        var p1 = IsEnabled();
        var p2 = GetFirstFailReason();
        UIStatusChanged?.Invoke(this, (p1, p2));
        _logger.LogInformation($"rule status changed to IsEnable={p1},FailReason='{p2}' ");
    }

    public bool IsEnabled()
    {
        var initialEnable = true;
        foreach (var rule in _rules)
        {
            if (!rule.IsEnabled())
            {
                _breakRule = rule;
                initialEnable = false;
                break;
            }
        }

        return initialEnable;
    }

    public string GetFirstFailReason()
    {
        return _breakRule?.GetFailReason() ?? string.Empty;
    }

    public event EventHandler<(bool, string)>? UIStatusChanged;
}

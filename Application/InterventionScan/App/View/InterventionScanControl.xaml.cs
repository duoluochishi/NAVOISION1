//-----------------------------------------------------------------------
// <copyright company="纳米维景">Intervention
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.InterventionScan.Layout;
using NV.CT.InterventionScan.ViewModel;

namespace NV.CT.InterventionScan.View;

public partial class InterventionScanControl
{
    private readonly ILogger<InterventionScanControl>? _logger;
    private readonly ILayoutManager? _layoutManager;
  
    public InterventionScanControl()
    {
        InitializeComponent();       
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<InterventionScanViewModel>();

        foreach (ResourceDictionary resourceDictionary in LoadingResource.LoadingInControl())
        {
            Resources.MergedDictionaries.Add(resourceDictionary);
        }

        _logger = CTS.Global.ServiceProvider?.GetRequiredService<ILogger<InterventionScanControl>>();
        _layoutManager = CTS.Global.ServiceProvider?.GetRequiredService<ILayoutManager>();
        if (_layoutManager != null)
            _layoutManager.LayoutChanged += LayoutChanged;
        InitContentView();
       
    }

    private void InitContentView()
    {
        _logger?.LogInformation($"Init content view");
        LayoutContainer.Content = CTS.Global.ServiceProvider?.GetRequiredService<LayoutControl>();
    }

    private void LayoutChanged(object? sender, EventArgs<ScanTaskAvailableLayout> e)
    {
        UserControl? uc = null;
        switch (e.Data)
        {
            case ScanTaskAvailableLayout.InterventionScan:
                uc = CTS.Global.ServiceProvider?.GetRequiredService<LayoutControl>();
                break;
        }

        if (uc is not null)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                LayoutContainer.Content = uc;
            });
        }
    }   
}
using NV.CT.ConfigManagement.ViewModel;
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.ConfigManagement;

public class Initializer : IInitializer
{
    public void Initialize()
    {
        CTS.Global.Logger?.LogDebug($"ConfigManagement");
        PreInitObjects();
    }

    private void PreInitObjects()
    {
        CTS.Global.ServiceProvider?.GetRequiredService<UserViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<RoleViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<ArchiveNodeViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<WindowingViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<WorklistNodeViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<PrintNodeViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<TabletViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<PrintProtocolViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<CoefficientViewModel>();
    }
}
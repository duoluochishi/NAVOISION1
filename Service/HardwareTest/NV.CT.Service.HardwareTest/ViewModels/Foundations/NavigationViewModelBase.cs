using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.HardwareTest.Services.Universal.Navigation.Abstractions;

namespace NV.CT.Service.HardwareTest.ViewModels.Foundations
{
    public abstract class NavigationViewModelBase : ObservableObject, INavigationAware
    {
        public abstract void BeforeNavigateToCurrentPage();

        public abstract void BeforeNavigateToOtherPage();
    }

}

namespace NV.CT.Service.HardwareTest.Services.Universal.Navigation.Abstractions
{
    public interface INavigationAware
    {
        void BeforeNavigateToCurrentPage();

        void BeforeNavigateToOtherPage();
    }

}

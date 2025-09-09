using System;
using System.Windows;

namespace NV.CT.Service.HardwareTest.Services.Universal.Navigation.Abstractions
{
    public interface INavigationService
    {

        FrameworkElement? CurrentView { get; set; }

        event Action? CurrentViewChanged;

        void NavigateTo(Type type);

        void NavigateTo<T>() where T : FrameworkElement;

        void AfterLoadHardwareTestModule();

        void BeforeLeaveHardwareTestModule();

    }
}
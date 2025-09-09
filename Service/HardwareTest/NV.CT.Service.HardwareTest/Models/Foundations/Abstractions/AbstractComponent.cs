using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Models.Foundations.Abstractions
{
    public partial class AbstractComponent : ObservableObject
    {
        [ObservableProperty]
        private string name = null!;
        [ObservableProperty]
        private List<ComponentTestItem> componentTestItemMenu = null!;
        [ObservableProperty]
        private ComponentTestItem? currentTestItem;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Models.Foundations
{
    public partial class ComponentNavigationItem : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;
        [ObservableProperty]
        private Type navigationViewType = null!;
        [ObservableProperty]
        private List<ComponentTestItem> componentTestItemMenu = null!;
        [ObservableProperty]
        private ComponentTestItem? currentTestItem;
    }

    public class ComponentTestItem
    {
        public string Name { get; set; } = string.Empty;
        public Type NavigationViewType { set; get; } = null!;
    }
}

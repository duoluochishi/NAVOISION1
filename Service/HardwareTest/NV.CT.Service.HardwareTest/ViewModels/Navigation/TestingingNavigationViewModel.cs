using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NV.CT.Service.HardwareTest.Models.Foundations;
using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;
using NV.CT.Service.HardwareTest.Services.Universal.Navigation.Abstractions;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.Service.HardwareTest.ViewModels.Navigation
{
    public partial class NavigationViewModel : ObservableObject
    {
        /** 导航服务 **/
        private readonly INavigationService navigationService;

        public NavigationViewModel(INavigationService navigationService)
        {
            /** Get from DI **/
            this.navigationService = navigationService;
            /** Initialize **/
            this.InitializeNavigation();          
        }

        #region Initialize

        private void InitializeNavigation()
        {
            /** 初始化导航页面列表 **/
            this.NavigationComponents = new ObservableCollection<AbstractComponent>
            {
                new SystemComponent(),
                new XRaySourceComponent(),
                new CollimatorComponent(),
                new TableComponent(),
                new DetectorComponent()            
            };
            /** 注册当前页改变事件 **/
            navigationService.CurrentViewChanged += NavigationService_CurrentViewChanged;
            /** 导航至默认页 **/
            this.CurrentNavigationComponent = this.NavigationComponents.First();
            this.CurrentNavigationComponent.CurrentTestItem = this.CurrentNavigationComponent.ComponentTestItemMenu.First();
            navigationService.NavigateTo(this.CurrentNavigationComponent.CurrentTestItem.NavigationViewType);
        }

        private void NavigationService_CurrentViewChanged()
        {
            this.CurrentNavigationView = navigationService.CurrentView!;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void NavigateSelectedComponent(AbstractComponent component) 
        {
            /** 校验 **/
            if (component is null) return;
            /** 更新被选中Component **/
            this.CurrentNavigationComponent = component;
            /** 更新Index **/
            this.CurrentTestItemIndex = 0;
        }

        [RelayCommand]
        private void NavigateSelectedTestingItem(ComponentTestItem testItem) 
        {
            /** 校验 **/
            if (testItem is null) return;
            /** 更新被选中TestItem **/
            this.CurrentNavigationComponent.CurrentTestItem = testItem;
            /** 导航至被选中的页面 **/
            navigationService.NavigateTo(testItem.NavigationViewType);
        }

        #endregion

        #region Properties

        public ObservableCollection<AbstractComponent> NavigationComponents { set; get; } = null!;

        [ObservableProperty]
        private AbstractComponent currentNavigationComponent = null!;

        [ObservableProperty]
        private int currentTestItemIndex = 0;

        [ObservableProperty]
        private object currentNavigationView = null!;

        #endregion

    }
}

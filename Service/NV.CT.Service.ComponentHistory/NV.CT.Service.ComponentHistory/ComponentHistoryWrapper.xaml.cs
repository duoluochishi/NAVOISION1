using NV.CT.Service.ComponentHistory.ViewModels;
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.Service.ComponentHistory
{
    /// <summary>
    /// ComponentHistoryWrapper.xaml 的交互逻辑
    /// </summary>
    public partial class ComponentHistoryWrapper : IServiceControl
    {
        public ComponentHistoryWrapper()
        {
            InitializeComponent();
        }

        #region 适应MCS服务框架接口要求

        public const string ServiceModuleName = "ComponentHistory";

        public string GetServiceAppName()
        {
            return ServiceModuleName;
        }

        public string GetServiceAppID()
        {
            return ServiceModuleName;
        }

        public string GetTipOnClosing()
        {
            if (ComponentHistoryView.DataContext is ComponentHistoryViewModel vm)
            {
                return vm.IsBusing ? "Component History is running!" : string.Empty;
            }

            return string.Empty;
        }

        #endregion
    }
}
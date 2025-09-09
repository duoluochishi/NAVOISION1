using NV.CT.Service.Common;
using NV.CT.Service.Upgrade.ViewModels;
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.Service.Upgrade
{
    /// <summary>
    /// UpgradeWrapper.xaml 的交互逻辑
    /// </summary>
    public partial class UpgradeWrapper : IServiceControl
    {
        public UpgradeWrapper()
        {
            InitializeComponent();
        }

        #region 适应MCS服务框架接口要求

        public string GetServiceAppName()
        {
            return Global.ServiceAppName;
        }

        public string GetServiceAppID()
        {
            return Global.ServiceAppName;
        }

        public string GetTipOnClosing()
        {
            return (UpgradeView.DataContext as UpgradeViewModel)?.OnMCSClosing() ?? string.Empty;
        }

        #endregion 适应MCS服务框架接口要求
    }
}
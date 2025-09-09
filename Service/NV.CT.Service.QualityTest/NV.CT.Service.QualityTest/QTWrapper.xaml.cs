using NV.CT.Service.QualityTest.ViewModels;
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.Service.QualityTest
{
    /// <summary>
    /// QTWrapper.xaml 的交互逻辑
    /// </summary>
    public partial class QTWrapper : IServiceControl
    {
        public QTWrapper()
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
            return (QTView.DataContext as QTViewModel)?.OnMCSClosing() ?? string.Empty;
        }

        #endregion 适应MCS服务框架接口要求
    }
}
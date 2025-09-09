using NV.CT.ServiceFramework.Contract;

namespace NV.CT.Service.TubeHistory
{
    /// <summary>
    /// TubeHistoryWrapper.xaml 的交互逻辑
    /// </summary>
    public partial class TubeHistoryWrapper : IServiceControl
    {
        public TubeHistoryWrapper()
        {
            InitializeComponent();
        }

        #region 适应MCS服务框架接口要求

        private const string ServiceModuleName = "TubeHistory";

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
            return string.Empty;
        }

        #endregion
    }
}
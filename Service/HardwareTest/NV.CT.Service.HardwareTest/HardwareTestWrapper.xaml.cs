using NV.CT.ServiceFramework.Contract;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest
{
    public partial class HardwareTestWrapper : UserControl, IServiceControl
    {        
        public HardwareTestWrapper()
        {
            InitializeComponent();
        }

        #region Fields

        public static readonly string Service_Module_Name = "HardwareTest";

        #endregion

        #region 适应MCS服务框架接口要求

        public string GetServiceAppID()
        {
            return Service_Module_Name;
        }

        public string GetServiceAppName()
        {
            return Service_Module_Name;
        }

        public string GetTipOnClosing()
        {
            return string.Empty;
        }

        #endregion
    }
}

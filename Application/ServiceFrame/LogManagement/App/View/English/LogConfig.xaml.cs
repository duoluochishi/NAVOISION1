using NV.CT.ServiceFramework.Contract;
using System.Windows.Controls;

namespace NV.CT.LogManagement.View.English
{
    /// <summary>
    /// LogConfig.xaml 的交互逻辑
    /// </summary>
    public partial class LogConfig : UserControl, IServiceControl
    {
        public LogConfig()
        {
            InitializeComponent();
        }
        public string GetServiceAppName()
        {
            return string.Empty;
        }

        public string GetServiceAppID()
        {
            return string.Empty;
        }

        public string GetTipOnClosing()
        {
            return string.Empty;
        }

    }
}

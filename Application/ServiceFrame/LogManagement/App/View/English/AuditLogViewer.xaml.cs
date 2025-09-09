using NV.CT.ServiceFramework.Contract;
using System.Windows.Controls;

namespace NV.CT.LogManagement.View.English
{
    /// <summary>
    /// AuditLogViewer.xaml 的交互逻辑
    /// </summary>
    public partial class AuditLogViewer : UserControl, IServiceControl
    {
        public AuditLogViewer()
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

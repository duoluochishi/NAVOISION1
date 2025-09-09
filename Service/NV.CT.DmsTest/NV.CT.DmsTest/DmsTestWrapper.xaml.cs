using NV.CT.ServiceFramework.Contract;
using System.Windows.Controls;

namespace NV.CT.DmsTest
{
    /// <summary>
    /// DmsTestWrapper.xaml 的交互逻辑
    /// </summary>
    public partial class DmsTestWrapper : UserControl, IServiceControl
    {
        public DmsTestWrapper()
        {
            //Debugger.Launch();
            InitializeComponent();
        }

        public string GetServiceAppID()
        {
            return "DMSTest";
        }

        public string GetServiceAppName()
        {
            return "DMSTest";
        }

        public string GetTipOnClosing()
        {
            return "DMSTest";
        }

    }
}

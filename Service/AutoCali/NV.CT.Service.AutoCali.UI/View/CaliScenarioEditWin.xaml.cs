using NV.CT.Service.UI.Util;
using NV.MPS.UI.Dialog;
using System.Windows;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// 新增或者修改 校准场景 的窗口
    /// CaliScenarioEditWin.xaml 的交互逻辑
    /// </summary>
    public partial class CaliScenarioEditWin : BaseCustomWindow
    {
        public CaliScenarioEditWin()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowHelper.Register<CaliItemArgProtocolEditWin>(ClassName);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            WindowHelper.Unregister(ClassName);
        }

        private static readonly string ClassName = nameof(CaliItemArgProtocolEditWin);
    }
}

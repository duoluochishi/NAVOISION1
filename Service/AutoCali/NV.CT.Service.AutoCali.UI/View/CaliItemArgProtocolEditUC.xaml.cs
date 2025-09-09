using NV.CT.Service.UI.Util;
using System.Windows;
using System.Windows.Controls;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// 新增或者修改 校准项目的参数协议 的窗口
    /// CaliItemArgProtocolEditUC.xaml 的交互逻辑
    /// </summary>
    public partial class CaliItemArgProtocolEditUC : UserControl
    {
        public CaliItemArgProtocolEditUC()
        {
            InitializeComponent();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            WindowHelper.Close(this);
        }
    }
}

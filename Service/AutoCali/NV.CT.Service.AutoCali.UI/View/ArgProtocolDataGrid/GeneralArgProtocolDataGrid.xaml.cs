using System.Windows.Controls;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// GeneralArgProtocolDataGrid.xaml 的交互逻辑
    /// </summary>
    public partial class GeneralArgProtocolDataGrid : UserControl
    {
        public GeneralArgProtocolDataGrid()
        {
            InitializeComponent();
        }
        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}

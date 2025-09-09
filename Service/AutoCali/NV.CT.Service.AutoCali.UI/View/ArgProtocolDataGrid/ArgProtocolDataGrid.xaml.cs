using System.Windows.Controls;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// ArgProtocolDataGrid.xaml 的交互逻辑
    /// </summary>
    public partial class ArgProtocolDataGrid : DataGrid
    {
        public ArgProtocolDataGrid()
        {
            InitializeComponent();
        }
        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}

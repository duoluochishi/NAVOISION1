//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.ConfigManagement.ViewModel;
using System.Windows.Input;

namespace NV.CT.ConfigManagement.View;

public partial class PrintProtocolWindow
{
    private bool _isInCellSelectionMode = false;
    public PrintProtocolWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<PrintProtocolViewModel>();
    }

    private void OnCellMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isInCellSelectionMode)
        {
            return;
        }
        if (sender is TextBlock block
            && block.DataContext is CellViewModel cellViewModel
            && this.DataContext is PrintProtocolViewModel model)
        {
            model.CurrentNode.Row = cellViewModel.RowNumber + 1;
            model.CurrentNode.Column = cellViewModel.ColumnNumber + 1;
        }
    }

    private void OnCellMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock block
            && block.DataContext is CellViewModel cellViewModel
            && this.DataContext is PrintProtocolViewModel model
            && _isInCellSelectionMode)
        {
            model.CurrentNode.Row = cellViewModel.RowNumber + 1;
            model.CurrentNode.Column = cellViewModel.ColumnNumber + 1;
            _isInCellSelectionMode = false;
        }
    }

    private void OnCellMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBlock block
            && this.DataContext is PrintProtocolViewModel
            && block.DataContext is CellViewModel cellViewModel)
        {
            if (cellViewModel.RowNumber == 0 && cellViewModel.ColumnNumber == 0)
            {
                _isInCellSelectionMode = true;
                cellViewModel.IsEnable = true.ToString();
            }
            else
            {
                _isInCellSelectionMode = false;
            }
        }
    }
}
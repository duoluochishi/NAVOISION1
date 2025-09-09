using System;
using System.Windows.Controls;

namespace NV.CT.Service.Common
{
    /// <summary>
    /// 表格控件工具
    /// </summary>
    public class DataGridUtil
    {
        public static void AddRowHeader(DataGrid dataGrid)
        {
            dataGrid.LoadingRow -= DataGrid_LoadingRow;
            dataGrid.LoadingRow += DataGrid_LoadingRow;
        }

        private static void DataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
            //Console.WriteLine($"[DataGrid({sender?.GetHashCode()}]Set RowHeader=" + e.Row.Header.ToString());
        }
    }
}

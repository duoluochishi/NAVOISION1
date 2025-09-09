using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.DataAcquisition;
using RawDataHelperWrapper;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.UserControls.Integrations.DataAcquistion
{
    public partial class DataAcquisitionImageSortView : UserControl
    {
        public DataAcquisitionImageSortView()
        {
            InitializeComponent();

            DataContext = Ioc.Default.GetService<DataAcquisitionTestingViewModel>();
        }

        #region Fields

        /// 数据列表头名称 与 排序对应属性列表的 映射
        /// </summary>
        private static readonly Dictionary<string, string[]> ColumnHeaderSortDescriptionsDictionary;

        static DataAcquisitionImageSortView()
        {
            ColumnHeaderSortDescriptionsDictionary = new()
            {
                { "Frame Series Number", new string[] { "FrameNoInSeries" } },
                { "Source ID", new string[] { "SourceId", "FrameNoInSeries" } },
                { "Table Position", new string[] { "TablePosition", "FrameNoInSeries" } },
                { "Gantry Rotate Angle", new string[] { "GantryAngle", "FrameNoInSeries" } }
            };
        }

        #endregion

        #region Image Sort

        private void RawDataGrid_Sorting(object sender, DataGridSortingEventArgs args)
        {
            var rawDataPool = (IEnumerable<RawData>)RawDataGrid.ItemsSource;
            //空校验 
            if (!rawDataPool.Any()) return;

            //获取被选中的列名 
            var columnHeaderName = args.Column.Header.ToString();
            //列名空校验
            if (string.IsNullOrWhiteSpace(columnHeaderName)) return;

            if (!ColumnHeaderSortDescriptionsDictionary.TryGetValue(columnHeaderName, out var sortPropertyNames))
            {
                return;
            }

            //获取排序方向(与Column.SortDirection相反) 
            var sortDirection = (args.Column.SortDirection == null)
                ? ListSortDirection.Ascending
                : (args.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);

            var viewSource = CollectionViewSource.GetDefaultView(rawDataPool);            
            viewSource.SortDescriptions.Clear();

            foreach (var sortPropertyName in sortPropertyNames)
            {
                viewSource.SortDescriptions.Add(new SortDescription(sortPropertyName, sortDirection));
            }
            args.Column.SortDirection = sortDirection;
            args.Handled = true;
        }

        private void ApplyImageSortButton_Click(object sender, RoutedEventArgs e)
        {
            //获取绑定的数据源 
            var viewSource = CollectionViewSource.GetDefaultView(RawDataGrid.ItemsSource);
            var SortedRawDataInfoList = viewSource.Cast<RawData>().ToList();
            // 更新数据源 
            if (SortedRawDataInfoList.Count > 0)
            {
                WeakReferenceMessenger.Default.Send(new RawDataSetSortedMessage(SortedRawDataInfoList));
            }
            // 关闭当前窗口 
            DialogHelper.Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ClearSortDescriptions();
        }

        private void ClearSortDescriptions()
        {
            var viewSource = CollectionViewSource.GetDefaultView(RawDataGrid.ItemsSource);
            viewSource.SortDescriptions.Clear();
        }

        #endregion
    }
}

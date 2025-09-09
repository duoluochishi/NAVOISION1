using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.ImageChain;
using RawDataHelperWrapper;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NV.CT.Service.HardwareTest.UserControls.Integrations.ImageChain
{
    public partial class ImageChainImageSortView : UserControl
    {
        public ImageChainImageSortView()
        {
            InitializeComponent();
            //DataContext 
            this.DataContext = Ioc.Default.GetService<ImageChainTestingViewModel>();
        }

        public IEnumerable<RawData> SortedImages { get; set; } = null!;   

        /// <summary>
        /// 数据列表头名称 与 排序对应属性列表的 映射
        /// </summary>
        private static readonly Dictionary<string, string[]> ColumnHeaderSortDescriptionsDictionary;

        static ImageChainImageSortView()
        {
            ColumnHeaderSortDescriptionsDictionary = new()
            {
                { "Frame Series Number", new string[] { "FrameNoInSeries" } },
                { "Source ID", new string[] { "SourceId", "FrameNoInSeries" } },
                { "Table Position", new string[] { "TablePosition", "FrameNoInSeries" } },
                { "Gantry Rotate Angle", new string[] { "GantryAngle", "FrameNoInSeries" } }
            };
        }

        private void RawDataGrid_Sorting(object sender, DataGridSortingEventArgs args)
        {
            //获取绑定的数据源 
            var rawDataPool = (IEnumerable<RawData>)this.RawDataGrid.ItemsSource;
            //数据空校验
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
            var viewSource = CollectionViewSource.GetDefaultView(this.RawDataGrid.ItemsSource);
            var SortedImages = viewSource.Cast<RawData>().ToList();

            //更新数据源 
            if (SortedImages is not null)
            {
                WeakReferenceMessenger.Default.Send(new ImageChainImageSortedMessage(SortedImages));
            }
            //关闭当前窗口 
            DialogHelper.Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ClearSortDescriptions();
        }

        private void ClearSortDescriptions()
        {
            var viewSource = CollectionViewSource.GetDefaultView(this.RawDataGrid.ItemsSource);
            viewSource.SortDescriptions.Clear();
        }
    }
}

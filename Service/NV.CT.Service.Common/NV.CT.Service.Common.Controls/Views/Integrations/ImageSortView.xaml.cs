using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.Common.Controls.Attachments.Helpers;
using NV.CT.Service.Common.Controls.Attachments.Managers;
using NV.CT.Service.Common.Controls.Attachments.Messages;
using NV.CT.Service.Common.Controls.ViewModels;
using RawDataHelperWrapper;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NV.CT.Service.Common.Controls.Integrations
{
    public partial class ImageSortView : UserControl
    {
        public ImageSortView()
        {
            InitializeComponent();
            //DataContext 
            //this.DataContext = IocContainer.Instance.Services.GetService<ImageViewerViewModel>();
        }

        /// <summary>
        /// 数据列表头名称 与 排序对应属性列表的 映射
        /// </summary>
        private static readonly Dictionary<string, string[]> ColumnHeaderSortDescriptionsDictionary;

        static ImageSortView()
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
                WeakReferenceMessenger.Default.Send(new ImageSortedMessage(SortedImages), _messengerToken);
            }
            //关闭当前窗口 
            WindowHelper.Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is IMessengerToken messengerToken)
            {
                _messengerToken = messengerToken?.MessengerToken;
            }

            ClearSortDescriptions();
        }

        private void ClearSortDescriptions()
        {
            var viewSource = CollectionViewSource.GetDefaultView(this.RawDataGrid.ItemsSource);
            viewSource.SortDescriptions.Clear();
        }
        private string _messengerToken = string.Empty;
    }
}

using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.ViewModel;

using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;
using ImageModel = NV.CT.ImageViewer.Model.ImageModel;
using SeriesModel = NV.CT.ImageViewer.Model.SeriesModel;
using Point = System.Windows.Point;

namespace NV.CT.ImageViewer.View;

/// <summary>
/// SeriesHistoryControl.xaml 的交互逻辑
/// </summary>
public partial class SeriesHistoryControl
{
	private Point _startPoint;

	public SeriesHistoryControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<HistorySeriesViewModel>();

		ShowListLayout();
	}

	private void DataGrid_OnLoadingRow(object? sender, DataGridRowEventArgs e)
	{
		e.Row.Header = e.Row.GetIndex() + 1;
	}

	/// <summary>
	/// 双击 序列 触发的事件
	/// </summary>
	private void ListSeries_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (sender is ListView listView)
		{
			if (listView.SelectedItem is ImageModel selectedItem)
			{
				EventAggregator.Instance.GetEvent<SelectedSeriesChangedEvent>().Publish(selectedItem);
			}
		}
	}
	private void tableSeries_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (sender is DataGrid dataGrid)
		{
			if (dataGrid.SelectedItem is SeriesModel selectedItem)
			{
				EventAggregator.Instance.GetEvent<SeletedDataGridSeriesChangedEvent>().Publish(selectedItem);
			}
		}
	}
	private void series_grid_layout(object sender, RoutedEventArgs e)
	{
		ShowListLayout();
	}

	private void series_table_layout(object sender, RoutedEventArgs e)
	{
		ShowTableLayout();
	}

	private void ShowListLayout()
	{
		listSeries.Visibility = Visibility.Visible;
		tableSeries.Visibility = Visibility.Collapsed;
	}

	private void ShowTableLayout()
	{
		listSeries.Visibility = Visibility.Collapsed;
		tableSeries.Visibility = Visibility.Visible;
	}

	#region dnd
	private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_startPoint = e.GetPosition(null);
	}

	private void OnPreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (e.LeftButton != MouseButtonState.Pressed)
		{
			return;
		}

		var mousePos = e.GetPosition(null);
		var diff = _startPoint - mousePos;

		if (e.LeftButton == MouseButtonState.Pressed &&
			Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
			Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
		{
			ListView? listView = sender as ListView;
			ListViewItem listViewItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

			if (listViewItem == null)
				return;

			var passedData = listView?.ItemContainerGenerator.ItemFromContainer(listViewItem) as ImageModel;

			if (passedData is null)
				return;

			DataObject dragData = new DataObject(GeneralImageViewer.DragObjectKey, passedData.SeriesPath);
			DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
		}
	}

	public static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
	{
		do
		{
			if (current is T)
				return (T)current;

			current = VisualTreeHelper.GetParent(current);

		} while (current != null);

		return null;
	}
	#endregion
}
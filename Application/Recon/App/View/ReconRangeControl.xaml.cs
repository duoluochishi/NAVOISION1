using System.Windows.Controls;
using System.Windows.Input;

namespace NV.CT.Recon.View;

public partial class ReconRangeControl : UserControl
{
	public ReconRangeControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<ReconRangeViewModel>();

		Loaded += ReconRangeControl_Loaded;
	}

	private void ReconRangeControl_Loaded(object sender, RoutedEventArgs e)
	{
		EventAggregator.Instance.GetEvent<ReconRangeLoadCompletedEvent>().Publish();
	}

	private void ScrollViewer1_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer viewer = ScrollViewer1;
		if (viewer == null)
			return;
		double num = Math.Abs((int)(e.Delta / 2));
		double offset = 0.0;
		if (e.Delta > 0)
		{
			offset = Math.Max((double)0.0, (double)(viewer.VerticalOffset - num));
		}
		else
		{
			offset = Math.Min(viewer.ScrollableHeight, viewer.VerticalOffset + num);
		}
		if (offset != viewer.VerticalOffset)
		{
			viewer.ScrollToVerticalOffset(offset);
			e.Handled = true;
		}
	}
}
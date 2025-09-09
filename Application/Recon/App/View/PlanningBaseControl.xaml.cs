using System.Windows.Controls;
using System.Windows.Input;

namespace NV.CT.Recon.View;

public partial class PlanningBaseControl : UserControl
{
	public PlanningBaseControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<PlanningBaseViewModel>();

		Loaded += PlanningBaseControl_Loaded;
	}

	private void PlanningBaseControl_Loaded(object sender, RoutedEventArgs e)
	{
		EventAggregator.Instance.GetEvent<PlanningBaseLoadCompletedEvent>().Publish();
	}

	private void ScrollViewer2_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer viewer = ScrollViewer2;
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
using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View;

public partial class FooterHistoryControl
{
	public FooterHistoryControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<HistorySeriesViewModel>();

		btnPrint.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelPrint_OnMouseLeftButtonDown), true);
		btnArchive.AddHandler(Label.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LabelArchive_OnMouseLeftButtonDown), true);
		menuPrint.DataContext = DataContext;
		menuArchive.DataContext = DataContext;
	}

	private void LabelPrint_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		menuPrint.IsOpen = true;
	}

	private void LabelArchive_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		menuArchive.IsOpen = true;
	}
}
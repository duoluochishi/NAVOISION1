using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.Model;
using NV.CT.ImageViewer.ViewModel;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;

namespace NV.CT.ImageViewer.View;

public partial class FilmWindow
{
	private readonly Image2DViewModel _viewModel;
	public FilmWindow()
	{
		InitializeComponent();

		WindowStartupLocation = WindowStartupLocation.Manual;

		MouseDown += (_, _) =>
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				DragMove();
			}
		};

		//grid.MouseLeftButtonDown += Grid_MouseLeftButtonDown;

		_viewModel = CTS.Global.ServiceProvider.GetRequiredService<Image2DViewModel>();
		DataContext = _viewModel;

		EventAggregator.Instance.GetEvent<SelectedSeriesChangedEvent>().Subscribe(SelectedSeriesChanged);

	}

	private void SelectedSeriesChanged(ImageModel model)
	{
		if (IsVisible)
		{
			CloseFilmWindow();
		}
	}

	private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		DragMove();
	}

	private void BtnClose_OnClick(object sender, RoutedEventArgs e)
	{
		CloseFilmWindow();
	}

	private void CloseFilmWindow()
	{
		_viewModel.CurrentImageViewer.CineStop();
        _viewModel.IsPlayVisible = true;
        _viewModel.IsPauseVisible = false;

        EventAggregator.Instance.GetEvent<FilmPlayChangedEvent>().Publish(false);

		Hide();
	}
}
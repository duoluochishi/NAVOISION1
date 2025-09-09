//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.ViewModel;
using NV.MPS.Configuration;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;

namespace NV.CT.ImageViewer.View;

public partial class MainControl
{
    public MainControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetRequiredService<MainControlViewerModel>();

		Loaded += MainControl_Loaded;
	}


	private void MainControl_Loaded(object sender, RoutedEventArgs e)
	{

		//TODO: 事件拦截的问题，导致前端界面拿不到事件
		//BindKeyBinding();

		HandleFilmPlayRelated();
	}

	private void HandleFilmPlayRelated()
	{
		EventAggregator.Instance.GetEvent<FilmPlayChangedEvent>().Subscribe(filmControlShown =>
		{
			if (filmControlShown)
			{
				tabContainer.IsEnabled = false;
			}
			else
			{
				tabContainer.IsEnabled = true;
			}
		});
	}

	private void BindKeyBinding()
	{
		var windowTypes = UserConfig.WindowingConfig.Windowings;
		if (windowTypes is null)
			return;

		if (Application.Current.MainWindow != null)
		{
			windowTypes.ForEach(item =>
			{
				CreateKeyBinding(Application.Current.MainWindow, item.Shortcut, (_, _) =>
				{
					if (item.BodyPart == "Custom")
					{
						ShowCustomWWWLWindow();
					}
					else
					{
						if (tabContainer.SelectedIndex == 0)
						{
							var _vm = CTS.Global.ServiceProvider.GetService<Image2DViewModel>();
							_vm?.CurrentImageViewer.SetWWWL(item.Width.Value,item.Level.Value);
						}
						else
						{
							var _vm = CTS.Global.ServiceProvider.GetService<Image3DViewModel>();
							_vm?.CurrentImageViewer.SetWWWL3D(item.Width.Value, item.Level.Value);
						}

					}
				});
			});
		}
	}

	public void ShowCustomWWWLWindow()
	{
		var _customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();

		if (_customWwwlWindow is null)
		{
			_customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();
		}

		if (_customWwwlWindow != null)
		{
			_customWwwlWindow.SetScene(tabContainer.SelectedIndex == 0 ? ViewScene.View2D : ViewScene.View3D);
			WindowDialogShow.Show(_customWwwlWindow);
		}
	}

	public bool CreateKeyBinding(UIElement target, string hotKey, ExecutedRoutedEventHandler handler)
	{
		if (target == null || string.IsNullOrEmpty(hotKey) || handler == null)
			return false;

		try
		{
			Key key = (Key)Enum.Parse(typeof(Key), hotKey);

			RoutedUICommand routedUICommand = new RoutedUICommand();
			CommandBinding commandBinding = new CommandBinding(routedUICommand, handler);
			KeyBinding keyBinding = new KeyBinding(routedUICommand, new KeyGesture(key));

			target.CommandBindings.Add(commandBinding);
			target.InputBindings.Add(keyBinding);
		}
		catch
		{
			return false;
		}

		return true;
	}
}
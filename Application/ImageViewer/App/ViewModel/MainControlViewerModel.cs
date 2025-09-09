//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.DicomImageViewer;
using NV.CT.ImageViewer.Extensions;
using System;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;

namespace NV.CT.ImageViewer.ViewModel;

public class MainControlViewerModel : BaseViewModel
{
	public const int Width = 1502;
	public const int Height = 1022;

	private readonly ILogger<MainControlViewerModel> _logger;
	private readonly IDialogService _dialogService;
	private readonly IApplicationCommunicationService _applicationCommunicationService;

	public GeneralImageViewer? ImageViewer2D { get; set; }
	public GeneralImageViewer? ImageViewer3D { get; set; }
    public SimpleWebServer? WebServer { get; set; }
    public ScreenshotViewModel? ScreenshotViewModelInstance { get; set; }

    #region 属性
    public ViewScene CurrentView { get; set; }

	private int? _selectedTabIndex = 0;

	public int? SelectedTabIndex
	{
		get => _selectedTabIndex;
		set
		{
			SetProperty(ref _selectedTabIndex, value);
			CurrentView = _selectedTabIndex == 0 ? ViewScene.View2D : ViewScene.View3D;
			_logger?.LogInformation($"view scene changed to {CurrentView}");
            EventAggregator.Instance.GetEvent<ViewSceneChangedEvent>().Publish(_selectedTabIndex.GetValueOrDefault());
            ViewSceneChanged();
        }
}

	private Visibility _2dVisibility = Visibility.Visible;
	public Visibility _2DVisibility
	{
		get => _2dVisibility;
		set => SetProperty(ref _2dVisibility, value);
	}

	private Visibility _3dVisibility = Visibility.Collapsed;
	public Visibility _3DVisibility
	{
		get => _3dVisibility;
		set => SetProperty(ref _3dVisibility, value);
	}
	#endregion


	private void ViewSceneChanged()
	{
		if (CurrentView == ViewScene.View3D)
		{
			_3DVisibility = Visibility.Visible;
			_2DVisibility = Visibility.Collapsed;
		}
		else if (CurrentView == ViewScene.View2D)
		{
			_2DVisibility = Visibility.Visible;
			_3DVisibility = Visibility.Collapsed;
		}
    }

	public MainControlViewerModel(ILogger<MainControlViewerModel> logger, IDialogService dialogService, IApplicationCommunicationService applicationCommunicationService)
	{
		_logger = logger;
		_dialogService = dialogService;
		_applicationCommunicationService = applicationCommunicationService;

		ImageViewer2D = CTS.Global.ServiceProvider.GetService<Image2DViewModel>()?.CurrentImageViewer;
		ImageViewer3D = CTS.Global.ServiceProvider.GetService<Image3DViewModel>()?.CurrentImageViewer;
        WebServer = CTS.Global.ServiceProvider.GetService<SimpleWebServer>();
        ScreenshotViewModelInstance = CTS.Global.ServiceProvider.GetService<ScreenshotViewModel>();

        _applicationCommunicationService.NotifyApplicationClosing += NotifyApplicationClosing;
        WebServer?.Stop();
        WebServer?.Start(ScreenShotPath.ScreenShotDir);
		ScreenshotViewModelInstance?.SafeParallelDelete(ScreenShotPath.ScreenShotDir);
    }

	private void NotifyApplicationClosing(object? sender, ApplicationResponse e)
	{
		if (!(e.ApplicationName == ApplicationParameterNames.APPLICATIONNAME_VIEWER &&
		      Process.GetCurrentProcess().Id == e.ProcessId))
			return;

		if(e.NeedConfirm)
		{
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				_dialogService.ShowDialog(true, MessageLeveles.Info, LanguageResource.Message_Info_CloseConfirmTitle
					, LanguageResource.Message_Info_CloseMessage, arg =>
					{
						if (arg.Result == ButtonResult.OK)
						{
							Process.GetCurrentProcess().Kill();
						}
					}, ConsoleSystemHelper.WindowHwnd);
			});
		}
		else
		{
			Process.GetCurrentProcess().Kill();
		}
	}
}
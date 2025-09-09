//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.Model;
using NV.CT.ImageViewer.View;
using NV.MPS.Configuration;
using System.Drawing;
using System.Threading.Tasks;
using DicomDetail = NV.CT.DicomUtility.DicomImage.DicomDetail;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;

namespace NV.CT.ImageViewer.ViewModel;

public class Image2DViewModel : BaseViewModel
{
	private readonly ILogger<Image2DViewModel>? _logger;
	private readonly IImageAnnotationService _imageAnnotationService;
	private readonly IPrintConfigService _printConfigService; //用于连接Print进程，进行远程联系中心服务操作PrintConfig信息
	private readonly IPrintConfigManager _printConfigManager; //当Print进程未启动时，进行离线操作PrintConfig信息
    public GeneralImageViewer CurrentImageViewer { get; set; }
	private ObservableCollection<WindowingInfo>? _wwwlItems = new();
	private LayoutType2D _layoutType = LayoutType2D.Tile;
	#region 属性

	private Switch2DButtonType _switchButtonItem = Switch2DButtonType.txtWWWL;
	public Switch2DButtonType SwitchButtonItem
	{
		get => _switchButtonItem;
		set => SetProperty(ref _switchButtonItem, value);
	}
	/// <summary>
	/// 是否显示四角信息
	/// </summary>
	private bool _isOverlayTextShown = true;
	public bool IsOverlayTextShown
	{
		get => _isOverlayTextShown;
		set
		{
			SetProperty(ref _isOverlayTextShown, value);
			CurrentImageViewer.ShowOverlay(_isOverlayTextShown);
		}
	}
	private int _fps = 10;

	public int FPS
	{
		get => _fps;
		set => SetProperty(ref _fps, value);
	}

	private bool _isPlayVisible = true;
	public bool IsPlayVisible
	{
		get => _isPlayVisible;
		set => SetProperty(ref _isPlayVisible, value);
	}

	private bool _isPauseVisible;
	public bool IsPauseVisible
	{
		get => _isPauseVisible;
		set => SetProperty(ref _isPauseVisible, value);
	}
	private bool _isSynchronizationEnabled = false;

	public bool IsSynchronizationEnabled
	{
		get => _isSynchronizationEnabled;
		set => SetProperty(ref _isSynchronizationEnabled, value);

	}
	/// <summary>
	/// 同步开关是否开启
	/// </summary>
	private bool _syncEnabled;
	public bool SyncEnabled
	{
		get => _syncEnabled;
		set => SetProperty(ref _syncEnabled, value);
	}

	/// <summary>
	/// 同步序列模式
	/// </summary>
	private SyncSeriesMode _syncSeriesMode = SyncSeriesMode.Auto;
	public SyncSeriesMode SyncSeriesMode
	{
		get => _syncSeriesMode;
		set
		{
			SetProperty(ref _syncSeriesMode, value);
			SetSyncParameter();
		}
	}

	/// <summary>
	/// 同步窗宽窗位
	/// </summary>
	private bool _syncWWWL = true;
	public bool SyncWWWL
	{
		get => _syncWWWL;
		set
		{
			SetProperty(ref _syncWWWL, value);

			SetSyncParameter();
		}
	}

	/// <summary>
	/// 同步camera
	/// </summary>
	private bool _syncPosition = true;
	public bool SyncPosition
	{
		get => _syncPosition;
		set
		{
			SetProperty(ref _syncPosition, value);

			SetSyncParameter();
		}
	}

	public ObservableCollection<WindowingInfo>? WWWLItems
	{
		get => _wwwlItems;
		set => SetProperty(ref _wwwlItems, value);
	}
    private ObservableCollection<DicomDetail> _originalDicomDetailItems = new();
    public ObservableCollection<DicomDetail> OriginalDicomDetailItems
    {
        get => _originalDicomDetailItems;
        set => SetProperty(ref _originalDicomDetailItems, value);
    }

    private ObservableCollection<DicomDetail> _dicomDetailItems = new();
    /// <summary>
    /// Dicom Tag列表弹窗
    /// </summary>
    public ObservableCollection<DicomDetail> DicomDetailItems
    {
        get => _dicomDetailItems;
        set => SetProperty(ref _dicomDetailItems, value);
    }

    private string? _selectedDicomFile = string.Empty;
    public string? SelectedDicomFile
    {
        get => _selectedDicomFile;
        set
        {
            SetProperty(ref _selectedDicomFile, value);
			Task.Run(new Action(() => {
                if (!string.IsNullOrEmpty(value))
                {
                    var list = DicomImageHelper.Instance.GetDicomDetails(value);
                    var pixeldata = list.FirstOrDefault(r => r.TagID == "(7fe0,0010)");
                    if (pixeldata is not null)
                    {
                        list.Remove(pixeldata);
                    }
                    DicomDetailItems = new ObservableCollection<DicomDetail>(list);
                    OriginalDicomDetailItems = DicomDetailItems;
                }
            }));
        }
    }
    private int _seriesimagecount = 0;
	public int SeriesImageCount
	{
		get => _seriesimagecount;
		set => _seriesimagecount = value;
	}

	public Action<int> GetSeriesImageCountEvent;
    #endregion

    public Image2DViewModel(ILogger<Image2DViewModel> logger, IImageAnnotationService imageAnnotationService, IPrintConfigService printConfigService, IPrintConfigManager printConfigManager)
    {
        _logger = logger;
        _imageAnnotationService = imageAnnotationService;
        _printConfigService = printConfigService;
        _printConfigManager = printConfigManager;
        EventAggregator.Instance.GetEvent<SelectedSeriesChangedEvent>().Subscribe(ShowSeriesImage);
        EventAggregator.Instance.GetEvent<StudyChangedEvent>().Subscribe(ClearViewData);
        EventAggregator.Instance.GetEvent<SeletedDataGridSeriesChangedEvent>().Subscribe(ShowDataGridSeries);
        EventAggregator.Instance.GetEvent<MousePositionEvent>().Subscribe(ShowFilmWindow);
        EventAggregator.Instance.GetEvent<UpdateSelectedSeriesModel>().Subscribe(UpdateSelectedSeiresFile);
        EventAggregator.Instance.GetEvent<UpdateSelectedImageModel>().Subscribe(UpdateSelectedImageFile);

        CurrentImageViewer = new GeneralImageViewer(MainControlViewerModel.Width - 20, MainControlViewerModel.Height, false);

        CurrentImageViewer.OnCineFinished += CurrentImageViewer_OnCineFinished;
        CurrentImageViewer.OnAddPrintImageEvent += CurrentImageViewer_OnAddPrintImageFinished;
        CurrentImageViewer.OnROICreateSucceedEvent += CurrentImageViewer_OnROICreateSucceed;
        CurrentImageViewer.TextInputFocusPositionChanged += CurrentImageViewer_TextInputFocusPositionChanged;
        CurrentImageViewer.TextInputDoubleClickChanged += CurrentImageViewer_TextInputDoubleClickChanged;

        Init();
    }

	private void UpdateDicomTag()
	{
		if (_layoutType == LayoutType2D.Stack)
		{
			string path = CurrentImageViewer.GetMultiSeriesDicomFilePath();
			if (!string.IsNullOrEmpty(path))
			{
				SelectedDicomFile = path;
			}
		}
	}

    private void UpdateSelectedImageFile(ImageModel model)
    {
        if (model is null)
            return;
        if (model.IsFile)
        {
            SelectedDicomFile = model.SeriesPath;
        }
        else
        {
            SelectedDicomFile = Directory.EnumerateFiles(model.SeriesPath).FirstOrDefault();
        }
    }

    private void UpdateSelectedSeiresFile(SeriesModel model)
    {
        if (model is null)
            return;
        if (model.SeriesType != SeriesType.DoseReport)
        {
            SelectedDicomFile = Directory.EnumerateFiles(model.SeriesPath).FirstOrDefault();
        }
        else
        {
            SelectedDicomFile = model.SeriesPath;
        }
    }

    private void ClearViewData(bool obj)
    {
		CurrentImageViewer?.ClearView();
    }

    private void CurrentImageViewer_OnAddPrintImageFinished(object? sender, string e)
    {
        var printingImages = JsonConvert.DeserializeObject<List<PrintingImageProperty>>(e);

        var studyViewModel = CTS.Global.ServiceProvider.GetRequiredService<StudyViewModel>();
		var currentStudyId = studyViewModel.SelectedItem.StudyId;

        var seriesViewModel = CTS.Global.ServiceProvider.GetRequiredService<SeriesViewModel>();
		var currentSeriesId = seriesViewModel.ImageModels.FirstOrDefault(s => s.IsSelected)?.SeriesId;
        foreach (var image in printingImages)
        {
            image.SeriesUID = currentSeriesId;
        }

		Task.Run( () => { SendAppendingImages(currentStudyId, printingImages); });
    }

	public void SendAppendingImages(string currentStudyId, List<PrintingImageProperty> printingImages)
	{
        //如果Print进程已启动,远程联系中心服务操作PrintConfig信息
        if (Global.Instance.CheckConnectivityOfPrintConfig())
        {
            this._logger?.LogDebug("Success to connect to PrintConfigServce from ImageViewer.");

            //由于Print服务可能更晚才启动，所以每次均手动尝试订阅PrintConfig服务的事件
            Global.Instance.SubscribePrintConfigService();
            _printConfigService.AcceptAppendingImages(currentStudyId, printingImages);
        }
        else //如果Print进程尚未启动,进行离线操作PrintConfig信息
        {
            this._logger?.LogDebug("Failed to connect to PrintConfigServce from ImageViewer.");
            var isSuccessful = _printConfigManager.AppendImagesToPrint(currentStudyId, printingImages);
            if (isSuccessful)
            {
                _printConfigManager.Save(currentStudyId);
            }
        }
    }

    private void Init2DButtonState()
	{
		IsOverlayTextShown = true;
		CurrentImageViewer.InitInvert();
        ConstName2D.StateButtonDictionary[ConstName2D.tbFlipVertical] = true;
        ConstName2D.StateButtonDictionary[ConstName2D.tbFlipHorizontal] = true;
        ConstName2D.StateButtonDictionary[ConstName2D.tbInvert] = true;
        ConstName2D.StateButtonDictionary[ConstName2D.tbHideTexts] = true;
        ConstName2D.StateButtonDictionary[ConstName2D.tbReverse] = true;
	}
    /// <summary>
    /// 显示到文本变更窗口里面
    /// </summary>
    private void CurrentImageViewer_TextInputDoubleClickChanged(object? sender, string e)
	{
		EventAggregator.Instance.GetEvent<TextInputEvent>().Publish(new TextInputData()
		{
			Action = TextInputAction.ChangeText,
			Text = e
		});

		CommonMethod.ShowCustomWindow(typeof(TextInputWindow));
	}

	/// <summary>
	/// 文本输入 获取焦点之后，显示输入框窗口
	/// </summary>
	private void CurrentImageViewer_TextInputFocusPositionChanged(object? sender, System.Drawing.Point e)
	{
		var targetWindow = CTS.Global.ServiceProvider?.GetRequiredService<TextInputWindow>();

		if (targetWindow is null)
			return;

		EventAggregator.Instance.GetEvent<TextInputEvent>().Publish(new TextInputData()
		{
			Action = TextInputAction.InputText
		});

        CommonMethod.ShowCustomWindow(typeof(TextInputWindow));
	}

	private void Init()
	{
		_logger?.LogInformation($"2D control init");

		var windowTypes = UserConfig.WindowingConfig.Windowings;
		if (windowTypes is null)
			return;
		//添加自定义ww/wl
		windowTypes.Add(new WindowingInfo() {
			Width = new ItemField<int> { Value = 350, Default = 350 },
			Level = new ItemField<int> { Value = 20, Default = 20 },
			BodyPart = "Custom",
			Shortcut = "F12",
			Description ="Custom"
		});
		foreach (var windowType in windowTypes)
		{
			windowType.Description = $"{windowType.BodyPart} ({windowType.Shortcut})";
        }
		WWWLItems = windowTypes.ToObservableCollection();

		InitFourCornerInformation();

		//_Screenshot.ReturnScreenShotEvent += GetScreen;

		//初始化默认 Flow 2x2 布局
		CurrentImageViewer.Layout(ViewLayout.Browser2x2);

		Commands.Add("DicomTagSearchCommand", new DelegateCommand<string?>(DicomTagSearchCommand));

		ViewCommands();
		MarkCommands();
		FilmCommands();

		//SetDefaultSwitchButton(SwitchButtonItem);

    }

	private void DicomTagSearchCommand(string? searchKeyword)
	{
		if (string.IsNullOrEmpty(searchKeyword))
		{
			DicomDetailItems = OriginalDicomDetailItems;
			return;
		}

		var lowerSearchKeyword = searchKeyword.ToLower().Trim();
		DicomDetailItems = OriginalDicomDetailItems
			.Where(n =>
				n.TagID.Contains(searchKeyword) ||
				n.TagVR.Contains(searchKeyword) ||
				n.TagVM.Contains(searchKeyword) ||
				n.TagDescription.ToLower().Contains(lowerSearchKeyword) ||
				n.TagValue.ToLower().Contains(lowerSearchKeyword)).ToList().ToObservableCollection();
	}

	private void GetScreen(BitmapSource bitmap)
	{
		System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
		{
			var randomName = $"{DateTime.Now:yyyy-MM-dd_HHmmss}.png";
			bitmap?.SaveToFile($"D:\\{randomName}");
		});
	}

	public void ShowSeriesImage(ImageModel imageModel)
	{
		if (imageModel is null)
			return;
		if (CTS.Global.ServiceProvider.GetRequiredService<MainControlViewerModel>().CurrentView != ViewScene.View2D)
			return;
		if (_layoutType!= LayoutType2D.Stack)
		{
            CurrentImageViewer?.ClearView();
        }
        if (imageModel.IsFile)
		{
			CurrentImageViewer?.LoadImageWithFilePath(imageModel.SeriesPath);
            SelectedDicomFile = imageModel.SeriesPath;
		}
		else
		{
			CurrentImageViewer?.LoadImageWithDirectoryPath(imageModel.SeriesPath);
            SelectedDicomFile = Directory.EnumerateFiles(imageModel.SeriesPath).FirstOrDefault();
			SeriesImageCount = Directory.GetFiles(imageModel.SeriesPath,"*.dcm").Count();
			GetSeriesImageCountEvent.Invoke(SeriesImageCount);
        }
    }
    public void ShowDataGridSeries(SeriesModel seriesModel)
    {
        if (seriesModel is null)
            return;
        if (CTS.Global.ServiceProvider.GetRequiredService<MainControlViewerModel>().CurrentView != ViewScene.View2D)
            return;
        if (_layoutType != LayoutType2D.Stack)
		{
            CurrentImageViewer?.ClearView();
        }
        if (seriesModel.SeriesType != SeriesType.DoseReport)
		{
            CurrentImageViewer?.LoadImageWithDirectoryPath(seriesModel.SeriesPath);
            SelectedDicomFile = Directory.EnumerateFiles(seriesModel.SeriesPath).FirstOrDefault();
            SeriesImageCount = Directory.GetFiles(seriesModel.SeriesPath, "*.dcm").Count();
            GetSeriesImageCountEvent.Invoke(SeriesImageCount);
        }
        else
		{
            CurrentImageViewer?.LoadImageWithFilePath(seriesModel.SeriesPath);
            SelectedDicomFile = seriesModel.SeriesPath;
        }
	}

    public void InitFourCornerInformation()
	{
		var (topoTextStyle, topoTexts) = ImageSettingToOverlayText.Get(_imageAnnotationService.GetConfigs().ViewSettings);
		if (topoTexts.Count > 0)
		{
			CurrentImageViewer?.ShowCursorRelativeValue(true);
			CurrentImageViewer?.SetFourCornersMessage(topoTextStyle, topoTexts);
		}
	}

	private void ViewCommands()
	{
		Commands.Add(CommandName.Layout, new DelegateCommand<string?>(layoutType =>
		{
			_layoutType = layoutType?[0].ToString() == "S" ? LayoutType2D.Stack : LayoutType2D.Tile;

			if (_layoutType == LayoutType2D.Stack)
			{
				CurrentImageViewer.SetImageBrowserMode(BrowseMode.OverlayMode);
                IsSynchronizationEnabled = true;
            }
			else if (_layoutType == LayoutType2D.Tile)
			{
				CurrentImageViewer.SetImageBrowserMode(BrowseMode.TileMode);
                IsSynchronizationEnabled = false;
				SyncEnabled = false;
            }
            EventAggregator.Instance.GetEvent<UpdateUnStateButtonEvent>().Publish(ConstName2D.txtLayout);
            var realLayoutParameter = layoutType?.Substring(1);
			switch (realLayoutParameter)
			{
				case "1x1":
					CurrentImageViewer.Layout(ViewLayout.Browser1x1);
					break;
				case "1x2":
					CurrentImageViewer.Layout(ViewLayout.Browser1x2);
					break;
				case "2x2":
					CurrentImageViewer.Layout(ViewLayout.Browser2x2);
					break;
				case "3x3":
					CurrentImageViewer.Layout(ViewLayout.Browser3x3);
					break;
				case "4x2":
					CurrentImageViewer.SetCustomLayout2D(4,2);
					break;
				case "4x3":
					CurrentImageViewer.Layout(ViewLayout.Browser4x3);
					break;
                case "Custom":
                    CommonMethod.ShowCustomWindow(typeof(CustomLayoutWindow));
                    break;
                default:
					CurrentImageViewer.Layout(ViewLayout.Browser2x2);
					break;
			}
		}, _ => true));
		Commands.Add(CommandName.Move, new DelegateCommand(() => 
		{
			SetDefaultSwitchButton(Switch2DButtonType.txtMove);

        }));
		Commands.Add(CommandName.Zoom, new DelegateCommand(() => 
		{
            SetDefaultSwitchButton(Switch2DButtonType.tbZoom);

        }));
		Commands.Add(CommandName.RotateMode, new DelegateCommand(() => 
		{
            SetDefaultSwitchButton(Switch2DButtonType.txtRotate);
        }));
		Commands.Add(CommandName.Scroll, new DelegateCommand(() => 
		{
            SetDefaultSwitchButton(Switch2DButtonType.tbScroll);
        }));
		Commands.Add(CommandName.Wwwl, new DelegateCommand<string?>(_ =>
		{
            SetDefaultSwitchButton(Switch2DButtonType.txtWWWL);
        }));
		Commands.Add(CommandName.Rotate, new DelegateCommand<int?>(degree =>
		{
			if (degree is not null)
			{
				if (int.TryParse(degree.ToString(), out int validDegree))
				{
					CurrentImageViewer.Rotate(validDegree);
				}
			}
			else
			{
				//自定义角度
				CommonMethod.ShowCustomWindow(typeof(CustomRotateDegreeWindow));
			}
            SetDefaultSwitchButton(Switch2DButtonType.txtRotate);
        }));
		Commands.Add(CommandName.InitialState, new DelegateCommand(() =>
		{
			CurrentImageViewer.Reset();
			Init2DButtonState();
            //InitFourCornerInformation();
            EventAggregator.Instance.GetEvent<Update2DPreviousStateButtonEvent>().Publish(true);
        }));
		Commands.Add(CommandName.FlipHorizontal, new DelegateCommand(() =>{UpdateStateButton(StateButtonType2D.tbFlipHorizontal);}));
		Commands.Add(CommandName.FlipVertical, new DelegateCommand(() => {UpdateStateButton(StateButtonType2D.tbFlipVertical);}));
		Commands.Add(CommandName.HiddenText, new DelegateCommand(() =>{UpdateStateButton(StateButtonType2D.tbHideTexts);}));
		//Screen shot [没好]
		Commands.Add(CommandName.Screenshot, new DelegateCommand(ScreenShot2D));
		Commands.Add(CommandName.ReverseSequence, new DelegateCommand(()=>{ UpdateStateButton(StateButtonType2D.tbReverse);}));
		Commands.Add(CommandName.Invert, new DelegateCommand(() => { UpdateStateButton(StateButtonType2D.tbInvert);}));
		Commands.Add(CommandName.Kernel, new DelegateCommand<string?>(kernelType =>
		{
			if (!string.IsNullOrEmpty(kernelType))
			{
				CurrentImageViewer.Kernel(kernelType);
                EventAggregator.Instance.GetEvent<UpdateUnStateButtonEvent>().Publish(ConstName2D.txtKernel);
            }
		}));
		Commands.Add(CommandName.Dicomtag, new DelegateCommand(() => 
		{
			UpdateDicomTag();
            CommonMethod.ShowCustomWindow(typeof(DicomTagWindow)); 
		}));

		Commands.Add(CommandName.Synchronization, new DelegateCommand(SyncMultiSeries));
	}
	private void UpdateStateButton(StateButtonType2D stateButtonType)
	{
        StateButton stateButton = new StateButton();
		switch (stateButtonType)
		{
			case StateButtonType2D.tbFlipVertical:
                ConstName2D.StateButtonDictionary[ConstName2D.tbFlipVertical] = !ConstName2D.StateButtonDictionary[ConstName2D.tbFlipVertical];
                CurrentImageViewer.FlipVertical();
                stateButton.ButtonName = ConstName2D.tbFlipVertical;
                stateButton.ButtonState = ConstName2D.StateButtonDictionary[ConstName2D.tbFlipVertical];
                break;
			case StateButtonType2D.tbFlipHorizontal:
                ConstName2D.StateButtonDictionary[ConstName2D.tbFlipHorizontal] = !ConstName2D.StateButtonDictionary[ConstName2D.tbFlipHorizontal];
                CurrentImageViewer.FlipHorizontal();
                stateButton.ButtonName = ConstName2D.tbFlipHorizontal;
                stateButton.ButtonState = ConstName2D.StateButtonDictionary[ConstName2D.tbFlipHorizontal];
                break;
			case StateButtonType2D.tbInvert:
                ConstName2D.StateButtonDictionary[ConstName2D.tbInvert] = !ConstName2D.StateButtonDictionary[ConstName2D.tbInvert];
                CurrentImageViewer.Invert();
                stateButton.ButtonName = ConstName2D.tbInvert;
                stateButton.ButtonState = ConstName2D.StateButtonDictionary[ConstName2D.tbInvert];
                break;
			case StateButtonType2D.tbHideTexts:
                IsOverlayTextShown = !IsOverlayTextShown;
                ConstName2D.StateButtonDictionary[ConstName2D.tbHideTexts] = !ConstName2D.StateButtonDictionary[ConstName2D.tbHideTexts];
                stateButton.ButtonName = ConstName2D.tbHideTexts;
                stateButton.ButtonState = ConstName2D.StateButtonDictionary[ConstName2D.tbHideTexts];
                break;
			case StateButtonType2D.tbReverse:
                ConstName2D.StateButtonDictionary[ConstName2D.tbReverse] = !ConstName2D.StateButtonDictionary[ConstName2D.tbReverse];
                CurrentImageViewer.ReverseSequence();
                stateButton.ButtonName = ConstName2D.tbReverse;
                stateButton.ButtonState = ConstName2D.StateButtonDictionary[ConstName2D.tbReverse];
                break;
			default:
				break; 
		}
		EventAggregator.Instance.GetEvent<Update2DStateButtonEvent>().Publish(stateButton);
    }
	private void SyncMultiSeries()
	{
		SyncEnabled = !SyncEnabled;
		CurrentImageViewer.SetSyncBinding(SyncEnabled);
		SetSyncParameter();
	}

	private void SetSyncParameter()
	{
		var syncParameter = (int)SyncSeriesMode
							| (SyncWWWL ? (int)SynchronousMode.SynchronousWwwl : 0)
							| (SyncPosition ? (int)SynchronousMode.SynchronousCamera : 0);
		CurrentImageViewer.SetSyncParameters(syncParameter);
	}

	private void MarkCommands()
	{
		Commands.Add(CommandName.Create_Length_ROI, new DelegateCommand(() => SetDefaultSwitchButton(Switch2DButtonType.tb_Measure)));
		Commands.Add(CommandName.Create_Angle_ROI, new DelegateCommand(() => SetDefaultSwitchButton(Switch2DButtonType.tb_Angle)));
		Commands.Add(CommandName.Create_Circle_ROI, new DelegateCommand(() => SetDefaultSwitchButton(Switch2DButtonType.tb_CircleROI)));
		Commands.Add(CommandName.Create_Freehand_ROI, new DelegateCommand(() => SetDefaultSwitchButton(Switch2DButtonType.tb_FreehandROI)));
		Commands.Add(CommandName.Create_Rect_ROI, new DelegateCommand(() => SetDefaultSwitchButton(Switch2DButtonType.tb_RectangleROI)));

		//签名 [没好]
		Commands.Add(CommandName.Create_Sign_ROI, new DelegateCommand(() => CurrentImageViewer.CreateROI(ROIType.ROI_Point)));

		Commands.Add(CommandName.Create_Arrow_ROI, new DelegateCommand(() => SetDefaultSwitchButton(Switch2DButtonType.tb_Arrow)));

		Commands.Add(CommandName.Create_Text_ROI, new DelegateCommand(() => CurrentImageViewer.CreateROI(ROIType.ROI_Text)));
		//Grid [没好]
		Commands.Add(CommandName.Create_Grid_ROI, new DelegateCommand<int?>(width => CurrentImageViewer.CreateGridRoi(width)));
		Commands.Add(CommandName.Remove_All_ROI2D, new DelegateCommand(() => 
		{
			CurrentImageViewer.RemoveAllROI2D();
        } ));
	}
	private void CurrentImageViewer_OnCineFinished(object? sender, int e)
	{
        //when cinema play finished
        //IsPlayVisible = true;
        //IsPauseVisible = false;

        IsPlayVisible = false;
        IsPauseVisible = true;
    }
    private void CurrentImageViewer_OnROICreateSucceed(object? sender, int e)
	{
		//if (e != CurrentImageViewer.ViewHandle)
		//{
		//	return;
		//}
		//EventAggregator.Instance.GetEvent<Update2DPreviousStateButtonEvent>().Publish(true);
		//SetDefaultSwitchButton(SwitchButtonItem);
	}
	private void SetDefaultSwitchButton(Switch2DButtonType switchButtonType)
	{
		switch (switchButtonType)
		{
			case Switch2DButtonType.txtMove:
				CurrentImageViewer.Move();
                break;
			case Switch2DButtonType.tbZoom:
                CurrentImageViewer.Zoom();
                break;
			case Switch2DButtonType.txtRotate:
                CurrentImageViewer.Rotate();
                break;
			case Switch2DButtonType.tbScroll:
                CurrentImageViewer.Scroll();
                break;
			case Switch2DButtonType.txtWWWL:
                CurrentImageViewer.SetWWWL();
                break;
            case Switch2DButtonType.tb_Measure:
                CurrentImageViewer.CreateROI(ROIType.ROI_Length);
                break;
            case Switch2DButtonType.tb_RectangleROI:
                CurrentImageViewer.CreateROI(ROIType.ROI_Rect);
                break;
            case Switch2DButtonType.tb_FreehandROI:
                CurrentImageViewer.CreateROI(ROIType.ROI_PolygonClosed);
                break;
            case Switch2DButtonType.tb_CircleROI:
                CurrentImageViewer.CreateROI(ROIType.ROI_Circle);
                break;
            case Switch2DButtonType.tb_Arrow:
                CurrentImageViewer.CreateROI(ROIType.ROI_Arrow);
                break;
            case Switch2DButtonType.tb_Angle:
                CurrentImageViewer.CreateROI(ROIType.ROI_Angle);
                break;
            //default:
            //    CurrentImageViewer.SetWWWL();
            //    break;
		}
        EventAggregator.Instance.GetEvent<Update2DSwitchButtonEvent>().Publish(switchButtonType.GetDisplayName());
    }
    private void FilmCommands()
	{
		Commands.Add(CommandName.AdjustFrameRate, new DelegateCommand<int?>(deltaRate =>
		{
			if (deltaRate is not null)
			{
				FPS += (int)deltaRate;

				if (FPS >= 100)
				{
					FPS = 100;
				}
				else if (FPS <= 1)
				{
					FPS = 1;
				}

				CurrentImageViewer.AdjustFrameRateCine(FPS);
			}
		}));

		Commands.Add(CommandName.ShowFilm, new DelegateCommand(() =>
		{
			CurrentImageViewer.CineStart(FPS);
			//ShowFilmWindow();
		}));

		Commands.Add(CommandName.Play, new DelegateCommand(() =>
		{
			CurrentImageViewer.CinePauseResume();
            CurrentImageViewer.CineCyclePlay();
            IsPlayVisible = false;
			IsPauseVisible = true;
		}));
		Commands.Add(CommandName.Pause, new DelegateCommand(() =>
		{
			CurrentImageViewer.CinePause();
			IsPlayVisible = true;
			IsPauseVisible = false;
		}));
		Commands.Add(CommandName.PrevFrame, new DelegateCommand(() => CurrentImageViewer.CineBackward()));
		Commands.Add(CommandName.NextFrame, new DelegateCommand(() => CurrentImageViewer.CineForward()));
        Commands.Add(CommandName.BeginFrame, new DelegateCommand(() => CurrentImageViewer.MoveToPriorSlice()));
        Commands.Add(CommandName.EndFrame, new DelegateCommand(() => CurrentImageViewer.MoveToNextSlice()));
    }

	private void ScreenShot2D()
	{      
        CTS.Global.ServiceProvider.GetService<ScreenshotViewModel>()?.ScreeShot(DicomDetailItems?.ToList(),CurrentImageViewer);
    }
    public void ShowFilmWindow(FollowingPoints followingPoints)
	{
		var filmWindow = CTS.Global.ServiceProvider?.GetRequiredService<FilmWindow>();

		if (filmWindow is null)
			return;
        filmWindow.Top = followingPoints.ScreenPoint.Y- followingPoints.CinePoint.Y+35;
        filmWindow.Left = followingPoints.ScreenPoint.X;
        _logger?.LogInformation($" final top:{filmWindow.Top},left:{filmWindow.Left}");

		//publish event
		EventAggregator.Instance.GetEvent<FilmPlayChangedEvent>().Publish(true);

		var wih = new WindowInteropHelper(filmWindow);
		if (ConsoleSystemHelper.WindowHwnd != IntPtr.Zero)
		{
			if (wih.Owner == IntPtr.Zero)
			{
				wih.Owner = ConsoleSystemHelper.WindowHwnd;
			}
			if (!filmWindow.IsVisible)
			{
				//隐藏底部状态栏
				filmWindow.Topmost = true;
				filmWindow.Show();
			}
		}
		else
		{
			if (System.Windows.Application.Current.MainWindow is not null && wih.Owner == IntPtr.Zero)
			{
				wih.Owner = new WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle;
			}
			if (!filmWindow.IsVisible)
			{
				filmWindow.Show();
				filmWindow.Activate();
			}
		}

	}

   
}
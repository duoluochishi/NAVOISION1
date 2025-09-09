using NV.MPS.UI.Dialog.Service;

namespace NV.CT.RGT.ViewModel;

public class ScanControlsViewModel : BaseViewModel
{
	private bool examButtonStatus;
	private readonly IScreenSync _screenSync;
	private readonly IDataSync _dataSync;
	private readonly ILayoutManager _layoutManager;
	private readonly IDialogService _dialogService;
	private readonly ISelectionManager _selectionManager;
	private readonly ILogger<ScanControlsViewModel> _logger;
	private (DatabaseService.Contract.Models.StudyModel, DatabaseService.Contract.Models.PatientModel)? SelectionStudy;
	public ScanControlsViewModel(ILogger<ScanControlsViewModel> logger, IDialogService dialogService, IScreenSync screenSync, IDataSync dataSync, ILayoutManager layoutManager,ISelectionManager selectionManager)
	{
		_logger = logger;
		_screenSync = screenSync;
		_dataSync = dataSync;
		_dialogService = dialogService;
		_layoutManager = layoutManager;
		_selectionManager = selectionManager;
		_selectionManager.SelectionChanged += SelectionManager_SelectionChanged;

		_layoutManager.LayoutChanged += LayoutManager_LayoutChanged;

		//_dataSync.SelectionScanChanged += DataSync_SelectionScanChanged;
		//_dataSync.ReplaceProtocolCompleted += DataSync_ReplaceProtocolCompleted;
		//_dataSync.ExamStarted += DataSync_ExamStarted;
		//_dataSync.ExamClosed += DataSync_ExamClosed;

		_dataSync.ExamCloseFinished += DataSync_ExamCloseFinished;
		_screenSync.ScreenChanged += ScreenSync_ScreenChanged;

		Commands.Add("Go", new DelegateCommand(Go, () => true));
		Commands.Add("Back", new DelegateCommand(Back, () => true));
		Commands.Add("EmergencyCommand", new DelegateCommand(EmergencyCommand, () => true));

		IsEnable = true;
	}

	/// <summary>
	/// 检查完成
	/// </summary>
	private void DataSync_ExamCloseFinished(object? sender, EventArgs e)
	{
		_layoutManager.SwitchToView(SyncScreens.PatientBrowser);
	}

	/// <summary>
	/// 同步界面变化SyncScreen
	/// </summary>
	private void ScreenSync_ScreenChanged(object? sender, string e)
	{
		_logger.LogInformation($"ScreenSync_ScreenChanged to {e}");

		var navigatedTo = Enum.Parse<SyncScreens>(e);
		if (_layoutManager.CurrentLayout != navigatedTo)
		{
			_layoutManager.SwitchToView(navigatedTo);
		}
	}

	/// <summary>
	/// 急诊
	/// </summary>
	private void EmergencyCommand()
	{
		_dataSync.EmergencyExam();
	}

	private void LayoutManager_LayoutChanged(object? sender, EventArgs<SyncScreens> e)
	{
		//var currentLayout = e.Data;

		if (_layoutManager.CurrentLayout == SyncScreens.ScanDefault)
		{
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				IsEnable = false;
				_logger.LogInformation($"ScanControl: currentLayout {_layoutManager.CurrentLayout} , IsEnable=false");
			});
		}
		else
		{
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				IsEnable = true;

				_logger.LogInformation($"ScanControl: currentLayout {_layoutManager.CurrentLayout} , IsEnable=true");
			});
		}
	}

	private void SelectionManager_SelectionChanged(object? sender, EventArgs<(DatabaseService.Contract.Models.StudyModel, DatabaseService.Contract.Models.PatientModel)> e)
	{
		SelectionStudy = e.Data;
		ValidateGo();
	}

	private void ValidateGo()
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			IsEnable = SelectionStudy is not null;
		});
	}

	private void Go()
	{
		ValidateGo();

		var currentLayout = _layoutManager.CurrentLayout;
		if (currentLayout == SyncScreens.PatientBrowser)
		{
			PatientBrowser_Go();
		}
		else if (currentLayout == SyncScreens.ProtocolSelection)
		{
			ProtocolSelection_Go();
		}

		IsEnable = true;
	}


	/// <summary>
	/// 病人列表页面的 GO 逻辑
	/// </summary>
	private void PatientBrowser_Go()
	{
		if (SelectionStudy is null)
			return;

		if (examButtonStatus)
			return;

		try
		{
			examButtonStatus = true;
			_logger.LogInformation("GoToExam");

			_dataSync.NormalExam();
		}
		finally
		{
			examButtonStatus = false;
		}
	}

	private void ProtocolSelection_Go()
	{
		var vm = CTS.Global.ServiceProvider.GetService<ProtocolSelectMainViewModel>();
		if (vm is null || vm.SelectedProtocol is null)
		{
			return;
		}

		var selectedProtocolId = vm.SelectedProtocol.Id;
		if (string.IsNullOrEmpty(selectedProtocolId))
			return;

		_dataSync.ReplaceProtocol(selectedProtocolId);
	}


	private void Back()
	{
		_layoutManager.Back();
	}

	/// <summary>
	/// 初始可用
	/// </summary>
	private bool _isEnable;
	public bool IsEnable
	{
		get => _isEnable;
		set => SetProperty(ref _isEnable, value);
	}
}
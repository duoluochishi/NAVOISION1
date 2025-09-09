namespace NV.CT.Recon.Model;

public class ScanReconModel : BaseViewModel
{
	private string _scanID = string.Empty;
	public string ScanID
	{
		get => _scanID;
		set => SetProperty(ref _scanID, value);
	}

	private string _id = string.Empty;
	public string ID
	{
		get => _id;
		set => SetProperty(ref _id, value);
	}

	private int _seriesNumber;
	public int SeriesNumber
	{
		get => _seriesNumber;
		set => SetProperty(ref _seriesNumber, value);
	}

	private string _reconTaskID = string.Empty;
	public string ReconTaskID
	{
		get => _reconTaskID;
		set => SetProperty(ref _reconTaskID, value);
	}

	private string _seriesInStanceUID = string.Empty;
	public string SeriesInStanceUID
	{
		get => _seriesInStanceUID;
		set => SetProperty(ref _seriesInStanceUID, value);
	}

	private string _content = string.Empty;
	public string Content
	{
		get => _content;
		set => SetProperty(ref _content, value);
	}

	private WriteableBitmap? _image;
	public WriteableBitmap? Image
	{
		get => _image;
		set => SetProperty(ref _image, value);
	}

	private bool _isSelected = false;
	public bool IsSelected
	{
		get => _isSelected;
		set => SetProperty(ref _isSelected, value);
	}

	public bool IsRTD { get; set; }

	private string _imagePath = string.Empty;
	public string ImagePath
	{
		get => _imagePath;
		set => SetProperty(ref _imagePath, value);
	}

	private PerformStatus _performStatus;
	public PerformStatus PerformStatus
	{
		get=>_performStatus;
		set=>SetProperty(ref _performStatus, value);
	}

	private bool _isTomo;
	/// <summary>
	/// 是否Tomo，用于界面绑定非Tomo的时候右键菜单是否可用
	/// </summary>
	public bool IsTomo
	{
		get => _isTomo;
		set => SetProperty(ref _isTomo, value);
	}

	private bool _isReconProcessing;
	/// <summary>
	/// 表明当前Recon是在进行中还是 （已经完成，或者未开始阶段）
	/// </summary>
	public bool IsReconProcessing
	{
		get => _isReconProcessing;
		set => SetProperty(ref _isReconProcessing, value);
	}

	private bool _isCutEnable;
	public bool IsCutEnable
	{
		get => _isCutEnable;
		set => SetProperty(ref _isCutEnable, value);
	}
	
	private string? _toolTipMessage = null;
	public string? ToolTipMessage
	{
		get => _toolTipMessage;
		set => SetProperty(ref _toolTipMessage, value);
	}

	private ReconModel _reconModel = new();
	public ReconModel ReconModel
	{
		get => _reconModel;
		set => SetProperty(ref _reconModel, value);
	}

	public ReconGroupType ReconGroupType { get; set; }

	public ScanReconModel()
	{
		Commands.Add(CommandName.ReconOnCommand, new DelegateCommand(() =>
			SendCommand(CommandName.ReconOnCommand), IsCanRecon));

		Commands.Add(CommandName.ReconCancelCommand, new DelegateCommand(() =>
			SendCommand(CommandName.ReconCancelCommand), IsCanCancel));

		Commands.Add(CommandName.ReconRepeatCommand, new DelegateCommand(() =>
		SendCommand(CommandName.ReconRepeatCommand), IsCanRepeat));

		Commands.Add(CommandName.ReconCutCommand, new DelegateCommand(() =>
			SendCommand(CommandName.ReconCutCommand), IsCanCut));

		Commands.Add(CommandName.ShowParameterCommand, new DelegateCommand(() =>
			SendCommand(CommandName.ShowParameterCommand)));

		Commands.Add(CommandName.BrowserCommand, new DelegateCommand(() =>SendCommand(CommandName.BrowserCommand), IsCanBrowser));

		Commands.Add(CommandName.BrowserRawDataCommand, new DelegateCommand(() => SendCommand(CommandName.BrowserRawDataCommand),IsCanBrowserRawData));
	}

	private void SendCommand(string commandName)
	{
		EventAggregator.Instance.GetEvent<CommandNameChangedEvent>()
			.Publish(new CommandModel()
			{
				CommandName = commandName,
				CommandData = this
			});
	}

	private bool IsCanCut()
	{
		//已扫描完成的 不允许cut
		if (ReconModel.Status == PerformStatus.Performed)
			return false;

		return (IsCutEnable || ReconModel.Status == PerformStatus.Unperform);
	}

	private bool IsCanCancel()
	{
		//return IsReconProcessing;
		return ReconModel.Status == PerformStatus.Performing || ReconModel.Status == PerformStatus.Waiting;
	}

	private bool IsCanRepeat()
	{
		//先暂时配合算法不加限制个数
		// return ScanReconModelList.Count < 8 && _selectionManager.CurrentSelection.Scan?.ScanOption != ScanOption.SURVIEW;
		//return _selectionManager.CurrentSelection.Scan?.ScanOption != ScanOption.SURVIEW;

		return true;
	}

	private bool IsCanRecon()
	{
		//rtd肯定已经完成了
		//var rtdReconModel = ScanReconModelList.FirstOrDefault(n => n.IsRTD);
		//能不能recon必须 是未扫描的recon ，并且RTD已经完成
		return ReconModel.Status == PerformStatus.Unperform && !IsReconProcessing;
		// && rtdReconModel?.ReconModel.Status == PerformStatus.Performed;
	}

	private bool IsCanBrowser()
	{
		return ReconModel.Status == PerformStatus.Performed && !string.IsNullOrEmpty(ReconModel.ImagePath);
	}

	private bool IsCanBrowserRawData()
	{
		return true;
	}
}

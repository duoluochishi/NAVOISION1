//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract;
using NV.CT.ErrorCodes;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.UI.Exam.Extensions;
using NV.MPS.Environment;
using System.Text;
using TubePosition = NV.CT.FacadeProxy.Common.Enums.TubePosition;

namespace NV.CT.UI.Exam.ViewModel;

public class ScanReconViewModel : BaseViewModel
{
	private readonly ILogger<ScanReconViewModel> _logger;
	private readonly ISelectionManager _selectionManager;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IOfflineReconService _offlineReconService;
	private readonly IStudyHostService _studyHostService;
	private readonly IRTDReconService _rtdReconService;
	private readonly IDialogService _dialogService;
	private readonly IImageOperationService _imageOperationService;
	private readonly IOfflineConnectionService _offlineConnectionService;
	private readonly IRawDataService _rawDataService;

	private ObservableCollection<ScanReconModel> _scanReconModelList = new();
	public ObservableCollection<ScanReconModel> ScanReconModelList
	{
		get => _scanReconModelList;
		set => SetProperty(ref _scanReconModelList, value);
	}

	private ScanReconModel _selectScanReconModel = new();
	public ScanReconModel SelectScanReconModel
	{
		get => _selectScanReconModel;
		set
		{
			SetProperty(ref _selectScanReconModel, value);

			FlushedCommandStatus();
		}
	}

	private ScanModel currentScanModel;

	private WriteableBitmap RTDImage
	{
		get => new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/reconrt.png", UriKind.RelativeOrAbsolute)));
	}
	private WriteableBitmap WaitingImage
	{
		get => new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/reconwaiting.png", UriKind.RelativeOrAbsolute)));
	}
	private WriteableBitmap FailedImage
	{
		get => new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/reconfailed.png", UriKind.RelativeOrAbsolute)));
	}

	private WriteableBitmap PerformingImage
	{
		get => new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/reconing.png", UriKind.RelativeOrAbsolute)));
	}
	public ScanReconViewModel(
		ILogger<ScanReconViewModel> logger,
		ISelectionManager scanSelectManager,
		IProtocolHostService protocolHostService,
		IOfflineReconService offlineReconService,
		IStudyHostService studyHostService,
		IRTDReconService rtdReconService,
		IImageOperationService imageOperationService,
		IDialogService dialogService,
		IOfflineConnectionService offlineConnectionService,
		IRawDataService rawDataService)
	{
		_logger = logger;
		_selectionManager = scanSelectManager;
		_imageOperationService = imageOperationService;
		_protocolHostService = protocolHostService;
		_offlineReconService = offlineReconService;
		_studyHostService = studyHostService;
		_rtdReconService = rtdReconService;
		_dialogService = dialogService;
		_offlineConnectionService = offlineConnectionService;
		_rawDataService = rawDataService;
		_dialogService = dialogService;

		Commands.Add(CommandParameters.COMMAND_RECON_ON, new DelegateCommand(ReconOn, IsCanRecon));
		Commands.Add(CommandParameters.COMMAND_RECON_CUT, new DelegateCommand(ReconCut, IsCanCut));
		Commands.Add(CommandParameters.COMMAND_RECON_REPEAT, new DelegateCommand(ReconRepeat, IsCanRepeat));
		Commands.Add(CommandParameters.COMMAND_RECON_CANCEL, new DelegateCommand(ReconCancel, IsCanCancel));
		Commands.Add(CommandParameters.COMMAND_RECON_ITEMCLICK, new DelegateCommand(ReconItemClicked));

		_selectionManager.SelectionScanChanged -= SelectionManager_SelectionScanChanged;
		_selectionManager.SelectionScanChanged += SelectionManager_SelectionScanChanged;
		_selectionManager.SelectionReconChanged -= SelectionManager_SelectionReconChanged;
		_selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;
		_protocolHostService.PerformStatusChanged -= ProtocolPerformStatusService_PerformStatusChanged;
		_protocolHostService.PerformStatusChanged += ProtocolPerformStatusService_PerformStatusChanged;
		_protocolHostService.StructureChanged -= ProtocolHostService_StructureChanged;
		_protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;
		_protocolHostService.ParameterChanged -= ProtocolHostService_ParameterChanged;
		_protocolHostService.ParameterChanged += ProtocolHostService_ParameterChanged;
		_rtdReconService.ReconDone -= RTDReconService_ReconReconDone;
		_rtdReconService.ReconDone += RTDReconService_ReconReconDone;
		_imageOperationService.SwitchViewsChanged -= ImageOperationService_SwitchViewsChanged;
		_imageOperationService.SwitchViewsChanged += ImageOperationService_SwitchViewsChanged;
		_imageOperationService.OnSelectionReconIDChanged -= ImageOperationService_OnSelectionReconIDChanged;
		_imageOperationService.OnSelectionReconIDChanged += ImageOperationService_OnSelectionReconIDChanged;
	}

	[UIRoute]
	private void ProtocolHostService_ParameterChanged(object? sender, EventArgs<(BaseModel baseModel, List<string> list)> e)
	{
		if (e is null || e.Data.baseModel is null || !(e.Data.baseModel is ReconModel reconModel && reconModel.Parent.ScanOption == ScanOption.DualScout && e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.RECON_IS_RTD)) is not null))
		{
			return;
		}
		var recon = ScanReconModelList.FirstOrDefault(t => t.ScanID.Equals(reconModel.Parent.Descriptor.Id) && t.ID.Equals(reconModel.Descriptor.Id));
		if (recon is null)
		{
			return;
		}
		recon.IsRTD = reconModel.IsRTD;
		if (reconModel.IsRTD)
		{
			int indexR = reconModel.Parent.Children.IndexOf(reconModel);
			if (indexR < reconModel.Parent.Children.Count && (reconModel.Parent.TubePositions[indexR] == TubePosition.Angle0 || reconModel.Parent.TubePositions[indexR] == TubePosition.Angle180))
			{
				recon.Content = "AP-" + reconModel.SeriesNumber;
			}
			else
			{
				recon.Content = "LAT-" + reconModel.SeriesNumber;
			}
		}
		else
		{
			recon.Content = ConvertTo(reconModel.Parent, reconModel).Content;
		}
		SetReconModelBackgroundImage(recon, reconModel);
	}

	[UIRoute]
	private void ImageOperationService_OnSelectionReconIDChanged(object? sender, EventArgs<string> e)
	{
		if (string.IsNullOrEmpty(e.Data))
		{
			return;
		}
		if (SelectScanReconModel.ReconTaskID.Equals(e.Data))
		{
			return;
		}
		if (!(ScanReconModelList.Count > 0
			&& SelectScanReconModel is not null
			&& !SelectScanReconModel.ReconTaskID.Equals(e.Data)))
		{
			return;
		}
		var model = ScanReconModelList.FirstOrDefault(t => t.ReconTaskID.Equals(e.Data));
		if (model is not null && (_selectionManager.CurrentSelectionRecon is null || (_selectionManager.CurrentSelectionRecon is ReconModel recon && !recon.Descriptor.Id.Equals(model.ReconTaskID))))
		{
			_selectionManager.SelectRecon(model.ReconModel);
			SelectScanReconModel = model;
		}
	}

	[UIRoute]
	private void SelectionManager_SelectionReconChanged(object? sender, EventArgs<ReconModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		TrySelectRecon(e.Data);
	}

	private void TrySelectRecon(ReconModel reconModel)
	{
		var model = ScanReconModelList.FirstOrDefault(x => x.ID == reconModel.Descriptor.Id);
		if (ScanReconModelList.Count > 0 && model is not null)
		{
			SelectScanReconModel = model;
		}
	}

	[UIRoute]
	private void ImageOperationService_SwitchViewsChanged(object? sender, EventArgs<bool> e)
	{
		if (e is null)
		{
			return;
		}
		if (!(currentScanModel is not null
			&& currentScanModel.ScanOption == ScanOption.DualScout
			&& ScanReconModelList.Count > 0 && ScanReconModelList.Count == 2))
		{
			return;
		}
		ObservableCollection<ScanReconModel> list = new ObservableCollection<ScanReconModel>();
		for (int i = ScanReconModelList.Count - 1; i >= 0; i--)
		{
			list.Add(ScanReconModelList[i]);
		}
		ScanReconModelList = new ObservableCollection<ScanReconModel>();
		ScanReconModelList = list;
	}

	[UIRoute]
	private void ProtocolHostService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
	{
		if (e is null || e.Data.Current is null)
		{
			return;
		}
		if (e.Data.Parent is ScanModel scanModel && e.Data.Current is ReconModel reconModel)
		{
			switch (e.Data.ChangeType)
			{
				case StructureChangeType.Add:
					ScanReconAdded(scanModel, reconModel);
					break;
				case StructureChangeType.Delete:
					ScanReconDeleted(reconModel);
					break;
			}
		}
	}

	[UIRoute]
	private void ProtocolPerformStatusService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		if (e is null || e.Data.Model is null || e.Data.Model.GetType() != typeof(ReconModel))
		{
			return;
		}
		ReconModel reconModel = (ReconModel)e.Data.Model;
		var recon = ScanReconModelList.FirstOrDefault(t => t.ScanID.Equals(reconModel.Parent.Descriptor.Id) && t.ID.Equals(reconModel.Descriptor.Id));
		if (recon is null)
		{
			_logger.LogInformation($"Generate thumbnails,NewStatus:{e.Data.NewStatus}");
			return;
		}
		_logger.LogInformation($"Generate thumbnails Recon model ID is :{recon.ID}");
		switch (e.Data.NewStatus)
		{
			case PerformStatus.Waiting:
				recon.Image = WaitingImage;
				break;
			case PerformStatus.Performing:
				recon.Image = PerformingImage;
				break;
			case PerformStatus.Performed:
				SetPerformedImage(recon, reconModel);
				break;
			case PerformStatus.Unperform:
				recon.Image = null;
				break;
		}
		recon.ReconModel.Status = e.Data.NewStatus;
		FlushedCommandStatus();
	}

	[UIRoute]
	private void RTDReconService_ReconReconDone(object? sender, EventArgs<RealtimeReconInfo> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		HandleImageSavedInfo(e);
		Task.Run(() =>
		{
			Task.Delay(1000).Wait(); //先定为延迟1秒就启动自动重建任务
			var recon = ProtocolHelper.GetRecon(_protocolHostService.Instance, e.Data.ScanId, e.Data.ReconId);
			if (recon is not null && recon.IsRTD && recon.Status == PerformStatus.Performed)
			{
				AutoReconHelper.StartMeasurementAutoReconTasksByRtdRecon(
					_offlineConnectionService,
					_protocolHostService,
					_rawDataService,
					_studyHostService,
					_offlineReconService,
					e.Data.ScanId,
					e.Data.ReconId,
					_logger);
				FlushedCommandStatus();
			}
		});
	}

	private void ScanReconAdded(ScanModel scanModel, ReconModel reconModel)
	{
		var rtdReconModel = ConvertTo(scanModel, reconModel);
		ScanReconModelList.Add(rtdReconModel);
	}

	private void ScanReconDeleted(ReconModel reconModel)
	{
		var targetRecon = ScanReconModelList.FirstOrDefault(n => n.ID.Equals(reconModel.Descriptor.Id));
		if (targetRecon is null)
		{
			return;
		}
		ScanReconModelList.Remove(targetRecon);
	}

	private string GetReconIndicator(int index)
	{
		return $"{_selectionManager.CurrentSelection.Scan?.ScanNumber}-{index}";
	}

	/// <summary>
	/// 切换Scan面板和Recon面板
	/// </summary>
	public void ReconItemClicked()
	{
		_logger.LogInformation($"recon item clicked");
		_selectionManager.SelectRecon(SelectScanReconModel.ReconModel);
	}

	[UIRoute]
	private void SelectionManager_SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		var list = ReconModelConvertToViewModelList(e.Data).ToObservableCollection();

		ScanReconModelList.Clear();
		if (e.Data is not null)
		{
			ScanReconModelList = list;
		}
		//切换选中扫描任务后，默认选择第一个recon任务
		if (ScanReconModelList.Count > 0)
		{
			SelectScanReconModel = ScanReconModelList[0];
		}
	}

	private List<ScanReconModel> ReconModelConvertToViewModelList(ScanModel scanModel)
	{
		var list = new List<ScanReconModel>();
		foreach (var reconItem in scanModel.Children)
		{
			var rtdReconModel = ConvertTo(scanModel, reconItem);
			list.Add(rtdReconModel);
		}
		return list;
	}

	private ScanReconModel ConvertTo(ScanModel scanModel, ReconModel reconModel)
	{
		currentScanModel = scanModel;
		ScanReconModel scanReconModel = new ScanReconModel();
		scanReconModel.ScanID = scanModel.Descriptor.Id;
		scanReconModel.ID = reconModel.Descriptor.Id;
		scanReconModel.ReconTaskID = reconModel.Descriptor.Id;
		if (!reconModel.IsRTD)
		{
			scanReconModel.Content = GetReconIndicator(reconModel.SeriesNumber);
		}
		else
		{
			scanReconModel.Content = "RT-" + reconModel.SeriesNumber;
		}
		scanReconModel.IsRTD = reconModel.IsRTD;
		scanReconModel.ReconModel = reconModel;
		scanReconModel.IsTomo = scanModel.ScanImageType == ScanImageType.Tomo;
		scanReconModel.ImagePath = reconModel.ImagePath;

		if (scanModel.ScanOption == ScanOption.DualScout)
		{
			int indexR = scanModel.Children.IndexOf(reconModel);
			if (indexR < scanModel.Children.Count && (scanModel.TubePositions[indexR] == TubePosition.Angle0 || scanModel.TubePositions[indexR] == TubePosition.Angle180))
			{
				scanReconModel.Content = "AP-" + reconModel.SeriesNumber;
			}
			else
			{
				scanReconModel.Content = "LAT-" + reconModel.SeriesNumber;
			}
		}

		SetReconModelBackgroundImage(scanReconModel, reconModel);
		return scanReconModel;
	}

	private void SetReconModelBackgroundImage(ScanReconModel scanReconModel, ReconModel reconModel)
	{
		switch (reconModel.Status)
		{
			case PerformStatus.Performed:
				SetPerformedImage(scanReconModel, reconModel);
				break;
			case PerformStatus.Waiting:
				scanReconModel.Image = WaitingImage;
				break;
			case PerformStatus.Performing:
				scanReconModel.Image = PerformingImage;
				break;
			default:
				scanReconModel = GetRTDImage(scanReconModel, reconModel);
				break;
		}
		SetTooltipMessage(scanReconModel, reconModel);
	}

	private void SetPerformedImage(ScanReconModel scanReconModel, ReconModel reconModel)
	{
		_logger.LogInformation($"Generate thumbnails : (ScanId, ReconId,FailureReason) => ({scanReconModel.ScanID},{reconModel.Descriptor.Id}, {reconModel.FailureReason})");
		switch (reconModel.FailureReason)
		{
			case FailureReasonType.None:
				if (!string.IsNullOrEmpty(reconModel.ImagePath) && Directory.Exists(reconModel.ImagePath))
				{
					var image = GetGenerateThumbImageByPath(reconModel.ImagePath);
					if (image is null)
					{
						scanReconModel = GetRTDImage(scanReconModel, reconModel);
					}
					else
					{
						scanReconModel.Image = image;
					}
				}
				else
				{
					_logger.LogInformation($"Generate thumbnails,The path does not exist:{reconModel.ImagePath}");
				}
				break;
			default:
				scanReconModel.Image = FailedImage;
				break;
		}
		SetTooltipMessage(scanReconModel, reconModel);
	}

	private void SetTooltipMessage(ScanReconModel scanReconModel, ReconModel reconModel)
	{
		try
		{
			var errorCodes = reconModel.ErrorCodes;

			if (reconModel.FailureReason != FailureReasonType.None && errorCodes.Any())
			{
				StringBuilder sb = new StringBuilder();
				foreach (var error in ErrorCodeHelper.GetErrorCodeList(errorCodes))
				{
					sb.AppendLine($"Error:{error.Code}, {error.Reason}");
				}
				scanReconModel.ToolTipMessage = sb.ToString();
			}
			else
			{
				scanReconModel.ToolTipMessage = null;
			}
		}
		catch (Exception ex)
		{
			scanReconModel.ToolTipMessage = ex.Message;
		}
	}

	private ScanReconModel GetRTDImage(ScanReconModel scanReconModel, ReconModel reconModel)
	{
		if (reconModel.IsRTD)
		{
			scanReconModel.Image = RTDImage;
		}
		else
		{
			scanReconModel.Image = null;
		}
		return scanReconModel;
	}

	private void ReconOn()
	{
		if (!(SelectScanReconModel is not null && !string.IsNullOrEmpty(SelectScanReconModel.ID)))
		{
			return;
		}
		if (!_offlineConnectionService.IsConnected)
		{
			_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, LanguageResource.Message_Warring_OfflineConnectFailed, null, ConsoleSystemHelper.WindowHwnd);
			return;
		}
		var selectedRecon = ProtocolHelper.GetRecon(_protocolHostService.Instance, SelectScanReconModel.ScanID, SelectScanReconModel.ID);
		string mesage = string.Empty;
		if (selectedRecon is ReconModel && !ReconCalculateExtension.ReconParamCalculate(selectedRecon, out mesage))
		{
			_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, mesage, null, ConsoleSystemHelper.WindowHwnd);
			return;
		}
		var rawList = _rawDataService.GetRawDataListByStudyId(_studyHostService.StudyId);
		if (!RuntimeConfig.IsDevelopment && (rawList is null || rawList.Count == 0))
		{
			_dialogService.Show(false, MessageLeveles.Warning, "No raw data", "The study has no raw data!", null, ConsoleSystemHelper.WindowHwnd);
			return;
		}
		var currentRaw = rawList.FirstOrDefault(r => r.ScanId == SelectScanReconModel.ScanID);
		if (!RuntimeConfig.IsDevelopment && (currentRaw is null || string.IsNullOrEmpty(currentRaw.Path)))
		{
			_dialogService.Show(false, MessageLeveles.Warning, "No raw data", "The scan of the study has no raw data!", null, ConsoleSystemHelper.WindowHwnd);
			return;
		}
		if (!RuntimeConfig.IsDevelopment && !Directory.Exists(currentRaw.Path))
		{
			_dialogService.Show(false, MessageLeveles.Warning, "No raw data", "The path of the raw data is not exists!", null, ConsoleSystemHelper.WindowHwnd);
			return;
		}
		_studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
		_logger.LogDebug($"CreateOfflineReconTask: ({_studyHostService.StudyId}, {SelectScanReconModel.ScanID}, {selectedRecon.Descriptor.Id}) => {JsonConvert.SerializeObject(selectedRecon)}");
		var offlineCommand = _offlineReconService.CreateReconTask(_studyHostService.StudyId, SelectScanReconModel.ScanID, SelectScanReconModel.ID);
		if (offlineCommand is not null && offlineCommand.Status != CommandExecutionStatus.Success)
		{
			_logger.LogDebug($"CreateOfflineReconTask exception: ({_studyHostService.StudyId}, {SelectScanReconModel.ScanID}, {selectedRecon.Descriptor.Id}) => {JsonConvert.SerializeObject(offlineCommand)}");
			_dialogService.Show(false, MessageLeveles.Error, "Failed", "The recon is failed", null, ConsoleSystemHelper.WindowHwnd);
		}
		FlushedCommandStatus();
	}

	private void ReconCut()
	{
		if (!(SelectScanReconModel is not null && !string.IsNullOrEmpty(SelectScanReconModel.ID)))
		{
			return;
		}
		_protocolHostService.DeleteRecon(SelectScanReconModel.ScanID, SelectScanReconModel.ID);
		_offlineReconService.DeleteTask(_studyHostService.StudyId, SelectScanReconModel.ScanID, SelectScanReconModel.ID);
		FlushedCommandStatus();
	}

	private void ReconRepeat()
	{
		if (!(SelectScanReconModel is not null && !string.IsNullOrEmpty(SelectScanReconModel.ID)))
		{
			return;
		}
		_protocolHostService.CopyRecon(SelectScanReconModel.ScanID, SelectScanReconModel.ID);
		FlushedCommandStatus();
	}

	private void ReconCancel()
	{
		if (!(SelectScanReconModel is not null && !string.IsNullOrEmpty(SelectScanReconModel.ID)))
		{
			return;
		}
		_offlineReconService.CloseReconTask(_studyHostService.StudyId, SelectScanReconModel.ScanID, SelectScanReconModel.ID);
	}

	[UIRoute]
	private void FlushedCommandStatus()
	{
		(Commands[CommandParameters.COMMAND_RECON_ON] as DelegateCommand)?.RaiseCanExecuteChanged();
		(Commands[CommandParameters.COMMAND_RECON_CUT] as DelegateCommand)?.RaiseCanExecuteChanged();
		(Commands[CommandParameters.COMMAND_RECON_REPEAT] as DelegateCommand)?.RaiseCanExecuteChanged();
		(Commands[CommandParameters.COMMAND_RECON_CANCEL] as DelegateCommand)?.RaiseCanExecuteChanged();
	}

	private void HandleImageSavedInfo(EventArgs<RealtimeReconInfo> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		var srModel = ScanReconModelList.FirstOrDefault(t => t.ScanID.Equals(e.Data.ScanId) && t.ID.Equals(e.Data.ReconId));
		if (!(srModel is not null && !string.IsNullOrEmpty(e.Data.ImagePath) && Directory.Exists(e.Data.ImagePath)))
		{
			_logger.LogInformation($"Generate thumbnails RTD,The path does not exist:{e.Data.ImagePath}");
			return;
		}
		srModel.Image = GetGenerateThumbImageByPath(e.Data.ImagePath);
	}

	private WriteableBitmap? GetGenerateThumbImageByPath(string path)
	{
		WriteableBitmap? writeableBitmap = null;
		var fileName = Directory.GetFiles(path, "*.dcm").FirstOrDefault();
		if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
		{
			try
			{
				_logger.LogInformation($"Start generate thumbnails:{fileName}");
				writeableBitmap = DicomImageHelper.Instance.GenerateThumbImage(fileName, 75, 75);
				_logger.LogInformation($"End generate thumbnails:{fileName}");
			}
			catch (Exception ex)
			{
				_logger.LogError("GenerateThumbImage_StackTrace:" + ex.StackTrace + ";Message:" + ex.Message);
			}
		}
		else
		{
			_logger.LogInformation($"Generate thumbnails,The path does not exist *.dcm files:{path}");
		}
		return writeableBitmap;
	}

	#region context menu can execute
	private bool IsCanRecon()
	{
		if (SelectScanReconModel is null)
		{
			return false;
		}
		if (IsRTD())
		{
			return false;
		}

		var rtdReconModel = ScanReconModelList.FirstOrDefault(n => n.IsRTD);
		//能不能recon必须 是未扫描的recon ，并且RTD已经完成
		return (SelectScanReconModel.ReconModel.Status == PerformStatus.Unperform)
			&& !SelectScanReconModel.IsReconProcessing && rtdReconModel?.ReconModel.Status == PerformStatus.Performed;
	}

	private bool IsCanCancel()
	{
		if (SelectScanReconModel is null)
		{
			return false;
		}
		if (IsRTD())
		{
			return false;
		}
		return SelectScanReconModel.IsReconProcessing;
	}

	private bool IsRTD()
	{
		return SelectScanReconModel is not null && SelectScanReconModel.IsRTD;
	}

	private bool IsCanCut()
	{
		if (SelectScanReconModel is null)
		{
			return false;
		}

		//已完成的扫描,不允许 cut
		if (SelectScanReconModel.ReconModel.Status == PerformStatus.Performed)
		{
			return false;
		}

		return !IsRTD() && !SelectScanReconModel.IsReconProcessing;
	}

	private bool IsCanRepeat()
	{
		if (SelectScanReconModel is null)
		{
			return false;
		}
		//先暂时配合算法不加限制个数
		// return ScanReconModelList.Count < 8 && _selectionManager.CurrentSelection.Scan?.ScanOption != ScanOption.SURVIEW;
		return _selectionManager.CurrentSelection.Scan?.ScanOption != ScanOption.Surview;
	}
	#endregion
}
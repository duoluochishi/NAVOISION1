//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.ViewModel;

public class TableControlViewModel : BaseViewModel
{
	private readonly ILogger<TableControlViewModel> _logger;
	private readonly ITablePositionService _tablePositionService;
	private readonly TableMoveWindow? _tableMoveWindow;
	private readonly IDialogService _dialogService;
	private readonly IConsoleApplicationService _consoleApplicationService;

	public TableControlViewModel(ILogger<TableControlViewModel> logger,
		ITablePositionService tablePositionService,
		IDialogService dialogService,
		IConsoleApplicationService consoleApplicationService)
	{
		_logger = logger;
		_dialogService = dialogService;
		_tablePositionService = tablePositionService;
		_consoleApplicationService = consoleApplicationService;

		_tablePositionService.TablePositionChanged += TablePositionService_TablePositionChanged;
		_tablePositionService.TableArrived += TablePositionService_TableArrived;

		_tableMoveWindow = CTS.Global.ServiceProvider.GetService<TableMoveWindow>();

		Commands.Add("ShowTableMoveDialogCommand", new DelegateCommand(ShowTableMoveDialogCommand));
		Commands.Add("Simulate",new DelegateCommand(() =>
		{
			IsXInIsoCenter = !IsXInIsoCenter;
		}));
		Commands.Add("ResetXToCenter", new DelegateCommand(ResetXToIsoCenter));
		

		CheckIsXAxisInIsoCenter();

		UpdateTablePositionInitially();
	}

	private void ResetXToIsoCenter()
	{
		_dialogService.ShowDialog(true,MessageLeveles.Warning,"Reset ISO-X","Reset X-axis direction of table to ISO center ?",
			dialogResult =>
			{
				if (dialogResult.Result is ButtonResult.OK)
				{
					try
					{
						var resetResult = _tablePositionService.ResetAxisX();

						_logger.LogInformation($"ResetXToIsoCenter called with result : {resetResult}");

						//recheck x-axis is in iso center
						if (resetResult)
						{
							CheckIsXAxisInIsoCenter();
						}
					}
					catch (Exception ex)
					{
						_logger.LogError($"ResetXToIsoCenter with error {ex.Message}");
					}
				}
			},ConsoleSystemHelper.WindowHwnd);
	}

	private void CheckIsXAxisInIsoCenter()
	{
		Task.Run(() =>
		{
			try
			{
				var checkResult = _tablePositionService.CheckISOCenterWithAxisX();
				_logger.LogInformation($"CheckISOCenterWithAxisX with result : {checkResult} ");

				Application.Current?.Dispatcher?.Invoke(() =>
				{
					IsXInIsoCenter = checkResult;
				});
			}
			catch (Exception ex)
			{
				_logger.LogError($"CheckISOCenterWithAxisX with error {ex.Message}");
			}
		});
	}

	private void UpdateTablePositionInitially()
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			if (null != _tablePositionService.CurrentTablePosition)
			{
				HorizontalPosition = OneDot(_tablePositionService.CurrentTablePosition.HorizontalPosition.Micron2Millimeter());
				VerticalPosition = OneDot(((int)_tablePositionService.CurrentTablePosition.VerticalPosition).Micron2Millimeter());
			}

			
		});
	}

	private void TablePositionService_TableArrived(object? sender, EventArgs e)
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			_logger.LogInformation($"table arrived");
			Thread.Sleep(1000);
			_tableMoveWindow?.Hide();
		});
	}

	private void TablePositionService_TablePositionChanged(object? sender, EventArgs<TablePositionInfo> e)
	{
		if (e.Data is null)
			return;

		//_logger.LogInformation("HorizontalPosition: "+e.HorizontalPosition.ToString());
		//_logger.LogInformation("VerticalPosition: " + e.VerticalPosition.ToString());

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			try
			{
				HorizontalPosition = OneDot(e.Data.HorizontalPosition / 1000.0);
				VerticalPosition = OneDot(e.Data.VerticalPosition / 1000.0);
			}
			catch (Exception ex)
			{
				_logger.LogError($"StatusStrip_TablePositionChanged with error {ex.Message}-{ex.StackTrace}");
			}
		});
	}

	private void ShowTableMoveDialogCommand()
	{
		_consoleApplicationService.StartMoveTable();
		_tableMoveWindow?.ShowWindowDialog(true);
	}

	private double _horizontalPosition;
	private double _verticalPosition;

	/// <summary>
	/// 床水平位置
	/// </summary>
	public double HorizontalPosition
	{
		get => _horizontalPosition;
		set => SetProperty(ref _horizontalPosition, value);
	}

	/// <summary>
	/// 床垂直位置
	/// </summary>
	public double VerticalPosition
	{
		get => _verticalPosition;
		set => SetProperty(ref _verticalPosition, value);
	}

	private double OneDot(double val)
	{
		return Math.Truncate(val * 10) / 10;
	}


	private bool _isXInIsoCenter;
	public bool IsXInIsoCenter
	{
		get => _isXInIsoCenter;
		set => SetProperty(ref _isXInIsoCenter, value);
	}

}
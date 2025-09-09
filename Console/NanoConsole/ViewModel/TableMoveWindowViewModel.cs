//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.ViewModel;

public class TableMoveWindowViewModel : BaseViewModel
{
	private readonly ITablePositionService _tablePositionService;
	private readonly ILogger<TableMoveWindowViewModel> _logger;
	private readonly IConsoleApplicationService _consoleApplicationService;

	public TableMoveWindowViewModel(ITablePositionService tablePositionService, ILogger<TableMoveWindowViewModel> logger, IConsoleApplicationService consoleApplicationService
		)
	{
		_logger = logger;
		_tablePositionService = tablePositionService;
		_consoleApplicationService = consoleApplicationService;
		_consoleApplicationService.MoveTableStarted += MoveTableStarted;

		UpdateTablePositionInitially();

		var validRange = _tablePositionService.GetValidRange();
		ValidHMin = validRange.horizontalRange.Item1.Micron2Millimeter();
		ValidHMax = validRange.horizontalRange.Item2.Micron2Millimeter();
		ValidVMin = validRange.verticalRange.Item1.Micron2Millimeter();
		ValidVMax = validRange.verticalRange.Item2.Micron2Millimeter();
		Tips = $"Tips :  valid table position from {ValidHMin} to {ValidHMax} , height from {ValidVMin} to {ValidVMax} ";

		Commands.Add("CloseCommand", new DelegateCommand<object>(CloseClicked, _ => true));
		Commands.Add("CancelCommand", new DelegateCommand<object>(CancelClicked, _ => true));
		Commands.Add("DragMoveCommand", new DelegateCommand<object>(DragMove, _ => true));
		Commands.Add("PrepareTableMoveCommand", new DelegateCommand(PrepareTableMoveCommand));
		Commands.Add("MoveCommand", new DelegateCommand(PrepareTableMoveCommand));
	}

	/// <summary>
	/// 用户点击移床按钮，开始移床
	/// </summary>
	private void MoveTableStarted(object? sender, EventArgs e)
	{
		//update table position initially

		UpdateTablePositionInitially();
	}

	/// <summary>
	/// 重新打开移床界面的时候，默认再次初始化
	/// </summary>
	private void UpdateTablePositionInitially()
	{
		Application.Current?.Dispatcher?.Invoke(() =>
		{
			if (null != _tablePositionService.CurrentTablePosition)
			{
				TextTablePosition = ((double)_tablePositionService.CurrentTablePosition.HorizontalPosition).Micron2Millimeter();
				TextTableHeight = ((double)_tablePositionService.CurrentTablePosition.VerticalPosition).Micron2Millimeter();

				InputPosition = StringOneDot(TextTablePosition);
				InputHeight = StringOneDot(TextTableHeight);
			}

			ErrorMsg = "";
		});
	}

	private void PrepareTableMoveCommand()
	{
		int verticalPosition = TextTableHeight.Millimeter2Micron();
		int horizontalPosition = TextTablePosition.Millimeter2Micron();

		var checkResult = _tablePositionService.CheckPosition(horizontalPosition, verticalPosition);
		if (checkResult.Item1)
		{
			_logger.LogInformation($"Prepare to send bed transfer command，target horizontal position：{horizontalPosition}  target vertical position：{verticalPosition}");

			if (!_tablePositionService.Move(horizontalPosition, verticalPosition))
			{
				_logger.LogInformation($"Table move failed or system exception!");

				ErrorMsg = $"Table move failed or system malfunction!";
			}
			else
			{
				ErrorMsg = "";
			}
		}
		else
		{

			//ErrorMsg = $"The value of table position is out of range, please change!";
			ErrorMsg = checkResult.Item2;
		}
	}

	public void DragMove(object parameter)
	{
		if ((parameter as Window) is not null)
		{
			((Window)parameter).DragMove();
		}
	}

	public void CloseClicked(object parameter)
	{
		if (parameter is Window window)
		{
			window.Hide();
		}
	}

	public void CancelClicked(object parameter)
	{
		if (parameter is Window window)
		{
			_logger.LogInformation("Cancelled move table.");
			_tablePositionService.CancelMove();
			window.Hide();
		}
	}

	private string StringOneDot(double val)
	{
		return DoubleOneDot(val).ToString("F1");
	}

	private double DoubleOneDot(double val)
	{
		return Math.Truncate(val * 10) / 10.0;
	}

	#region properties

	/// <summary>
	/// 水平床位 有效值
	/// </summary>
	private double _textTablePosition = 0;
	public double TextTablePosition
	{
		get => _textTablePosition;
		set => SetProperty(ref _textTablePosition, value);
	}

	/// <summary>
	/// 垂直床位 有效值
	/// </summary>
	private double _textTableHeight;
	public double TextTableHeight
	{
		get => _textTableHeight;
		set => SetProperty(ref _textTableHeight, value);
	}

	/// <summary>
	/// 水平位置 输入值 保留1位小数
	/// </summary>
	private string _inputPosition = "";
	public string InputPosition
	{
		get => _inputPosition;
		set
		{
			if (double.TryParse(value, out double inputValue))
			{
				//真实值
				TextTablePosition = DoubleOneDot(inputValue);

				//格式化显示值
				SetProperty(ref _inputPosition, StringOneDot(inputValue));
			}
		}
	}

	/// <summary>
	/// 垂直位置 输入值 保留1位小数
	/// </summary>
	private string _inputHeight = "";
	public string InputHeight
	{
		get => _inputHeight;
		set
		{
			if (double.TryParse(value, out double inputValue))
			{
				//真实值
				TextTableHeight = DoubleOneDot(inputValue);

				//格式化显示值
				SetProperty(ref _inputHeight, StringOneDot(inputValue));
			}
		}
	}

	private double _validHMin;
	public double ValidHMin
	{
		get => _validHMin;
		set => SetProperty(ref _validHMin, value);
	}

	private double _validHMax;
	public double ValidHMax
	{
		get => _validHMax;
		set => SetProperty(ref _validHMax, value);
	}

	private double _validVMin;
	public double ValidVMin
	{
		get => _validVMin;
		set => SetProperty(ref _validVMin, value);
	}

	private double _validVMax;
	public double ValidVMax
	{
		get => _validVMax;
		set => SetProperty(ref _validVMax, value);
	}

	private string _tips = string.Empty;
	public string Tips
	{
		get => _tips;
		set => SetProperty(ref _tips, value);
	}

	private string _errorMsg = string.Empty;
	public string ErrorMsg
	{
		get => _errorMsg;
		set => SetProperty(ref _errorMsg, value);
	}
	#endregion

}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:19    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class TablePositionService : ITablePositionService
{
	private TablePositionInfo _currentTablePosition = new()
	{
		AxisXPosition = 0,
		AxisXSpeed = 0,
		HorizontalPosition = 0,
		VerticalPosition = 0,
		HorizontalSpeed = 0,
		VerticalSpeed = 0,
		Locked = false
	};

	private GantryPositionInfo _currentGantryPosition = new()
	{
		Position = 0,
		Speed = 0
	};

	private PartStatus _currentTableStatus = PartStatus.Normal;

	private PartStatus _currentGantryStatus = PartStatus.Normal;

	public PartStatus CurrentTableStatus => _currentTableStatus;

	public PartStatus CurrentGantryStatus => _currentGantryStatus;

	public TablePositionInfo CurrentTablePosition => _currentTablePosition;

	public GantryPositionInfo CurrentGantryPosition => _currentGantryPosition;

	public event EventHandler<EventArgs<TablePositionInfo>>? TablePositionChanged;
	public event EventHandler<EventArgs<GantryPositionInfo>>? GantryPositionChanged;
	public event EventHandler? TableArrived;
	public event EventHandler<PartStatus> TableStatusChanged;
	public event EventHandler<PartStatus> GantryStatusChanged;
	public event EventHandler<bool> TableLocked;

	private readonly IRealtimeStatusProxyService _realtimeStatusProxyService;

	private readonly ILogger<TablePositionService> _logger;

	private int _minY;
	private int _maxY;
	private int _minZ;
	private int _maxZ;
	private int _freeY2Z;
	private int _freeZ2Y;

	public TablePositionService(ILogger<TablePositionService> logger, IRealtimeStatusProxyService realtimeStatusProxyService)
	{
		_logger = logger;
		_realtimeStatusProxyService = realtimeStatusProxyService;
		_realtimeStatusProxyService.CycleStatusChanged += OnProxyService_CycleStatusChanged;
		AcqReconProxy.Instance.CurrentDeviceSystem.Table.Arrived += OnAcqReconProxy_TableArrived;

		_minY = SystemConfig.TableConfig.Table.MinY.Value;
		_maxY = SystemConfig.TableConfig.Table.MaxY.Value;
		_minZ = SystemConfig.TableConfig.Table.MinZ.Value;
		_maxZ = SystemConfig.TableConfig.Table.MaxZ.Value;

		_freeY2Z = SystemConfig.TableConfig.Table.YFreeMoveZThreshold.Value;
		_freeZ2Y = SystemConfig.TableConfig.Table.ZFreeMoveYThreshold.Value;
	}

	private void OnAcqReconProxy_TableArrived(object? sender, EventArgs e)
	{
		_logger.LogInformation($"Table.Arrived: {_currentTablePosition.HorizontalPosition}, {_currentTablePosition.VerticalPosition}");
		TableArrived?.Invoke(sender, e);
	}

	private void OnProxyService_CycleStatusChanged(object? sender, EventArgs<Contract.Models.DeviceSystem> e)
	{
		var tableInfo = e.Data.Table;
		if (_currentTableStatus != tableInfo.Status)
		{
			_currentTableStatus = tableInfo.Status;
			TableStatusChanged?.Invoke(this, _currentTableStatus);
		}

		if (_currentTablePosition.Locked != tableInfo.Locked)
		{
			_currentTablePosition.Locked = tableInfo.Locked;
			TableLocked?.Invoke(this, _currentTablePosition.Locked);
		}

		if (Compare(tableInfo.HorizontalPosition, _currentTablePosition.HorizontalPosition)
			|| Compare(tableInfo.VerticalPosition, _currentTablePosition.VerticalPosition))
		{
			_currentTablePosition.HorizontalPosition = tableInfo.HorizontalPosition;
			_currentTablePosition.VerticalPosition = tableInfo.VerticalPosition;
			_currentTablePosition.HorizontalSpeed = tableInfo.HorizontalSpeed;
			_currentTablePosition.VerticalSpeed = tableInfo.VerticalSpeed;
			_currentTablePosition.AxisXPosition = tableInfo.AxisXPosition;
			_currentTablePosition.AxisXSpeed = tableInfo.AxisXSpeed;
			TablePositionChanged?.Invoke(this, new EventArgs<TablePositionInfo>(_currentTablePosition));
		}

		var gantryInfo = e.Data.Gantry;
		if (_currentGantryStatus != gantryInfo.Status)
		{
			_currentGantryStatus = gantryInfo.Status;
			GantryStatusChanged?.Invoke(this, _currentGantryStatus);
		}

		if (Compare(gantryInfo.Position, _currentGantryPosition.Position))
		{
			_currentGantryPosition.Position = gantryInfo.Position;
			_currentGantryPosition.Speed = gantryInfo.Speed;
			GantryPositionChanged?.Invoke(this, new EventArgs<GantryPositionInfo>(_currentGantryPosition));
		}
	}

	private bool Compare(int source, int destination)
	{
		return (source - destination) != 0;
	}

	private bool Compare(uint source, uint destination)
	{
		return (source - destination) != 0;
	}

	/// <summary>
	/// 见配置文件 Table.Config
	/// </summary>
	/// <param name="horizontal"></param>
	/// <param name="vertical"></param>
	public (bool, string) CheckPosition(int horizontal, int vertical)
	{
		var errorMsg = "";

		_logger.LogInformation($"table checkposition h:{horizontal},v:{vertical}");

		if (horizontal > _maxZ || horizontal < _minZ)
		{
			errorMsg = $"Valid table position should between [{_minZ.Micron2Millimeter()},{_maxZ.Micron2Millimeter()}]";
			return (false, errorMsg);
		}
		if (vertical > _maxY || vertical < _minY)
		{
			errorMsg = $"Valid table height should between [{_minY.Micron2Millimeter()},{_maxY.Micron2Millimeter()}]";
			return (false, errorMsg);
		}

		TableDirection? moveDirection = TableDirection.Out;
		//当前位置 < 期望位置                  -1960  当前位置    期望位置   0 
		if (_currentTablePosition.HorizontalPosition > horizontal)
		{
			moveDirection = TableDirection.In;
		}
		//当前位置 > 期望位置					-1960  期望位置    当前位置    0 
		else if (_currentTablePosition.HorizontalPosition < horizontal)
		{
			moveDirection = TableDirection.Out;
		}

		//如果是出床，直接ok
		if (moveDirection == TableDirection.Out)
		{
			return (true, "");
		}

		//3）当垂直方向低于750时，水平方向 不允许进床，只能退床；
		if (vertical < _freeZ2Y && moveDirection is TableDirection.In)
		{
			return (false, $"When table height lower than {_freeZ2Y.Micron2Millimeter()} , table can only be moved out!");
		}

		//有用的废话
		////4）当垂直方向高于750时，水平方向 全范围内可任意移动；
		//if (vertical > _freeZ2Y)
		//{
		//	return (true, "");
		//}

		////1）水平Z轴 0~280范围内，垂直全范围内可自由升降，即 从最低点480至最高点980；
		////  h<=0 && h>=-280     &&      v>=480 && v<=980
		//if (horizontal <= _maxZ && horizontal >= (_freeY2Z * -1.0) && vertical >= _minY && vertical <= _maxY)
		//{
		//	return (true, "");
		//}

		////2）水平Z轴在280~1960范围内时，垂直可运动的范围变为：最低750至最高980；
		////  h<-280 && h>=-1960      &&     v>=750 && v<=980
		//if (horizontal < (_freeY2Z * -1.0) && horizontal >= _minZ && vertical >= _freeZ2Y && vertical <= _maxY)
		//{
		//	return (true, "");
		//}

		//1）水平Z轴 0~280范围内，垂直全范围内可自由升降，即 从最低点480至最高点980；
		//h<=0 && h>=-280 
		if (horizontal <= _maxZ && horizontal >= (_freeY2Z * -1.0))
		{
			if (vertical >= _minY && vertical <= _maxY)
			{
				return (true, "");
			}
		}
		//2）水平Z轴在280~1960范围内时，垂直可运动的范围变为：最低750至最高980；
		// h<-280 && h>=-1960
		else if (horizontal < (_freeY2Z * -1.0) && horizontal >= _minZ)
		{
			// v>=750 && v<=980
			if (vertical >= _freeZ2Y && vertical <= _maxY)
			{
				return (true, "");
			}
		}

		_logger.LogInformation($"table checkposition skip all condition with h:{horizontal},v:{vertical} , seems to be unnormal");

		errorMsg = $"H between [{_minZ.Micron2Millimeter()},-{_freeY2Z.Micron2Millimeter()}) valid V is [{_freeZ2Y.Micron2Millimeter()},{_maxY.Micron2Millimeter()}] , H between [-{_freeY2Z.Micron2Millimeter()},{_maxZ.Micron2Millimeter()}] valid V is [{_minY.Micron2Millimeter()},{_maxY.Micron2Millimeter()}]";
		return (false, errorMsg);
	}

	public bool ValidatePosition(int horizontal, int vertical)
	{
		TableDirection? moveDirection = TableDirection.Out;
		//当前位置 < 期望位置                  -1960  当前位置    期望位置   0 
		if (_currentTablePosition.HorizontalPosition > horizontal)
		{
			moveDirection = TableDirection.In;
		}
		//当前位置 > 期望位置					-1960  期望位置    当前位置    0 
		else if (_currentTablePosition.HorizontalPosition < horizontal)
		{
			moveDirection = TableDirection.Out;
		}
		//如果是出床，直接ok
		if (moveDirection == TableDirection.Out)
		{
			return true;
		}
		//3）当垂直方向低于750时，水平方向 不允许进床，只能退床；
		if (vertical < _freeZ2Y && moveDirection is TableDirection.In)
		{
			return false;
		}
		//1）水平Z轴 0~280范围内，垂直全范围内可自由升降，即 从最低点480至最高点980；
		//h<=0 && h>=-280 
		if (horizontal <= _maxZ && horizontal >= (_freeY2Z * -1.0))
		{
			if (vertical >= _minY && vertical <= _maxY)
			{
				return true;
			}
		}
		//2）水平Z轴在280~1960范围内时，垂直可运动的范围变为：最低750至最高980；
		// h<-280 && h>=-1960
		else if (horizontal < (_freeY2Z * -1.0) && horizontal >= _minZ)
		{
			// v>=750 && v<=980
			if (vertical >= _freeZ2Y && vertical <= _maxY)
			{
				return true;
			}
		}
		_logger.LogInformation($"table checkposition skip all condition with h:{horizontal},v:{vertical} , seems to be unnormal");
		return false;
	}

	/// <summary>
	/// 仅下发移床命令（判断是否可移床，使用CheckPosition）
	/// </summary>
	/// <param name="horizontal"></param>
	/// <param name="vertical"></param>
	public bool Move(int horizontal, int vertical)
	{
		CommandResult result = null;

		try
		{
			result = AcqReconProxy.Instance.CurrentDeviceSystem.Table.Move(horizontal, vertical);
		}
		catch (Exception ex)
		{
			HandleException(ex, "TableMove", string.Empty, ex.Message);
			return false;
		}

		if (result.Status != CommandStatus.Success)
		{
			_logger.LogInformation($"TableMove fail: {result?.Status.ToString()}, details:{JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
			//todo: 待处理ErrorCode
			return false;
		}

		return true;
	}

	public bool ResetAxisX()
	{
		var result = AcqReconProxy.Instance.CurrentDeviceSystem.Table.MoveToAxisXCenterPosition();
		return result.Status == CommandStatus.Success;
	}

	public bool CheckISOCenterWithAxisX()
	{
		return AcqReconProxy.Instance.CurrentDeviceSystem.Table.CheckTableInAxisXCenterPosition();
	}

    public (Tuple<int, int> horizontalRange, Tuple<int, int> verticalRange) GetValidRange()
	{
		return (Tuple.Create(_minZ, _maxZ), Tuple.Create(_minY, _maxY));
	}

	public void CancelMove()
	{
		CommandResult result = null;

		try
		{
			result = AcqReconProxy.Instance.CurrentDeviceSystem.Table.CancelMove();
		}
		catch (Exception ex)
		{
			HandleException(ex, "CancelMove", string.Empty, ex.Message);
			return;
		}
		
		if (result.Status != CommandStatus.Success)
		{
			_logger.LogInformation($"CancelMove fail: {result?.Status.ToString()}, details:{JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
			//todo: 待处理ErrorCode
		}
	}

	private void HandleException(Exception ex, string actionName, string errorCode, string errorDescription)
	{
		_logger.LogError($"{actionName} failed, {ex.GetType().Name}: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
		//todo: 待处理
		//if (ex is ArgumentException || ex is TimeoutException || ex is BadNetworkExcption)
		//{
		//    throw new NanoException("", $"{actionName} {ex.GetType().Name}: {ex.Message}", ex);
		//}
		//else
		//{
		//    throw new NanoException(errorCode, errorDescription, ex);
		//}
	}
}
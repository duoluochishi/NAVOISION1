//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CommonAttribute.AOPAttribute;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Helpers;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Exceptions;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class RealtimeReconProxyService : IRealtimeReconProxyService
{
	private readonly IMapper _mapper;
	private readonly ILogger<RealtimeReconProxyService> _logger;
	private readonly IRealtimeProxyService _proxyService;


	private SystemStatusInfo _currentSystemStatus;
	private string _currentRecon = string.Empty;
	private RealtimeReconInfo _currentRawData;

	public SystemStatus SystemStatus { get; private set; }

	/// <summary>
	/// 系统状态变更事件
	/// </summary>
	public event EventHandler<EventArgs<SystemStatusInfo>>? SystemStatusChanged;

	/// <summary>
	/// 重建图像保存事件（IsOver = false）
	/// </summary>
	public event EventHandler<EventArgs<RealtimeReconInfo>> ImageReceived;

	/// <summary>
	/// 采集重建状态变更
	/// </summary>
	public event EventHandler<EventArgs<RealtimeReconInfo>> RealtimeReconStatusChanged;

	public event EventHandler<EventArgs<RealtimeReconInfo>> RawDataSaved;

	public RealtimeReconProxyService(IMapper mapper, ILogger<RealtimeReconProxyService> logger, IRealtimeProxyService proxyService)
	{
		_mapper = mapper;
		_logger = logger;
		_proxyService = proxyService;

		_proxyService.AcqReconStatusChanged += OnReconProxy_AcqReconStatusChanged;
		_proxyService.SystemStatusChanged += OnReconProxy_SystemStatusChanged;
		_proxyService.ReconImageSaved += OnReconProxy_ReconImageSaved;
		_proxyService.RawImageSaved += OnReconProxy_RawImageSaved;

		SystemStatus = _proxyService.SystemStatus;
	}

	[MainWorkingBox]
	private void OnReconProxy_RawImageSaved(object? sender, FacadeProxy.Common.Arguments.RawImageSavedEventArgs rawImageArgs)
	{
		try
		{
			var realtimeInfo = _mapper.Map<RealtimeReconInfo>(rawImageArgs);
			realtimeInfo.ImagePath = rawImageArgs.Directory;
			if (_currentRawData is null || _currentRawData.ScanId != realtimeInfo.ScanId)
			{
				_currentRawData = realtimeInfo;
				RawDataSaved?.Invoke(this, new EventArgs<RealtimeReconInfo>(_currentRawData));
			}
		}
		catch (Exception ex)
		{
            _logger.LogError(ex, $"RealtimeFacadeProxy.RawImageSaved exception: {JsonConvert.SerializeObject(new { StudyInstanceUID = rawImageArgs.StudyUID, ScanId = rawImageArgs.ScanUID, ReconId = rawImageArgs.ReconID, Directory = rawImageArgs.Directory })}");
        }
    }

	[MainWorkingBox]
	private void OnReconProxy_AcqReconStatusChanged(object arg1, FacadeProxy.Common.Arguments.AcqReconStatusArgs acqStatusArgs)
	{
		_logger.LogInformation($"RealtimeFacadeProxy.AcqReconStatusChanged arguments: {JsonConvert.SerializeObject(new { StudyInstanceUID = acqStatusArgs.StudyUID, ScanId = acqStatusArgs.ScanUID, ReconId = acqStatusArgs.ReconID, AcqReconStatus = acqStatusArgs.Status.ToString(), Directory = acqStatusArgs.ReconDataPath })}");
		try
		{
			var realtimeReconInfo = _mapper.Map<RealtimeReconInfo>(acqStatusArgs);
			RealtimeReconStatusChanged?.Invoke(this, new EventArgs<RealtimeReconInfo>(realtimeReconInfo));
		}
		catch (Exception ex)
		{
            _logger.LogError(ex, $"RealtimeFacadeProxy.AcqReconStatusChanged exception: {JsonConvert.SerializeObject(new { StudyInstanceUID = acqStatusArgs.StudyUID, ScanId = acqStatusArgs.ScanUID, ReconId = acqStatusArgs.ReconID, AcqReconStatus = acqStatusArgs.Status.ToString(), Directory = acqStatusArgs.ReconDataPath })}");
		}
	}

	[MainWorkingBox]
	private void OnReconProxy_SystemStatusChanged(object sender, FacadeProxy.Common.Arguments.SystemStatusArgs systemStatusArgs)
	{
		_logger.LogInformation($"RealtimeFacadeProxy.SystemStatusChanged arguments: {JsonConvert.SerializeObject(new { ScanId = systemStatusArgs.ScanUID, SystemStatus = systemStatusArgs.Status.ToString() })}");
		var systemStatus = new SystemStatusInfo
		{
			ScanId = systemStatusArgs.ScanUID,
			Status = systemStatusArgs.Status
		};
		//关闭上一次状态和当前事件状态相同的情况
		//if (_currentSystemStatus is not null && _currentSystemStatus.Status == systemStatus.Status && _currentSystemStatus.ScanId == systemStatus.ScanId) return;
		_currentSystemStatus = systemStatus;

		if (_currentSystemStatus.Status == SystemStatus.NormalScanStopped || _currentSystemStatus.Status == SystemStatus.EmergencyStopped || _currentSystemStatus.Status == SystemStatus.ErrorScanStopped)
		{
			try
			{
				var doseInfo = AcqReconProxy.Instance.GetDoseInfo(_currentSystemStatus.ScanId);
				_logger.LogDebug($"RealtimeFacadeProxy.SystemStatusChanged GetDoseInfo: {JsonConvert.SerializeObject(doseInfo)}");
                _currentSystemStatus.DoseInfo = _mapper.Map<CTS.Models.RealDoseInfo>(doseInfo);
			}
			catch (Exception ex)
			{
                _logger.LogWarning(ex, $"RealtimeFacadeProxy.SystemStatusChanged GetDoseInfo exception: {JsonConvert.SerializeObject(new { ScanId = systemStatusArgs.ScanUID, ExceptionMessage = ex.Message })}");
			}
		}

		try
		{
			SystemStatusChanged?.Invoke(this, new EventArgs<SystemStatusInfo>(_currentSystemStatus));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"RealtimeFacadeProxy.SystemStatusChanged handled exception: {JsonConvert.SerializeObject(new { ScanId = _currentSystemStatus.ScanId, SystemStatus = _currentSystemStatus.Status.ToString(), ExceptionMessage = ex.Message })}");
		}
	}

	[MainWorkingBox]
	private void OnReconProxy_ReconImageSaved(object sender, FacadeProxy.Common.Arguments.ImageSavedEventArgs imageSavedArgs)
	{
		var realtimeReconInfo = _mapper.Map<RealtimeReconInfo>(imageSavedArgs);

		if (_currentRecon != realtimeReconInfo.ReconId)
		{
			_currentRecon = string.Empty;
		}

		if (!string.IsNullOrEmpty(_currentRecon)) return;

		realtimeReconInfo.LastImage = imageSavedArgs.DataPaths.LastOrDefault();

		try
		{
			ImageReceived?.Invoke(this, new EventArgs<RealtimeReconInfo>(realtimeReconInfo));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"RealtimeFacadeProxy.ReconImageSaved (WorkingBox) exception: {ex.Message} => {realtimeReconInfo.ScanId}, {realtimeReconInfo.ReconId}, {realtimeReconInfo.IsOver}");
		}

		if (realtimeReconInfo.IsOver)
		{
			if (string.IsNullOrEmpty(_currentRecon))
			{
				_currentRecon = realtimeReconInfo.ReconId;
			}
		}
		else
		{
			_currentRecon = string.Empty;
		}
	}

	/// <summary>
	/// 开始扫描
	/// </summary>
	public RealtimeCommandResult StartScan(IList<ScanReconParam> infos)
	{
		_logger.LogInformation($"StartScan parameters: {JsonConvert.SerializeObject(infos)}");

		CommandResult result = null;

		try
		{
			result = PerformanceMonitorHelper.Execute("AcqReconProxy.StartScan", () => AcqReconProxy.Instance.StartScan(infos.ToList()));
			return new RealtimeCommandResult
			{
				Status = _mapper.Map<CommandExecutionStatus>(result.Status),
				Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
			};
        }
		catch (Exception ex)
		{
            _logger.LogDebug($"StartScan failed: {result?.Status.ToString()}, details: {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
			return HandleException(ex, "StartScan");
		}
	}

	/// <summary>
	/// 停止扫描
	/// </summary>
	public RealtimeCommandResult AbortScan(AbortCause abortCause, bool isDeleteRawData)
	{
		CommandResult result = null;

		try
		{
			result = PerformanceMonitorHelper.Execute("AcqReconProxy.AbortScan", () => AcqReconProxy.Instance.AbortScan(new FacadeProxy.Common.Models.AbortOperation(abortCause, isDeleteRawData)));
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
		{
            _logger.LogDebug($"AbortScan failed: {result?.Status.ToString()}, details: {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
            return HandleException(ex, "AbortScan");
        }
	}

	/// <summary>
	/// 设置周期消息发送时间间隔，单位：秒
	/// </summary>
	public RealtimeCommandResult SetCycleMessageInterval(uint interval)
	{
		CommandResult result = null;

		try
		{
			result = AcqReconProxy.Instance.SetCycleMessageInterval(interval);
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
		{
            _logger.LogDebug($"SetCycleMessageInterval failed: {result?.Status.ToString()}, details: {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
            return HandleException(ex, "SetCycleMessageInterval");
        }
	}

	public BaseCommandResult Resume()
	{
		CommandResult result = null;

		try
		{
			result = AcqReconProxy.Instance.Resume();
            return new BaseCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
		{
            _logger.LogDebug($"Resume failed: {result?.Status.ToString()}, details: {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
            return HandleException(ex, "Resume");
        }
	}

	private RealtimeCommandResult HandleException(Exception ex, string actionName)
	{
		var result = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };

        if (ex is ArgumentException || ex is TimeoutException || ex is BadNetworkExcption)
        {
			result.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Failed_Code, string.Format(GlobalHelper.GetErrorMessage(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Failed_Code), actionName)) };
        }
        else
        {
			result.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(GlobalHelper.GetErrorMessage(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code), actionName)) };
        }
		return result;
    }
}
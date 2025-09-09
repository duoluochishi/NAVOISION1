//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
//     2024/8/13 10:14:50    V1.0.0       朱正广
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
using NV.CT.FacadeProxy.Common.Models.DetectorTemperature;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Configuration;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class DetectorTemperatureService : IDetectorTemperatureService
{
	private readonly IRealtimeStatusProxyService _realtimeStatusProxyService;

	private readonly ILogger<DetectorTemperatureService> _logger;

	public DetectorTemperatureService(ILogger<DetectorTemperatureService> logger, IRealtimeStatusProxyService realtimeStatusProxyService)
	{
		_logger = logger;
		_realtimeStatusProxyService = realtimeStatusProxyService;
		IsTemperatureNormalStatus = _realtimeStatusProxyService.IsDetectorTemperatureNormalStatus;
		_realtimeStatusProxyService.CycleStatusChanged += OnProxyService_CycleStatusChanged;
	}

	private void OnProxyService_CycleStatusChanged(object? sender, EventArgs<Contract.Models.DeviceSystem> e)
	{
		CurrentDetector = e.Data.Detector;

		if (IsTemperatureNormalStatus != e.Data.IsDetectorTemperatureNormalStatus)
		{
			_logger.LogInformation($"Detector temperature status: {IsTemperatureNormalStatus} => {e.Data.IsDetectorTemperatureNormalStatus}");
			IsTemperatureNormalStatus = e.Data.IsDetectorTemperatureNormalStatus;
			TemperatureStatusChanged?.Invoke(this, IsTemperatureNormalStatus);
		}

		var temperatureRange = SystemConfig.DetectorTemperatureConfig.FloatRange / 10.0;
		List<Contract.Models.DetectorModule> modules = new List<Contract.Models.DetectorModule>();
		foreach (var detectorModule in CurrentDetector.DetectorModules)
		{
			var detectorTemperatureSetting = SystemConfig.DetectorTemperatureConfig.DetectorModules.FirstOrDefault(m => m.Index == detectorModule.Number);
			if (detectorTemperatureSetting is null)
			{
				continue;
			}

			for (var index = 0; index < 4; index++)
			{
				var channelTemperature = index switch
				{
					0 => detectorTemperatureSetting.Channel1Temperature / 10.0,
					1 => detectorTemperatureSetting.Channel2Temperature / 10.0,
					2 => detectorTemperatureSetting.Channel3Temperature / 10.0,
					3 => detectorTemperatureSetting.Channel4Temperature / 10.0,
					_ => 0
				};

                if (CheckTemperatureStatus(detectorModule.DetectBoards[index].Chip1Temperature / 10.0, detectorModule.DetectBoards[index].Chip2Temperature / 10.0, channelTemperature, temperatureRange))
                {
                    modules.Add(detectorModule);
                    break;
                }
            }
        }
        if (modules.Count > 0)
        {
            //_logger.LogDebug($"Detector temperature abnormal: {JsonConvert.SerializeObject(modules)}");
            DetectorTemperatureOvershot?.Invoke(this, modules);
        }
    }

	private bool CheckTemperatureStatus(double topTemperature, double bottomTemperature, double configTemperature, double range)
	{
		return Math.Abs(topTemperature - configTemperature) > range || Math.Abs(bottomTemperature - configTemperature) > range;
	}

	public bool IsTemperatureNormalStatus { get; private set; } = true;

	public event EventHandler<bool> TemperatureStatusChanged;

	public Contract.Models.Detector CurrentDetector { get; set; }

	public event EventHandler<List<Contract.Models.DetectorModule>> DetectorTemperatureOvershot;

	public BaseCommandResult SetDetectorTargetTemperature(DetectorModuleTemperatureInfo temperature)
	{
        _logger.LogInformation($"SetDetectorTargetTemperature parameter: {JsonConvert.SerializeObject(temperature)}");
        var result = DeviceInteractProxy.Instance.SetDetectorTargetTemperature(new DetectorTargetTemperature
		{
			DetectorIndex = temperature.Index,
			Channel1TargetTemperature = temperature.Channel1Temperature,
			Channel2TargetTemperature = temperature.Channel2Temperature,
			Channel3TargetTemperature = temperature.Channel3Temperature,
			Channel4TargetTemperature = temperature.Channel4Temperature,
		});
		if (result.Status != FacadeProxy.Common.Enums.CommandStatus.Success)
		{
			_logger.LogDebug($"SetDetectorTargetTemperature faild: {result?.Status.ToString()}, details: {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
			return new BaseCommandResult { Status = CTS.Enums.CommandExecutionStatus.Failure };
		}

		return new BaseCommandResult { Status = CTS.Enums.CommandExecutionStatus.Success };
	}

	public BaseCommandResult SetDetectorTargetTemperature(List<DetectorModuleTemperatureInfo> temperatures, bool continueWithError = false)
	{
		_logger.LogInformation($"SetDetectorTargetTemperatures parameter: {JsonConvert.SerializeObject(temperatures)}");
		var result = DeviceInteractProxy.Instance.SetDetectorTargetTemperature(temperatures.Select(temperature => new DetectorTargetTemperature
		{
			DetectorIndex = temperature.Index,
			Channel1TargetTemperature = temperature.Channel1Temperature,
			Channel2TargetTemperature = temperature.Channel2Temperature,
			Channel3TargetTemperature = temperature.Channel3Temperature,
			Channel4TargetTemperature = temperature.Channel4Temperature,
		}).ToArray(), continueWithError);
		if (result.Status != FacadeProxy.Common.Enums.CommandStatus.Success)
		{
			_logger.LogDebug($"SetDetectorTargetTemperature faild: {result?.Status.ToString()}, details: {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
			return new BaseCommandResult { Status = CTS.Enums.CommandExecutionStatus.Failure };
		}

		return new BaseCommandResult { Status = CTS.Enums.CommandExecutionStatus.Success };
	}
}

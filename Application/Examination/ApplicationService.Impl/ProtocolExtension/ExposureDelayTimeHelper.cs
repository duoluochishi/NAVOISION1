//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2025, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
// 修改日期           版本号       创建人
// 2025/7/15 13:25:12     V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.MPS.Configuration;
using NV.MPS.Environment;
using System.Globalization;

namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtension;

/// <summary>
/// 扫描曝光延迟时间计算类
/// </summary>
public class ExposureDelayTimeHelper
{
	public static bool CalcDelayTimeInMeasurement(IProtocolHostService _protocolHostService, IVoiceService _voiceService, MeasurementModel measurementModel)
	{
		bool flag = true;
		if (measurementModel is null
			|| measurementModel.Children.Count == 0
			|| measurementModel.Status != PerformStatus.Unperform
			|| measurementModel.Children.Any(t => t.Status != PerformStatus.Unperform))
		{
			return flag;
		}
		int minScanIntervalTime = SystemConfig.ScanningParamConfig.ScanningParam.CombinedScanIntervalTime.Min;
		int maxScanIntervalTime = SystemConfig.ScanningParamConfig.ScanningParam.CombinedScanIntervalTime.Max;
		int postPreVoiceDelayTime = SystemConfig.AcquisitionConfig.Acquisition.PostPreVoiceDelayTime.Value;
		int configMinExposureDelayTime = SystemConfig.ScanningParamConfig.ScanningParam.MinExposureDelayTime.Value;

		Dictionary<BaseModel, List<ParameterModel>> dict = new Dictionary<BaseModel, List<ParameterModel>>();
		for (int i = 0; i < measurementModel.Children.Count; i++)
		{
			ScanModel currentScan = measurementModel.Children[i];
			if (i == 0) //第一个扫描任务,判断扫描延迟时间
			{
				int minExposureDelayTime = GetScanMinExposureDelayTime(_voiceService, currentScan, configMinExposureDelayTime, postPreVoiceDelayTime);
				if (minExposureDelayTime > currentScan.ExposureDelayTime)
				{
					flag = false;
					break;
				}
			}
			if (i > 0)  //对于非第一个扫描任务判断扫描时间间隔
			{
				int intervalTime = (int)currentScan.ExposureIntervalTime;
				if (intervalTime > maxScanIntervalTime || intervalTime < minScanIntervalTime)
				{
					flag = false;
					break;
				}
				ScanModel preScan = measurementModel.Children[i - 1];
				int actualIntervalTime = GetCurrentScanIntervalTime(_voiceService, preScan, currentScan, postPreVoiceDelayTime);
				if (intervalTime < actualIntervalTime)
				{
					flag = false;
					break;
				}
				int lastScanTime = GetLastScanTime(preScan);
				int delayTime = lastScanTime + intervalTime;

				int preVoiceDelayTime = delayTime - GetCurrentScanPreVoiceTime(_voiceService, currentScan, postPreVoiceDelayTime);

				List<ParameterModel> parameterModels = new List<ParameterModel>();
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME,
					Value = delayTime.ToString(CultureInfo.InvariantCulture)
				});
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME,
					Value = preVoiceDelayTime.ToString(CultureInfo.InvariantCulture)
				});
				dict.Add(currentScan, parameterModels);
			}
		}
		if (dict.Count > 0)
		{
			_protocolHostService.SetParameters(dict);
		}
		return flag;
	}

	private static int GetScanMinExposureDelayTime(IVoiceService _voiceService, ScanModel scanModel, int minExposureDelayTime, int postPreVoiceDelayTime)
	{
		int exposureDelayTime = (int)scanModel.ExposureDelayTime;
		if (exposureDelayTime < minExposureDelayTime)
		{
			exposureDelayTime = minExposureDelayTime;
		}
		int currentScanPreVoiceLength = GetCurrentScanPreVoiceTime(_voiceService, scanModel, postPreVoiceDelayTime);
		if (exposureDelayTime < currentScanPreVoiceLength)
		{
			exposureDelayTime = currentScanPreVoiceLength;
		}
		return exposureDelayTime;
	}

	private static int GetCurrentScanIntervalTime(IVoiceService _voiceService, ScanModel lastScanModel, ScanModel currentScanModel, int postPreVoiceDelayTime)
	{
		int minScanIntervalTime = 0;
		if (lastScanModel.IsVoiceSupported && lastScanModel.PostVoiceId > 0)  //0:表示不播放语音
		{
			var model = _voiceService.GetVoiceInfo(lastScanModel.PostVoiceId.ToString());
			if (model is not null)
			{
				minScanIntervalTime += UnitConvert.Second2Microsecond(model.VoiceLength);
			}
		}
		if (currentScanModel.IsVoiceSupported && currentScanModel.PreVoiceId > 0)  //0:表示不播放语音
		{
			var model = _voiceService.GetVoiceInfo(currentScanModel.PreVoiceId.ToString());
			if (model is not null)
			{
				minScanIntervalTime += UnitConvert.Second2Microsecond(model.VoiceLength) + postPreVoiceDelayTime;
			}
		}
		return minScanIntervalTime;
	}

	private static int GetLastScanTime(ScanModel lastScanModel, int lastExposureDelayTime = 0)
	{
		if (lastExposureDelayTime == 0)
		{
			return (int)(lastScanModel.ExposureDelayTime + UnitConvert.Second2Microsecond(ScanTimeHelper.GetScanTime(lastScanModel)));
		}
		else
		{
			return lastExposureDelayTime + UnitConvert.Second2Microsecond(ScanTimeHelper.GetScanTime(lastScanModel));
		}
	}

	public static void CorrectDelayTimeMeasurement(IProtocolHostService _protocolHostService, IVoiceService _voiceService, string scanID)
	{
		var item = _protocolHostService.Models.FirstOrDefault(t => t.Scan is not null && t.Scan.Descriptor.Id.Equals(scanID));
		if (item.Scan.Status != PerformStatus.Unperform || item.Scan.Parent is not MeasurementModel)
		{
			return;
		}
		var measurementModel = item.Scan.Parent;
		int postPreVoiceDelayTime = SystemConfig.AcquisitionConfig.Acquisition.PostPreVoiceDelayTime.Value;
		int minScanIntervalTime = SystemConfig.ScanningParamConfig.ScanningParam.CombinedScanIntervalTime.Min;
		int maxScanIntervalTime = SystemConfig.ScanningParamConfig.ScanningParam.CombinedScanIntervalTime.Max;
		int configMinExposureDelayTime = SystemConfig.ScanningParamConfig.ScanningParam.MinExposureDelayTime.Value;

		int lastExposureDelayTime = 0;
		Dictionary<BaseModel, List<ParameterModel>> dict = new Dictionary<BaseModel, List<ParameterModel>>();
		for (int i = 0; i < measurementModel.Children.Count; i++)
		{
			ScanModel currentScan = measurementModel.Children[i];
			if (i == 0) //第一个扫描任务,判断扫描延迟时间
			{
				int minExposureDelayTime = GetScanMinExposureDelayTime(_voiceService, currentScan, configMinExposureDelayTime, postPreVoiceDelayTime);
				if (minExposureDelayTime > currentScan.ExposureDelayTime)
				{
					lastExposureDelayTime = minExposureDelayTime;
				}
				else
				{
					lastExposureDelayTime = (int)currentScan.ExposureDelayTime;
				}

				int preVoiceDelayTime = lastExposureDelayTime - GetCurrentScanPreVoiceTime(_voiceService, currentScan, postPreVoiceDelayTime);

				List<ParameterModel> parameterModels = new List<ParameterModel>();
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME,
					Value = lastExposureDelayTime.ToString(CultureInfo.InvariantCulture)
				});
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME,
					Value = preVoiceDelayTime.ToString(CultureInfo.InvariantCulture)
				});
				dict.Add(currentScan, parameterModels);
			}
			if (i > 0)  //对于非第一个扫描任务判断扫描时间间隔
			{
				int intervalTime = (int)currentScan.ExposureIntervalTime;
				ScanModel preScan = measurementModel.Children[i - 1];
				int actualIntervalTime = GetCurrentScanIntervalTime(_voiceService, preScan, currentScan, postPreVoiceDelayTime);
				if (intervalTime < actualIntervalTime)
				{
					intervalTime = actualIntervalTime;
				}
				if (intervalTime < minScanIntervalTime)
				{
					intervalTime = minScanIntervalTime;
				}
				if (intervalTime > maxScanIntervalTime)
				{
					intervalTime = maxScanIntervalTime;
				}
				int lastScanTime = GetLastScanTime(preScan, lastExposureDelayTime);
				int delayTime = lastScanTime + intervalTime;
				int preVoiceDelayTime = delayTime - GetCurrentScanPreVoiceTime(_voiceService, currentScan, postPreVoiceDelayTime);

				List<ParameterModel> parameterModels = new List<ParameterModel>();
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME,
					Value = delayTime.ToString(CultureInfo.InvariantCulture)
				});
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_EXPOSURE_INTERVAL_TIME,
					Value = intervalTime.ToString(CultureInfo.InvariantCulture)
				});
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME,
					Value = preVoiceDelayTime.ToString(CultureInfo.InvariantCulture)
				});
				lastExposureDelayTime = delayTime;
				dict.Add(currentScan, parameterModels);
			}
		}
		if (dict.Count > 0)
		{
			_protocolHostService.SetParameters(dict);
		}
	}

	private static int GetCurrentScanPreVoiceTime(IVoiceService _voiceService, ScanModel scanModel, int postPreVoiceDelayTime)
	{
		int voiceTime = 0;
		if (scanModel.IsVoiceSupported && scanModel.PreVoiceId > 0)
		{
			var voiceModel = _voiceService.GetVoiceInfo(scanModel.PreVoiceId.ToString());
			if (voiceModel is not null)
			{
				voiceTime = UnitConvert.Second2Microsecond(voiceModel.VoiceLength) + postPreVoiceDelayTime;
			}
		}
		return voiceTime;
	}
}
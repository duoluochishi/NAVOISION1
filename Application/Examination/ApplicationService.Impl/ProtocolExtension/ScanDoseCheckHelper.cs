//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2025/06/17 11:26:03           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Environment;
using System.Globalization;

namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtension;

public static class ScanDoseCheckHelper
{
	/// <summary>
	/// 获取Measurement下所有Unperform扫描任务的Dose预计值及累计估计值
	/// </summary>
	/// <param name="doseEstimateService"></param>
	/// <param name="protocolHostService"></param>
	/// <param name="measurementModel"></param>
	public static void GetDoseEstimatedInfoByUnperformMeasurement(IDoseEstimateService doseEstimateService, IProtocolHostService protocolHostService, MeasurementModel measurementModel)
	{
		if (measurementModel is null || measurementModel.Status != PerformStatus.Unperform || measurementModel.Children.Count == 0)
		{
			return;
		}
		//获取扫描的估计值
		if (measurementModel.Children.Count == 1)
		{
			foreach (ScanModel scanModel in measurementModel.Children)
			{
				GetDoseEstimatedInfoByUnperformScan(doseEstimateService, protocolHostService, scanModel);
			}
		}
		//获取连扫的剂量估计值
		if (measurementModel.Children.Count > 1)
		{
			GetDoseEstimatedInfoByUnperformMeasurementAutoScan(doseEstimateService, protocolHostService, measurementModel);
		}
	}

	/// <summary>
	///	连扫的Dose预计值及估计累计值
	/// </summary>
	/// <param name="doseEstimateService"></param>
	/// <param name="protocolHostService"></param>
	/// <param name="measurementModel"></param>
	private static void GetDoseEstimatedInfoByUnperformMeasurementAutoScan(IDoseEstimateService doseEstimateService, IProtocolHostService protocolHostService, MeasurementModel measurementModel)
	{
		if (measurementModel.Children.Any(t => t.Status != PerformStatus.Unperform))
		{
			return;
		}
		for (int i = 0; i < measurementModel.Children.Count; i++)
		{
			GetDoseEstimatedInfoByUnperformScan(doseEstimateService, protocolHostService, measurementModel.Children[i]);
			if (i > 0)
			{
				float CTDIs = measurementModel.Children[i - 1].AccumulatedDoseEstimatedCTDI + measurementModel.Children[i].DoseEstimatedCTDI;
				float DLPs = measurementModel.Children[i - 1].AccumulatedDoseEstimatedDLP + measurementModel.Children[i].DoseEstimatedDLP;
				List<ParameterModel> parameterModels = new List<ParameterModel>
					{
						new ParameterModel
						{
							Name = ProtocolParameterNames.SCAN_DOSE_ACCUMULATED_ESTIMATED_CTDI,
							Value = CTDIs.ToString(CultureInfo.InvariantCulture)
						},
						new ParameterModel
						{
							Name = ProtocolParameterNames.SCAN_DOSE_ACCUMULATED_ESTIMATED_DLP,
							Value = DLPs.ToString(CultureInfo.InvariantCulture)
						}
					};
				protocolHostService.SetParameters(measurementModel.Children[i], parameterModels);
			}
		}
	}

	/// <summary>
	/// 扫描的Dose估计值及累计值
	/// </summary>
	/// <param name="doseEstimateService"></param>
	/// <param name="protocolHostService"></param>
	/// <param name="scanModel"></param>
	public static void GetDoseEstimatedInfoByUnperformScan(IDoseEstimateService doseEstimateService, IProtocolHostService protocolHostService, ScanModel scanModel)
	{
		if (scanModel is null || scanModel.Status != PerformStatus.Unperform)
		{
			return;
		}
		EstimateDoseInfo estimateDoseInfo = doseEstimateService.GetEstimateDoseInfo(GetDoseEstimateParam(scanModel));
		if (estimateDoseInfo is not null && estimateDoseInfo.IsEstimateSuccess)
		{
			float CTDIs = estimateDoseInfo.CTDIvol + GetBerforPerformedEffectiveCTDIsByScan(protocolHostService, scanModel);
			float DLPs = estimateDoseInfo.DLP + GetBerforPerformedEffectiveDLPsByScan(protocolHostService, scanModel);
			List<ParameterModel> parameterModels = new List<ParameterModel>
			{
				new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_DOSE_ESTIMATED_DLP,
					Value = estimateDoseInfo.DLP.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_DOSE_ESTIMATED_CTDI,
					Value = estimateDoseInfo.CTDIvol.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_DOSE_ACCUMULATED_ESTIMATED_CTDI,
					Value = CTDIs.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_DOSE_ACCUMULATED_ESTIMATED_DLP,
					Value = DLPs.ToString(CultureInfo.InvariantCulture)
				}
			};
			protocolHostService.SetParameters(scanModel, parameterModels);
		}
	}

	/// <summary>
	/// 根据扫描模型构造Dose计算参数模型
	/// </summary>
	/// <param name="scanModel"></param>
	/// <returns></returns>
	private static DoseEstimateParam GetDoseEstimateParam(ScanModel scanModel)
	{
		DoseEstimateParam doseEstimateParam = new DoseEstimateParam();
		doseEstimateParam.BodyPart = scanModel.BodyPart;
		doseEstimateParam.FramePerCycle = (int)scanModel.FramesPerCycle;
		doseEstimateParam.ScanOption = scanModel.ScanOption;
		doseEstimateParam.Pitch = UnitConvert.ReduceHundred((float)scanModel.Pitch);
		doseEstimateParam.ExposureMode = scanModel.ExposureMode;
		doseEstimateParam.ExposureTime = scanModel.ExposureTime;
		doseEstimateParam.KV = (int)scanModel.Kilovolt[0];
		doseEstimateParam.MA = (int)scanModel.Milliampere[0];

		doseEstimateParam.ScanLength = UnitConvert.Micron2Millimeter(scanModel.ScanLength);
		doseEstimateParam.TableFeed = UnitConvert.Micron2Millimeter(scanModel.TableFeed);
		doseEstimateParam.CollimatorOpenWidth = scanModel.CollimatorZ;

		return doseEstimateParam;
	}

	/// <summary>
	/// 计算当前扫描任务的累计使用CTDIs
	/// </summary>
	/// <param name="protocolHostService"></param>
	/// <param name="scanModel"></param>
	/// <returns></returns>
	public static float GetEffectiveCTDIsByScan(IProtocolHostService protocolHostService, ScanModel scanModel)
	{
		if (scanModel is null || scanModel.Status != PerformStatus.Performed || protocolHostService.Models is null || protocolHostService.Models.Count == 0)
		{
			return 0;
		}
		float CTDIs = 0;
		var model = protocolHostService.Models.FirstOrDefault(t => t.Scan.Descriptor.Id.Equals(scanModel.Descriptor.Id));
		int index = protocolHostService.Models.IndexOf(model);
		for (int i = 0; i <= index; i++)
		{
			if (protocolHostService.Models[i].Scan.Status == PerformStatus.Performed)
			{
				CTDIs += protocolHostService.Models[i].Scan.DoseEffectiveCTDI;
			}
			if (protocolHostService.Models[i].Scan.Descriptor.Id == scanModel.Descriptor.Id)
			{
				break;
			}
		}
		return CTDIs;
	}

	public static float GetBerforPerformedEffectiveCTDIsByScan(IProtocolHostService protocolHostService, ScanModel scanModel)
	{
		if (scanModel is null || scanModel.Status != PerformStatus.Unperform || protocolHostService.Models is null || protocolHostService.Models.Count == 0)
		{
			return 0;
		}
		float CTDIs = 0;
		var model = protocolHostService.Models.FirstOrDefault(t => t.Scan.Descriptor.Id.Equals(scanModel.Descriptor.Id));
		int index = protocolHostService.Models.IndexOf(model);
		for (int i = 0; i < index; i++)
		{
			if (protocolHostService.Models[i].Scan.Status == PerformStatus.Performed && protocolHostService.Models[i].Scan.Descriptor.Id != scanModel.Descriptor.Id)
			{
				CTDIs += protocolHostService.Models[i].Scan.DoseEffectiveCTDI;
			}
		}
		return CTDIs;
	}

	/// <summary>
	///	计算当前扫描任务的累计使用DLPs
	/// </summary>
	/// <param name="protocolHostService"></param>
	/// <param name="scanModel"></param>
	/// <returns></returns>
	public static float GetEffectiveDLPsByScan(IProtocolHostService protocolHostService, ScanModel scanModel)
	{
		if (scanModel is null || scanModel.Status != PerformStatus.Performed || protocolHostService.Models is null || protocolHostService.Models.Count == 0)
		{
			return 0;
		}
		float DLPs = 0;
		var model = protocolHostService.Models.FirstOrDefault(t => t.Scan.Descriptor.Id.Equals(scanModel.Descriptor.Id));
		int index = protocolHostService.Models.IndexOf(model);
		for (int i = 0; i <= index; i++)
		{
			if (protocolHostService.Models[i].Scan.Status == PerformStatus.Performed)
			{
				DLPs += protocolHostService.Models[i].Scan.DoseEffectiveDLP;
			}
			if (protocolHostService.Models[i].Scan.Descriptor.Id == scanModel.Descriptor.Id)
			{
				break;
			}
		}
		return DLPs;
	}

	public static float GetBerforPerformedEffectiveDLPsByScan(IProtocolHostService protocolHostService, ScanModel scanModel)
	{
		if (scanModel is null || scanModel.Status != PerformStatus.Unperform || protocolHostService.Models is null || protocolHostService.Models.Count == 0)
		{
			return 0;
		}
		float DLPs = 0;
		var model = protocolHostService.Models.FirstOrDefault(t => t.Scan.Descriptor.Id.Equals(scanModel.Descriptor.Id));
		int index = protocolHostService.Models.IndexOf(model);
		for (int i = 0; i < index; i++)
		{
			if (protocolHostService.Models[i].Scan.Status == PerformStatus.Performed && protocolHostService.Models[i].Scan.Descriptor.Id != scanModel.Descriptor.Id)
			{
				DLPs += protocolHostService.Models[i].Scan.DoseEffectiveDLP;
			}
		}
		return DLPs;
	}
}
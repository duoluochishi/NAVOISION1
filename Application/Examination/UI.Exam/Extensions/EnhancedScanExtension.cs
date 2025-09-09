//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2025/8/11 14:28:29           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;
using PatientPosition = NV.CT.FacadeProxy.Common.Enums.PatientPosition;

namespace NV.CT.UI.Exam.ViewModel;

public class EnhancedScanExtension
{
	private readonly IProtocolHostService _protocolHostService;
	private readonly ILogger<EnhancedScanExtension> _logger;
	private bool isAutoChange = false;

	public EnhancedScanExtension(IProtocolHostService protocolHostService,
		ILogger<EnhancedScanExtension> logger)
	{
		_protocolHostService = protocolHostService;
		_logger = logger;
		_protocolHostService.ParameterChanged -= ProtocolHostService_ParameterChanged;
		_protocolHostService.ParameterChanged += ProtocolHostService_ParameterChanged;
	}

	private void ProtocolHostService_ParameterChanged(object? sender, EventArgs<(BaseModel baseModel, List<string> list)> e)
	{
		if (e is null || e.Data.baseModel is null)
		{
			return;
		}
		if (!Global.IsGoing
			&& !isAutoChange
			&& _protocolHostService.Instance.IsEnhanced
			&& e.Data.baseModel is ScanModel scan
			&& scan.Status == PerformStatus.Unperform
			&& (scan.ScanOption == ScanOption.Helical
				|| scan.ScanOption == ScanOption.NVTestBolusBase
				|| scan.ScanOption == ScanOption.TestBolus
				|| scan.ScanOption == ScanOption.NVTestBolus
				|| scan.Parent.Children.Count > 1)
			&& (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION)) is not null
			|| e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION)) is not null
			|| e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_TABLE_DIRECTION)) is not null))
		{
			try
			{
				isAutoChange = true;
				ParameterSynchronization(scan);
				isAutoChange = false;
			}
			catch (Exception ex)
			{
				_logger.LogError("Synchronization parameter error is:" + ex.Message + ", StackTrace is :" + ex.StackTrace);
			}
		}
	}

	private void ParameterSynchronization(ScanModel baseTomoScan)
	{
		var rtdModel = baseTomoScan.Children.FirstOrDefault(t => t.IsRTD);
		if (rtdModel is null)
		{
			return;
		}
		ImageOrders imageOrders = rtdModel.ImageOrder;
		Dictionary<BaseModel, List<ParameterModel>> resultDic = new Dictionary<BaseModel, List<ParameterModel>>();
		if (baseTomoScan.Parent.Children.Count == 1)
		{
			var items = _protocolHostService.Models.FirstOrDefault(t => t.Scan is not null && t.Scan.Descriptor.Id.Equals(baseTomoScan.Descriptor.Id));
			int index = _protocolHostService.Models.IndexOf(items);
			for (int i = index + 1; i < _protocolHostService.Models.Count; i++)
			{
				var tomo = _protocolHostService.Models[i].Scan;
				if (tomo.ScanOption is ScanOption.Surview or ScanOption.DualScout)
				{
					continue;
				}
				if (tomo.Status == PerformStatus.Unperform)
				{
					uint scanLength = baseTomoScan.ScanLength;
					if (baseTomoScan.ScanOption == ScanOption.NVTestBolus || baseTomoScan.ScanOption == ScanOption.NVTestBolusBase || baseTomoScan.ScanOption == ScanOption.TestBolus)
					{
						scanLength = tomo.ScanLength;
					}
					foreach (var item in AdjustTomoScanByPreScanParameters(tomo, baseTomoScan.TableDirection, baseTomoScan.ReconVolumeStartPosition, scanLength, baseTomoScan.Parent.Parent.PatientPosition, imageOrders))
					{
						resultDic.Add(item.Key, item.Value);
					}
				}
			}
		}
		else
		{
			var measurement = baseTomoScan.Parent;
			var isEnhancedModel = measurement.Children.FirstOrDefault(t => t.IsEnhanced);  //判断连扫是否增强连扫任务
			if (isEnhancedModel is not null)
			{
				int index = measurement.Children.IndexOf(baseTomoScan);
				for (int i = index + 1; i < measurement.Children.Count; i++)
				{
					var tomo = measurement.Children[i];
					if (tomo.Status == PerformStatus.Unperform)
					{
						uint scanLength = baseTomoScan.ScanLength;
						if (baseTomoScan.ScanOption == ScanOption.NVTestBolus || baseTomoScan.ScanOption == ScanOption.NVTestBolusBase || baseTomoScan.ScanOption == ScanOption.TestBolus)
						{
							scanLength = tomo.ScanLength;
						}
						foreach (var item in AdjustTomoScanByPreScanParameters(tomo, baseTomoScan.TableDirection, baseTomoScan.ReconVolumeStartPosition, scanLength, baseTomoScan.Parent.Parent.PatientPosition, imageOrders))
						{
							resultDic.Add(item.Key, item.Value);
						}
					}
				}
			}
		}

		if (resultDic.Count > 0)
		{
			_protocolHostService.SetParameters(resultDic);
		}
	}

	private Dictionary<BaseModel, List<ParameterModel>> AdjustTomoScanByPreScanParameters(ScanModel currentTomoScan,
		TableDirection baseTableDirection,
		int baseReconVolumeStartPosition,
		uint baseScanLength,
		PatientPosition pp,
		ImageOrders imageOrder)
	{
		//todo: 扫描长度联动接口修改完毕后，直接使用扫描长度修改接口。
		Dictionary<BaseModel, List<ParameterModel>> resultDic = new Dictionary<BaseModel, List<ParameterModel>>();
		var scanStart = baseReconVolumeStartPosition;
		var tomoRTDRecon = currentTomoScan.Children.FirstOrDefault(x => x.IsRTD);
		if (tomoRTDRecon is null)
		{
			return resultDic;
		}
		//校正扫描长度	
		var correctedLength = currentTomoScan.ScanOption is ScanOption.Axial ?
			(int)currentTomoScan.ScanLength :
			ScanLengthCorrectionHelper.GetCorrectedHelicalScanLength((int)baseScanLength, tomoRTDRecon.SliceThickness);

		//获取BaseScan的tableDirection，然后使用改TableDirection作为后续呗修改扫描的扫描方向
		//注意：RTD的ImageOrder需要进行对应的修改
		var baseScanDirection = baseTableDirection;
		var correctedScanEnd = baseScanDirection is TableDirection.In ? scanStart - correctedLength : scanStart + correctedLength;

		//设置的scan参数
		resultDic.Add(currentTomoScan, new List<ParameterModel>());
		resultDic[currentTomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_TABLE_DIRECTION, Value = baseScanDirection.ToString() });
		//小剂量基底扫描跟小剂量测试扫描的起始点位置在同一个点上
		if (currentTomoScan.ScanOption == ScanOption.NVTestBolusBase || currentTomoScan.ScanOption == ScanOption.NVTestBolus || currentTomoScan.ScanOption == ScanOption.TestBolus)
		{
			resultDic[currentTomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_LENGTH, Value = 0.ToString() });
			resultDic[currentTomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = scanStart.ToString() });
			resultDic[currentTomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = scanStart.ToString() });
		}
		else
		{
			resultDic[currentTomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_LENGTH, Value = correctedLength.ToString() });
			resultDic[currentTomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, Value = scanStart.ToString() });
			resultDic[currentTomoScan].Add(new ParameterModel() { Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, Value = correctedScanEnd.ToString() });
		}
		AdjustReconByPreScanParameters(resultDic, currentTomoScan, scanStart, correctedScanEnd, pp, imageOrder);

		return resultDic;
	}

	private void AdjustReconByPreScanParameters(Dictionary<BaseModel, List<ParameterModel>> resultDic,
		ScanModel currentTomoScan,
		int scanStart,
		int correctedScanEnd,
		PatientPosition pp,
		ImageOrders imageOrder)
	{
		//重建参数单位mm
		var pos1 = UnitConvert.Micron2Millimeter((double)scanStart);
		var pos2 = UnitConvert.Micron2Millimeter((double)correctedScanEnd);
		//遍历修改重建参数：
		foreach (var recon in currentTomoScan.Children)
		{
			resultDic.Add(recon, new List<ParameterModel>());
			double[] posResult;
			if (recon.IsRTD)   //RTD的imageOrder跟随变更
			{
				posResult = ScanReconCoordinateHelper.GetTomoDefaultFirstLastCenterByScanRange(pp, imageOrder, pos1, pos2);
				resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_IMAGE_ORDER, Value = imageOrder.ToString() });
			}
			else
			{
				posResult = ScanReconCoordinateHelper.GetTomoDefaultFirstLastCenterByScanRange(pp, recon.ImageOrder, pos1, pos2);
			}
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_FIRST_X, Value = ((int)UnitConvert.Millimeter2Micron(posResult[0])).ToString() });
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y, Value = ((int)UnitConvert.Millimeter2Micron(posResult[1])).ToString() });
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z, Value = ((int)UnitConvert.Millimeter2Micron(posResult[2])).ToString() });
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_X, Value = ((int)UnitConvert.Millimeter2Micron(posResult[3])).ToString() });
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_Y, Value = ((int)UnitConvert.Millimeter2Micron(posResult[4])).ToString() });

			var dirResult = ScanReconCoordinateHelper.GetDefaultTomoReconOrientation(pp);
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X, Value = ((int)dirResult[0]).ToString() });
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y, Value = ((int)dirResult[1]).ToString() });
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z, Value = ((int)dirResult[2]).ToString() });
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X, Value = ((int)dirResult[3]).ToString() });
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y, Value = ((int)dirResult[4]).ToString() });
			resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z, Value = ((int)dirResult[5]).ToString() });

			if (currentTomoScan.ScanOption == ScanOption.NVTestBolusBase || currentTomoScan.ScanOption == ScanOption.NVTestBolus || currentTomoScan.ScanOption == ScanOption.TestBolus)
			{
				resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)UnitConvert.Millimeter2Micron(posResult[2])).ToString() });
			}
			else
			{
				resultDic[recon].Add(new ParameterModel() { Name = ProtocolParameterNames.RECON_CENTER_LAST_Z, Value = ((int)UnitConvert.Millimeter2Micron(posResult[5])).ToString() });
			}
		}
	}
}
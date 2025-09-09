//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2025/2/19 9:28:29           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract;
using NV.MPS.Environment;

namespace NV.CT.UI.Exam.Extensions;

public class AutoReconHelper
{
	public static void StartMeasurementAutoReconTasksByRtdRecon(IOfflineConnectionService _offlineConnectionService,
		IProtocolHostService _protocolHostService,
		IRawDataService _rawDataService,
		IStudyHostService _studyHostService,
		IOfflineReconService _offlineReconService,
		string scanID,
		string reconID,
		ILogger _logger)
	{
		if (string.IsNullOrEmpty(scanID) || string.IsNullOrEmpty(reconID))
		{
			return;
		}
		if (!_offlineConnectionService.IsConnected)
		{
			_logger.LogDebug($"Changed IsAutoRecon offlineConnectionService is  disconnected!");
			return;
		}
		var rtdRecon = ProtocolHelper.GetRecon(_protocolHostService.Instance, scanID, reconID);
		if (rtdRecon is null || rtdRecon.Parent is null || !rtdRecon.IsRTD || rtdRecon.Status != PerformStatus.Performed)
		{
			return;
		}
		ScanModel scanModel = rtdRecon.Parent;
		var rawList = _rawDataService.GetRawDataListByStudyId(_studyHostService.StudyId);
		if (!RuntimeConfig.IsDevelopment && (rawList is null || rawList.Count == 0))
		{
			_logger.LogDebug("No raw data the study has no raw data!");
			return;
		}
		var currentRaw = rawList.FirstOrDefault(r => r.ScanId == scanModel.Descriptor.Id);
		if (!RuntimeConfig.IsDevelopment && (currentRaw is null || string.IsNullOrEmpty(currentRaw.Path)))
		{
			_logger.LogDebug("No raw data the scan of the study has no raw data!");
			return;
		}
		if (!RuntimeConfig.IsDevelopment && currentRaw is not null && !Directory.Exists(currentRaw.Path))
		{
			_logger.LogDebug("No raw data the path of the raw data is not exists!");
			return;
		}
		MeasurementModel measurementModel = scanModel.Parent;
		foreach (ScanModel scan in measurementModel.Children)
		{
			var rtdReconed = scanModel.Children.FirstOrDefault(t => t.IsRTD && t.Status == PerformStatus.Performed);
			if (rtdReconed is null)
			{
				continue;
			}
			foreach (ReconModel reconModel in scanModel.Children)
			{
				if (!reconModel.IsRTD
					&& reconModel.Status == PerformStatus.Unperform
					&& reconModel.IsAutoRecon)
				{
					StartAllReconTasks(_studyHostService, _offlineReconService, scan, reconModel, _logger);
				}
			}
		}
	}

	private static void StartAllReconTasks(IStudyHostService _studyHostService,
		IOfflineReconService _offlineReconService,
		ScanModel scanModel,
		ReconModel reconModel,
		ILogger _logger)
	{
		var offlineTaskInfo = _offlineReconService.GetReconTask(_studyHostService.StudyId, scanModel.Descriptor.Id, reconModel.Descriptor.Id);
		if (offlineTaskInfo is not null && !string.IsNullOrEmpty(offlineTaskInfo.TaskId))
		{
			_logger.LogDebug($"Changed IsAutoRecon the recon task is exists: ({scanModel.Descriptor.Id}, {reconModel.Descriptor.Id}) => {JsonConvert.SerializeObject(reconModel)}");
			return;
		}
		string mesage = string.Empty;
		if (!ReconCalculateExtension.ReconParamCalculate(reconModel, out mesage))
		{
			_logger.LogDebug($"Changed IsAutoRecon ReconParamCalculate fiald:{mesage}=> ({scanModel.Descriptor.Id}, {reconModel.Descriptor.Id}) => {JsonConvert.SerializeObject(reconModel)}");
			return;
		}
		_logger.LogDebug($"Changed IsAutoRecon CreateOfflineReconTask: ({_studyHostService.StudyId}, {scanModel.Descriptor.Id}, {reconModel.Descriptor.Id}) => {JsonConvert.SerializeObject(reconModel)}");
		var offlineCommand = _offlineReconService.CreateReconTask(_studyHostService.StudyId, scanModel.Descriptor.Id, reconModel.Descriptor.Id);
		if (offlineCommand is not null && offlineCommand.Status != CommandExecutionStatus.Success)
		{
			_logger.LogDebug($"Changed IsAutoRecon CreateOfflineReconTask exception: ({_studyHostService.StudyId}, {scanModel.Descriptor.Id}, {reconModel.Descriptor.Id}) => {JsonConvert.SerializeObject(offlineCommand)}");
			return;
		}
	}
}
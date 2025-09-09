//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Logging;

using NV.CT.Alg.ScanReconValidation.Model;
using NV.CT.Alg.ScanReconValidation.Scan.RawData;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;

using ScanTaskModel = NV.CT.DatabaseService.Contract.Models.ScanTaskModel;

namespace NV.CT.Examination.ApplicationService.Impl;

public class ScanControlHandler : IHostedService
{
	private readonly ILogger<ScanControlHandler> _logger;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IStudyHostService _studyHostService;
	private readonly IScanTaskService _scanTaskService;
	private readonly IScanStatusService _scanStatusService;
	private readonly IMeasurementStatusService _measurementStatusService;
	private readonly IRawDataService _rawDataService;
	private readonly IRawDataValidator _rawDataValidator;

	public ScanControlHandler(ILogger<ScanControlHandler> logger, IProtocolHostService protocolHostService, IScanStatusService scanStatusService, IMeasurementStatusService measurementStatusService, IProtocolPerformStatusService performStatusService, IStudyHostService studyHostService, IScanTaskService scanTaskService, IRawDataService rawDataService, IRawDataValidator rawDataValidator)
	{
		_logger = logger;
		_rawDataValidator = rawDataValidator;
		_scanTaskService = scanTaskService;
		_studyHostService = studyHostService;
		_scanStatusService = scanStatusService;
		_protocolHostService = protocolHostService;
		_measurementStatusService = measurementStatusService;
		_rawDataService = rawDataService;
	}

	private void ScanStatusService_ScanStarted(object? sender, CTS.EventArgs<string> e)
	{
		_logger.LogInformation($"ScanStatusService.ScanStarted: {e.Data}");

		var item = _protocolHostService.Models.FirstOrDefault(m => m.Scan.Descriptor.Id == e.Data);

		if (item.Scan is null)
		{
			_logger.LogWarning($"ScanStatusService.ScanStarted: Scan ({e.Data}) is not exist.");
			return;
		}

		_protocolHostService.SetPerformStatus(item.Scan, PerformStatus.Performing);
	}

	private void ScanStatusService_ScanCancelled(object? sender, CTS.EventArgs<(string MeasurementId, string ScanId, bool IsUserCancelled)> e)
	{
		_logger.LogInformation($"ScanStatusService.ScanCanceled: {e.Data.MeasurementId}, {e.Data.ScanId}");

		var currentMeasurement = _protocolHostService.Models.Where(s => s.Measurement.Descriptor.Id == e.Data.MeasurementId).Select(m => m.Measurement).FirstOrDefault();
		if (currentMeasurement is null)
		{
            _logger.LogWarning($"ScanStatusService.ScanCanceled: Measurement ({e.Data.MeasurementId}, {e.Data.ScanId}) is not exist.");
            return;
        }

		var currentScan = _protocolHostService.Models.Where(s => s.Scan.Descriptor.Id == e.Data.ScanId).Select(m => m.Scan).FirstOrDefault();
        if (currentScan is null)
        {
            _logger.LogWarning($"ScanStatusService.ScanCanceled: Scan ({e.Data.MeasurementId}, {e.Data.ScanId}) is not exist.");
            return;
        }

		if (currentMeasurement.Children.Count == 1)
		{
            _protocolHostService.SetPerformStatus(currentScan, PerformStatus.Unperform);
            _measurementStatusService.RaiseMeasurementCancelled(currentMeasurement.Descriptor.Id);
        }
		else
		{
			var firstScan = currentMeasurement.Children.FirstOrDefault();

			if (firstScan.Descriptor.Id == currentScan.Descriptor.Id)
			{
                foreach (var childScan in currentMeasurement.Children)
				{
                    _protocolHostService.SetPerformStatus(childScan, PerformStatus.Unperform);

					var childRecons = childScan.Children.Where(r => r.IsRTD).ToList();
					foreach(var childRecon in childRecons)
					{
                        _protocolHostService.SetPerformStatus(childRecon, PerformStatus.Unperform);
                    }
                }
                _measurementStatusService.RaiseMeasurementCancelled(currentMeasurement.Descriptor.Id);
            }
            else
            {
                var reasonType = e.Data.IsUserCancelled ? FailureReasonType.UserCancellation : FailureReasonType.SystemCancellation;
                foreach (var childScan in currentMeasurement.Children)
				{
					if (childScan.Status == PerformStatus.Performed) continue;
					_protocolHostService.SetPerformStatus(childScan, PerformStatus.Performed, reasonType);

					var childRecons = childScan.Children.Where(r => r.IsRTD).ToList();
                    foreach (var childRecon in childRecons)
                    {
                        _protocolHostService.SetPerformStatus(childRecon, PerformStatus.Performed, reasonType);
                    }
                }
                _measurementStatusService.RasiseMeasurementAborted(currentMeasurement.Descriptor.Id, true, currentScan.Descriptor.Id, null, reasonType);
            }
        }
        UpdateProtocol();
    }

    private void ScanStatusService_ScanAborted(object? sender, CTS.EventArgs<(string ScanId, RealDoseInfo DoseInfo, bool IsEmergencyStopped, bool IsUserCancelled)> e)
	{
		_logger.LogInformation($"ScanStatusService.ScanAborted: {JsonConvert.SerializeObject(e.Data)}");

		var item = _protocolHostService.Models.FirstOrDefault(m => m.Scan.Descriptor.Id == e.Data.ScanId);
		if (item.Scan is null)
		{
			_logger.LogInformation($"ScanStatusService.ScanAborted: Scan ({e.Data.ScanId}) is not exist.");
			return;
		}

		if (e.Data.DoseInfo != null)
		{
			var parameters = new List<ParameterModel>
			{
                new ParameterModel { Name = ProtocolParameterNames.SCAN_ACTUAL_SCAN_TIME, Value =((uint)(e.Data.DoseInfo.TotalExposureTime * 1000)).ToString() },
                new ParameterModel { Name = ProtocolParameterNames.SCAN_ACTUAL_SCAN_LENGTH, Value = ((uint)(e.Data.DoseInfo.ScanLength * 1000)).ToString() }
            };

			if (!IsInfinityOrNaN(e.Data.DoseInfo.CTDIvol))
			{
				parameters.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_CTDI, Value = e.Data.DoseInfo.CTDIvol.ToString() });
                parameters.Add(new ParameterModel { Name = ProtocolParameterNames.DOSE_INFO_CTDI, Value = e.Data.DoseInfo.CTDIvol.ToString() });
            }

			if (!IsInfinityOrNaN(e.Data.DoseInfo.mAs))
			{
                parameters.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_MA_S, Value = e.Data.DoseInfo.mAs.ToString() });
            }

            if (!IsInfinityOrNaN(e.Data.DoseInfo.DLP))
            {
                parameters.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_DLP, Value = e.Data.DoseInfo.DLP.ToString() });
            }

			if (e.Data.DoseInfo.TubeDoses is not null && e.Data.DoseInfo.TubeDoses.Count > 0)
			{
				parameters.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_KVP,
					Value = (e.Data.DoseInfo.TubeDoses.Sum(t => t.KVP) / e.Data.DoseInfo.TubeDoses.Count).ToString()
				});
				parameters.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_MEANMA,
					Value = (e.Data.DoseInfo.TubeDoses.Sum(t => t.MeanMA) / e.Data.DoseInfo.TubeDoses.Count).ToString()
				});
			}

			_protocolHostService.SetParameters(item.Scan, parameters);
			InsertScanTask(item, e.Data.DoseInfo);
		}

		var failureReason = e.Data.IsUserCancelled ? FailureReasonType.UserCancellation : (e.Data.IsEmergencyStopped ? FailureReasonType.SystemCancellation : FailureReasonType.SystemError);

		if (item.Measurement.Children.Count == 1)
		{
            _protocolHostService.SetPerformStatus(item.Scan, PerformStatus.Performed, failureReason);
            _measurementStatusService.RasiseMeasurementAborted(item.Measurement.Descriptor.Id, true, item.Scan.Descriptor.Id, null, failureReason);
        }
		else
		{
			foreach(var childScan in item.Measurement.Children)
			{
				if (childScan.Status == PerformStatus.Performed) continue;
				_protocolHostService.SetPerformStatus(childScan, PerformStatus.Performed, failureReason);

                var childRecon = childScan.Children.FirstOrDefault(r => r.IsRTD);
                _protocolHostService.SetPerformStatus(childRecon, PerformStatus.Performed, failureReason);
            }
            _measurementStatusService.RasiseMeasurementAborted(item.Measurement.Descriptor.Id, true, item.Scan.Descriptor.Id, null, failureReason);
        }

        UpdateProtocol();
	}

	private void ScanStatusService_ScanDone(object? sender, CTS.EventArgs<(string ScanId, RealDoseInfo DoseInfo, bool IsUserCancelled)> e)
	{
		_logger.LogInformation($"ScanStatusService.ScanDone: {JsonConvert.SerializeObject(e.Data)}");

		var item = _protocolHostService.Models.FirstOrDefault(m => m.Scan.Descriptor.Id == e.Data.ScanId);

		if (item.Scan is null)
		{
			_logger.LogInformation($"ScanStatusService.ScanDone: Scan ({e.Data.ScanId}) is not exist.");
			return;
		}

		////TODO:生数据校验有问题
		////验证生数据信息
		//ValidateRawData(e.Data.ScanId);

		if (e.Data.DoseInfo is not null)
		{
			var parameters = new List<ParameterModel>
			{
				new ParameterModel { Name = ProtocolParameterNames.SCAN_ACTUAL_SCAN_TIME, Value = ((uint)(e.Data.DoseInfo.TotalExposureTime * 1000)).ToString() },
				new ParameterModel { Name = ProtocolParameterNames.SCAN_ACTUAL_SCAN_LENGTH, Value = ((uint)(e.Data.DoseInfo.ScanLength * 1000)).ToString() }
			};


			if (!IsInfinityOrNaN(e.Data.DoseInfo.CTDIvol))
			{
				parameters.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_CTDI, Value = e.Data.DoseInfo.CTDIvol.ToString() });
				parameters.Add(new ParameterModel { Name = ProtocolParameterNames.DOSE_INFO_CTDI, Value = e.Data.DoseInfo.CTDIvol.ToString() });
			}

			if (!IsInfinityOrNaN(e.Data.DoseInfo.mAs))
			{
				parameters.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_MA_S, Value = e.Data.DoseInfo.mAs.ToString() });
			}

			if (!IsInfinityOrNaN(e.Data.DoseInfo.DLP))
			{
				parameters.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_DLP, Value = e.Data.DoseInfo.DLP.ToString() });
			}

			if (e.Data.DoseInfo.TubeDoses is not null && e.Data.DoseInfo.TubeDoses.Count > 0)
			{
				parameters.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_KVP,
					Value = (e.Data.DoseInfo.TubeDoses.Sum(t => t.KVP) / e.Data.DoseInfo.TubeDoses.Count).ToString()
				});
				parameters.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_MEANMA,
					Value = (e.Data.DoseInfo.TubeDoses.Sum(t => t.MeanMA) / e.Data.DoseInfo.TubeDoses.Count).ToString()
				});
			}

			_protocolHostService.SetParameters(item.Scan, parameters);
			InsertScanTask(item, e.Data.DoseInfo);
		}

        _protocolHostService.SetPerformStatus(item.Scan, PerformStatus.Performed, e.Data.IsUserCancelled ? FailureReasonType.UserCancellation : FailureReasonType.None);
        _measurementStatusService.RaiseMeasurementDone(item.Measurement.Descriptor.Id);
        
		if (item.Measurement.Children.Count > 1)
		{
            var lastScan = item.Measurement.Children.LastOrDefault();

            if (lastScan.Descriptor.Id != item.Scan.Descriptor.Id && e.Data.IsUserCancelled)
            {
                foreach (var childScan in item.Measurement.Children)
                {
                    if (childScan.Status == PerformStatus.Performed) continue;
                    _protocolHostService.SetPerformStatus(childScan, PerformStatus.Performed, FailureReasonType.UserCancellation);

                    var childRecon = childScan.Children.FirstOrDefault(r => r.IsRTD);
                    _protocolHostService.SetPerformStatus(childRecon, PerformStatus.Performed, FailureReasonType.UserCancellation);
                }
                _measurementStatusService.RasiseMeasurementAborted(item.Measurement.Descriptor.Id, true, item.Scan.Descriptor.Id, null, FailureReasonType.UserCancellation);
            }
        }

        UpdateProtocol();
	}

	private bool IsInfinityOrNaN(float value)
	{
		return value == float.PositiveInfinity || value == float.NegativeInfinity || value == float.NaN;
	}

	/// <summary>
	/// 验证生数据信息
	/// </summary>
	private void ValidateRawData(string scanId)
	{
		var rawDataPath = Path.Combine(RuntimeConfig.Console.RawData.Path, _studyHostService.Instance.StudyInstanceUID
			, scanId);
		_logger.LogInformation($"RawDataValidate rawdatapath is {rawDataPath}");

		Task.Run(() =>
		{
			try
			{
				var validateResults = _rawDataValidator.StartValidate(new RawDataValidatorInput(rawDataPath));

				_logger.LogInformation($"RawDataValidate result is {validateResults.ToJson()}");
			}
			catch (Exception ex)
			{
				_logger.LogError($"RawDataValidate catch error {ex.Message}-{ex.StackTrace}");
			}
		});
	}

	private void ScanStatusService_RawDataSaved(object? sender, CTS.EventArgs<(string ScanId, string ImagePath)> e)
	{
		_logger.LogInformation($"ScanStatusService.RawDataSaved: {JsonConvert.SerializeObject(e.Data)}");

		var item = _protocolHostService.Models.FirstOrDefault(m => m.Scan.Descriptor.Id == e.Data.ScanId);

		if (item.Scan is null)
		{
			_logger.LogInformation($"ScanStatusService.RawDataSaved: Scan ({e.Data.ScanId}) is not exist.");
			return;
		}
		//todo:待写入数据
		_rawDataService.Add(new DatabaseService.Contract.Models.RawDataModel
		{
			Id = Guid.NewGuid().ToString(),
			InternalStudyId = _studyHostService.StudyId,
			FrameOfReferenceUID = item.Frame.Descriptor.Id,
			ScanId = item.Scan.Descriptor.Id,
			ScanName = item.Scan.Descriptor.Name,
			ScanEndTime = DateTime.Now,
			ScanModel = JsonConvert.SerializeObject(item.Scan),
			BodyPart = item.Scan.BodyPart.ToString(),
			ProtocolName = _protocolHostService.Instance.Descriptor.Name,
			PatientPosition = item.Frame.PatientPosition.ToString(),
			Path = e.Data.ImagePath,
			IsDeleted = false,
			IsExported = false,
			CreateTime = DateTime.Now,
			Creator = string.Empty
		});
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		if (_scanStatusService is not null)
		{
			_scanStatusService.ScanStarted += ScanStatusService_ScanStarted;
			_scanStatusService.ScanCancelled += ScanStatusService_ScanCancelled;
			_scanStatusService.ScanAborted += ScanStatusService_ScanAborted;
			_scanStatusService.ScanDone += ScanStatusService_ScanDone;
			_scanStatusService.RawDataSaved += ScanStatusService_RawDataSaved;
		}

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		if (_scanStatusService is not null)
		{
			_scanStatusService.ScanStarted -= ScanStatusService_ScanStarted;
			_scanStatusService.ScanCancelled -= ScanStatusService_ScanCancelled;
			_scanStatusService.ScanAborted -= ScanStatusService_ScanAborted;
			_scanStatusService.ScanDone -= ScanStatusService_ScanDone;
			_scanStatusService.RawDataSaved -= ScanStatusService_RawDataSaved;
		}

		return Task.CompletedTask;
	}

	private void UpdateProtocol()
	{
		_studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
	}

	public void InsertScanTask((FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan) item, RealDoseInfo doseInfo)
	{
		var scanTaskModel = new ScanTaskModel();
		scanTaskModel.Id = Guid.NewGuid().ToString();
		scanTaskModel.IssuingParameters = JsonConvert.SerializeObject(item.Scan.Parameters);
		if (doseInfo is not null)
		{
			scanTaskModel.ActuralParameters = JsonConvert.SerializeObject(doseInfo);
		}
		scanTaskModel.BodyPart = $"{item.Scan.BodyPart}";
		scanTaskModel.Creator = string.Empty;
		scanTaskModel.CreateTime = DateTime.Now;
		scanTaskModel.ScanEndDate = DateTime.Now;
		scanTaskModel.FrameOfReferenceUid = item.Frame.Descriptor.Id;
		scanTaskModel.Description = item.Scan.Descriptor.Name;
		scanTaskModel.IsLinkScan = item.Scan.AutoScan;
		scanTaskModel.MeasurementId = item.Measurement.Descriptor.Id;
		scanTaskModel.InternalPatientId = _studyHostService.Instance.InternalPatientId;
		scanTaskModel.ScanId = item.Scan.Descriptor.Id;
		scanTaskModel.ScanOption = item.Scan.ScanMode.ToString();
		scanTaskModel.InternalStudyId = _studyHostService.StudyId;
		scanTaskModel.TaskStatus = (int)PerformStatus.Performed;
		_scanTaskService.Insert(scanTaskModel);
	}
}
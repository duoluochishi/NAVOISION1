//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract.Models;
using NV.MPS.Environment;

namespace NV.CT.Examination.ApplicationService.Impl;

public class RTDReconHandler : IHostedService
{
    private readonly ILogger<RTDReconHandler> _logger;
    private readonly IProtocolHostService _protocolHostService;
    private readonly IRTDReconService _reconService;
    private readonly IReconTaskService _reconTaskService;
    private readonly ISeriesService _seriesService;
    private readonly IStudyHostService _studyHostService;
    private readonly IMeasurementStatusService _measurementStatusService;

    public RTDReconHandler(ILogger<RTDReconHandler> logger, IProtocolHostService protocolHostService, IRTDReconService reconService, IReconTaskService reconTaskService, ISeriesService seriesService, IStudyHostService studyHostService, IMeasurementStatusService measurementStatusService)
    {
        _logger = logger;
        _seriesService = seriesService;
        _protocolHostService = protocolHostService;
        _reconService = reconService;
        _studyHostService = studyHostService;
        _reconTaskService = reconTaskService;
        _measurementStatusService = measurementStatusService;
    }

    private void ReconService_ReconLoaded(object? sender, CTS.EventArgs<RealtimeReconInfo> e)
    {
        _logger.LogInformation("RTDReconHandler.ReconLoaded: {0}", JsonConvert.SerializeObject(new { ScanId = e.Data.ScanId, ReconId = e.Data.ReconId }));

        var reconModel = ProtocolHelper.GetRecon(_protocolHostService.Instance, e.Data.ScanId, e.Data.ReconId);
        if (reconModel is null)
        {
            _logger.LogInformation($"RTDReconHandler.ReconLoaded: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }

        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Waiting);
    }

    private void ReconService_ReconReconning(object? sender, CTS.EventArgs<RealtimeReconInfo> e)
    {
        _logger.LogInformation("RTDReconHandler.Reconning: {0}", JsonConvert.SerializeObject(new { ScanId = e.Data.ScanId, ReconId = e.Data.ReconId }));

        var reconModel = ProtocolHelper.GetRecon(_protocolHostService.Instance, e.Data.ScanId, e.Data.ReconId);
        if (reconModel is null)
        {
            _logger.LogInformation($"RTDReconHandler.ReconLoaded: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }

        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Performing);
    }

    private void ReconService_ReconCancelled(object? sender, CTS.EventArgs<RealtimeReconInfo> e)
    {
        _logger.LogInformation("RTDReconHandler.ReconCancelled: {0}", JsonConvert.SerializeObject(new { ScanId = e.Data.ScanId, ReconId = e.Data.ReconId }));

        var reconModel = ProtocolHelper.GetRecon(_protocolHostService.Instance, e.Data.ScanId, e.Data.ReconId);
        if (reconModel is null)
        {
            _logger.LogInformation($"RTDReconHandler.ReconCancelled: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }

        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Unperform);

        _studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
    }

    private void ReconService_ReconDone(object? sender, CTS.EventArgs<RealtimeReconInfo> e)
    {
        _logger.LogInformation("RTDReconHandler.ReconDone: {0}", JsonConvert.SerializeObject(new { ScanId = e.Data.ScanId, ReconId = e.Data.ReconId, SeriesNumber = e.Data.SeriesNumber, SeriesId = e.Data.SeriesUID, ImagePath = e.Data.ImagePath }));

        var currentItem = _protocolHostService.Models.FirstOrDefault(m => m.Scan.Descriptor.Id == e.Data.ScanId);
        var reconModel = currentItem.Scan?.Children.FirstOrDefault(r => r.Descriptor.Id == e.Data.ReconId);

        if (reconModel is null)
        {
            _logger.LogDebug($"RTDReconHandler.ReconDone: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }

        var imagePath = e.Data.ImagePath;
        var filePath = Path.Combine(RuntimeConfig.Console.RTDData.Path, "..", e.Data.StudyUID, $"{reconModel.SeriesNumber}_{e.Data.SeriesUID}");
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        if (currentItem.Scan.ScanImageType == ScanImageType.Topo)
        {
            var fileName = Directory.GetFiles(imagePath, "*.dcm").LastOrDefault();
            if (!string.IsNullOrEmpty(fileName))
            {
                var fileInfo = new FileInfo(fileName);
                File.Copy(fileName, Path.Combine(filePath, fileInfo.Name));
            }
            else
            {
                _logger.LogDebug($"RTDReconHandler.ReconDone: The directory does not contain a valid DICOM file.");
            }
        }
        else
        {
            //图像文件拷贝（非开发环境，需要拷贝）
            var fileNames = Directory.GetFiles(imagePath, "*.dcm").ToList();
            if (fileNames is not null && fileNames.Count > 0)
            {
                foreach (var fileName in fileNames)
                {
                    var fileInfo = new FileInfo(fileName);
                    File.Copy(fileName, Path.Combine(filePath, fileInfo.Name));
                }
            }
            else
            {
                _logger.LogDebug($"RTDReconHandler.ReconDone: The directory does not contain a valid DICOM file.");
            }
        }
        e.Data.ImagePath = filePath;

        _protocolHostService.SetParameter(reconModel, ProtocolParameterNames.RECON_IMAGE_PATH, e.Data.ImagePath);

        InsertReconTask((currentItem.Frame, currentItem.Measurement, currentItem.Scan, reconModel), e.Data);
        InsertSeries((currentItem.Frame, currentItem.Measurement, currentItem.Scan, reconModel), e.Data);

        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Performed);
        _measurementStatusService.RaiseMeasurementDone(currentItem.Measurement.Descriptor.Id);

        _studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
    }

    private void ReconService_ReconAborted(object? sender, CTS.EventArgs<RealtimeReconInfo> e)
    {
        //TODO: 实时重建异常
        _logger.LogInformation("RTDReconHandler.ReconAborted: {0}", JsonConvert.SerializeObject(new { ScanId = e.Data.ScanId, ReconId = e.Data.ReconId }));

        var currentItem = _protocolHostService.Models.FirstOrDefault(m => m.Scan.Descriptor.Id == e.Data.ScanId);
        var reconModel = currentItem.Scan?.Children.FirstOrDefault(r => r.Descriptor.Id == e.Data.ReconId);

        if (reconModel is null)
        {
            _logger.LogInformation($"RTDReconHandler.ReconAborted: Scan ({e.Data.ScanId}) or Recon ({e.Data.ReconId}) is not exist.");
            return;
        }

        _protocolHostService.SetPerformStatus(reconModel, PerformStatus.Performed, FailureReasonType.SystemError);
        _measurementStatusService.RasiseMeasurementAborted(currentItem.Measurement.Descriptor.Id, false, currentItem.Scan.Descriptor.Id, reconModel.Descriptor.Id, FailureReasonType.SystemError);

        _studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
    }

    private void InsertReconTask((FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan, ReconModel Recon) item, RealtimeReconInfo reconData)
    {
        var reconTaskModel = new DatabaseService.Contract.Models.ReconTaskModel();
        reconTaskModel.Id = Guid.NewGuid().ToString();
        reconTaskModel.IssuingParameters = JsonConvert.SerializeObject(item.Recon.Parameters);
        reconTaskModel.IsRTD = item.Recon.IsRTD;
        reconTaskModel.Creator = string.Empty;
        reconTaskModel.SeriesNumber = item.Recon.SeriesNumber;
        reconTaskModel.SeriesDescription = string.IsNullOrEmpty(item.Recon.SeriesDescription) ? item.Recon.DefaultSeriesDescription : $"{item.Recon.SeriesDescription}";
        reconTaskModel.ScanId = item.Scan.Descriptor.Id;
        reconTaskModel.CreateTime = DateTime.Now;
        reconTaskModel.ReconEndDate = DateTime.Now;
        reconTaskModel.Description = item.Recon.Descriptor.Name;
        reconTaskModel.FrameOfReferenceUid = item.Frame.Descriptor.Id;
        reconTaskModel.InternalPatientId = _studyHostService.Instance.InternalPatientId;
        reconTaskModel.ReconId = item.Recon.Descriptor.Id;
        reconTaskModel.InternalStudyId = _studyHostService.StudyId;
        reconTaskModel.TaskStatus = (int)OfflineTaskStatus.Finished;
        reconTaskModel.WindowWidth = item.Recon.WindowWidth[0].ToString();
        reconTaskModel.WindowLevel = item.Recon.WindowCenter[0].ToString();
        _reconTaskService.Insert(reconTaskModel);
    }

    private void InsertSeries((FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan, ReconModel Recon) item, RealtimeReconInfo reconData)
    {
        var series = new SeriesModel();
        series.Id = Guid.NewGuid().ToString();
        series.InternalStudyId = _studyHostService.StudyId;
        series.BodyPart = $"{item.Scan.BodyPart}";
        series.ProtocolName = _protocolHostService.Instance.Descriptor.Name;

        series.SeriesNumber = item.Recon.SeriesNumber.ToString();
        series.SeriesInstanceUID = reconData.SeriesUID;
        series.ScanId = reconData.ScanId;
        series.ReconId = reconData.ReconId;
        series.ImageCount = reconData.FinishCount;
        series.ReconEndDate = DateTime.Now;
        series.SeriesType = "image";
        series.SeriesPath = reconData.ImagePath;
        series.SeriesDescription = string.IsNullOrEmpty(item.Recon.SeriesDescription) ? item.Recon.DefaultSeriesDescription : $"{item.Recon.SeriesDescription}";

        if (!string.IsNullOrEmpty(reconData.ImagePath))
        {
            var fileNames = Directory.GetFiles(reconData.ImagePath, "*.dcm");
            series.ImageCount = fileNames.Length;
        }

        if (item.Frame is not null)
        {
            series.FrameOfReferenceUID = item.Frame.Descriptor.Id;
            series.PatientPosition = item.Frame.PatientPosition.ToString();
        }

        series.ImageType = item.Scan.ScanImageType.ToString();
        series.WindowType = $"{item.Recon.WindowType}";
        series.WindowWidth = item.Recon.WindowWidth[0].ToString();
        series.WindowLevel = item.Recon.WindowCenter[0].ToString();

        _seriesService.Add(series);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_reconService is not null)
        {
            _reconService.ReconLoaded += ReconService_ReconLoaded;
            _reconService.ReconReconning += ReconService_ReconReconning;
            _reconService.ReconCancelled += ReconService_ReconCancelled;
            _reconService.ReconDone += ReconService_ReconDone;
            _reconService.ReconAborted += ReconService_ReconAborted;
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_reconService is not null)
        {
            _reconService.ReconLoaded -= ReconService_ReconLoaded;
            _reconService.ReconReconning -= ReconService_ReconReconning;
            _reconService.ReconCancelled -= ReconService_ReconCancelled;
            _reconService.ReconDone -= ReconService_ReconDone;
            _reconService.ReconAborted -= ReconService_ReconAborted;
        }
        return Task.CompletedTask;
    }
}
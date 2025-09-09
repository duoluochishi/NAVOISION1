using NV.CT.DatabaseService.Contract;
using NV.CT.ImageViewer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.DatabaseService.Contract.Models;
using System.Drawing;
using Newtonsoft.Json;
using NV.CT.DicomImageViewer;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;
using NV.CT.Protocol;

namespace NV.CT.ImageViewer.ViewModel
{
    public class ScreenshotViewModel : BaseViewModel
    {
        private readonly ISeriesService _seriesService;
        private readonly ILogger<ScreenshotViewModel>? _logger;
        private readonly IMapper _mapper;
        public ScreenshotViewModel(IMapper mapper,ISeriesService seriesService, ILogger<ScreenshotViewModel>? logger)
        {
            _seriesService = seriesService;
            _logger = logger;
            _mapper = mapper;
        }

        #region ScreenshotMethod

        public void ScreeShot(List<DicomDetail>? dicomDetails,GeneralImageViewer generalImageViewer)
        {
            try
            {
                var seriesModel = _seriesService.GetScreenshotSeriesByImageType(Global.Instance.StudyId, SeriesType.ScreenshotTypeImage);
                var studyInstanceUID = dicomDetails?.Find(r => r.TagDescription == "Study Instance UID");
                var seriesDescription = dicomDetails?.Find(r => r.TagDescription == "Series Description");
                var bitMapSource = _Screenshot.CaptureRegion();
                var bitLogoMap = _Screenshot.StartScreenshot(ScreenShotPath.ScreenShotDir, bitMapSource);
                if (bitLogoMap is not null && bitMapSource is not null
                    && seriesModel is not null && studyInstanceUID?.TagValue is not null && seriesDescription?.TagValue is not null)
                {
                    var bitMap = _Screenshot.BitmapFromSource(bitMapSource);
                    bool addedorupdatedState = AddOrUpdateScreenshotSeries(Global.Instance.StudyId, bitMap, seriesModel, studyInstanceUID.TagValue, seriesDescription.TagValue, generalImageViewer);
                    var seriesViewModel = CTS.Global.ServiceProvider.GetRequiredService<SeriesViewModel>();
                    System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        if (addedorupdatedState)
                        {
                            seriesViewModel.GetSeriesModelsByStudyId(Global.Instance.StudyId);
                            EventAggregator.Instance.GetEvent<ViewSceneChangedEvent>().Publish(seriesViewModel.Selected2DOR3D);
                        }
                        CommonMethod.ShowQRWindow(bitLogoMap);
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogInformation($" ScreeShot {JsonConvert.SerializeObject(ex.ToString())}");
            }
        }
        private bool AddOrUpdateScreenshotSeries(string studyID,Bitmap bitMap, SeriesModel seriesModel, string studyInstanceUID, string seriesDescription,GeneralImageViewer generalImageViewer)
        {

            ScreenshotParameters screenshotParameters = new ScreenshotParameters(bitMap, studyInstanceUID, seriesModel.SeriesInstanceUID, seriesDescription, seriesModel.ImageCount + 1, seriesModel.SeriesPath, seriesModel.SeriesNumber);
            if (!string.IsNullOrEmpty(seriesModel?.Id) && !string.IsNullOrEmpty(studyInstanceUID))
            {
                screenshotParameters.SeriesDescription= seriesDescription+" "+SeriesType.ScreenshotTypeImage;
                bool savedstate = SaveScreenshotDicom(screenshotParameters, generalImageViewer);
                if (savedstate)
                {
                    UpdateScreenshotSeries(seriesModel);
                }
                return false;
            }
            else
            {
                return AddScreenshotSeries(studyID, screenshotParameters,generalImageViewer);
            }
        }
        private int GetImageCount(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return Directory.GetFiles(path).Length;
            }
            return 0;
        }
        private bool SaveScreenshotDicom(ScreenshotParameters screenshotParameters,GeneralImageViewer currentImageViewer)
        {
            _logger?.LogInformation($" ScreenshotParameters {JsonConvert.SerializeObject(screenshotParameters)}");
            ScreenshotProperties screenshotProperties= _mapper.Map<ScreenshotProperties>(screenshotParameters);
            bool savedstate = currentImageViewer.SaveScreenshotToSeires(screenshotProperties);
            return savedstate;
        }
        private bool UpdateScreenshotSeries(SeriesModel seriesModel)
        {
            if (!string.IsNullOrEmpty(seriesModel?.Id))
            {
                var imageCount = GetImageCount(seriesModel.SeriesPath);
                seriesModel.ImageCount = imageCount;
                return _seriesService.UpdateScreenshotSeriesByImageType(seriesModel);
            }
            return false;
        }
        private SeriesModel? CreateSeries(string studyID, ScreenshotParameters screenshotParameters)
        {
            if (!string.IsNullOrEmpty(screenshotParameters.StudyInstanceUID))
            {
                var series = new SeriesModel();
                series.Id = Guid.NewGuid().ToString();
                series.InternalStudyId = studyID;
                series.BodyPart = "";
                series.ProtocolName = "";

                series.SeriesNumber = GlobalSeriesNumber.ScreenShotIndex.ToString();

                series.SeriesInstanceUID = UIDHelper.CreateSeriesInstanceUID();
                series.ScanId = string.Empty;
                series.ReconId = string.Empty;
                series.ImageCount = 0;
                series.ReconEndDate = DateTime.Now;
                series.SeriesType = SeriesType.ScreenshotTypeImage;
                var filePath = Path.Combine(MPS.Environment.RuntimeConfig.Console.MCSAppData.Path, screenshotParameters.StudyInstanceUID, series.SeriesNumber+"_"+series.SeriesInstanceUID);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                series.SeriesPath = filePath;
                series.SeriesDescription = SeriesType.ScreenshotTypeImage.ToString();

                series.FrameOfReferenceUID = string.Empty;
                series.PatientPosition = string.Empty;

                series.ImageType = SeriesType.ScreenshotTypeImage;
                series.WindowWidth = string.Empty;
                series.WindowLevel = string.Empty;
                series.Modality = "CT";
                return series;
            }
            return null;
        }
        private bool AddScreenshotSeries(string studyID, ScreenshotParameters screenshotParameters, GeneralImageViewer generalImageViewer)
        {
            var series = CreateSeries(studyID, screenshotParameters);
            if (series != null)
            {
                screenshotParameters.SeriesDescription = series.SeriesDescription;
                screenshotParameters.SeriesInstanceUID = series.SeriesInstanceUID;
                screenshotParameters.Dir = series.SeriesPath;
                screenshotParameters.SeriesNumber = series.SeriesNumber;
                var savedstate = SaveScreenshotDicom(screenshotParameters, generalImageViewer);
                if (savedstate)
                {
                    series.ImageCount = 1;
                    return _seriesService.Add(series); ;
                }
                return savedstate;
            }
            return false;
        }
        public async Task SafeParallelDelete(string path, int maxDegree = 4)
        {
            await Task.Run(() =>
            {
                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxDegree
                };
                try
                {
                    Parallel.ForEach(Directory.GetFiles(path), options, file =>
                    {
                        DateTime createTime = File.GetCreationTime(file);
                        if (createTime <= DateTime.Today.AddDays(-1))
                        {
                            File.Delete(file);
                        }
                    });
                }
                catch (AggregateException ae)
                {
                    foreach (var e in ae.InnerExceptions)
                    {
                        _logger?.LogError($"Failed to delete png {e.Message} . ");
                    }
                }
            });
        }
        #endregion
    }
}

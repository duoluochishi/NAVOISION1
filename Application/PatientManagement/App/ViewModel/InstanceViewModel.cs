//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Contract;
using NV.CT.DicomImageViewer;
using NV.CT.JobService.Contract;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.Models;

namespace NV.CT.PatientManagement.ViewModel;

public class InstanceViewModel : BaseViewModel
{
    private readonly IMapper _mapper;
    private readonly ILogger<InstanceViewModel> _logger;
    private readonly ISeriesApplicationService _seriesApplicationService;
    private readonly IImageAnnotationService _imageAnnotationService;
    private readonly IDicomFileService _dicomFileService;
    private readonly int _imageWidth = 525;
    private readonly int _imageHeight = 474;
    private ObservableCollection<ImageModel>? _imageModels;
    public ObservableCollection<ImageModel>? ImageModels
    {
        get => _imageModels;
        set { SetProperty(ref _imageModels, value); }
    }

    public TomoImageViewer TomoImageViewerWrapper { get; set; }

    public InstanceViewModel(IMapper mapper,
                             ILogger<TomoImageViewer> tomoLogger,
                             ILogger<InstanceViewModel> instanceLogger,
                             ISeriesApplicationService seriesApplicationService,
                             IImageAnnotationService imageAnnotationService,
                             IDicomFileService dicomFileService)
    {
        _mapper = mapper;
        _logger = instanceLogger;
        _seriesApplicationService = seriesApplicationService;
        _imageAnnotationService = imageAnnotationService;
        _dicomFileService = dicomFileService;

        _logger.LogDebug("InstanceViewModel is starting...");

        _dicomFileService.LoadImageInstancesCompleted += OnInstancesRefreshed;
        _seriesApplicationService.SelectItemChanged += OnSelectedSeriesChanged;
        ImageModels = new ObservableCollection<ImageModel>();
        TomoImageViewerWrapper = new TomoImageViewer(this._imageWidth, this._imageHeight, tomoLogger);
        InitImageViewerFourCornersInfo();
        InitializeImageInstance();

        _logger.LogDebug("InstanceViewModel has started successfully.");
    }

    private void OnInstancesRefreshed(object? sender, List<JobService.Contract.Model.ImageInstanceModel> imageInstances)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            this.ReloadImageModels(imageInstances);
        });
    }

    private void InitImageViewerFourCornersInfo()
    {
        try
        {
            //todo:待调整
            var setting = ImageSettingToOverlayText.Get(_imageAnnotationService.GetConfigs().ScanTomoSettings);
            if (setting.texts.Count > 0)
            {
                TomoImageViewerWrapper.SetFourCornersMessage(setting.textStyle, setting.texts);
            }
        }
        catch(Exception ex) 
        {
            this._logger.LogError($"Failed to SetFourCornersMessage in InstanceViewModel with exception:{ex.Message}. The stacktrace is:{ex.StackTrace}");
            throw;
        }
    }

    private void InitializeImageInstance()
    {
        var seriesViewModel = Global.Instance.ServiceProvider.GetService<SeriesViewModel>();
        if (seriesViewModel is null || seriesViewModel.SelectedItem is null)
        {
            return;
        }

        this.ShowImageViewBySeries(seriesViewModel.SelectedItem.SeriesPath, seriesViewModel.SelectedItem.ImageType);
    }

    /// <summary>
    /// 不同序列变化对应的事件，比如 Topo, Tomo,DoseReport等
    /// </summary>
    private void OnSelectedSeriesChanged(object? sender, EventArgs<string[]> e)
    {
        this.ClearImages();

        if (e.Data.Length < 2)
        {
            return;
        }
        var imagePath = e.Data[1];
        var imageType = e.Data[2];

        this.ShowImageViewBySeries(imagePath, imageType);
    }

    private void ShowImageViewBySeries(string imagePath, string imageType)
    {
        this.ShowImageView(imagePath, imageType);

        if (imageType == Constants.SERIES_TYPE_SR)
        {
            //if the selected series type is Dose SR, clear image instance list because that there is no any image.
            ImageModels?.Clear();
        }
        else
        {
            this.ShowListView(imagePath);
        }
    }

    private void ShowImageView(string imagePath, string imageType)
    {
        TomoImageViewerWrapper.ClearView();

        //if image type is DoseReport, then don't show rule in picture
        //bool isShowRule = true;
        //if (imageType.ToLower() == "dosereport")
        //{
        //    isShowRule = false;
        //}

        //始终不显示尺子
        bool isShowRule = false;
        if (Directory.Exists(imagePath))
        {
            TomoImageViewerWrapper.LoadImageWithDirectoryPath(imagePath, isShowRule);
        }
        else
        {
            TomoImageViewerWrapper.LoadImageWithFilePath(imagePath, isShowRule);
        }
    }

    private void ShowListView(string imagePath)
    {
        _dicomFileService.LoadImageInstances(imagePath);
    }

    private void ReloadImageModels(List<JobService.Contract.Model.ImageInstanceModel> imageInstances)
    {
        ImageModels?.Clear();

        foreach (var imageInstance in imageInstances)
        {
            ImageModels?.Add(new ImageModel
            {
                Id = imageInstance.Id,
                ImageNumber = imageInstance.ImageNumber,
                ImageTime = imageInstance.ImageTime,
                Path = imageInstance.Path
            });
        }
    }

    private void ClearImages()
    {
        TomoImageViewerWrapper.ClearView();
        ImageModels?.Clear();
    }

}
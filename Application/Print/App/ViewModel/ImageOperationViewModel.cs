//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/22 10:43:11    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using FellowOakDicom;
using Newtonsoft.Json;
using NV.CT.ConfigService.Contract.Models;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DicomImageViewer;
using NV.CT.Print.Events;
using NV.CT.Print.Models;
using NV.MPS.UI.Dialog.Service;
using NVCTImageViewerInterop;
using System.Collections.Generic;

namespace NV.CT.Print.ViewModel
{
    public class ImageOperationViewModel : BaseViewModel
    {
        private readonly ILogger<ImageOperationViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly IApplicationCommunicationService _applicationCommunicationService;
        private readonly IPrint _printService;
        private readonly IImageAnnotationService _imageAnnotationService;
        private readonly IFilmSettingsConfigService _filmSettingsConfigService;
        private readonly IPrintConfigManager _printConfigManager;
        private readonly IPrintConfigService _printConfigService;

        private int _previousPageNumber;
        private string _pageSize = PrintConstants.PAGE_SIZE_VALUE_14X17;
        private string _orientation = Constants.DICOM_ORIENTATION_PORTRAIT;
        private int _imageOperationAreaWidth = (int)SystemParameters.WorkArea.Width - 656 - 34; //屏幕总宽-左区-右区
        private int _imageOperationAreaHeight = (int)SystemParameters.WorkArea.Height - 24; //屏幕总高-Header区
        private readonly int _totalBothMargins = 19;  //胶片两侧边缘留余间距

        private volatile bool _isClearViewing = false; //标识正在切换StudyID时发生的ClearView，避免误保存错误的数据

        private PrintImageViewer? _imageViewer;
        public PrintImageViewer? ImageViewer
        {
            get => _imageViewer;
            set => SetProperty(ref _imageViewer, value);
        }

        public PrintingSeriesModel SelectedPrintingSeriesModel { get; set; }

        private int _minPageNumber = 0;
        public int MinPageNumber
        { 
            get 
            { 
                return _minPageNumber; 
            } 
            set 
            {  
                SetProperty(ref _minPageNumber, value);
                //Trace.WriteLine($"########## MinPageNumber is:{value}");
            }
        }

        private int _maxPageNumber = 0;
        public int MaxPageNumber
        {
            get
            {
                return _maxPageNumber;
            }
            set
            {
                SetProperty(ref _maxPageNumber, value);
                //Trace.WriteLine($"########## MaxPageNumber is:{value}");
            }
        }

        private double _viewPortValue = 0.2;
        public double ViewPortValue
        {
            get
            {
                return _viewPortValue;
            }
            set
            {
                SetProperty(ref _viewPortValue, value);
                //Trace.WriteLine($"########## ViewPortValue is:{value}");
            }
        }

        private double _scrollBarValue = 0;
        public double ScrollBarValue
        {
            get
            {
                return _scrollBarValue;
            }
            set
            {
                SetProperty(ref _scrollBarValue, value);

                int currentPageNumber = (int)Math.Round(value);
                //_logger.LogDebug($"***************** changed value:{value}   currentPageNumber:{currentPageNumber} ");

                Trace.WriteLine($"^^^^^^^^^^ changed value:{value}   currentPageNumber:{currentPageNumber}");

                //invoke method of TurnToPage only current page number is different with previous page number.
                if (_previousPageNumber != currentPageNumber)
                {
                    //Trace.WriteLine($"^^^^^^^^^^ Invoke TurnToPrintPageByNumber with currentPageNumber:{currentPageNumber} ");
                    //_logger.LogDebug($"***************** Invoke TurnToPrintPageByNumber with currentPageNumber:{currentPageNumber} ");
                    Global.Instance.ImageViewer.TurnToPrintPageByNumber(currentPageNumber);
                    _previousPageNumber = currentPageNumber;
                }                
            }
        }

        private int _imageAreaWidth;
        public int ImageAreaWidth
        {
            get
            {
                return _imageAreaWidth;
            }
            set
            {
                SetProperty(ref _imageAreaWidth, value);
            }
        }

        private int _imageAreaHeight;
        public int ImageAreaHeight
        {
            get
            {
                return _imageAreaHeight;
            }
            set
            {
                SetProperty(ref _imageAreaHeight, value);
            }
        }
        public int TotalBothMargins
        {
            get => this._totalBothMargins;
        }

        public ImageOperationViewModel(ILogger<ImageOperationViewModel> logger, 
                                       IDialogService dialogService, 
                                       IApplicationCommunicationService applicationCommunicationService,
                                       IPrint printService,
                                       IImageAnnotationService imageAnnotationService,
                                       IFilmSettingsConfigService filmSettingsConfigService,
                                       IPrintConfigManager printConfigManager,
                                       IPrintConfigService printConfigService)
        {   
            _logger = logger;
            _dialogService = dialogService;
            _applicationCommunicationService = applicationCommunicationService;
            _printService = printService;
            _imageAnnotationService = imageAnnotationService;
            _filmSettingsConfigService = filmSettingsConfigService;
            _printConfigManager = printConfigManager;
            _printConfigService = printConfigService;

            _printService.StudyChanged += OnPrintServiceStudyChanged;
            _printConfigService.PrintImagesUpdated += OnPrintImagesUpdated;
            EventAggregator.Instance.GetEvent<SelectedSeriesChangedEvent>().Subscribe(OnSelectedSeriesChanged);
            EventAggregator.Instance.GetEvent<PageSizeChangedEvent>().Subscribe(OnPageSizeChanged);
            EventAggregator.Instance.GetEvent<OrientationChangedEvent>().Subscribe(OnOrientationChanged);
            Global.Instance.ImageViewer.NotifyPrintPageChanged += OnPrintPageChanged;
            Global.Instance.ImageViewer.NotifyPrintImageModified += ImageViewer_NotifyPrintImageModified;
            Global.Instance.ImageViewer.NotifyPrintLayoutModified += ImageViewer_NotifyPrintLayoutModified; 

            this.LoadImages();
        } 

        private void OnPrintImagesUpdated(object? sender, PrintingImageUpdateInfo updateInfo)
        {
            if (updateInfo.StudyId != _printService.GetCurrentStudyId())
            {
                return;
            }
            var printingImages = JsonConvert.DeserializeObject<List<MedImageProperty>>(updateInfo.PrintingImageList.ToJson());
            ImageViewer?.AppendPrintViewData(printingImages);
        }

        private void OnPrintServiceStudyChanged(object? sender, string newStudyId)
        {   
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                var selectPagesViewModel = CTS.Global.ServiceProvider.GetService<SelectPagesViewModel>();
                selectPagesViewModel.ClearPageList();

                this.LoadImages();
            });
        }

        private void LoadImages()
        {
            this._isClearViewing = true;
            ImageViewer?.ClearView();
            this._isClearViewing = false;

            var config = _printConfigManager.LoadConfig(_printService.GetCurrentStudyId());
            InitFourCornerInformation(Global.Instance.ImageViewer);
            this.RefreshImageSize();
            ImageViewer?.SetShowRuler(false);
            _logger.LogDebug("sllin LoadImages 215 ");
            if (config.ImageList.Count > 0)
            {
                var printingImages = JsonConvert.DeserializeObject<List<MedImageProperty>>(config.ImageList.ToJson());
                ImageViewer?.SetPrintViewData(printingImages);

                var layoutItems = JsonConvert.DeserializeObject<List<NVCTImageViewerInterop.ItemRect>>(config.LayoutItemList.ToJson());
                ImageViewer?.SetUserDefineLayout(layoutItems);
            }
            _logger.LogDebug("sllin LoadImages 224 ");
        }

        private void ImageViewer_NotifyPrintImageModified(object? sender, string imageList)
        {
            if (this._isClearViewing)
            {
                return;
            }
            _logger.LogDebug("sllin ImageViewer_NotifyPrintImageModified232 " + imageList.Count());//sllin
            var printingImages = JsonConvert.DeserializeObject<List<PrintingImageProperty>>(imageList);
            var studyId = _printService.GetCurrentStudyId();
            _printConfigManager.UpdateImageInformation(studyId, printingImages);
            _logger.LogDebug("sllin ImageViewer_NotifyPrintImageModified236");
        }

        private void ImageViewer_NotifyPrintLayoutModified(object? sender, string layoutItemList)
        { 
            if (this._isClearViewing)
            {
                return;
            }

            var printingLayoutItems = JsonConvert.DeserializeObject<List<NV.CT.CTS.Models.ItemRect>>(layoutItemList);
            var studyId = _printService.GetCurrentStudyId();
            _printConfigManager.UpdateLayOutInformation(studyId, printingLayoutItems);

            this.CheckShowImage(_printService.GetCurrentStudyId());
        }

        public void OnSelectedSeriesChanged(PrintingSeriesModel seriesModel)
        {
            if (seriesModel is null)
            {
                _logger.LogDebug("Global.Instance.PrintingStudy is null.");
                return;
            }

            SelectedPrintingSeriesModel = seriesModel;
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                ImageViewer = Global.Instance.ImageViewer;
                _logger.LogDebug("get defaultSeries.");

                //InitFourCornerInformation(Global.Instance.ImageViewer);
                //this.RefreshImageSize();
                //ImageViewer?.ClearView();
                //ImageViewer?.SetShowRuler(false);
                var imageList = this.ConvertToImageList(seriesModel);
                ImageViewer?.AppendPrintViewData(imageList);

            });
        }

        private bool CheckShowImage(string studyId)
        {
            var config = _printConfigManager.LoadConfig(studyId);
            return (config.ImageList.Count > 0 && config.LayoutItemList.Count > 0);
        }

        private List<MedImageProperty> ConvertToImageList(PrintingSeriesModel seriesModel)
        {
           var imageList = new List<MedImageProperty>();

            if (seriesModel.SeriesPath.EndsWith(".dcm") && File.Exists(seriesModel.SeriesPath))
            {
                imageList.Add(new MedImageProperty() { SeriesUID = seriesModel.SeriesInstanceUID, ImagePath = seriesModel.SeriesPath });
            }
            else
            {
                var sortedDicomFiles = this.GetValidDicomFiles(seriesModel.SeriesPath); 
                foreach (var dicomFile in sortedDicomFiles)
                {
                    imageList.Add(new MedImageProperty() { SeriesUID = seriesModel.SeriesInstanceUID, ImagePath = dicomFile.Value });
                }
            }
            return imageList;
        }

        private void OnPrintPageChanged(object? sender, (int totalNumberOfPrintPages, int pageNumber) e)
        {
            if (e.totalNumberOfPrintPages == 0)
            {
                this.MinPageNumber = 0;
                this.MaxPageNumber = 0;
                this.ViewPortValue = 1;
                this.ScrollBarValue = 0;

                //Trace.WriteLine("*********** totalNumberOfPrintPages is 0");
                return;
            }
            //_logger.LogDebug("sllin OnTotalNumberOfPrintPagesChanged313.");//sllin
            this.MinPageNumber = 0;
            this.MaxPageNumber = e.totalNumberOfPrintPages - 1;
            this.ViewPortValue = 1d / e.totalNumberOfPrintPages;
            //Trace.WriteLine($"########## ViewPortValue = 1 / {totalNumberOfPrintPages} = {ViewPortValue}");
            this._previousPageNumber = e.pageNumber;
            this.ScrollBarValue = e.pageNumber;
            //_logger.LogDebug($"*********** MaxPageNumber:{MaxPageNumber}  ViewPortValue:{ViewPortValue} ");
            //_logger.LogDebug("OnTotalNumberOfPrintPagesChanged326.");
        }

        public void InitFourCornerInformation(PrintImageViewer imageViewer)
        {
            var (topoTextStyle, topoTexts) = ImageSettingToOverlayText.Get(_imageAnnotationService.GetConfigs().PrintSettings);
            if (topoTexts.Count > 0)
            {
                imageViewer?.ShowCursorRelativeValue(true);
                imageViewer?.SetFourCornersMessage(topoTextStyle, topoTexts);
            }
        }

        private void OnPageSizeChanged(string pageSize)
        {
            this._pageSize = pageSize;
            this.RefreshImageSize();
        }

        private void OnOrientationChanged(string orientation)
        {
            this._orientation = orientation;
            this.RefreshImageSize();

        }

        private void RefreshImageSize()
        {
            if (this._orientation == Constants.DICOM_ORIENTATION_PORTRAIT)
            {
                this.ImageAreaHeight = this._imageOperationAreaHeight;
                this.ImageAreaWidth = this.CalculateWidthOfPortrait();

            }
            else if (this._orientation == Constants.DICOM_ORIENTATION_LANDSCAPE)
            {
                this.ImageAreaWidth = this._imageOperationAreaWidth;
                this.ImageAreaHeight = this.CalculateHeightOfLandscape();
            }

            if(ImageViewer is null)
            {
                ImageViewer = Global.Instance.ImageViewer;
            }

            ImageViewer.MoveView(this.ImageAreaWidth- TotalBothMargins, this.ImageAreaHeight- TotalBothMargins);
            this.SetFilmHeaderAndFooter();
        }

        private int CalculateWidthOfPortrait()
        {
            int resultWidth = this._imageOperationAreaWidth;
            switch (this._pageSize)
            {
                case PrintConstants.PAGE_SIZE_VALUE_8X10: // resultWidth/_imageOperationAreaHeight = 8/10
                    resultWidth = (int)Math.Round((decimal)(_imageOperationAreaHeight * 8) / 10);
                    break;

                case PrintConstants.PAGE_SIZE_VALUE_10X12: // resultWidth/_imageOperationAreaHeight = 10/12
                    resultWidth = (int)Math.Round((decimal)(_imageOperationAreaHeight * 10) / 12);
                    break;

                case PrintConstants.PAGE_SIZE_VALUE_11X14: // resultWidth/_imageOperationAreaHeight = 11/14
                    resultWidth = (int)Math.Round((decimal)(_imageOperationAreaHeight * 11) / 14);
                    break;

                case PrintConstants.PAGE_SIZE_VALUE_14X17: // resultWidth/_imageOperationAreaHeight = 14/17
                    resultWidth = (int)Math.Round((decimal)(_imageOperationAreaHeight * 14) / 17);
                    break;

                default:
                    break;
            }

            return resultWidth;
        }

        private int CalculateHeightOfLandscape()
        {
            int resultHeight = this._imageOperationAreaHeight;
            switch (this._pageSize)
            {
                case PrintConstants.PAGE_SIZE_VALUE_8X10: // resultHeight/_imageOperationAreaWidth = 8/10
                    resultHeight = (int)Math.Round((decimal)(_imageOperationAreaWidth * 8) / 10);
                    break;

                case PrintConstants.PAGE_SIZE_VALUE_10X12: // resultHeight/_imageOperationAreaWidth = 10/12
                    resultHeight = (int)Math.Round((decimal)(_imageOperationAreaWidth * 10) / 12);
                    break;

                case PrintConstants.PAGE_SIZE_VALUE_11X14: // resultHeight/_imageOperationAreaWidth = 11/14
                    resultHeight = (int)Math.Round((decimal)(_imageOperationAreaWidth * 11) / 14);
                    break;

                case PrintConstants.PAGE_SIZE_VALUE_14X17: // resultHeight/_imageOperationAreaWidth = 14/17
                    resultHeight = (int)Math.Round((decimal)(_imageOperationAreaWidth * 14) / 17);
                    break;

                default:
                    break;
            }

            return resultHeight;
        }

        private FilmField ConvertToFildItem(string fildText)
        {
            FilmField result;
            switch (fildText)
            {
                case "PatientName":
                    result = FilmField.PatientName;
                    break;
                case "Gender":
                    result = FilmField.Gender;
                    break;
                case "Age":
                    result = FilmField.Age;
                    break;
                case "PageNumber":
                    result = FilmField.PageNumber;
                    break;
                default:
                    result = FilmField.PatientName;
                    break;
            }

            return result;
        }

        /// <summary>
        /// //临时处理方案：后续将在设置功能模块中改成按实际页脚区域计算归一值
        /// </summary>
        private void SetFilmHeaderAndFooter()
        {
            //根据横版或竖版获取对应的页眉页脚设置
            bool isPortrait = (this._orientation == Constants.DICOM_ORIENTATION_PORTRAIT);
            var filmSettings = _filmSettingsConfigService.GetConfigs().FilmSettingsList.First(o => o.IsPortrait == isPortrait);

            //图像框的实际可用高度
            var actualFilmHeight = this.ImageAreaHeight - this.TotalBothMargins;
            float headerHeight = (float)actualFilmHeight * filmSettings.NormalizedHeaderHeight;
            float footerHeight = (float)actualFilmHeight * filmSettings.NormalizedFooterHeight;
            float offsetY = (float)actualFilmHeight - footerHeight; //页脚区偏移量

            //设置页眉区显示字段
            var headerItems = new List<FilmFieldItem>();
            foreach (var field in filmSettings.HeaderCellsList.CellList)
            {
                var fieldItem = new FilmFieldItem();
                fieldItem.FieldItem = ConvertToFildItem(field.Text);
                fieldItem.LeftTopPosition = new FieldPosition()
                {
                    Left = field.NormalizedLocationX,
                    Top = field.NormalizedLocationY * (float)actualFilmHeight / headerHeight,
                };

                fieldItem.Width = field.NormalizedWidth;
                fieldItem.Height = field.NormalizedHeight * (float)actualFilmHeight / headerHeight;
                fieldItem.FontSize = field.FontSize;

                headerItems.Add(fieldItem);
            }

            //设置页脚区显示字段
            var footerItems = new List<FilmFieldItem>();
            foreach (var field in filmSettings.FooterCellsList.CellList)
            {
                var fieldItem = new FilmFieldItem();
                fieldItem.FieldItem = ConvertToFildItem(field.Text);
                fieldItem.LeftTopPosition = new FieldPosition()
                {
                    Left = field.NormalizedLocationX,
                    Top = (field.NormalizedLocationY * (float)actualFilmHeight - offsetY) / footerHeight,
                };

                fieldItem.Width = field.NormalizedWidth;
                fieldItem.Height = field.NormalizedHeight * (float)actualFilmHeight / headerHeight;
                fieldItem.FontSize = field.FontSize;

                footerItems.Add(fieldItem);
            }

            Global.Instance.ImageViewer.SetFilmHeaderAndFooter(filmSettings.NormalizedHeaderHeight, headerItems, filmSettings.NormalizedFooterHeight, footerItems);

            //设置页眉Logo
            FilmLogoItem headerLogo = new FilmLogoItem();
            headerLogo.ImageURL = string.Empty;
            if (filmSettings.HeaderLogo.IsVisible)
            {
                headerLogo.LeftTopPosition = new FieldPosition()
                {
                    Left = filmSettings.HeaderLogo.NormalizedLocationX,
                    Top = filmSettings.HeaderLogo.NormalizedLocationY * (float)actualFilmHeight / headerHeight
                };
                headerLogo.Width = filmSettings.HeaderLogo.NormalizedWidth;
                headerLogo.Height = filmSettings.HeaderLogo.NormalizedHeight * (float)actualFilmHeight / headerHeight;
                headerLogo.ImageURL = filmSettings.HeaderLogo.PicturePath;
            }

            //设置页脚Logo
            FilmLogoItem footerLogo = new FilmLogoItem();
            footerLogo.ImageURL = string.Empty;
            if (filmSettings.FooterLogo.IsVisible)
            { 
                footerLogo.LeftTopPosition = new FieldPosition()
                {
                    Left = filmSettings.FooterLogo.NormalizedLocationX,
                    Top = (filmSettings.FooterLogo.NormalizedLocationY * (float)actualFilmHeight - offsetY)/footerHeight,
                };
                footerLogo.Width = filmSettings.FooterLogo.NormalizedWidth;
                footerLogo.Height = filmSettings.FooterLogo.NormalizedHeight * (float)actualFilmHeight / footerHeight;
                footerLogo.ImageURL = filmSettings.FooterLogo.PicturePath;
            }
            Global.Instance.ImageViewer.SetLogo(headerLogo, footerLogo);
        }

        private SortedList<int, string> GetValidDicomFiles(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return new SortedList<int, string>();
            }

            if (!Directory.Exists(directory))
            {
                return new SortedList<int, string>();
            }

            var files = (new DirectoryInfo(directory)).GetFiles();
            var sortedDicomFiles = new SortedList<int, string>();
            foreach (var file in files)
            {
                if (!DicomFile.HasValidHeader(file.FullName))
                {
                    continue;
                }

                var dicomFile = DicomFile.Open(file.FullName);
                if (dicomFile.Dataset.TryGetSingleValue<int>(DicomTag.InstanceNumber, out int instanceNumber))
                {
                    sortedDicomFiles.Add(instanceNumber, file.FullName);
                    continue;
                }
            }

            return sortedDicomFiles;
        }
    }

}

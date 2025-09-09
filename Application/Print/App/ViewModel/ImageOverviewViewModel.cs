//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.Print.Events;
using NV.CT.Print.Extensions;
using NV.CT.Print.Models;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NV.CT.Print.ViewModel
{
    public class ImageOverviewViewModel : BaseViewModel
    {
        private readonly IPrintConfigService _printConfigService;
        private readonly ILogger<ImageOverviewViewModel> _logger;
        private bool _isPreviewing = false; //用于区别打印的OnNotifyReadyPage事件
        private string _pageSize = PrintConstants.PAGE_SIZE_VALUE_14X17;
        private string _orientation = Constants.DICOM_ORIENTATION_PORTRAIT;
        private int _previewImageWidth = 1230; //图像库生成的图像原始尺寸
        private int _previewImageHeight = 1016;
        private int _totalPageNumber;
        private CancellationTokenSource _batchLoadCancellationTokenSource = new CancellationTokenSource();

        #region Properties

        public ListView ListViewControl { get; set; }

        public PrintingSeriesModel? SelectedPrintingSeriesModel { get; set; }

        private ObservableCollection<PreviewImagePageModel>? _imageModelList = new ObservableCollection<PreviewImagePageModel>();
        public ObservableCollection<PreviewImagePageModel>? ImageModelList
        {
            get => _imageModelList;
            set => SetProperty(ref _imageModelList, value);
        }

        private int _itemStandardLength = 390;
        public int ItemStandardLength
        { 
            get => _itemStandardLength;
            set =>  SetProperty<int>(ref _itemStandardLength, value);
        }

        private int _outerlineWidth;
        public int OuterlineWidth
        { 
            get => _outerlineWidth;
            set => SetProperty<int>(ref _outerlineWidth, value);
        }

        private int _outerlineHeight;
        public int OuterlineHeight
        {
            get => _outerlineHeight;
            set => SetProperty<int>(ref _outerlineHeight, value);
        }

        private int _imageActualWidth;
        public int ImageActualWidth
        {
            get => _imageActualWidth;
            set => SetProperty<int>(ref _imageActualWidth, value);
        }

        private int _imageActualHeight;
        public int ImageActualHeight
        {
            get => _imageActualHeight;
            set => SetProperty<int>(ref _imageActualHeight, value);
        }

        private bool _isVisible = false;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                if (_isVisible)
                {
                    // 取消之前的任务（如果有的话）
                    _batchLoadCancellationTokenSource.Cancel();
                    _batchLoadCancellationTokenSource = new CancellationTokenSource();
                    this.LoadThumbImages(_totalPageNumber);
                }
                else
                {
                    // 取消正在进行的批处理加载任务
                    _batchLoadCancellationTokenSource.Cancel();
                }
            }
        }
        #endregion

        #region Consturction
        public ImageOverviewViewModel(ILogger<ImageOverviewViewModel> logger, IPrintConfigService printConfigService)
        {   
            _logger = logger;
            _printConfigService = printConfigService;
            //EventAggregator.Instance.GetEvent<SelectedSeriesChangedEvent>().Subscribe(OnSelectedSeriesChanged);
            EventAggregator.Instance.GetEvent<TotalNumberPageChangedEvent>().Subscribe(OnTotalNumberPageChanged);
            EventAggregator.Instance.GetEvent<HeaderFooterVisibilityChangedEvent>().Subscribe(OnHeaderFooterVisibilityChanged);
            EventAggregator.Instance.GetEvent<PageSizeChangedEvent>().Subscribe(OnPageSizeChanged);
            EventAggregator.Instance.GetEvent<OrientationChangedEvent>().Subscribe(OnOrientationChanged);
            EventAggregator.Instance.GetEvent<SelectedPagesChangedEvent>().Subscribe(OnSelectedPagesChanged);
            Global.Instance.ImageViewer.NotifyReadyPage += OnNotifyReadyPage;
            Global.Instance.ImageViewer.NotifyImageLog += OnNotifyImageLog;
            _printConfigService.PrintImagesUpdated += OnPrintImagesUpdated;

            Commands.Add(PrintConstants.COMMAND_IMAGE_SELECTION_CHANGED, new DelegateCommand<IList>(OnImagesSelectionChanged));
        }

        private void OnNotifyImageLog(object? sender, (NVCTImageViewerInterop.LogLevel logLevel, string message) e)
        {
            this._logger.LogDebug($"###ImageLibrary log: LogLevel:{e.logLevel.ToString()} Message;{e.message}");
        }

        private void OnPrintImagesUpdated(object? sender, ConfigService.Contract.Models.PrintingImageUpdateInfo e)
        {
            this.RefreshThumbImages();
        }

        #endregion

        private void OnSelectedSeriesChanged(PrintingSeriesModel seriesModel)
        {
            if (seriesModel is null)
            {
                return;
            }

            SelectedPrintingSeriesModel = seriesModel;
            this.RefreshThumbImages();
        }

        private void RefreshThumbImages()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                int scale = 1;  //thumb image
                this.PreviewPrint(scale);
                this.RefreshImageSize();
            });
        }

        private void RefreshThumbImages(int index)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                int scale = 1;  //thumb image
                PreviewPrint(index, scale);
                this.RefreshImageSize();
            });
        }

        private void OnNotifyReadyPage(object? sender, (int, System.Drawing.Bitmap) e)
        {
            if (!_isPreviewing)
            {
                //如果不是预览请求，则不予处理。
                return;
            }

            //Temp code for test purpose
            //var picturePath = Path.Combine("F:/", $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.bmp" );
            //e.Item2.Save(picturePath);

            this._previewImageWidth = e.Item2.Width;
            this._previewImageHeight = e.Item2.Height;

            int imageIndex = e.Item1;
            if (imageIndex > ImageModelList?.Count - 1)
            {
                _logger?.LogDebug($"invalid imageIndex in OnNotifyReadyPage:{imageIndex}.");
                return;
            }

            if (e.Item2 is null)
            {
                _logger?.LogError($"bitmap in OnNotifyReadyPage is null with imageIndex:{imageIndex}.");
                return;
            }

            ImageModelList[imageIndex].ImageData = BitmapSourceConvertHelper.ToBitmapSource(e.Item2);
        }

        private void OnTotalNumberPageChanged(int totalPageNumber)
        {
            _totalPageNumber = totalPageNumber;
            if (IsVisible)
                LoadThumbImages(totalPageNumber);
        }

        private void LoadThumbImages(int totalPageNumber)
        {
            //ImageModelList?.Clear();
            if (totalPageNumber < 1)
            {
                _logger?.LogWarning($"totalPageNumber of LoadThumbImages is not valid:{totalPageNumber}");
                return;
            }

            //remember previous selected numbers
            var previousSelectedNumbers = new HashSet<int>(ImageModelList.Where(p => p.IsChoosen).Select(p => p.Number));
            var imageModels = new List<PreviewImagePageModel>();
            for (int i = 0; i < totalPageNumber; i++)
            {
                var printingImageModel = new PreviewImagePageModel();
                printingImageModel.Number = i + 1; //设置序号
                printingImageModel.Description = string.Empty;
                imageModels.Add(printingImageModel);
            }

            ImageModelList = imageModels.ToObservableCollection();

            // 先加载1张图片 计算时间，根据时间计算batch size
            var startTime = Stopwatch.StartNew();
            RefreshThumbImages(0);
            startTime.Stop();
            // 动态调整
            int dynamicBatchSize = CalculateBatchSize(startTime.ElapsedMilliseconds);

            // _logger.LogInformation($"leo --- ElapsedMilliseconds:{startTime.ElapsedMilliseconds} dynamicBatchSize:{dynamicBatchSize}");

            // 前dynamicBatchSize个立即加载 1s左右
            int immediateLoadCount = Math.Min(dynamicBatchSize, totalPageNumber);
            // index 0已经加载了，从1开始
            for (int i = 1; i < immediateLoadCount; i++)
            {
                RefreshThumbImages(i);
            }
            // 剩余的分批在后台线程加载，每批dynamicBatchSize个
            if (totalPageNumber > dynamicBatchSize)
            {
                BatchLoadImages(totalPageNumber, dynamicBatchSize, _batchLoadCancellationTokenSource.Token);
            }

            //restore previous selected items
            var previousSelectedItems = ImageModelList.Where(p => previousSelectedNumbers.Contains(p.Number));
            foreach (var item in previousSelectedItems)
            { 
                item.IsChoosen = true;
                this.ListViewControl.SelectedItems.Add(item);   
            }
        }

       /// <summary>
       /// 根据加载图片耗时计算batch size (1 - 20)
       /// </summary>
       /// <param name="elapsedMilliseconds">单次加载的时间</param>
       /// <returns>一个批次要加载的数量</returns>
        private int CalculateBatchSize(long elapsedMilliseconds)
        {
            if (elapsedMilliseconds <= 0)
            {
                return 20;
            }

            // 一个批次总耗时控制在1s左右
            int dynamicBatchSize = (int)(800 / elapsedMilliseconds);

            // 数量在1 - 20之间
            if (dynamicBatchSize < 1)
            {
                dynamicBatchSize = 1;
            }
            else if (dynamicBatchSize > 20)
            {
                dynamicBatchSize = 20;
            }

            return dynamicBatchSize;
        }

        /// <summary>
        /// 批量加载图片
        /// </summary>
        /// <param name="totalPageNumber">总也是</param>
        /// <param name="dynamicBatchSize">一个批次要加载的数量</param>
        /// <param name="cancellationToken">取消task的token</param>
        /// <returns></returns>
        private Task BatchLoadImages(int totalPageNumber, int dynamicBatchSize, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                for (int start = dynamicBatchSize; start < totalPageNumber; start += dynamicBatchSize)
                {
                    // 检查是否已取消
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Batch loading task was cancelled.");
                        return;
                    }

                    var dispather = Application.Current?.Dispatcher;
                    if (dispather == null)
                        continue;

                    // 使用Dispatcher.Invoke确保在UI线程上执行一批加载
                    await dispather.Invoke(async () =>
                    {
                        // 再次检查是否已取消
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("Batch loading task was cancelled during dispatcher invoke.");
                            return;
                        }

                        int batchEnd = Math.Min(start + dynamicBatchSize, totalPageNumber);

                        // 加载当前批次
                        for (int i = start; i < batchEnd; i++)
                        {
                            // 检查是否已取消
                            if (cancellationToken.IsCancellationRequested)
                            {
                                _logger.LogInformation($"Batch loading task was cancelled during loading image {i}.");
                                return;
                            }

                            RefreshThumbImages(i);
                        }

                        // 强制UI刷新
                        await Task.Delay(1);
                    }, DispatcherPriority.Background);

                    // 等待一段时间让UI稳定，同时也检查是否已取消
                    try
                    {
                        await Task.Delay(50, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Batch loading task was cancelled during delay.");
                        return;
                    }
                }
            }, cancellationToken);
        }

        private void OnHeaderFooterVisibilityChanged((bool isShowHeader, bool isShowFooter) arg)
        {
            var selectPagesViewModel = CTS.Global.ServiceProvider.GetRequiredService<SelectPagesViewModel>();
            this.LoadThumbImages(selectPagesViewModel.PageWithCheckModelList.Count);
        }

        private void OnPageSizeChanged(string pageSize)
        {
            this._pageSize = pageSize;
            this.PreviewPrint(1);
            this.RefreshImageSize();
        }

        private void OnOrientationChanged(string orientation)
        {
            this._orientation = orientation;
            this.PreviewPrint(1);
            this.RefreshImageSize();
        }

        private void PreviewPrint(int scale)
        {
            this._isPreviewing = true;
            Global.Instance.ImageViewer?.GetAllPagesOfPrintPreview(scale);
            this._isPreviewing = false;
        }

        private void PreviewPrint(int index, int scale)
        {
            this._isPreviewing = true;
            Global.Instance.ImageViewer?.GetSinglePrintPageByIndex(index, scale);
            this._isPreviewing = false;
        }

        private void RefreshImageSize()
        {
            if (this._orientation == Constants.DICOM_ORIENTATION_PORTRAIT)
            {
                this.OuterlineHeight = this.ItemStandardLength;
                this.OuterlineWidth = this.CalculateLengthByPageSize();

                this.ImageActualWidth = this.OuterlineWidth;
                this.ImageActualHeight = this._previewImageWidth == 0 ? 0 : (int)Math.Round((decimal)(this.ImageActualWidth * this._previewImageHeight)/this._previewImageWidth); //ImageActualHeight/ImageActualWidth = _previewImageHeight/_previewImageWidth

            }
            else if (this._orientation == Constants.DICOM_ORIENTATION_LANDSCAPE)
            {
                this.OuterlineWidth  = this.ItemStandardLength;
                this.OuterlineHeight = this.CalculateLengthByPageSize();

                this.ImageActualHeight = this.OuterlineHeight;
                this.ImageActualWidth = this._previewImageHeight == 0 ? 0 : (int)Math.Round((decimal)(this._previewImageWidth*this.ImageActualHeight)/this._previewImageHeight); //ImageActualWidth/ImageActualHeight = _previewImageWidth/_previewImageHeight
            }

            int offset = 16;
            this.ImageActualWidth = this.ImageActualWidth - offset;
            this.ImageActualHeight = this.ImageActualHeight - offset;

        }

        private int CalculateLengthByPageSize()
        {
            int resultLength = this.ItemStandardLength;
            switch (this._pageSize)
            {
                case PrintConstants.PAGE_SIZE_VALUE_8X10: // resultLength/ItemStandardLength = 8/10
                    resultLength = (int)Math.Round((decimal)(ItemStandardLength * 8) / 10);
                    break;

                case PrintConstants.PAGE_SIZE_VALUE_10X12: // resultLength/ItemStandardLength = 10/12
                    resultLength = (int)Math.Round((decimal)(ItemStandardLength * 10) / 12);
                    break;

                case PrintConstants.PAGE_SIZE_VALUE_11X14: // resultLength/ItemStandardLength = 11/14
                    resultLength = (int)Math.Round((decimal)(ItemStandardLength * 11) / 14);
                    break;

                case PrintConstants.PAGE_SIZE_VALUE_14X17: // resultLength/ItemStandardLength = 14/17
                    resultLength = (int)Math.Round((decimal)(ItemStandardLength * 14) / 17);
                    break;

                default: 
                    break;
            }

            return resultLength;
        }

        private void OnImagesSelectionChanged(IList selectedItems)
        {
            if (this.ImageModelList is null)
            {
                return;
            }

            var selectedNumbers = new HashSet<int>();
            foreach (var item in selectedItems) 
            {
                selectedNumbers.Add(((PreviewImagePageModel)item).Number);
            }

            foreach (var imageModel in ImageModelList)
            {
                imageModel.IsChoosen = selectedNumbers.Contains(imageModel.Number);
            }

            var selectPagesViewModel = CTS.Global.ServiceProvider.GetRequiredService<SelectPagesViewModel>(); 
            foreach (var pageModel in selectPagesViewModel.PageWithCheckModelList)
            {
                pageModel.IsChecked = selectedNumbers.Contains(pageModel.PageNumber);
            }
        }

        private void OnSelectedPagesChanged(bool hasSelectedPages)
        {
            if (this.ImageModelList is null)
            {
                return;
            }

            if (ListViewControl is null)
            {
                return;
            }

            var selectPagesViewModel = CTS.Global.ServiceProvider.GetRequiredService<SelectPagesViewModel>();
            var selectedNumbers = new HashSet<int>(selectPagesViewModel.PageWithCheckModelList.Where(p => p.IsChecked).Select(p => p.PageNumber));

            foreach (var imageModel in this.ImageModelList)
            {
                imageModel.IsChoosen = selectedNumbers.Contains(imageModel.Number);

                if (imageModel.IsChoosen && !ListViewControl.SelectedItems.Contains(imageModel))
                {
                    ListViewControl.SelectedItems.Add(imageModel);
                }
                else if (!imageModel.IsChoosen && ListViewControl.SelectedItems.Contains(imageModel))
                {
                    ListViewControl.SelectedItems.Remove(imageModel);
                }
            }

        }

    }
}

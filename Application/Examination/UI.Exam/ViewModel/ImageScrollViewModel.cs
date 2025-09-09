//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//----------------------------------------------------------------------

using System.Windows.Threading;

namespace NV.CT.UI.Exam.ViewModel;

public class ImageScrollViewModel : BaseViewModel
{
    private IImageOperationService _imageOperationService;
    public readonly ILogger<ImageScrollViewModel> _logger;
    private int _smallChange = 1;
    public int SmallChange
    {
        get => _smallChange;
        set => SetProperty(ref _smallChange, value);
    }

    private int _minimum = 0;
    public int Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }

    private int _maximum = 10;
    public int Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }

    private int _currentIndex = 0;
    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            SetProperty(ref _currentIndex, value);
        }
    }

    private int _seriesCurrentIndex = 0;
    public int SeriesCurrentIndex
    {
        get => _seriesCurrentIndex;
        set => SetProperty(ref _seriesCurrentIndex, value);
    }

    private bool IsScrollLeftButtonDown = false;
    DispatcherTimer timer = new DispatcherTimer();
    public ImageScrollViewModel(IImageOperationService imageOperationService,
        ILogger<ImageScrollViewModel> logger)
    {
        _imageOperationService = imageOperationService;
        _logger = logger;
        Commands.Add(CommandParameters.COMMAND_PREVIEW_MOUSE_LEFT_BUTTON_DOWN, new DelegateCommand(PreviewMouseLeftButtonDown));
        Commands.Add(CommandParameters.COMMAND_PREVIEW_MOUSE_LEFT_BUTTON_UP, new DelegateCommand(PreviewMouseLeftButtonUp));
        // Commands.Add(CommandParameters.COMMAND_VALUE_CHANGED, new DelegateCommand(ValueChanged));
        Commands.Add("MouseLeave", new DelegateCommand(MouseLeave));
        _imageOperationService.ImageSliceIndexChanged -= ImageOperationService_ImageSliceIndexChanged;
        _imageOperationService.ImageSliceIndexChanged += ImageOperationService_ImageSliceIndexChanged;
        _imageOperationService.ImageCountChanged -= ImageOperationService_ImageCountChanged;
        _imageOperationService.ImageCountChanged += ImageOperationService_ImageCountChanged;

        timer.Interval = TimeSpan.FromMilliseconds(30);
        timer.Tick -= Timer_Tick;
        timer.Tick += Timer_Tick;
    }

    [UIRoute]
    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (IsScrollLeftButtonDown && CurrentIndex != SeriesCurrentIndex)
        {
            _logger.LogDebug($"The SetImageSliceLocation index is：{CurrentIndex}");
            _imageOperationService.SetImageSliceLocation(CurrentIndex);
        }
    }

    private void MouseLeave()
    {
        IsScrollLeftButtonDown = false;
    }

    private void ImageOperationService_ImageCountChanged(object? sender, EventArgs<int> e)
    {
        if (e is null)
        {
            return;
        }
        CurrentIndex = 0;
        Minimum = 0;
        Maximum = e.Data - 1;
        SeriesCurrentIndex = 0;
    }

    private void ImageOperationService_ImageSliceIndexChanged(object? sender, EventArgs<int> e)
    {
        if (e is null)
        {
            return;
        }
        SeriesCurrentIndex = e.Data;
        if (!IsScrollLeftButtonDown)
        {
            CurrentIndex = e.Data;
        }
        _logger.LogDebug($"The series index from dicom is：{e.Data}");
    }

    public void PreviewMouseLeftButtonDown()
    {
        IsScrollLeftButtonDown = true;
        timer.Start();
    }

    public void PreviewMouseLeftButtonUp()
    {
        IsScrollLeftButtonDown = false;
        timer.Stop();
        Task.Delay(50).Wait();
        if (CurrentIndex == Maximum)
        {
            _imageOperationService.SetImageSliceLocation(Maximum);
        }
        if (CurrentIndex == Minimum)
        {
            _imageOperationService.SetImageSliceLocation(0);
        }
    }
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.ImageViewer.Model;

public class ImageModel : BindableBase
{
	private string title = string.Empty;
	private string fullTitle=string.Empty;
	private WriteableBitmap? imageSource;
	private bool isSelected;
	private bool isEnable;
	private string seriesType = string.Empty;
	private string seriesId = string.Empty;
	private ICommand? mouseLeftButtonDownCommand;
	private bool isFile;
	private string seriesPath = string.Empty;
	private string studyId = string.Empty;
	private string seriesNumber=string.Empty;
	private string reconId = string.Empty;
    private string imageType = string.Empty;

    public string DisplayTitle => $"[{SeriesNumber}] {FullTitle}";

	public string SeriesNumber
	{
		get => seriesNumber;
		set => SetProperty(ref seriesNumber, value);
	}
	public string SeriesPath
	{
		get => seriesPath;
		set => SetProperty(ref seriesPath, value);
	}

	public bool IsFile
	{
		get => isFile;
		set => SetProperty(ref isFile, value);
	}

	public string Title
	{
		get => title;
		set => SetProperty(ref title, value);
	}

	public string FullTitle
	{
		get => fullTitle;
		set => SetProperty(ref fullTitle, value);
	}

	public WriteableBitmap? ImageSource
	{
		get => imageSource;
		set => SetProperty(ref imageSource, value);
	}

	public bool IsSelected
	{
		get => isSelected;
		set => SetProperty(ref isSelected, value);
	}

	public bool IsEnable
	{
		get => isEnable;
		set => SetProperty(ref isEnable, value);
	}
	public string SeriesType
	{
		get => seriesType;
		set => SetProperty(ref seriesType, value);
	}
	public string SeriesId
	{
		get => seriesId;
		set => SetProperty(ref seriesId, value);
	}

    public string ReconId
    {
        get => reconId;
        set => SetProperty(ref reconId, value);
    }
    public string StudyId
	{
		get => studyId;
		set => SetProperty(ref studyId, value);
	}
    public string ImageType
    {
        get => imageType;
        set => SetProperty(ref imageType, value);
    }

    public ICommand? MouseLeftButtonDownCommand
	{
		get => mouseLeftButtonDownCommand;
		set => SetProperty(ref mouseLeftButtonDownCommand, value);
	}

}
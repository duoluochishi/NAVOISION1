//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.ImageViewer.Model;

public class SeriesModel : BaseViewModel
{
    private string id = string.Empty;
    private string studyId = string.Empty;
    private string seriesNumber = string.Empty;
    private string reconId = string.Empty;
    private string seriesDescription = string.Empty;
    private string seriesType = string.Empty;
    private string seriesPath = string.Empty;
    private string imageType = string.Empty;

    public string Id
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    public string StudyId
    {
        get => studyId;
        set => SetProperty(ref studyId, value);
    }

    public string ReconId
    {
        get => reconId;
        set => SetProperty(ref reconId, value);
    }

    public string SeriesNumber
    {
        get => seriesNumber;
        set => SetProperty(ref seriesNumber, value);
    }

    public string SeriesDescription
    {
        get => seriesDescription;
        set => SetProperty(ref seriesDescription, value);
    }

    public string SeriesType
    {
        get => seriesType;
        set => SetProperty(ref seriesType, value);
    }

    public string SeriesPath
    {
        get => seriesPath;
        set => SetProperty(ref seriesPath, value);
    }
    public string ImageType
    {
        get => imageType;
        set => SetProperty(ref imageType, value);
    }
}
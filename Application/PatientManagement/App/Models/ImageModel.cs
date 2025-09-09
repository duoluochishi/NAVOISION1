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
// <key>
//
// </key>
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="VStudy.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------

namespace NV.CT.PatientManagement.Models;

public class ImageModel : BaseViewModel
{   
    private string _id = string.Empty;
    public string Id
    {
        get => _id;
        set
        {
            _id = value;
            SetProperty(ref _id, value);
        }
    }

    private string _seriesId = string.Empty;
    public string SeriesId
    {
        get => _seriesId;
        set
        {
            _seriesId = value;
            SetProperty(ref _seriesId, value);
        }
    }

    private int _imageNumber;
    public int ImageNumber
    {
        get => _imageNumber;
        set
        {
            _imageNumber = value;
            SetProperty(ref _imageNumber, value);
        }
    }

    private DateTime _imageTime;
    public DateTime ImageTime
    {
        get => _imageTime;
        set
        {
            _imageTime = value;
            SetProperty(ref _imageTime, value);
        }
    }

    private string _path = string.Empty;
    public string Path
    {
        get => _path;
        set
        {
            _path = value;
            SetProperty(ref _path, value);
        }
    }
}
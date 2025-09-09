//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/19 13:38:00           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.InterventionScan.Models;
public class SeriesModel : BaseViewModel
{
    /// <summary>
    /// ID唯一号
    /// </summary>
    private string _id = string.Empty;
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// 名称
    /// </summary>
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private int _sortNO = 0;
    public int SortNO
    {
        get => _sortNO;
        set => SetProperty(ref _sortNO, value);
    }

    /// <summary>
    /// 存储路径
    /// </summary>
    private string _storagePath = string.Empty;
    public string StoragePath
    {
        get => _storagePath;
        set => SetProperty(ref _storagePath, value);
    }

    /// <summary>
    /// 
    /// </summary>
    private string _reconID = string.Empty;
    public string ReconID
    {
        get => _reconID;
        set => SetProperty(ref _reconID, value);
    }

    private bool _isIntervention = false;
    public bool IsIntervention
    {
        get { return _isIntervention; }
        set => SetProperty(ref _isIntervention, value);
    }
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using System.Collections.Generic;

namespace NV.CT.ConfigManagement.ViewModel;

public class BaseCoefficientViewModel : BaseViewModel
{
    private string _id = string.Empty;
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private int _kv = 10000;
    public int KV
    {
        get => _kv;
        set => SetProperty(ref _kv, value);
    }

    private int _ma = 10000;
    public int MA
    {
        get => _ma;
        set => SetProperty(ref _ma, value);
    }

    List<SourceViewModel> _sources = new List<SourceViewModel>();
    public List<SourceViewModel> Sources
    {
        get => _sources;
        set => SetProperty(ref _sources, value);
    }

    public string SourceDisplay
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            foreach (SourceViewModel source in Sources)
            {
                sb.Append(string.Format($"Id:{source.Id},Kv:{source.KVFactor},Ma:{source.MAFactor};"));
            }
            return sb.ToString();
        }
    }
}
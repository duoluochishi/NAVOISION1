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
namespace NV.CT.ConfigManagement.ViewModel;

public class SourceViewModel : BaseViewModel
{
    private int _id = 0;
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private int _kvFactor = 10000;
    public int KVFactor
    {
        get => _kvFactor;
        set => SetProperty(ref _kvFactor, value);
    }

    private int _maFactor = 10000;
    public int MAFactor
    {
        get => _maFactor;
        set => SetProperty(ref _maFactor, value);
    }
}
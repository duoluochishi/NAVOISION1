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
public class NeedleModel : BaseViewModel
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

    /// <summary>
    /// 颜色:以带"#"的RGB字符串形式存在，如：#FFFFFF
    /// </summary>
    private string _needleColor = string.Empty;
    public string NeedleColor
    {
        get => _needleColor;
        set => SetProperty(ref _needleColor, value);
    }
}
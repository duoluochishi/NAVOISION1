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

public class ComboItem : BaseViewModel
{
    private string text;
    public string Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }

    private string value;
    public string Value
    {
        get => value;
        set => SetProperty(ref this.value, value);
    }

    public ComboItem(string text, string value)
    {
        this.text = text;
        this.value = value;
    }
}
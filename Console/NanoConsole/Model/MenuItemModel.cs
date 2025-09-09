//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.Model;

/// <summary>
/// 配置页面自定义控件菜单数据Model
/// </summary>
public class MenuItemModel
{
    public MenuItemModel(string name, string describe, string imgSource, string returnOnClickUri = "")
    {
        Name = name;
        Describe = describe;
        ImgSource = imgSource;
        ReturnOnClickUri = returnOnClickUri;

    }

    /// <summary>
    /// 配置菜单名字
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 配置菜单描述
    /// </summary>
    public string Describe { get; set; }

    /// <summary>
    /// 配置菜单图片资源
    /// </summary>
    public string ImgSource { get; set; }

    /// <summary>
    /// 配置菜单点击事件跳转页面地址
    /// </summary>
    public string ReturnOnClickUri { get; set; }



}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/16 13:45:36    V1.0.0         胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;

namespace NV.CT.DatabaseService.Contract;

public interface IPrintConfigManager
{
    /// <summary>
    /// 根据StudyID加载并反序列化成内存对象
    /// </summary>
    /// <param name="studyId"></param>
    /// <returns></returns>
    public (List<PrintingImageProperty> ImageList, List<ItemRect> LayoutItemList, ItemRect SelectedLayoutItem) LoadConfig(string studyId);

    /// <summary>
    /// 添加打印图像信息
    /// </summary>
    /// <param name="studyId"></param>
    /// <param name="imagePropertyList"></param>
    /// <returns></returns>
    public bool AppendImagesToPrint(string studyId, List<PrintingImageProperty> imagePropertyList);

    /// <summary>
    /// 由打印模块更新内存对象的图像信息
    /// </summary>
    /// <param name="studyId"></param>
    /// <param name="imagePropertyList"></param>
    /// <returns></returns>
    public bool UpdateImageInformation(string studyId, List<PrintingImageProperty> imagePropertyList);

    /// <summary>
    /// 由打印模块更新内存对象的布局信息
    /// </summary>
    /// <param name="studyId"></param>
    /// <param name="layoutItems"></param>
    /// <returns></returns>
    public bool UpdateLayOutInformation(string studyId, List<ItemRect> layoutItemList);

    /// <summary>
    /// 由打印模块更新内存对象的选中的布局信息
    /// </summary>
    /// <param name="studyId"></param>
    /// <param name="selectedLayoutItem">选中的布局</param>
    /// <returns></returns>
    public bool UpdateSelectedLayOutInformation(string studyId, ItemRect selectedLayoutItem);

    /// <summary>
    /// 持久化内存对象
    /// </summary>
    /// <param name="studyId"></param>
    /// <returns></returns>
    public bool Save(string studyId);  

}
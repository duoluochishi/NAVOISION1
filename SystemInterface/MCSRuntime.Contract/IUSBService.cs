//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/19 10:31:40     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MCSRuntime.Contract;

public interface IUSBService
{
    event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> Inserted;
    event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> Removed;

    /// <summary>
    /// 说明： 该属性的返回结果有缺陷，仅包含U盘，不包含移动硬盘，推荐使用方法FindExternalDisks()
    /// </summary>
    List<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> Currents { get; }


    /// <summary>
    /// 说明： 该方法返回结果包括U盘和移动硬盘，推荐使用
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> FindExternalDisks();

    IEnumerable<string> GetAllDisks();
}

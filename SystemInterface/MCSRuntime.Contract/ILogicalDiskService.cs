//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/19 9:54:06     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MCSRuntime.Contract;

public interface ILogicalDiskService
{
    event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> DiskInserted;
    event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> DiskRemoved;

    List<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> GetDisks(DriveType driveType);

}

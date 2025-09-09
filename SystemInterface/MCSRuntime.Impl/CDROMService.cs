//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/19 9:58:48     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.SystemInterface.MCSRuntime.Contract;

namespace NV.CT.SystemInterface.MCSRuntime.Impl;

public class CDROMService : ICDROMService
{
    private readonly ILogicalDiskService _diskService;

    public event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> Inserted;
    public event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> Removed;

    public CDROMService(ILogicalDiskService diskService)
    {
        _diskService = diskService;
        _diskService.DiskInserted += DiskService_DiskInserted;
        _diskService.DiskRemoved += DiskService_DiskRemoved;
    }

    public List<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> Currents => _diskService.GetDisks(DriveType.CDRom);

    private void DiskService_DiskInserted(object? sender, (string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory) e)
    {
        if (e.DriveType == DriveType.CDRom)
        {
            Inserted?.Invoke(this, e);
        }
    }

    private void DiskService_DiskRemoved(object? sender, (string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory) e)
    {
        if (e.DriveType == DriveType.CDRom)
        {
            Removed?.Invoke(this, e);
        }
    }
}

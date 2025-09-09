//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/19 9:58:32     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.SystemInterface.MCSRuntime.Contract;
using System.Management;

namespace NV.CT.SystemInterface.MCSRuntime.Impl;

public class USBService : IUSBService
{
    private readonly ILogicalDiskService _diskService;

    public event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> Inserted;
    public event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> Removed;

    public USBService(ILogicalDiskService diskService)
    {
        _diskService = diskService;
        _diskService.DiskInserted += DiskService_DiskInserted;
        _diskService.DiskRemoved += DiskService_DiskRemoved;
    }

    public List<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> Currents => _diskService.GetDisks(DriveType.Removable);

    public IEnumerable<string> FindExternalDisks()
    {
        var driverNameList = new List<string>();

        var managementClass = new ManagementClass("Win32_DiskDrive");
        var disks = managementClass.GetInstances();
        foreach (ManagementObject diskObject in disks)
        {
            if (diskObject.Properties["MediaType"].Value is not null &&
              !(diskObject.Properties["MediaType"].Value.ToString() == "Fixed hard disk media"))
            {
                foreach (ManagementObject diskPartition in diskObject.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementBaseObject logicDisk in diskPartition.GetRelated("Win32_LogicalDisk"))
                    {
                        driverNameList.Add(logicDisk.Properties["Name"].Value.ToString());
                    }
                }
            }
        }

        driverNameList.Sort();
        return driverNameList.ToArray();

    }
    public  IEnumerable<string> GetAllDisks()
    {
        var diskList = new List<string>();

        // 获取所有逻辑驱动器
        var drives = DriveInfo.GetDrives();
        foreach (var drive in drives)
        {
            if (drive.IsReady)
            {
                diskList.Add(drive.Name.TrimEnd('\\'));
            }
        }
        // 获取外部磁盘
        //diskList.AddRange(FindExternalDisks());
        diskList.Sort();
        return diskList.Distinct(); // 去重
    }
    private void DiskService_DiskInserted(object? sender, (string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory) e)
    {
        if (e.DriveType == DriveType.Removable)
        {
            Inserted?.Invoke(this, e);
        }
    }

    private void DiskService_DiskRemoved(object? sender, (string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory) e)
    {
        if (e.DriveType == DriveType.Removable)
        {
            Removed?.Invoke(this, e);
        }
    }
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/18 17:43:02     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.SystemInterface.MCSDVDCDBurner;
using NV.CT.SystemInterface.MCSRuntime.Contract;

namespace NV.CT.SystemInterface.MCSRuntime.Impl;

public class LogicalDiskService : ILogicalDiskService
{
    private List<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> _disks = new List<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)>();
    private readonly IDeviceService _deviceService;

    public event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> DiskInserted;
    public event EventHandler<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> DiskRemoved;

    public LogicalDiskService(IDeviceService deviceService)
    {
        _deviceService = deviceService;
        _deviceService.DeviceCreated += DeviceService_DeviceCreated;
        _deviceService.DeviceRemoved += DeviceService_DeviceRemoved;
    }

    public List<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)> GetDisks(DriveType driveType)
    {
        //每次都实时获取接入的磁盘信息，避免出现接入光驱后又弹出导致就绪状态变化不能反映的情况
        _disks.Clear();

        var driveInfos = DriveInfo.GetDrives();
        foreach (var driveInfo in driveInfos)
        {
            try
            {
                if (driveInfo.DriveType == DriveType.CDRom)
                {
                    //DriveInfo 读取到的光驱信息不准确，需要借助IMAPI接口进一步判断是否可以刻录
                    var availableRecord = RecorderManager.GetAvailableRecorderList().FirstOrDefault(r => r.DriverName == driveInfo.Name);
                    if (availableRecord is not null)
                    {
                        _disks.Add((driveInfo.Name, availableRecord.CheckBurnable(), DriveType.CDRom, string.Empty, driveInfo.RootDirectory));
                    }
                }
                else
                {
                    _disks.Add((driveInfo.Name, driveInfo.IsReady, driveInfo.DriveType, driveInfo.DriveFormat, driveInfo.RootDirectory));
                }
            }
            catch
            {
                //如果是失效的映射盘
                continue;
            }
        }

        return _disks.Where(d => d.DriveType == driveType).ToList();
    }

    private void DeviceService_DeviceCreated(object? sender, EventArgs e)
    {
        var driveInfos = DriveInfo.GetDrives();
        foreach(var driveInfo in driveInfos)
        {
            var diskName = driveInfo.Name.Replace("\\", "");
            var temp = _disks.FirstOrDefault(d => d.Name == diskName);
            if (string.IsNullOrEmpty(temp.Name))
            {
                var data = (diskName, driveInfo.IsReady, driveInfo.DriveType, driveInfo.DriveFormat, driveInfo.RootDirectory);
                _disks.Add(data);
                DiskInserted?.Invoke(this, data);
            }
        }
    }

    private void DeviceService_DeviceRemoved(object? sender, EventArgs e)
    {
        var driveInfos = DriveInfo.GetDrives();
        var removeDisks = new List<(string Name, bool IsReady, DriveType DriveType, string FileSystem, DirectoryInfo RootDirectory)>();
        foreach(var diskInfo in _disks)
        {
            var temp = driveInfos.FirstOrDefault(d => d.Name.Replace("\\", "") == diskInfo.Name);
            if (temp is null)
            {
                removeDisks.Add(diskInfo);
            }
        }
        while (removeDisks.Count > 0)
        {
            var removeDisk = removeDisks.First();
            removeDisks.Remove(removeDisk);
            _disks.Remove(removeDisk);
            DiskRemoved?.Invoke(this, removeDisk);
        }
    }
}

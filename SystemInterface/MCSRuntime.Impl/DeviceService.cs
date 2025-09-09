//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/18 17:23:28     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.SystemInterface.MCSRuntime.Contract;
using System.Management;

namespace NV.CT.SystemInterface.MCSRuntime.Impl;

public class DeviceService : IDeviceService
{
    public event EventHandler DeviceCreated;
    public event EventHandler DeviceRemoved;

    public DeviceService()
    {
        DeviceMonitoring();
    }

    private void DeviceMonitoring()
    {
        var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 or EventType = 3");
        var watcher = new ManagementEventWatcher(query);
        watcher.EventArrived += (s, e) => {
            var eventType = Convert.ToUInt16(e.NewEvent.Properties["EventType"].Value);
            if (eventType == 2)
            {
                //TODO:暂时不处理是不是磁盘, property.Name == DriveName
                DeviceCreated?.Invoke(this, EventArgs.Empty);
            }
            else if (eventType == 3)
            {
                DeviceRemoved?.Invoke(this, EventArgs.Empty);
            }
        };
        watcher.Start();
    }
}

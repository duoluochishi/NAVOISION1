//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/9/19 11:06:42           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Examination.ApplicationService.Contract.Models;

public class DeviceMonitorConfig
{
    public bool MonitorCTBox { get; set; }

    public bool MonitorDoorClosed { get; set; }

    public bool MonitorReconConnectionStatus { get; set; }

    public bool MonitorTableStatus { get; set; }
}

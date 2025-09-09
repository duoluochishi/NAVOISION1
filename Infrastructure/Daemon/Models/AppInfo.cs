//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/20 12:39:59     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Daemon.Models;

public class AppInfo
{
    public bool IsRelative { get; set; }

    public string Path { get; set; }

    public string Name { get; set; }

    public bool IsLaunch { get; set; }

    public bool IsMonitor { get; set; }

    public bool IsRestart { get; set; }

    public bool IsShowWindow { get; set; }

    public int ProcessId { get; set; }

    public string ProcessName { get; set; }
}

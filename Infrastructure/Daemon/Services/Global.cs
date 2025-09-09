//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/20 12:41:55     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.Daemon.Models;
using System.Collections.Concurrent;

namespace NV.CT.Daemon.Services;

public class Global
{
    private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

    private Global()
    {
        Applications = new ConcurrentDictionary<string, AppInfo>();
    }

    public static Global Instance => _instance.Value;

    public ConcurrentDictionary<string, AppInfo> Applications { get; private set; }
}

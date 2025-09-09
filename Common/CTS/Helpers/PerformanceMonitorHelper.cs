//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/25 10:19:27    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace NV.CT.CTS.Helpers;

public static class PerformanceMonitorHelper
{
    public static void Execute(string name, Action action)
    {
        var watcher = new Stopwatch();
        watcher.Start();
        action.Invoke();
        watcher.Stop();
        Global.Logger.LogDebug($"Action execution time ({name}): {watcher.Elapsed.TotalMilliseconds} ms");
    }

    public static T Execute<T>(string name, Func<T> func)
    {
        var watcher = new Stopwatch();
        watcher.Start();
        var result = func.Invoke();
        watcher.Stop();
        Global.Logger.LogDebug($"Action execution time ({name}): {watcher.Elapsed.TotalMilliseconds} ms");
        return result;
    }
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 13:17:30     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace NV.CT.SystemInterface.MCSRuntime.Contract.CPU;

public class CPUHelper
{
    /*
    IdleTime 空闲时间
    KernelTime 内核时间
    UserTime 用户时间

    系统时间 = 内核时间 + 用户时间
    SystemTime = KernelTime + UserTime
    */

    //https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getsystemtimes
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

    /// <summary>
    /// 获取 CPU 工作时间
    /// </summary>
    /// <param name="lpIdleTime"></param>
    /// <param name="lpKernelTime"></param>
    /// <param name="lpUserTime"></param>
    /// <returns></returns>
    public static CPUTime GetCPUTime(FILETIME lpIdleTime, FILETIME lpKernelTime, FILETIME lpUserTime)
    {
        var IdleTime = (ulong)lpIdleTime.DateTimeHigh << 32 | lpIdleTime.DateTimeLow;
        var KernelTime = (ulong)lpKernelTime.DateTimeHigh << 32 | lpKernelTime.DateTimeLow;
        var UserTime = (ulong)lpUserTime.DateTimeHigh << 32 | lpUserTime.DateTimeLow;

        var SystemTime = KernelTime + UserTime;

        return new CPUTime(IdleTime, SystemTime);
    }

    /// <summary>
    /// 获取 CPU 工作时间
    /// </summary>
    /// <returns></returns>
    public static CPUTime GetCPUTime()
    {
        FILETIME lpIdleTime = default;
        FILETIME lpKernelTime = default;
        FILETIME lpUserTime = default;
        if (!GetSystemTimes(out lpIdleTime, out lpKernelTime, out lpUserTime))
        {
            return default;
        }
        return GetCPUTime(lpIdleTime, lpKernelTime, lpUserTime);
    }

    /// <summary>
    /// 计算 CPU 使用率
    /// </summary>
    /// <param name="oldTime"></param>
    /// <param name="newTime"></param>
    /// <returns></returns>
    public static double CalculateCPULoad(CPUTime oldTime, CPUTime newTime)
    {
        ulong totalTicksSinceLastTime = newTime.SystemTime - oldTime.SystemTime;
        ulong idleTicksSinceLastTime = newTime.IdleTime - oldTime.IdleTime;

        double ret = 1.0f - (totalTicksSinceLastTime > 0 ? (double)idleTicksSinceLastTime / totalTicksSinceLastTime : 0);

        return ret;
    }
}

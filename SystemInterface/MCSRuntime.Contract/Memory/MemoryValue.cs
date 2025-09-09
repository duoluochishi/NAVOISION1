//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 13:41:29     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MCSRuntime.Contract.Memory;

/// <summary>
/// 内存值表示
/// </summary>
public struct MemoryValue
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="totalPhysicalMemory">物理内存字节数</param>
    /// <param name="availablePhysicalMemory">可用的物理内存字节数</param>
    /// <param name="usedPercentage">已用物理内存百分比</param>
    /// <param name="totalVirtualMemory">虚拟内存字节数</param>
    /// <param name="availableVirtualMemory">可用虚拟内存字节数</param>
    public MemoryValue(
        ulong totalPhysicalMemory,
        ulong availablePhysicalMemory,
        double usedPercentage,
        ulong totalVirtualMemory,
        ulong availableVirtualMemory)
    {
        TotalPhysicalMemory = totalPhysicalMemory;
        AvailablePhysicalMemory = availablePhysicalMemory;
        UsedPercentage = usedPercentage;
        TotalVirtualMemory = totalVirtualMemory;
        AvailableVirtualMemory = availableVirtualMemory;
    }

    /// <summary>
    /// 物理内存字节数
    /// </summary>
    public ulong TotalPhysicalMemory { get; private set; }

    /// <summary>
    /// 可用的物理内存字节数
    /// </summary>
    public ulong AvailablePhysicalMemory { get; private set; }

    /// <summary>
    /// 已用物理内存字节数
    /// </summary>
    public ulong UsedPhysicalMemory => TotalPhysicalMemory - AvailablePhysicalMemory;

    /// <summary>
    /// 已用物理内存百分比，0~100，100表示内存已用尽
    /// </summary>
    public double UsedPercentage { get; private set; }

    /// <summary>
    /// 虚拟内存字节数
    /// </summary>
    public ulong TotalVirtualMemory { get; private set; }

    /// <summary>
    /// 可用虚拟内存字节数
    /// </summary>
    public ulong AvailableVirtualMemory { get; private set; }

    /// <summary>
    /// 已用虚拟内存字节数
    /// </summary>
    public ulong UsedVirtualMemory => TotalVirtualMemory - AvailableVirtualMemory;
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 10:53:41     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace NV.CT.SystemInterface.MCSRuntime.Contract.System;

public class SystemPlatformInfo
{
    /// <summary>
    /// .NET Runtime version
    /// <para>ex: .NET Core 3.1.9</para>
    /// </summary>
    public static string FrameworkDescription => RuntimeInformation.FrameworkDescription;

    /// <summary>
    /// .NET version
    /// <para>
    /// ex:<br />
    /// 3.1.9
    /// </para>
    /// </summary>
    public static string FrameworkVersion => Environment.Version.ToString();

    /// <summary>
    /// 操作系统平台架构，可点击 <see cref="Architecture" /> 获取详细的信息
    /// <para>
    /// ex:<br />
    /// X86<br />
    /// X64<br />
    /// Arm<br />
    /// Arm64
    /// </para>
    /// </summary>
    public static string OSArchitecture => RuntimeInformation.OSArchitecture.ToString();

    /// <summary>
    /// 获取操作系统的类型 <see cref="PlatformID"/>
    /// <para>
    /// ex:<br />
    /// Win32S、Win32Windows、Win32NT、WinCE、Unix、Xbox、MacOSX
    /// </para>
    /// </summary>
    public static string OSPlatform => Environment.OSVersion.Platform.ToString();

    /// <summary>
    /// 操作系统内核版本
    /// <para>
    /// ex:<br />
    /// Microsoft Windows NT 6.2.9200.0<br />
    /// Unix 4.4.0.19041
    /// </para>
    /// </summary>
    public static string OSVersion => Environment.OSVersion.ToString();

    /// <summary>
    /// 操作系统的版本描述
    /// <para>
    /// ex: <br />
    /// Microsoft Windows 10.0.19041
    /// <br />
    /// Linux 4.4.0-19041-Microsoft #488-Microsoft Mon Sep 01 13:43:00 PST 2020
    /// </para>
    /// </summary>
    public static string OSDescription => RuntimeInformation.OSDescription;

    /// <summary>
    /// 当前计算机上的处理器数
    /// </summary>
    /// <remarks>如 4核心8线程的 CPU，这里会获取到 8</remarks>
    public static int ProcessorCount => Environment.ProcessorCount;

    /// <summary>
    /// 计算机名称
    /// </summary>
    public static string MachineName => Environment.MachineName;

    /// <summary>
    /// 当前登录到此系统的用户名称
    /// </summary>
    public static string UserName => Environment.UserName;

    /// <summary>
    /// 用户网络域名称，即 hostname
    /// </summary>
    public static string UserDomainName => Environment.UserDomainName;

    /// <summary>
    /// 是否在交互模式中运行
    /// </summary>
    public static bool IsUserInteractive => Environment.UserInteractive;

    /// <summary>
    /// 系统根目录完全路径。<b>Linux 没有系统根目录</b>
    /// <para>
    /// ex:<br />
    /// Windows: X:\WINDOWS\system32<br></br>
    /// Linux  : null
    /// </para>
    /// </summary>
    public static string SystemDirectory => Environment.SystemDirectory;

    /// <summary>
    /// 操作系统内存页一页的字节数
    /// </summary>
    public static int MemoryPageSize => Environment.SystemPageSize;
}

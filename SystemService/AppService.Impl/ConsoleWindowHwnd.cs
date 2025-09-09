//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
namespace NV.CT.AppService.Impl;

public class ConsoleWindowHwnd
{
	private static readonly Lazy<ConsoleWindowHwnd> _instance = new(() => new ConsoleWindowHwnd());

	private ConsoleWindowHwnd() { }

	public static ConsoleWindowHwnd Instance => _instance.Value;

	public IntPtr MasterWindowHwnd { get; set; } = IntPtr.Zero;

	public IntPtr AuxilaryWindowHwnd { get; set; } = IntPtr.Zero;
}

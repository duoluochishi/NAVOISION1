//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.SystemInterface.MCSRuntime.Contract.Disk;

namespace NV.CT.SystemInterface.MCSRuntime.Contract;

public interface ISpecialDiskService
{
	/// <summary>
	/// D盘写日志和数据库
	/// </summary>
	DriveInfo? D { get; }
	/// <summary>
	/// 影像数据
	/// </summary>
	DriveInfo? E { get; }
	/// <summary>
	/// 生数据目录
	/// </summary>
	DriveInfo? F { get; }

	public double DFreeSpaceRate { get; }
	public double EFreeSpaceRate { get; }
	public double FFreeSpaceRate { get; }

	public long GetDriverFreeSpace(string driverName);
	public long GetDirectorySize(string directoryName);

	/// <summary>
	/// 获取警告级别
	/// </summary>
	public DiskSpaceWarnLevel GetWarnLevel(double freeSpaceRate);

	public void TriggerCurrentWarnLevel();

	event EventHandler<DiskSpaceWarnLevel>? FreeDiskSpaceWarnedEvent;
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 13:38:08     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MCSRuntime.Contract.Disk;

/// <summary>
/// 磁盘信息
/// </summary>
public class DiskInfo
{
	/// <summary>
	/// 获取磁盘类
	/// </summary>
	public DriveInfo DriveInfo { get; private set; }

	public long AvailableFreeSpace => DriveInfo.AvailableFreeSpace;

	public string DriveFormat => DriveInfo.DriveFormat;

	public DriveType DriveType => DriveInfo.DriveType;

	public bool IsReady => DriveInfo.IsReady;

	public string Name => DriveInfo.Name;

	public DirectoryInfo RootDirectory => DriveInfo.RootDirectory;

	public long TotalFreeSpace => DriveInfo.TotalFreeSpace;

	public long TotalSize => DriveInfo.TotalSize;

	public string VolumeLabel => DriveInfo.VolumeLabel;

	/// <summary>
	/// 磁盘已使用容量
	/// </summary>
	public long UsedSize => DriveInfo.TotalSize - DriveInfo.TotalFreeSpace;

	public decimal UsedPercentage => (decimal)Math.Round((DriveInfo.TotalSize - DriveInfo.TotalFreeSpace) * 100.0 / DriveInfo.TotalSize, 2);

	/// <summary>
	/// 获取本地所有磁盘信息
	/// </summary>
	/// <returns></returns>
	public static List<DiskInfo> Disks => DriveInfo.GetDrives().Select(x => new DiskInfo { DriveInfo = x }).ToList();

	/// <summary>
	/// 获取 Docker 运行的容器其容器文件系统在主机中的存储位置
	/// </summary>
	/// <remarks>程序需要在宿主机运行才有效果，在容器中运行，调用此API获取不到相关信息</remarks>
	/// <returns></returns>
	public static List<DiskInfo> DockerMerge => DriveInfo.GetDrives().Where(x => x.DriveFormat.Equals("overlay", StringComparison.OrdinalIgnoreCase) && x.DriveFormat.Contains("docker")).Select(x => new DiskInfo { DriveInfo = x }).ToList();

	/// <summary>
	/// 筛选出真正能够使用的磁盘
	/// </summary>
	/// <returns></returns>
	public static List<DiskInfo> RealDisks => DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed && x.TotalSize != 0 && x.DriveFormat != "overlay").Select(x => new DiskInfo { DriveInfo = x }).Distinct(new DiskInfoEquality()).ToList();

	/// <summary>
	/// 筛选重复项
	/// </summary>
	private class DiskInfoEquality : IEqualityComparer<DiskInfo>
	{
		public bool Equals(DiskInfo? x, DiskInfo? y)
		{
			return x?.DriveInfo.Name == y?.DriveInfo.Name;
		}

		public int GetHashCode(DiskInfo obj)
		{
			return obj.DriveInfo.Name.GetHashCode();
		}
	}
}
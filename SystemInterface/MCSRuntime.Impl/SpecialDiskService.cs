//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Security.Principal;
using Microsoft.Extensions.Logging;

using NV.CT.SystemInterface.MCSDVDCDBurner;
using NV.CT.SystemInterface.MCSRuntime.Contract;
using NV.CT.SystemInterface.MCSRuntime.Contract.Disk;
using NV.MPS.Configuration;

namespace NV.CT.SystemInterface.MCSRuntime.Impl;

public class SpecialDiskService : ISpecialDiskService, IDisposable
{
	private readonly FileSystemWatcher? _dWatcher;
	private readonly FileSystemWatcher? _eWatcher;
	private readonly FileSystemWatcher? _fWatcher;

	private readonly FileSystemWatcher[] _watchers;

	private DiskSpaceWarnLevel _previousWarnLevel = DiskSpaceWarnLevel.Green;

	private readonly double _eWarnValue = UserConfig.DiskspaceSettingConfig.DiskspaceSetting.ImageDataWarningThreshold.Value;
	private readonly double _eErrorValue = UserConfig.DiskspaceSettingConfig.DiskspaceSetting.ImageDataErrorThreshold.Value;
	private readonly double _fWarnValue = UserConfig.DiskspaceSettingConfig.DiskspaceSetting.RawDataWarningThreshold.Value;
	private readonly double _fErrorValue = UserConfig.DiskspaceSettingConfig.DiskspaceSetting.RawDataErrorThreshold.Value;

	private readonly ILogger<SpecialDiskService> _logger;
	public SpecialDiskService(ILogger<SpecialDiskService> logger)
	{
		_logger = logger;
		if (D is not null && D.IsReady)
		{
			_dWatcher = new FileSystemWatcher(D.Name);
			_dWatcher.EnableRaisingEvents = true;
			_dWatcher.Changed += FileSystemWatcher_Changed;
		}

		if (E is not null && E.IsReady)
		{
			_eWatcher = new FileSystemWatcher(E.Name);
			_eWatcher.EnableRaisingEvents = true;
			_eWatcher.Changed += FileSystemWatcher_Changed;
		}

		if (F is not null && F.IsReady)
		{
			_fWatcher = new FileSystemWatcher(F.Name);
			_fWatcher.EnableRaisingEvents = true;
			_fWatcher.Changed += FileSystemWatcher_Changed;
		}

		var sid = WindowsIdentity.GetCurrent().User?.Value ?? "";

		var drives = DriveInfo.GetDrives();
		var watchers = new List<FileSystemWatcher>();
		foreach (var drive in drives)
		{
			try
			{
				string recycleBinPath = $@"{drive.RootDirectory.FullName}$Recycle.Bin\{sid}";

				if (Directory.Exists(recycleBinPath))
				{
					var watcher = new FileSystemWatcher(recycleBinPath);
					watcher.EnableRaisingEvents = true;
					watcher.IncludeSubdirectories = true;
					watcher.NotifyFilter =
						NotifyFilters.FileName |
						NotifyFilters.LastWrite |
						NotifyFilters.Size |
						NotifyFilters.CreationTime;
					watcher.Changed += FileSystemWatcher_Changed;
					watcher.Deleted += FileSystemWatcher_Changed;
					watcher.Renamed += FileSystemWatcher_Changed;

					watchers.Add(watcher);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"[SpecialDiskService] catch error : {ex.Message}-{ex.StackTrace}");
			}
		}

		_watchers = watchers.ToArray();

	}

	private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
	{
		TriggerCurrentWarnLevel();
	}

	public DiskSpaceWarnLevel GetWarnLevel(double freeSpaceRate)
	{
		_logger.LogInformation($"GetWarnLevel freeSpaceRate {freeSpaceRate} , EWarnValue {_eWarnValue} EErrorValue {_eErrorValue} , FWarnValue:{_fWarnValue},FErrorValue:{_fErrorValue}");

		if (freeSpaceRate >= _eWarnValue && freeSpaceRate <= _eErrorValue)
		{
			return DiskSpaceWarnLevel.Orange;
		}

		if (freeSpaceRate > _eErrorValue)
		{
			return DiskSpaceWarnLevel.Red;
		}

		return DiskSpaceWarnLevel.Green;
	}

	public void TriggerCurrentWarnLevel()
	{
		UpdateDiskAnalysisResult();
	}

	private void UpdateDiskAnalysisResult()
	{
		var dStage = DiskSpaceWarnLevel.Green;
		var eStage = DiskSpaceWarnLevel.Green;
		var fStage = DiskSpaceWarnLevel.Green;

		if (D is not null)
		{
			if (DConsumedSpaceRate >= _eWarnValue && DConsumedSpaceRate <= _eErrorValue)
			{
				dStage = DiskSpaceWarnLevel.Orange;
			}
			else if (DConsumedSpaceRate > _eErrorValue)
			{
				dStage = DiskSpaceWarnLevel.Red;
			}
		}
		if (E is not null)
		{
			if (EConsumedSpaceRate >= _eWarnValue && EConsumedSpaceRate <= _eErrorValue)
			{
				eStage = DiskSpaceWarnLevel.Orange;
			}
			else if (EConsumedSpaceRate > _eErrorValue)
			{
				eStage = DiskSpaceWarnLevel.Red;
			}
		}
		if (F is not null)
		{
			//_logger.LogInformation($"[SpecialDiskService] FConsumed : {FConsumedSpaceRate} , _fWarnValue {_fWarnValue} , _fErrorValue:{_fErrorValue}");

			if (FConsumedSpaceRate >= _fWarnValue && FConsumedSpaceRate <= _fErrorValue)
			{
				fStage = DiskSpaceWarnLevel.Orange;
			}
			else if (FConsumedSpaceRate > _fErrorValue)
			{
				fStage = DiskSpaceWarnLevel.Red;
			}
		}

		var maxWarnLevel = (DiskSpaceWarnLevel)Math.Max((int)fStage, Math.Max((int)dStage, (int)eStage));

		_logger.LogInformation($"[SpecialDiskService] dStage is {dStage} , eStage is {eStage} , fStage is {fStage} , _previousStage is {_previousWarnLevel} , maxStage is {maxWarnLevel}");

		if (_previousWarnLevel != maxWarnLevel)
		{
			FreeDiskSpaceWarnedEvent?.Invoke(this, maxWarnLevel);

			_previousWarnLevel = maxWarnLevel;
		}
	}

	public DriveInfo? D => DriveInfo.GetDrives().FirstOrDefault(d => d.Name == @"D:\");
	public DriveInfo? E => DriveInfo.GetDrives().FirstOrDefault(d => d.Name == @"E:\");
	public DriveInfo? F => DriveInfo.GetDrives().FirstOrDefault(d => d.Name == @"F:\");

	public double DFreeSpaceRate => D is not null && D.IsReady ? (D.AvailableFreeSpace * 100.0 / D.TotalSize) : 0;
	public double EFreeSpaceRate => E is not null && E.IsReady ? (E.AvailableFreeSpace * 100.0 / E.TotalSize) : 0;
	public double FFreeSpaceRate => F is not null && F.IsReady ? (F.AvailableFreeSpace * 100.0 / F.TotalSize) : 0;

	public double DConsumedSpaceRate
	{
		get
		{
			if (D is not null && D.IsReady)
			{
				return 100 - D.AvailableFreeSpace * 100.0 / D.TotalSize;
			}

			return 0;
		}
	}
	public double EConsumedSpaceRate
	{
		get
		{
			if (E is not null && E.IsReady)
			{
				return 100 - E.AvailableFreeSpace * 100.0 / E.TotalSize;
			}

			return 0;
		}
	}
	public double FConsumedSpaceRate
	{
		get
		{
			if (F is not null && F.IsReady)
			{
				return 100 - F.AvailableFreeSpace * 100.0 / F.TotalSize;
			}

			return 0;
		}
	}

	/// <summary>
	/// 获取驱动器大小，返回大小的单位是字节
	/// </summary>
	public long GetDriverFreeSpace(string driverName)
	{
		var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == driverName);
		if (drive is null)
		{
			return 0;
		}

		if (drive.DriveType == DriveType.CDRom)
		{
			var availableRecord = RecorderManager.GetAvailableRecorderList().FirstOrDefault(r => r.DriverName == drive.Name);
			if (availableRecord is null)
			{
				return 0;
			}

			var cdromSize = availableRecord.GetDiskSize();
			return cdromSize.Item1;
		}
		else
		{
			return drive.AvailableFreeSpace;
		}
	}

	/// <summary>
	/// 获取指定文件夹的大小，返回大小的单位是字节
	/// </summary>
	/// <exception cref="ArgumentNullException"></exception>
	public long GetDirectorySize(string directoryName)
	{
		if (string.IsNullOrEmpty(directoryName))
		{
			throw new ArgumentNullException(directoryName);
		}

		if (!Directory.Exists(directoryName))
		{
			return 0;
		}

		long size = 0;
		var dir = new DirectoryInfo(directoryName);

		var files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			size += file.Length;
		}

		var directories = dir.GetDirectories();
		foreach (DirectoryInfo subDir in directories)
		{
			if ((subDir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
			{
				continue;
			}

			size += GetDirectorySize(subDir.FullName);
		}

		return size;
	}

	public event EventHandler<DiskSpaceWarnLevel>? FreeDiskSpaceWarnedEvent;
	public void Dispose()
	{
		foreach (var w in _watchers)
		{
			w.EnableRaisingEvents = false;
			w.Dispose();
		}
	}
}
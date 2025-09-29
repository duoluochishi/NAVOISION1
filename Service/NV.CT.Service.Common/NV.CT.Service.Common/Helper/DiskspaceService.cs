using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common.Interfaces;
using NV.MPS.Configuration;
using System;
using System.IO;
using System.Linq;

namespace NV.CT.Service.Common.Helper
{
    public class DiskspaceService
    {
        #region private fields

        private static readonly Lazy<DiskspaceService> _lazyInstance = new Lazy<DiskspaceService>(() => new DiskspaceService());

        private readonly ILogService _logService;

        #endregion


        #region Init

        private DiskspaceService()
        {
            _logService = LogService.Instance;
            _diskspaceSetting = UserConfig.DiskspaceSettingConfig.DiskspaceSetting;
        }

        #endregion

        public static readonly string RawDataDriveName = @"F:\";
        public static readonly string ImageDataDriveName = @"E:\";
        public static DiskspaceService Instance
        {
            get
            {
                lock (typeof(DiskspaceService))
                {
                    return _lazyInstance.Value;
                }
            }
        }

        /// <summary>
        /// 生数据所在驱动器（磁盘分区）
        /// 默认F:\
        /// </summary>
        public DriveInfo? RawDataDrive { get => DriveInfo.GetDrives().FirstOrDefault(d => d.Name == RawDataDriveName); }

        /// <summary>
        /// 离线重建数据所在驱动器（磁盘分区）。
        /// 默认E:\
        /// </summary>
        public DriveInfo? ImageDataDrive { get => DriveInfo.GetDrives().FirstOrDefault(d => d.Name == ImageDataDriveName); }

        /// <summary>
        /// 检查驱动器可用大小，如果已经达到警戒值以下，返回false。
        /// </summary>
        /// <returns></returns>
        public bool ValidateRawDataDiskFreeSapce()
        {
            bool result = true;

            var thresholdValue = _diskspaceSetting.RawDataErrorThreshold.Value;
            _logService.Debug(ServiceCategory.Common, $"Got the config value 'RawDataErrorThreshold' is '{thresholdValue}'");

            result = ValidateFreeSpace(RawDataDrive, thresholdValue);
            return result;
        }
        private bool ValidateFreeSpace(DriveInfo? driveInfo, double maxUsedThresholdValue)
        {
            bool result = true;
            var expectedFreeSpacePercent = 100 - maxUsedThresholdValue;
            double? currentFreeSapcePercent = GetFreeSpacePercent(driveInfo);
            _logService.Debug(ServiceCategory.Common, $"The percent of available free space on the drive '{RawDataDriveName}' is '{currentFreeSapcePercent:f2}%', " +
                $"and the expected value from config is less than '{expectedFreeSpacePercent}%'.");

            if (currentFreeSapcePercent < expectedFreeSpacePercent)
            {
                result = false;
                _logService.Error(ServiceCategory.Common, $"Not matched the expected value!");
            }

            return result;
        }

        private double? GetFreeSpacePercent(DriveInfo? dirve)
        {
            double? freeSapceRate = null;
            if (dirve is not null && dirve.IsReady)
            {
                freeSapceRate = (dirve.AvailableFreeSpace * 1.0 / dirve.TotalSize) * 100;
            }
            return freeSapceRate;
        }

        private DiskspaceSettingInfo _diskspaceSetting;
    }
}

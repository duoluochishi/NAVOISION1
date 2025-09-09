using System;

namespace NV.CT.Service.ComponentHistory.Models
{
    public class ComponentEntryItemHistoryModel
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// SN
        /// </summary>
        public required string SN { get; init; }

        /// <summary>
        /// 安装时间
        /// </summary>
        public required DateTime InstallTime { get; init; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public required DateTime? RetireTime { get; init; }

        /// <summary>
        /// 使用时长 (天)
        /// </summary>
        public double UsageTime => ((RetireTime ?? DateTime.Now) - InstallTime).TotalDays;
    }
}
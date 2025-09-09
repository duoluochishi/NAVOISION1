using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums.Components;
using NV.CT.Service.ComponentHistory.Enums;

namespace NV.CT.Service.ComponentHistory.Models
{
    public class ComponentEntryItemModel : ObservableObject
    {
        #region Field

        private string _localSN = string.Empty;
        private string _deviceSN = string.Empty;
        private DateTime _installTime;
        private ReadDeviceSNStatus _readDeviceSNStatus;
        private string _readDeviceSNStr = string.Empty;

        #endregion

        /// <summary>
        /// 上级名称
        /// </summary>
        public required string SeniorName { get; init; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// 部件类型
        /// </summary>
        public required SerialNumberComponentType ComponentType { get; init; }

        /// <summary>
        /// ID 从1开始
        /// </summary>
        public required int ID { get; init; }

        /// <summary>
        /// 本地配置文件中记录的SN
        /// </summary>
        public string LocalSN
        {
            get => _localSN;
            set
            {
                if (SetProperty(ref _localSN, value))
                {
                    OnPropertyChanged(nameof(IsCanUpdate));
                }
            }
        }

        /// <summary>
        /// 读取的硬件中的SN
        /// </summary>
        public string DeviceSN
        {
            get => _deviceSN;
            set
            {
                if (SetProperty(ref _deviceSN, value))
                {
                    OnPropertyChanged(nameof(IsCanUpdate));
                }
            }
        }

        /// <summary>
        /// 安装时间
        /// </summary>
        public required DateTime InstallTime
        {
            get => _installTime;
            set => SetProperty(ref _installTime, value);
        }

        /// <summary>
        /// 读取硬件中SN号时的状态
        /// </summary>
        public ReadDeviceSNStatus ReadDeviceSNStatus
        {
            get => _readDeviceSNStatus;
            set => SetProperty(ref _readDeviceSNStatus, value);
        }

        /// <summary>
        /// 读取硬件中SN号时的提示信息
        /// </summary>
        public string ReadDeviceSNStr
        {
            get => _readDeviceSNStr;
            set => SetProperty(ref _readDeviceSNStr, value);
        }

        /// <summary>
        /// 是否可以Update
        /// </summary>
        public bool IsCanUpdate => !string.IsNullOrWhiteSpace(DeviceSN) && LocalSN != DeviceSN;

        public void SetWhenReadingDeviceSN()
        {
            DeviceSN = string.Empty;
            ReadDeviceSNStatus = ReadDeviceSNStatus.Reading;
            ReadDeviceSNStr = "Reading...";
        }

        public void ReceivedDeviceSN(string deviceSN)
        {
            if (string.IsNullOrWhiteSpace(deviceSN))
            {
                DeviceSN = string.Empty;
                ReadDeviceSNStatus = ReadDeviceSNStatus.Error;
                ReadDeviceSNStr = "Received null or white space series number";
            }
            else
            {
                DeviceSN = deviceSN;
                ReadDeviceSNStatus = LocalSN == DeviceSN ? ReadDeviceSNStatus.Identical : ReadDeviceSNStatus.Different;
                ReadDeviceSNStr = LocalSN == DeviceSN ? string.Empty : deviceSN;
            }
        }

        public void ReceivedErrorWhenGetDeviceSN(string errorMsg)
        {
            DeviceSN = string.Empty;
            ReadDeviceSNStatus = ReadDeviceSNStatus.Error;
            ReadDeviceSNStr = errorMsg;
        }

        /// <summary>
        /// 更新SN号
        /// </summary>
        /// <returns>更新成功时返回<see langword="true"/>，不可更新时返回<see langword="false"/></returns>
        public bool TryUpdate(DateTime installTime)
        {
            if (!IsCanUpdate)
            {
                return false;
            }

            LocalSN = DeviceSN;
            InstallTime = installTime;
            ReadDeviceSNStatus = ReadDeviceSNStatus.Identical;
            ReadDeviceSNStr = string.Empty;
            return true;
        }
    }
}
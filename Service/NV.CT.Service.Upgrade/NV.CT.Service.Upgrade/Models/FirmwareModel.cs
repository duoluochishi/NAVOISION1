using System.Collections.Generic;
using System.Collections.ObjectModel;
using NV.CT.FacadeProxy.Models.Upgrade;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.Upgrade.Enums;

namespace NV.CT.Service.Upgrade.Models
{
    public class FirmwareModel : ViewModelBase
    {
        #region Field

        private int _id;
        private int _num;
        private string _displayName = string.Empty;
        private string _upgradeName = string.Empty;
        private FirmwareType _firmwareType = FirmwareType.Node;
        private string _packagePath = string.Empty;
        private string _currentVersion = string.Empty;
        private string _upgradeVersion = string.Empty;
        private FirmwareModel? _parent;
        private ObservableCollection<FirmwareModel>? _children;
        private bool? _isChecked = false;
        private bool _isEnabled;
        private bool _isEverUpgraded;
        private bool _isCanUpgrade;
        private string _canUpgradeMsg = string.Empty;
        private GetVersionStatusType _getVerStatus;
        private string _getVerMsg = string.Empty;
        private UpgradeStatusType _upgradeStatus = UpgradeStatusType.None;
        private double _progress;
        private string _upgradeMsg = string.Empty;

        #endregion

        /// <summary>
        /// ID
        /// </summary>
        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// 序号
        /// <para>用于确认对应数组（同类型多固件）内的index</para>
        /// </summary>
        public int Num
        {
            get => _num;
            set => SetProperty(ref _num, value);
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value.GetLocalizationStr());
        }

        /// <summary>
        /// 对应升级包的名称
        /// </summary>
        public string UpgradeName
        {
            get => _upgradeName;
            set => SetProperty(ref _upgradeName, value);
        }

        /// <summary>
        /// 固件类型
        /// </summary>
        public FirmwareType FirmwareType
        {
            get => _firmwareType;
            set => SetProperty(ref _firmwareType, value);
        }

        /// <summary>
        /// zip包路径
        /// </summary>
        public string PackagePath
        {
            get => _packagePath;
            set => SetProperty(ref _packagePath, value);
        }

        /// <summary>
        /// 允许升级的文件列表
        /// </summary>
        public List<string> CanUpgradeFiles { get; set; } = new();

        /// <summary>
        /// 读取的固件当前版本号
        /// </summary>
        public string CurrentVersion
        {
            get => _currentVersion;
            set => SetProperty(ref _currentVersion, value);
        }

        /// <summary>
        /// 解析升级zip包的升级版本号
        /// </summary>
        public string UpgradeVersion
        {
            get => _upgradeVersion;
            set => SetProperty(ref _upgradeVersion, value);
        }

        public FirmwareModel? Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value);
        }

        public ObservableCollection<FirmwareModel>? Children
        {
            get => _children;
            set => SetProperty(ref _children, value);
        }

        public bool? IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// 是否已经执行过升级逻辑
        /// </summary>
        public bool IsEverUpgraded
        {
            get => _isEverUpgraded;
            set => SetProperty(ref _isEverUpgraded, value);
        }

        /// <summary>
        /// 是否可升级状态
        /// </summary>
        public bool IsCanUpgrade
        {
            get => _isCanUpgrade;
            set => SetProperty(ref _isCanUpgrade, value);
        }

        /// <summary>
        /// 状态为不可升级时的错误信息
        /// </summary>
        public string CanUpgradeMsg
        {
            get => _canUpgradeMsg;
            set => SetProperty(ref _canUpgradeMsg, value);
        }

        /// <summary>
        /// 获取版本号的状态
        /// </summary>
        public GetVersionStatusType GetVerStatus
        {
            get => _getVerStatus;
            set => SetProperty(ref _getVerStatus, value);
        }

        /// <summary>
        /// 当 <see cref="GetVerStatus"/> 不是 <see cref="GetVersionStatusType.None"/> 或 <see cref="GetVersionStatusType.Success"/> 时的提示信息
        /// </summary>
        public string GetVerMsg
        {
            get => _getVerMsg;
            set => SetProperty(ref _getVerMsg, value);
        }

        /// <summary>
        /// 升级状态
        /// </summary>
        public UpgradeStatusType UpgradeStatus
        {
            get => _upgradeStatus;
            set => SetProperty(ref _upgradeStatus, value);
        }

        /// <summary>
        /// 升级进度
        /// </summary>
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        /// <summary>
        /// 升级有问题时的错误信息
        /// </summary>
        public string UpgradeMsg
        {
            get => _upgradeMsg;
            set => SetProperty(ref _upgradeMsg, value);
        }
    }
}
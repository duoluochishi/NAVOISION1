using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.FacadeProxy.Models.Upgrade;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.Upgrade.Enums;

namespace NV.CT.Service.Upgrade.Models
{
    public class FirmwareTypeModel : ViewModelBase
    {
        private string _displayName = string.Empty;
        private FirmwareType _firmwareType;
        private bool? _isChecked = false;
        private bool _isEnabled;

        public FirmwareTypeModel()
        {
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                SetProperty(ref _displayName, value.GetLocalizationStr());
            }
        }

        public FirmwareType FirmwareType
        {
            get => _firmwareType;
            set => SetProperty(ref _firmwareType, value);
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
    }
}
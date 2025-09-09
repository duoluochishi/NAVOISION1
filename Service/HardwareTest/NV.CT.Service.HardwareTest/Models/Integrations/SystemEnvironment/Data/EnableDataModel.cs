using NV.CT.Service.Common.Enums;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;

namespace NV.CT.Service.HardwareTest.Models.Integrations.SystemEnvironment.Data
{
    internal class EnableDataModel(SystemEnvironmentPartType partType, string title) : SystemEnvironmentDataAbstract(partType, SystemEnvironmentDataType.AlarmEnable, title)
    {
        private EnableType _isEnable;

        public EnableDataModel(SystemEnvironmentPartType partType, string title, bool enable) : this(partType, title)
        {
            IsEnable = enable ? EnableType.Enable : EnableType.Disable;
        }

        public EnableType IsEnable
        {
            get => _isEnable;
            set => SetProperty(ref _isEnable, value);
        }
    }
}
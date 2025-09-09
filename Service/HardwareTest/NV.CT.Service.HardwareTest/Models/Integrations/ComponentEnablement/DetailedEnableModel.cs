using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums.ShieldableComponent;

namespace NV.CT.Service.HardwareTest.Models.Integrations.ComponentEnablement
{
    public class DetailedEnableModel : ObservableObject
    {
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Index，从1开始
        /// </summary>
        public required int Index { get; init; }

        public required ShieldableComponentType ComponentType { get; init; }
    }
}
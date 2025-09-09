using CommunityToolkit.Mvvm.ComponentModel;

namespace NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions
{
    public partial class RelatedComponent : ObservableObject
    {
        /// <summary>
        /// 名称
        /// </summary>
        [ObservableProperty]
        private string name = string.Empty;
        /// <summary>
        /// 是否被选中
        /// </summary>
        [ObservableProperty]
        private bool isChecked;
        /// <summary>
        /// 是否使能
        /// </summary>
        [ObservableProperty]
        private bool isEnabled = true;
        /// <summary>
        /// Bit占位
        /// </summary>
        public int BitOffset { get; set; }
    }
}

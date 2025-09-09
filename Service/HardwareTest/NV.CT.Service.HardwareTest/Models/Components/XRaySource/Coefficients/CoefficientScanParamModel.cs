using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Service.HardwareTest.Models.Components.XRaySource.Coefficients
{
    public partial class CoefficientScanParamModel : ObservableObject
    {
        /// <summary>
        /// 曝光时间，单位ms
        /// </summary>
        [ObservableProperty]
        private float _exposureTime = 10;

        /// <summary>
        /// 帧时间，单位ms
        /// </summary>
        [ObservableProperty]
        private float _frameTime = 240;

        /// <summary>
        /// 大/小焦点
        /// </summary>
        [ObservableProperty]
        private FocalType _focus = FocalType.Small;

        /// <summary>
        /// 扫描张数
        /// </summary>
        [ObservableProperty]
        private uint _framesCount = 1;
    }
}
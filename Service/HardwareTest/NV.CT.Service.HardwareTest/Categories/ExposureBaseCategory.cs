using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using NV.CT.FacadeProxy.Common.Fields;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using NV.MPS.Configuration;
using System;

namespace NV.CT.Service.HardwareTest.Categories
{
    public partial class ExposureBaseCategory : ObservableObject
    {
        public ExposureBaseCategory()
        {
            kVs = [XRaySourceDefaults.ScanTubekV, 0, 0, 0, 0, 0, 0, 0];
            mAs = [XRaySourceDefaults.ScanTubemA, 0, 0, 0, 0, 0, 0, 0];
            activeExposureSourceCount = 24;
            exposureDelayTime = 10000;

            UpdateECGParameters();
        }

        public bool IsInTwelveExposureMode => ExposureMode is ExposureMode.Twelve;
        public bool IsNotInScoutScanOption => ScanOption is not (ScanOption.Surview or ScanOption.DualScout);

        /** 电压 **/
        [ObservableProperty]
        private uint[] kVs;
        /** 电流 **/
        [ObservableProperty]
        private uint[] mAs;
        /** 曝光时间 **/
        [ObservableProperty]
        private float exposureTime = XRaySourceDefaults.ExposureTime;
        /** 帧时间 **/
        [ObservableProperty]
        private float frameTime = XRaySourceDefaults.FrameTime;
        /** 采集图像张数 **/
        [ObservableProperty]
        private uint totalFrames = XRaySourceDefaults.SeriesLength;
        /** 自动删图张数 **/
        [ObservableProperty]
        private uint autoDeleteNumber;
        /** 扫描模式 **/
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotInScoutScanOption))]
        private ScanOption scanOption = ScanOption.Axial;
        /** 扫描类型 **/
        [ObservableProperty]
        private ScanMode scanMode = ScanMode.Plain;
        /** 曝光模式 **/
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsInTwelveExposureMode))]
        [NotifyPropertyChangedFor(nameof(ExposureSourceNumber))]
        private ExposureMode exposureMode = ExposureMode.Single;
        /** 参与曝光源数 **/
        [ObservableProperty]
        private uint activeExposureSourceCount;
        /** 曝光触发模式 **/
        [ObservableProperty]
        private ExposureTriggerMode exposureTriggerMode = ExposureTriggerMode.TimeTrigger;
        /** 12源曝光模式射线源起始Index **/
        [ObservableProperty]
        private XRaySourceIndex twelveExposureModeXRaySourceStartIndex = XRaySourceIndex.XRaySource05;
        /** 波太使能 **/
        [ObservableProperty]
        private CommonSwitch bowtie = CommonSwitch.Disable;
        /** 限束器Offset使能 **/
        [ObservableProperty]
        private CommonSwitch collimatorOffsetEnable = CommonSwitch.Enable;
        /** 限束器遮挡使能 **/
        [ObservableProperty]
        private CommonSwitch collimatorSwitch = CommonSwitch.Disable;
        /** 能谱模式 **/
        [ObservableProperty]
        private EnergySpectrumMode energySpectrumMode = EnergySpectrumMode.Single;
        /** 开口 **/
        [ObservableProperty]
        private CollimatorValidOpenType collimatorOpenType = CollimatorValidOpenType.NearCenter_288;
        /** 床门触发控制使能 **/
        [ObservableProperty]
        private CommonSwitch tableTriggerEnable = CommonSwitch.Disable;
        /** 投影图数量 **/
        [ObservableProperty]
        private uint framesPerCycle = 1080;
        /** 旋阳曝光时间 **/
        [ObservableProperty]
        private float exposureDelayTime = XRaySourceDefaults.ExposureDelayTime;
        /** 当前射线源编号 **/
        [ObservableProperty]
        private XRaySourceIndex xRaySourceIndex = XRaySourceIndex.All;
        /** 大小焦点 **/
        [ObservableProperty]
        private FocalType focus = FocalType.Small;
        /** 子节点曝光使能 **/
        [ObservableProperty]
        private uint exposureRelatedChildNodesConfig = 8193;
        /** 子节点硬件测试控制寄存器 **/
        [ObservableProperty]
        private uint slaveDevTest = XRaySourceDefaults.CTBoxSlaveDevTest;
        /** post序列长度，用于post offset采图 **/
        [ObservableProperty]
        private uint postOffsetFrames = 50;
        /** 各节点prepare超时时间(ms) **/
        [ObservableProperty]
        private uint prepareTimeout = 3000;
        /** 曝光超时时间(ms) **/
        [ObservableProperty]
        private uint exposureTimeout = 3000;
        /** 机架速度（0.01°/s） **/
        [ObservableProperty]
        private uint gantryVelocity = 400;
        /** 机架起始角度 **/
        [ObservableProperty]
        private uint gantryStartAngle = 8000;
        /** 机架旋转方向 **/
        [ObservableProperty]
        private GantryDirection gantryDirection = GantryDirection.Clockwise;
        /** 允许错误源数 **/
        [ObservableProperty]
        private uint allowErrorXRaySourceCount = 0;

        [ObservableProperty]
        private CommonSwitch eCGEnable = CommonSwitch.Disable;
        [ObservableProperty]
        private float limitation = 0.67f;
        [ObservableProperty]
        private float heartRate = 90;
        [ObservableProperty]
        private float phasePosition = 0.7f;
        [ObservableProperty]
        private float phaseWidth = 0.2f;

        public uint SourceNumber => SystemConfig.SourceComponentConfig.SourceComponent.SourceCount;
        public uint ExposureSourceNumber => ExposureMode switch 
        {
            ExposureMode.Single => 1,
            ExposureMode.Dual => 2,
            ExposureMode.Three => 3,
            ExposureMode.Six => 6,
            ExposureMode.Twelve => 12,
            _ => 1
        };

        [ObservableProperty]
        private float eCG_TotalExposureTime = 0;
        [ObservableProperty]
        private float eCG_HT = 0;
        [ObservableProperty]
        private float eCG_GateStartPosition = 0;
        [ObservableProperty]
        private float eCG_GateStopPosition = 0;
        [ObservableProperty]
        private int eCG_GateW = 0;
        [ObservableProperty]
        private float eCG_Beats = 0;
        [ObservableProperty]
        private float eCG_GantryVelocityA = 0;
        [ObservableProperty]
        private float eCG_GantrySweepAngle = 0;
        [ObservableProperty]
        private float eCG_GantryVelocityB = 0;
        /// <summary>
        /// 单位 °
        /// </summary>
        [ObservableProperty]
        private float eCG_ActualGantryVelocity = 0;
        [ObservableProperty]
        private float eCG_ImageNumber = 0;
        [ObservableProperty]
        private float eCG_GantryRotateAngle = 0;
        [ObservableProperty]
        private float eCG_ProjectionNumber = 0;
        [ObservableProperty]
        private float eCG_ScanTime = 0;
        [ObservableProperty]
        private float eCG_AngleInterval = 0;
        [ObservableProperty]
        private float eCG_AngleRange = 0;

        private void UpdateECGParameters() 
        {
            ECG_TotalExposureTime = FrameTime * TotalFrames / ExposureSourceNumber;

            ECG_HT = 60 / HeartRate * 1000;
            ECG_GateStartPosition = ECG_HT * (PhasePosition - PhaseWidth / 2);
            ECG_GateStopPosition = ECG_HT * (PhasePosition + PhaseWidth / 2);
            ECG_GateW = (int)((ECG_GateStopPosition - ECG_GateStartPosition) / 4 * 4);
            ECG_Beats = ECG_TotalExposureTime / ECG_GateW;

            ECG_GantryVelocityA = 360 / SourceNumber / (ECG_HT + ECG_GateW) * Coefficients.ExpandCoef_1000;
            ECG_GantrySweepAngle = 360 / SourceNumber * Limitation;
            ECG_GantryVelocityB = ECG_GantrySweepAngle / ECG_Beats / ECG_GateW * Coefficients.ExpandCoef_1000;
            ECG_ActualGantryVelocity = ECG_Beats > 1 ? ECG_GantryVelocityA : ECG_GantryVelocityB;

            ECG_ImageNumber = 360 / SourceNumber / ECG_ActualGantryVelocity / (FrameTime / 1000);
            ECG_GantryRotateAngle = ECG_ActualGantryVelocity * ECG_Beats * ECG_HT * Coefficients.ReduceCoef_1000;
            ECG_ProjectionNumber = ECG_GateW / FrameTime * ExposureSourceNumber;
            ECG_ScanTime = ECG_HT * ECG_Beats * Coefficients.ReduceCoef_1000;
            ECG_AngleInterval = ECG_ActualGantryVelocity * FrameTime * SourceNumber / ExposureSourceNumber / 1000;
            ECG_AngleRange = ECG_AngleInterval * TotalFrames;
        }

        partial void OnScanOptionChanged(ScanOption oldValue, ScanOption newValue)
        {
            ActiveExposureSourceCount = newValue switch
            {
                ScanOption.Surview => 1,
                ScanOption.DualScout => 2,
                _ => 24
            };
        }

        partial void OnECGEnableChanged(CommonSwitch oldValue, CommonSwitch newValue)
        {
            UpdateECGParameters();
        }

        partial void OnLimitationChanged(float oldValue, float newValue)
        {
            UpdateECGParameters();
        }

        partial void OnHeartRateChanged(float oldValue, float newValue)
        {
            UpdateECGParameters();
        }

        partial void OnPhasePositionChanged(float oldValue, float newValue)
        {
            UpdateECGParameters();
        }

        partial void OnPhaseWidthChanged(float oldValue, float newValue)
        {
            UpdateECGParameters();
        }

        partial void OnTotalFramesChanged(uint oldValue, uint newValue)
        {
            UpdateECGParameters();
        }

        partial void OnExposureTimeChanged(float oldValue, float newValue)
        {
            ExposureTimeChanged?.Invoke(newValue);

            UpdateECGParameters();
        }

        partial void OnFrameTimeChanged(float oldValue, float newValue)
        {
            FrameTimeChanged?.Invoke(newValue);

            UpdateECGParameters();
        }

        partial void OnXRaySourceIndexChanged(XRaySourceIndex oldValue, XRaySourceIndex newValue)
        {
            ScanOption = (XRaySourceIndex == XRaySourceIndex.All) ? ScanOption.Axial : ScanOption.Surview;
            XRaySourceIndexChanged?.Invoke(newValue);
        }

        partial void OnExposureDelayTimeChanged(float oldValue, float newValue)
        {
            DelayExposureTimeChanged?.Invoke(newValue);
        }

        partial void OnExposureTriggerModeChanged(ExposureTriggerMode oldValue, ExposureTriggerMode newValue)
        {
            if (newValue is ExposureTriggerMode.AngleTrigger)
            {
                var velocity = Math.Ceiling(((360 / 24) / (FrameTime * (int)FramesPerCycle * Coefficients.ReduceCoef_1000)) * Coefficients.ExpandCoef_100);
                GantryVelocity = velocity > 500 ? 500 : (uint)velocity;
            }
        }

        partial void OnFramesPerCycleChanged(uint oldValue, uint newValue)
        {
            if (ExposureTriggerMode is ExposureTriggerMode.AngleTrigger) 
            {
                var velocity = Math.Ceiling(((360 / 24) / (FrameTime * (int)FramesPerCycle * Coefficients.ReduceCoef_1000)) * Coefficients.ExpandCoef_100);

                GantryVelocity = velocity > 500 ? 500 : (uint)velocity;
            }
        }

        public event Action<XRaySourceIndex>? XRaySourceIndexChanged;
        public event Action<float>? ExposureTimeChanged;
        public event Action<float>? FrameTimeChanged;
        public event Action<float>? DelayExposureTimeChanged;

    }

}

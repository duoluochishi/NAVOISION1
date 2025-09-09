using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.Service.HardwareTest.Share.Enums;

namespace NV.CT.Service.HardwareTest.Categories
{
    public partial class DetectorBaseCategory : ObservableObject
    {
        /// <summary>
        /// 双倍读取
        /// </summary>
        [ObservableProperty]
        private ReadType currentDoubleRead = ReadType.DoubleRead;
        /// <summary>
        /// 图像翻转
        /// </summary>
        [ObservableProperty]
        private uint imageFlip = 9;
        /// <summary>
        /// 配置PD寄存器
        /// </summary>
        [ObservableProperty]
        private uint writeConfig;
        /// <summary>
        /// 工作模式
        /// </summary>
        [ObservableProperty]
        private ImageMode currentImageMode = ImageMode.Normal_Data_Acquisition;
        /// <summary>
        /// 增益积分范围
        /// </summary>
        [ObservableProperty]
        private Gain currentGain = Gain.Fix24pC;
        /// <summary>
        /// 像素合并模式
        /// </summary>
        [ObservableProperty]
        private ScanBinning currentBinning = ScanBinning.Bin11;
        /// <summary>
        /// 快门模式
        /// </summary>
        [ObservableProperty]
        private ShutterMode currentShutterMode = ShutterMode.Global;
        /// <summary>
        /// 扫描时曝光时间
        /// </summary>
        [ObservableProperty]
        private float exposureTime;
        /// <summary>
        /// 延迟曝光时间
        /// </summary>
        [ObservableProperty]
        private float delayExposureTime;
        /// <summary>
        /// 扫描时帧时间
        /// </summary>
        [ObservableProperty]
        private float frameTime;
        /// <summary>
        /// 采集模式
        /// </summary>
        [ObservableProperty]
        private AcquisitionMode currentAcquisitionMode = AcquisitionMode.HighLevelTriggering;
        /// <summary>
        /// Read Delay
        /// </summary>
        [ObservableProperty]
        private int readDealyTime;
        /// <summary>
        /// 设置周期状态上传时间间隔，单位毫秒(ms)
        /// </summary>
        [ObservableProperty]
        private int heartBeatTimeInterval = 1000;
        /// <summary>
        /// 打印模块使能
        /// </summary>
        [ObservableProperty]
        private CommonSwitch printfEnable = CommonSwitch.Disable;
        /// <summary>
        /// 检出板温度控制寄存器
        /// </summary>
        [ObservableProperty]
        private int targetTemperature;
        /// <summary>
        /// 散射板增益设置
        /// </summary>
        [ObservableProperty]
        private ScatteringDetectorGain currentScatteringGain = ScatteringDetectorGain.Fix16Pc;
        /// <summary>
        /// 散射板积分时间
        /// </summary>
        [ObservableProperty]
        private float currentSpiTime;
        /// <summary>
        /// 编码格式 - 原始编码，Flag编码，LOG2编码，默认为原始编码
        /// </summary>
        [ObservableProperty]
        private DetectorEncodeMode encodeMode = DetectorEncodeMode.Origin;
        /// <summary>
        /// Trigger信号相对于Spot延时tDelay的时间再输出,默认值0.4ms
        /// </summary>
        [ObservableProperty]
        private float rDelay = 0.4f;
        /// <summary>
        /// Trigger信号提前驱动时间 bit15 - bit0:TRIG信号提前驱动时间,默认值0.5ms
        /// </summary>
        [ObservableProperty]
        private float tDelay = 0.5f;
        /// <summary>
        /// preoffset使能
        /// </summary>
        [ObservableProperty]
        private CommonSwitch preOffsetEnable = CommonSwitch.Disable;
        /// <summary>
        /// 探测器PreOffset采集总帧数，默认值为15
        /// </summary>
        [ObservableProperty] 
        private int preOffsetAcqTotalFrame = 15;
        /// <summary>
        /// 探测器PreOffset有效帧前删除张数，默认为3
        /// </summary>
        [ObservableProperty]
        private int preOffsetAcqStartVaildFrame = 3;

        /// <summary>
        /// 散射板积分时间与ExposureTime默认保持一致
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnExposureTimeChanged(float oldValue, float newValue)
        {
            this.CurrentSpiTime = newValue;
        }
        /// <summary>
        /// 目标温度输入校验
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnTargetTemperatureChanged(int oldValue, int newValue)
        {
            if (newValue < 160) 
            {
                this.TargetTemperature = 160;
            }

            if (newValue > 400) 
            {
                this.TargetTemperature = 400;
            }
        }
    }
}

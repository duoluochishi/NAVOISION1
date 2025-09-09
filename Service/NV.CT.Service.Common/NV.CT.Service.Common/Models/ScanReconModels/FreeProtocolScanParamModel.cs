using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.Service.Common.Enums;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.Common.Models.ScanReconModels
{
    public class FreeProtocolScanParamModel : ScanParamModel
    {
        #region Field

        private string _studyUID = string.Empty;
        private SwitchType _bowtieSwitch;
        private SwitchType _collimatorSwitch;
        private EnableType _tableTriggerEnable;
        private uint _currentXRaySourceIndex;
        private uint _exposureRelatedChildNodesConfig;
        private uint _slaveDevTest;
        private float _prepareTimeout;
        private float _exposureTimeout;
        private ReadType _currentDoubleRead;
        private uint _imageFlip;
        private uint _writeConfig;
        private ImageMode _currentImageMode;
        private ShutterMode _currentShutterMode;
        private AcquisitionMode _currentAcquisitionMode;
        private float _readDealyTime;
        private float _heartBeatTimeInterval;
        private EnableType _printfEnable;
        private float _targetTemperature;
        private DetectorEncodeMode _encodeMode;
        private EnableType _preOffsetEnable;
        private int _preOffsetAcqStartVaildFrame;
        private ScatteringDetectorGain _currentScatteringGain;

        #endregion

        public FreeProtocolScanParamModel()
        {
            RawDataType = RawDataType.DarkL;
            Voltage = [100, 0, 0, 0, 0, 0, 0, 0];
            Current = [80, 0, 0, 0, 0, 0, 0, 0];
            ExposureTime = 5;
            FrameTime = 10;
            TotalFrames = 100;
            ScanOption = ScanOption.Axial;
            ScanMode = ScanMode.Plain;
            ExposureMode = ExposureMode.Single;
            BowtieSwitch = SwitchType.On;
            BowtieEnable = EnableType.Disable;
            CollimatorSwitch = SwitchType.On;
            CollimatorOpenMode = FacadeProxy.Common.Enums.Collimator.CollimatorOpenMode.NearCenter;
            CollimatorZ = 288;
            TableTriggerEnable = EnableType.Disable;
            FramesPerCycle = 1080;
            AutoScan = EnableType.Enable;
            ExposureDelayTime = 20;
            XRaySourceIndex = 0;
            Focal = FocalType.Small;
            ExposureRelatedChildNodesConfig = 8193; //0x2001: [0]ifbox + [13]workstation
            SlaveDevTest = 1;
            PostOffsetFrames = 0;
            PrepareTimeout = 3000f;
            ExposureTimeout = 3000f;
            TableEndPosition = 0;
            TableDirection = TableDirection.In;
            GantryStartPosition = 60;
            GantryEndPosition = 540;
            GantryDirection = GantryDirection.CounterClockwise;
            GantrySpeed = 4;
            CurrentDoubleRead = ReadType.DoubleRead;
            ImageFlip = 9;
            WriteConfig = 0;
            CurrentImageMode = ImageMode.Normal_Data_Acquisition;
            Gain = Gain.Fix24pC;
            ScanBinning = ScanBinning.Bin11;
            CurrentShutterMode = ShutterMode.Global;
            CurrentAcquisitionMode = AcquisitionMode.HighLevelTriggering;
            ReadDealyTime = 0f;
            HeartBeatTimeInterval = 90f;
            PrintfEnable = EnableType.Disable;
            TargetTemperature = 26.0f;
            EncodeMode = DetectorEncodeMode.Origin;
            RDelay = 0.4f;
            TDelay = 0.5f;
            SpotDelay = 0;
            PreOffsetFrames = 0;
            PreOffsetEnable = EnableType.Disable;
            PreOffsetAcqStartVaildFrame = 0;
            CurrentScatteringGain = ScatteringDetectorGain.Disable;
        }

        #region Property
         
        /// <inheritdoc cref="DataAcquisitionParams.StudyUID"/>
        public string StudyUID
        {
            get => _studyUID;
            set => SetProperty(ref _studyUID, value);
        }

        #region ExposureParams

        /// <inheritdoc cref="ExposureParams.Bowtie"/>
        /// <remarks>注意！在此处使用<see cref="SwitchType"/>枚举</remarks>
        public SwitchType BowtieSwitch
        {
            get => _bowtieSwitch;
            set => SetProperty(ref _bowtieSwitch, value);
        }

        /// <inheritdoc cref="ExposureParams.CollimatorSwitch"/>
        /// <remarks>注意！在此处使用<see cref="SwitchType"/>枚举</remarks>
        public SwitchType CollimatorSwitch
        {
            get => _collimatorSwitch;
            set => SetProperty(ref _collimatorSwitch, value);
        }

        /// <inheritdoc cref="ExposureParams.TableTriggerEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        public EnableType TableTriggerEnable
        {
            get => _tableTriggerEnable;
            set => SetProperty(ref _tableTriggerEnable, value);
        }

        /// <inheritdoc cref="ExposureParams.XRaySourceIndex"/>
        public uint XRaySourceIndex
        {
            get => _currentXRaySourceIndex;
            set => SetProperty(ref _currentXRaySourceIndex, value);
        }

        /// <inheritdoc cref="ExposureParams.ExposureRelatedChildNodesConfig"/>
        public uint ExposureRelatedChildNodesConfig
        {
            get => _exposureRelatedChildNodesConfig;
            set => SetProperty(ref _exposureRelatedChildNodesConfig, value);
        }

        /// <inheritdoc cref="ExposureParams.SlaveDevTest"/>
        public uint SlaveDevTest
        {
            get => _slaveDevTest;
            set => SetProperty(ref _slaveDevTest, value);
        }

        /// <inheritdoc cref="ExposureParams.PrepareTimeout"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位：毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        public float PrepareTimeout
        {
            get => _prepareTimeout;
            set => SetProperty(ref _prepareTimeout, value);
        }

        /// <inheritdoc cref="ExposureParams.ExposureTimeout"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位：毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        public float ExposureTimeout
        {
            get => _exposureTimeout;
            set => SetProperty(ref _exposureTimeout, value);
        }

        #endregion

        #region DetectorParams

        /// <inheritdoc cref="DetectorParams.CurrentDoubleRead"/>
        public ReadType CurrentDoubleRead
        {
            get => _currentDoubleRead;
            set => SetProperty(ref _currentDoubleRead, value);
        }

        /// <inheritdoc cref="DetectorParams.ImageFlip"/>
        public uint ImageFlip
        {
            get => _imageFlip;
            set => SetProperty(ref _imageFlip, value);
        }

        /// <inheritdoc cref="DetectorParams.WriteConfig"/>
        public uint WriteConfig
        {
            get => _writeConfig;
            set => SetProperty(ref _writeConfig, value);
        }

        /// <inheritdoc cref="DetectorParams.CurrentImageMode"/>
        public ImageMode CurrentImageMode
        {
            get => _currentImageMode;
            set => SetProperty(ref _currentImageMode, value);
        }

        /// <inheritdoc cref="DetectorParams.CurrentShutterMode"/>
        public ShutterMode CurrentShutterMode
        {
            get => _currentShutterMode;
            set => SetProperty(ref _currentShutterMode, value);
        }

        /// <inheritdoc cref="DetectorParams.CurrentAcquisitionMode"/>
        public AcquisitionMode CurrentAcquisitionMode
        {
            get => _currentAcquisitionMode;
            set => SetProperty(ref _currentAcquisitionMode, value);
        }

        /// <inheritdoc cref="DetectorParams.ReadDealyTime"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位：毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        public float ReadDealyTime
        {
            get => _readDealyTime;
            set => SetProperty(ref _readDealyTime, value);
        }

        /// <inheritdoc cref="DetectorParams.HeartBeatTimeInterval"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位：毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        public float HeartBeatTimeInterval
        {
            get => _heartBeatTimeInterval;
            set => SetProperty(ref _heartBeatTimeInterval, value);
        }

        /// <inheritdoc cref="DetectorParams.PrintfEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        public EnableType PrintfEnable
        {
            get => _printfEnable;
            set => SetProperty(ref _printfEnable, value);
        }

        /// <inheritdoc cref="DetectorParams.TargetTemperature"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 度(°)</para>
        /// <para>精度: 小数点后一位，即0.1°</para>
        /// </remarks>
        public float TargetTemperature
        {
            get => _targetTemperature;
            set => SetProperty(ref _targetTemperature, value);
        }

        /// <inheritdoc cref="DetectorParams.EncodeMode"/>
        public DetectorEncodeMode EncodeMode
        {
            get => _encodeMode;
            set => SetProperty(ref _encodeMode, value);
        }

        /// <inheritdoc cref="DetectorParams.PreOffsetEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        public EnableType PreOffsetEnable
        {
            get => _preOffsetEnable;
            set => SetProperty(ref _preOffsetEnable, value);
        }

        /// <inheritdoc cref="DetectorParams.PreOffsetAcqStartVaildFrame"/>
        public int PreOffsetAcqStartVaildFrame
        {
            get => _preOffsetAcqStartVaildFrame;
            set => SetProperty(ref _preOffsetAcqStartVaildFrame, value);
        }

        /// <inheritdoc cref="DetectorParams.CurrentScatteringGain"/>
        public ScatteringDetectorGain CurrentScatteringGain
        {
            get => _currentScatteringGain;
            set => SetProperty(ref _currentScatteringGain, value);
        }

        #endregion

        #endregion

        public new DataAcquisitionParams Converter()
        {
            return Global.Instance.ServiceProvider.GetRequiredService<IMapper>().Map<DataAcquisitionParams>(this);
        }
    }
}
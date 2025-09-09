using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.Collimator;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using System.Xml.Serialization;

namespace NV.CT.Protocol.Models;

[Serializable]
public class ScanModel : BaseModel<MeasurementModel, ReconModel>
{
    [XmlArray("Recons")]
    [XmlArrayItem("Recon")]
    public override List<ReconModel> Children { get => base.Children; set => base.Children = value; }

    /// <summary>
    /// 拼接参数。
    /// 范围：1：1*1，2：1*2；4：2*2 代表xy*z
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public ScanBinning Binning => GetParameterValue<ScanBinning>(ProtocolParameterNames.SCAN_BINNING);

    /// <summary>
    /// 是否支持API语音
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public bool IsVoiceSupported => GetParameterValue<bool>(ProtocolParameterNames.SCAN_IS_VOICE_SUPPORTED, true);

    /// <summary>
    /// 前语音ID
    /// ≥0,0表示不适用
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint PreVoiceId => GetParameterValue<uint>(ProtocolParameterNames.SCAN_PRE_VOICE_ID);

    /// <summary>
    /// 前语音播放语音时长 单位：us
    /// </summary>
    public uint PreVoicePlayTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_PRE_VOICE_PLAY_TIME);

    /// <summary>
    /// 扫描前语音延迟时间 单位：微秒(μs) 范围：0-600 000 000 微秒，即0-600秒
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint PreVoiceDelayTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME);

    /// <summary>
    /// 扫描后语音播放语音ID
    /// ≥0,0表示不适用
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint PostVoiceId => GetParameterValue<uint>(ProtocolParameterNames.SCAN_POST_VOICE_ID);

    /// <summary>
    /// 是否联扫
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public bool AutoScan => GetParameterValue<bool>(ProtocolParameterNames.SCAN_AUTO_SCAN);

    /// <summary>
    /// 扫描编号
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public int ScanNumber => GetParameterValue<int>(ProtocolParameterNames.SCAN_NUMBER);

    /// <summary>
    /// 千伏（管电压）
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint[] Kilovolt => GetParameterValue<uint[]>(ProtocolParameterNames.SCAN_KILOVOLT);

    /// <summary>
    /// 电流（管电流）
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint[] Milliampere => GetParameterValue<uint[]>(ProtocolParameterNames.SCAN_MILLIAMPERE);

    /// <summary>
    /// 曝光时间；
    /// 单位：微秒(μs)；范围：1000-30000(μs) ,即：1-30毫秒。
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint ExposureTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_EXPOSURE_TIME);

    /// <summary>
    /// 帧时间=曝光时间+间隔时间(大于等于1000微秒(μs)) 
    /// 单位：微秒(μs)；范围：2000-30,000,000(μs) ,即：2毫秒- 30秒。
    /// 间隔时间=帧时间-曝光时间
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint FrameTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_FRAME_TIME);

    /// <summary>
    /// 单圈投影图数量。 
    /// 范围：1,180，360，720，1080；
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint FramesPerCycle => GetParameterValue<uint>(ProtocolParameterNames.SCAN_FRAMES_PER_CYCLE);

    [XmlIgnore, JsonIgnore]
    public uint ScanLength => GetParameterValue<uint>(ProtocolParameterNames.SCAN_LENGTH, true);

    /// <summary>
    /// 扫描方式：平扫、轴扫、螺旋扫描
    /// 范围：1-SURVIEW(定位片)，2-AXIAL(轴扫)，3-HELICAL(螺旋)，4-DualScout正+侧(未实现)
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public ScanOption ScanOption => GetParameterValue<ScanOption>(ProtocolParameterNames.SCAN_OPTION);

    /// <summary>
    /// 扫描模式：平扫、灌注扫描、小计量测试扫描
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public ScanMode ScanMode => GetParameterValue<ScanMode>(ProtocolParameterNames.SCAN_MODE);

    /// <summary>
    /// 扫描的生数据类型，比如，DarkH，DarkL，Bowtie,GainH,GainL
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public RawDataType RawDataType => GetParameterValue<RawDataType>(ProtocolParameterNames.SCAN_RAW_DATA_TYPE);

    /// <summary>
    /// 生数据路径。 对于校准模块，存在调试模式，手动指定生数据文件夹； 在此之前，通过StudyUID\SacnUID组装的路径，在遇到余晖校准时，无法应付“特定根目录+ 485/490/496/502等不固定参数”
    /// MCS不使用
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public string RawDataDirectory => string.Empty;

    /// <summary>
    /// 曝光触发模式（时间，角度，床位置）
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public ExposureTriggerMode ExposureTrigger => GetParameterValue<ExposureTriggerMode>(ProtocolParameterNames.SCAN_EXPOSURE_TRIGGER);

    /// <summary>
    /// ECG心电图字段 范围：门控标识：0=None；1=ECG前瞻；2=ECG回顾；3=呼吸;4=时间;5=介入扫描
    /// </summary>
    [Obsolete]
    [XmlIgnore, JsonIgnore]
    public TriggerMode TriggerMode => GetParameterValue<TriggerMode>(ProtocolParameterNames.SCAN_TRIGGER_MODE);

    /// <summary>
    /// 门控期相起始位置
    /// TriggerMode为前瞻时使用
    /// 备注：硬件根据该值确定曝光开始位置 范围：开始门控期相(0~100)
    /// </summary>
    [Obsolete]
    [XmlIgnore, JsonIgnore]
    public ushort TriggerStart => GetParameterValue<ushort>(ProtocolParameterNames.SCAN_TRIGGER_START);

    /// <summary>
    /// 门控期相结束位置
    /// TriggerMode为前瞻时使用
    /// 备注：硬件根据该值确定曝光结束位置 范围：结束门控期相(0~100)
    /// </summary>
    [Obsolete]
    [XmlIgnore, JsonIgnore]
    public ushort TriggerEnd => GetParameterValue<ushort>(ProtocolParameterNames.SCAN_TRIGGER_END);

    /// <summary>
    /// 曝光模式(球管数量)。
    /// 曝光模式：1=单源，2=双源(能谱)，3=三源，6=6源(用于心脏扫描)
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public ExposureMode ExposureMode => GetParameterValue<ExposureMode>(ProtocolParameterNames.SCAN_EXPOSURE_MODE);

    /// <summary>
    /// 曝光源总数。
    /// 单次扫描使用到实际曝光源总数
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint ActiveExposureSourceCount => GetParameterValue<uint>(ProtocolParameterNames.SCAN_ACTIVE_EXPOSURE_SOURCE_COUNT, true);

    /// <summary>
    /// 扫描方向
    /// In：进床，Out：出床。
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public TableDirection TableDirection => GetParameterValue<TableDirection>(ProtocolParameterNames.SCAN_TABLE_DIRECTION);

    [XmlIgnore, JsonIgnore]
    public uint SmallAngleDeleteLength => GetParameterValue<uint>(ProtocolParameterNames.SCAN_SMALL_ANGLE_DELETE_LENGTH);

    [XmlIgnore, JsonIgnore]
    public uint LargeAngleDeleteLength => GetParameterValue<uint>(ProtocolParameterNames.SCAN_LARGE_ANGLE_DELETE_LENGTH);

    [XmlIgnore, JsonIgnore]
    public uint PostDeleteLength => GetParameterValue<uint>(ProtocolParameterNames.SCAN_POST_DELETE_LENGTH);

    /// <summary>
    /// NearSmallAngle, NearCenter
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public CollimatorOpenMode CollimatorOpenMode => GetParameterValue<CollimatorOpenMode>(ProtocolParameterNames.SCAN_COLLIMATOR_OPEN_MODE);

    /// <summary>
    /// 限束器开口位置Z(0-100)【准直铅挡片】
    /// 限速器开口位置:一些固定数代表固定挡位，其他代表厚度。0-100 代表开口百分比。
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint CollimatorZ => GetParameterValue<uint>(ProtocolParameterNames.SCAN_COLLIMATOR_Z);

    /// <summary>
    /// 准直探测器宽度 单位：1μm
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint CollimatorSliceWidth => GetParameterValue<uint>(ProtocolParameterNames.SCAN_COLLIMATOR_SLICE_WIDTH);

    /// <summary>
    /// 重建数据开始位置（1μm） 单位(精度):1μm 范围：-1700000 - 0
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public int ReconVolumeStartPosition => GetParameterValue<int>(ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION);

    /// <summary>
    /// 重建数据开始位置（1μm） 单位(精度):1μm 范围：-1700000 - 0
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public int ReconVolumeEndPosition => GetParameterValue<int>(ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION);

    /// <summary>
    /// 床运动开始位置包括加速距离
    /// 单位(精度):1μm 范围：-1700000 - 0
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public int TableStartPosition => GetParameterValue<int>(ProtocolParameterNames.SCAN_TABLE_START_POSITION);

    /// <summary>
    /// 床运动结束位置包括减速距离
    /// 单位(精度):1μm 范围：-1700000 - 0
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public int TableEndPosition => GetParameterValue<int>(ProtocolParameterNames.SCAN_TABLE_END_POSITION);

    /// <summary>
    /// 曝光数据开始位置（1μm） 单位(精度):1μm 范围：-1700000 - 0
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public int ExposureStartPosition => GetParameterValue<int>(ProtocolParameterNames.SCAN_EXPOSURE_START_POSITION);

    /// <summary>
    /// 曝光数据结束位置（1μm） 单位(精度):1μm 范围：-1700000 - 0
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public int ExposureEndPosition => GetParameterValue<int>(ProtocolParameterNames.SCAN_EXPOSURE_END_POSITION);

    /// <summary>
    /// 按下曝光键到出射线开始的时间 单位：微秒(μs) 范围：0-600 000 000 微秒，即0-600秒。
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint ExposureDelayTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME);

    /// <summary>
    /// 检查床加速度 单位：1μm/s^2
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint TableAcceleration => GetParameterValue<uint>(ProtocolParameterNames.SCAN_TABLE_ACCELERATION);

    /// <summary>
    /// 检查床加速时间 单位:us
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint TableAccelerationTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_TABLE_ACCELERATION_TIME);

    /// <summary>
    /// 设置球管的焦点,初始化默认小焦点 0：小焦点 1：大焦点
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public FocalType FocalType => GetParameterValue<FocalType>(ProtocolParameterNames.SCAN_FOCAL_TYPE);

    /// <summary>
    /// 是否时球馆预热
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public bool WarmUp => GetParameterValue<bool>(ProtocolParameterNames.SCAN_WARM_UP);

    /// <summary>
    /// 当球馆预热时，需要预热的球馆编号 如果为0表示预热24个球馆
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint WarmUpTubeNumber => GetParameterValue<uint>(ProtocolParameterNames.SCAN_WARM_UP_TUBE_NUMBER);

    /// <summary>
    /// 旋架开始位置（0.01°） 单位(精度) :0.01° 范围：6000-54000
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint GantryStartPosition => GetParameterValue<uint>(ProtocolParameterNames.SCAN_GANTRY_START_POSITION);

    /// <summary>
    /// 旋架结束位置（0.01°） 单位(精度) :0.01° 范围：6000-54000
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint GantryEndPosition => GetParameterValue<uint>(ProtocolParameterNames.SCAN_GANTRY_END_POSITION);

    /// <summary>
    /// 旋架运动方向 1：逆时针，2：顺时针
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public GantryDirection GantryDirection => GetParameterValue<GantryDirection>(ProtocolParameterNames.SCAN_GANTRY_DIRECTION);

    /// <summary>
    /// 旋架运动加速度 单位 0.01°/s^2
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint GantryAcceleration => GetParameterValue<uint>(ProtocolParameterNames.SCAN_GANTRY_ACCELERATION);

    /// <summary>
    /// 旋架运动加速时间 单位:us
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint GantryAccelerationTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_GANTRY_ACCELERATION_TIME);

    /// <summary>
    /// 旋架运动速度 单位:0.01°/s
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint GantrySpeed => GetParameterValue<uint>(ProtocolParameterNames.SCAN_GANTRY_SPEED);

    /// <summary>
    /// 自定义删图张数
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint AutoDeleteNum => GetParameterValue<uint>(ProtocolParameterNames.SCAN_AUTO_DELETE_NUMBER, true);

    /// <summary>
    /// 曝光总帧数，包括AutoDeleteNum
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint TotalFrames => GetParameterValue<uint>(ProtocolParameterNames.SCAN_TOTAL_FRAMES);

    /// <summary>
    /// 造影剂 Tag(0018,0010)
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public string ContrastBolusAgent => GetParameterValue(ProtocolParameterNames.SCAN_CONTRAST_BOLUS_AGENT, null, true);

    /// <summary>
    /// 造影剂药量 Tag(0018,1041)
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float? ContrastBolusVolume => GetParameterValue<float?>(ProtocolParameterNames.SCAN_CONTRAST_BOLUS_VOLUME, true);

    /// <summary>
    /// 造影剂速率,单位：ml/s Tag(0018,1046)
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float? ContrastFlowRate => GetParameterValue<float?>(ProtocolParameterNames.SCAN_CONTRAST_FLOW_RATE, true);

    /// <summary>
    /// 注入时间（秒） Tag(0018,1047)
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float? ContrastFlowDuration => GetParameterValue<float?>(ProtocolParameterNames.SCAN_CONTRAST_FLOW_DURATION, true);

    /// <summary>
    /// 每毫升（稀释的）制剂的毫克活性成分. Tag(0018,1049)
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float? ContrastBolusIngredientConcentration => GetParameterValue<float?>(ProtocolParameterNames.SCAN_CONTRAST_BOLUS_INGREDIENT_CONCENTRATION, true);

    /// <summary>
    /// 是否启用波钛(吕碗)
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public bool BowtieEnable => GetParameterValue<bool>(ProtocolParameterNames.SCAN_BOWTIE_ENABLE);

    [XmlIgnore, JsonIgnore]
    public Gain Gain => GetParameterValue<Gain>(ProtocolParameterNames.SCAN_GAIN);

    [XmlIgnore, JsonIgnore]
    public CTS.Enums.BodyPart BodyPart => GetParameterValue<CTS.Enums.BodyPart>(ProtocolParameterNames.BODY_PART);

    [XmlIgnore, JsonIgnore]
    public CTS.Enums.TubePositionType TubePositionType => GetParameterValue<TubePositionType>(ProtocolParameterNames.SCAN_TUBE_POSITION_TYPE);

    //TODO: 需处理成枚举
    [XmlIgnore, JsonIgnore]
    public TubePosition[] TubePositions => GetParameterValue<TubePosition[]>(ProtocolParameterNames.SCAN_TUBE_POSITIONS);

    /// <summary>
    /// 仅单/双定位像可用
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public int[] TubeNumbers => GetParameterValue<int[]>(ProtocolParameterNames.SCAN_TUBE_NUMBERS);

    [XmlIgnore, JsonIgnore]
    public byte[] DoseCurve => GetParameterValue<byte[]>(ProtocolParameterNames.SCAN_DOSE_CURVE, true);

    [XmlIgnore, JsonIgnore]
    public uint TableHeight => GetParameterValue<uint>(ProtocolParameterNames.SCAN_TABLE_HEIGHT);

    [XmlIgnore, JsonIgnore]
    public uint TableSpeed => GetParameterValue<uint>(ProtocolParameterNames.SCAN_TABLE_SPEED);

    [XmlIgnore, JsonIgnore]
    public uint TableFeed => GetParameterValue<uint>(ProtocolParameterNames.SCAN_TABLE_FEED);

    [XmlIgnore, JsonIgnore]
    public uint Pitch => GetParameterValue<uint>(ProtocolParameterNames.SCAN_PITCH);

    [XmlIgnore, JsonIgnore]
    public uint PreOffsetFrames => GetParameterValue<uint>(ProtocolParameterNames.SCAN_PRE_OFFSET_FRAMES);

    [XmlIgnore, JsonIgnore]
    public uint PostOffsetFrames => GetParameterValue<uint>(ProtocolParameterNames.SCAN_POST_OFFSET_FRAMES);

    [XmlIgnore, JsonIgnore]
    public FunctionMode FunctionMode => GetParameterValue<FunctionMode>(ProtocolParameterNames.SCAN_FUNCTION_MODE);

    //
    // 摘要:
    //     ifBox 发出的nvSync的Triger信号与其Spot 信号前沿的延时。 在ifbox程序实现 单位：微秒us，默认值：500
    [XmlIgnore, JsonIgnore]
    public uint RDelay => GetParameterValue<uint>(ProtocolParameterNames.SCAN_R_DELAY);

    //
    // 摘要:
    //     tubeintf收到ifBox的nvSync的Spot信号后自身进行Spot的延时。 在tubeintf程序实现 单位：微秒us，默认值：0
    [XmlIgnore, JsonIgnore]
    public uint TDelay => GetParameterValue<uint>(ProtocolParameterNames.SCAN_T_DELAY);

    //
    // 摘要:
    //     tubeintf收到ifBox的nvSync的Spot信号后自身进行Spot的延时。 在tubeintf程序实现 单位：微秒us，默认值：0
    [XmlIgnore, JsonIgnore]
    public uint SpotDelay => GetParameterValue<uint>(ProtocolParameterNames.SCAN_SPOT_DELAY);

    /// <summary>
    /// 循环扫次数
    /// TriggleMode=Time时使用 范围：0-32
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint Loops => GetParameterValue<uint>(ProtocolParameterNames.SCAN_LOOPS);

    /// <summary>
    /// 一次循环扫时间
    /// TriggleMode=Time时使用 范围：0~600000000=, 单位us, 即0-600s
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public uint LoopTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_LOOP_TIME);

    [XmlIgnore, JsonIgnore]
    public uint ScanFOV => GetParameterValue<uint>(ProtocolParameterNames.SCAN_FOV);

    [XmlIgnore, JsonIgnore]
    public uint ObjectFOV => GetParameterValue<uint>(ProtocolParameterNames.SCAN_OBJECT_FOV);

    /// <summary>
    /// 剂量通知，CTDI值
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float DoseNotificationCTDI => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_CTDI, true);

    /// <summary>
    /// 剂量通知，DLP值
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float DoseNotificationDLP => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_DLP, true);

    /// <summary>
    /// 剂量预估值，CTDI值
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float DoseEstimatedCTDI => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_ESTIMATED_CTDI, true);

    /// <summary>
    /// 剂量预估值，DLP值
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float DoseEstimatedDLP => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_ESTIMATED_DLP, true);

    /// <summary>
    /// 累积的剂量预估值，CTDI值
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float AccumulatedDoseEstimatedCTDI => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_ACCUMULATED_ESTIMATED_CTDI, true);

    /// <summary>
    /// 累积的剂量预估值，DLP值
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float AccumulatedDoseEstimatedDLP => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_ACCUMULATED_ESTIMATED_DLP, true);

    /// <summary>
    /// 剂量实际值，CTDI值
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float DoseEffectiveCTDI => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_CTDI, true);

    /// <summary>
    /// 剂量实际值，DLP值
    /// </summary>
    [XmlIgnore, JsonIgnore]
    public float DoseEffectiveDLP => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_DLP, true);

    [XmlIgnore, JsonIgnore]
    public float DoseEffectiveKVP => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_KVP, true);

    [XmlIgnore, JsonIgnore]
    public float DoseEffectiveMeanMA => GetParameterValue<float>(ProtocolParameterNames.SCAN_DOSE_EFFECTIVE_MEANMA, true);

    [XmlIgnore, JsonIgnore]
    public uint ActualScanTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_ACTUAL_SCAN_TIME, true);

    [XmlIgnore, JsonIgnore]
    public uint ActualScanLength => GetParameterValue<uint>(ProtocolParameterNames.SCAN_ACTUAL_SCAN_LENGTH, true);

    [XmlIgnore, JsonIgnore]
    public string DoseNotificationId => GetParameterValue(ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_ID, null, true);

    [XmlIgnore, JsonIgnore]
    public string DoseNotificationMessage => GetParameterValue(ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_MESSAGE, null, true);

    [XmlIgnore, JsonIgnore]
    public string DoseNotificationOperator => GetParameterValue(ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_OPERATOR, null, true);

    [XmlIgnore, JsonIgnore]
    public string DoseAlertId => GetParameterValue(ProtocolParameterNames.SCAN_DOSE_ALERT_ID, null, true);

    [XmlIgnore, JsonIgnore]
    public string DoseAlertMessage => GetParameterValue(ProtocolParameterNames.SCAN_DOSE_ALERT_MESSAGE, null, true);

    [XmlIgnore, JsonIgnore]
    public string DoseAlertOperator => GetParameterValue(ProtocolParameterNames.SCAN_DOSE_ALERT_OPERATOR, null, true);

    [XmlIgnore, JsonIgnore]
    public string DoseNotificationReason => GetParameterValue(ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_REASON, null, true);

    [XmlIgnore, JsonIgnore]
    public string DoseAlertReason => GetParameterValue(ProtocolParameterNames.SCAN_DOSE_ALERT_REASON, null, true);

    [XmlIgnore, JsonIgnore]
    public bool IsEnhanced => GetParameterValue<bool>(ProtocolParameterNames.IS_ENHANCED, true);

    [XmlIgnore, JsonIgnore]
    public bool IsIntervention => GetParameterValue<bool>(ProtocolParameterNames.IS_INTERVENTION, true);

    [XmlIgnore, JsonIgnore]
    public ScanImageType ScanImageType => (!(ScanOption == ScanOption.Surview || ScanOption == ScanOption.DualScout)) ? ScanImageType.Tomo : ScanImageType.Topo;

    [XmlIgnore, JsonIgnore]
    public PatientPosition? PatientPosition => Parent?.Parent?.PatientPosition;

    [XmlIgnore, JsonIgnore]
    public DateTime ExposureStartTime => GetParameterValue<DateTime>(ProtocolParameterNames.SCAN_START_EXPOSURE_TIME);

    [XmlIgnore, JsonIgnore]
    public uint AllowErrorTubeCount => GetParameterValue<uint>(ProtocolParameterNames.SCAN_ALLOW_ERROR_TUBE_COUNT, true);

    [XmlIgnore, JsonIgnore]
    public float CTDIvol => GetParameterValue<float>(ProtocolParameterNames.DOSE_INFO_CTDI, true);

    [XmlIgnore, JsonIgnore]
    public float mAs => GetParameterValue<float>(ProtocolParameterNames.SCAN_MA_S, true);

    [XmlIgnore, JsonIgnore]
    public uint ExposureIntervalTime => GetParameterValue<uint>(ProtocolParameterNames.SCAN_EXPOSURE_INTERVAL_TIME, true);
}
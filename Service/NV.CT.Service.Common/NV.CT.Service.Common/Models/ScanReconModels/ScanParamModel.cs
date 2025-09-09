using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.Collimator;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Enums;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.Common.Managers;
using NV.CT.Service.Common.MapUI;
using NV.CT.Service.Common.MapUI.Templates;
using NV.CT.Service.Common.Resources;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.Common.Models.ScanReconModels
{
    /// <inheritdoc cref="ScanParam"/>
    public class ScanParamModel : ViewModelBase
    {
        #region Field

        private double _scanLength;
        private uint _effectiveFrames;
        private int _displayFrames;
        private int _numOfScan;
        private ObjectFovMode _objectFov;
        private double _postDeleteLength;
        private string _scanUID;
        private EnableType _autoScan;
        private uint? _scanNumber;
        private ObservableCollection<uint> _voltage;
        private ObservableCollection<double> _current;
        private double _exposureTime;
        private double _frameTime;
        private uint _framesPerCycle;
        private ScanOption _scanOption;
        private ScanMode _scanMode;
        private RawDataType _rawDataType;
        private string _rawDataDirectory;
        private ExposureMode _exposureMode;
        private ExposureTriggerMode _exposureTriggerMode;
        private TableDirection _tableDirection;
        private CollimatorOpenMode _collimatorOpenMode;
        private uint _collimatorZ;
        private EnableType _collimatorOffsetEnable;
        private double _largeAngleDeleteLength;
        private double _smallAngleDeleteLength;
        private EnableType _bowtieEnable;
        private double _reconVolumeStartPosition;
        private double _reconVolumeEndPosition;
        private double _tableStartPosition;
        private double _tableEndPosition;
        private double _exposureStartPosition;
        private double _exposureEndPosition;
        private Gain _gain;
        private double _tableHeight;
        private BodyPart _bodyPart;
        private BodyCategory _bodyCategory;
        private ObservableCollection<TubePosition> _tubePositions;
        private ObservableCollection<int> _tubeNumbers;
        private ObservableCollection<byte> _doseCurve;
        private ScanBinning _scanBinning;
        private double _exposureDelayTime;
        private double _tableSpeed;
        private double _tableFeed;
        private double _tableAcceleration;
        private double _tableAccelerationTime;
        private FocalType _focal;
        private uint _preVoiceID;
        private uint _postVoiceID;
        private double _preVoicePlayTime;
        private double _preVoiceDelayTime;
        private double _pitch;
        private uint _preOffsetFrames;
        private uint _postOffsetFrames;
        private uint _loops;
        private double _loopTime;
        private EnableType _warmUp;
        private uint _warmUpTubeNumber;
        private double _gantryStartPosition;
        private double _gantryEndPosition;
        private GantryDirection _gantryDirection;
        private double _gantryAcceleration;
        private double _gantryAccelerationTime;
        private double _gantrySpeed;
        private uint _autoDeleteNum;
        private uint _totalFrames;
        private double _scanFOV;
        private double _rDelay;
        private double _tDelay;
        private double _spotDelay;
        private uint _allowErrorXRaySourceCount;
        private float _mAs;
        private float _ctdIvol;
        private FunctionMode _functionMode;
        private string _contrastBolusAgent;
        private float? _contrastBolusVolume;
        private float? _contrastFlowRate;
        private float? _contrastFlowDuration;
        private float? _contrastBolusIngredientConcentration;

        #endregion

        public ScanParamModel()
        {
            _postDeleteLength = 0;
            _scanUID = string.Empty;
            _autoScan = EnableType.Disable;
            _rawDataDirectory = string.Empty;
            _bowtieEnable = EnableType.Disable;
            _warmUp = EnableType.Disable;
            _voltage = [0, 0, 0, 0, 0, 0, 0, 0];
            _current = [0, 0, 0, 0, 0, 0, 0, 0];
            _tableDirection = TableDirection.In;
            _bodyCategory = BodyCategory.Adult;
            _tubePositions = [default, default];
            _collimatorOpenMode = CollimatorOpenMode.NearSmallAngle;
            _collimatorOffsetEnable = EnableType.Enable;
            _tubeNumbers = [default, default];
            _doseCurve = [];
            _rDelay = 0.3;
            _tDelay = 0.3;
            _scanFOV = 506.88;
            _contrastBolusAgent = string.Empty;
            _voltage.CollectionChanged += Voltage_CollectionChanged;
            _current.CollectionChanged += Current_CollectionChanged;
        }

        #region Property

        #region 前端使用

        /// <summary>
        /// 是否正在内部计算中，为了防止循环触发 <see cref="INotifyPropertyChanged.PropertyChanged"/> 事件
        /// </summary>
        public bool IsCalculating { get; set; }

        /// <summary>
        /// 此参数代表 <see cref="Voltage"/> 数组的第一个值
        /// </summary>
        [JsonIgnore]
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_Voltage), typeof(SimpleMapUITemplate<,>))]
        public uint VoltageSingle
        {
            get
            {
                if (Voltage.Count == 0)
                {
                    Voltage.Add(0);
                    return 0;
                }

                return Voltage[0];
            }
            set
            {
                if (Voltage.Count == 0)
                {
                    Voltage.Add(value);
                    RaisePropertyChanged();
                }
                else if (!EqualityComparer<uint>.Default.Equals(Voltage[0], value))
                {
                    Voltage[0] = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 此参数代表 <see cref="Current"/> 数组的第一个值
        /// </summary>
        [JsonIgnore]
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_Current), typeof(SimpleMapUITemplate<,>))]
        public double CurrentSingle
        {
            get
            {
                if (Current.Count == 0)
                {
                    Current.Add(0);
                    return 0;
                }

                return Current[0];
            }
            set
            {
                if (Current.Count == 0)
                {
                    Current.Add(value);
                    RaisePropertyChanged();
                }
                else if (!EqualityComparer<double>.Default.Equals(Current[0], value))
                {
                    Current[0] = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 用户关注的扫描长度(重建长度)，用于计算各种床位等参数，此值应该与 <see cref="EffectiveFrames"/> 进行联动
        /// <para>注意！此值仅是提供给用户使用的参数</para>
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </summary>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ScanLength), typeof(SimpleMapUITemplate<,>))]
        public double ScanLength
        {
            get => _scanLength;
            set => SetProperty(ref _scanLength, value);
        }

        /// <summary>
        /// 用户关注的总帧数，用于辅助计算 <see cref="ScanLength"/>，此值应该与 <see cref="ScanLength"/> 进行联动
        /// <para>注意！此值仅是提供给用户使用的参数。注意与 <see cref="TotalFrames"/> 的区分</para>
        /// </summary>
        /// <remarks>
        /// 一般情况下，算法部使用时会按照如下规则计算:
        /// <para>定位扫/双平片正侧同时曝光: 曝光次数 * 1或2</para>
        /// <para>轴扫: 每圈扫描数量 * 圈数</para>
        /// <para>螺旋扫: 重建长度 / (有效探测器宽度 * Pitch) * 每圈扫描数量</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_EffectiveFrames), typeof(SimpleMapUITemplate<,>))]
        public uint EffectiveFrames
        {
            get => _effectiveFrames;
            set => SetProperty(ref _effectiveFrames, value);
        }

        /// <summary>
        /// 通过其它参数计算得出的理论上的应展示在图像控件中的生数据图数量
        /// <para>注意！此值仅是提供给用户查看的参数。注意与 <see cref="TotalFrames"/> 的区分</para>
        /// </summary>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_DisplayFrames), typeof(SimpleMapUITemplate<,>))]
        public int DisplayFrames
        {
            get => _displayFrames;
            private set => SetProperty(ref _displayFrames, value);
        }

        /// <summary>
        /// 扫描圈数 (通过其他参数计算后得到)
        /// <para>注意！此值仅是提供给用户使用的参数</para>
        /// </summary>
        public int NumOfScan
        {
            get => _numOfScan;
            set => SetProperty(ref _numOfScan, value);
        }

        /// <summary>
        /// ObjectFov
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </summary>
        /// <remarks>仅供前端使用，参与TableControl相关的计算，并不向下传给MRS</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ObjectFov), typeof(EnumMapUITemplate<,>))]
        public ObjectFovMode ObjectFov
        {
            get => _objectFov;
            set => SetProperty(ref _objectFov, value);
        }

        /// <summary>
        /// PostDeleteLength
        /// </summary>
        /// <remarks>仅供前端使用，参与TableControl相关的计算，并不向下传给MRS</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_PostDeleteLength), typeof(SimpleMapUITemplate<,>))]
        public double PostDeleteLength
        {
            get => _postDeleteLength;
            set => SetProperty(ref _postDeleteLength, value);
        }

        #endregion

        /// <inheritdoc cref="ScanParam.ScanUID"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ScanUID), typeof(SimpleMapUITemplate<,>))]
        public string ScanUID
        {
            get => _scanUID;
            set => SetProperty(ref _scanUID, value);
        }

        /// <inheritdoc cref="ScanParam.AutoScan"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_AutoScan), typeof(EnumMapUITemplate<,>))]
        public EnableType AutoScan
        {
            get => _autoScan;
            set => SetProperty(ref _autoScan, value);
        }

        /// <inheritdoc cref="ScanParam.ScanNumber"/>
        public uint? ScanNumber
        {
            get => _scanNumber;
            set => SetProperty(ref _scanNumber, value);
        }

        /// <inheritdoc cref="ScanParam.kV"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_Voltage), typeof(SimpleCollectionMapUITemplate<,,>))]
        public ObservableCollection<uint> Voltage
        {
            get => _voltage;
            set
            {
                SetProperty(ref _voltage, value);
                RaisePropertyChanged(nameof(VoltageSingle));
            }
        }

        /// <inheritdoc cref="ScanParam.mA"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫安(mA)</para>
        /// <para>精度: 小数点后三位，即0.001mA(1μA)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_Current), typeof(SimpleCollectionMapUITemplate<,,>))]
        public ObservableCollection<double> Current
        {
            get => _current;
            set
            {
                SetProperty(ref _current, value);
                RaisePropertyChanged(nameof(CurrentSingle));
            }
        }

        /// <inheritdoc cref="ScanParam.ExposureTime"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ExposureTime), typeof(SimpleMapUITemplate<,>))]
        public double ExposureTime
        {
            get => _exposureTime;
            set => SetProperty(ref _exposureTime, value);
        }

        /// <inheritdoc cref="ScanParam.FrameTime"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_FrameTime), typeof(SimpleMapUITemplate<,>))]
        public double FrameTime
        {
            get => _frameTime;
            set => SetProperty(ref _frameTime, value);
        }

        /// <inheritdoc cref="ScanParam.FramesPerCycle"/>
        [MapUI(typeof(Common_Lang),
               nameof(Common_Lang.MapUI_Scan_FramesPerCycle),
               typeof(SimpleMapUITemplate<,>),
               typeof(ScanReconIsEnabledManager),
               nameof(ScanReconIsEnabledManager.WhenNotSurview),
               [nameof(ScanOption)])]
        public uint FramesPerCycle
        {
            get => _framesPerCycle;
            set => SetProperty(ref _framesPerCycle, value);
        }

        /// <inheritdoc cref="ScanParam.ScanOption"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ScanOption), typeof(EnumMapUITemplate<,>))]
        public ScanOption ScanOption
        {
            get => _scanOption;
            set => SetProperty(ref _scanOption, value);
        }

        /// <inheritdoc cref="ScanParam.ScanMode"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ScanMode), typeof(EnumMapUITemplate<,>))]
        public ScanMode ScanMode
        {
            get => _scanMode;
            set => SetProperty(ref _scanMode, value);
        }

        /// <inheritdoc cref="ScanParam.RawDataType"/>
        public RawDataType RawDataType
        {
            get => _rawDataType;
            set => SetProperty(ref _rawDataType, value);
        }

        /// <inheritdoc cref="ScanParam.RawDataDirectory"/>
        public string RawDataDirectory
        {
            get => _rawDataDirectory;
            set => SetProperty(ref _rawDataDirectory, value);
        }

        /// <inheritdoc cref="ScanParam.ExposureMode"/>
        [MapUI(typeof(Common_Lang),
               nameof(Common_Lang.MapUI_Scan_ExposureMode),
               typeof(EnumMapUITemplate<,>),
               typeof(ScanReconIsEnabledManager),
               nameof(ScanReconIsEnabledManager.WhenNotSurview),
               [nameof(ScanOption)])]
        public ExposureMode ExposureMode
        {
            get => _exposureMode;
            set => SetProperty(ref _exposureMode, value);
        }

        /// <inheritdoc cref="ScanParam.ExposureTriggerMode"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ExposureTriggerMode), typeof(EnumMapUITemplate<,>))]
        public ExposureTriggerMode ExposureTriggerMode
        {
            get => _exposureTriggerMode;
            set => SetProperty(ref _exposureTriggerMode, value);
        }

        /// <inheritdoc cref="ScanParam.TableDirection"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_TableDirection), typeof(EnumMapUITemplate<,>))]
        public TableDirection TableDirection
        {
            get => _tableDirection;
            set => SetProperty(ref _tableDirection, value);
        }

        /// <inheritdoc cref="ScanParam.CollimatorOpenMode"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_CollimatorOpenMode), typeof(EnumMapUITemplate<,>))]
        public CollimatorOpenMode CollimatorOpenMode
        {
            get => _collimatorOpenMode;
            set => SetProperty(ref _collimatorOpenMode, value);
        }

        /// <inheritdoc cref="ScanParam.CollimatorZ"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_CollimatorZ), typeof(SimpleMapUITemplate<,>))]
        public uint CollimatorZ
        {
            get => _collimatorZ;
            set => SetProperty(ref _collimatorZ, value);
        }

        /// <inheritdoc cref="ScanParam.CollimatorOffsetEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_CollimatorOffsetEnable), typeof(EnumMapUITemplate<,>))]
        public EnableType CollimatorOffsetEnable
        {
            get => _collimatorOffsetEnable;
            set => SetProperty(ref _collimatorOffsetEnable, value);
        }

        /// <inheritdoc cref="ScanParam.SmallAngleDeleteLength"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double SmallAngleDeleteLength
        {
            get => _smallAngleDeleteLength;
            set => SetProperty(ref _smallAngleDeleteLength, value);
        }

        /// <inheritdoc cref="ScanParam.LargeAngleDeleteLength"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double LargeAngleDeleteLength
        {
            get => _largeAngleDeleteLength;
            set => SetProperty(ref _largeAngleDeleteLength, value);
        }

        /// <inheritdoc cref="ScanParam.BowtieEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_Bowtie), typeof(EnumMapUITemplate<,>))]
        public EnableType BowtieEnable
        {
            get => _bowtieEnable;
            set => SetProperty(ref _bowtieEnable, value);
        }

        /// <inheritdoc cref="ScanParam.ReconVolumeStartPosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ReconVolumeStartPosition), typeof(SimpleMapUITemplate<,>))]
        public double ReconVolumeStartPosition
        {
            get => _reconVolumeStartPosition;
            set => SetProperty(ref _reconVolumeStartPosition, value);
        }

        /// <inheritdoc cref="ScanParam.ReconVolumeEndPosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ReconVolumeEndPosition), typeof(SimpleMapUITemplate<,>))]
        public double ReconVolumeEndPosition
        {
            get => _reconVolumeEndPosition;
            set => SetProperty(ref _reconVolumeEndPosition, value);
        }

        /// <inheritdoc cref="ScanParam.TableStartPosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double TableStartPosition
        {
            get => _tableStartPosition;
            set => SetProperty(ref _tableStartPosition, value);
        }

        /// <inheritdoc cref="ScanParam.TableEndPosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double TableEndPosition
        {
            get => _tableEndPosition;
            set => SetProperty(ref _tableEndPosition, value);
        }

        /// <inheritdoc cref="ScanParam.ExposureStartPosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double ExposureStartPosition
        {
            get => _exposureStartPosition;
            set => SetProperty(ref _exposureStartPosition, value);
        }

        /// <inheritdoc cref="ScanParam.ExposureEndPosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double ExposureEndPosition
        {
            get => _exposureEndPosition;
            set => SetProperty(ref _exposureEndPosition, value);
        }

        /// <inheritdoc cref="ScanParam.Gain"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_Gain), typeof(EnumMapUITemplate<,>))]
        public Gain Gain
        {
            get => _gain;
            set => SetProperty(ref _gain, value);
        }

        /// <inheritdoc cref="ScanParam.TableHeight"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_TableHeight), typeof(SimpleMapUITemplate<,>))]
        public double TableHeight
        {
            get => _tableHeight;
            set => SetProperty(ref _tableHeight, value);
        }

        /// <inheritdoc cref="ScanParam.BodyPart"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_BodyPart), typeof(EnumMapUITemplate<,>))]
        public BodyPart BodyPart
        {
            get => _bodyPart;
            set => SetProperty(ref _bodyPart, value);
        }

        /// <inheritdoc cref="ScanParam.BodyCategory"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_BodyCategory), typeof(EnumMapUITemplate<,>))]
        public BodyCategory BodyCategory
        {
            get => _bodyCategory;
            set => SetProperty(ref _bodyCategory, value);
        }

        /// <inheritdoc cref="ScanParam.TubePositions"/>
        [MapUI(typeof(Common_Lang),
               nameof(Common_Lang.MapUI_Scan_TubePositions),
               typeof(EnumCollectionMapUITemplate<,,>),
               typeof(ScanReconIsEnabledManager),
               nameof(ScanReconIsEnabledManager.WhenSurview),
               [nameof(ScanOption)])]
        public ObservableCollection<TubePosition> TubePositions
        {
            get => _tubePositions;
            set => SetProperty(ref _tubePositions, value);
        }

        /// <inheritdoc cref="ScanParam.TubeNumbers"/>
        public ObservableCollection<int> TubeNumbers
        {
            get => _tubeNumbers;
            set => SetProperty(ref _tubeNumbers, value);
        }

        /// <inheritdoc cref="ScanParam.DoseCurve"/>
        public ObservableCollection<byte> DoseCurve
        {
            get => _doseCurve;
            set => SetProperty(ref _doseCurve, value);
        }

        /// <inheritdoc cref="ScanParam.ScanBinning"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ScanBinning), typeof(EnumMapUITemplate<,>))]
        public ScanBinning ScanBinning
        {
            get => _scanBinning;
            set => SetProperty(ref _scanBinning, value);
        }

        /// <inheritdoc cref="ScanParam.ExposureDelayTime"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 秒(s)</para>
        /// <para>精度: 小数点后六位，即0.000001s(1μs)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ExposureDelayTime), typeof(SimpleMapUITemplate<,>))]
        public double ExposureDelayTime
        {
            get => _exposureDelayTime;
            set => SetProperty(ref _exposureDelayTime, value);
        }

        /// <inheritdoc cref="ScanParam.TableSpeed"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米/秒(mm/s)</para>
        /// <para>精度: 小数点后三位，即0.001mm/s(1μm/s)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_TableSpeed), typeof(SimpleMapUITemplate<,>))]
        public double TableSpeed
        {
            get => _tableSpeed;
            set => SetProperty(ref _tableSpeed, value);
        }

        /// <inheritdoc cref="ScanParam.TableFeed"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_TableFeed), typeof(SimpleMapUITemplate<,>))]
        public double TableFeed
        {
            get => _tableFeed;
            set => SetProperty(ref _tableFeed, value);
        }

        /// <inheritdoc cref="ScanParam.TableAcceleration"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米/平方秒(mm/s²)</para>
        /// <para>精度: 小数点后三位，即0.001mm/s²(1μm/s²)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_TableAcceleration), typeof(SimpleMapUITemplate<,>))]
        public double TableAcceleration
        {
            get => _tableAcceleration;
            set => SetProperty(ref _tableAcceleration, value);
        }

        /// <inheritdoc cref="ScanParam.TableAccelerationTime"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_TableAccelerationTime), typeof(SimpleMapUITemplate<,>))]
        public double TableAccelerationTime
        {
            get => _tableAccelerationTime;
            set => SetProperty(ref _tableAccelerationTime, value);
        }

        /// <inheritdoc cref="ScanParam.Focal"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_Focal), typeof(EnumMapUITemplate<,>))]
        public FocalType Focal
        {
            get => _focal;
            set => SetProperty(ref _focal, value);
        }

        /// <inheritdoc cref="ScanParam.PreVoiceID"/>
        public uint PreVoiceID
        {
            get => _preVoiceID;
            set => SetProperty(ref _preVoiceID, value);
        }

        /// <inheritdoc cref="ScanParam.PostVoiceID"/>
        public uint PostVoiceID
        {
            get => _postVoiceID;
            set => SetProperty(ref _postVoiceID, value);
        }

        /// <inheritdoc cref="ScanParam.PreVoicePlayTime"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        public double PreVoicePlayTime
        {
            get => _preVoicePlayTime;
            set => SetProperty(ref _preVoicePlayTime, value);
        }

        /// <inheritdoc cref="ScanParam.PreVoiceDelayTime"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        public double PreVoiceDelayTime
        {
            get => _preVoiceDelayTime;
            set => SetProperty(ref _preVoiceDelayTime, value);
        }

        /// <inheritdoc cref="ScanParam.Pitch"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>在此处设置实际螺距值，例如设置为0.33</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang),
               nameof(Common_Lang.MapUI_Scan_Pitch),
               typeof(SimpleMapUITemplate<,>),
               typeof(ScanReconIsEnabledManager),
               nameof(ScanReconIsEnabledManager.WhenHelical),
               [nameof(ScanOption)])]
        public double Pitch
        {
            get => _pitch;
            set => SetProperty(ref _pitch, value);
        }

        /// <inheritdoc cref="ScanParam.PreOffsetFrames"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_PreOffset), typeof(SimpleMapUITemplate<,>))]
        public uint PreOffsetFrames
        {
            get => _preOffsetFrames;
            set => SetProperty(ref _preOffsetFrames, value);
        }

        /// <inheritdoc cref="ScanParam.PostOffsetFrames"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_PostOffset), typeof(SimpleMapUITemplate<,>))]
        public uint PostOffsetFrames
        {
            get => _postOffsetFrames;
            set => SetProperty(ref _postOffsetFrames, value);
        }

        /// <inheritdoc cref="ScanParam.Loops"/>
        public uint Loops
        {
            get => _loops;
            set => SetProperty(ref _loops, value);
        }

        /// <inheritdoc cref="ScanParam.LoopTime"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        public double LoopTime
        {
            get => _loopTime;
            set => SetProperty(ref _loopTime, value);
        }

        /// <inheritdoc cref="ScanParam.WarmUp"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        public EnableType WarmUp
        {
            get => _warmUp;
            set => SetProperty(ref _warmUp, value);
        }

        /// <inheritdoc cref="ScanParam.WarmUpTubeNumber"/>
        public uint WarmUpTubeNumber
        {
            get => _warmUpTubeNumber;
            set => SetProperty(ref _warmUpTubeNumber, value);
        }

        /// <inheritdoc cref="ScanParam.GantryStartPosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 度(°)</para>
        /// <para>精度: 小数点后两位，即0.01°</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_GantryStart), typeof(SimpleMapUITemplate<,>))]
        public double GantryStartPosition
        {
            get => _gantryStartPosition;
            set => SetProperty(ref _gantryStartPosition, value);
        }

        /// <inheritdoc cref="ScanParam.GantryEndPosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 度(°)</para>
        /// <para>精度: 小数点后两位，即0.01°</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_GantryEnd), typeof(SimpleMapUITemplate<,>))]
        public double GantryEndPosition
        {
            get => _gantryEndPosition;
            set => SetProperty(ref _gantryEndPosition, value);
        }

        /// <inheritdoc cref="ScanParam.GantryDirection"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_GantryDirection), typeof(EnumMapUITemplate<,>))]
        public GantryDirection GantryDirection
        {
            get => _gantryDirection;
            set => SetProperty(ref _gantryDirection, value);
        }

        /// <inheritdoc cref="ScanParam.GantryAcceleration"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: °/s²</para>
        /// <para>精度: 小数点后两位，即0.01°/s²</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_GantryAcceleration), typeof(SimpleMapUITemplate<,>))]
        public double GantryAcceleration
        {
            get => _gantryAcceleration;
            set => SetProperty(ref _gantryAcceleration, value);
        }

        /// <inheritdoc cref="ScanParam.GantryAccelerationTime"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_GantryAccelerationTime), typeof(SimpleMapUITemplate<,>))]
        public double GantryAccelerationTime
        {
            get => _gantryAccelerationTime;
            set => SetProperty(ref _gantryAccelerationTime, value);
        }

        /// <inheritdoc cref="ScanParam.GantrySpeed"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: °/s</para>
        /// <para>精度: 小数点后两位，即0.01°/s</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_GantrySpeed), typeof(SimpleMapUITemplate<,>))]
        public double GantrySpeed
        {
            get => _gantrySpeed;
            set => SetProperty(ref _gantrySpeed, value);
        }

        /// <inheritdoc cref="ScanParam.AutoDeleteNum"/>
        [MapUI(typeof(Common_Lang),
               nameof(Common_Lang.MapUI_Scan_AutoDeleteNum),
               typeof(SimpleMapUITemplate<,>),
               typeof(ScanReconIsEnabledManager),
               nameof(ScanReconIsEnabledManager.WhenNotSurview),
               [nameof(ScanOption)])]
        public uint AutoDeleteNum
        {
            get => _autoDeleteNum;
            set => SetProperty(ref _autoDeleteNum, value);
        }

        /// <inheritdoc cref="ScanParam.TotalFrames"/>
        /// <remarks>
        /// 注意！此值表示下参给机器的参数。注意与 <see cref="EffectiveFrames"/> 的区分
        /// </remarks>
        public uint TotalFrames
        {
            get => _totalFrames;
            set => SetProperty(ref _totalFrames, value);
        }

        /// <inheritdoc cref="ScanParam.ScanFOV"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_ScanFOV), typeof(SimpleMapUITemplate<,>))]
        public double ScanFOV
        {
            get => _scanFOV;
            set => SetProperty(ref _scanFOV, value);
        }

        /// <inheritdoc cref="ScanParam.RDelay"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_RDelay), typeof(SimpleMapUITemplate<,>))]
        public double RDelay
        {
            get => _rDelay;
            set => SetProperty(ref _rDelay, value);
        }

        /// <inheritdoc cref="ScanParam.TDelay"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_TDelay), typeof(SimpleMapUITemplate<,>))]
        public double TDelay
        {
            get => _tDelay;
            set => SetProperty(ref _tDelay, value);
        }

        /// <inheritdoc cref="ScanParam.SpotDelay"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫秒(ms)</para>
        /// <para>精度: 小数点后三位，即0.001ms(1μs)</para>
        /// </remarks>
        public double SpotDelay
        {
            get => _spotDelay;
            set => SetProperty(ref _spotDelay, value);
        }

        /// <inheritdoc cref="ScanParam.AllowErrorXRaySourceCount"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_AllowErrorXRaySourceCount), typeof(SimpleMapUITemplate<,>))]
        public uint AllowErrorXRaySourceCount
        {
            get => _allowErrorXRaySourceCount;
            set => SetProperty(ref _allowErrorXRaySourceCount, value);
        }

        /// <inheritdoc cref="ScanParam.mAs"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_mAs), typeof(SimpleMapUITemplate<,>))]
        public float mAs
        {
            get => _mAs;
            set => SetProperty(ref _mAs, value);
        }

        /// <inheritdoc cref="ScanParam.CTDIvol"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_CTDIvol), typeof(SimpleMapUITemplate<,>))]
        public float CTDIvol
        {
            get => _ctdIvol;
            set => SetProperty(ref _ctdIvol, value);
        }

        /// <inheritdoc cref="ScanParam.FunctionMode"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Scan_FunctionMode), typeof(EnumMapUITemplate<,>))]
        public FunctionMode FunctionMode
        {
            get => _functionMode;
            set => SetProperty(ref _functionMode, value);
        }

        /// <inheritdoc cref="ScanParam.ContrastBolusAgent"/>
        public string ContrastBolusAgent
        {
            get => _contrastBolusAgent;
            set => SetProperty(ref _contrastBolusAgent, value);
        }

        /// <inheritdoc cref="ScanParam.ContrastBolusVolume"/>
        public float? ContrastBolusVolume
        {
            get => _contrastBolusVolume;
            set => SetProperty(ref _contrastBolusVolume, value);
        }

        /// <inheritdoc cref="ScanParam.ContrastFlowRate"/>
        public float? ContrastFlowRate
        {
            get => _contrastFlowRate;
            set => SetProperty(ref _contrastFlowRate, value);
        }

        /// <inheritdoc cref="ScanParam.ContrastFlowDuration"/>
        public float? ContrastFlowDuration
        {
            get => _contrastFlowDuration;
            set => SetProperty(ref _contrastFlowDuration, value);
        }

        /// <inheritdoc cref="ScanParam.ContrastBolusIngredientConcentration"/>
        public float? ContrastBolusIngredientConcentration
        {
            get => _contrastBolusIngredientConcentration;
            set => SetProperty(ref _contrastBolusIngredientConcentration, value);
        }

        #endregion

        private void Current_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChangedWhenCollectionChanged(e, nameof(CurrentSingle));
        }

        private void Voltage_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChangedWhenCollectionChanged(e, nameof(VoltageSingle));
        }

        private void RaisePropertyChangedWhenCollectionChanged(NotifyCollectionChangedEventArgs e, string propertyName)
        {
            if (e.Action != NotifyCollectionChangedAction.Replace)
            {
                return;
            }

            if (e is { OldItems: not null, NewItems: not null }
             && e.OldItems.Count == e.NewItems.Count
             && e.OldStartingIndex == e.NewStartingIndex
             && e.OldStartingIndex == 0)
            {
                RaisePropertyChanged(propertyName);
            }
        }

        public void UpdateDisplayFrames()
        {
            var exposureSourceCount = ExposureMode switch
            {
                ExposureMode.Single => 1u,
                ExposureMode.Dual => 2u,
                ExposureMode.Three => 3u,
                ExposureMode.Six => 6u,
                _ => 1u
            };
            DisplayFrames = ScanOption switch
            {
                ScanOption.None => 0,
                ScanOption.Axial => (int)((TotalFrames - AutoDeleteNum / exposureSourceCount) * NumOfScan),
                _ => (int)(TotalFrames - AutoDeleteNum / exposureSourceCount)
            };
        }

        public ScanParam Converter()
        {
            return Global.Instance.ServiceProvider.GetRequiredService<IMapper>().Map<ScanParam>(this);
        }
    }
}
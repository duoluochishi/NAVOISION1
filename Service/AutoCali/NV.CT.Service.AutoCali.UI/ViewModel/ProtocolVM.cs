using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.Collimator;
using NV.CT.FacadeProxy.Common.Enums.ReconEnums;
using NV.CT.FacadeProxy.Common.Helpers;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.AutoCali.UI;
using NV.CT.Service.AutoCali.Util;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Helper;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.UI.Util;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace NV.CT.Service.AutoCali.Logic
{
    internal class ProtocolVM : BindableBase
    {
        public static readonly LogWrapper _logger = new(ServiceCategory.AutoCali);

        public static readonly string CALI_ITEM_ARG_PROTOCOL_STUDY = "Default";

        /// <summary>
        /// 参数协议所属Study，多个相同Study的参数协议可以组合作为一个整体，完成某个任务
        /// 比如，Gain校准项目有3个参数协议GainH，GainL，GainLForce，依次使用每一个参数协议进行扫描采集，最后3个扫描结果作为一个整体给校准算法
        /// </summary>
        public string Study { get; set; } = CALI_ITEM_ARG_PROTOCOL_STUDY;

        public string UID { get; } = ScanUIDHelper.Generate18UID();

        public FunctionMode FunctionMode { get; set; }

        /// <summary>
        /// 扫描功能的多选项
        /// </summary>
        public virtual RawDataType[] RawDataTypeOptions { get; set; } = AllRawDataTypeOptions;

        public static RawDataType[] AllRawDataTypeOptions { get; private set; } = Enum.GetValues<RawDataType>();

        /// <summary>
        /// 扫描作用
        /// 比如，Gain校准项目有3个参数协议GainH，GainL，GainLForce，依次使用每一个参数协议进行扫描采集，最后3个扫描结果作为一个整体给校准算法
        /// </summary>
        public RawDataType RawDataType { get; set; }

        public Handler XHandler { get; set; }

        #region Command

        public EventHandler<Handler> SaveEventHandler;

        public ICommand ConfirmCommand { get; private set; }

        public void EnableEditMode()
        {
            InitCommand();
        }

        private void InitCommand()
        {
            ConfirmCommand = new DelegateCommand(
                () =>
                {
                    if (null != SaveEventHandler)
                    {
                        SetValueForDomain();
                        SaveEventHandler(this, XHandler);
                    }
                });
        }

        #endregion Command

        public virtual void SetValueFrom(Handler handler)
        {
            this.XHandler = handler;
            //add logic: set value from wrapped domain object
            if (handler == null || string.IsNullOrEmpty(handler.Study))
            {
                _logger.Error("Handler is null or Handler.Study is null");
                return;
            }
            this.Study = handler.Study;
        }

        /// <summary>
        /// 保存到Domain对象中
        /// </summary>
        public virtual void SetValueForDomain()
        {
            if (null == XHandler)
            {
                XHandler = new Handler();
                XHandler.ID = ScanUIDHelper.Generate18UID();
            }
            else if (string.IsNullOrEmpty(XHandler.ID))
            {
                XHandler.ID = ScanUIDHelper.Generate18UID();
            }

            XHandler.Parameters ??= new List<Parameter>();

            XHandler.Study = this.Study;
            XHandler.Parameters.Clear();
        }

        public virtual ScanReconParam PrepareScanParam()
        {
            return null;
        }

        protected Parameter MakeParameter(string name, string value)
        {
            return new Parameter() { Name = name, Value = value };
        }

        public static TEnum ParseEnum<TEnum>(string paramName, string paramStringValue, TEnum? defaultValue = null) where TEnum : struct, Enum
        {
            _logger.Debug($"Parsing the parameter '{paramName}'. [Input] {paramName}:{paramStringValue}, to Type:{typeof(TEnum)}");

            // 尝试解析字符串为枚举值
            if (Enum.TryParse<TEnum>(paramStringValue, ignoreCase: true, out TEnum result))
            {
                // 判断枚举是否包含该值
                if (Enum.IsDefined(typeof(TEnum), result))
                {
                    _logger.Debug($"Successfully parsed the parameter '{paramName}'. [Output] '{result}'");
                    return result;
                }
            }

            //内联函数：获取枚举的默认值，1）从传参中读取，如果有的话；2）枚举类型的第1个值；
            var GetEnumDefaultValue = () =>
            {
                TEnum output;
                if (null != defaultValue)
                {
                    // 从传参中读取，作为使用默认值
                    output = defaultValue.Value;
                }
                else
                {
                    // 使用枚举类型的第1个值作为默认值
                    output = GetEnumDefaultValue<TEnum>();
                }

                _logger.Warn($"Failed to parse the parameter '{paramName}', then use the default. [Output] '{output}'");

                return output;
            };

            return GetEnumDefaultValue();
        }

        public static TEnum GetEnumDefaultValue<TEnum>() where TEnum : Enum
        {
            // 获取枚举的第一个值作为默认值
            Array enumValues = Enum.GetValues(typeof(TEnum));
            if (enumValues.Length > 0)
            {
                return (TEnum)enumValues.GetValue(0);
            }
            else
            {
                throw new InvalidOperationException($"Enum {typeof(TEnum).Name} no defined any value.");
            }
        }
    }

    /// <summary>
    /// 通用校准参数
    /// </summary>
    internal class GeneralArgProtocolViewModel : ProtocolVM
    {
        public uint KV { get; set; } = 120;
        public uint MA { get; set; } = 100;

        public bool BowtieEnable { get; set; } = false;

        /// <summary>
        /// 当BowtieSwitch为true时，才下发BowtieEnable指令
        /// </summary>
        public bool BowtieSwitch { get; set; } = true;

        public Gain GainMode { get; set; } = Gain.Dynamic;

        /// <summary>
        /// 扫描模式，校准的扫描类型默认为轴扫Axial，除水值校准使用螺旋Helical
        /// </summary>
        public ScanOption ScanOption { get; set; } = ScanOption.Axial;

        public ScanMode ScanMode { get; set; } = ScanMode.Plain;

        public ExposureMode ExposureMode { get; set; } = ExposureMode.Single;
        /// <summary>
        /// 十二源同时院曝光模式下，起始曝光源编号，比如，17，那么同时曝光17，18，19，  20，21，22，  23，24，1，  2，3，4
        /// </summary>
        public uint TwelveExposureModeXRaySourceStartIndex { get; set; } = 1;

        /// <summary>
        /// 毫秒
        /// </summary>
        public float ExposureTime { get; set; } = 5.0f;

        /// <summary>
        /// 名称：曝光前的延迟时间
        /// 单位：秒(s)
        /// 范围：0-600秒。
        /// </summary>
        public uint ExposureDelayTime { get; set; } = 5;//s

        /// <summary>
        /// 探测器 收到ifBox的nvSync的Triger信号后，探测器自身进行Triger与Spot信号后沿的延时。
        /// 单位：毫秒，默认值：0.3
        /// </summary>
        public float RDelay { get; set; } = 0.3f;

        /// <summary>
        /// ifBox 发出的nvSync的Triger信号与其Spot 信号前沿的延时。
        /// 单位：毫秒，默认值：0.5
        /// </summary>
        public float TDelay { get; set; } = 0.3f;

        /// <summary>
        /// tubeintf收到ifBox的nvSync的Spot信号后自身进行Spot的延时。
        /// 单位：毫秒，默认值：0
        /// </summary>
        public float SpotDelay { get; set; } = 0;

        /// <summary>
        /// [弃用]这个值下游直接复用RDelay，单独设置不再有效
        /// 限束器的积分Delay是ifbox发出限束器的Spot信号进行的延时。
        /// 单位：毫秒，默认值：0.4
        /// </summary>
        public float CollimatorSpotDelay { get; set; } = 0.4f;

        /// <summary>
        /// 设定参与曝光的个数，默认全部
        /// XRaySourceIndex.All (0)代表所有射线源参与曝光，
        /// XRaySourceInd01 - XRaySourceIndex24 (1 - 24)代表对应编号的射线源参与曝光
        /// </summary>
        public XRaySourceIndex XRaySourceIndex { get; set; } = XRaySourceIndex.All;

        /// <summary>
        /// 毫秒
        /// </summary>
        public float FrameTime { get; set; } = 10.0f;

        /// <summary>
        /// 总张数
        /// </summary>
        public uint TotalFrames { get; set; } = 360;

        /// <summary>
        /// 名称：单圈投影图数量。
        ///范围：360~1080，为保护机架，每圈最低360张，限制转速
        /// </summary>
        public uint FramesPerCycle { get; set; } = 360;

        /// <summary>
        /// 限束器开口排数，0/64/128/242/288
        /// </summary>
        public uint Collimator { get; set; } = 288;

        /// <summary>
        /// 限束器控制开关，1：控制运动，0：不控制运动（底层不下发限束器参数）
        /// </summary>
        public uint CollimatorSwitch { get; set; } = 1;

        public ScanBinning BinningMode { get; set; } = ScanBinning.Bin11;

        /// <summary>
        /// 床的起始位置（毫秒）
        /// </summary>
        public int? ScanPositionStart { get; set; } = -100;//默认回到原位，-1000 * 10;

        public uint? ScanLength { get; set; }

        //
        // 摘要:
        //     名称：床步进范围，含义待定 单位：0.1毫米 范围：待定
        public uint TableFeed { get; set; } = 42 * 10;

        /// <summary>
        /// us
        /// </summary>
        public uint TableAccelerationTime { get; set; } = 300 * 1000;

        //  <Parameter Name="ExposureMode" Value="Twelve" />
        //<!--开始曝光时源编号-->
        //<Parameter Name="TwelveExposureModeXRaySourceStartIndex" Value="17" />
        //<!--开始曝光时机架的开始角度-->
        //<Parameter Name="GantryStartPosition" Value="75" />
        //<Parameter Name="GantryLength" Value="70" />
        /// <summary>
        /// 机架开始转动的角度，单位：度（底层嵌入式单位0.01度，需要转换）
        /// </summary>        
        public uint GantryStartPosition { get; set; } = 70;//度

        /// <summary>
        /// 机架开始转动幅度，单位：度（底层嵌入式单位0.01度，需要转换）
        /// </summary>
        public uint GantryLengthPerCycle { get; set; } = 15;//度

        /// <summary>
        /// 曝光采图前，探测器预先采集一定数量的暗场图，作为本次曝光采图的本底。
        /// 亮场图数据将自动减去本底。
        /// 默认值100张
        /// </summary>
        public uint PreOffsetFrames { get; set; } = 100;

        /// <summary>
        /// 按设计，由上游嵌入式实现自动采集PreOffset，并将亮场采集的图减去本底（自动采集的PreOffset-DarkH和DarkL的各自均值文件）。
        /// 然而，上游嵌入式迟迟未能提供正确的PreOffset功能，暂由软件额外采集PreOffset，具体张数有参数值决定。
        /// 0：不由软件采集PreOffset；
        /// </summary>
        public uint PreOffsetFramesBySoftware { get; set; } = 0;

        /// <summary>
        /// 默认32（ctbox内部有效范围0-32），余晖校准/校正要求PostOffset的帧时间大于150mm，避免在帧时间5ms时，*20张=100ms 小于150ms 不符合要求
        /// </summary>
        public uint PostOffsetFrames { get; set; } = 32;

        /// <summary>
        /// 采图时，自动删除指定数量的帧图像；
        /// </summary>
        public uint AutoDeleteFrames { get; set; } = 0;

        /// <summary>
        /// 按设计，由上游采集程序实现自动删图。
        /// 然而，上游迟迟未能提供正确的自动删图功能，暂由软件代为实现，通过调用删图工具库，自动删图后另存。
        /// 同时，如果由软件实现的PreOffset（PreOffsetFramesBySoftware>0)，删图工具库先删图后减本底，成功后另存。
        /// </summary>
        public bool AutoDeleteBySoftware { get; set; } = false;

        /// <summary>
        /// 射线源小焦点 或者 大焦点
        /// 默认 小焦点
        /// </summary>
        public FocalType Focal { get; set; }

        /// <summary>
        /// 扫描前是否移动机架，回到初始位置
        /// </summary>
        public bool MoveGantryToInit { get; set; } = false;

        /// <summary>
        /// 追加 激光灯与探测器的固定距离后，并且请求移床，由用户按移动键完成移床。
        /// 减少用户遗忘或者手动计算错误造成采图位置不正确扫描前是否移动机架，回到初始位置
        /// </summary>
        public bool MoveTableAfterAddFixDistance { get; set; } = false;

        #region 重建参数
        /// <summary>
        //层间距，单位MM
        /// 范围：0.16,0.32,0.50,0.65,1.0,1.3,1.5,2.0,2.5,3.0,4.0,5.0,6.0,8.0,10.0;
        /// </summary>
        public float SliceThickness { get; set; } = 3.0f;//0;

        /// <summary>
        /// SharpPlus = 0,// R_L
        /// Sharp = 1,//S_L
        /// Std = 2,//Cosine
        /// Balance = 3,//Hanning
        /// Smooth = 4,//Blackman
        /// SmoothPlus = 5,//Parzen
        /// None = 6
        /// /// </summary>
        public FilterType FilterType { get; set; } = FilterType.SmoothPlus;
        //public uint TvDenoiseCoef = 20;// demoSeries.ReconParam.CurrentTvDenoiseCoef;
        public uint ImageMatrixHor = 768;
        public uint ImageMatrixVert = 768;

        /// <summary>
        /// 是否启用骨硬化相关重建
        /// </summary>
        public bool BoneHarden { get; set; }

        public float FoVLengthHor { get; set; } = 253.44f;
        public float FoVLengthVert { get; set; } = 253.44f;

        public float ImageIncrement = 0.33f;//from LaiYongJin,0.165;
        public uint WindowCenter = 0;//35;
        public uint WindowWidth = 100;

        /// <summary>
        /// 二次重建Enable
        /// </summary>
        public bool TwoPassEnable { get; set; }

        /// <summary>
        /// 前降噪类型
        /// </summary>
        public PreDenoiseType PreDenoiseType { get; set; }

        /// <summary>
        /// 后降噪类型
        /// </summary>
        public PostDenoiseType PostDenoiseType { get; set; }

        /// <summary>
        /// 前降噪系数
        /// </summary>
        public int PreDenoiseCoef { get; set; }

        /// <summary>
        /// 后降噪系数
        /// </summary>
        public int PostDenoiseCoef { get; set; }

        public ReconType ReconType { get; set; } = ReconType.IVR_TV;//[ToDo]后续支持可配置使用新Tv，或者旧Tv

        /// <summary>
        /// 默认0.2
        /// </summary>
        public float IVRTVCoef { get; set; } = 0.2f;

        //reconParameter.ReconType = ReconType.IVR_TV;//[ToDo]后续支持可配置使用新Tv，或者旧Tv
        //reconParameter.IVRTVCoef = (reconParameter.ReconType == ReconType.IVR_TV) ? 0.2f : 0.02f;//新Tv，对应系数0.2；旧Tv，对应系数0.02（默认值）

        /// <summary>
        /// 是否单独的离线重建
        /// </summary>
        public bool OfflineReconProxyEnable { get; set; }

        #endregion 重建参数

        #region 机架控制参数

        /// <summary>
        /// 每次扫描一个Cycle后间隔运动机架，单位：度，比如，0.5度
        /// </summary>
        public double MoveGantryPositionByInterval { get; set; } = 0.5;

        /// <summary>
        /// 总共运动机架的角度，单位：度，比如，360度，对应就是720个Cycle（360/0.5）
        /// </summary>
        public double MoveGantryPositionByTotal { get; set; } = 360;

        #endregion 机架控制参数

        #region 多选框的可选项

        public static IList<ExposureMode> ExposureModes
        {
            get => GeneralUtil.GetEnumList<ExposureMode>();
        }

        public static IList<ScanBinning> Binnings
        {
            get => GeneralUtil.GetEnumList<ScanBinning>();
        }

        public static IList<Gain> Gains
        {
            get => GeneralUtil.GetEnumList<Gain>();
        }

        public static IList<bool> BowtieOptions
        {
            get => new bool[] { false, true };
        }

        #endregion 多选框的可选项

        public override void SetValueFrom(Handler handler)
        {
            string msgHeader = "[SetValueFrom]";
            try
            {
                base.SetValueFrom(handler);
                if (null == handler)
                {
                    return;
                }

                uint tempUintValue;
                foreach (var parameter in handler.Parameters)
                {
                    string parameterName = parameter.Name?.ToLower();
                    string parameterValue = parameter.Value;
                    if (parameterName == "RawDataType".ToLower())
                    {
                        if (!string.IsNullOrEmpty(parameterValue))
                        {
                            this.RawDataType = GetRawDataTypeFrom(parameterValue);
                        }
                    }
                    else if (parameterName == "KV".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out tempUintValue))
                        {
                            this.KV = tempUintValue;
                        }
                    }
                    else if (parameterName == "MA".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out tempUintValue))
                        {
                            this.MA = tempUintValue;
                        }
                    }
                    else if (parameterName == "BowtieEnable".ToLower() || parameterName == "bowtie".ToLower())
                    {
                        this.BowtieEnable = (parameterValue == "1" || parameterValue == "true");
                    }
                    else if (parameterName == "BowtieSwitch".ToLower())
                    {
                        this.BowtieSwitch = (parameterValue == "1" || parameterValue == "true");
                    }
                    else if (parameterName == "GainMode".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out tempUintValue))
                        {
                            this.GainMode = (Gain)tempUintValue;
                        }
                        else if (Enum.TryParse<Gain>(parameterValue, true, out var value))
                        {
                            this.GainMode = value;
                        }
                        else
                        {
                            this.GainMode = Gain.Dynamic;
                            _logger.Warn($"Failed to convert param '{{GainMode:\"{parameterValue}\"}}', use default '{this.GainMode}'");
                        }
                    }
                    else if (parameterName == "ExposureMode".ToLower())
                    {
                        this.ExposureMode = ParseEnum<ExposureMode>(nameof(ExposureMode), parameterValue);
                    }
                    else if (parameterName == "ScanOption".ToLower())
                    {
                        if (Enum.TryParse<ScanOption>(parameterValue, true, out var value))
                        {
                            this.ScanOption = value;
                        }
                        else
                        {
                            this.ScanOption = ScanOption.Axial;
                            _logger.Warn($"Failed to convert param '{{ScanOption:\"{parameterValue}\"}}', use default '{this.ScanOption}'");
                        }
                    }
                    else if (parameterName == "ScanMode".ToLower())
                    {
                        if (Enum.TryParse<ScanMode>(parameterValue, true, out var value))
                        {
                            this.ScanMode = value;
                        }
                        else
                        {
                            this.ScanMode = ScanMode.Plain;
                            _logger.Warn($"Failed to convert param '{{ScanMode:\"{parameterValue}\"}}', use default '{this.ScanMode}'");
                        }
                    }
                    else if (parameterName == "ExposureTime".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var value))
                        {
                            this.ExposureTime = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{ExposureTime:\"{parameterValue}\"}}' to float, use default '{this.ExposureTime}'");
                        }
                    }
                    else if (parameterName == nameof(XRaySourceIndex).ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var value))
                        {
                            this.XRaySourceIndex = (XRaySourceIndex)value;
                        }
                        else if (Enum.TryParse<XRaySourceIndex>(parameterValue, true, out XRaySourceIndex type))
                        {
                            this.XRaySourceIndex = type;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{XRaySourceIndex:\"{parameterValue}\"}}' to uint, use default '{this.XRaySourceIndex}'");
                        }
                    }
                    else if (parameterName == "FrameTime".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var value))
                        {
                            this.FrameTime = value;
                        }
                    }
                    else if (parameterName == "TotalFrames".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out tempUintValue))
                        {
                            this.TotalFrames = tempUintValue;
                        }
                    }
                    else if (parameterName == "FramesPerCycle".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out tempUintValue))
                        {
                            this.FramesPerCycle = tempUintValue;
                        }
                    }
                    else if (parameterName == "Collimator".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out tempUintValue))
                        {
                            this.Collimator = tempUintValue;
                        }
                    }
                    else if (parameterName == "CollimatorSwitch".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var value))
                        {
                            this.CollimatorSwitch = value;
                        }
                    }
                    else if (parameterName == "BinningMode".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out tempUintValue))
                        {
                            this.BinningMode = (ScanBinning)tempUintValue;
                        }
                    }
                    else if (parameterName == "ScanPositionStart".ToLower())
                    {
                        SetScanPositionStart(parameterValue);
                    }
                    else if (parameterName == "ScanLength".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var tempValue))
                        {
                            this.ScanLength = tempValue;
                        }
                    }
                    else if (parameterName == "TableFeed".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var tempValue))
                        {
                            this.TableFeed = tempValue;
                        }
                    }
                    else if (parameterName == "PreOffsetFrames".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var tempValue))
                        {
                            this.PreOffsetFrames = tempValue;
                        }
                    }
                    else if (string.Equals(parameterName, nameof(GantryStartPosition), StringComparison.OrdinalIgnoreCase))
                    {
                        string propertyName = nameof(GantryStartPosition);
                        if (uint.TryParse(parameterValue, out var tempValue))
                        {
                            this.GantryStartPosition = tempValue;
                            _logger.Debug($"Converted {propertyName} from '{parameterValue}' to '{this.GantryStartPosition}'");
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert {propertyName} from '{parameterValue}', use default '{this.GantryStartPosition}'");
                        }
                    }

                    else if (string.Equals(parameterName, nameof(GantryLengthPerCycle), StringComparison.OrdinalIgnoreCase))
                    {
                        string propertyName = nameof(GantryLengthPerCycle);
                        if (uint.TryParse(parameterValue, out var tempValue))
                        {
                            this.GantryLengthPerCycle = tempValue;
                            _logger.Debug($"Converted {propertyName} from '{parameterValue}' to '{this.GantryLengthPerCycle}'");
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert {propertyName} from '{parameterValue}', use default '{this.GantryLengthPerCycle}'");
                        }
                    }
                    else if (string.Equals(parameterName, nameof(TwelveExposureModeXRaySourceStartIndex), StringComparison.OrdinalIgnoreCase))
                    {
                        string propertyName = nameof(TwelveExposureModeXRaySourceStartIndex);
                        if (uint.TryParse(parameterValue, out var tempValue))
                        {
                            this.TwelveExposureModeXRaySourceStartIndex = tempValue;
                            _logger.Debug($"Converted {propertyName} from '{parameterValue}' to '{this.TwelveExposureModeXRaySourceStartIndex}'");
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert {propertyName} from '{parameterValue}', use default '{this.TwelveExposureModeXRaySourceStartIndex}'");
                        }
                    }

                    else if (parameterName == "ExpDelayTime".ToLower() || parameterName == "ExposureDelayTime".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var tempValue))
                        {
                            this.ExposureDelayTime = tempValue;
                        }
                    }
                    else if (parameterName == "RDelay".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var tempValue))
                        {
                            this.RDelay = tempValue;
                        }
                    }
                    else if (parameterName == "TDelay".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var tempValue))
                        {
                            this.TDelay = tempValue;
                        }
                    }
                    else if (parameterName == "SpotDelay".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var tempValue))
                        {
                            this.SpotDelay = tempValue;
                        }
                    }
                    else if (parameterName == "CollSpotDelay".ToLower() || parameterName == "CollimatorSpotDelay".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var tempValue))
                        {
                            this.CollimatorSpotDelay = tempValue;
                        }
                    }
                    else if (parameterName == "PostOffsetFrames".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var tempValue))
                        {
                            this.PostOffsetFrames = tempValue;
                        }
                    }

                    else if (parameterName == "TableAccelerationTime".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var tempValue))
                        {
                            this.TableAccelerationTime = tempValue;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{TableAccelerationTime:\"{parameterValue}\"}}', use default '{this.TableAccelerationTime}'");
                        }
                    }

                    else if (parameterName == "Focal".ToLower() || parameterName == "XRayFocusType".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var value))
                        {
                            this.Focal = (FocalType)value;
                        }
                        else if (Enum.TryParse<FocalType>(parameterValue, true, out var value2))
                        {
                            this.Focal = value2;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{FocalType:\"{parameterValue}\"}}', use default '{this.Focal}'");
                        }
                    }


                    else if (parameterName == "MoveGantryToInit".ToLower())
                    {
                        if (bool.TryParse(parameterValue, out var value))
                        {
                            this.MoveGantryToInit = value;
                        }
                    }

                    else if (parameterName == nameof(MoveGantryPositionByInterval).ToLower())
                    {
                        if (double.TryParse(parameterValue, out var value))
                        {
                            this.MoveGantryPositionByInterval = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert the param ({nameof(MoveGantryPositionByInterval)}:{parameterValue}), use the default ({this.MoveGantryPositionByInterval})");
                        }
                    }
                    else if (parameterName == nameof(MoveGantryPositionByTotal).ToLower())
                    {
                        if (double.TryParse(parameterValue, out var value))
                        {
                            this.MoveGantryPositionByTotal = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert the param ({nameof(MoveGantryPositionByTotal)}:{parameterValue}), use the default ({this.MoveGantryPositionByTotal})");
                        }
                    }

                    else if (parameterName == "MoveTableAfterAddFixDistance".ToLower())
                    {
                        if (bool.TryParse(parameterValue, out var value))
                        {
                            this.MoveTableAfterAddFixDistance = value;
                        }
                    }
                    else if (parameterName == "AutoDeleteFrames".ToLower() || parameterName == "AutoDeleteNum")
                    {
                        if (uint.TryParse(parameterValue, out tempUintValue))
                        {
                            this.AutoDeleteFrames = tempUintValue;
                        }
                    }
                    else if (parameterName == "PreOffsetFramesBySoftware".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out tempUintValue))
                        {
                            this.PreOffsetFramesBySoftware = tempUintValue;
                        }
                    }
                    else if (parameterName == "AutoDeleteBySoftware".ToLower())
                    {
                        if (bool.TryParse(parameterValue, out bool pasedAutoDeleteBySoftware))
                        {
                            this.AutoDeleteBySoftware = pasedAutoDeleteBySoftware;
                        }
                    }
                    else if (parameterName == "SliceThickness".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var value))
                        {
                            this.SliceThickness = value;
                        }
                    }
                    else if (parameterName == "FilterType".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var value))
                        {
                            this.FilterType = (FilterType)value;
                        }
                        else if (Enum.TryParse<FilterType>(parameterValue, true, out FilterType type))
                        {
                            this.FilterType = type;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{FilterType:\"{parameterValue}\"}}', use default '{this.FilterType}'");
                        }
                    }

                    else if (parameterName == "BoneHarden".ToLower())
                    {
                        if (bool.TryParse(parameterValue, out var value))
                        {
                            this.BoneHarden = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{BoneHarden:\"{parameterValue}\"}}', use default '{this.BoneHarden}'");
                        }
                    }

                    //else if (parameterName == "TvDenoiseCoef".ToLower())
                    //{
                    //    if (uint.TryParse(parameterValue, out var value))
                    //    {
                    //        this.TvDenoiseCoef = value;
                    //    }
                    //}
                    else if (parameterName == "ImageMatrixHor".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var value))
                        {
                            this.ImageMatrixHor = value;
                        }
                    }
                    else if (parameterName == "ImageMatrixVert".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var value))
                        {
                            this.ImageMatrixVert = value;
                        }
                    }
                    else if (parameterName == "FoVLengthHor".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var value))
                        {
                            this.FoVLengthHor = value;
                        }
                    }
                    else if (parameterName == "FoVLengthVert".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var value))
                        {
                            this.FoVLengthVert = value;
                        }
                    }

                    else if (parameterName == "ImageIncrement".ToLower())
                    {
                        if (float.TryParse(parameterValue, out var value))
                        {
                            this.ImageIncrement = value;
                        }
                    }
                    else if (parameterName == "WindowCenter".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var value))
                        {
                            this.WindowCenter = value;
                        }
                    }
                    else if (parameterName == "WindowWidth".ToLower())
                    {
                        if (uint.TryParse(parameterValue, out var value))
                        {
                            this.WindowWidth = value;
                        }
                    }

                    else if (parameterName == "TwoPassEnable".ToLower())
                    {
                        if (bool.TryParse(parameterValue, out var value))
                        {
                            this.TwoPassEnable = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{TwoPassEnable:\"{parameterValue}\"}}', use default '{this.TwoPassEnable}'");
                        }
                    }
                    else if (parameterName == "PreDenoiseType".ToLower())
                    {
                        if (Enum.TryParse<PreDenoiseType>(parameterValue, true, out var value))
                        {
                            this.PreDenoiseType = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{PreDenoiseType:\"{parameterValue}\"}}', use default '{this.PreDenoiseType}'");
                        }
                    }
                    else if (parameterName == "PreDenoiseCoef".ToLower())
                    {
                        if (int.TryParse(parameterValue, out var value))
                        {
                            this.PreDenoiseCoef = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{PreDenoiseCoef:\"{parameterValue}\"}}', use default '{this.PreDenoiseCoef}'");
                        }
                    }
                    else if (parameterName == "PostDenoiseType".ToLower())
                    {
                        if (Enum.TryParse<PostDenoiseType>(parameterValue, true, out var value))
                        {
                            this.PostDenoiseType = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{PostDenoiseType:\"{parameterValue}\"}}', use default '{this.PostDenoiseType}'");
                        }
                    }
                    else if (parameterName == "PostDenoiseCoef".ToLower())
                    {
                        if (int.TryParse(parameterValue, out var value))
                        {
                            this.PostDenoiseCoef = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{PostDenoiseCoef:\"{parameterValue}\"}}', use default '{this.PostDenoiseCoef}'");
                        }
                    }

                    else if (parameterName == nameof(IVRTVCoef).ToLower())
                    {
                        if (float.TryParse(parameterValue, out var parsedValue))
                        {
                            this.IVRTVCoef = parsedValue;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{IVRTVCoef:\"{parameterValue}\"}}', use default '{this.IVRTVCoef}'");
                        }
                    }
                    else if (parameterName == nameof(ReconType).ToLower())
                    {
                        this.ReconType = ParseEnum<ReconType>(nameof(ReconType), parameterValue, ReconType.IVR_TV);
                    }

                    else if (parameterName == "OfflineReconProxyEnable".ToLower())
                    {
                        if (bool.TryParse(parameterValue, out var value))
                        {
                            this.OfflineReconProxyEnable = value;
                        }
                        else
                        {
                            _logger.Warn($"Failed to convert param '{{OfflineReconProxyEnable:\"{parameterValue}\"}}', use default '{this.OfflineReconProxyEnable}'");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Warn($"{msgHeader} Failed to parse, due to {e.ToString()}");
            }
        }

        private void SetScanPositionStart(string strValue)
        {
            if (int.TryParse(strValue, out var tempValue))
            {
                this.ScanPositionStart = tempValue;
                _logger.Debug($"Parsed {strValue} successfully and set to ScanPositionStart={ScanPositionStart}");
            }
        }

        /// <summary>
        /// 保存到Domain对象中
        /// </summary>
        public override void SetValueForDomain()
        {
            base.SetValueForDomain();

            XHandler.Parameters.Add(MakeParameter("RawDataType", RawDataType.ToString()));

            XHandler.Parameters.Add(MakeParameter("KV", KV.ToString()));
            XHandler.Parameters.Add(MakeParameter("MA", MA.ToString()));

            XHandler.Parameters.Add(MakeParameter("BowtieEnable", (BowtieEnable ? 1 : 0).ToString()));

            XHandler.Parameters.Add(MakeParameter("GainMode", ((uint)GainMode).ToString()));
            XHandler.Parameters.Add(MakeParameter("ExposureMode", ((uint)ExposureMode).ToString()));
            XHandler.Parameters.Add(MakeParameter("ScanOption", ((uint)ScanOption).ToString()));

            XHandler.Parameters.Add(MakeParameter("ExposureTime", ExposureTime.ToString()));
            XHandler.Parameters.Add(MakeParameter("FrameTime", FrameTime.ToString()));

            XHandler.Parameters.Add(MakeParameter("TotalFrames", TotalFrames.ToString()));
            XHandler.Parameters.Add(MakeParameter("FramesPerCycle", FramesPerCycle.ToString()));
            XHandler.Parameters.Add(MakeParameter("Collimator", Collimator.ToString()));

            XHandler.Parameters.Add(MakeParameter("BinningMode", ((uint)BinningMode).ToString()));
        }

        /// <summary>
        /// 探测器宽度（单位：毫米mm）=472mm
        /// </summary>
        private static readonly int DetectorSize = 472;

        private static readonly uint PitchRatio = 100;//螺距系数0.5，转换比例100，结果为50

        private int CalculateScanLength(ScanParam scanParameter)
        {
            var totalFrames = this.TotalFrames;
            int userScanLength = DetectorSize;//默认值
            uint tableFeed = scanParameter.TableFeed;

            if (scanParameter.ScanOption == ScanOption.Axial)
            {
                int cycles = (int)Math.Ceiling(totalFrames * 1.0 / scanParameter.FramesPerCycle);

                int subLength = (int)tableFeed * (cycles - 1);
                userScanLength = subLength + DetectorSize;
                _logger.Info($"TotalFrames:{totalFrames}, FramesPerCycle:{scanParameter.FramesPerCycle}, calculated Cycles:{cycles}, ScanLength:{userScanLength}");
            }
            else if (scanParameter.ScanOption == ScanOption.Helical)
            {
                userScanLength = this.ScanLength.HasValue ? (int)(this.ScanLength * DistanceCoefficient) : 0;
                if (userScanLength < DetectorSize)
                {
                    float pitch = scanParameter.Pitch / PitchRatio;
                    float validDetectorWidthAfterCollimated = DetectorSize * pitch;
                    float cycles = totalFrames * 1.0f / scanParameter.FramesPerCycle;

                    int oldUserScanLength = userScanLength;
                    userScanLength = (int)Math.Ceiling(cycles * validDetectorWidthAfterCollimated);
                    _logger.Info($"Adopt the ScanLength [{oldUserScanLength}] -> [{userScanLength}] due to invalid.");
                }
            }

            return userScanLength;
        }

        /// <summary>
        /// mm to um, *1000
        /// </summary>
        private const int DistanceCoefficient = 1000;
        public override ScanReconParam PrepareScanParam()
        {
            #region Set ScanParameter

            var currentScanRecon = new ScanReconParam();
            ScanParam scanParameter = new ScanParam();
            scanParameter.FunctionMode = this.FunctionMode;

            scanParameter.AutoScan = true;
            scanParameter.kV[0] = KV;// 120;
            scanParameter.mA[0] = MA * 1000;//100 * 1000, mA -> uA;
            scanParameter.RawDataType = (RawDataType)RawDataType;
            scanParameter.ScanOption = this.ScanOption;// ScanOption.AXIAL;
            scanParameter.TableDirection = TableDirection.In;
            scanParameter.ScanMode = this.ScanMode;// ScanMode.Plain;

            scanParameter.CollimatorZ = this.Collimator;
            //开口模式，1:基于小锥角模式（256,242,128,64）；2：基于中心位置模式（288，128,64）
            scanParameter.CollimatorOpenMode = (288 == scanParameter.CollimatorZ)
                ? CollimatorOpenMode.NearCenter : CollimatorOpenMode.NearSmallAngle;// 2u : 1;

            scanParameter.Gain = this.GainMode;// Gain.Dynamic;
            scanParameter.ExposureMode = this.ExposureMode;
            scanParameter.ExposureTime = (uint)(this.ExposureTime * 1000);
            scanParameter.FrameTime = (uint)(this.FrameTime * 1000);

            scanParameter.ExposureDelayTime = this.ExposureDelayTime * 1000 * 1000;//配置中获取的数值（秒 s）-> DelayTime的单位是 微秒us

            //nvSync Delay
            scanParameter.RDelay = (uint)(this.RDelay * 1000);
            scanParameter.TDelay = (uint)(this.TDelay * 1000);
            scanParameter.SpotDelay = (uint)(this.SpotDelay * 1000);

            scanParameter.Pitch = 50;// 1 * 100;todo
            scanParameter.BowtieEnable = this.BowtieEnable;

            //根据RawDataType切换BodyPart
            scanParameter.BodyPart = this.RawDataType switch
            {
                RawDataType.Water20_Single => BodyPart.HEAD,
                RawDataType.Water20_Three => BodyPart.HEAD,
                _ => BodyPart.CHEST,
            };

            //scanParameter.ScanUID必须16位
            scanParameter.ScanUID = UidUtil.GenerateScanUid_16(scanParameter.RawDataType);

            //todo:将默认的扫描参数和重建参数，挪到配置文件中
            scanParameter.TableSpeed = 50 * 10;// 100;

            scanParameter.TableFeed = this.TableFeed;// 40 * 10;// 400;
            //[Todo]临时设置TableAccTime
            scanParameter.TableAccelerationTime = this.TableAccelerationTime;

            scanParameter.TableHeight = 7000;

            var cycle = (uint)Math.Ceiling(1.0 * this.TotalFrames / this.FramesPerCycle);
            var gantryLength = this.GantryLengthPerCycle * cycle * 100;
            scanParameter.GantryStartPosition = this.GantryStartPosition * 100;
            scanParameter.GantryEndPosition = scanParameter.GantryStartPosition + gantryLength;
            scanParameter.GantryAccelerationTime = 500 * 1000;//机架加速度需要设置，默认500ms
            scanParameter.ScanBinning = this.BinningMode;

            scanParameter.FramesPerCycle = this.FramesPerCycle;// 1080;
            scanParameter.TotalFrames = this.TotalFrames;
            //scanParameter.Cycles = (uint)Math.Ceiling(1.0 * this.TotalFrames / this.FramesPerCycle);// 扫描圈数Cycles虽然不直接给底层ctBox，但仍需设置，避免CTBox之前的校验失败

            if (this.AutoDeleteBySoftware)
            {
                scanParameter.AutoDeleteNum = 0;
            }
            else
            {
                scanParameter.AutoDeleteNum = this.AutoDeleteFrames;
            }

            scanParameter.Focal = this.Focal;

            //todo: 没有ScanLength了
            var scanLength = (uint)CalculateScanLength(scanParameter);// 起作用的是ScanLength和FramesPerCycle，由ctBox内部计算cycles

            scanParameter.ExposureStartPosition = this.ScanPositionStart.Value;// -1000 * 10; //1000 * 10;//500*10
            //scanParameter.ScanPositionEnd = -11272;// 11272;//550*10
            //scanParameter.ScanLength = 1272;// 50 * 10;

            //todo: 没有ScanLength了
            scanParameter.ExposureEndPosition = scanParameter.ExposureStartPosition - (int)scanLength;// 11272;//550*10
            scanParameter.ReconVolumeStartPosition = scanParameter.ExposureStartPosition;
            scanParameter.ReconVolumeEndPosition = scanParameter.ExposureEndPosition;

            scanParameter.PreOffsetFrames = this.PreOffsetFrames;// 15;//扫描前自动采集15张暗场图，由探测器内部做Offset校准，生成的RawData已经被实时的Offset校正了
            //if (this.FunctionMode == FunctionMode.Cali_AfterGlow)
            //if (scanParameter.kV[0] > 0)//Todo:remove,暂时为了规避Offset校准的DarkH没有PostOffset导致校准时调用GetAxialRawData崩溃
            {
                scanParameter.PostOffsetFrames = this.PostOffsetFrames;//20;//曝光扫描后自动采集20张暗场图，并输出对应的RawData，与本次亮场（曝光）数据对应
            }

            currentScanRecon.ScanParameter = scanParameter;

            #endregion Set ScanParameter

            #region Set ReconParamter

            ReconSeriesParam reconParameter = new ReconSeriesParam();

            //[Attension!!!]只能是“0-9和.”，不能其他，否则重建后保存Dicom时报错“校验字段不合法”
            reconParameter.SOPClassUID = UIDHelper.CreateSOPClassUID();
            //[Attension!!!]只能是“0-9和.”，不能其他，否则重建后保存Dicom时报错“校验字段不合法”
            reconParameter.SOPClassUIDHeader = IdGenerator.Next(7);

            //[Attension!!!]必须设置并且不重复，用于重建后在StudyInstanceUID目录下保存本次Dicom
            reconParameter.SeriesInstanceUID = UIDHelper.CreateSeriesInstanceUID();

            reconParameter.Modality = "CT";

            //reconParameter.ReconType = ReconType.HCT;
            reconParameter.ReconType = this.ReconType;// ReconType.IVR_TV;//[ToDo]后续支持可配置使用新Tv，或者旧Tv
            reconParameter.IVRTVCoef = this.IVRTVCoef;// (reconParameter.ReconType == ReconType.IVR_TV) ? 0.2f : 0.02f;//新Tv，对应系数0.2；旧Tv，对应系数0.02（默认值）
            //reconParameter.Binning = ReconBinning.Bin11;// demoSeries.ScanParam.CurrentBinningMode == DemoBinningMode.Bin2x2 ? 2 : 1;
            //reconParameter.Bm3dDenoiseCoef = 20;// demoSeries.ReconParam.CurrentBM3DDenoiseCoef;
            reconParameter.BoneAritifactEnable = this.BoneHarden;// ? 1 : 0;
            //reconParameter.CenterX = 256;// demoSeries.ReconParam.ReconCenterX;
            //reconParameter.CenterY = 256;// demoSeries.ReconParam.ReconCenterY;

            reconParameter.FilterType = this.FilterType;
            reconParameter.MetalAritifactEnable = false;// -1;// demoSeries.ReconParam.MetalAritifactEnable ? 1 : -1;
            //reconParameter.ReconFOV = 350;// demoSeries.ReconParam.CurrentReconFOV;
            reconParameter.SliceThickness = (this.SliceThickness * DistanceCoefficient);// 3 * 1000, mm ->um
            //reconParameter.TvDenoiseCoef = this.TvDenoiseCoef;// 20;// demoSeries.ReconParam.CurrentTvDenoiseCoef;

            //以定位片的左上点三维坐标为原点。
            reconParameter.CenterFirstX = 47 * DistanceCoefficient;// demoSeries.ReconParam.CurrentReconFOV / 2;
            reconParameter.CenterFirstY = 0 * DistanceCoefficient;
            reconParameter.CenterFirstZ = 0 * DistanceCoefficient;// demoSeries.ScanParam.ScanStartPosition;

            reconParameter.CenterLastX = 0 * DistanceCoefficient;// demoSeries.ReconParam.CurrentReconFOV / 2;
            reconParameter.CenterLastY = 0 * DistanceCoefficient;
            reconParameter.CenterLastZ = 0 * DistanceCoefficient;// demoSeries.ScanParam.ScanEndPosition;

            //这两个参数也要赋值 ImageMatrixHor与ImageMatrixVert值要相等
            reconParameter.ImageMatrixHor = (int)this.ImageMatrixHor;
            reconParameter.ImageMatrixVert = (int)this.ImageMatrixVert;

            //var imgWidth = 768; //from laiyongjin, 1024;//不小于1024
            //var imgHeight = 768; //from LaiYongJin,1024;//不小于1024
            reconParameter.ImageIncrement = (this.ImageIncrement * DistanceCoefficient);// 3 * 1000, mm ->um

            //FoVLengthHor与FoVLengthVert值要相等
            reconParameter.FoVLengthHor = (int)(this.FoVLengthHor * DistanceCoefficient);
            reconParameter.FoVLengthVert = (int)(this.FoVLengthVert * DistanceCoefficient);

            //PreBinning: 0.165的倍数 1*1,2*2
            var preBinning = (int)Math.Round(FoVLengthHor / ImageMatrixHor / 0.165);
            reconParameter.PreBinning = (preBinning <= 1) ? PreBinning.XY_1x1 : PreBinning.XY_2x2;//默认2*2
            _logger.Debug($"Dynamic calc the PreBinning '{reconParameter.PreBinning}', [Input] FoVLengthHor '{FoVLengthHor}', ImageMatrixHor '{ImageMatrixHor}'.");

            reconParameter.FoVDirectionHorX = 1;
            reconParameter.FoVDirectionHorY = 0;
            reconParameter.FoVDirectionHorZ = 0;

            reconParameter.FoVDirectionVertX = 0;
            reconParameter.FoVDirectionVertY = 0;
            reconParameter.FoVDirectionVertZ = -1;

            //reconParameter.VoxelNumX = 512;// demoSeries.ReconParam.CurrentImageMatrix;
            //reconParameter.VoxelNumY = 512;// demoSeries.ReconParam.CurrentImageMatrix;
            //reconParameter.VoxelNumZ = 144;// (int)demoSeries.ReconParam.ImageCount;
            //reconParameter.VoxelSizeX = reconParameter.ReconFOV / reconParameter.VoxelNumX;
            //reconParameter.VoxelSizeY = reconParameter.ReconFOV / reconParameter.VoxelNumY;
            //reconParameter.VoxelSizeZ = 0.332f;// demoSeries.ReconParam.CurrentThickness;//reconParameter.ReconFOV / reconParameter.VoxelNumZ;

            //reconParameter.Bowtie = 1;// demoSeries.ScanParam.BowtieEnable ? 1 : 0;
            reconParameter.WindowCenter = new int[1] {
               (int)this.WindowCenter// 35
            };
            reconParameter.WindowWidth = new int[1] {
                (int)this.WindowWidth// 80
            };

            reconParameter.TwoPassEnable = this.TwoPassEnable;
            //Denoise
            reconParameter.PreDenoiseType = this.PreDenoiseType;
            reconParameter.PreDenoiseCoef = this.PreDenoiseCoef;
            reconParameter.PostDenoiseType = this.PostDenoiseType;
            reconParameter.PostDenoiseCoef = this.PostDenoiseCoef;

            reconParameter.SmoothZEnable = true;

            currentScanRecon.ReconSeriesParams[0] = reconParameter;

            #endregion Set ReconParamter



            #region Set Study TODO....
            currentScanRecon.Study = new Study();
            currentScanRecon.Study.StudyID = "StudyID";
            currentScanRecon.Study.StudyInstanceUID = ScanUIDHelper.Generate16UID();

            #endregion Set Study TODO....

            return currentScanRecon;
        }

        #region Private

        #region 生数据类型相关

        private static Dictionary<string, RawDataType> rawDataTypeDic;

        private static void TryInit()
        {
            if (null == rawDataTypeDic)
            {
                rawDataTypeDic = new Dictionary<string, RawDataType>();

                foreach (var rawDataType in ProtocolVM.AllRawDataTypeOptions)
                {
                    string key = rawDataType.ToString();
                    if (!rawDataTypeDic.ContainsKey(key))
                    {
                        rawDataTypeDic.Add(key, rawDataType);
                    }
                }
            }
        }

        private static RawDataType GetRawDataTypeFrom(string rawDataTypeName)
        {
            if (null == rawDataTypeDic)
            {
                TryInit();
            }

            RawDataType rawDataType;
            if (!rawDataTypeDic.TryGetValue(rawDataTypeName ?? String.Empty, out rawDataType))
            {
                rawDataType = default(RawDataType);
                _logger.Error($"Failed to find the RawDataType by the key({rawDataTypeName}) due to not contained in the RawDataType Enums!!!");
            }

            return rawDataType;
        }

        #endregion 生数据类型相关

        #endregion Private
    }

    internal class CaliItemVM : BindableBase
    {
        public static readonly LogWrapper _logger = new(ServiceCategory.AutoCali);

        public CalibrationItem DomainObj { get; set; }

        public CaliItemVM(CalibrationItem domainObj)
        {
            DomainObj = domainObj;
            Init();
        }

        public FunctionMode FunctionMode { get; set; }
        public Type UIControlType { get; set; }

        public Type ArgProtocolVMType { get; set; }

        private bool StringCompareIgnoreCase(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        public void Init()
        {
            string calibrationName = DomainObj.Name;
            string calibrationType = DomainObj.CalibrationType;
            if (string.IsNullOrEmpty(calibrationType))
            {
                calibrationType = calibrationName;
                _logger.Warn($"Not Expected the CalibrationType is NULL, so use the CalibrationName '{calibrationName}' as default");
            }

            if (ProtocolVM_Factory.TryGetFunctionMode(calibrationType, out FunctionMode mode))
            {
                FunctionMode = mode;
                ArgProtocolVMType = typeof(GeneralArgProtocolViewModel);
                UIControlType = typeof(GeneralArgProtocolDataGrid);
            }
            else
            {
                _logger.Error($"Not find the FunctionMode by the CalibrationType '{calibrationType}' for the CalibrationName '{calibrationName}'");
                ArgProtocolVMType = typeof(ProtocolVM);
                UIControlType = typeof(ArgProtocolDataGrid);
            }
        }
    }

    internal class ProtocolVM_Factory
    {
        private static Dictionary<string, FunctionMode> CaliFuncModeDic = new Dictionary<string, FunctionMode>()
        {
            { "DetectorOffset", FunctionMode.Cali_DetectorOffset},
            { "DetectorOffset_Old", FunctionMode.Cali_DetectorOffset},
            { "Offset", FunctionMode.Cali_DynamicGain_Offset },
            { "Defect", FunctionMode.Cali_DynamicGain_Defect },

            { "BackgroundScatter", FunctionMode.Cali_DynamicGain_BackgroundScatter },

            { "Gain", FunctionMode.Cali_DynamicGain_Gain},
            { "Air", FunctionMode.Cali_DynamicGain_Air},
            { "AirBowtie", FunctionMode.Cali_DynamicGain_AirBowtie},

            { "CenterOffset", FunctionMode.Cali_CenterOffset_OffsetCorr},

            { "AfterGlow", FunctionMode.Cali_AfterGlow},
            { "Harden", FunctionMode.Cali_Harden},
            { "NonLinear", FunctionMode.Cali_NonLinear},

            { "WaterValue", FunctionMode.Cali_WaterValue},

            { "DLT", FunctionMode.Cali_DLT},
            { "SourceXZ", FunctionMode.Cali_SourceXZ },
            { "SourceXZ_Old", FunctionMode.Cali_SourceXZ_Old}
        };

        public static bool TryGetFunctionMode(string type, out FunctionMode funcMode)
        {
            return CaliFuncModeDic.TryGetValue(type, out funcMode);
        }

        public static ProtocolVM GetProtocolViewModel(CalibrationItem caliItem, Handler argProtocol = null)
        {
            var caliItemVM = new CaliItemVM(caliItem);

            ProtocolVM viewModel;
            viewModel = (ProtocolVM)System.Activator.CreateInstance(caliItemVM.ArgProtocolVMType);
            viewModel.FunctionMode = caliItemVM.FunctionMode;
            //viewModel.UIControlType = caliItemVM.UIControlType;
            viewModel.XHandler = argProtocol;
            viewModel.SetValueFrom(argProtocol);
            return viewModel;
        }
    }
}
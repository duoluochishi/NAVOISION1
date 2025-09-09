using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.ReconEnums;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using System.Xml.Serialization;

namespace NV.CT.Protocol.Models
{
    [Serializable]
    public class ReconModel : BaseModel<ScanModel>
    {
        [XmlArray("ROIs")]
        [XmlArrayItem("ROI")]
        public List<CycleROIModel> CycleROIs { get; set; }

        [XmlArray("PostProcesses")]
        [XmlArrayItem("PostProcess")]
        public List<PostProcessModel> PostProcesses { get; set; }

        [XmlIgnore, JsonIgnore]
        public CTS.Enums.BodyPart? BodyPart => GetParameterValue<CTS.Enums.BodyPart?>(ProtocolParameterNames.BODY_PART, true);

        [XmlIgnore, JsonIgnore]
        public ReconType ReconType => GetParameterValue<ReconType>(ProtocolParameterNames.RECON_RECON_TYPE);

        [XmlIgnore, JsonIgnore]
        public AirCorrectionMode AirCorrectionMode => GetParameterValue<AirCorrectionMode>(ProtocolParameterNames.RECON_AIR_CORRECTION_MODE);

        [XmlIgnore, JsonIgnore]
        public FilterType FilterType => GetParameterValue<FilterType>(ProtocolParameterNames.RECON_FILTER_TYPE);

        [XmlIgnore, JsonIgnore]
        public string FilterTypeDisplay => $"{FilterType.ToString().Replace("Plus", "+")}";

        [XmlIgnore, JsonIgnore]
        public bool BoneAritifactEnable => GetParameterValue<bool>(ProtocolParameterNames.RECON_BONE_ARITIFACT_ENABALE);

        [XmlIgnore, JsonIgnore]
        public InterpType InterpType => GetParameterValue<InterpType>(ProtocolParameterNames.RECON_INTERP_TYPE);

        [XmlIgnore, JsonIgnore]
        public PreBinning PreBinning => GetParameterValue<PreBinning>(ProtocolParameterNames.RECON_PRE_BINNING);

        [XmlIgnore, JsonIgnore]
        public bool MetalAritifactEnable => GetParameterValue<bool>(ProtocolParameterNames.RECON_METAL_ARITIFACT_ENABLE);

        [XmlIgnore, JsonIgnore]
        public bool WindmillArtifactEnable => GetParameterValue<bool>(ProtocolParameterNames.RECON_WINDMILL_ARTIFACT_ENABLE);

        [XmlIgnore, JsonIgnore]
        public bool ConeAngleArtifactEnable => GetParameterValue<bool>(ProtocolParameterNames.RECON_CONE_ANGLE_ARTIFACT_ENABLE);

        [XmlIgnore, JsonIgnore]
        public float SliceThickness => GetParameterValue<float>(ProtocolParameterNames.RECON_SLICE_THICKNESS);

        [XmlIgnore, JsonIgnore]
        public PreDenoiseType PreDenoiseType => GetParameterValue<PreDenoiseType>(ProtocolParameterNames.RECON_PRE_DENOISE_TYPE);

        [XmlIgnore, JsonIgnore]
        public int PreDenoiseCoef => GetParameterValue<int>(ProtocolParameterNames.RECON_PRE_DENOISE_COEF);

        [XmlIgnore, JsonIgnore]
        public PostDenoiseType PostDenoiseType => GetParameterValue<PostDenoiseType>(ProtocolParameterNames.RECON_POST_DENOISE_TYPE);

        [XmlIgnore, JsonIgnore]
        public int PostDenoiseCoef => GetParameterValue<int>(ProtocolParameterNames.RECON_POST_DENOISE_COEF);

        [XmlIgnore, JsonIgnore]
        public int RingCorrectionCoef => GetParameterValue<int>(ProtocolParameterNames.RECON_RING_CORRECTION_COEF);

        [XmlIgnore, JsonIgnore]
        public ScatterAlgorithm ScatterAlgorithm => GetParameterValue<ScatterAlgorithm>(ProtocolParameterNames.RECON_SCATTER_ALGORITHM);

        [XmlIgnore, JsonIgnore]
        public bool RingAritifactEnable => GetParameterValue<bool>(ProtocolParameterNames.RECON_RING_ARITIFACT_ENABLE);

        [XmlIgnore, JsonIgnore]
        public bool SmoothZEnable => GetParameterValue<bool>(ProtocolParameterNames.RECON_SMOOTH_Z_ENABLE);

        [XmlIgnore, JsonIgnore]
        public bool TwoPassEnable => GetParameterValue<bool>(ProtocolParameterNames.RECON_TWO_PASS_ENABLE);

        [XmlIgnore, JsonIgnore]
        public int MaxTablePosition => GetParameterValue<int>(ProtocolParameterNames.RECON_MAX_TABLE_POSITION);

        [XmlIgnore, JsonIgnore]
        public int MinTablePosition => GetParameterValue<int>(ProtocolParameterNames.RECON_MIN_TABLE_POSITION);

        [XmlIgnore, JsonIgnore]
        public string WindowType => GetParameterValue(ProtocolParameterNames.RECON_WINDOW_TYPE, ProtocolParameterNames.RECON_WINDOW_TYPE_CUSTOM);

        [XmlIgnore, JsonIgnore]
        public int[] WindowCenter => GetParameterValue<int[]>(ProtocolParameterNames.RECON_WINDOW_CENTER);

        [XmlIgnore, JsonIgnore]
        public int[] WindowWidth => GetParameterValue<int[]>(ProtocolParameterNames.RECON_WINDOW_WIDTH);

        [XmlIgnore, JsonIgnore]
        public int ViewWindowCenter => GetParameterValue<int>(ProtocolParameterNames.RECON_VIEW_WINDOW_CENTER);

        [XmlIgnore, JsonIgnore]
        public int ViewWindowWidth => GetParameterValue<int>(ProtocolParameterNames.RECON_VIEW_WINDOW_WIDTH);

        [XmlIgnore, JsonIgnore]
        public int CenterFirstX => GetParameterValue<int>(ProtocolParameterNames.RECON_CENTER_FIRST_X);

        [XmlIgnore, JsonIgnore]
        public int CenterFirstY => GetParameterValue<int>(ProtocolParameterNames.RECON_CENTER_FIRST_Y);

        [XmlIgnore, JsonIgnore]
        public int CenterFirstZ => GetParameterValue<int>(ProtocolParameterNames.RECON_CENTER_FIRST_Z);

        [XmlIgnore, JsonIgnore]
        public int CenterLastX => GetParameterValue<int>(ProtocolParameterNames.RECON_CENTER_LAST_X);

        [XmlIgnore, JsonIgnore]
        public int CenterLastY => GetParameterValue<int>(ProtocolParameterNames.RECON_CENTER_LAST_Y);

        [XmlIgnore, JsonIgnore]
        public int CenterLastZ => GetParameterValue<int>(ProtocolParameterNames.RECON_CENTER_LAST_Z);

        [XmlIgnore, JsonIgnore]
        public double FOVDirectionHorizontalX => GetParameterValue<double>(ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X);

        [XmlIgnore, JsonIgnore]
        public double FOVDirectionHorizontalY => GetParameterValue<double>(ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y);

        [XmlIgnore, JsonIgnore]
        public double FOVDirectionHorizontalZ => GetParameterValue<double>(ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z);

        [XmlIgnore, JsonIgnore]
        public double FOVDirectionVerticalX => GetParameterValue<double>(ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X);

        [XmlIgnore, JsonIgnore]
        public double FOVDirectionVerticalY => GetParameterValue<double>(ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y);

        [XmlIgnore, JsonIgnore]
        public double FOVDirectionVerticalZ => GetParameterValue<double>(ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z);

        [XmlIgnore, JsonIgnore]
        public int FOVLengthHorizontal => GetParameterValue<int>(ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL);

        [XmlIgnore, JsonIgnore]
        public int FOVLengthVertical => GetParameterValue<int>(ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL);

        [XmlIgnore, JsonIgnore]
        public int ImageMatrixHorizontal => GetParameterValue<int>(ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL);

        [XmlIgnore, JsonIgnore]
        public int ImageMatrixVertical => GetParameterValue<int>(ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL);

        [XmlIgnore, JsonIgnore]
        public float ImageIncrement => GetParameterValue<float>(ProtocolParameterNames.RECON_IMAGE_INCREMENT);

        [XmlIgnore, JsonIgnore]
        public float IVRTVCoef => GetParameterValue<float>(ProtocolParameterNames.RECON_IVR_TV_COEF);

        /// <summary>
        /// 是否靶重建
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool IsTargetRecon => GetParameterValue<bool>(ProtocolParameterNames.RECON_IS_TARGET_RECON);

        /// <summary>
        /// 是否实时重建（RTD重建）
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool IsRTD
        {
            get
            {
                return GetParameterValue<bool>(ProtocolParameterNames.RECON_IS_RTD);
            }
            set
            {
                base[ProtocolParameterNames.RECON_IS_RTD].Value = value.ToString();
            }
        }

        /// <summary>
        /// 图像序列目录路径
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string ImagePath => GetParameterValue(ProtocolParameterNames.RECON_IMAGE_PATH, null, true);

        /// <summary>
        /// 参考图像路径
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string ReferenceImagePath => GetParameterValue(ProtocolParameterNames.RECON_REFERENCE_IMAGE_PATH, null, true);

        /// <summary>
        /// 图像方向
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public ImageOrders ImageOrder => GetParameterValue<ImageOrders>(ProtocolParameterNames.RECON_IMAGE_ORDER);

        /// <summary>
        /// 序列描述
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string SeriesDescription => GetParameterValue(ProtocolParameterNames.RECON_SERIES_DESCRIPTION, null, true);

        /// <summary>
        /// 序列描述（默认缺省）
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string DefaultSeriesDescription
        {
            get
            {
                return (Parent.ScanOption == ScanOption.DualScout || Parent.ScanOption == ScanOption.Surview) ? Parent.Descriptor.Name : $"{(Parent.IsEnhanced ? Parent.Descriptor.Name : string.Empty)} {(IsRTD ? "RTD " : string.Empty)}{Parent.BodyPart.ToString()} {Math.Round(SliceThickness / 1000.0, 3)} {FilterType.ToString().Replace("Plus", "+")} {WindowType.ToString()} {(IsHDRecon ? "HD " : string.Empty)}";
            }
        }

        /// <summary>
        /// 图像序列编号
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public int SeriesNumber => GetParameterValue<int>(ProtocolParameterNames.RECON_SERIES_NUMBER);

        /// <summary>
        /// 重建失败错误码
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public List<string> ErrorCodes => GetParameterValue<List<string>>(ProtocolParameterNames.ERROR_CODE, true);

        /// <summary>
        /// 增强扫描基底序列图像
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string TestBolusBaseImagePath => GetParameterValue(ProtocolParameterNames.RECON_TESTBOLUS_BASE_IMAGE_PATH, null, true);

        /// <summary>
        /// 是否自动离线重建（关闭检查后）
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool IsAutoRecon => GetParameterValue<bool>(ProtocolParameterNames.RECON_IS_AUTO_RECON);

        /// <summary>
        /// 重建部位
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string ReconBodyPart => GetParameterValue(ProtocolParameterNames.RECON_BODY_PART,null, true);

        /// <summary>
        /// 是否高清重建
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool IsHDRecon => GetParameterValue<bool>(ProtocolParameterNames.RECON_IS_HD_RECON, true);
    }
}
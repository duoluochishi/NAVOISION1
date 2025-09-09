using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.ReconEnums;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Enums;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.Common.MapUI;
using NV.CT.Service.Common.MapUI.Templates;
using NV.CT.Service.Common.Resources;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.Common.Models.ScanReconModels
{
    /// <inheritdoc cref="ReconSeriesParam"/>
    public class ReconSeriesParamModel : ViewModelBase
    {
        #region Field

        private string _reconID = string.Empty;
        private ReconType _reconType;
        private FilterType _filterType;
        private PreBinning _preBinning;
        private EnableType _boneAritifactEnable;
        private EnableType _ringAritifactEnable;
        private EnableType _smoothZEnable;
        private EnableType _isHdRecon;
        private BodyPart? _reconBodyPart;
        private InterpType _interpType;
        private AirCorrectionMode _airCorrectionMode;
        private ScatterAlgorithm _scatterAlgorithm;
        private double _minTablePosition;
        private double _maxTablePosition;
        private string? _nvTestBolusBaseImagePath;
        private ObservableCollection<CircleROIModel> _nvTestBolusRoIs = [];
        private EnableType _twoPassEnable;
        private PreDenoiseType _preDenoiseType;
        private PostDenoiseType _postDenoiseType;
        private int _preDenoiseCoef;
        private int _postDenoiseCoef;
        private float _ivrtvCoef;
        private EnableType _metalAritifactEnable;
        private EnableType _windmillArtifactReduceEnable;
        private EnableType _coneAngleArtifactReduceEnable;
        private double _sliceThickness;
        private ObservableCollection<int> _windowCenter;
        private ObservableCollection<int> _windowWidth;
        private YesNoType _isTargetRecon;
        private double _roiFovCenterX;
        private double _roiFovCenterY;
        private double _centerFirstX;
        private double _centerFirstY;
        private double _centerFirstZ;
        private double _centerLastX;
        private double _centerLastY;
        private double _centerLastZ;
        private double _fovDirectionHorX;
        private double _fovDirectionHorY;
        private double _fovDirectionHorZ;
        private double _fovDirectionVertX;
        private double _fovDirectionVertY;
        private double _fovDirectionVertZ;
        private double _fovLengthHor;
        private double _fovLengthVert;
        private int _imageMatrixHor;
        private int _imageMatrixVert;
        private double _imageIncrement;
        private string _seriesInstanceUID = string.Empty;
        private string _frameOfReferenceUID = string.Empty;
        private ObservableCollection<ReferencedImageModel> _referencedImages;
        private string _modality = string.Empty;
        private int? _seriesNumber;
        private DateTime? _seriesDate;
        private DateTime? _seriesTime;
        private string _protocolName = string.Empty;
        private string _seriesDescription = string.Empty;
        private DateTime? _acquisitionDate;
        private DateTime? _acquisitionTime;
        private DateTime? _acquisitionDateTime;
        private PatientPosition _patientPosition;
        private string _sopClassUID = string.Empty;
        private string _sopClassUIDHeader = string.Empty;
        private string _specificCharacterSet = string.Empty;

        #endregion

        public ReconSeriesParamModel()
        {
            _isHdRecon = EnableType.Disable;
            _metalAritifactEnable = EnableType.Enable;
            _windmillArtifactReduceEnable = EnableType.Disable;
            _coneAngleArtifactReduceEnable = EnableType.Disable;
            _boneAritifactEnable = EnableType.Disable;
            _twoPassEnable = EnableType.Disable;
            _preDenoiseType = PreDenoiseType.BM3D;
            _postDenoiseType = PostDenoiseType.TV;
            _preDenoiseCoef = 2;
            _postDenoiseCoef = 2;
            _ivrtvCoef = 0.02f;
            _windowCenter = [0, 0];
            _windowWidth = [0, 0];
            _isTargetRecon = YesNoType.No;
            _referencedImages = [];
            _reconBodyPart = null;
        }

        #region Property

        /// <inheritdoc cref="ReconSeriesParam.ReconID"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_ReconID), typeof(SimpleMapUITemplate<,>))]
        public string ReconID
        {
            get => _reconID;
            set => SetProperty(ref _reconID, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.ReconType"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_ReconType), typeof(EnumMapUITemplate<,>))]
        public ReconType ReconType
        {
            get => _reconType;
            set => SetProperty(ref _reconType, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.FilterType"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_FilterType), typeof(EnumMapUITemplate<,>))]
        public FilterType FilterType
        {
            get => _filterType;
            set => SetProperty(ref _filterType, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.PreBinning"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_PreBinning), typeof(EnumMapUITemplate<,>))]
        public PreBinning PreBinning
        {
            get => _preBinning;
            set => SetProperty(ref _preBinning, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.BoneAritifactEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_BoneAritifactEnable), typeof(EnumMapUITemplate<,>))]
        public EnableType BoneAritifactEnable
        {
            get => _boneAritifactEnable;
            set => SetProperty(ref _boneAritifactEnable, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.RingAritifactEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_RingAritifactEnable), typeof(EnumMapUITemplate<,>))]
        public EnableType RingAritifactEnable
        {
            get => _ringAritifactEnable;
            set => SetProperty(ref _ringAritifactEnable, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SmoothZEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_SmoothZEnable), typeof(EnumMapUITemplate<,>))]
        public EnableType SmoothZEnable
        {
            get => _smoothZEnable;
            set => SetProperty(ref _smoothZEnable, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.IsHDRecon"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_IsHDRecon), typeof(EnumMapUITemplate<,>))]
        public EnableType IsHDRecon
        {
            get => _isHdRecon;
            set => SetProperty(ref _isHdRecon, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.InterpType"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_InterpType), typeof(EnumMapUITemplate<,>))]
        public InterpType InterpType
        {
            get => _interpType;
            set => SetProperty(ref _interpType, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.ReconBodyPart"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_ReconBodyPart), typeof(EnumNullableMapUITemplate<ReconSeriesParamModel, BodyPart>))]
        public BodyPart? ReconBodyPart
        {
            get => _reconBodyPart;
            set => SetProperty(ref _reconBodyPart, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.AirCorrectionMode"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_AirCorrectionMode), typeof(EnumMapUITemplate<,>))]
        public AirCorrectionMode AirCorrectionMode
        {
            get => _airCorrectionMode;
            set => SetProperty(ref _airCorrectionMode, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.ScatterAlgorithm"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_ScatterAlgorithm), typeof(EnumMapUITemplate<,>))]
        public ScatterAlgorithm ScatterAlgorithm
        {
            get => _scatterAlgorithm;
            set => SetProperty(ref _scatterAlgorithm, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.MinTablePosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double MinTablePosition
        {
            get => _minTablePosition;
            set => SetProperty(ref _minTablePosition, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.MaxTablePosition"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double MaxTablePosition
        {
            get => _maxTablePosition;
            set => SetProperty(ref _maxTablePosition, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.NVTestBolusBaseImagePath"/>
        public string? NVTestBolusBaseImagePath
        {
            get => _nvTestBolusBaseImagePath;
            set => SetProperty(ref _nvTestBolusBaseImagePath, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.NVTestBolusROIs"/>
        public ObservableCollection<CircleROIModel> NVTestBolusROIs
        {
            get => _nvTestBolusRoIs;
            set => SetProperty(ref _nvTestBolusRoIs, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.TwoPassEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_TwoPassEnable), typeof(EnumMapUITemplate<,>))]
        public EnableType TwoPassEnable
        {
            get => _twoPassEnable;
            set => SetProperty(ref _twoPassEnable, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.PreDenoiseType"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_PreDenoiseType), typeof(EnumMapUITemplate<,>))]
        public PreDenoiseType PreDenoiseType
        {
            get => _preDenoiseType;
            set => SetProperty(ref _preDenoiseType, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.PostDenoiseType"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_PostDenoiseType), typeof(EnumMapUITemplate<,>))]
        public PostDenoiseType PostDenoiseType
        {
            get => _postDenoiseType;
            set => SetProperty(ref _postDenoiseType, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.PreDenoiseCoef"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_PreDenoiseCoef), typeof(SimpleMapUITemplate<,>))]
        public int PreDenoiseCoef
        {
            get => _preDenoiseCoef;
            set => SetProperty(ref _preDenoiseCoef, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.PostDenoiseCoef"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_PostDenoiseCoef), typeof(SimpleMapUITemplate<,>))]
        public int PostDenoiseCoef
        {
            get => _postDenoiseCoef;
            set => SetProperty(ref _postDenoiseCoef, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.IVRTVCoef"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_IVRTVCoef), typeof(SimpleMapUITemplate<,>))]
        public float IVRTVCoef
        {
            get => _ivrtvCoef;
            set => SetProperty(ref _ivrtvCoef, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.MetalAritifactEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_MetalAritifactEnable), typeof(EnumMapUITemplate<,>))]
        public EnableType MetalAritifactEnable
        {
            get => _metalAritifactEnable;
            set => SetProperty(ref _metalAritifactEnable, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.WindmillArtifactReduceEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_WindmillArtifactReduceEnable), typeof(EnumMapUITemplate<,>))]
        public EnableType WindmillArtifactReduceEnable
        {
            get => _windmillArtifactReduceEnable;
            set => SetProperty(ref _windmillArtifactReduceEnable, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.ConeAngleArtifactReduceEnable"/>
        /// <remarks>注意！在此处使用<see cref="EnableType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_ConeAngleArtifactReduceEnable), typeof(EnumMapUITemplate<,>))]
        public EnableType ConeAngleArtifactReduceEnable
        {
            get => _coneAngleArtifactReduceEnable;
            set => SetProperty(ref _coneAngleArtifactReduceEnable, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SliceThickness"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_SliceThickness), typeof(SimpleMapUITemplate<,>))]
        public double SliceThickness
        {
            get => _sliceThickness;
            set => SetProperty(ref _sliceThickness, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.WindowCenter"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_WindowCenter), typeof(SimpleCollectionMapUITemplate<,,>))]
        public ObservableCollection<int> WindowCenter
        {
            get => _windowCenter;
            set
            {
                SetProperty(ref _windowCenter, value);
                RaisePropertyChanged(nameof(WindowCenterSingle));
            }
        }

        /// <inheritdoc cref="ReconSeriesParam.WindowWidth"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_WindowWidth), typeof(SimpleCollectionMapUITemplate<,,>))]
        public ObservableCollection<int> WindowWidth
        {
            get => _windowWidth;
            set
            {
                SetProperty(ref _windowWidth, value);
                RaisePropertyChanged(nameof(WindowCenterSingle));
            }
        }

        /// <summary>
        /// 此参数代表 <see cref="WindowCenter"/> 数组的第一个值
        /// </summary>
        [JsonIgnore]
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_WindowCenter), typeof(SimpleMapUITemplate<,>))]
        public int WindowCenterSingle
        {
            get
            {
                if (WindowCenter.Count == 0)
                {
                    WindowCenter.Add(0);
                    return 0;
                }

                return WindowCenter[0];
            }
            set
            {
                if (WindowCenter.Count == 0)
                {
                    WindowCenter.Add(value);
                    RaisePropertyChanged();
                }
                else if (!EqualityComparer<int>.Default.Equals(WindowCenter[0], value))
                {
                    WindowCenter[0] = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 此参数代表 <see cref="WindowWidth"/> 数组的第一个值
        /// </summary>
        [JsonIgnore]
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_WindowWidth), typeof(SimpleMapUITemplate<,>))]
        public int WindowWidthSingle
        {
            get
            {
                if (WindowWidth.Count == 0)
                {
                    WindowWidth.Add(0);
                    return 0;
                }

                return WindowWidth[0];
            }
            set
            {
                if (WindowWidth.Count == 0)
                {
                    WindowWidth.Add(value);
                    RaisePropertyChanged();
                }
                else if (!EqualityComparer<double>.Default.Equals(WindowWidth[0], value))
                {
                    WindowWidth[0] = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <inheritdoc cref="ReconSeriesParam.IsTargetRecon"/>
        /// <remarks>注意！在此处使用<see cref="YesNoType"/>枚举</remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_IsTargetRecon), typeof(EnumMapUITemplate<,>))]
        public YesNoType IsTargetRecon
        {
            get => _isTargetRecon;
            set => SetProperty(ref _isTargetRecon, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.ROIFovCenterX"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double ROIFovCenterX
        {
            get => _roiFovCenterX;
            set => SetProperty(ref _roiFovCenterX, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.ROIFovCenterY"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double ROIFovCenterY
        {
            get => _roiFovCenterY;
            set => SetProperty(ref _roiFovCenterY, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.CenterFirstX"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double CenterFirstX
        {
            get => _centerFirstX;
            set => SetProperty(ref _centerFirstX, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.CenterFirstY"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double CenterFirstY
        {
            get => _centerFirstY;
            set => SetProperty(ref _centerFirstY, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.CenterFirstZ"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double CenterFirstZ
        {
            get => _centerFirstZ;
            set => SetProperty(ref _centerFirstZ, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.CenterLastX"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double CenterLastX
        {
            get => _centerLastX;
            set => SetProperty(ref _centerLastX, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.CenterLastY"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double CenterLastY
        {
            get => _centerLastY;
            set => SetProperty(ref _centerLastY, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.CenterLastZ"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double CenterLastZ
        {
            get => _centerLastZ;
            set => SetProperty(ref _centerLastZ, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.FoVDirectionHorX"/>
        public double FovDirectionHorX
        {
            get => _fovDirectionHorX;
            set => SetProperty(ref _fovDirectionHorX, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.FoVDirectionHorY"/>
        public double FovDirectionHorY
        {
            get => _fovDirectionHorY;
            set => SetProperty(ref _fovDirectionHorY, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.FoVDirectionHorZ"/>
        public double FovDirectionHorZ
        {
            get => _fovDirectionHorZ;
            set => SetProperty(ref _fovDirectionHorZ, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.FoVDirectionVertX"/>
        public double FovDirectionVertX
        {
            get => _fovDirectionVertX;
            set => SetProperty(ref _fovDirectionVertX, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.FoVDirectionVertY"/>
        public double FovDirectionVertY
        {
            get => _fovDirectionVertY;
            set => SetProperty(ref _fovDirectionVertY, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.FoVDirectionVertZ"/>
        public double FovDirectionVertZ
        {
            get => _fovDirectionVertZ;
            set => SetProperty(ref _fovDirectionVertZ, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.FoVLengthHor"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double FovLengthHor
        {
            get => _fovLengthHor;
            set
            {
                SetProperty(ref _fovLengthHor, value);
                RaisePropertyChanged(nameof(FOV));
            }
        }

        /// <inheritdoc cref="ReconSeriesParam.FoVLengthVert"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </remarks>
        public double FovLengthVert
        {
            get => _fovLengthVert;
            set => SetProperty(ref _fovLengthVert, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.ImageMatrixHor"/>
        public int ImageMatrixHor
        {
            get => _imageMatrixHor;
            set
            {
                SetProperty(ref _imageMatrixHor, value);
                RaisePropertyChanged(nameof(ImageMatrix));
            }
        }

        /// <inheritdoc cref="ReconSeriesParam.ImageMatrixVert"/>
        public int ImageMatrixVert
        {
            get => _imageMatrixVert;
            set => SetProperty(ref _imageMatrixVert, value);
        }

        /// <summary>
        /// FOV长度
        /// <para>单位: 毫米(mm)</para>
        /// <para>精度: 小数点后三位，即0.001mm(1μm)</para>
        /// </summary>
        /// <remarks>
        /// 对应 <seealso cref="FovLengthHor"/> 和 <seealso cref="FovLengthVert"/>，这两个值大部分情况下是相同的，在这种情况下前端展示和修改只需要一个值即可。如果需要分别修改，请不要使用此值。
        /// </remarks>
        [JsonIgnore]
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_FOV), typeof(SimpleMapUITemplate<,>))]
        public double FOV
        {
            get => FovLengthHor;
            set
            {
                if (!EqualityComparer<double>.Default.Equals(FovLengthHor, value))
                {
                    FovLengthHor = value;
                    FovLengthVert = value;
                }
            }
        }

        /// <summary>
        /// 矩阵大小，数量值，无单位
        /// </summary>
        /// <remarks>
        /// 对应 <seealso cref="ImageMatrixHor"/> 和 <seealso cref="ImageMatrixVert"/>，这两个值大部分情况下是相同的，在这种情况下前端展示和修改只需要一个值即可。如果需要分别修改，请不要使用此值。
        /// </remarks>
        [JsonIgnore]
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_ImageMatrix), typeof(SimpleMapUITemplate<,>))]
        public int ImageMatrix
        {
            get => ImageMatrixHor;
            set
            {
                if (!EqualityComparer<int>.Default.Equals(ImageMatrixHor, value))
                {
                    ImageMatrixHor = value;
                    ImageMatrixVert = value;
                }
            }
        }

        /// <inheritdoc cref="ReconSeriesParam.ImageIncrement"/>
        /// <remarks>
        /// 注意！在此处与上述源注释中的的单位可能不同！
        /// <para>单位: 毫米(mm)</para>
        /// </remarks>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_ImageIncrement), typeof(SimpleMapUITemplate<,>))]
        public double ImageIncrement
        {
            get => _imageIncrement;
            set => SetProperty(ref _imageIncrement, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SeriesInstanceUID"/>
        public string SeriesInstanceUID
        {
            get => _seriesInstanceUID;
            set => SetProperty(ref _seriesInstanceUID, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.FrameOfReferenceUID"/>
        public string FrameOfReferenceUID
        {
            get => _frameOfReferenceUID;
            set => SetProperty(ref _frameOfReferenceUID, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.ReferencedImages"/>
        public ObservableCollection<ReferencedImageModel> ReferencedImages
        {
            get => _referencedImages;
            set => SetProperty(ref _referencedImages, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.Modality"/>
        public string Modality
        {
            get => _modality;
            set => SetProperty(ref _modality, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SeriesNumber"/>
        public int? SeriesNumber
        {
            get => _seriesNumber;
            set => SetProperty(ref _seriesNumber, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SeriesDate"/>
        public DateTime? SeriesDate
        {
            get => _seriesDate;
            set => SetProperty(ref _seriesDate, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SeriesTime"/>
        public DateTime? SeriesTime
        {
            get => _seriesTime;
            set => SetProperty(ref _seriesTime, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.ProtocolName"/>
        public string ProtocolName
        {
            get => _protocolName;
            set => SetProperty(ref _protocolName, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SeriesDescription"/>
        public string SeriesDescription
        {
            get => _seriesDescription;
            set => SetProperty(ref _seriesDescription, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.AcquisitionDate"/>
        public DateTime? AcquisitionDate
        {
            get => _acquisitionDate;
            set => SetProperty(ref _acquisitionDate, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.AcquisitionTime"/>
        public DateTime? AcquisitionTime
        {
            get => _acquisitionTime;
            set => SetProperty(ref _acquisitionTime, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.AcquisitionDateTime"/>
        public DateTime? AcquisitionDateTime
        {
            get => _acquisitionDateTime;
            set => SetProperty(ref _acquisitionDateTime, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.PatientPosition"/>
        [MapUI(typeof(Common_Lang), nameof(Common_Lang.MapUI_Recon_PatientPosition), typeof(EnumMapUITemplate<,>))]
        public PatientPosition PatientPosition
        {
            get => _patientPosition;
            set => SetProperty(ref _patientPosition, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SOPClassUID"/>
        public string SOPClassUID
        {
            get => _sopClassUID;
            set => SetProperty(ref _sopClassUID, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SOPClassUIDHeader"/>
        public string SOPClassUIDHeader
        {
            get => _sopClassUIDHeader;
            set => SetProperty(ref _sopClassUIDHeader, value);
        }

        /// <inheritdoc cref="ReconSeriesParam.SpecificCharacterSet"/>
        public string SpecificCharacterSet
        {
            get => _specificCharacterSet;
            set => SetProperty(ref _specificCharacterSet, value);
        }

        #endregion

        public ReconSeriesParam Converter()
        {
            return Global.Instance.ServiceProvider.GetRequiredService<IMapper>().Map<ReconSeriesParam>(this);
        }
    }
}
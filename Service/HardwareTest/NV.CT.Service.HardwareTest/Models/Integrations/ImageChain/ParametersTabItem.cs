using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.Collimator;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using NV.CT.Service.Common.Enums;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.MapUI;
using NV.CT.Service.Common.MapUI.Templates;
using NV.CT.Service.Common.Models.ScanReconModels;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Share.Enums;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using NV.CT.Service.Models;

namespace NV.CT.Service.HardwareTest.Models.Integrations.ImageChain
{
    /// <summary>
    /// 参数Tab页
    /// </summary>
    public abstract partial class AbstractParametersTabItem : ObservableObject
    {
        private readonly StringBuilder _messageBuilder = new();
        private ObservableCollection<AbstractMapUITemplate> _parameters = null!;

        protected AbstractParametersTabItem(string header)
        {
            TabItemHeader = header;
        }

        [ObservableProperty]
        private string _tabItemHeader = string.Empty;

        [ObservableProperty]
        private string _consoleMessage = string.Empty;

        [ObservableProperty]
        private double _progressValue;

        public ObservableCollection<AbstractMapUITemplate> Parameters
        {
            get => _parameters;
            protected set => SetProperty(ref _parameters, value);
        }

        [RelayCommand]
        private void ClearConsole()
        {
            _messageBuilder.Clear();
            ConsoleMessage = string.Empty;
        }

        public void PrintConsoleMessage(string message, PrintLevel level)
        {
            _messageBuilder.AppendLine(MessagePrinterHelper.MessageWrapper(message, level));
            ConsoleMessage = _messageBuilder.ToString();
        }

        public abstract string GetImageDataFolder();
    }

    /// <summary>
    /// 扫描参数Tab页
    /// </summary>
    public partial class ScanParametersTabItem : AbstractParametersTabItem
    {
        private ScanParamModel _scanParam;

        public ScanParametersTabItem(string header) : base(header)
        {
            _scanParam = new()
            {
                AutoScan = EnableType.Enable,
                FunctionMode = FunctionMode.AcqService,
                ScanNumber = 0,
                ScanLength = 19.93,
                VoltageSingle = 120,
                CurrentSingle = 140,
                ExposureTime = 3,
                FrameTime = 5,
                FramesPerCycle = 360,
                ScanOption = ScanOption.Axial,
                ScanMode = ScanMode.JumpExposure,
                ExposureMode = ExposureMode.Single,
                ExposureTriggerMode = ExposureTriggerMode.TimeTrigger,
                TableDirection = TableDirection.In,
                CollimatorOpenMode = CollimatorOpenMode.NearSmallAngle,
                CollimatorZ = 242,
                BowtieEnable = EnableType.Enable,
                ReconVolumeStartPosition = -1000,
                Gain = Gain.Dynamic,
                TableHeight = 900,
                BodyPart = BodyPart.HEAD,
                ObjectFov = ObjectFovMode.Fov150,
                PostDeleteLength = 0,
                ScanBinning = ScanBinning.Bin11,
                ExposureDelayTime = 20,
                TableFeed = 20,
                TableAcceleration = 200,
                Focal = FocalType.Small,
                Pitch = 0.5,
                RawDataType = RawDataType.UserScan,
                GantryAcceleration = 25,
                AutoDeleteNum = 48,
                ScanFOV = 506.88,
                RDelay = 0.3,
                TDelay = 0.3,
                BodyCategory = BodyCategory.Adult,
                CollimatorOffsetEnable = EnableType.Enable,
                Loops = 0,
                LoopTime = 0,
                PreVoiceID = 0,
                PreVoiceDelayTime = 0,
                PreVoicePlayTime = 0,
                PostVoiceID = 0,
                SpotDelay = 0,
                WarmUp = EnableType.Disable,
                WarmUpTubeNumber = 0,
            };
            ScanParam.AmendScanLength();
            ScanParam.CalculateEffectiveFrames();
            var res = ScanParam.CalculateTotalFramesAndDisplayFrames();

            if (!res.status)
            {
                throw new ArithmeticException(res.message);
            }

            ScanParam.PropertyChanged += ScanParam_PropertyChanged;
            CreateParameters(ScanParam);
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanExecuteReconProcedure))]
        [NotifyPropertyChangedFor(nameof(CanLoadRawData))]
        [NotifyPropertyChangedFor(nameof(CanOpenDataAnalysisTool))]
        private string _rawDataDirectory = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanStartScan))]
        private ScanStatus _scanStatus = ScanStatus.NormalStop;

        public ScanParamModel ScanParam
        {
            get => _scanParam;
            private set => SetProperty(ref _scanParam, value);
        }

        public bool CanStartScan => ScanStatus == ScanStatus.NormalStop;
        public bool CanLoadRawData => !string.IsNullOrWhiteSpace(RawDataDirectory) && Directory.Exists(RawDataDirectory);
        public bool CanOpenDataAnalysisTool => !string.IsNullOrWhiteSpace((RawDataDirectory)) && Directory.Exists((RawDataDirectory));
        public bool CanExecuteReconProcedure => !string.IsNullOrWhiteSpace(RawDataDirectory) && Directory.Exists(RawDataDirectory);
        public event Action<string>? OnCalculateError;

        public GenericResponse ModifyScanParam(ScanParamModel model)
        {
            model.AmendScanLength();
            model.CalculateEffectiveFrames();
            var res = model.CalculateTotalFramesAndDisplayFrames();

            if (!res.status)
            {
                return res;
            }

            ScanParam.PropertyChanged -= ScanParam_PropertyChanged;
            model.PropertyChanged += ScanParam_PropertyChanged;
            ScanParam = model;
            CreateParameters(ScanParam);
            return new(true, string.Empty);
        }

        public override string GetImageDataFolder()
        {
            return RawDataDirectory;
        }

        private void CreateParameters(ScanParamModel scanParam)
        {
            Parameters =
            [
                scanParam.GetMapUITemplate(nameof(scanParam.ScanLength)),
                // scanParam.GetMapUITemplate(nameof(scanParam.EffectiveFrames)), //TODO:暂时屏蔽，等Alg库提供相关计算后再放开
                scanParam.GetMapUITemplate(nameof(scanParam.DisplayFrames)),
                scanParam.GetMapUITemplate(nameof(scanParam.VoltageSingle)),
                scanParam.GetMapUITemplate(nameof(scanParam.CurrentSingle)),
                // scanParam.GetMapUITemplate(nameof(scanParam.Voltage)),
                // scanParam.GetMapUITemplate(nameof(scanParam.Current)),
                scanParam.GetMapUITemplate(nameof(scanParam.ExposureTime)),
                scanParam.GetMapUITemplate(nameof(scanParam.FrameTime)),
                scanParam.GetMapUITemplate(nameof(scanParam.ExposureDelayTime)),
                scanParam.GetMapUITemplate(nameof(scanParam.ExposureTriggerMode)),
                scanParam.GetMapUITemplate(nameof(scanParam.FramesPerCycle)),
                scanParam.GetMapUITemplate(nameof(scanParam.ScanOption)),
                scanParam.GetMapUITemplate(nameof(scanParam.ScanMode)),
                scanParam.GetMapUITemplate(nameof(scanParam.ExposureMode)),
                scanParam.GetMapUITemplate(nameof(scanParam.BowtieEnable)),
                scanParam.GetMapUITemplate(nameof(scanParam.Gain)),
                scanParam.GetMapUITemplate(nameof(scanParam.ScanBinning)),
                scanParam.GetMapUITemplate(nameof(scanParam.Focal)),
                scanParam.GetMapUITemplate(nameof(scanParam.BodyCategory)),
                scanParam.GetMapUITemplate(nameof(scanParam.BodyPart)),
                scanParam.GetMapUITemplate(nameof(scanParam.ObjectFov)),
                scanParam.GetMapUITemplate(nameof(scanParam.PostDeleteLength)),
                scanParam.GetMapUITemplate(nameof(scanParam.CollimatorOpenMode)),
                scanParam.GetMapUITemplate(nameof(scanParam.CollimatorZ)),
                scanParam.GetMapUITemplate(nameof(scanParam.CollimatorOffsetEnable)),
                scanParam.GetMapUITemplate(nameof(scanParam.AutoDeleteNum)),
                scanParam.GetMapUITemplate(nameof(scanParam.ScanFOV)),
                scanParam.GetMapUITemplate(nameof(scanParam.TubePositions)),
                scanParam.GetMapUITemplate(nameof(scanParam.Pitch)),
                scanParam.GetMapUITemplate(nameof(scanParam.TableDirection)),
                scanParam.GetMapUITemplate(nameof(scanParam.ReconVolumeStartPosition)),
                scanParam.GetMapUITemplate(nameof(scanParam.TableHeight)),
                scanParam.GetMapUITemplate(nameof(scanParam.TableFeed)),
                scanParam.GetMapUITemplate(nameof(scanParam.TableAcceleration)),
                scanParam.GetMapUITemplate(nameof(scanParam.GantryAcceleration)),
                scanParam.GetMapUITemplate(nameof(scanParam.RDelay)),
                scanParam.GetMapUITemplate(nameof(scanParam.TDelay)),
                scanParam.GetMapUITemplate(nameof(scanParam.AllowErrorXRaySourceCount)),
                scanParam.GetMapUITemplate(nameof(scanParam.mAs)),
                scanParam.GetMapUITemplate(nameof(scanParam.CTDIvol)),
            ];
        }

        private void ScanParam_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var res = ScanParam.CalculateWhenPropertyChanged(e.PropertyName, true);

            if (!res.status)
            {
                OnCalculateError?.Invoke(res.message);
            }
        }
    }

    /// <summary>
    /// 重建参数Tab页
    /// </summary>
    public partial class ReconParametersTabItem : AbstractParametersTabItem
    {
        public ReconParametersTabItem(string header) : base(header)
        {
            ReconParam = new()
            {
                ReconType = ReconType.IVR_TV,
                FilterType = FilterType.SmoothPlus,
                BoneAritifactEnable = EnableType.Disable,
                RingAritifactEnable = EnableType.Disable,
                SmoothZEnable = EnableType.Disable,
                IsHDRecon = EnableType.Disable,
                ScatterAlgorithm = ScatterAlgorithm.SD,
                InterpType = InterpType.InterpNone,
                AirCorrectionMode = AirCorrectionMode.None,
                TwoPassEnable = EnableType.Disable,
                PreDenoiseType = PreDenoiseType.BM3D,
                PreDenoiseCoef = 2,
                PostDenoiseType = PostDenoiseType.TV,
                PostDenoiseCoef = 2,
                MetalAritifactEnable = EnableType.Disable,
                SliceThickness = 3,
                WindowCenter = [0],
                WindowWidth = [100],
                CenterFirstX = 0,
                CenterFirstY = 0,
                CenterFirstZ = 0,
                CenterLastX = 0,
                CenterLastY = 0,
                CenterLastZ = 0,
                FovDirectionHorX = 1,
                FovDirectionHorY = 0,
                FovDirectionHorZ = 0,
                FovDirectionVertX = 0,
                FovDirectionVertY = 1,
                FovDirectionVertZ = 0,
                FOV = 253.44,
                ImageMatrix = 768,
                ImageIncrement = 0.33,
                PatientPosition = PatientPosition.HFS,
            };
            Parameters =
            [
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.ReconType)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.FilterType)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.InterpType)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.PreBinning)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.ReconBodyPart)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.AirCorrectionMode)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.TwoPassEnable)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.BoneAritifactEnable)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.MetalAritifactEnable)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.WindmillArtifactReduceEnable)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.ConeAngleArtifactReduceEnable)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.RingAritifactEnable)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.SmoothZEnable)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.IsHDRecon)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.ScatterAlgorithm)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.PreDenoiseType)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.PreDenoiseCoef)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.PostDenoiseType)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.PostDenoiseCoef)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.IVRTVCoef)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.SliceThickness)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.WindowWidthSingle)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.WindowCenterSingle)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.FOV)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.ImageMatrix)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.ImageIncrement)),
                ReconParam.GetMapUITemplate(nameof(ReconSeriesParamModel.PatientPosition))
            ];
        }

        public string TaskID { get; set; } = string.Empty;
        public string ReconID => ReconParam.ReconID;
        public string ReconSeriesUID => ReconParam.SeriesInstanceUID;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanStartRecon))]
        [NotifyPropertyChangedFor(nameof(CanStopRecon))]
        [NotifyPropertyChangedFor(nameof(CanLoadReconImage))]
        [NotifyPropertyChangedFor(nameof(CanOpenDataAnalysisTool))]
        private string _reconImageDirectory = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanStartRecon))]
        private ReconStatus _reconStatus = ReconStatus.NormalStop;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanStartRecon))]
        [NotifyPropertyChangedFor(nameof(CanStopRecon))]
        private bool _hasValidRawData;

        public bool CanStartRecon => HasValidRawData && string.IsNullOrWhiteSpace(ReconImageDirectory) && ReconStatus == ReconStatus.NormalStop;
        public bool CanStopRecon => HasValidRawData && string.IsNullOrWhiteSpace(ReconImageDirectory);
        public bool CanLoadReconImage => !string.IsNullOrWhiteSpace(ReconImageDirectory) && Directory.Exists(ReconImageDirectory);
        public bool CanOpenDataAnalysisTool => !string.IsNullOrWhiteSpace(ReconImageDirectory) && Directory.Exists(ReconImageDirectory);
        public ReconSeriesParamModel ReconParam { get; }

        public override string GetImageDataFolder()
        {
            return ReconImageDirectory;
        }
    }
}
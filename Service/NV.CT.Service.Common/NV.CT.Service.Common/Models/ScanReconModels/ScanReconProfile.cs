using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NV.CT.FacadeProxy.Common.Enums.Collimator;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Common.Models.ScanRecon;
using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.Service.Common.Enums;
using NV.MPS.Environment;

namespace NV.CT.Service.Common.Models.ScanReconModels
{
    internal class ScanReconProfile : Profile
    {
        public ScanReconProfile()
        {
            AllowNullCollections = true;
            AllowNullDestinationValues = true;
            CreateMap<PatientModel, Patient>().ReverseMap();
            CreateMap<StudyModel, Study>().ReverseMap();
            CreateMap<ReferencedImageModel, ReferencedImage>().ReverseMap();
            CreateMap<CircleROIModel, CircleROI>().ReverseMap();
            CreateScanMap();
            CreateScanReverseMap();
            CreateFreeScanMap();
            CreateReconMap();
            CreateReconReverseMap();
            CreateMap<ScanReconParamModel, ScanReconParam>().ForAllMembers(opt => opt.Condition((_, _, srcMember) => srcMember != null));
            CreateMap<ScanReconParam, ScanReconParamModel>();
        }

        /// <summary>
        /// 注意与 <see cref="CreateScanReverseMap"/> 中的一一对应
        /// </summary>
        private void CreateScanMap()
        {
            CreateMap<ScanParamModel, ScanParam>()
                   .IncludeAllDerived()
                   .IgnoreAllPropertiesWithAnInaccessibleSetter()
                   .ForMember(dest => dest.AutoScan,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.AutoScan)))
                   .ForMember(dest => dest.BowtieEnable,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.BowtieEnable)))
                   .ForMember(dest => dest.WarmUp,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.WarmUp)))
                   .ForMember(dest => dest.CollimatorOffsetEnable,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.CollimatorOffsetEnable)))
                   .ForMember(dest => dest.kV,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.Voltage, arr)))
                   .ForMember(dest => dest.mA,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.Current, arr, i => (uint)i.ExpandThousand())))
                   .ForMember(dest => dest.TubePositions,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.TubePositions, arr)))
                   .ForMember(dest => dest.TubeNumbers,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.TubeNumbers, arr)))
                   .ForMember(dest => dest.ExposureTime,
                              opt => opt.MapFrom(src => (uint)src.ExposureTime.Millisecond2Microsecond()))
                   .ForMember(dest => dest.FrameTime,
                              opt => opt.MapFrom(src => (uint)src.FrameTime.Millisecond2Microsecond()))
                   .ForMember(dest => dest.ExposureDelayTime,
                              opt => opt.MapFrom(src => (uint)src.ExposureDelayTime.Second2Microsecond()))
                   .ForMember(dest => dest.TableAccelerationTime,
                              opt => opt.MapFrom(src => (uint)src.TableAccelerationTime.Millisecond2Microsecond()))
                   .ForMember(dest => dest.PreVoicePlayTime,
                              opt => opt.MapFrom(src => (uint)src.PreVoicePlayTime.Millisecond2Microsecond()))
                   .ForMember(dest => dest.PreVoiceDelayTime,
                              opt => opt.MapFrom(src => (uint)src.PreVoiceDelayTime.Millisecond2Microsecond()))
                   .ForMember(dest => dest.LoopTime,
                              opt => opt.MapFrom(src => (uint)src.LoopTime.Millisecond2Microsecond()))
                   .ForMember(dest => dest.RDelay,
                              opt => opt.MapFrom(src => (uint)src.RDelay.Millisecond2Microsecond()))
                   .ForMember(dest => dest.TDelay,
                              opt => opt.MapFrom(src => (uint)src.TDelay.Millisecond2Microsecond()))
                   .ForMember(dest => dest.SpotDelay,
                              opt => opt.MapFrom(src => (uint)src.SpotDelay.Millisecond2Microsecond()))
                   .ForMember(dest => dest.Pitch,
                              opt => opt.MapFrom(src => (uint)src.Pitch.ExpandHundred()))
                   .ForMember(dest => dest.ReconVolumeStartPosition,
                              opt => opt.MapFrom(src => src.ReconVolumeStartPosition.Millimeter2Micron()))
                   .ForMember(dest => dest.ReconVolumeEndPosition,
                              opt => opt.MapFrom(src => src.ReconVolumeEndPosition.Millimeter2Micron()))
                   .ForMember(dest => dest.TableStartPosition,
                              opt => opt.MapFrom(src => src.TableStartPosition.Millimeter2Micron()))
                   .ForMember(dest => dest.TableEndPosition,
                              opt => opt.MapFrom(src => src.TableEndPosition.Millimeter2Micron()))
                   .ForMember(dest => dest.ExposureStartPosition,
                              opt => opt.MapFrom(src => src.ExposureStartPosition.Millimeter2Micron()))
                   .ForMember(dest => dest.ExposureEndPosition,
                              opt => opt.MapFrom(src => src.ExposureEndPosition.Millimeter2Micron()))
                   .ForMember(dest => dest.TableHeight,
                              opt => opt.MapFrom(src => (uint)src.TableHeight.Millimeter2Micron()))
                   .ForMember(dest => dest.TableSpeed,
                              opt => opt.MapFrom(src => (uint)src.TableSpeed.Millimeter2Micron()))
                   .ForMember(dest => dest.TableFeed,
                              opt => opt.MapFrom(src => (uint)src.TableFeed.Millimeter2Micron()))
                   .ForMember(dest => dest.TableAcceleration,
                              opt => opt.MapFrom(src => (uint)src.TableAcceleration.Millimeter2Micron()))
                   .ForMember(dest => dest.GantryStartPosition,
                              opt => opt.MapFrom(src => (uint)src.GantryStartPosition.ExpandHundred()))
                   .ForMember(dest => dest.GantryEndPosition,
                              opt => opt.MapFrom(src => (uint)src.GantryEndPosition.ExpandHundred()))
                   .ForMember(dest => dest.GantryAcceleration,
                              opt => opt.MapFrom(src => (uint)src.GantryAcceleration.ExpandHundred()))
                   .ForMember(dest => dest.GantryAccelerationTime,
                              opt => opt.MapFrom(src => (uint)src.GantryAccelerationTime.Millisecond2Microsecond()))
                   .ForMember(dest => dest.GantrySpeed,
                              opt => opt.MapFrom(src => (uint)src.GantrySpeed.ExpandHundred()))
                   .ForMember(dest => dest.LargeAngleDeleteLength,
                              opt => opt.MapFrom(src => (uint)src.LargeAngleDeleteLength.Millimeter2Micron()))
                   .ForMember(dest => dest.SmallAngleDeleteLength,
                              opt => opt.MapFrom(src => (uint)src.SmallAngleDeleteLength.Millimeter2Micron()))
                   .ForMember(dest => dest.ScanFOV,
                              opt => opt.MapFrom(src => (uint)src.ScanFOV.Millimeter2Micron()));
        }

        /// <summary>
        /// 注意与 <see cref="CreateScanMap"/> 中的一一对应
        /// </summary>
        private void CreateScanReverseMap()
        {
            CreateMap<ScanParam, ScanParamModel>()
                   .IncludeAllDerived()
                   .IgnoreAllPropertiesWithAnInaccessibleSetter()
                   .ForMember(dest => dest.AutoScan,
                              opt => opt.MapFrom(src => BoolToEnableType(src.AutoScan)))
                   .ForMember(dest => dest.BowtieEnable,
                              opt => opt.MapFrom(src => BoolToEnableType(src.BowtieEnable)))
                   .ForMember(dest => dest.WarmUp,
                              opt => opt.MapFrom(src => BoolToEnableType(src.WarmUp)))
                   .ForMember(dest => dest.CollimatorOffsetEnable,
                              opt => opt.MapFrom(src => BoolToEnableType(src.CollimatorOffsetEnable)))
                   .ForMember(dest => dest.Voltage,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.kV, arr)))
                   .ForMember(dest => dest.Current,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.mA, arr, i => Math.Round(((double)i).ReduceThousand(), 3))))
                   .ForMember(dest => dest.TubePositions,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.TubePositions, arr)))
                   .ForMember(dest => dest.TubeNumbers,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.TubeNumbers, arr)))
                   .ForMember(dest => dest.ExposureTime,
                              opt => opt.MapFrom(src => ((double)src.ExposureTime).Microsecond2Millisecond()))
                   .ForMember(dest => dest.FrameTime,
                              opt => opt.MapFrom(src => ((double)src.FrameTime).Microsecond2Millisecond()))
                   .ForMember(dest => dest.ExposureDelayTime,
                              opt => opt.MapFrom(src => ((double)src.ExposureDelayTime).Microsecond2Second()))
                   .ForMember(dest => dest.TableAccelerationTime,
                              opt => opt.MapFrom(src => ((double)src.TableAccelerationTime).Microsecond2Millisecond()))
                   .ForMember(dest => dest.PreVoicePlayTime,
                              opt => opt.MapFrom(src => ((double)src.PreVoicePlayTime).Microsecond2Millisecond()))
                   .ForMember(dest => dest.PreVoiceDelayTime,
                              opt => opt.MapFrom(src => ((double)src.PreVoiceDelayTime).Microsecond2Millisecond()))
                   .ForMember(dest => dest.LoopTime,
                              opt => opt.MapFrom(src => ((double)src.LoopTime).Microsecond2Millisecond()))
                   .ForMember(dest => dest.RDelay,
                              opt => opt.MapFrom(src => ((double)src.RDelay).Microsecond2Millisecond()))
                   .ForMember(dest => dest.TDelay,
                              opt => opt.MapFrom(src => ((double)src.TDelay).Microsecond2Millisecond()))
                   .ForMember(dest => dest.SpotDelay,
                              opt => opt.MapFrom(src => ((double)src.SpotDelay).Microsecond2Millisecond()))
                   .ForMember(dest => dest.Pitch,
                              opt => opt.MapFrom(src => Math.Round(((double)src.Pitch).ReduceHundred(), 2)))
                   .ForMember(dest => dest.ScanLength,
                              opt => opt.MapFrom(src => ((double)Math.Abs(src.ReconVolumeStartPosition - src.ReconVolumeEndPosition)).Micron2Millimeter()))
                   .ForMember(dest => dest.ReconVolumeStartPosition,
                              opt => opt.MapFrom(src => ((double)src.ReconVolumeStartPosition).Micron2Millimeter()))
                   .ForMember(dest => dest.ReconVolumeEndPosition,
                              opt => opt.MapFrom(src => ((double)src.ReconVolumeEndPosition).Micron2Millimeter()))
                   .ForMember(dest => dest.TableStartPosition,
                              opt => opt.MapFrom(src => ((double)src.TableStartPosition).Micron2Millimeter()))
                   .ForMember(dest => dest.TableEndPosition,
                              opt => opt.MapFrom(src => ((double)src.TableEndPosition).Micron2Millimeter()))
                   .ForMember(dest => dest.ExposureStartPosition,
                              opt => opt.MapFrom(src => ((double)src.ExposureStartPosition).Micron2Millimeter()))
                   .ForMember(dest => dest.ExposureEndPosition,
                              opt => opt.MapFrom(src => ((double)src.ExposureEndPosition).Micron2Millimeter()))
                   .ForMember(dest => dest.TableHeight,
                              opt => opt.MapFrom(src => ((double)src.TableHeight).Micron2Millimeter()))
                   .ForMember(dest => dest.TableSpeed,
                              opt => opt.MapFrom(src => ((double)src.TableSpeed).Micron2Millimeter()))
                   .ForMember(dest => dest.TableFeed,
                              opt => opt.MapFrom(src => ((double)src.TableFeed).Micron2Millimeter()))
                   .ForMember(dest => dest.TableAcceleration,
                              opt => opt.MapFrom(src => ((double)src.TableAcceleration).Micron2Millimeter()))
                   .ForMember(dest => dest.GantryStartPosition,
                              opt => opt.MapFrom(src => Math.Round(((double)src.GantryStartPosition).ReduceHundred(), 2)))
                   .ForMember(dest => dest.GantryEndPosition,
                              opt => opt.MapFrom(src => Math.Round(((double)src.GantryEndPosition).ReduceHundred(), 2)))
                   .ForMember(dest => dest.GantryAcceleration,
                              opt => opt.MapFrom(src => Math.Round(((double)src.GantryAcceleration).ReduceHundred(), 2)))
                   .ForMember(dest => dest.GantryAccelerationTime,
                              opt => opt.MapFrom(src => ((double)src.GantryAccelerationTime).Microsecond2Millisecond()))
                   .ForMember(dest => dest.GantrySpeed,
                              opt => opt.MapFrom(src => Math.Round(((double)src.GantrySpeed).ReduceHundred(), 2)))
                   .ForMember(dest => dest.LargeAngleDeleteLength,
                              opt => opt.MapFrom(src => ((double)src.LargeAngleDeleteLength).Micron2Millimeter()))
                   .ForMember(dest => dest.SmallAngleDeleteLength,
                              opt => opt.MapFrom(src => ((double)src.SmallAngleDeleteLength).Micron2Millimeter()))
                   .ForMember(dest => dest.ScanFOV,
                              opt => opt.MapFrom(src => ((double)src.ScanFOV).Micron2Millimeter()));
        }

        private void CreateFreeScanMap()
        {
            CreateMap<FreeProtocolScanParamModel, DataAcquisitionParams>()
                   .ConvertUsing((src, _) => new()
                    {
                        ScanUID = src.ScanUID,
                        StudyUID = src.StudyUID,
                        FuncMode = src.FunctionMode,
                        XRawDataType = src.RawDataType,
                        ExposureParams =
                        {
                            kVs = [..src.Voltage],
                            mAs = [..src.Current.Select(i => (uint)i.ExpandThousand())],
                            ExposureTime = src.ExposureTime.Millisecond2Microsecond(),
                            FrameTime = src.FrameTime.Millisecond2Microsecond(),
                            TotalFrames = src.TotalFrames,
                            AutoDeleteNumber = src.AutoDeleteNum,
                            ScanOption = src.ScanOption,
                            ScanMode = src.ScanMode,
                            ExposureMode = src.ExposureMode,
                            ExposureTriggerMode = src.ExposureTriggerMode,
                            Bowtie = EnableTypeToBool(src.BowtieEnable),
                            CollimatorSwitch = src.CollimatorSwitch == SwitchType.On,
                            CollimatorOpenMode = CollimatorOpenTypeToUInt(src.CollimatorOpenMode),
                            CollimatorOpenWidth = src.CollimatorZ,
                            TableTriggerEnable = (uint)EnableToInt(src.TableTriggerEnable),
                            FramesPerCycle = src.FramesPerCycle,
                            AutoScan = EnableTypeToBool(src.AutoScan),
                            ExposureDelayTime = src.ExposureDelayTime.Second2Microsecond(),
                            XRaySourceIndex = src.XRaySourceIndex,
                            Focal = src.Focal,
                            ExposureRelatedChildNodesConfig = src.ExposureRelatedChildNodesConfig,
                            SlaveDevTest = src.SlaveDevTest,
                            PostOffsetFrames = src.PostOffsetFrames,
                            PrepareTimeout = (uint)src.PrepareTimeout.Millisecond2Microsecond(),
                            ExposureTimeout = (uint)src.ExposureTimeout.Millisecond2Microsecond(),
                            TableEndPosition = (uint)src.TableEndPosition.Millimeter2Micron(),
                            TableDirection = src.TableDirection,
                            GantryStartPosition = (uint)src.GantryStartPosition.ExpandHundred(),
                            GantryEndPosition = (uint)src.GantryEndPosition.ExpandHundred(),
                            GantryDirection = src.GantryDirection,
                            GantrySpeed = (uint)src.GantrySpeed.ExpandHundred(),
                            GantryAccTime = (uint)src.GantryAccelerationTime.Millisecond2Microsecond(),
                        },
                        DetectorParams =
                        {
                            CurrentDoubleRead = src.CurrentDoubleRead,
                            ImageFlip = src.ImageFlip,
                            WriteConfig = src.WriteConfig,
                            CurrentImageMode = src.CurrentImageMode,
                            CurrentGain = src.Gain,
                            CurrentBinning = src.ScanBinning,
                            CurrentShutterMode = src.CurrentShutterMode,
                            DelayExposureTime = src.ExposureDelayTime.Second2Microsecond(),
                            FrameTime = src.FrameTime.Millisecond2Microsecond(),
                            CurrentAcquisitionMode = src.CurrentAcquisitionMode,
                            ReadDealyTime = src.ReadDealyTime.Millisecond2Microsecond(),
                            HeartBeatTimeInterval = src.HeartBeatTimeInterval.Millisecond2Microsecond(),
                            PrintfEnable = (uint)EnableToInt(src.PrintfEnable),
                            TargetTemperature = (int)src.TargetTemperature.ExpandTen(),
                            EncodeMode = src.EncodeMode,
                            RDelay = src.RDelay.Millisecond2Microsecond(),
                            TDelay = src.TDelay.Millisecond2Microsecond(),
                            SpotDelay = src.SpotDelay.Millisecond2Microsecond(),
                            // CollimatorSpotDelay = src.CollimatorSpotDelay.Millisecond2Microsecond(),
                            PreOffsetEnable = EnableToInt(src.PreOffsetEnable),
                            PreOffsetAcqTotalFrame = (int)src.PreOffsetFrames,
                            PreOffsetAcqStartVaildFrame = src.PreOffsetAcqStartVaildFrame,
                            CurrentScatteringGain = src.CurrentScatteringGain,
                            CurrentSpiTime = src.ExposureTime.Millisecond2Microsecond(),
                            ExposureTime = src.ExposureTime.Millisecond2Microsecond(),
                        },
                    });
        }

        /// <summary>
        /// 注意与 <see cref="CreateReconReverseMap"/> 中的一一对应
        /// </summary>
        private void CreateReconMap()
        {
            CreateMap<ReconSeriesParamModel, ReconSeriesParam>()
                   .IgnoreAllPropertiesWithAnInaccessibleSetter()
                   .ForMember(dest => dest.BoneAritifactEnable,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.BoneAritifactEnable)))
                   .ForMember(dest => dest.RingAritifactEnable,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.RingAritifactEnable)))
                   .ForMember(dest => dest.SmoothZEnable,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.SmoothZEnable)))
                   .ForMember(dest => dest.IsHDRecon,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.IsHDRecon)))
                   .ForMember(dest => dest.TwoPassEnable,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.TwoPassEnable)))
                   .ForMember(dest => dest.MetalAritifactEnable,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.MetalAritifactEnable)))
                   .ForMember(dest => dest.WindmillArtifactReduceEnable,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.WindmillArtifactReduceEnable)))
                   .ForMember(dest => dest.ConeAngleArtifactReduceEnable,
                              opt => opt.MapFrom(src => EnableTypeToBool(src.ConeAngleArtifactReduceEnable)))
                   .ForMember(dest => dest.IsTargetRecon,
                              opt => opt.MapFrom(src => YesNoToBool(src.IsTargetRecon)))
                   .ForMember(dest => dest.WindowCenter,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.WindowCenter, arr)))
                   .ForMember(dest => dest.WindowWidth,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.WindowWidth, arr)))
                   .ForMember(dest => dest.SliceThickness,
                              opt => opt.MapFrom(src => (float)src.SliceThickness.ExpandThousand()))
                   .ForMember(dest => dest.MinTablePosition,
                              opt => opt.MapFrom(src => src.MinTablePosition.Millimeter2Micron()))
                   .ForMember(dest => dest.MaxTablePosition,
                              opt => opt.MapFrom(src => src.MaxTablePosition.Millimeter2Micron()))
                   .ForMember(dest => dest.ROIFovCenterX,
                              opt => opt.MapFrom(src => src.ROIFovCenterX.Millimeter2Micron()))
                   .ForMember(dest => dest.ROIFovCenterY,
                              opt => opt.MapFrom(src => src.ROIFovCenterY.Millimeter2Micron()))
                   .ForMember(dest => dest.CenterFirstX,
                              opt => opt.MapFrom(src => src.CenterFirstX.Millimeter2Micron()))
                   .ForMember(dest => dest.CenterFirstY,
                              opt => opt.MapFrom(src => src.CenterFirstY.Millimeter2Micron()))
                   .ForMember(dest => dest.CenterFirstZ,
                              opt => opt.MapFrom(src => src.CenterFirstZ.Millimeter2Micron()))
                   .ForMember(dest => dest.CenterLastX,
                              opt => opt.MapFrom(src => src.CenterLastX.Millimeter2Micron()))
                   .ForMember(dest => dest.CenterLastY,
                              opt => opt.MapFrom(src => src.CenterLastY.Millimeter2Micron()))
                   .ForMember(dest => dest.CenterLastZ,
                              opt => opt.MapFrom(src => src.CenterLastZ.Millimeter2Micron()))
                   .ForMember(dest => dest.FoVLengthHor,
                              opt => opt.MapFrom(src => src.FovLengthHor.Millimeter2Micron()))
                   .ForMember(dest => dest.FoVLengthVert,
                              opt => opt.MapFrom(src => src.FovLengthVert.Millimeter2Micron()))
                   .ForMember(dest => dest.ImageIncrement,
                              opt => opt.MapFrom(src => (float)src.ImageIncrement.ExpandThousand()));
        }

        /// <summary>
        /// 注意与 <see cref="CreateReconMap"/> 中的一一对应
        /// </summary>
        private void CreateReconReverseMap()
        {
            CreateMap<ReconSeriesParam, ReconSeriesParamModel>()
                   .IgnoreAllPropertiesWithAnInaccessibleSetter()
                   .ForMember(dest => dest.BoneAritifactEnable,
                              opt => opt.MapFrom(src => BoolToEnableType(src.BoneAritifactEnable)))
                   .ForMember(dest => dest.RingAritifactEnable,
                              opt => opt.MapFrom(src => BoolToEnableType(src.RingAritifactEnable)))
                   .ForMember(dest => dest.SmoothZEnable,
                              opt => opt.MapFrom(src => BoolToEnableType(src.SmoothZEnable)))
                   .ForMember(dest => dest.IsHDRecon,
                              opt => opt.MapFrom(src => BoolToEnableType(src.IsHDRecon)))
                   .ForMember(dest => dest.TwoPassEnable,
                              opt => opt.MapFrom(src => BoolToEnableType(src.TwoPassEnable)))
                   .ForMember(dest => dest.MetalAritifactEnable,
                              opt => opt.MapFrom(src => BoolToEnableType(src.MetalAritifactEnable)))
                   .ForMember(dest => dest.WindmillArtifactReduceEnable,
                              opt => opt.MapFrom(src => BoolToEnableType(src.WindmillArtifactReduceEnable)))
                   .ForMember(dest => dest.ConeAngleArtifactReduceEnable,
                              opt => opt.MapFrom(src => BoolToEnableType(src.ConeAngleArtifactReduceEnable)))
                   .ForMember(dest => dest.IsTargetRecon,
                              opt => opt.MapFrom(src => BoolToYesNo(src.IsTargetRecon)))
                   .ForMember(dest => dest.WindowCenter,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.WindowCenter, arr)))
                   .ForMember(dest => dest.WindowWidth,
                              opt => opt.MapFrom((src, _, arr) => ListConverter(src.WindowWidth, arr)))
                   .ForMember(dest => dest.SliceThickness,
                              opt => opt.MapFrom(src => ((double)src.SliceThickness).Micron2Millimeter()))
                   .ForMember(dest => dest.MinTablePosition,
                              opt => opt.MapFrom(src => ((double)src.MinTablePosition).Micron2Millimeter()))
                   .ForMember(dest => dest.MaxTablePosition,
                              opt => opt.MapFrom(src => ((double)src.MaxTablePosition).Micron2Millimeter()))
                   .ForMember(dest => dest.ROIFovCenterX,
                              opt => opt.MapFrom(src => ((double)src.ROIFovCenterX).Micron2Millimeter()))
                   .ForMember(dest => dest.ROIFovCenterY,
                              opt => opt.MapFrom(src => ((double)src.ROIFovCenterY).Micron2Millimeter()))
                   .ForMember(dest => dest.CenterFirstX,
                              opt => opt.MapFrom(src => ((double)src.CenterFirstX).Micron2Millimeter()))
                   .ForMember(dest => dest.CenterFirstY,
                              opt => opt.MapFrom(src => ((double)src.CenterFirstY).Micron2Millimeter()))
                   .ForMember(dest => dest.CenterFirstZ,
                              opt => opt.MapFrom(src => ((double)src.CenterFirstZ).Micron2Millimeter()))
                   .ForMember(dest => dest.CenterLastX,
                              opt => opt.MapFrom(src => ((double)src.CenterLastX).Micron2Millimeter()))
                   .ForMember(dest => dest.CenterLastY,
                              opt => opt.MapFrom(src => ((double)src.CenterLastY).Micron2Millimeter()))
                   .ForMember(dest => dest.CenterLastZ,
                              opt => opt.MapFrom(src => ((double)src.CenterLastZ).Micron2Millimeter()))
                   .ForMember(dest => dest.FovLengthHor,
                              opt => opt.MapFrom(src => ((double)src.FoVLengthHor).Micron2Millimeter()))
                   .ForMember(dest => dest.FovLengthVert,
                              opt => opt.MapFrom(src => ((double)src.FoVLengthVert).Micron2Millimeter()))
                   .ForMember(dest => dest.ImageIncrement,
                              opt => opt.MapFrom(src => ((double)src.ImageIncrement).Micron2Millimeter()));
        }

        private bool EnableTypeToBool(EnableType type)
        {
            return type == EnableType.Enable;
        }

        private EnableType BoolToEnableType(bool @bool)
        {
            return @bool ? EnableType.Enable : EnableType.Disable;
        }

        private bool YesNoToBool(YesNoType type)
        {
            return type == YesNoType.Yes;
        }

        private YesNoType BoolToYesNo(bool @bool)
        {
            return @bool ? YesNoType.Yes : YesNoType.No;
        }

        private int EnableToInt(EnableType type)
        {
            return type == EnableType.Enable ? 1 : 0;
        }

        private uint CollimatorOpenTypeToUInt(CollimatorOpenMode type)
        {
            return type switch
            {
                CollimatorOpenMode.NearSmallAngle => 1,
                CollimatorOpenMode.NearCenter => 2,
                _ => 1
            };
        }

        private TDestArr? ListConverter<T, TDestArr>(IList<T>? src, TDestArr? dest) where TDestArr : IList<T>
        {
            if (src == null || src.Count == 0 || dest == null)
            {
                return dest;
            }

            if (typeof(TDestArr).IsArray)
            {
                for (var i = 0; i < dest.Count; i++)
                {
                    if (src.Count > i)
                    {
                        dest[i] = src[i];
                    }
                }
            }
            else
            {
                dest.Clear();

                foreach (var item in src)
                {
                    dest.Add(item);
                }
            }

            return dest;
        }

        private TDestArr? ListConverter<TSrc, TDest, TDestArr>(IList<TSrc>? src, TDestArr? dest, Func<TSrc, TDest> converter) where TDestArr : IList<TDest>
        {
            if (src == null || src.Count == 0 || dest == null)
            {
                return dest;
            }

            if (typeof(TDestArr).IsArray)
            {
                for (var i = 0; i < dest.Count; i++)
                {
                    if (src.Count > i)
                    {
                        dest[i] = converter(src[i]);
                    }
                }
            }
            else
            {
                dest.Clear();

                foreach (var item in src)
                {
                    dest.Add(converter(item));
                }
            }

            return dest;
        }
    }
}
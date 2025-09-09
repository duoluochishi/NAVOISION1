//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:19    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.OfflineMachineEnums;
using NV.CT.FacadeProxy.Common.Enums.ScanEnums;
using NV.CT.FacadeProxy.Common.Models;

namespace NV.CT.SystemInterface.MRSIntegration.Impl.Profiles;

public class ToInternalProfile : Profile
{
    public ToInternalProfile()
    {
        ConfigEnums();

        CreateMap<CommandResult, CTS.Models.BaseCommandResult>()
            .Include<CommandResult, CTS.Models.RealtimeCommandResult>()
            .Include<ReconCommandResult, CTS.Models.OfflineCommandResult>();

        CreateMap<CommandResult, CTS.Models.RealtimeCommandResult>();

        CreateMap<ReconCommandResult, CTS.Models.OfflineCommandResult>()
            .ForMember(dst => dst.ReconId, opt => opt.MapFrom(src => src.ReconID));

        CreateMap<TubeDose, CTS.Models.TubeDoseInfo>();
        CreateMap<RealDoseInfo, CTS.Models.RealDoseInfo>();

        CreateMap<SystemStatusArgs, CTS.Models.SystemStatusInfo>()
            .ForMember(dst => dst.ScanId, opt => opt.MapFrom(src => src.ScanUID));

        CreateMap<AcqReconStatusArgs, CTS.Models.RealtimeReconInfo>()
            .ForMember(dst => dst.StudyUID, opt => opt.MapFrom(src => src.StudyUID))
            .ForMember(dst => dst.PatientId, opt => opt.MapFrom(src => src.PatientUID))
            .ForMember(dst => dst.ScanId, opt => opt.MapFrom(src => src.ScanUID))
            .ForMember(dst => dst.ReconId, opt => opt.MapFrom(src => src.ReconID))
            .ForMember(dst => dst.SeriesUID, opt => opt.MapFrom(src => src.SeriesUID))
            .ForMember(dst => dst.ImagePath, opt => opt.MapFrom(src => src.ReconDataPath));

        CreateMap<RawImageSavedEventArgs, CTS.Models.RealtimeReconInfo>()
            .ForMember(dst => dst.StudyUID, opt => opt.MapFrom(src => src.StudyUID))
            .ForMember(dst => dst.PatientId, opt => opt.MapFrom(src => src.PatientUID))
            .ForMember(dst => dst.ScanId, opt => opt.MapFrom(src => src.ScanUID))
            .ForMember(dst => dst.ReconId, opt => opt.MapFrom(src => src.ReconID))
            .ForMember(dst => dst.SeriesUID, opt => opt.MapFrom(src => src.SeriesUID))
            .ForMember(dst => dst.ImagePath, opt => opt.MapFrom(src => src.Directory));

        CreateMap<ImageSavedEventArgs, CTS.Models.OfflineTaskInfo>()
            .ForMember(dst => dst.StudyUID, opt => opt.MapFrom(src => src.StudyUID))
            .ForMember(dst => dst.PatientId, opt => opt.MapFrom(src => src.PatientUID))
            .ForMember(dst => dst.ScanId, opt => opt.MapFrom(src => src.ScanUID))
            .ForMember(dst => dst.ReconId, opt => opt.MapFrom(src => src.ReconID))
            .ForMember(dst => dst.SeriesUID, opt => opt.MapFrom(src => src.SeriesUID))
            .ForMember(dst => dst.ImagePath, opt => opt.MapFrom(src => src.Directory))
            .ForMember(dst => dst.IsOver, opt => opt.MapFrom(src => src.IsFinished))
            .ForMember(dst => dst.Progress, opt => opt.MapFrom(src => src.FinishCount * 100.0 / src.TotalCount));

        //todo: 需要确认状态一下是否可以转换；
        CreateMap<OfflineReconTaskInfo, CTS.Models.OfflineTaskInfo>()
            .ForMember(dst => dst.StudyUID, opt => opt.MapFrom(src => src.StudyUID))
            .ForMember(dst => dst.PatientId, opt => opt.MapFrom(src => src.PatientID))
            .ForMember(dst => dst.ScanId, opt => opt.MapFrom(src => src.ScanUID))
            .ForMember(dst => dst.ReconId, opt => opt.MapFrom(src => src.ReconID))
            .ForMember(dst => dst.SeriesUID, opt => opt.MapFrom(src => src.SeriesUID))
            .ForMember(dst => dst.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dst => dst.Priority, opt => opt.MapFrom(src => src.priority))
            .ForMember(dst => dst.Index, opt => opt.MapFrom(src => src.Index));

        CreateMap<ImageSavedEventArgs, CTS.Models.RealtimeReconInfo>()
            .ForMember(dst => dst.StudyUID, opt => opt.MapFrom(src => src.StudyUID))
            .ForMember(dst => dst.PatientId, opt => opt.MapFrom(src => src.PatientUID))
            .ForMember(dst => dst.ScanId, opt => opt.MapFrom(src => src.ScanUID))
            .ForMember(dst => dst.ReconId, opt => opt.MapFrom(src => src.ReconID))
            .ForMember(dst => dst.SeriesUID, opt => opt.MapFrom(src => src.SeriesUID))
            .ForMember(dst => dst.ImagePath, opt => opt.MapFrom(src => src.Directory))
            .ForMember(dst => dst.IsOver, opt => opt.MapFrom(src => src.IsFinished))
            .ForMember(dst => dst.Progress, opt => opt.MapFrom(src => src.FinishCount * 100.0 / src.TotalCount ));

        CreateMap<AbstractPart, Contract.Models.DevicePart>()
            .Include<Pdu, Contract.Models.DevicePart>()
            .Include<CTBoxInfo, Contract.Models.DevicePart>()
            .Include<IFBox, Contract.Models.DevicePart>()
            .Include<AuxBoard, Contract.Models.DevicePart>()
            .Include<ExtBoard, Contract.Models.DevicePart>()
            .Include<ControlBox, Contract.Models.DevicePart>()
            .Include<Gantry, Contract.Models.Gantry>()
            .Include<TubeIntf, Contract.Models.TubeIntf>()
            .Include<Table, Contract.Models.Table>()
            .Include<RTDRecon, Contract.Models.DevicePart>();

        CreateMap<Pdu, Contract.Models.DevicePart>()
            .ForMember(dst => dst.Status, opt => opt.MapFrom(src => src.Status));
        CreateMap<CTBoxInfo, Contract.Models.DevicePart>();
        CreateMap<IFBox, Contract.Models.DevicePart>();
        CreateMap<AuxBoard, Contract.Models.DevicePart>();
        CreateMap<ExtBoard, Contract.Models.DevicePart>();
        CreateMap<ControlBox, Contract.Models.DevicePart>();
        CreateMap<Gantry, Contract.Models.Gantry>()
            .ForMember(dst => dst.FrontRearCoverClosed, opt => opt.MapFrom(src => src.FrontRearCoverAllClosed));
        CreateMap<TubeIntf, Contract.Models.TubeIntf>();
        CreateMap<Table, Contract.Models.Table>();
        CreateMap<RTDRecon, Contract.Models.DevicePart>();

        CreateMap<DiskInfo, CTS.Models.OfflineDiskInfo>();

        CreateMap<OfflineConnectionStatusArgs, CTS.Models.ServiceStatusInfo>();
    }

    private void ConfigEnums()
    {
        CreateMap<CommandStatus, CTS.Enums.CommandExecutionStatus>().ConvertUsingEnumMapping(opt => opt.MapByName()).ReverseMap();
        CreateMap<AcqReconStatus, CTS.Enums.RealtimeReconStatus>().ConvertUsingEnumMapping(opt => opt.MapByName()).ReverseMap();
        CreateMap<BodyCategory, CTS.Enums.BodySize>().ConvertUsingEnumMapping(opt => opt.MapByName()).ReverseMap();
        CreateMap<OfflineMachineTaskStatus, CTS.Enums.OfflineTaskStatus>().ConvertUsingEnumMapping(opt => opt.MapByName()).ReverseMap();
    }
}
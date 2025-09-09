//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/3/8 18:13:23           V1.0.0       张震
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using AutoMapper;
//using NV.CT.DataRepository.Contract.Entities;

namespace NV.CT.JobViewer.ApplicationService.Impl
{
    public class ToProfile : Profile
    {
        public ToProfile()
        {
            //CreateMap<OfflineReconStatus, ReconTaskStatus>().ConvertUsingEnumMapping(opt => opt.MapByName()).ReverseMap();
            //CreateMap<ReconTaskEntity, OfflineReconModel>()
            //    .ForMember(dst => dst.SeriesUID, opt => opt.MapFrom(src => src.ReconId))
            //    .ForMember(dst => dst.Status, opt => opt.MapFrom(src => src.TaskStatus))
            //    .ForMember(dst => dst.PatientName, opt => opt.MapFrom(src => src.PatientName))
            //    .ForMember(dst => dst.PatientId, opt => opt.MapFrom(src => src.PatientId))
            //    .ForMember(dst => dst.ScanId, opt => opt.MapFrom(src => src.ScanId))
            //    .ForMember(dst => dst.StudyUID, opt => opt.MapFrom(src => src.StudyInstanceUID))
            //    .ForMember(dst => dst.Progress, opt => opt.MapFrom(src => src.Progress))
            //    .ForMember(dst => dst.ReconTaskDateTime, opt => opt.MapFrom(src => src.ReconStartDate))
            //    .ForMember(dst => dst.SeriesDescription, opt => opt.MapFrom(src => src.SeriesDescription));
            //    //.ForMember(dst => dst.CreateTime, opt => opt.MapFrom(src => src.CreateTime));

            ////CreateMap<OfflineReconInfo, OfflineReconModel>();
        }
    }
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>

using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Job.Contract.Model;

namespace NV.CT.JobService;

public class ToProfile : Profile
{
    public ToProfile()
    {
        CreateMap<PatientEntity, PatientModel>().ReverseMap();
        CreateMap<StudyModel, StudyEntity>().ForSourceMember(n => n.RequestProcedure, opt => opt.DoNotValidate())
            .ForSourceMember(n => n.Birthday, opt => opt.DoNotValidate());//.ReverseMap();

        CreateMap<StudyEntity, StudyModel>().ForMember(n => n.Birthday, opt => opt.Ignore()).ForMember(n => n.RequestProcedure, opt => opt.Ignore());

        CreateMap<SeriesEntity, SeriesModel>().ReverseMap();
        CreateMap<ScanTaskEntity, ScanTaskModel>().ReverseMap();
        CreateMap<ReconTaskEntity, ReconTaskModel>().ReverseMap();
        CreateMap<(PatientEntity, StudyEntity), (PatientModel, StudyModel)>().ReverseMap();
        CreateMap<JobTaskEntity, JobTaskModel>().ReverseMap();
        CreateMap<JobTaskModel, JobTaskInfo>().ReverseMap();
    }
}
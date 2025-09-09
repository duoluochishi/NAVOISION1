//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;

namespace NV.CT.PatientBrowser.ApplicationService.Impl;
public class ToDomainProfile : Profile
{
    public ToDomainProfile()
    {
        CreateMap<Contract.Models.PatientModel, DatabaseService.Contract.Models.PatientModel>()
       .ReverseMap();
        CreateMap<Contract.Models.StudyModel, DatabaseService.Contract.Models.StudyModel>()
        .ForMember(dst => dst.AgeType, opt => opt.MapFrom(src => src.AgeType))
        .ReverseMap();
        CreateMap<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel), (Contract.Models.PatientModel, Contract.Models.StudyModel)>()
     .ReverseMap();
    }
}
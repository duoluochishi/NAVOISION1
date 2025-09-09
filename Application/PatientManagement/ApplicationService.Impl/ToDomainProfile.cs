//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using AutoMapper;
using NV.CT.ConfigService.Models.UserConfig;

namespace NV.CT.PatientManagement.ApplicationService.Impl;

public class ToDomainProfile : Profile
{
    public ToDomainProfile()
    {
        CreateMap<Contract.Models.PatientModel, DatabaseService.Contract.Models.PatientModel>()
            //.ForMember(dst => dst.PatientName, opt => opt.MapFrom(src => src.PatientName))
            //.ForMember(dst => dst.PatientSex, opt => opt.MapFrom(src => src.PatientSex))
            //.ForMember(dst => dst.PatientID, opt => opt.MapFrom(src => src.PatientID))
            //.ForMember(dst => dst.CreateTime, opt => opt.MapFrom(src => src.CreateTime))
            //.ForMember(dst => dst.ID, opt => opt.MapFrom(src => src.PatientID))
            .ReverseMap();
        CreateMap<Contract.Models.StudyModel, DatabaseService.Contract.Models.StudyModel>()
            .ReverseMap();
        CreateMap<Contract.Models.SeriesModel, DatabaseService.Contract.Models.SeriesModel>()
            .ReverseMap();
        CreateMap<Contract.Models.ImageModel, DatabaseService.Contract.Models.ImageModel>()
            .ReverseMap();
        CreateMap<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel), (Contract.Models.PatientModel, Contract.Models.StudyModel)>()
            .ReverseMap();
    }
}
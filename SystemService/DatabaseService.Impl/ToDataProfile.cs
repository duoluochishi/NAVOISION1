//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Models;
using PatientModel = NV.CT.DatabaseService.Contract.Models.PatientModel;
using StudyModel = NV.CT.DatabaseService.Contract.Models.StudyModel;

namespace NV.CT.DatabaseService.Impl;
public class ToDataProfile : Profile
{
    public ToDataProfile()
    {
        CreateMap<PatientEntity, PatientModel>().ReverseMap();

        CreateMap<StudyModel, StudyEntity>().ForSourceMember(n => n.RequestProcedure, opt => opt.DoNotValidate())
            .ForSourceMember(n => n.Birthday, opt => opt.DoNotValidate());//.ReverseMap();

        CreateMap<StudyEntity, StudyModel>()
            .ForMember(n => n.Birthday, opt => opt.Ignore())
            .ForMember(n => n.RequestProcedure, opt => opt.Ignore());

        CreateMap<SeriesEntity, SeriesModel>().ReverseMap();
        CreateMap<ScanTaskEntity, ScanTaskModel>().ReverseMap();
        CreateMap<ReconTaskEntity, ReconTaskModel>().ReverseMap();
        CreateMap<(PatientEntity, StudyEntity), (PatientModel, StudyModel)>().ReverseMap();
        CreateMap<JobTaskEntity, JobTaskModel>().ReverseMap();

        CreateMap<DoseCheckEntity, DoseCheckModel>().ReverseMap();
        CreateMap<RawDataEntity, RawDataModel>().ReverseMap();

        CreateMap<PermissionEntity, PermissionModel>().ReverseMap();
        CreateMap<RoleEntity, RoleModel>()
            .ForMember(n => n.PermissionList, opt => opt.Ignore()).ReverseMap();
        CreateMap<UserEntity, UserModel>()
            .ForMember(n => n.UserName, opt => opt.Ignore())
            .ForMember(n => n.RoleList, opt => opt.Ignore())
            .ForMember(n => n.Behavior, opt => opt.Ignore())
            .ForMember(n => n.AllUserPermissionList, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<LoginHistoryEntity, LoginHistoryModel>().ReverseMap();
    }
}
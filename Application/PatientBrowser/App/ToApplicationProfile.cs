//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.PatientBrowser.Models;

namespace NV.CT.PatientBrowser;

public class ToApplicationProfile : Profile
{
    public ToApplicationProfile()
    {
        CreateMap<VStudyModel, ApplicationService.Contract.Models.PatientModel>()
        .ForMember(dst => dst.PatientSex, opt => opt.MapFrom(src => src.Gender))
        .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Pid))
        .ReverseMap();
        CreateMap<VStudyModel, ApplicationService.Contract.Models.StudyModel>()
        .ForMember(dst => dst.PatientWeight, opt => opt.MapFrom(src => src.Weight))
        .ForMember(dst => dst.PatientSize, opt => opt.MapFrom(src => src.Height))
        .ForMember(dst => dst.AdmittingDiagnosisDescription, opt => opt.MapFrom(src => src.AdmittingDiagnosis))
        .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.StudyId))
        .ForMember(dst => dst.InternalPatientId, opt => opt.MapFrom(src => src.Pid))
        .ForMember(dst => dst.StudyId, opt => opt.MapFrom(src => src.HisStudyID))
        .ForMember(dst => dst.AgeType, opt => opt.MapFrom(src => src.AgeType))
        .ReverseMap();
        CreateMap<StudyModel, ApplicationService.Contract.Models.StudyModel>()
        .ForMember(dst => dst.PatientSize, opt => opt.MapFrom(src => src.Height))
        .ForMember(dst => dst.PatientWeight, opt => opt.MapFrom(src => src.Weight))
        .ForMember(dst => dst.AdmittingDiagnosisDescription, opt => opt.MapFrom(src => src.AdmittingDiagnosis))
        .ForMember(dst => dst.StudyId, opt => opt.MapFrom(src => src.StudyID_Dicom))
        .ForMember(dst => dst.AgeType, opt => opt.MapFrom(src => src.AgeType));
        CreateMap<VStudyModel, StudyModel>()
        .ForMember(dst => dst.Height, opt => opt.MapFrom(src => src.Height == string.Empty ? null : src.Height))
        .ForMember(dst => dst.Weight, opt => opt.MapFrom(src => src.Weight == string.Empty ? null: src.Weight))
        .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.StudyId))
        .ForMember(dst => dst.AgeType, opt => opt.MapFrom(src => src.AgeType))
        .ReverseMap();
    }
}
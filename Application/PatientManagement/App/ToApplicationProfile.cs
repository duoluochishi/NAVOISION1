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

using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.PatientManagement.Models;

namespace NV.CT.PatientManagement;

public class ToApplicationProfile : Profile
{
    public ToApplicationProfile()
    {
        CreateMap<VStudyModel, ApplicationService.Contract.Models.PatientModel>()
            .ForMember(dst => dst.PatientName, opt => opt.MapFrom(src => src.PatientName))
            .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dst => dst.PatientId, opt => opt.MapFrom(src => src.PatientId))
            .ForMember(dst => dst.CreateTime, opt => opt.MapFrom(src => src.CreateTime))
            .ForMember(dst => dst.PatientSex, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Pid))
            .ReverseMap();
        CreateMap<VStudyModel, ApplicationService.Contract.Models.StudyModel>()
            .ForMember(dst => dst.InstitutionName, opt => opt.MapFrom(src => src.InstitutionName))
            .ForMember(dst => dst.InstitutionAddress, opt => opt.MapFrom(src => src.InstitutionAddress))
            .ForMember(dst => dst.AccessionNo, opt => opt.MapFrom(src => src.AccessionNo))
            .ForMember(dst => dst.Age, opt => opt.MapFrom(src => src.Age))
            .ForMember(dst => dst.AgeType, opt => opt.MapFrom(src => src.AgeType))
            .ForMember(dst => dst.PatientWeight, opt => opt.MapFrom(src => src.Weight))
            .ForMember(dst => dst.PatientSize, opt => opt.MapFrom(src => src.Height))
            .ForMember(dst => dst.AdmittingDiagnosisDescription, opt => opt.MapFrom(src => src.AdmittingDiagnosis))
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.StudyId))
            .ForMember(dst => dst.InternalPatientId, opt => opt.MapFrom(src => src.Pid))
            .ForMember(dst => dst.Ward, opt => opt.MapFrom(src => src.Ward))
            .ForMember(dst => dst.Comments, opt => opt.MapFrom(src => src.Comments))
            .ForMember(dst => dst.StudyId, opt => opt.MapFrom(src => src.HisStudyId))
            .ForMember(dst => dst.CorrectStatus, opt => opt.MapFrom(src => src.CorrectStatus))
            .ForMember(dst => dst.ReferringPhysicianName, opt => opt.MapFrom(src => src.ReferringPhysician))
            .ForMember(dst => dst.Birthday, opt => opt.MapFrom(src => src.Birthday))
            .ForMember(dst => dst.CreateTime, opt => opt.MapFrom(src => src.StudyCreateTime))
            .ReverseMap();


        CreateMap<StudyModel, ApplicationService.Contract.Models.StudyModel>()
            .ForMember(dst => dst.Age, opt => opt.MapFrom(src => src.Age))
            .ForMember(dst => dst.InternalPatientId, opt => opt.MapFrom(src => src.InternalPatientId))
            .ForMember(dst => dst.PatientSize, opt => opt.MapFrom(src => src.Height))
            .ForMember(dst => dst.PatientWeight, opt => opt.MapFrom(src => src.Weight))
            .ForMember(dst => dst.Ward, opt => opt.MapFrom(src => src.Ward))
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.AdmittingDiagnosisDescription, opt => opt.MapFrom(src => src.AdmittingDiagnosis))
            .ForMember(dst => dst.AccessionNo, opt => opt.MapFrom(src => src.AccessionNo))
            .ForMember(dst => dst.StudyId, opt => opt.MapFrom(src => src.StudyId_Dicom))
            .ForMember(dst => dst.Comments, opt => opt.MapFrom(src => src.Comments))
            .ForMember(dst => dst.InstitutionName, opt => opt.MapFrom(src => src.InstitutionName))
            .ForMember(dst => dst.InstitutionAddress, opt => opt.MapFrom(src => src.InstitutionAddress))
            .ForMember(dst => dst.ReferringPhysicianName, opt => opt.MapFrom(src => src.ReferringPhysician))
            .ForMember(dst => dst.PatientType, opt => opt.MapFrom(src => src.PatientType))
            .ForMember(dst => dst.StudyDescription, opt => opt.MapFrom(src => src.StudyDescription))
            .ReverseMap();
        CreateMap<VStudyModel, StudyModel>()
            .ForMember(dst => dst.Age, opt => opt.MapFrom(src => src.Age))
            .ForMember(dst => dst.AgeType, opt => opt.MapFrom(src => src.AgeType))
            .ForMember(dst => dst.Height, opt => opt.MapFrom(src => src.Height))
            .ForMember(dst => dst.Weight, opt => opt.MapFrom(src => src.Weight))
            .ForMember(dst => dst.Ward, opt => opt.MapFrom(src => src.Ward))
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.StudyId))
            .ForMember(dst => dst.AdmittingDiagnosis, opt => opt.MapFrom(src => src.AdmittingDiagnosis))
            .ForMember(dst => dst.BodyPart, opt => opt.MapFrom(src => src.BodyPart))
            .ForMember(dst => dst.AccessionNo, opt => opt.MapFrom(src => src.AccessionNo))
            .ForMember(dst => dst.StudyId_Dicom, opt => opt.MapFrom(src => src.StudyId_Dicom))
            .ForMember(dst => dst.Comments, opt => opt.MapFrom(src => src.Comments))
            .ForMember(dst => dst.InstitutionAddress, opt => opt.MapFrom(src => src.InstitutionAddress))
            .ForMember(dst => dst.ReferringPhysician, opt => opt.MapFrom(src => src.ReferringPhysician))
            .ForMember(dst => dst.PatientType, opt => opt.MapFrom(src => src.PatientType))
            .ForMember(dst => dst.StudyDescription, opt => opt.MapFrom(src => src.StudyDescription))
            .ForMember(dst => dst.InstitutionName, opt => opt.MapFrom(src => src.InstitutionName)).ReverseMap();
        CreateMap<SeriesModel, ApplicationService.Contract.Models.SeriesModel>()
            .ForMember(dst => dst.InternalStudyId, opt => opt.MapFrom(src => src.StudyId))
            .ReverseMap();
        CreateMap<ImageModel, ApplicationService.Contract.Models.ImageModel>().ReverseMap();

        CreateMap<RawDataViewModel, NV.CT.DatabaseService.Contract.Models.RawDataModel>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.InternalStudyId, opt => opt.MapFrom(src => src.StudyId))
            .ForMember(dst => dst.ScanName, opt => opt.MapFrom(src => src.ScanRange))
            .ForMember(dst => dst.CreateTime, opt => opt.MapFrom(src => src.CreatedTime))
            .ForMember(dst => dst.ScanEndTime, opt => opt.MapFrom(src => src.ScanEndtime))
            .ForMember(dst => dst.Path, opt => opt.MapFrom(src => src.Path))
            .ForMember(dst => dst.IsExported, opt => opt.MapFrom(src => src.IsExported))
            .ReverseMap();

        CreateMap<ColumnItemModel, ColumnItem>().ReverseMap();

    }
}

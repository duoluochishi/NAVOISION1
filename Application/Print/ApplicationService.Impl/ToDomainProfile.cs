//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.Print.ApplicationService.Contract.Models;

namespace NV.CT.Print.ApplicationService.Impl
{
    public class ToDomainProfile : Profile
    {
        public ToDomainProfile()
        {
            CreateMap<PatientModel, DatabaseService.Contract.Models.PatientModel>().ReverseMap();
            CreateMap<StudyModel, DatabaseService.Contract.Models.StudyModel>()
                .ReverseMap();
            CreateMap<(DatabaseService.Contract.Models.StudyModel, DatabaseService.Contract.Models.PatientModel), (StudyModel, PatientModel)>()
                .ReverseMap();
            CreateMap<SeriesModel, DatabaseService.Contract.Models.SeriesModel>().ReverseMap();
        }
    }
}

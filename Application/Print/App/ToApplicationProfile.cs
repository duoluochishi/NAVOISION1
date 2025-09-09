//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.Print.ApplicationService.Contract.Models;
using NV.CT.Print.Models;

namespace NV.CT.Print
{
    public class ToApplicationProfile : Profile
    {
        public ToApplicationProfile()
        {
            CreateMap<PrintingPatientModel, PatientModel>().ReverseMap();
            CreateMap<PrintingStudyModel, StudyModel>().ReverseMap();
            CreateMap<PrintingSeriesModel, SeriesModel>().ReverseMap();
        }
    }
}

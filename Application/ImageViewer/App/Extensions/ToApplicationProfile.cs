//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.ApplicationService.Contract.Models;
using NV.CT.ImageViewer.Model;
using SeriesModel = NV.CT.ImageViewer.Model.SeriesModel;

namespace NV.CT.ImageViewer.Extensions;

public class ToApplicationProfile : Profile
{
    public ToApplicationProfile()
    {
        CreateMap<VStudyModel, PatientModel>()
            .ReverseMap();
        CreateMap<VStudyModel, StudyModel>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.StudyId))
            .ReverseMap();
        CreateMap<SeriesModel, ImageViewer.ApplicationService.Contract.Models.SeriesModel>()
            .ReverseMap();
    }

}
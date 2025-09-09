//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/5 11:01:27      V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.UI.Controls.Archive;
using NV.MPS.Configuration;

namespace NV.CT.UI.Controls
{
    public class ToApplicationProfile : Profile
    {
        public ToApplicationProfile() 
        {
            CreateMap<ArchiveModel, ArchiveInfo>()
            .ForMember(dst => dst.ClientAETitle, opt => opt.MapFrom(src => src.AECaller))
            .ForMember(dst => dst.ServerAETitle, opt => opt.MapFrom(src => src.AETitle))
            .ForMember(dst => dst.IP, opt => opt.MapFrom(src => src.Host))
            .ForMember(dst => dst.Port, opt => opt.MapFrom(src => src.Port))
            .ReverseMap();
        }
    }
}

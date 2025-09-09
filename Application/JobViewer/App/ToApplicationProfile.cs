using AutoMapper;
using NV.CT.JobService.Contract.Model;

namespace NV.CT.JobViewer;

public class ToApplicationProfile : Profile
{
    public ToApplicationProfile()
    {
        CreateMap<ExportTaskInfo, Models.ExportTaskInfo>().ReverseMap();
        CreateMap<ImportTaskInfo, Models.ImportTaskInfo>().ReverseMap();
    }
}

using AutoMapper;

using NV.CT.ImageViewer.ApplicationService.Contract.Models;

namespace NV.CT.ImageViewer.ApplicationService.Impl;

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
using AutoMapper;
using NV.CT.Console.ApplicationService.Contract.Models;
using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.Console.ApplicationService.Impl.Extensions;

public class ToDomainProfile : Profile
{
	public ToDomainProfile()
	{
		CreateMap<PatientModel, ScanStudyModel>()
			.ForMember(dest => dest.FirstName, opt => opt.Ignore())
			.ForMember(dest => dest.LastName, opt => opt.Ignore());
	}
}
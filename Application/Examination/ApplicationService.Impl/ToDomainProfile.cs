//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using ApplicationServiceModels = NV.CT.Examination.ApplicationService.Contract.Models;
using DataServiceModels = NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.Examination.ApplicationService.Impl;

public class ToDomainProfile : Profile
{
    public ToDomainProfile()
    {
        CreateMap<DataServiceModels.PatientModel, ApplicationServiceModels.StudyModel>();
        CreateMap<DataServiceModels.StudyModel, ApplicationServiceModels.StudyModel>();
    }
}
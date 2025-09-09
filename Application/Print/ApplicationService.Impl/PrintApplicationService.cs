//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.Print.ApplicationService.Contract.Interfaces;
using NV.CT.Print.ApplicationService.Contract.Models;

namespace NV.CT.Print.ApplicationService.Impl
{
    public class PrintApplicationService : IPrintApplicationService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<PrintApplicationService> _logger;
        private readonly IStudyService _studyService;
        private readonly ISeriesService _seriesService;

        public PrintApplicationService(IMapper mapper, 
                                       ILogger<PrintApplicationService> logger,
                                       IStudyService studyService,
                                       ISeriesService seriesService)
        {
            _mapper = mapper;
            _logger = logger;
            _studyService = studyService;
            _seriesService = seriesService;
        }

        public (StudyModel Study, PatientModel Patient) Get(string studyId)
        {
            var result = _studyService.Get(studyId);
            return _mapper.Map<(StudyModel Study, PatientModel Patient)>(result);
        }

        public List<SeriesModel> GetSeriesByStudyId(string studyId)
        {
            var result = _seriesService.GetSeriesByStudyId(studyId);
            var list = _mapper.Map<List<DatabaseService.Contract.Models.SeriesModel>, List<SeriesModel>>(result);
            return list;
        }

    }
}

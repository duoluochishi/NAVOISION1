//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.Print.ApplicationService.Contract.Models;

namespace NV.CT.Print.ApplicationService.Contract.Interfaces
{
    public interface IPrintApplicationService
    {
       public (StudyModel Study, PatientModel Patient) Get(string studyId);

       public List<SeriesModel> GetSeriesByStudyId(string studyId);

    }
}

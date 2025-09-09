//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Impl.Repository;

namespace NV.CT.DatabaseService.Impl;

public class PatientService : IPatientService
{
    private readonly PatientRepository _patientRepository;

    public PatientService(PatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public PatientEntity Get(string PatientId)
    {
        return _patientRepository.Get(PatientId);
    }

    public PatientEntity GetPatientById(string id)
    {
        return _patientRepository.GetPatientById(id);
    }

    public bool DeleteByGuid(string patientGuid)
    {
        return _patientRepository.DeleteByGuid(patientGuid);
    }

    public List<PatientEntity> GetExistingPatientList(string patientId, string patientName, Gender sex, DateTime birthday)
    {
        return _patientRepository.GetExistingPatientList(patientId, patientName, sex, birthday);
    }
}
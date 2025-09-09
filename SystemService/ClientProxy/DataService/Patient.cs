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
using Newtonsoft.Json;
using NV.CT.CTS.Extensions;
using NV.MPS.Communication;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;

namespace NV.CT.ClientProxy.DataService;

public class Patient : IPatientService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public Patient(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }
    public PatientEntity Get(string patientId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPatientService).Namespace,
            SourceType = nameof(IPatientService),
            ActionName = nameof(IPatientService.Get),
            Data = patientId
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<PatientEntity>(commandResponse.Data);
            return res;
        }

        return new PatientEntity();
    }


    public PatientEntity GetPatientById(string id)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPatientService).Namespace,
            SourceType = nameof(IPatientService),
            ActionName = nameof(IPatientService.GetPatientById),
            Data = id
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<PatientEntity>(commandResponse.Data);
            return res;
        }

        return new PatientEntity();
    }

    public bool DeleteByGuid(string patientGuid)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPatientService).Namespace,
            SourceType = nameof(IPatientService),
            ActionName = nameof(IPatientService.DeleteByGuid),
            Data = patientGuid
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public List<PatientEntity> GetExistingPatientList(string patientId, string patientName, Gender sex, DateTime birthday)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPatientService).Namespace,
            SourceType = nameof(IPatientService),
            ActionName = nameof(IPatientService.GetExistingPatientList),
            Data = Tuple.Create(patientId, patientName, sex, birthday).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<PatientEntity>>(commandResponse.Data);
            return res;
        }

        return new List<PatientEntity>();
    }

    
}

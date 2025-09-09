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
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.SystemConfig;

namespace NV.CT.ClientProxy.ConfigService
{
    //public class HardwareService : IHardwareService
    //{
    //    private readonly MCSServiceClientProxy _clientProxy;

    //    public HardwareService(MCSServiceClientProxy clientProxy)
    //    {
    //        _clientProxy = clientProxy;
    //    }

    //    public event EventHandler ConfigRefreshed;

    //    public HardwareConfig GetConfigs()
    //    {
    //        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
    //        {
    //            Namespace = typeof(IHardwareService).Namespace,
    //            SourceType = nameof(IHardwareService),
    //            ActionName = nameof(IHardwareService.GetConfigs),
    //            Data = string.Empty
    //        });
    //        if (response is not null && response.Success)
    //        {
    //            return JsonConvert.DeserializeObject<HardwareConfig>(response.Data);
    //        }
    //        return default;
    //    }

    //    public void Save(HardwareConfig hardwareConfig)
    //    {
    //        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
    //        {
    //            Namespace = typeof(IHardwareService).Namespace,
    //            SourceType = nameof(IHardwareService),
    //            ActionName = nameof(IHardwareService.Save),
    //            Data = JsonConvert.SerializeObject(hardwareConfig)
    //        });
    //    }
    //}
}

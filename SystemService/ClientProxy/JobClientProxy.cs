//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/11 8:18:10       V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Grpc.Core;
using NV.MPS.Communication;
using NV.MPS.Environment;

namespace NV.CT.ClientProxy;

public class JobClientProxy : MPS.Communication.ClientProxy
{
    public JobClientProxy(BusClient busClient) : base(new CommunicationClient(new Channel($"{RuntimeConfig.MCSServices.JobService.IP}:{RuntimeConfig.MCSServices.JobService.Port}", ChannelCredentials.Insecure)), busClient) { }
}

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
using Grpc.Core;
using NV.MPS.Communication;
using NV.MPS.Environment;

namespace NV.CT.ClientProxy
{

    public class SyncServiceClientProxy : MPS.Communication.ClientProxy
    {
        public SyncServiceClientProxy(BusClient busClient) : base(new CommunicationClient(new Channel($"{RuntimeConfig.MCSServices.SyncService.IP}:{RuntimeConfig.MCSServices.SyncService.Port}", ChannelCredentials.Insecure)), busClient) { }
    }

}

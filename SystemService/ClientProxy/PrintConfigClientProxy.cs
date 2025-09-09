//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/14              V1.0.0       胡安
// </summary>

using Grpc.Core;
using NV.MPS.Communication;
using NV.MPS.Environment;

namespace NV.CT.ClientProxy;

public class PrintConfigClientProxy : MPS.Communication.ClientProxy
{
    public PrintConfigClientProxy(BusClient busClient) : base(new CommunicationClient(new Channel($"{RuntimeConfig.MCSServices.PrintConfigService.IP}:{RuntimeConfig.MCSServices.PrintConfigService.Port}", ChannelCredentials.Insecure)), busClient) { }
}

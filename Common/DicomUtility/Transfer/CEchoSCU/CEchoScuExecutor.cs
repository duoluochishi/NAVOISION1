//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/21 14:34:03     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using FellowOakDicom.Network.Client;
using FellowOakDicom.Network;


namespace NV.CT.DicomUtility.Transfer.CEchoSCU
{
    public class CEchoScuExecutor
    {
        public event EventHandler<ExecuteStatusInfo> ExecuteStatusInfoChanged;

        public ExecuteStatus ExecuteStatus { get; private set; }
        public string ResponseMessage { get; private set; } = string.Empty;


        public (ExecuteStatus, string) Echo(DicomNode node)
        {
            return Task.Run(async () =>
            {
                await SendEcho(node);
                return (ExecuteStatus, ResponseMessage);
            }).Result;
        }

        public void EchoAsync(DicomNode node)
        {
            Task.Run(async () =>
            {
                await SendEcho(node);
            });
        }

        private async Task SendEcho(DicomNode node)
        {
            try
            {
                ExecuteStatus = ExecuteStatus.Started;
                ResponseMessage = string.Empty;
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(0, 0, ExecuteStatus.Started, ResponseMessage));


                var client = DicomClientFactory.Create(node.HostIP, node.Port, node.UseTlSecurity, node.CallingAE, node.CalledAE);
                client.NegotiateAsyncOps();
                var req = new DicomCEchoRequest();
                req.OnResponseReceived += OnEchoResponse;

                await client.AddRequestAsync(req);
                await client.SendAsync();
            }
            catch (Exception ex)
            {
                //当前发现可能发生的异常： 
                // AggregateException                   :连接失败，连接被拒。主要发生在服务方未开启服务等
                // DicomAssociationRequestTimedOutException :连接请求超时。与上面最大的不同是这里的服务方开启了服务，但服务连接超时。
                // DicomAssociationRejectedException    :连接被拒绝。在Message中会给出理由。
                // DicomAssociationAbortedException     :连接中断

                ExecuteStatus = ExecuteStatus.Failed;
                ResponseMessage += ex.Message;
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(0, 0, ExecuteStatus, ResponseMessage));
                return;
            }

            if (ExecuteStatus is not ExecuteStatus.Failed)
            {
                ExecuteStatus = ExecuteStatus.Succeeded;
            }

            ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(0, 0, ExecuteStatus, ResponseMessage));
        }


        private void OnEchoResponse(DicomCEchoRequest request, DicomCEchoResponse response)
        {
            switch (response.Status.State)
            {
                case DicomState.Success:
                    break;
                case DicomState.Pending:
                    break;
                case DicomState.Warning:
                    ResponseMessage += $"CEcho Warning:{response.Status.Description}";
                    break;
                case DicomState.Failure:
                    ExecuteStatus = ExecuteStatus.Failed;
                    ResponseMessage += $"CEcho Failed:{response.Status.Description}";
                    break;
            }

        }
    }
}

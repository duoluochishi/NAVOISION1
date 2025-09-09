//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/10 15:48:32     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using System.Text;

namespace NV.CT.DicomUtility.Transfer.ModalityWorklist
{
    /// <summary>
    /// ModalityWorklist 查询执行者
    /// 根据当前配置信息与查询条件，向Worklist SCP查询。
    /// 提供异步、同步两种查询方式。
    /// 异步方式下，需要监听Executor事件判断当前执行过程，并在执行完成后获取查询结果。
    /// 同步方式下，在执行完成后获取结果。
    /// </summary>
    public class ModalityWorklistSCUExecutor
    {

        public event EventHandler<ExecuteStatusInfo> ExecuteStatusInfoChanged;

        public List<WorklistResult> Results { get; }

        public ExecuteStatus ExecuteStatus { get; private set; }


        public ModalityWorklistSCUExecutor() {
            ExecuteStatus = ExecuteStatus.None;
            Results = new List<WorklistResult>();
        }

        /// <summary>
        /// 同步查询
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public WorklistResult[] Query(DicomNode node, WorklistFilter filter)
        {
            return Task.Run(async () =>
            {
                await StartQuery( node, filter);
                return Results.ToArray();
            }).Result;
        }

        /// <summary>
        /// 异步查询
        /// </summary>
        /// <param name="filter"></param>
        public async Task<WorklistResult[]> QueryAsync(DicomNode node, WorklistFilter filter)
        {
            await StartQuery(node, filter);
            return Results.ToArray();
        }

        private async Task StartQuery(DicomNode node, WorklistFilter filter)
        {
            if(ExecuteStatus is ExecuteStatus.Started or ExecuteStatus.InProgress)    //执行过程中
            {
                Global.Logger.LogWarning($"Modality worklist executor {filter.WorkFlowID} is busy, no more query allowed");
                return;
            }

            try
            {
                Results.Clear();
                ExecuteStatus = ExecuteStatus.Started;
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(0, 0, ExecuteStatus.Started, ""));

                var client = DicomClientFactory.Create(node.HostIP, node.Port, node.UseTlSecurity, node.CallingAE, node.CalledAE);
                var request = CreateCFindRequest(filter);
                request.Dataset.AddOrUpdate(DicomTag.PatientSize,"");
                request.Dataset.AddOrUpdate(DicomTag.InstitutionName, "");
                request.Dataset.AddOrUpdate(DicomTag.InstitutionAddress, "");
                request.OnResponseReceived = HandleCFindResponse;

                await client.AddRequestAsync(request);
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
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(0, 0, ExecuteStatus.Failed, ex.Message));

                return;
            }

            ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(Results.Count, Results.Count, ExecuteStatus.Succeeded, ""));
        }

        private void HandleCFindResponse(DicomCFindRequest request, DicomCFindResponse response)
        {
            if(response is null)
            {
                return;
            }

            switch(response.Status.State)
            {
                case DicomState.Success:
                    HandleCFindSuccess(request, response);
                    break;
                case DicomState.Pending:
                    HandleCFindPending(request, response);
                    break;
                case DicomState.Failure:
                case DicomState.Warning:
                case DicomState.Cancel:
                    HandleCFindFailure(request, response);
                    break;

            }
        }

        private void HandleCFindSuccess(DicomCFindRequest request, DicomCFindResponse response)
        {
            //查询成功，不需要做什么。在成功后会释放连结，并在StartQuery线程中触发Finish
        }
        private void HandleCFindPending(DicomCFindRequest request, DicomCFindResponse response)
        {
            var ds = response.Dataset;
            if (ds is null)
            {
                return;
            }
            var rawBytes = response.Dataset.GetDicomItem<DicomElement>(DicomTag.PatientName).Buffer.Data;
            string patientName = Encoding.GetEncoding("GB18030").GetString(rawBytes).Trim('\0');
            ds.AddOrUpdate(DicomTag.PatientName, patientName);
            var wlResult = WorklistResult.Create(ds);
            if(wlResult is null) 
            {
                return;
            }
            Results.Add(wlResult);

            ExecuteStatus = ExecuteStatus.InProgress;
            ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(Results.Count, Results.Count, ExecuteStatus.InProgress, ""));
        }
        private void HandleCFindFailure(DicomCFindRequest request, DicomCFindResponse response)
        {
            //查询失败，按理说不需要做什么。释放连接后处理按照当前已完成的Worlist查询返回结果。
        }

        private DicomCFindRequest CreateCFindRequest(WorklistFilter filter)
        {
            return DicomCFindRequest.CreateWorklistQuery(filter.PatientID, filter.PatientName, filter.StationAE,
                filter.StationName, filter.Modality, new DicomDateRange(filter.StartDate.Date, filter.EndDate.Date.AddDays(1).AddSeconds(-1)));
        }
    }

}

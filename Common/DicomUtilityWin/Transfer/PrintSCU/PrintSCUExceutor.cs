//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/22 13:18:31     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using FellowOakDicom.Printing;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NV.CT.DicomUtility.Transfer.PrintSCU
{
    public class PrintSCUExecutor
    {
        public event EventHandler<ExecuteStatusInfo>? ExecuteStatusInfoChanged;
        private readonly ILogger _logger;

        public ExecuteStatus ExecuteStatus { get; private set; }

        public string ResponseMessage { get; private set; } = string.Empty;

        private volatile CancellationTokenSource _cancellationTokenSource;

        public PrintSCUExecutor(ILogger logger, CancellationTokenSource cancellationTokenSource)
        {
            ExecuteStatus = ExecuteStatus.None;
            this._logger = logger;
            this._cancellationTokenSource = cancellationTokenSource;
        }

        public async Task PrintAsync(DicomNode printNode, PrintJob printJob)
        {
            try
            {
                await SendPrint(printNode, printJob);
            }
            catch (OperationCanceledException canceledException)
            {
                this._logger.LogWarning($"PrintSCUExecutor has canceled Task with JobId:{printJob.FilmSession.FilmSessionLabel}");
                throw canceledException;
            }
            catch(Exception ex)
            {
                //当前发现可能发生的异常： 
                // AggregateException                   :连接失败，连接被拒。主要发生在服务方未开启服务等
                // DicomAssociationRequestTimedOutException :连接请求超时。与上面最大的不同是这里的服务方开启了服务，但服务连接超时。
                // DicomAssociationRejectedException    :连接被拒绝。在Message中会给出理由。
                // DicomAssociationAbortedException     :连接中断
                this._logger.LogWarning($"PrintSCUExecutor has failed for JobId:{printJob.FilmSession.FilmSessionLabel}, the exception is:{ex.Message}");
                throw;
            }

        }

        private async Task SendPrint(DicomNode printNode, PrintJob printJob)
        {
            var dicomClient = DicomClientFactory.Create(printNode.HostIP, printNode.Port, false, printNode.CallingAE, printNode.CalledAE);

            ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(printJob.FilmSession.FilmSessionLabel, printJob.FilmSession.BasicFilmBoxes.Count, 0, ExecuteStatus.Started, ""));

            var printJobCreateRequest = new DicomNCreateRequest(printJob.FilmSession.SOPClassUID, printJob.FilmSession.SOPInstanceUID)
            {
                Dataset = printJob.FilmSession
            };

            var printActionRequest = new DicomNActionRequest(printJob.FilmSession.SOPClassUID, printJob.FilmSession.SOPInstanceUID, 0x0001);

            printJobCreateRequest.OnResponseReceived = (req, res) =>
            {
                //在测试过程中发现打印机有可能修改FilmSession的SopInstanceUID，
                //需要更新的后续FilmBox的referencedFilmSessionSequence中。
                switch (res.Status.State)
                {
                    case DicomState.Success:
                        HandleFilmSessionResponse(printJob.FilmSession, printActionRequest, res);
                        return;
                    case DicomState.Cancel:
                        ExecuteStatus = ExecuteStatus.Cancelled;
                        break;
                    case DicomState.Failure:
                        ExecuteStatus = ExecuteStatus.Failed;
                        break;
                    default:
                        break;
                }
                ResponseMessage += $"@PrintJob NCreate :{res.Status.Description}";
            };

            await dicomClient.AddRequestAsync(printJobCreateRequest);

            foreach (var filmbox in printJob.FilmSession.BasicFilmBoxes)
            {
                var imageBoxRequests = new List<DicomNSetRequest>();

                var filmBoxRequest = new DicomNCreateRequest(FilmBox.SOPClassUID, filmbox.SOPInstanceUID)
                {
                    Dataset = filmbox
                };
                filmBoxRequest.OnResponseReceived = (request, response) =>
                {
                    /// Handle filmbox response
                    switch (response.Status.State)
                    {
                        case DicomState.Warning:
                            ResponseMessage += $"@FilmBox NCreate Warning:{response.Status.Description}";
                            break;
                        case DicomState.Cancel:
                            ExecuteStatus = ExecuteStatus.Cancelled;
                            ResponseMessage += $"@FilmBox NCreate Cancelled:{response.Status.Description}";
                            break;
                        case DicomState.Failure:
                            ExecuteStatus = ExecuteStatus.Failed;
                            ResponseMessage += $"@FilmBox NCreate Failed:{response.Status.Description}";
                            break;
                        default:
                            break;
                    }

                    /// The created SOP instance uid should be updated to the imageboxes in current filmbox.
                    if (response.HasDataset)
                    {
                        var seq = response.Dataset.GetSequence(DicomTag.ReferencedImageBoxSequence);
                        for (int i = 0; i < seq.Items.Count; i++)
                        {
                            var req = imageBoxRequests[i];
                            var imageBox = req.Dataset;
                            var sopInstanceUid = seq.Items[i].GetSingleValue<string>(DicomTag.ReferencedSOPInstanceUID);
                            imageBox.AddOrUpdate(DicomTag.SOPInstanceUID, sopInstanceUid);
                            req.Command.AddOrUpdate(DicomTag.RequestedSOPInstanceUID, sopInstanceUid);
                        }
                    }
                };
                await dicomClient.AddRequestAsync(filmBoxRequest);

                foreach (var image in filmbox.BasicImageBoxes)
                {
                    var req = new DicomNSetRequest(image.SOPClassUID, image.SOPInstanceUID) { Dataset = image };
                    req.OnResponseReceived += HandleImageBoxNSetResponse;
                    //here the image box is inited. the SOP instance uid should be updated during handling the film box response.
                    imageBoxRequests.Add(req);
                    await dicomClient.AddRequestAsync(req);
                }
            }

            await dicomClient.AddRequestAsync(printActionRequest);

            //check if task is asked to cancel
            this.CheckIfAskedToCancel();
            await dicomClient.SendAsync();

            //Send Succeeded message
            ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(printJob.FilmSession.FilmSessionLabel, 
                                                                         printJob.FilmSession.BasicFilmBoxes.Count, 
                                                                         printJob.FilmSession.BasicFilmBoxes.Count, 
                                                                         ExecuteStatus.Succeeded,
                                                                         string.Empty));
        }

        private void HandleFilmSessionResponse(FilmSession filmSession, DicomNActionRequest actionReq, DicomNCreateResponse res)
        {
            //从Create Response中获取uid
            var newUID = res.Command.GetSingleValue<string>(DicomTag.AffectedSOPInstanceUID);

            //更新
            var newSopUID = DicomUID.Parse(newUID);
            //更新下属FilmBox中ReferencedFilmSessionSequence：
            foreach (var filmBox in filmSession.BasicFilmBoxes)
            {
                var seq = filmBox.GetSequence(DicomTag.ReferencedFilmSessionSequence);
                if (seq is not null && seq.Items.Count >= 1)
                {
                    seq.Items[0].AddOrUpdate(DicomTag.ReferencedSOPInstanceUID, newSopUID);
                }
            }
            //更新ActionRequest
            actionReq.Command.AddOrUpdate(DicomTag.RequestedSOPInstanceUID, newSopUID);
        }

        private void HandleImageBoxNSetResponse(DicomNSetRequest request, DicomNSetResponse response)
        {
            switch (response.Status.State)
            {
                case DicomState.Warning:
                    ResponseMessage += $"@ImageBox NSET Warning:{response.Status.Description}";
                    break;
                case DicomState.Cancel:
                    ExecuteStatus = ExecuteStatus.Cancelled;
                    ResponseMessage += $"@ImageBox NSET Cancelled:{response.Status.Description}";
                    break;
                case DicomState.Failure:
                    ExecuteStatus = ExecuteStatus.Failed;
                    ResponseMessage += $"@ImageBox NSET Failed:{response.Status.Description}";
                    break;
                default:
                    break;
            }
        }

        private void CheckIfAskedToCancel()
        {
            if (this._cancellationTokenSource is null)
            {
                return;
            }

            if (this._cancellationTokenSource.Token.IsCancellationRequested)
            {
                //收到取消通知后，立即通知Processor取消后续处理
                this._logger.LogTrace("PrintSCUExecutor received cancellation request.");
                Trace.WriteLine("=== PrintSCUExecutor : cancelled.");
                this._cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }


        }

    }
}

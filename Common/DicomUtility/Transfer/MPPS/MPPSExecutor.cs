//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/13 15:47:12     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;

namespace NV.CT.DicomUtility.Transfer.MPPS
{
    public class MPPSExecutor
    {

        public event EventHandler<ExecuteStatusInfo> ExecuteStatusInfoChanged;
        public ExecuteStatus ExecuteStatus { get; private set; }
        public string ExecuteMessage { get; private set; }
        public MPPSExecutor()
        {
            ExecuteStatus = ExecuteStatus.NotStarted;
            ExecuteMessage = string.Empty;
        }

        public (ExecuteStatus, string) SendMPPSInProgress(DicomNode mppsNode, MPPSNCreateInfo info)
        {
            return Task.Run(async () =>
            {
                await HandleNCreate(mppsNode, info);
                return (ExecuteStatus ,ExecuteMessage);
            }).Result;
        }

        public void SendMPPSInProgressAsync(DicomNode mppsNode, MPPSNCreateInfo info)
        {
            Task.Run(async () => {
                await HandleNCreate( mppsNode,  info);
            });
        }

        public (ExecuteStatus, string) SendMPPSPerformed(DicomNode mppsNode, MPPSNSetInfo info) {

            return Task.Run(async () =>
            {
                await HandleNSet(mppsNode, info);
                return (ExecuteStatus, ExecuteMessage);
            }).Result;
        }

        public void SendMPPSPerformedAsync(DicomNode mppsNode, MPPSNSetInfo info)
        {
            Task.Run(async () =>
            {
                await HandleNSet(mppsNode, info);
            });
        }


        private async Task HandleNSet(DicomNode mppsNode,MPPSNSetInfo info)
        {
            try
            {
                DicomStatus responseStatus = DicomStatus.UnrecognizedOperation;
                string responseMessage = string.Empty;
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(1, 0, ExecuteStatus.Started, ""));

                var client = DicomClientFactory.Create(mppsNode.HostIP, mppsNode.Port, false, mppsNode.CallingAE, mppsNode.CalledAE);

                var dataset = CreateNSetDataset(info);
                var request = new DicomNSetRequest(DicomUID.ModalityPerformedProcedureStep, DicomUID.Parse(info.AffectedInstanceUid))
                {
                    Dataset = dataset
                };

                request.OnResponseReceived += (req, response) =>
                {
                    if (response is not null)
                    {
                        ExecuteStatus = HandleDicomStatus(response.Status);
                        responseMessage = response.ToString();
                    }
                };

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
                // todo: 这里的错误码要定义么- -
                ExecuteStatus = ExecuteStatus.Failed;
                ExecuteMessage = ex.Message;
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(1, 0, ExecuteStatus.Failed, ""));
            }

            ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(1, 1, ExecuteStatus, ""));
        }


        private async Task HandleNCreate(DicomNode mppsNode, MPPSNCreateInfo info)
        {
            try
            {
                DicomStatus responseStatus = DicomStatus.UnrecognizedOperation;
                string responseMessage = string.Empty;
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(1, 0, ExecuteStatus.Started, ""));

                var client = DicomClientFactory.Create(mppsNode.HostIP, mppsNode.Port, false, mppsNode.CallingAE, mppsNode.CalledAE);

                var dataset = CreateNCreateDataset(info);
                var request = new DicomNCreateRequest(DicomUID.ModalityPerformedProcedureStep, DicomUID.Parse(info.AffectedInstanceUid))
                {
                    Dataset = dataset
                };

                request.OnResponseReceived += (req, response) =>
                {
                    if (response is not null)
                    {
                        ExecuteStatus = HandleDicomStatus(response.Status);
                        responseMessage = response.ToString();
                    }
                };

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
                // todo: 这里的错误码要定义么- -
                ExecuteStatus = ExecuteStatus.Failed;
                ExecuteMessage = ex.Message;
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(1, 0, ExecuteStatus.Failed, ""));
            }

            ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(1, 1, ExecuteStatus, ""));
        }


        private DicomDataset CreateNCreateDataset(MPPSNCreateInfo mppsInfo)
        {
            var mppsDS = new DicomDataset();

            //MODULE:Performed Procedure Step Relationship​
            //Fill Scheduled Step Attribute Sequence​
            DicomDataset ssasDS = new DicomDataset();
            ssasDS.Add(DicomTag.StudyInstanceUID, mppsInfo.StudyInstanceUID);
            ssasDS.Add(new DicomSequence(DicomTag.ReferencedStudySequence)); //empty ReferencedStudySequence
            ssasDS.Add(DicomTag.AccessionNumber, mppsInfo.AccessionNumber);
            ssasDS.Add(DicomTag.RequestedProcedureID, mppsInfo.RequestedProcedureID);   
            ssasDS.Add(DicomTag.RequestedProcedureDescription, mppsInfo.RequestedProcedureDescription);
            ssasDS.Add(DicomTag.ScheduledProcedureStepID, mppsInfo.ScheduledProcedureStepID);
            ssasDS.Add(DicomTag.ScheduledProcedureStepDescription, mppsInfo.ScheduledProcedureStepDescription);
            ssasDS.Add(new DicomSequence(DicomTag.ScheduledProtocolCodeSequence));        //empty ScheduledProtocolCodeSequence

            DicomSequence ssas = new DicomSequence(DicomTag.ScheduledStepAttributesSequence, ssasDS);
            mppsDS.Add(ssas);

            mppsDS.Add(new DicomSequence(DicomTag.ProcedureCodeSequence));              //empty ProcedureCodeSequence

            mppsDS.Add( new DicomSequence(DicomTag.PerformedSeriesSequence));  //empty PerformedSeriesSequence

            mppsDS.Add(DicomTag.PatientName, mppsInfo.PatientName);
            mppsDS.Add(DicomTag.PatientID, mppsInfo.PatientID);
            mppsDS.Add(DicomTag.PatientBirthDate, mppsInfo.PatientBirthDate);
            mppsDS.Add(DicomTag.PatientSex, mppsInfo.PatientSex);
            mppsDS.Add(new DicomSequence(DicomTag.ReferencedPatientSequence));          // empty ReferencedPatientSequence
            mppsDS.Add(DicomTag.AdmissionID, mppsInfo.AdmissionID);

            //MODULE: Performed Procedure Step Information​
            mppsDS.Add(DicomTag.PerformedProcedureStepID, mppsInfo.PerformedProcedureStepID);
            mppsDS.Add(DicomTag.PerformedStationAETitle, mppsInfo.PerformedStationAETitle);
            mppsDS.Add(DicomTag.PerformedStationName, mppsInfo.PerformedStationName);
            mppsDS.Add(DicomTag.PerformedLocation, "");                                 // empty location
            mppsDS.Add(DicomTag.PerformedProcedureStepStartDate, mppsInfo.PerformedProcedureStepStartDateTime);
            mppsDS.Add(DicomTag.PerformedProcedureStepStartTime, mppsInfo.PerformedProcedureStepStartDateTime);
            mppsDS.Add(DicomTag.PerformedProcedureStepStatus, "IN PROGRESS");
            mppsDS.Add(DicomTag.PerformedProcedureStepDescription, mppsInfo.PerformedProcedureStepDescription);
            mppsDS.Add(DicomTag.PerformedProcedureTypeDescription, "");
            mppsDS.Add(DicomTag.PerformedProcedureStepEndDate, "");
            mppsDS.Add(DicomTag.PerformedProcedureStepEndTime, "");

            mppsDS.Add(DicomTag.Modality, "CT");               //当前为CT设备，仅支持CT
            mppsDS.Add(DicomTag.StudyID, mppsInfo.StudyID);

            mppsDS.Add(new DicomSequence(DicomTag.PerformedProtocolCodeSequence));          // empty PerformedProtocolCodeSequence

            return mppsDS;
        }

        private DicomDataset CreateNSetDataset(MPPSNSetInfo info)
        {
            var mppsDS = new DicomDataset();

            mppsDS.Add(DicomTag.PerformedProcedureStepEndDate, info.PerformedProcedureStepEndDateTime);
            mppsDS.Add(DicomTag.PerformedProcedureStepEndTime, info.PerformedProcedureStepEndDateTime);
            mppsDS.Add(DicomTag.PerformedProcedureStepStatus, info.PerformedProcedureStepStatus);
            mppsDS.Add(DicomTag.PerformedProcedureStepDescription, info.PerformedProcedureStepDescription);
            mppsDS.Add(DicomTag.PerformedProcedureTypeDescription, string.Empty);

            mppsDS.Add(new DicomSequence(DicomTag.PerformedProtocolCodeSequence));

            var performedSeriesSq = new DicomSequence(DicomTag.PerformedSeriesSequence);

            foreach (var series in info.SeriesInfos)
            {
                var seriesDS = new DicomDataset
                {
                    { DicomTag.RetrieveAETitle, series.RetrieveAETitle}, // the aetitle of the archive where the images have been sent to
                    { DicomTag.SeriesDescription, series.SeriesDescription },
                    { DicomTag.PerformingPhysicianName, series.PerformingPhysicianName },
                    { DicomTag.OperatorsName, series.OperatorsName },
                    { DicomTag.ProtocolName, series.ProtocolName },
                    { DicomTag.SeriesInstanceUID, series.SeriesInstanceUID }
                };
                var refImageDS = new DicomDataset
                {
                    { DicomTag.ReferencedSOPClassUID, series.ReferencedSOPClassUID },
                    { DicomTag.ReferencedSOPInstanceUID, series.ReferencedSOPInstanceUID }
                 };

                var refSQ = new DicomSequence(DicomTag.ReferencedImageSequence, refImageDS);
                seriesDS.Add(refSQ);
                seriesDS.Add(new DicomSequence(DicomTag.ReferencedNonImageCompositeSOPInstanceSequence));
                performedSeriesSq.Items.Add(seriesDS);
            }
            mppsDS.Add(performedSeriesSq);

            return mppsDS;
        }

        private ExecuteStatus HandleDicomStatus(DicomStatus ds)
        { 
            switch(ds.State)
            {
                case DicomState.Success:
                case DicomState.Warning:                //wanring应该也算执行结束吧- -
                    return ExecuteStatus.Succeeded;
                case DicomState.Pending:
                    return ExecuteStatus.InProgress;
                case DicomState.Failure:
                    return ExecuteStatus.Failed;
                case DicomState.Cancel:                 //should not happen
                    return ExecuteStatus.Cancelled;
                default:
                    return ExecuteStatus.None;

            }
        }

    }
}

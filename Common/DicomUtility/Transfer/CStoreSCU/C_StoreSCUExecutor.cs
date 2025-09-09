//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using FellowOakDicom;
using FellowOakDicom.Imaging.Codec;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.DicomUtility.Transfer.DicomDataModifier;
using System.Diagnostics;
using System.IO;

namespace NV.CT.DicomUtility.Transfer.CStoreScu
{
    public class C_StoreSCUExecutor
    {
        #region Members
        private readonly ILogger _logger;
        private readonly List<string> _fileList = new();
        private volatile CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region Properties
        public string WorkFlowID { get; }
        public string StudyID { get; }
        public string SeriesID { get; }
        public string Path { get; }
        public string HostIP { get; }
        public int Port { get; }
        public string CallingAE { get; }
        public string CalledAE { get; }
        public DicomPriority Priority { get; }
        public int ProcessedCount { get; private set; }
        public int TotalCount =>_fileList.Count;

        public DicomTransferSyntax TransferSynctax { get; }
        public bool UseTls { get; private set; }
        public bool UseAnonymouse { get; private set; }

        public ExecuteStatus ExecuteStatus { get; private set; }

        private IDicomDataModificationHandler AnonymouseHandler{get;set;}

        #endregion

        #region Events
        public event EventHandler<ExecuteStatusInfo>? ExecuteStatusInfoChanged;
        #endregion

        #region Constructor
        public C_StoreSCUExecutor(ILogger logger,
                                  CancellationTokenSource cancellationTokenSource,
                                  string workflowID,
                                  string seriesID,
                                  string path,
                                  string ip,
                                  int port,
                                  string callingAE,
                                  string calledAE,
                                  DicomPriority priority,
                                  SupportedTransferSyntax transferSyntax = SupportedTransferSyntax.ImplicitVRLittleEndian,
                                  bool useTls = false,
                                  bool anonymouse = false)
        {
            this._logger = logger;
            this._cancellationTokenSource = cancellationTokenSource;
            WorkFlowID = workflowID;
            SeriesID = seriesID;
            Path = path;
            HostIP = ip;
            Port = port;
            CallingAE = callingAE;
            CalledAE = calledAE;
            TransferSynctax = TransferSyntaxMapper.GetMappedTransferSyntax(transferSyntax) ;
            Priority = priority;
            ExecuteStatus = ExecuteStatus.None;
            UseTls = useTls;
            UseAnonymouse = anonymouse;

            AnonymouseHandler = new DicomAnonymouseHandler();
        }

        #endregion

        #region Public Methods

        public void StartAnsyc()
        {
            InitTransferList();
            ActivateTransformAsync();
        }

        #endregion

        #region Private Methodsd
        private void InitTransferList()
        {
            _fileList.Clear();
            if (Directory.Exists(Path))
            {
                //默认当前文件夹下所有文件需要传输，不考虑后缀名
                var dir = new DirectoryInfo(Path);
                _fileList.AddRange(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(x => x.FullName).ToList());
            }
            else if (File.Exists(Path))
            {
                _fileList.Add(Path);
            }
            else
            {
                throw new InvalidOperationException($"The path is not valid directory or file name: {Path}");
            }
        }        

        private void ActivateTransformAsync()
        {
            if (TotalCount == 0)
            {
                //没有传输内容，直接返回完成
                this._logger.LogWarning($"Faield to run ActivateTransform of ArchiveJob: No any files under path:{Path}");
                return;
            }

            //初始化传输请求
            var requestList = new List<DicomCStoreRequest>();
            try
            {
                //开始传输初始化
                ProcessedCount = 0;
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(WorkFlowID, _fileList.Count, 0, ExecuteStatus.Started, string.Empty, string.Empty, SeriesID));
                //连接初始化
                var client = DicomClientFactory.Create(HostIP, Port, UseTls, CallingAE, CalledAE);
                client.NegotiateAsyncOps();

                _fileList.ForEach(x =>
                {
                    //检查是否取消标志
                    this.CheckIfAskedToCancel();

                    var originalFile = DicomFile.Open(x);
                    var newDicomFile = originalFile;
                    if (TransferSynctax != originalFile.Dataset.InternalTransferSyntax)
                    {
                        newDicomFile = new DicomFile(originalFile.Dataset.Clone(TransferSynctax));
                    }
                    if(UseAnonymouse)
                    {
                        AnonymouseHandler.ModifyDicomFile(newDicomFile);
                    }
                    var request = new DicomCStoreRequest(newDicomFile, Priority);

                    request.OnResponseReceived += OnHandleResponseReceived;
                    requestList.Add(request);
                });
                //任务开始
                 Task taskRequest= client.AddRequestsAsync(requestList);
                taskRequest.Wait();

                //检查是否取消标志
                 this.CheckIfAskedToCancel();
                Task taskSend=client.SendAsync(this._cancellationTokenSource.Token);
                taskSend.Wait();

                //Send Succeeded message
                this.ExecuteStatus = TotalCount == ProcessedCount ? ExecuteStatus.Succeeded : ExecuteStatus.Failed;
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(WorkFlowID, TotalCount, ProcessedCount, this.ExecuteStatus, string.Empty, string.Empty, SeriesID));
                this._logger.LogInformation($"C_StoreSCUExecutor has result for JobId:{this.WorkFlowID}, the TotalCount is: {TotalCount},the ProcessedCount is: {ProcessedCount},  the ExecuteStatus is {ExecuteStatus}, the SeriesID is {SeriesID}");

            }
            catch (OperationCanceledException canceledException)
            {
                this._logger.LogWarning($"C_StoreSCUExecutor has been canceled with JobId:{this.WorkFlowID}");
                throw canceledException;
            }
            catch (Exception ex)
            {
                //发生异常时，主动cancel后续操作。
                //当前发现可能发生的异常： 
                // AggregateException                   :连接失败，连接被拒。主要发生在服务方未开启服务等
                // DicomAssociationRequestTimedOutException :连接请求超时。与上面最大的不同是这里的服务方开启了服务，但服务连接超时。
                // DicomAssociationRejectedException    :连接被拒绝。在Message中会给出理由。
                // DicomAssociationAbortedException     :连接中断
                this._logger.LogWarning($"C_StoreSCUExecutor has failed for JobId:{this.WorkFlowID}, the exception is:{ex.Message},the SeriesID is :{SeriesID}");
                ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(WorkFlowID, TotalCount, ProcessedCount, ExecuteStatus.Failed, ex.Message,string.Empty, SeriesID));
            }
            finally
            {
                foreach (var request in requestList)
                {
                    request.OnResponseReceived -= OnHandleResponseReceived;
                }
            }
        }

        private void OnHandleResponseReceived(DicomCStoreRequest request, DicomCStoreResponse response)
        {
            switch(response.Status.State)
            {
                case DicomState.Success:
                    ProcessedCount++;
                    RaiseStorageProcessEvent();
                    break;
                case DicomState.Warning:
                    ProcessedCount++;
                    RaiseStorageProcessEvent();
                    break;
                case DicomState.Pending:            //非错误情况的等待，跳过。
                    this._logger.LogWarning($"C_StoreSCUExecutor has Pended for JobId:{this.WorkFlowID}, ProcessedCount is:{ProcessedCount},SeriesID is :{SeriesID}");
                    return;
                case DicomState.Failure:  //没碰到- - 暂不处理。
                    ProcessedCount++;
                    ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(WorkFlowID, TotalCount, ProcessedCount, ExecuteStatus.Failed, string.Empty, string.Empty, SeriesID));
                    this._logger.LogWarning($"C_StoreSCUExecutor has failed for JobId:{this.WorkFlowID}, ProcessedCount is:{ProcessedCount},SeriesID is :{SeriesID}");
                    break;
                case DicomState.Cancel:
                    this._logger.LogWarning($"C_StoreSCUExecutor has Canceled for JobId:{this.WorkFlowID}, ProcessedCount is:{ProcessedCount},SeriesID is :{SeriesID}");
                    break;
                default:
                    break;
            }
        }

        private void RaiseStorageProcessEvent()
        {
            ExecuteStatusInfoChanged?.Invoke(this, new ExecuteStatusInfo(WorkFlowID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, string.Empty,string.Empty, SeriesID));
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
                this._logger.LogTrace("C_StoreSCUExecutor received cancellation request.");
                Trace.WriteLine("=== C_StoreSCUExecutor : cancelled.");
                this._cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }

        #endregion
    }
}

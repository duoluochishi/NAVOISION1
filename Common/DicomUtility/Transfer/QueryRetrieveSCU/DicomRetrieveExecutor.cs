//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/29 15:22:39     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using FellowOakDicom;
using FellowOakDicom.Media;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using System.IO;

namespace NV.CT.DicomUtility.Transfer.QueryRetrieveSCU
{
    public class DicomRetrieveExecutor
    {
        public string ImageDataRootPath { get; set; }

        public event EventHandler<ExecuteStatusInfo> ExecuteStatusInfoChanged;      //todo:获取事件、进度？

        public DicomRetrieveExecutor(string imageDataRootPath) {
            ImageDataRootPath = imageDataRootPath; 
        }

        /// <summary>
        /// 返回一个保存完成的的DicomDictionary，可用于获取已保存完成的序列信息
        /// </summary>
        /// <param name="node"></param>
        /// <param name="studyFilter"></param>
        public (ExecuteStatus, string, DicomDirectory) RetrieveByGetOnStudyLevel(DicomNode node, string studyInstanceUID)
        {
            return Task.Run(() =>
            {
                return RetrieveByGet(node, studyInstanceUID).Result;
            }).Result;
        }

        public (ExecuteStatus, string, DicomDirectory) RetrieveByGetOnSeriesLevel(DicomNode node, string studyIntanceUid, string seriesInstanceUID)
        {
            return Task.Run(() =>
            {
                return RetrieveByGet(node, studyIntanceUid, seriesInstanceUID).Result;
            }).Result;
        }

        private async Task<(ExecuteStatus,string,DicomDirectory)> RetrieveByGet(DicomNode node, string studyInstanceUID,string seriesInstanceUID = "")
        {
            var dicomDic = new DicomDirectory();
            var status = ExecuteStatus.NotStarted;
            var message = string.Empty;
            try
            {
                var client = DicomClientFactory.Create(node.HostIP, node.Port, node.UseTlSecurity, node.CallingAE, node.CalledAE);
                client.NegotiateAsyncOps();
                // the client has to accept storage of the images. We know that the requested images are of SOP class Secondary capture, 
                // so we add the Secondary capture to the additional presentation context
                // a more general approach would be to mace a cfind-request on image level and to read a list of distinct SOP classes of all
                // the images. these SOP classes shall be added here.
                var pcs = DicomPresentationContext.GetScpRolePresentationContextsFromStorageUids(
                    DicomStorageCategory.Image,
                    DicomTransferSyntax.ExplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRBigEndian);
                client.AdditionalPresentationContexts.AddRange(pcs);

                var getRequest = string.IsNullOrEmpty(seriesInstanceUID) ?
                    new DicomCGetRequest(studyInstanceUID) :
                    new DicomCGetRequest(studyInstanceUID, seriesInstanceUID);
                getRequest.OnResponseReceived += (req, response) =>
                {
                    //handle get request response
                    if (response.Status.State is DicomState.Failure or DicomState.Warning)
                    {
                        message += $"C-Get Response: {response.Status.State}/{response.Status.Description}";
                    }
                    if (response.Status.State is DicomState.Failure)
                    {
                        status = ExecuteStatus.Failed;
                    }
                };

                client.OnCStoreRequest += (DicomCStoreRequest req) =>
                {
                    HandleSignleImageStorage(req.Dataset, dicomDic);
                    return Task.FromResult(new DicomCStoreResponse(req, DicomStatus.Success));  //todo:保存失败？
                };
                await client.AddRequestAsync(getRequest);
                await client.SendAsync();
            }
            catch (Exception ex)
            {
                //当前发现可能发生的异常： 
                // AggregateException                   :连接失败，连接被拒。主要发生在服务方未开启服务等
                // DicomAssociationRequestTimedOutException :连接请求超时。与上面最大的不同是这里的服务方开启了服务，但服务连接超时。
                // DicomAssociationRejectedException    :连接被拒绝。在Message中会给出理由。
                // DicomAssociationAbortedException     :连接中断
                status = ExecuteStatus.Failed;
                message += $"Retrieve {studyInstanceUID}/{seriesInstanceUID} failed on exception: {ex.Message}";
            }

            if (status != ExecuteStatus.Failed)
            {
                status = ExecuteStatus.Succeeded;
            }
            return (status, message, dicomDic);
        }

        private bool HandleSignleImageStorage(DicomDataset ds, DicomDirectory dicomDic)
        {
            var studyUid = ds.GetSingleValue<string>(DicomTag.StudyInstanceUID).Trim();
            var seriesUID = ds.GetSingleValue<string>(DicomTag.SeriesInstanceUID).Trim();
            var instanceNum = ds.GetSingleValue<int>(DicomTag.InstanceNumber);

            var seriesPath = GetSeriesPath(studyUid, seriesUID);
            EnsureSeriesPath(seriesPath);

            var instancePath = GetInstanceFileName(studyUid, seriesUID, instanceNum);
            var newFile = new DicomFile(ds);
            newFile.Save(instancePath);       //todo : 读写异常？

            dicomDic.AddFile(newFile);
            return true;
        }

        private string GetSeriesPath(string studyInstanceUID,string seriesInstanceUID)
        {
            var rootPath = Path.GetFullPath(ImageDataRootPath);
            return Path.Combine(rootPath, studyInstanceUID, seriesInstanceUID);
        }

        private void EnsureSeriesPath(string seriesPath)
        {
            if(!Directory.Exists(seriesPath))
            {
                Directory.CreateDirectory(seriesPath);
            }
        }


        private string GetInstanceFileName(string studyInstanceUID, string seriesInstanceUID,int instanceNumber)
        {
            var rootPath = Path.GetFullPath(ImageDataRootPath);
            var instanceNumStr = instanceNumber.ToString("00000");
            return Path.Combine(rootPath, studyInstanceUID, seriesInstanceUID, instanceNumStr) + ".dcm";
        }
    }
}

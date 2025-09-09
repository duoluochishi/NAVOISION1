//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/29 15:25:21     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using FellowOakDicom.Network;
using FellowOakDicom;
using FellowOakDicom.Network.Client;

namespace NV.CT.DicomUtility.Transfer.QueryRetrieveSCU
{
    public class DicomQueryExecutor
    {
        /// <summary>
        /// 同步调用
        /// </summary>
        /// <param name="studyFilter"></param>
        /// <returns></returns>
        public (ExecuteStatus, string, StudyQueryResult[]) QueryOnStudyLevel(DicomNode node, StudyQueryFilter studyFilter)
        {
            return Task.Run(() =>
            {
                return QueryOnStudyLevelAsync(node, studyFilter).Result;
            }).Result;
        }
        public (ExecuteStatus, string, SeriesQueryResult[]) QueryOnSeriesLevel(DicomNode node, string studyIntanceUid)
        {
            return Task.Run(() =>
            {
                return QueryOnSeriesLevelAsync(node, studyIntanceUid).Result;
            }).Result;
        }


        public async Task<(ExecuteStatus, string, StudyQueryResult[])> QueryOnStudyLevelAsync(DicomNode node,StudyQueryFilter studyFilter)
        {
            var results = new List<StudyQueryResult>();
            var status = ExecuteStatus.NotStarted;
            var stateMessage = string.Empty;
            try { 
                var client = DicomClientFactory.Create(node.HostIP, node.Port, node.UseTlSecurity,node.CallingAE, node.CalledAE);
                client.NegotiateAsyncOps();

                var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Study);

                //initial the empty tags for query
                StudyQueryResult.AttachEmptyStudyDataset(request.Dataset);
                //Adapt the query filter
                AdaptStudyQueryFilter(request.Dataset, studyFilter);

                request.OnResponseReceived += (req, response) =>
                {
                    if(response.Status.State is DicomState.Failure or DicomState.Warning)
                    {
                        stateMessage += $"Study Query Response: {response.Status.State}/{response.Status.Description}";
                    }
                    if(response.Status.State is DicomState.Failure)
                    {
                        status = ExecuteStatus.Failed;
                        return;
                    }
                    //Handle data:
                    if(response.Dataset is null)
                    {
                        return;
                    }

                    var result = StudyQueryResult.GetStudyQueryResult(response.Dataset);
                    results.Add(result);
                };

                await client.AddRequestAsync(request);
                await client.SendAsync();
            }
            catch (Exception ex)
            {
                status = ExecuteStatus.Failed;
                stateMessage += $"Study query failed on exception: {ex.Message}";
            }

            if (status != ExecuteStatus.Failed) {
                status = ExecuteStatus.Succeeded;
            }

            return (status, stateMessage, results.ToArray()) ;
        }

        /// <summary>
        /// 仅提供根据studyInstanceUID查询检查序列的方法就够了
        /// </summary>
        /// <param name="node"></param>
        /// <param name="studyInstanceUid"></param>
        /// <returns></returns>
        public async Task<(ExecuteStatus, string, SeriesQueryResult[])> QueryOnSeriesLevelAsync(DicomNode node, string studyInstanceUid)
        {
            var results = new List<SeriesQueryResult>();
            var status = ExecuteStatus.NotStarted;
            var stateMessage = string.Empty;
            try
            {
                var client = DicomClientFactory.Create(node.HostIP, node.Port, node.UseTlSecurity, node.CallingAE, node.CalledAE);
                client.NegotiateAsyncOps();

                var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Series);

                //initial the empty tags for query
                SeriesQueryResult.AttachEmptySeriesDataset(request.Dataset);
                //Adapt the query filter
                AdaptSeriesQueryFilter(request.Dataset, studyInstanceUid);

                request.OnResponseReceived += (req, response) =>
                {
                    if (response.Status.State is DicomState.Failure or DicomState.Warning)
                    {
                        stateMessage += $"Series Query Response: {response.Status.State}/{response.Status.Description}";
                    }
                    if (response.Status.State is DicomState.Failure)
                    {
                        status = ExecuteStatus.Failed;
                        return;
                    }
                    //Handle data:
                    if (response.Dataset is null)
                    {
                        return;
                    }

                    var result = SeriesQueryResult.GetSeriesQueryResult(response.Dataset);
                    results.Add(result);
                };

                await client.AddRequestAsync(request);
                await client.SendAsync();
            }
            catch (Exception ex)
            {
                status = ExecuteStatus.Failed;
                stateMessage += $"Series query failed on exception: {ex.Message}";
            }

            if (status != ExecuteStatus.Failed)
            {
                status = ExecuteStatus.Succeeded;
            }

            return (status, stateMessage, results.ToArray());
        }


        private void AdaptStudyQueryFilter(DicomDataset ds, StudyQueryFilter filter)
        {
            filter.AdaptDicomDataset(ds);
        }

        private void AdaptSeriesQueryFilter(DicomDataset ds, string studyInstanceUid)
        {
            ds.AddOrUpdate(DicomTag.StudyInstanceUID, studyInstanceUid);
        }

    }
}

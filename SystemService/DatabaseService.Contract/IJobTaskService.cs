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
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.DatabaseService.Contract
{
    public interface IJobTaskService
    {
        List<JobTaskModel> GetJobTasksByTypeAndStatus(string jobType, List<string> jobTaskStatusList, string beginDatetime, string endDatetime, string? patientName, string? bodyPart);

        bool Delete(string jobId, string jobTaskType);

        bool Insert(JobTaskModel jobTaskModel);

        bool Update(JobTaskModel jobTaskModel);

        JobTaskModel? GetJobById(string jobId, string jobTaskType);

        bool UpdatePriority(string jobId, string jobTaskType, int priority);

        bool UpdateTaskStatusByWorkflowId(string workflowId, string taskStatus);

        bool UpdateTaskStatusByJobId(string jobId, string jobStatus);

        JobTaskModel GetJobTaskByWorkflowId(string workflowId);

        JobTaskModel? FetchNextAvailableJob(string jobTaskType);

        JobTaskModel? FetchAvailableJobById(string jobId, string jobTaskType);
        

        int GetCountOfJobs(string jobTaskType, string jobTaskStatus);


    }
}

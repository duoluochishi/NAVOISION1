//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/6 11:24:39           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;
public interface IOfflineReconService
{
	OfflineTaskInfo GetReconTask(string studyId, string scanId, string reconId);

	OfflineCommandResult CreateReconTask(string studyId, string scanId, string reconId);

	OfflineCommandResult StartReconTask(string studyId, string scanId, string reconId);

	OfflineCommandResult CloseReconTask(string studyId, string scanId, string reconId);

	void DeleteTask(string studyId, string scanId, string reconId);

	OfflineCommandResult StartAllReconTasks(string studyId);

	void StartAutoRecons(string studyId);
}
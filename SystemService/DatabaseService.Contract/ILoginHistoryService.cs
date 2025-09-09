//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.Models;
using NV.CT.Models.User;

namespace NV.CT.DatabaseService.Contract;

public interface ILoginHistoryService
{
	bool Insert(LoginHistoryModel model);
	List<LoginHistoryModel> GetLoginHistoryListByAccount(string account);
	LoginHistoryModel GetLastLogin();
}
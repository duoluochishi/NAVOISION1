//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.CTS;
using NV.CT.CTS.Encryptions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Impl.Repository;
using NV.CT.Models;
using NV.CT.Models.User;

namespace NV.CT.DatabaseService.Impl;

public class LoginHistoryService : ILoginHistoryService
{
	private readonly IMapper _mapper;
	private readonly LoginHistoryRepository _loginHistoryRepository;

	public LoginHistoryService(IMapper maper, LoginHistoryRepository loginHistoryRepository)
	{
		_mapper = maper;
		_loginHistoryRepository = loginHistoryRepository;
	}

	public bool Insert(LoginHistoryModel model)
	{
		var tmp = _mapper.Map<LoginHistoryEntity>(model);
		return _loginHistoryRepository.Insert(tmp);
	}

	public List<LoginHistoryModel> GetLoginHistoryListByAccount(string account)
	{
		var users = new List<LoginHistoryModel>();
		foreach (var userEntity in _loginHistoryRepository.GetLoginHistoryListByAccount(account))
		{
			var userModel = _mapper.Map<LoginHistoryModel>(userEntity);
			users.Add(userModel);
		}
		return users;
	}

	public LoginHistoryModel GetLastLogin()
	{
		var userEntity = _loginHistoryRepository.GetLastLogin();
		var userModel = _mapper.Map<LoginHistoryModel>(userEntity);
		return userModel;
	}
}
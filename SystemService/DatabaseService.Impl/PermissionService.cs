//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/2 16:14:52           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Impl.Repository;
using NV.CT.Models;

namespace NV.CT.DatabaseService.Impl;

public class PermissionService : IPermissionService
{
	private readonly PermissionRepository _permissionRepository;
	private readonly IMapper _mapper;
	public PermissionService(IMapper mapper, PermissionRepository releRepository)
	{
		_mapper = mapper;
		_permissionRepository = releRepository;
	}

	public List<PermissionModel> GetAll()
	{
		List<PermissionModel> list = new List<PermissionModel>();
		foreach (var permission in _permissionRepository.GetAll())
		{
			PermissionModel permissionModel = _mapper.Map<PermissionModel>(permission);
			list.Add(permissionModel);
		}
		return list;
	}

	public PermissionModel GetById(string id)
	{
		PermissionModel permissionModel = new PermissionModel() { IsDeleted = true };
		if (_permissionRepository.GetRightByID(id) is PermissionEntity permission)
		{
			permissionModel = _mapper.Map<PermissionModel>(permission);
		}
		return permissionModel;
	}

	public PermissionModel GetByCode(string code)
	{
		PermissionModel permissionModel = new PermissionModel() { IsDeleted = true };
		if (_permissionRepository.GetRightByCode(code) is PermissionEntity permission)
		{
			permissionModel = _mapper.Map<PermissionModel>(permission);
		}
		return permissionModel;
	}
} 
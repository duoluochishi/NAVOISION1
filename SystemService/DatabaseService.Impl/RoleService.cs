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

public class RoleService : IRoleService
{
    private readonly IMapper _mapper;
    private readonly RoleRepository _roleRepository;
    public RoleService(IMapper maper, RoleRepository releRepository)
    {
        _mapper = maper;
        _roleRepository = releRepository;
    }

    public bool Insert(RoleModel model)
    {
        RoleEntity roleEntity = _mapper.Map<RoleEntity>(model);
        return _roleRepository.Insert(roleEntity);
    }

    public bool Update(RoleModel model)
    {
        RoleEntity roleEntity = _mapper.Map<RoleEntity>(model);
        return _roleRepository.Update(roleEntity);
    }

    public bool Delete(RoleModel model)
    {
        RoleEntity roleEntity = _mapper.Map<RoleEntity>(model);
        return _roleRepository.Delete(roleEntity);
    }

    public List<RoleModel> GetAllWithRight()
    {
        List<RoleModel> roleModels = new List<RoleModel>();
        foreach (var roleEntity in _roleRepository.GetAll())
        {
            roleModels.Add(GetRoleById(roleEntity.Id));
        }
        return roleModels;
    }

    public List<RoleModel> GetAll()
    {
        List<RoleModel> roleModels = new List<RoleModel>();
        foreach (var roleEntity in _roleRepository.GetAll())
        {
            RoleModel roleModel = _mapper.Map<RoleModel>(roleEntity);
            roleModels.Add(roleModel);
        }
        return roleModels;
    }

    public RoleModel GetRoleById(string roleId)
    {
        var endtity = _roleRepository.GetRoleById(roleId);
        RoleEntity roleEntity = endtity.Item1;
        RoleModel roleModel = _mapper.Map<RoleModel>(roleEntity);
        foreach (var permission in endtity.Item2)
        {
            PermissionModel permissionModel = _mapper.Map<PermissionModel>(permission);
            roleModel.PermissionList.Add(permissionModel);
        }
        return roleModel;
    }

    public RoleModel GetRoleByName(string nabme)
    {
        var endtity = _roleRepository.GetRoleByName(nabme);
        RoleEntity roleEntity = endtity.Item1;
        RoleModel roleModel = _mapper.Map<RoleModel>(roleEntity);
        foreach (var permission in endtity.Item2)
        {
            PermissionModel permissionModel = _mapper.Map<PermissionModel>(permission);
            roleModel.PermissionList.Add(permissionModel);
        }
        return roleModel;
    }

    public bool InsertRoleAndRight(RoleModel model, List<PermissionModel> permissions)
    {
        RoleEntity roleEntity = _mapper.Map<RoleEntity>(model);
        List<PermissionEntity> permissionEntities = new List<PermissionEntity>();
        foreach (var pr in permissions)
        {
            PermissionEntity permissionEntity = _mapper.Map<PermissionEntity>(pr);
            permissionEntities.Add(permissionEntity);
        }
        return _roleRepository.InsertRoleAndRight(roleEntity, permissionEntities);
    }

    public bool UpdateRoleAndRight(RoleModel model, List<PermissionModel> permissions)
    {
        RoleEntity roleEntity = _mapper.Map<RoleEntity>(model);
        List<PermissionEntity> permissionEntities = new List<PermissionEntity>();
        foreach (var pr in permissions)
        {
            PermissionEntity permissionEntity = _mapper.Map<PermissionEntity>(pr);
            permissionEntities.Add(permissionEntity);
        }
        return _roleRepository.UpdateRoleAndRight(roleEntity, permissionEntities);
    }
}
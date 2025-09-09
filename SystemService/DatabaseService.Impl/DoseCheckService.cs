//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/8/30 14:31:08           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Impl.Repository;

namespace NV.CT.DatabaseService.Impl;

public class DoseCheckService : IDoseCheckService
{
    private readonly IMapper _mapper;
    private readonly DoseCheckRepository _doseCheckRepository;

    public DoseCheckService(IMapper mapper, DoseCheckRepository patientRepository)
    {
        _mapper = mapper;
        _doseCheckRepository = patientRepository;
    }

    public bool Add(DoseCheckModel doseCheckModel)
    {
        var doseCheckEntity = _mapper.Map<DoseCheckEntity>(doseCheckModel);
        return _doseCheckRepository.Insert(doseCheckEntity);
    }

    public bool AddList(List<DoseCheckModel> doseCheckModels)
    {
        var doseCheckEntity = _mapper.Map<List<DoseCheckModel>, List<DoseCheckEntity>>(doseCheckModels);
        return _doseCheckRepository.Insert(doseCheckEntity);
    }

    public bool Delete(DoseCheckModel doseCheckModel)
    {
        var doseCheckEntity = _mapper.Map<DoseCheckEntity>(doseCheckModel);
        return _doseCheckRepository.Delete(doseCheckEntity);
    }

    public bool Update(DoseCheckModel doseCheckModel)
    {
        var doseCheckEntity = _mapper.Map<DoseCheckEntity>(doseCheckModel);
        return _doseCheckRepository.Update(doseCheckEntity);
    }

    public bool UpdateList(List<DoseCheckModel> doseCheckModels)
    {
        var doseCheckEntity = _mapper.Map<List<DoseCheckModel>, List<DoseCheckEntity>>(doseCheckModels);
        return _doseCheckRepository.Update(doseCheckEntity);
    }

    public DoseCheckModel Get(string doseCheckId)
    {
        var doseCheckEntity = _doseCheckRepository.Get(doseCheckId);
        return _mapper.Map<DoseCheckModel>(doseCheckEntity);
    }

    public List<DoseCheckModel> GetAll()
    {
        var doseCheckEntity = _doseCheckRepository.GetAll();
        var list = _mapper.Map<List<DoseCheckEntity>, List<DoseCheckModel>>(doseCheckEntity);
        return list;
    }
}